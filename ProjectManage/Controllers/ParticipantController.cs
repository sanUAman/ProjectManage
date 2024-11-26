using Microsoft.AspNetCore.Mvc;
using ProjectManage.Models;
using ProjectManage.Data;

namespace ProjectManage.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Nickname_error(Participant participant)
        {
            return View(participant);
        }

        public IActionResult Password_error(Participant participant)
        {
            return View(participant);
        }

        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Registration(Participant participant)
        {
            if (_context.Participants.Any(u => u.nickname == participant.nickname))
            {
                return RedirectToAction("Nickname_error", "User", new { participant.nickname });
            }

            if (_context.Participants.Any(u => u.password == participant.password))
            {
                return RedirectToAction("Password_error", "User", new { participant.password });
            }

            _context.Participants.Add(participant);
            _context.SaveChanges();

            return Redirect("/");
        }
    }
}
