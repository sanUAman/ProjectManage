using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManage.Data;
using ProjectManage.Models;
using System.Data;

namespace ProjectManage.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EnterProject()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnterProject(int id, string name)
        {
            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist!");
            }

            return RedirectToAction("Project", new { id = id, name = name });
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

            return RedirectToAction("Project", "Manager", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName });
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

            return RedirectToAction("Project", "Manager", new { id = participant.ProjectId, name = participant.ProjectName });
        }

        [HttpPost]
        public IActionResult AddingRedirect(NameOfProject nameofproject)
        {
            return RedirectToAction("Adding", "Manager", nameofproject);
        }

        public IActionResult Adding(int id, string name)
        {
            var projectParticipant = new ProjectParticipant { ProjectId = id, ProjectName = name };
            return View(projectParticipant);
        }

        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(NameOfProject nameofproject)
        {
            if (string.IsNullOrEmpty(nameofproject.Name))
            {
                return Redirect("/Manager");
            }

            var maxProjectId = _context.NamesOfProjects.Select(p => p.Id).ToList().Max();

            var res = _context.NamesOfProjects.Add(nameofproject);
            _context.SaveChanges();

            nameofproject.Id = maxProjectId + 1;

            return RedirectToAction("Project", "Manager", nameofproject);
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

            return RedirectToAction("Project", "Manager", new { id = projectId, name = projectName });
        }
    }
}