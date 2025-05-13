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
            var managingProjects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Role == "CEO" || pu.Role == "Manager").Select(pu => new { pu.ProjectId, pu.ProjectName }).Distinct().ToList();
            var tasks = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id && pu.Status == false).Select(pu => new { pu.ProjectId, pu.ProjectName, pu.Tasks }).Distinct().ToList();

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

        public IActionResult ParticipantProject(int id, string name, string nickname, string password)
        {
            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("Project not found!");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("Participant not found!");
            }

            var projectParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ProjectId == id && pu.ParticipantId == participant.Id);

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
            ViewBag.Nickname = nickname;
            ViewBag.Password = password;
            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;
            ViewBag.UserRole = role;

            return View(new NameOfProject { Id = id, Name = name });
        }

        public IActionResult Info(int projectId, int participantId, string nickname, string password)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            if (participant == null)
            {
                return NotFound("This participant doesn`t exist in this project :(");
            }

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;
            ViewBag.ProjectName = participant.ProjectName;
            ViewBag.ProjectId = participant.ProjectId;

            ViewBag.CanEditStatus = string.Equals(participant.ParticipantName, nickname, StringComparison.OrdinalIgnoreCase);

            return View(participant);
        }

        public IActionResult Save(ProjectParticipant updatedParticipant, string nickname, string password)
        {
            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("We got some troubles with link :(");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == updatedParticipant.Id);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Status = updatedParticipant.Status;

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            _context.SaveChanges();

            return RedirectToAction("ParticipantProject", "Participant", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName, nickname = nickname, password = password });
        }

        /// Manager Controller

        public IActionResult CreatingProjects(string nickname, string password)
        {

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return View();
        }

        [HttpPost]
        public IActionResult Create(NameOfProject nameofproject, string nickname, string password)
        {
            var maxProjectId = _context.NamesOfProjects.Select(p => p.Id).ToList().Max();

            var res = _context.NamesOfProjects.Add(nameofproject);
            _context.SaveChanges();

            nameofproject.Id = maxProjectId + 1;

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return RedirectToAction("Project", "Participant", new { id = nameofproject.Id, name = nameofproject.Name, nickname = nickname, password = password });
        }

        public IActionResult Project(int id, string name, string nickname, string password)
        {
            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("This project doesn't exist");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.password == password);

            if (participant == null)
            {
                return NotFound("Participant not found");
            }

            var projectParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ProjectId == id && pu.ParticipantId == participant.Id);

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
            ViewBag.Nickname = nickname;
            ViewBag.Password = password;
            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;
            ViewBag.UserRole = role;

            return View(new NameOfProject { Id = id, Name = name });
        }

        [HttpGet]
        public IActionResult Configuration(int participantId, int projectId, string nickname, string password)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return View(participant);
        }

        [HttpPost]
        public IActionResult SaveParticipant(ProjectParticipant updatedParticipant, string nickname, string password)
        {
            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == updatedParticipant.Id);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found!");
            }

            existingParticipant.Role = updatedParticipant.Role;
            existingParticipant.Tasks = updatedParticipant.Tasks;
            existingParticipant.Status = updatedParticipant.Status;

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName, nickname = nickname, password = password });
        }

        [HttpPost]
        public IActionResult AddingRedirect(NameOfProject nameofproject, string nickname, string password)
        {
            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            return RedirectToAction("Adding", "Participant", new { id = nameofproject.Id, name = nameofproject.Name, nickname = nickname, password = password });
        }

        public IActionResult Adding(int id, string name, string nickname, string password)
        {
            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            var projectParticipant = new ProjectParticipant { ProjectId = id, ProjectName = name };
            return View(projectParticipant);
        }

        [HttpPost]
        public IActionResult AddingParticipants(int projectId, string projectName, string nickname, string password, string nicknameManager, string passwordManager, string role, string tasks, bool status)
        {
            var participant = _context.Participants.FirstOrDefault(u => u.nickname == nickname && u.password == password);

            if (participant == null)
            {
                return NotFound("Participant data not valid! (nickname or password)");
            }

            bool alreadyInProject = _context.ProjectParticipants.Any(pu => pu.ParticipantId == participant.Id && pu.ParticipantName == participant.nickname && pu.ProjectId == projectId && pu.ProjectName == projectName);

            if (alreadyInProject)
            {
                return NotFound("This one already in project or one just doesn`t exist!");
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

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            _context.ProjectParticipants.Add(projectParticipant);
            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = projectId, name = projectName, nickname = nicknameManager, password = passwordManager });
        }

        [HttpPost]
        public IActionResult DeleteParticipant(int participantId, string nickname, string password)
        {
            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.Id == participantId);

            if (participant == null)
            {
                return NotFound("Participant not found");
            }

            if (participant.ParticipantName == nickname && participant.ParticipantPassword == password)
            {
                _context.ProjectParticipants.Remove(participant);
                _context.SaveChanges();

                return RedirectToAction("ExistingProjects", new { nickname = nickname, password = password });
            }

            ViewBag.Nickname = nickname;
            ViewBag.Password = password;

            _context.ProjectParticipants.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction("Project", "Participant", new { id = participant.ProjectId, name = participant.ProjectName, nickname = nickname, password = password });
        }

    }
}