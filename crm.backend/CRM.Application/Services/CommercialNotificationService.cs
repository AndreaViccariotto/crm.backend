using System.Security.Claims;
using System.Text.Json;
using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class CommercialNotificationService
    {
        private readonly AppDbContext _db;
        private readonly CommercialAutomationService _automation;
        private readonly GeneralSettingsService _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommercialNotificationService(
            AppDbContext db,
            CommercialAutomationService automation,
            GeneralSettingsService settings,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _automation = automation;
            _settings = settings;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<NotificationCenterDto> Get()
        {
            await _automation.RunQuoteReminders();

            var userId = GetCurrentUserId();
            var dismissedIds = await GetDismissedIds(userId);
            var items = new List<NotificationItemDto>();
            var now = DateTime.UtcNow;
            var tomorrow = now.Date.AddDays(1);

            var acceptedQuotes = await _db.Quotes
                .Where(quote => quote.Status == "Accettato" && (quote.UpdatedAt ?? quote.CreatedAt) >= now.AddDays(-30))
                .OrderByDescending(quote => quote.UpdatedAt ?? quote.CreatedAt)
                .Take(5)
                .ToListAsync();

            items.AddRange(acceptedQuotes.Select(quote => new NotificationItemDto
            {
                Id = $"quote-accepted:{quote.Id}",
                Type = "quote-accepted",
                Severity = "success",
                Title = $"Preventivo accettato: {quote.Number}",
                Description = $"{quote.CustomerName} ha un preventivo accettato. Verifica l'ordine vendita collegato.",
                CreatedAt = quote.UpdatedAt ?? quote.CreatedAt,
                Route = $"/quotes-edit/{quote.Id}",
                EntityId = quote.Id
            }));

            var dueTasksQuery = _db.Tasks.Where(task => !task.completed && task.due_date <= tomorrow);
            if (userId.HasValue)
                dueTasksQuery = dueTasksQuery.Where(task => task.user_id == userId.Value);

            var dueTasks = await dueTasksQuery.OrderBy(task => task.due_date).ThenBy(task => task.due_time).Take(10).ToListAsync();
            items.AddRange(dueTasks.Select(task => new NotificationItemDto
            {
                Id = $"task-due:{task.Id}",
                Type = "task-due",
                Severity = task.due_date.Date < now.Date ? "danger" : "warning",
                Title = task.due_date.Date < now.Date ? $"Task scaduta: {task.Title}" : $"Task in scadenza: {task.Title}",
                Description = $"Scadenza {task.due_date:dd/MM/yyyy}" + (string.IsNullOrWhiteSpace(task.due_time) ? "" : $" alle {task.due_time}"),
                CreatedAt = task.due_date,
                Route = $"/add-task/{task.Id}",
                EntityId = task.Id
            }));

            if (userId.HasValue)
            {
                var assignedTickets = await _db.Tickets
                    .Where(ticket => ticket.AssignedUserId == userId.Value && ticket.AssignedAt.HasValue && ticket.AssignedAt >= now.AddDays(-30))
                    .OrderByDescending(ticket => ticket.AssignedAt)
                    .Take(10)
                    .ToListAsync();

                items.AddRange(assignedTickets.Select(ticket => new NotificationItemDto
                {
                    Id = $"ticket-assigned:{ticket.Id}:{ticket.AssignedAt!.Value.Ticks}",
                    Type = "ticket-assigned",
                    Severity = ticket.Priority is "Urgente" or "Alta" ? "warning" : "info",
                    Title = $"Ticket assegnato: {ticket.Number}",
                    Description = ticket.Subject,
                    CreatedAt = ticket.AssignedAt!.Value,
                    Route = $"/tickets-edit/{ticket.Id}",
                    EntityId = ticket.Id
                }));

                var assistance = await _settings.GetAssistance();
                if (assistance.InterventionReminderEnabled)
                {
                    var cutoff = now.AddDays(-assistance.InterventionReminderDays);
                    var unsentInterventions = await _db.Interventions
                        .Where(item => item.Visibility == "public" && item.Status != "Inviato" && item.CreatedAt <= cutoff &&
                            (item.UserId == userId.Value || item.Ticket!.AssignedUserId == userId.Value))
                        .OrderBy(item => item.CreatedAt)
                        .Take(10)
                        .ToListAsync();

                    items.AddRange(unsentInterventions.Select(item => new NotificationItemDto
                    {
                        Id = $"intervention-unsent:{item.Id}:{assistance.InterventionReminderDays}",
                        Type = "intervention-unsent",
                        Severity = "warning",
                        Title = $"Rapporto da inviare: {item.Number}",
                        Description = $"Il rapporto pubblico non risulta inviato da almeno {assistance.InterventionReminderDays} giorni.",
                        CreatedAt = item.CreatedAt,
                        Route = $"/interventions-edit/{item.Id}",
                        EntityId = item.Id
                    }));
                }
            }

            var ordersToFulfill = await _db.SalesOrders
                .Where(order => order.Status == "Da evadere")
                .OrderByDescending(order => order.OrderDate)
                .Take(5)
                .ToListAsync();

            items.AddRange(ordersToFulfill.Select(order => new NotificationItemDto
            {
                Id = $"sales-order-open:{order.Id}",
                Type = "sales-order-open",
                Severity = "info",
                Title = $"Ordine da evadere: {order.Number}",
                Description = $"{order.CustomerName} attende evasione ordine.",
                CreatedAt = order.OrderDate,
                Route = "/sales-orders",
                EntityId = order.Id
            }));

            var disabledArticles = await _db.Articles
                .Where(article => !article.Active)
                .OrderByDescending(article => article.CreatedAt)
                .Take(5)
                .ToListAsync();

            items.AddRange(disabledArticles.Select(article => new NotificationItemDto
            {
                Id = $"article-disabled:{article.Id}",
                Type = "article-disabled",
                Severity = "warning",
                Title = $"Articolo disattivato: {article.Code}",
                Description = article.Name,
                CreatedAt = article.CreatedAt,
                Route = $"/articles-edit/{article.Id}",
                EntityId = article.Id
            }));

            var orderedItems = items
                .Where(item => !dismissedIds.Contains(item.Id))
                .OrderByDescending(item => item.CreatedAt)
                .Take(30)
                .ToList();

            return new NotificationCenterDto
            {
                UnreadCount = orderedItems.Count,
                Items = orderedItems
            };
        }

        public async Task<NotificationCenterDto> Dismiss(NotificationDismissRequest request)
        {
            var userId = GetCurrentUserId();
            var id = request.Id?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(id))
            {
                var ids = await GetDismissedIds(userId);
                ids.Add(id);
                var compactIds = ids
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .TakeLast(250)
                    .ToList();

                await Upsert(GetDismissedKey(userId), JsonSerializer.Serialize(compactIds));
                await _db.SaveChangesAsync();
                _db.ChangeTracker.Clear();
            }

            return await Get();
        }

        private int? GetCurrentUserId()
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private async Task<HashSet<string>> GetDismissedIds(int? userId)
        {
            var key = GetDismissedKey(userId);
            var value = await _db.GeneralSettings
                .AsNoTracking()
                .Where(setting => setting.Key == key)
                .Select(setting => setting.Value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(value))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var ids = JsonSerializer.Deserialize<List<string>>(value);
                return (ids ?? new List<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string GetDismissedKey(int? userId) => userId.HasValue
            ? $"dismissedNotifications:{userId.Value}"
            : "dismissedNotifications";

        private async System.Threading.Tasks.Task Upsert(string key, string? value)
        {
            var setting = await _db.GeneralSettings.FirstOrDefaultAsync(item => item.Key == key);
            if (setting == null)
            {
                _db.GeneralSettings.Add(new GeneralSetting { Key = key, Value = value ?? "" });
                return;
            }

            setting.Value = value ?? "";
        }
    }
}
