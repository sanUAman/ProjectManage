using System.ComponentModel.DataAnnotations;

namespace ProjectManage.Models
{
    public class ProjectUser
    {
        public int Id { get; set; }

        public User user { get; set; }

        public NameOfProject nameofproject { get; set; }


        [Required]
        public string role { get; set; }

        public string tasks { get; set; }
    }
}