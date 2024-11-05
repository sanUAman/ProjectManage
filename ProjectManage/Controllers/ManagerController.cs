using Microsoft.AspNetCore.Mvc;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;

namespace ProjectManage.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Project(NameOfProject nameofproject)
        {
            return View(nameofproject);
        }

        public IActionResult Adding(NameOfProject nameofproject)
        {
            return View(new ProjectUser() 
            { 
                nameofproject = nameofproject
            });
        }

        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(NameOfProject nameofproject)
        {
            if (string.IsNullOrEmpty(nameofproject.name))
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
        public IActionResult AddingParticipants(NameOfProject nameofproject, int projectId, string nickname, string password, string role, string tasks)
        {
            var user = _context.Users.FirstOrDefault(u => u.nickname == nickname && u.password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found or incorrect credentials.");
                return View("Adding");
            }

            bool alreadyInProject = _context.ProjectUsers.Any(pu => pu.user.Id == user.Id && pu.nameofproject.Id == projectId);

            if (alreadyInProject)
            {
                ModelState.AddModelError("", "User is already assigned to this project.");
                return View("Error");
            }

            var projectUser = new ProjectUser
            {
                user = user,
                nameofproject = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId),
                role = role,
                tasks = tasks
            };

            _context.ProjectUsers.Add(projectUser);
            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { nameofproject.name });
        }
    }
}
