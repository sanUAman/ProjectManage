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

        public IActionResult EnterProject_error()
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
                return View("EnterProject_error");
            }

            return RedirectToAction("Project", new { id = id, name = name });
        }

        public IActionResult Project(int id, string name)
        {
            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("Project not found");
            }

            var participantIds = _context.ProjectUsers
                .Where(pu => pu.ProjectId == id)
                .Select(pu => pu.UserId)
                .ToList();

            var participants = _context.Users
                .Where(u => participantIds.Contains(u.Id))
                .ToList();

            var projectUsers = _context.ProjectUsers
                .Where(pu => pu.ProjectId == id)
                .ToList();

            int totalParticipants = projectUsers.Count;
            int completedTasks = projectUsers.Count(pu => pu.Status);

            ViewBag.Participants = participants;

            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;

            return View(new NameOfProject { Id = id, Name = name });
        }

        public IActionResult Adding(int id, string name)
        {
            var projectUser = new ProjectUser { ProjectId = id, ProjectName = name };
            return View(projectUser);
        }

        [HttpGet]
        public IActionResult Configuration(int userId, int projectId)
        {
            // Отримуємо дані учасника за UserId і ProjectId
            var participant = _context.ProjectUsers.FirstOrDefault(pu => pu.UserId == userId && pu.ProjectId == projectId);

            if (participant == null)
            {
                return NotFound("Participant not found");
            }

            return View(participant);
        }

        [HttpPost]
        public IActionResult SaveParticipant(ProjectUser updatedParticipant)
        {
            var existingParticipant = _context.ProjectUsers
                .FirstOrDefault(pu => pu.Id == updatedParticipant.Id);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found");
            }

            // Оновлюємо дані учасника
            existingParticipant.Role = updatedParticipant.Role;
            existingParticipant.Tasks = updatedParticipant.Tasks;
            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { id = existingParticipant.ProjectId, name = existingParticipant.ProjectName });
        }

        [HttpPost]
        public IActionResult DeleteParticipant(int participantId)
        {
            var participant = _context.ProjectUsers.FirstOrDefault(pu => pu.Id == participantId);

            if (participant == null)
            {
                return NotFound("Participant not found");
            }

            _context.ProjectUsers.Remove(participant);
            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { id = participant.ProjectId, name = participant.ProjectName });
        }

        [HttpPost]
        public IActionResult AddingRedirect(NameOfProject nameofproject)
        {
            return RedirectToAction("Adding", "Manager", nameofproject);
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
            var user = _context.Users.FirstOrDefault(u => u.nickname == nickname && u.password == password);

            bool alreadyInProject = _context.ProjectUsers.Any(pu => pu.UserId == user.Id && pu.ProjectId == projectId && pu.ProjectName == projectName);

            if (alreadyInProject)
            {
                ModelState.AddModelError("", "User is already assigned to this project.");
                return View("Error");
            }

            var projectUser = new ProjectUser
            {
                UserId = user.Id,
                ProjectId = projectId,
                ProjectName = projectName,
                UserName = nickname,
                UserPassword = password,
                Role = role,
                Tasks = tasks,
                Status = status
            };

            _context.ProjectUsers.Add(projectUser);
            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { id = projectId, name = projectName });
        }
    }
}