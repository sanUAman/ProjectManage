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

            ViewBag.Participants = participants;

            return View(new NameOfProject { Id = id, Name = name });
        }

        public IActionResult Adding(int id, string name)
        {
            var projectUser = new ProjectUser { ProjectId = id, ProjectName = name };
            return View(projectUser);
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
        public IActionResult AddingRedirect(NameOfProject nameofproject)
        {
            return RedirectToAction("Adding", "Manager", nameofproject);
        }

        [HttpPost]
        public IActionResult AddingParticipants(int projectId, string projectName, string nickname, string password, string role, string tasks)
        {
            var user = _context.Users.FirstOrDefault(u => u.nickname == nickname && u.password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found or incorrect credentials.");
                return View("Error");
            }

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
                Role = role,
                Tasks = tasks
            };

            _context.ProjectUsers.Add(projectUser);
            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { id = projectId, name = projectName });
        }
    }
}