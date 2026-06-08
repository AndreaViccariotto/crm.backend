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
        public DbSet<Article> Articles { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteLine> QuoteLines { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        public DbSet<GeneralSetting> GeneralSettings { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Intervention> Interventions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomField>(entity =>
            {
                entity.ToTable("custom_fields");
                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(64);
                entity.Property(x => x.FieldName).HasColumnName("field_name").HasMaxLength(120);
                entity.Property(x => x.Label).HasColumnName("label").HasMaxLength(180);
                entity.Property(x => x.FieldType).HasColumnName("field_type").HasMaxLength(40);
                entity.Property(x => x.Options).HasColumnName("options").HasColumnType("text");
                entity.Property(x => x.IsRequired).HasColumnName("is_required");
                entity.Property(x => x.Active).HasColumnName("active");
                entity.Property(x => x.SortOrder).HasColumnName("sort_order");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
                entity.HasIndex(x => new { x.EntityName, x.FieldName }).IsUnique();
            });

            modelBuilder.Entity<CustomFieldValue>(entity =>
            {
                entity.ToTable("custom_field_values");
                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.CustomFieldId).HasColumnName("custom_field_id");
                entity.Property(x => x.EntityId).HasColumnName("entity_id");
                entity.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(64);
                entity.Property(x => x.Value).HasColumnName("value").HasColumnType("text");
                entity.HasIndex(x => new { x.EntityName, x.EntityId });
                entity.HasIndex(x => new { x.CustomFieldId, x.EntityId }).IsUnique();
                entity.HasOne(x => x.CustomField)
                    .WithMany(x => x.Values)
                    .HasForeignKey(x => x.CustomFieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>()
                .ToTable("users")
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Permission>().ToTable("permissions");
            modelBuilder.Entity<Module>().ToTable("modules");

            modelBuilder.Entity<RolePermission>()
                .ToTable("role_permissions")
                .HasKey(x => new { x.RoleId, x.PermissionId });
            modelBuilder.Entity<RolePermission>().HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
            modelBuilder.Entity<RolePermission>().HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);

            modelBuilder.Entity<RoleModule>()
                .ToTable("role_modules")
                .HasKey(x => new { x.RoleId, x.ModuleId });
            modelBuilder.Entity<RoleModule>().HasOne(x => x.Role).WithMany(x => x.RoleModules).HasForeignKey(x => x.RoleId);
            modelBuilder.Entity<RoleModule>().HasOne(x => x.Module).WithMany(x => x.RoleModules).HasForeignKey(x => x.ModuleId);

            modelBuilder.Entity<Domain.Entities.Task>()
                .HasOne(x => x.Ticket)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ticket_id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");
                entity.HasIndex(x => x.Number).IsUnique();
                entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Intervention>(entity =>
            {
                entity.ToTable("interventions");
                entity.HasIndex(x => x.Number).IsUnique();
                entity.HasIndex(x => x.TaskId).IsUnique();
                entity.HasOne(x => x.Ticket).WithMany(x => x.Interventions).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Task).WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Article>().ToTable("articles").HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Article>().Property(x => x.Price).HasPrecision(12, 2);
            modelBuilder.Entity<Article>().Property(x => x.VatRate).HasPrecision(5, 2);

            modelBuilder.Entity<Quote>().ToTable("quotes").HasIndex(x => x.Number).IsUnique();
            modelBuilder.Entity<Quote>().HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Quote>().HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<QuoteLine>().ToTable("quote_lines").HasOne(x => x.Quote).WithMany(x => x.Lines).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<QuoteLine>().HasOne(x => x.Article).WithMany(x => x.QuoteLines).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<QuoteLine>().Property(x => x.Quantity).HasPrecision(12, 2);
            modelBuilder.Entity<QuoteLine>().Property(x => x.UnitPrice).HasPrecision(12, 2);
            modelBuilder.Entity<QuoteLine>().Property(x => x.Discount).HasPrecision(5, 2);
            modelBuilder.Entity<QuoteLine>().Property(x => x.VatRate).HasPrecision(5, 2);

            modelBuilder.Entity<SalesOrder>().ToTable("sales_orders").HasIndex(x => x.Number).IsUnique();
            modelBuilder.Entity<SalesOrder>().HasIndex(x => x.QuoteId).IsUnique();
            modelBuilder.Entity<SalesOrder>().HasOne(x => x.Quote).WithOne(x => x.SalesOrder).HasForeignKey<SalesOrder>(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SalesOrder>().HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SalesOrder>().HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SalesOrderLine>().ToTable("sales_order_lines").HasOne(x => x.SalesOrder).WithMany(x => x.Lines).HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SalesOrderLine>().HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SalesOrderLine>().Property(x => x.Quantity).HasPrecision(12, 2);
            modelBuilder.Entity<SalesOrderLine>().Property(x => x.UnitPrice).HasPrecision(12, 2);
            modelBuilder.Entity<SalesOrderLine>().Property(x => x.Discount).HasPrecision(5, 2);
            modelBuilder.Entity<SalesOrderLine>().Property(x => x.VatRate).HasPrecision(5, 2);

            modelBuilder.Entity<PurchaseOrder>().ToTable("purchase_orders").HasIndex(x => x.Number).IsUnique();
            modelBuilder.Entity<PurchaseOrderLine>().ToTable("purchase_order_lines").HasOne(x => x.PurchaseOrder).WithMany(x => x.Lines).HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseOrderLine>().HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<PurchaseOrderLine>().Property(x => x.Quantity).HasPrecision(12, 2);
            modelBuilder.Entity<PurchaseOrderLine>().Property(x => x.UnitCost).HasPrecision(12, 2);
            modelBuilder.Entity<PurchaseOrderLine>().Property(x => x.VatRate).HasPrecision(5, 2);

            modelBuilder.Entity<GeneralSetting>().ToTable("general_settings").HasIndex(x => x.Key).IsUnique();
        }
    }
}

