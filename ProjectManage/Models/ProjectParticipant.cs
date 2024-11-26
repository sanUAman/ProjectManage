using System.ComponentModel.DataAnnotations;

namespace ProjectManage.Models
{
    public class ProjectParticipant
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantPassword { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }


        [Required]
        public string Role { get; set; }

        [Required]
        public string Tasks { get; set; }

        public bool Status { get; set; }
    }
}