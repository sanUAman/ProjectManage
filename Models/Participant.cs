using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManage.Models
{
    public class Participant
    {
        public int Id { get; set; }

        public required string nickname { get; set; }

        [NotMapped]
        public required string password { get; set; }
        public required string passwordHash { get; set; }
    }
}
