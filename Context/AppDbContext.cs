using MeetingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MeetingApp.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            builder.Entity<Meeting>().ToTable(tb => tb.HasTrigger("trg_Meeting_Delete"));

            builder.Entity<Meeting>().HasOne(m => m.Document).WithOne(d => d.Meeting).HasForeignKey<Document>(d => d.MeetingId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
