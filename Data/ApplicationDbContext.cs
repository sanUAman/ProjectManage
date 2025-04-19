using Microsoft.EntityFrameworkCore;
using ProjectManage.Models;

namespace ProjectManage.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Participant> Participants { get; set; }

        public DbSet<NameOfProject> NamesOfProjects { get; set; }

        public DbSet<ProjectParticipant> ProjectParticipants { get; set; }
    }
}