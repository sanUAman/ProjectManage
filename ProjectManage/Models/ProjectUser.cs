using System.ComponentModel.DataAnnotations;

namespace ProjectManage.Models
{
    public class ProjectUser
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }


        [Required]
        public string Role { get; set; }

        [Required]
        public string Tasks { get; set; }

        public bool Status { get; set; }
    }
}