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

        public IActionResult Login()
        {
            return View();
        }

        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Registration(Participant participant)
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
        public IActionResult Login(string nickname, string password)
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

            var projects = _context.ProjectParticipants.Where(pu => pu.ParticipantId == participant.Id).Select(pu => new { pu.ProjectId, pu.ProjectName }).Distinct().ToList();

            ViewBag.Projects = projects;
            ViewBag.Nickname = nickname;

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
    }
}
