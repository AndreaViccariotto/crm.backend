using System.Text.Json;
using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class GeneralSettingsService
    {
        private readonly AppDbContext _db;

        public GeneralSettingsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<GeneralSettingsDto> Get()
        {
            var values = await _db.GeneralSettings.AsNoTracking().ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            return new GeneralSettingsDto
            {
                CompanyName = GetValue(values, "companyName", ""),
                Currency = GetValue(values, "currency", "EUR"),
                DefaultVatRate = decimal.TryParse(GetValue(values, "defaultVatRate", "22"), out var vat) ? vat : 22,
                QuotePrefix = GetValue(values, "quotePrefix", "PREV"),
                SalesOrderPrefix = GetValue(values, "salesOrderPrefix", "OV"),
                PurchaseOrderPrefix = GetValue(values, "purchaseOrderPrefix", "OA"),
                PaymentTerms = GetValue(values, "paymentTerms", ""),
                QuoteFooterNotes = GetValue(values, "quoteFooterNotes", "")
            };
        }

        public async Task<GeneralSettingsDto> Save(GeneralSettingsDto settings)
        {
            await Upsert("companyName", settings.CompanyName);
            await Upsert("currency", settings.Currency);
            await Upsert("defaultVatRate", settings.DefaultVatRate.ToString());
            await Upsert("quotePrefix", settings.QuotePrefix);
            await Upsert("salesOrderPrefix", settings.SalesOrderPrefix);
            await Upsert("purchaseOrderPrefix", settings.PurchaseOrderPrefix);
            await Upsert("paymentTerms", settings.PaymentTerms);
            await Upsert("quoteFooterNotes", settings.QuoteFooterNotes);
            await _db.SaveChangesAsync();
            return await Get();
        }

        public async Task<CommercialSettingsDto> GetCommercial()
        {
            var values = await _db.GeneralSettings.AsNoTracking().ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            var articleCategories = await _db.Articles
                .Where(article => article.Category != "")
                .Select(article => article.Category)
                .Distinct()
                .ToListAsync();
            var articleUnits = await _db.Articles
                .Where(article => article.Unit != "")
                .Select(article => article.Unit)
                .Distinct()
                .ToListAsync();

            var savedCategories = ParseList(GetValue(values, "articleCategories", ""), new[] { "Servizi", "Prodotti", "Interventi" });
            var savedUnits = ParseList(GetValue(values, "articleUnits", ""), new[] { "pz", "ora", "giorno", "mese" });
            var generalCompanyName = GetValue(values, "companyName", "");
            var generalPaymentTerms = GetValue(values, "paymentTerms", "");
            var generalFooterNotes = GetValue(values, "quoteFooterNotes", "");
            var logoContentType = GetValue(values, "quoteTemplateLogoContentType", "");

            return new CommercialSettingsDto
            {
                ArticleCategories = MergeLists(savedCategories, articleCategories),
                ArticleUnits = MergeLists(savedUnits, articleUnits),
                QuoteReminderEnabled = ParseBool(GetValue(values, "quoteReminderEnabled", "true"), true),
                QuoteReminderDays = ParseInt(GetValue(values, "quoteReminderDays", "3"), 3, 1, 365),
                QuoteTemplateCompanyName = GetNonEmptyValue(values, "quoteTemplateCompanyName", generalCompanyName),
                QuoteTemplateLogoUrl = GetValue(values, "quoteTemplateLogoUrl", ""),
                QuoteTemplateLogoFileName = GetValue(values, "quoteTemplateLogoFileName", ""),
                QuoteTemplateLogoContentType = logoContentType,
                QuoteTemplateLogoDataUrl = await ReadLogoDataUrl(values, logoContentType),
                QuoteTemplateBrandColor = NormalizeHexColor(GetValue(values, "quoteTemplateBrandColor", "#14b8a6")),
                QuoteTemplatePaymentTerms = GetNonEmptyValue(values, "quoteTemplatePaymentTerms", generalPaymentTerms),
                QuoteTemplateFooterNotes = GetNonEmptyValue(values, "quoteTemplateFooterNotes", generalFooterNotes),
                QuoteTemplateSignatureLabel = GetNonEmptyValue(values, "quoteTemplateSignatureLabel", "Firma per accettazione"),
                QuoteTemplateShowSignature = ParseBool(GetValue(values, "quoteTemplateShowSignature", "true"), true)
            };
        }

        public async Task<CommercialSettingsDto> SaveCommercial(CommercialSettingsDto settings)
        {
            await Upsert("articleCategories", JsonSerializer.Serialize(Clean(settings.ArticleCategories)));
            await Upsert("articleUnits", JsonSerializer.Serialize(Clean(settings.ArticleUnits)));
            await Upsert("quoteReminderEnabled", settings.QuoteReminderEnabled ? "true" : "false");
            await Upsert("quoteReminderDays", Math.Clamp(settings.QuoteReminderDays, 1, 365).ToString());
            await Upsert("quoteTemplateCompanyName", CleanText(settings.QuoteTemplateCompanyName));
            await Upsert("quoteTemplateLogoUrl", CleanText(settings.QuoteTemplateLogoUrl));
            await Upsert("quoteTemplateBrandColor", NormalizeHexColor(settings.QuoteTemplateBrandColor));
            await Upsert("quoteTemplatePaymentTerms", CleanText(settings.QuoteTemplatePaymentTerms));
            await Upsert("quoteTemplateFooterNotes", CleanText(settings.QuoteTemplateFooterNotes));
            await Upsert("quoteTemplateSignatureLabel", CleanText(settings.QuoteTemplateSignatureLabel));
            await Upsert("quoteTemplateShowSignature", settings.QuoteTemplateShowSignature ? "true" : "false");
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return await GetCommercial();
        }

        public async Task<CommercialSettingsDto> UploadQuoteLogo(QuoteLogoUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new Exception("Logo non valido");

            var contentType = CleanText(request.ContentType).ToLowerInvariant();
            var extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new Exception("Formato logo non supportato")
            };

            var content = request.Content.Contains(',') ? request.Content.Split(',').Last() : request.Content;
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(content);
            }
            catch
            {
                throw new Exception("Contenuto logo non valido");
            }

            if (bytes.Length > 2 * 1024 * 1024)
                throw new Exception("Il logo non puo superare 2 MB");

            var values = await _db.GeneralSettings.AsNoTracking().ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            DeletePhysicalLogo(GetValue(values, "quoteTemplateLogoPath", ""));

            var directory = Path.Combine("uploads", "settings");
            Directory.CreateDirectory(directory);

            var safeName = Path.GetFileNameWithoutExtension(request.FileName);
            safeName = string.IsNullOrWhiteSpace(safeName) ? "logo-preventivo" : safeName;
            var fileName = $"{Guid.NewGuid():N}_{safeName}{extension}";
            var path = Path.Combine(directory, fileName);
            await System.IO.File.WriteAllBytesAsync(path, bytes);

            await Upsert("quoteTemplateLogoPath", path);
            await Upsert("quoteTemplateLogoFileName", Path.GetFileName(request.FileName));
            await Upsert("quoteTemplateLogoContentType", contentType == "image/jpg" ? "image/jpeg" : contentType);
            await Upsert("quoteTemplateLogoUrl", "");
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return await GetCommercial();
        }

        public async Task<CommercialSettingsDto> DeleteQuoteLogo()
        {
            var values = await _db.GeneralSettings.AsNoTracking().ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            DeletePhysicalLogo(GetValue(values, "quoteTemplateLogoPath", ""));

            await Upsert("quoteTemplateLogoPath", "");
            await Upsert("quoteTemplateLogoFileName", "");
            await Upsert("quoteTemplateLogoContentType", "");
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return await GetCommercial();
        }

        public async Task<AssistanceSettingsDto> GetAssistance()
        {
            var values = await _db.GeneralSettings.AsNoTracking().ToDictionaryAsync(item => item.Key, item => item.Value ?? "");
            var logoContentType = GetValue(values, "quoteTemplateLogoContentType", "");

            return new AssistanceSettingsDto
            {
                InterventionReminderEnabled = ParseBool(GetValue(values, "interventionReminderEnabled", "true"), true),
                InterventionReminderDays = ParseInt(GetValue(values, "interventionReminderDays", "3"), 3, 1, 365),
                PublicActivityTypes = ParseList(
                    GetValue(values, "supportPublicActivityTypes", ""),
                    new[] { "generic", "appointment", "intervention", "call", "email", "reminder" }),
                AutoCloseTicketWhenAllTasksCompleted = ParseBool(GetValue(values, "supportAutoCloseTicket", "false"), false),
                InterventionTemplateCompanyName = GetNonEmptyValue(
                    values,
                    "interventionTemplateCompanyName",
                    GetNonEmptyValue(values, "quoteTemplateCompanyName", GetValue(values, "companyName", ""))),
                InterventionTemplateLogoUrl = GetValue(values, "quoteTemplateLogoUrl", ""),
                InterventionTemplateLogoFileName = GetValue(values, "quoteTemplateLogoFileName", ""),
                InterventionTemplateLogoDataUrl = await ReadLogoDataUrl(values, logoContentType),
                InterventionTemplateBrandColor = NormalizeHexColor(GetValue(values, "interventionTemplateBrandColor", "#0f766e")),
                InterventionTemplateFooterNotes = GetValue(values, "interventionTemplateFooterNotes", ""),
                InterventionTemplateSignatureLabel = GetNonEmptyValue(values, "interventionTemplateSignatureLabel", "Firma del cliente"),
                InterventionTemplateShowSignature = ParseBool(GetValue(values, "interventionTemplateShowSignature", "true"), true),
                InterventionTemplateIncludeInternalNotes = ParseBool(GetValue(values, "interventionTemplateIncludeInternalNotes", "false"), false)
            };
        }

        public async Task<AssistanceSettingsDto> SaveAssistance(AssistanceSettingsDto settings)
        {
            await Upsert("interventionReminderEnabled", settings.InterventionReminderEnabled ? "true" : "false");
            await Upsert("interventionReminderDays", Math.Clamp(settings.InterventionReminderDays, 1, 365).ToString());
            await Upsert("supportPublicActivityTypes", JsonSerializer.Serialize(Clean(settings.PublicActivityTypes)));
            await Upsert("supportAutoCloseTicket", settings.AutoCloseTicketWhenAllTasksCompleted ? "true" : "false");
            await Upsert("interventionTemplateCompanyName", CleanText(settings.InterventionTemplateCompanyName));
            await Upsert("interventionTemplateBrandColor", NormalizeHexColor(settings.InterventionTemplateBrandColor));
            await Upsert("interventionTemplateFooterNotes", CleanText(settings.InterventionTemplateFooterNotes));
            await Upsert("interventionTemplateSignatureLabel", CleanText(settings.InterventionTemplateSignatureLabel));
            await Upsert("interventionTemplateShowSignature", settings.InterventionTemplateShowSignature ? "true" : "false");
            await Upsert("interventionTemplateIncludeInternalNotes", settings.InterventionTemplateIncludeInternalNotes ? "true" : "false");
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return await GetAssistance();
        }
        public async Task<ClientModulesDto> GetClientModules()
        {
            var modules = await _db.Modules
                .OrderBy(module => module.Name)
                .ToListAsync();

            var activeSetting = await _db.GeneralSettings.FirstOrDefaultAsync(item => item.Key == "activeModules");
            var activeModules = activeSetting == null
                ? modules.Select(module => module.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : ParseList(activeSetting.Value ?? "", Array.Empty<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return new ClientModulesDto
            {
                Modules = modules.Select(module => new ClientModuleItemDto
                {
                    Name = module.Name,
                    Description = module.Description ?? "",
                    Active = activeModules.Contains(module.Name)
                }).ToList()
            };
        }

        public async Task<ClientModulesDto> SaveClientModules(ClientModulesSaveRequest request)
        {
            var existingModules = await _db.Modules.Select(module => module.Name).ToListAsync();
            var existing = existingModules.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var selected = Clean(request.Modules)
                .Where(module => existing.Contains(module))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            await Upsert("activeModules", JsonSerializer.Serialize(selected));
            await _db.SaveChangesAsync();

            return await GetClientModules();
        }

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

        private static async Task<string> ReadLogoDataUrl(Dictionary<string, string> values, string contentType)
        {
            var path = GetValue(values, "quoteTemplateLogoPath", "");
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(contentType) || !System.IO.File.Exists(path))
                return "";

            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
        }

        private static void DeletePhysicalLogo(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private static string GetValue(Dictionary<string, string> values, string key, string fallback)
        {
            return values.TryGetValue(key, out var value) ? value : fallback;
        }

        private static string GetNonEmptyValue(Dictionary<string, string> values, string key, string fallback)
        {
            var value = GetValue(values, key, fallback);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static List<string> ParseList(string value, IEnumerable<string> fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<string>>(value);
                    if (parsed != null)
                        return Clean(parsed);
                }
                catch
                {
                    return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
            }

            return fallback.ToList();
        }

        private static List<string> Clean(IEnumerable<string>? values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Select(value => value?.Trim() ?? "")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> MergeLists(params IEnumerable<string>[] sources)
        {
            return sources
                .SelectMany(source => source ?? Enumerable.Empty<string>())
                .Select(value => value?.Trim() ?? "")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
                return false;

            return bool.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static int ParseInt(string value, int fallback, int min, int max)
        {
            if (!int.TryParse(value, out var parsed))
                return fallback;

            return Math.Clamp(parsed, min, max);
        }

        private static string CleanText(string? value)
        {
            return value?.Trim() ?? "";
        }

        private static string NormalizeHexColor(string? value)
        {
            var color = string.IsNullOrWhiteSpace(value) ? "#14b8a6" : value.Trim();
            color = color.StartsWith("#") ? color : $"#{color}";
            return System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9a-fA-F]{6}$") ? color : "#14b8a6";
        }
    }
}



