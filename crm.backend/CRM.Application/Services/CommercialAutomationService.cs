using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DomainTask = crm.backend.CRM.Domain.Entities.Task;

namespace crm.backend.CRM.Application.Services
{
    public class CommercialAutomationService
    {
        private readonly AppDbContext _db;

        public CommercialAutomationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> RunQuoteReminders()
        {
            var settings = await _db.GeneralSettings.ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            if (!ParseBool(GetValue(settings, "quoteReminderEnabled", "true"), true))
                return 0;

            var days = ParseInt(GetValue(settings, "quoteReminderDays", "3"), 3, 1, 365);
            var cutoff = DateTime.UtcNow.AddDays(-days);
            var today = DateTime.UtcNow.Date;
            var userId = await _db.Users.OrderBy(user => user.Id).Select(user => user.Id).FirstOrDefaultAsync();

            if (userId <= 0)
                return 0;

            var statusId = await _db.Task_Statuses
                .OrderByDescending(status => status.Is_Default)
                .ThenBy(status => status.Position)
                .ThenBy(status => status.Id)
                .Select(status => (int?)status.Id)
                .FirstOrDefaultAsync();

            var quotes = await _db.Quotes
                .Include(quote => quote.Company)
                .Include(quote => quote.Contact)
                .Where(quote => quote.Status == "Inviato" && (quote.UpdatedAt ?? quote.CreatedAt) <= cutoff)
                .OrderBy(quote => quote.CreatedAt)
                .ToListAsync();

            var created = 0;
            foreach (var quote in quotes)
            {
                var marker = $"[QUOTE:{quote.Id}:FOLLOWUP]";
                var alreadyExists = await _db.Tasks.AnyAsync(task =>
                    task.internal_notes != null && task.internal_notes.Contains(marker));

                if (alreadyExists)
                    continue;

                var customer = !string.IsNullOrWhiteSpace(quote.Company?.name)
                    ? quote.Company.name
                    : quote.CustomerName;

                _db.Tasks.Add(new DomainTask
                {
                    Title = $"Richiamare cliente per preventivo {quote.Number}",
                    Description = $"Il preventivo {quote.Number} risulta Inviato da almeno {days} giorni. Contatta {customer} per follow-up commerciale.",
                    due_date = today,
                    due_time = "09:00",
                    activity_type = "call",
                    priority = "high",
                    reminder_at = today.AddHours(9),
                    internal_notes = $"{marker} Promemoria generato automaticamente dal modulo commerciale.",
                    all_day = false,
                    completed = false,
                    user_id = userId,
                    company_id = quote.CompanyId,
                    contact_id = quote.ContactId,
                    status_id = statusId
                });
                created++;
            }

            if (created > 0)
                await _db.SaveChangesAsync();

            return created;
        }

        private static string GetValue(Dictionary<string, string> values, string key, string fallback)
        {
            return values.TryGetValue(key, out var value) ? value : fallback;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            return bool.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static int ParseInt(string value, int fallback, int min, int max)
        {
            if (!int.TryParse(value, out var parsed))
                return fallback;

            return Math.Clamp(parsed, min, max);
        }
    }
}
