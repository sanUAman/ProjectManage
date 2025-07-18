using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectManage.Models;
using ProjectManage.Data;
using System.Data;

namespace ProjectManage.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ParticipantProject(int id, string name)
        {
            var participantId = HttpContext.Session.GetInt32("participantId");
            var nickname = HttpContext.Session.GetString("nickname");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == id && p.Name == name);

            if (project == null)
            {
                return NotFound("This project doesn't exist! (Server error)");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(p => p.ParticipantName == nickname && p.ParticipantId == participantId);

            string role = "";

            if (participant != null && (participant.Role != "CEO" || participant.Role != "Manager"))
            {
                role = "Participant";
            }

            if (participant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var participantIds = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).Select(pu => pu.ParticipantId).ToList();

            var participants = _context.Participants.Where(u => participantIds.Contains(u.Id)).ToList();

            var projectParticipants = _context.ProjectParticipants.Where(pu => pu.ProjectId == id).ToList();

            int totalParticipants = projectParticipants.Count;
            int completedTasks = projectParticipants.Count(pu => pu.Status);

            ViewBag.Participants = participants;
            ViewBag.CompletionPercentage = totalParticipants > 0 ? (completedTasks * 100) / totalParticipants : 0;
            ViewBag.UserRole = role;

            HttpContext.Session.SetInt32("participantId", participant.ParticipantId);
            HttpContext.Session.SetInt32("projectId", project.Id);
            HttpContext.Session.SetString("projectName", project.Name);

            return View(project);
        }

        public IActionResult Info(int id, string nickname)
        {
            var participantId = HttpContext.Session.GetInt32("participantId");
            var projectId = HttpContext.Session.GetInt32("projectId");
            var projectName = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId && p.Name == projectName);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var mainParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            if (mainParticipant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var participant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == id && pu.ParticipantName == nickname && pu.ProjectId == projectId);

            if (participant == null)
            {
                return NotFound("Participant not found! (Server error)");
            }

            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.Name;
            ViewBag.CanEditStatus = string.Equals(mainParticipant.ParticipantName, nickname, StringComparison.OrdinalIgnoreCase);

            HttpContext.Session.SetInt32("targetParticipantId", participant.ParticipantId);

            return View(participant);
        }

        public IActionResult Save(ProjectParticipant updatedParticipant)
        {
            var targetId = HttpContext.Session.GetInt32("targetParticipantId");
            var participantId = HttpContext.Session.GetInt32("participantId");
            var projectId = HttpContext.Session.GetInt32("projectId");
            var projectName = HttpContext.Session.GetString("projectName");

            var project = _context.NamesOfProjects.FirstOrDefault(p => p.Id == projectId && p.Name == projectName);

            if (project == null)
            {
                return NotFound("This project doesn't exist (Server error)");
            }

            var mainParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == participantId && pu.ProjectId == projectId);

            if (mainParticipant == null)
            {
                return NotFound("Your data isn`t valid! (Server error)");
            }

            var existingParticipant = _context.ProjectParticipants.FirstOrDefault(pu => pu.ParticipantId == targetId && pu.ProjectId == projectId);

            if (existingParticipant == null)
            {
                return NotFound("Participant not found! (Server error)");
            }

            existingParticipant.Status = updatedParticipant.Status;

            _context.SaveChanges();

            return RedirectToAction("ParticipantProject", "Participant", new { id = projectId, name = projectName });
        }
    }
}