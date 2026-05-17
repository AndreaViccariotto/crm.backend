using crm.backend.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<CustomFieldValue> CustomFieldValues { get; set; }
        public DbSet<Domain.Entities.Task> Tasks { get; set; }
        public DbSet<Domain.Entities.TaskStatus> Task_Statuses { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Domain.Entities.File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomFieldValue>()
                .HasIndex(x => new { x.EntityName, x.EntityId });

            modelBuilder.Entity<CustomFieldValue>()
                .HasIndex(x => x.CustomFieldId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role);
        }
    }
}
