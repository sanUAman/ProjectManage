using Microsoft.AspNetCore.Mvc;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;

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

        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SignUpController(Participant participant)
        {
            if (_context.Participants.Any(p => p.nickname == participant.nickname))
            {
                return NotFound("This nickname is already in use!");
            }

            if (_context.Participants.Any(p => p.password == participant.password))
            {
                return NotFound("This password is already in use!");
            }

            _context.Participants.Add(participant);
            _context.SaveChanges();

            return RedirectToAction("ExistingProjects", new { nickname = participant.nickname, password = participant.password });
        }

        [HttpPost]
        public IActionResult SignInController(string nickname, string password)
        {
            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("This profile doesn't exist!");
            }

            return RedirectToAction("ExistingProjects", new { nickname = nickname, password = password });
        }

        public IActionResult ExistingProjects(string nickname, string password)
        {
            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            var projects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Role != "CEO" && pu.Role != "Manager").Select(pu => new { pu.ProjectId, pu.ProjectName }).Distinct().ToList();
            var managingProjects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Role == "CEO" || pu.Role == "Manager").Select(pu => new { pu.ProjectId, pu.ProjectName}).Distinct().ToList();
            var tasks = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id).Select(pu => new { pu.ProjectId, pu.ProjectName, pu.Tasks }).Distinct().ToList();

            ViewBag.Tasks = tasks;
            ViewBag.ManagingProjects = managingProjects;
            ViewBag.Projects = projects;
            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return View();
        }

        public IActionResult Profile(string nickname, string password)
        {
            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return View();
        }

        public IActionResult ParticipantProject(string name, int id, string nickname)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound("Project not specified!");
            }

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("Project not found!");
            }

            var participantIds = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).Select(pu => pu.ParticipantId).ToList();

            var participants = _context.Participants.Where(u => participantIds.Contains(u.Id)).ToList();

            var projectParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).ToList();

            int totalParticipants = projectParticipants.Count;
            int completedTasks = projectParticipants.Count(pu => pu.Status);

            ViewBag.Nickname = nickname;

            ViewBag.Participants = participants;

            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;

            return View(project);
        }

        public IActionResult Info(int participantId, int projectId, string currentNickname)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            ViewBag.CurrentNickname = currentNickname;

            return View(participant);
        }

        public IActionResult Save(ProjectParticipant updatedParticipant, string nickname)
        {
            string currentUserNickname = nickname;

            if (string.IsNullOrEmpty(currentUserNickname))
            {
                return Unauthorized("You are not logged in!");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == updatedParticipant.Id);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Status = updatedParticipant.Status;

            ViewBag.CurrentNickname = currentUserNickname;

            _context.SaveChanges();

            return RedirectToAction("ParticipantProject", "Participant", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName, nickname = currentUserNickname });
        }

        /// Manager Controller

        public IActionResult CreatingProjects(string nickname, string password)
        {

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return View();
        }

        [HttpPost]
        public IActionResult Create(NameOfProject nameofproject)
        {
            if (string.IsNullOrEmpty(nameofproject.Name))
            {
                return Redirect("/Manager");
            }

            var maxProjectId = _context.NamesOfProjects.Select(p => p.Id).ToList().Max();

            var res = _context.NamesOfProjects.Add(nameofproject);
            _context.SaveChanges();

            nameofproject.Id = maxProjectId + 1;

            return RedirectToAction("Project", "Participant", nameofproject);
        }

        public IActionResult Project(int id, string name)
        {
            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var participantIds = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).Select(pu => pu.ParticipantId).ToList();

            var participants = _context.Participants.Where(u => participantIds.Contains(u.Id)).ToList();

            var projectParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).ToList();

            int totalParticipants = projectParticipants.Count;
            int completedTasks = projectParticipants.Count(pu => pu.Status);

            ViewBag.Participants = participants;

            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;

            return View(new NameOfProject { Id = id, Name = name });
        }

        [HttpGet]
        public IActionResult Configuration(int participantId, int projectId)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            return View(participant);
        }

        [HttpPost]
        public IActionResult AddingRedirect(NameOfProject nameofproject)
        {
            return RedirectToAction("Adding", "Participant", nameofproject);
        }

        public IActionResult Adding(int id, string name)
        {
            var projectParticipant = new ProjectParticipant { ProjectId = id, ProjectName = name };
            return View(projectParticipant);
        }

        ///

        [HttpPost]
        public IActionResult SaveParticipant(ProjectParticipant updatedParticipant)
        {
            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == updatedParticipant.Id);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Role = updatedParticipant.Role;
            existingParticipant.Tasks = updatedParticipant.Tasks;
            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName });
        }

        [HttpPost]
        public IActionResult DeleteParticipant(int participantId)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == participantId);

            if (participant == null)
            {
                return NotFound("Participant not found");
            }

            _context.ProjectParticipants.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = participant.ProjectId, name = participant.ProjectName });
        }

        [HttpPost]
        public IActionResult AddingParticipants(int projectId, string projectName, string nickname, string password, string role, string tasks, bool status)
        {
            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname && u.password == password);

            if (participant == null)
            {
                return NotFound("Participant data not valid! (nickname or password)");
            }

            bool alreadyInProject = _context.ProjectParticipants.Any(pu => pu.ParticipantId == participant.Id && pu.ProjectId == projectId && pu.ProjectName == projectName);

            if (alreadyInProject)
            {
                return NotFound("This one already in project!");
            }

            var projectParticipant = new ProjectParticipant
            {
                ParticipantId = participant.Id,
                ProjectId = projectId,
                ProjectName = projectName,
                ParticipantName = nickname,
                ParticipantPassword = password,
                Role = role,
                Tasks = tasks,
                Status = status
            };

            _context.ProjectParticipants.Add(projectParticipant);
            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = projectId, name = projectName });
        }

    }
}
