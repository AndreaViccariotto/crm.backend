using System.Text.Json;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services;

public static class DatabaseInitializer
{
    public static async System.Threading.Tasks.Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var created = await db.Database.EnsureCreatedAsync();
        await SeedRolesAndAccessAsync(db);
        await SeedTaskStatusesAsync(db);
        await SeedSettingsAsync(db);
        await SeedAdministratorAsync(db, configuration);

        logger.LogInformation(
            created
                ? "Database CRM creato e inizializzato."
                : "Bootstrap database CRM verificato.");
    }

    private static async System.Threading.Tasks.Task SeedRolesAndAccessAsync(AppDbContext db)
    {
        var roleDefinitions = new Dictionary<string, string>
        {
            ["ADMIN"] = "Accesso completo",
            ["USER"] = "Accesso standard"
        };
        var permissionDefinitions = new Dictionary<string, string>
        {
            ["users.read"] = "Visualizzare utenti",
            ["users.write"] = "Creare e modificare utenti",
            ["roles.read"] = "Visualizzare ruoli",
            ["roles.write"] = "Creare, modificare ed eliminare ruoli",
            ["crm.read"] = "Visualizzare dati CRM",
            ["crm.write"] = "Creare e modificare dati CRM",
            ["crm.delete"] = "Eliminare dati CRM"
        };
        var moduleDefinitions = new Dictionary<string, string>
        {
            ["users"] = "Gestione utenti",
            ["roles"] = "Gestione ruoli",
            ["companies"] = "Aziende",
            ["contacts"] = "Contatti",
            ["tasks"] = "Attivita",
            ["files"] = "File",
            ["articles"] = "Articoli",
            ["quotes"] = "Preventivi",
            ["sales-orders"] = "Ordini vendita",
            ["purchase-orders"] = "Ordini acquisto",
            ["settings"] = "Impostazioni",
            ["support"] = "Assistenza"
        };

        await AddMissingAsync(
            await db.Roles.ToDictionaryAsync(item => item.Name),
            roleDefinitions,
            (name, description) => db.Roles.Add(new Role { Name = name, Description = description }));
        await AddMissingAsync(
            await db.Permissions.ToDictionaryAsync(item => item.Name),
            permissionDefinitions,
            (name, description) => db.Permissions.Add(new Permission { Name = name, Description = description }));
        await AddMissingAsync(
            await db.Modules.ToDictionaryAsync(item => item.Name),
            moduleDefinitions,
            (name, description) => db.Modules.Add(new Module { Name = name, Description = description }));
        await db.SaveChangesAsync();

        var roles = await db.Roles.ToDictionaryAsync(item => item.Name);
        var permissions = await db.Permissions.ToDictionaryAsync(item => item.Name);
        var modules = await db.Modules.ToDictionaryAsync(item => item.Name);

        await AddRolePermissionsAsync(db, roles["ADMIN"], permissions.Values);
        await AddRoleModulesAsync(db, roles["ADMIN"], modules.Values);
        await AddRolePermissionsAsync(db, roles["USER"], permissions.Values.Where(item =>
            item.Name is "crm.read" or "crm.write"));
        await AddRoleModulesAsync(db, roles["USER"], modules.Values.Where(item =>
            item.Name is "companies" or "contacts" or "tasks" or "files" or "articles" or "quotes"
                or "sales-orders" or "purchase-orders" or "support"));

        await db.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task AddMissingAsync<T>(
        IDictionary<string, T> existing,
        IDictionary<string, string> definitions,
        Action<string, string> add)
    {
        foreach (var definition in definitions.Where(item => !existing.ContainsKey(item.Key)))
            add(definition.Key, definition.Value);

        await System.Threading.Tasks.Task.CompletedTask;
    }

    private static async System.Threading.Tasks.Task AddRolePermissionsAsync(
        AppDbContext db,
        Role role,
        IEnumerable<Permission> permissions)
    {
        var assigned = (await db.RolePermissions
            .Where(item => item.RoleId == role.Id)
            .Select(item => item.PermissionId)
            .ToListAsync()).ToHashSet();

        foreach (var permission in permissions.Where(item => !assigned.Contains(item.Id)))
            db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
    }

    private static async System.Threading.Tasks.Task AddRoleModulesAsync(
        AppDbContext db,
        Role role,
        IEnumerable<Module> modules)
    {
        var assigned = (await db.RoleModules
            .Where(item => item.RoleId == role.Id)
            .Select(item => item.ModuleId)
            .ToListAsync()).ToHashSet();

        foreach (var module in modules.Where(item => !assigned.Contains(item.Id)))
            db.RoleModules.Add(new RoleModule { RoleId = role.Id, ModuleId = module.Id });
    }

    private static async System.Threading.Tasks.Task SeedTaskStatusesAsync(AppDbContext db)
    {
        if (await db.Task_Statuses.AnyAsync())
            return;

        db.Task_Statuses.AddRange(
            new crm.backend.CRM.Domain.Entities.TaskStatus { Name = "Da fare", Is_Default = true, Position = 1 },
            new crm.backend.CRM.Domain.Entities.TaskStatus { Name = "In corso", Position = 2 },
            new crm.backend.CRM.Domain.Entities.TaskStatus { Name = "Completata", Position = 3 });
        await db.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task SeedSettingsAsync(AppDbContext db)
    {
        var modules = await db.Modules.OrderBy(item => item.Name).Select(item => item.Name).ToListAsync();
        var defaults = new Dictionary<string, string>
        {
            ["activeModules"] = JsonSerializer.Serialize(modules),
            ["currency"] = "EUR",
            ["defaultVatRate"] = "22",
            ["quotePrefix"] = "PREV",
            ["salesOrderPrefix"] = "OV",
            ["purchaseOrderPrefix"] = "OA",
            ["articleCategories"] = JsonSerializer.Serialize(new[] { "Servizi", "Prodotti", "Interventi" }),
            ["articleUnits"] = JsonSerializer.Serialize(new[] { "pz", "ora", "giorno", "mese" }),
            ["quoteReminderEnabled"] = "true",
            ["quoteReminderDays"] = "3",
            ["quoteTemplateBrandColor"] = "#14b8a6",
            ["quoteTemplateSignatureLabel"] = "Firma per accettazione",
            ["quoteTemplateShowSignature"] = "true",
            ["interventionReminderEnabled"] = "true",
            ["interventionReminderDays"] = "3",
            ["supportPublicActivityTypes"] = JsonSerializer.Serialize(
                new[] { "generic", "appointment", "intervention", "call", "email", "reminder" }),
            ["supportAutoCloseTicket"] = "false",
            ["interventionTemplateBrandColor"] = "#0f766e",
            ["interventionTemplateSignatureLabel"] = "Firma del cliente",
            ["interventionTemplateShowSignature"] = "true",
            ["interventionTemplateIncludeInternalNotes"] = "false"
        };
        var existing = (await db.GeneralSettings.Select(item => item.Key).ToListAsync()).ToHashSet();

        db.GeneralSettings.AddRange(defaults
            .Where(item => !existing.Contains(item.Key))
            .Select(item => new GeneralSetting { Key = item.Key, Value = item.Value }));
        await db.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task SeedAdministratorAsync(
        AppDbContext db,
        IConfiguration configuration)
    {
        if (await db.Users.AnyAsync())
            return;

        var password = configuration["Bootstrap:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        if (password.Length < 12)
            throw new InvalidOperationException(
                "Bootstrap:AdminPassword deve essere configurata con almeno 12 caratteri per inizializzare il database.");

        var adminRole = await db.Roles.SingleAsync(item => item.Name == "ADMIN");
        db.Users.Add(new User
        {
            username = configuration["Bootstrap:AdminUsername"] ?? "admin",
            Email = configuration["Bootstrap:AdminEmail"] ?? "admin@example.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = adminRole.Id
        });
        await db.SaveChangesAsync();
    }
}
