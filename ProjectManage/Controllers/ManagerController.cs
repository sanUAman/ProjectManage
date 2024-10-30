using Microsoft.AspNetCore.Mvc;
using ProjectManage.Models;
using ProjectManage.Data;

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
            return View(nameofproject);
        }

        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(NameOfProject nameofproject)
        {
            if (string.IsNullOrEmpty(nameofproject.name))
            {
                return Redirect("/Manager");
            }

            _context.NamesOfProjects.Add(nameofproject);
            _context.SaveChanges();

            return RedirectToAction("Project", "Manager", new { nameofproject.name });

        }

        [HttpPost]
        public IActionResult AddingParticipants(NameOfProject nameofproject)
        {
            return RedirectToAction("Adding", "Project");
        }
    }
}
