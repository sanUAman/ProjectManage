using System.ComponentModel.DataAnnotations;

namespace ProjectManage.Models
{
    public class Participant
    {
        public int Id { get; set; }

        public required string nickname { get; set; }

        public required string password { get; set; }
    }
}
