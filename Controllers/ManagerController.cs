using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;
using Mysqlx.Crud;

namespace ProjectManage.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CreatingProjects()
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(NameOfProject nameofproject)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            _context.NamesOfProjects.Add(nameofproject);
            _context.SaveChanges();

            var newProjectParticipant = new ProjectParticipant
            {
                ProjectId = nameofproject.Id,
                ProjectName = nameofproject.Name,
                ParticipantId = participant.Id,
                ParticipantName = participant.nickname,
                ParticipantPassword = participant.passwordHash,
                Role = "CEO",
                Tasks = "",
                Status = false,
            };

            _context.ProjectParticipants.Add(newProjectParticipant);
            _context.SaveChanges();

            return RedirectToAction("ManagerProject", "Manager", new { id = nameofproject.Id, name = nameofproject.Name });
        }

        public IActionResult ManagerProject(int id, string name)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantid);

            if (participant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var projectParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ProjectId == id && pu.ParticipantId == participantid);

            string role = "";

            if (projectParticipant != null && (projectParticipant.Role == "CEO" || projectParticipant.Role == "Manager"))
            {
                role = "CEO";
            }

            var participantIds = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).Select(pu => pu.ParticipantId).ToList();

            var participants = _context.Participants.Where(u => participantIds.Contains(u.Id)).ToList();

            var projectParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).ToList();

            int totalParticipants = projectParticipants.Count;
            int completedTasks = projectParticipants.Count(pu => pu.Status);

            ViewBag.Participants = participants;
            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;
            ViewBag.UserRole = role;
            ViewBag.ProjectId = id;
            ViewBag.ProjectName = name;

            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(project);
        }

        [HttpGet]
        public IActionResult Configuration(int id, string nickname)
        {
            var participantId = HttpContext.Session.GetInt32("participantId");
            var nicknameManager = HttpContext.Session.GetString("nickname");
            var projectId = HttpContext.Session.GetInt32("projectId");
            var projectName = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId && p.Name == projectName);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ParticipantName == nicknameManager && pu.ProjectId == projectId);

            if (manager == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == id && pu.ParticipantName == nickname && pu.ProjectId == projectId);

            if (participant == null)
            {
                return NotFound("Participant not found! (Server error)");
            }

            HttpContext.Session.SetInt32("targetParticipantId", participant.ParticipantId);
            HttpContext.Session.SetInt32("managerId", manager.ParticipantId);

            return View(participant);
        }

        [HttpPost]
        public IActionResult SaveParticipant(ProjectParticipant updatedParticipant)
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var managerId = HttpContext.Session.GetInt32("managerId");
            var projectId = HttpContext.Session.GetInt32("projectId");
            var projectName = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId && p.Name == projectName);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == managerId && pu.ProjectId == projectId);

            if (manager == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectId);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found! (Server error)");
            }

            existingParticipant.Role = updatedParticipant.Role;
            existingParticipant.Tasks = updatedParticipant.Tasks;
            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            HttpContext.Session.SetString("nicknamemanager", manager.ParticipantName);

            return RedirectToAction("ManagerProject", "Manager", new { id = projectId, name = projectName });
        }

        [HttpPost]
        public IActionResult DeleteParticipant()
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var managerId = HttpContext.Session.GetInt32("managerId");
            var projectId = HttpContext.Session.GetInt32("projectId");
            var projectName = HttpContext.Session.GetString("projectName");

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectId);

            if (participant == null)
            {
                return NotFound("Participant not found! (Server error)");
            }

            if (participant.ParticipantId == managerId && participant.ProjectId == projectId)
            {
                var allParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == projectId).ToList();

                var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId && p.Name == projectName);
                if (project != null)
                {
                    _context.NamesOfProjects.Remove(project);
                }
                else
                {
                    return NotFound("This project doesn't exist (Server error)");
                }

                _context.ProjectParticipants.RemoveRange(allParticipants);
                _context.ProjectParticipants.Remove(participant);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("participantId", participant.ParticipantId);
                HttpContext.Session.SetString("nickname", participant.ParticipantName);
                HttpContext.Session.SetString("password", participant.ParticipantPassword);

                return RedirectToAction("MainPage");
            }

            _context.ProjectParticipants.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction("ManagerProject", "Manager", new { id = projectId, name = projectName });
        }

        public IActionResult Adding(int id, string name)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ProjectId == id);

            if (participant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.Name;

            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(new ProjectParticipant { ProjectId = project.Id, ProjectName = project.Name });
        }

        [HttpPost]
        public IActionResult AddingParticipants(string nickname, string password, string role, string tasks, bool status)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nicknameManager = HttpContext.Session.GetString("nickname");
            var passwordManager = HttpContext.Session.GetString("password");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectid && p.Name == projectname);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ProjectId == projectid);

            if (manager == null)
            {
                return NotFound("Manager data not valid! (nickname)");
            }

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return NotFound("Participant data not valid! (Server error)");
            }

            var passwordHasher = new PasswordHasher<Participant>();
            var result = passwordHasher.VerifyHashedPassword(participant, participant.passwordHash, password);

            if (result == PasswordVerificationResult.Success)
            {
                bool alreadyInProject = _context.ProjectParticipants.Any(pu => pu.ParticipantId == participant.Id && pu.ParticipantName == participant.nickname && pu.ProjectId == projectid && pu.ProjectName == projectname);

                if (alreadyInProject)
                {
                    return NotFound("This participant already in project or one just doesn`t exist!");
                }

                var projectParticipant = new ProjectParticipant
                {
                    ParticipantId = participant.Id,
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ParticipantName = participant.nickname,
                    ParticipantPassword = participant.passwordHash,
                    Role = role,
                    Tasks = tasks,
                    Status = status
                };

                _context.ProjectParticipants.Add(projectParticipant);
                _context.SaveChanges();

                return RedirectToAction("ManagerProject", "Manager", new { id = projectid, name = projectname });
            }
            else
            {
                return NotFound("Something went wrong! (Server error)");
            }
        }
    }
}
