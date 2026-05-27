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
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RoleModule> RoleModules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomFieldValue>()
                .HasIndex(x => new { x.EntityName, x.EntityId });

            modelBuilder.Entity<CustomFieldValue>()
                .HasIndex(x => x.CustomFieldId);

            modelBuilder.Entity<User>()
                .ToTable("users")
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>()
                .ToTable("roles");

            modelBuilder.Entity<Permission>()
                .ToTable("permissions");

            modelBuilder.Entity<Module>()
                .ToTable("modules");

            modelBuilder.Entity<RolePermission>()
                .ToTable("role_permissions")
                .HasKey(x => new { x.RoleId, x.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId);

            modelBuilder.Entity<RoleModule>()
                .ToTable("role_modules")
                .HasKey(x => new { x.RoleId, x.ModuleId });

            modelBuilder.Entity<RoleModule>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RoleModules)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RoleModule>()
                .HasOne(x => x.Module)
                .WithMany(x => x.RoleModules)
                .HasForeignKey(x => x.ModuleId);
        }
    }
}
