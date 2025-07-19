using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;

namespace ProjectManage.Controllers
{
    public class MainPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MainPageController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        [Route("SignUpError")]
        public IActionResult SignUpError()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUpController(Participant participant)
        {
            bool nicknameExists = _context.Participants.Any(p => p.nickname == participant.nickname);
            if (nicknameExists)
            {
                return RedirectToAction("SignUpError");
            }

            var passwordHasher = new PasswordHasher<Participant>();
            participant.passwordHash = passwordHasher.HashPassword(participant, participant.password);

            _context.Participants.Add(participant);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("participantId", participant.Id);

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

                return RedirectToAction("MainPage");
            }
            else
            {
                return RedirectToAction("SignInError");
            }
        }

        public IActionResult MainPage()
        {
            var participantId = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantId);

            if (participant == null)
            {
                return RedirectToAction("SignInError");
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

            return View();
        }

        public IActionResult Profile()
        {
            var participantId = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var participant = _context.Participants.FirstOrDefault(p => p.nickname == nickname && p.Id == participantId);

            if (participant == null)
            {
                return RedirectToAction("SignInError");
            }

            ViewBag.Nickname = nickname;

            return View();
        }
    }
    internal record NewRecord(string Nickname, string Password);
}
