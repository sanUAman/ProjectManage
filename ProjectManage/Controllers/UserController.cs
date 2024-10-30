using Microsoft.AspNetCore.Mvc;
using ProjectManage.Models;
using ProjectManage.Data;

namespace ProjectManage.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Nickname_error(User user)
        {
            return View(user);
        }

        public IActionResult Password_error(User user)
        {
            return View(user);
        }

        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Registration(User user)
        {
            if (_context.Users.Any(u => u.nickname == user.nickname))
            {
                return RedirectToAction("Nickname_error", "User", new { user.nickname });
            }

            if (_context.Users.Any(u => u.password == user.password))
            {
                return RedirectToAction("Password_error", "User", new { user.password });
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Redirect("/");
        }
    }
}
