using Microsoft.EntityFrameworkCore;
using TeamManager.Models;

namespace TeamManager.Data
{
    public class TeamManagerContext : DbContext
    {
        public TeamManagerContext(DbContextOptions<TeamManagerContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamAssignment> TeamAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<Team>().ToTable("Team");
            modelBuilder.Entity<TeamAssignment>().ToTable("TeamAssignment");

            modelBuilder.Entity<TeamAssignment>().HasKey(t => new { t.PlayerId, t.TeamId });
        }
    }
}
