using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;
using Microsoft.AspNetCore.Http;
using Mysqlx.Crud;

namespace ProjectManage.Controllers
{
    public class ParticipantController : Controller
    {
        [Route("SignUp")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("SignIn")]
        public IActionResult SignIn()
        {
            return View();
        }

        [Route("SignInError")]
        public IActionResult SignInError()
        {
            return View();
        }

        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SignUpController(Participant participant)
        {
            var passwordHasher = new PasswordHasher<Participant>();
            participant.passwordHash = passwordHasher.HashPassword(participant, participant.password);

            _context.Participants.Add(participant);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);

            return RedirectToAction("MainPage");
        }

        [HttpPost]
        public IActionResult SignInController(string nickname, string password)
        {
            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname);

            if (participant == null)
            {
                return RedirectToAction("SignInError");
            }

            var passwordHasher = new PasswordHasher<Participant>();
            var result = passwordHasher.VerifyHashedPassword(participant, participant.passwordHash, password);

            if (result == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetInt32("participantId", participant.Id);
                HttpContext.Session.SetString("nickname", participant.nickname);
                HttpContext.Session.SetString("password", participant.passwordHash);

                return RedirectToAction("MainPage");
            }
            else
            {
                return RedirectToAction("SignInError");
            }
        }

        public IActionResult MainPage()
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            var projects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Role != "CEO" && pu.Role != "Manager").Select(pu => new { pu.ProjectId, pu.ProjectName }).Distinct().ToList();
            var managingProjects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Role == "CEO" || pu.Role == "Manager").Select(pu => new { pu.ProjectId, pu.ProjectName }).Distinct().ToList();
            var tasks = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Status == false).Select(pu => new { pu.ProjectId, pu.ProjectName, pu.Tasks }).Distinct().ToList();

            ViewBag.Tasks = tasks;
            ViewBag.ManagingProjects = managingProjects;
            ViewBag.Projects = projects;
            ViewBag.Nickname = nickname;

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);

            return View();
        }

        public IActionResult Profile()
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            ViewBag.Nickname = nickname;

            return View();
        }

        public IActionResult ParticipantProject(int id, string name)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            var projectParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ProjectId == id && pu.ParticipantId == participantid);

            string role = "";

            if (projectParticipant != null && (projectParticipant.Role == "CEO" || projectParticipant.Role == "Manager"))
            {
                role = "CEO";
            }
            else
            {
                role = "Participant";
            }

            var participantIds = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).Select(pu => pu.ParticipantId).ToList();

            var participants = _context.Participants.Where(u => participantIds.Contains(u.Id)).ToList();

            var projectParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).ToList();

            int totalParticipants = projectParticipants.Count;
            int completedTasks = projectParticipants.Count(pu => pu.Status);

            ViewBag.Participants = participants;
            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;
            ViewBag.UserRole = role;

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(project);
        }

        public IActionResult Info(int id, string nickname)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var MainParticipantNickname = HttpContext.Session.GetString("nickname");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectid && p.Name == projectname);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var mainParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ParticipantName == MainParticipantNickname && pu.ProjectId == projectid);

            if (mainParticipant == null)
            {
                return NotFound("your data is not valid");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == id && pu.ParticipantName == nickname && pu.ProjectId == projectid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.Name;
            ViewBag.CanEditStatus = string.Equals(mainParticipant.ParticipantName, nickname, StringComparison.OrdinalIgnoreCase);

            HttpContext.Session.SetInt32("targetParticipantId", participant.ParticipantId);
            HttpContext.Session.SetInt32("mainParticipantId", mainParticipant.ParticipantId);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(participant);
        }

        public IActionResult Save(ProjectParticipant updatedParticipant)
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var mainParticipantId = HttpContext.Session.GetInt32("mainParticipantId");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectid && p.Name == projectname);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var mainParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == mainParticipantId && pu.ProjectId == projectid);

            if (mainParticipant == null)
            {
                return NotFound("Your data is not valid! (nickname)");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectid);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            HttpContext.Session.SetInt32("managerid", mainParticipant.ParticipantId);
            HttpContext.Session.SetString("nicknamemanager", mainParticipant.ParticipantName);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return RedirectToAction("ParticipantProject", "Participant", new { id = projectid, name = projectname });
        }

        /// Manager Controller

        public IActionResult CreatingProjects()
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return RedirectToAction("SignInError");
            }

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);

            return View();
        }

        [HttpPost]
        public IActionResult Create(NameOfProject nameofproject)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return RedirectToAction("SignInError");
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

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);

            return RedirectToAction("ManagerProject", "Participant", new { id = nameofproject.Id, name = nameofproject.Name });
        }

        public IActionResult ManagerProject(int id, string name)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
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

            HttpContext.Session.SetInt32("participantId", participant.Id);
            HttpContext.Session.SetString("nickname", participant.nickname);
            HttpContext.Session.SetString("password", participant.passwordHash);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(project);
        }

        [HttpGet]
        public IActionResult Configuration(int id, string nickname)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nicknameManager = HttpContext.Session.GetString("nickname");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectid && p.Name == projectname);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ParticipantName == nicknameManager && pu.ProjectId == projectid);

            if (manager == null)
            {
                return NotFound("Manager data not valid! (nickname)");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == id && pu.ParticipantName == nickname && pu.ProjectId == projectid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            HttpContext.Session.SetInt32("targetParticipantId", participant.ParticipantId);
            HttpContext.Session.SetInt32("managerId", manager.ParticipantId);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(participant);
        }

        [HttpPost]
        public IActionResult SaveParticipant(ProjectParticipant updatedParticipant)
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var managerId = HttpContext.Session.GetInt32("managerId");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectid && p.Name == projectname);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == managerId && pu.ProjectId == projectid);

            if (manager == null)
            {
                return NotFound("Manager data not valid! (nickname)");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectid);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Role = updatedParticipant.Role;
            existingParticipant.Tasks = updatedParticipant.Tasks;
            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            HttpContext.Session.SetInt32("managerid", manager.ParticipantId);
            HttpContext.Session.SetString("nicknamemanager", manager.ParticipantName);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return RedirectToAction("ManagerProject", "Participant", new { id = projectid, name = projectname });
        }

        [HttpPost]
        public IActionResult DeleteParticipant()
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var managerId = HttpContext.Session.GetInt32("managerId");
            var projectid = HttpContext.Session.GetInt32("projectId");
            var projectname = HttpContext.Session.GetString("projectName");

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectid);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            if (participant.ParticipantId == managerId && participant.ProjectId == projectid)
            {
                _context.ProjectParticipants.Remove(participant);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("participantId", participant.ParticipantId);
                HttpContext.Session.SetString("nickname", participant.ParticipantName);
                HttpContext.Session.SetString("password", participant.ParticipantPassword);

                return RedirectToAction("MainPage");
            }

            _context.ProjectParticipants.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction("ManagerProject", "Participant", new { id = projectid, name = projectname });
        }

        public IActionResult Adding(int id, string name)
        {
            var participantid = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");
            var password = HttpContext.Session.GetString("password");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ProjectId == id);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            HttpContext.Session.SetInt32("participantId", participant.ParticipantId);
            HttpContext.Session.SetString("nickname", participant.ParticipantName);
            HttpContext.Session.SetString("password", participant.ParticipantPassword);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(participant);
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
                return NotFound("This project doesn't exist");
            }

            var manager = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantid && pu.ProjectId == projectid);

            if (manager == null)
            {
                return NotFound("Manager data not valid! (nickname)");
            }

            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname);

            if (participant == null)
            {
                return NotFound("Participant data not valid! (nickname or password)");
            }

            var passwordHasher = new PasswordHasher<Participant>();
            var result = passwordHasher.VerifyHashedPassword(participant, participant.passwordHash, password);

            if (result == PasswordVerificationResult.Success)
            {
                bool alreadyInProject = _context.ProjectParticipants.Any(pu => pu.ParticipantId == participant.Id && pu.ParticipantName == participant.nickname && pu.ProjectId == projectid && pu.ProjectName == projectname);

                if (alreadyInProject)
                {
                    return NotFound("This one already in project or one just doesn`t exist!");
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

                ViewBag.Nickname = nicknameManager;
                ViewBag.Password = passwordManager;

                _context.ProjectParticipants.Add(projectParticipant);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("participantId", manager.ParticipantId);
                HttpContext.Session.SetString("nickname", manager.ParticipantName);
                HttpContext.Session.SetString("password", manager.ParticipantPassword);
                HttpContext.Session.SetInt32("projectId", project.Id);
                HttpContext.Session.SetString("projectName", project.Name);

                return RedirectToAction("ManagerProject", "Participant", new { id = projectid, name = projectname });
            }
            else
            {
                return RedirectToAction("SignInError");
            }
        }
    }
    internal record NewRecord(string Nickname, string Password);
}