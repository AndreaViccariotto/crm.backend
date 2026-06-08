using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class CustomFieldService
    {
        private static readonly HashSet<string> SupportedModules = new(StringComparer.OrdinalIgnoreCase)
        {
            "companies",
            "contacts",
            "tasks",
            "articles",
            "quotes",
            "purchase-orders"
        };

        private static readonly HashSet<string> SupportedFieldTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text",
            "textarea",
            "number",
            "date",
            "checkbox",
            "select",
            "email",
            "phone",
            "url"
        };

        private readonly AppDbContext _db;

        public CustomFieldService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CustomFieldSettingsDto> GetSettings()
        {
            var modules = await GetVisibleModules();
            var moduleNames = modules.Select(module => module.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var fields = await _db.CustomFields
                .AsNoTracking()
                .Where(field => moduleNames.Contains(field.EntityName))
                .OrderBy(field => field.EntityName)
                .ThenBy(field => field.SortOrder)
                .ThenBy(field => field.Label)
                .Select(field => ToDefinition(field))
                .ToListAsync();

            return new CustomFieldSettingsDto
            {
                Modules = modules,
                Fields = fields
            };
        }

        public async Task<CustomFieldSettingsDto> SaveSettings(CustomFieldSettingsDto settings)
        {
            var modules = await GetVisibleModules();
            var visibleModuleNames = modules.Select(module => module.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var incoming = (settings.Fields ?? new List<CustomFieldDefinitionDto>())
                .Where(field => visibleModuleNames.Contains(field.ModuleName))
                .Select((field, index) => NormalizeDefinition(field, index))
                .Where(field => !string.IsNullOrWhiteSpace(field.Label))
                .ToList();

            var duplicates = incoming
                .GroupBy(field => $"{field.ModuleName}:{field.FieldName}", StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicates != null)
                throw new Exception("Esistono campi custom duplicati nello stesso modulo");

            var existing = await _db.CustomFields
                .Where(field => visibleModuleNames.Contains(field.EntityName))
                .ToListAsync();

            var incomingIds = incoming.Where(field => field.Id > 0).Select(field => field.Id).ToHashSet();
            var removed = existing.Where(field => !incomingIds.Contains(field.Id)).ToList();

            if (removed.Count > 0)
            {
                var removedIds = removed.Select(field => field.Id).ToList();
                var removedValues = await _db.CustomFieldValues
                    .Where(value => removedIds.Contains(value.CustomFieldId))
                    .ToListAsync();

                _db.CustomFieldValues.RemoveRange(removedValues);
                _db.CustomFields.RemoveRange(removed);
            }

            foreach (var item in incoming)
            {
                var field = item.Id > 0
                    ? existing.FirstOrDefault(existingField => existingField.Id == item.Id)
                    : null;

                if (field == null)
                {
                    field = new CustomField
                    {
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.CustomFields.Add(field);
                }

                field.EntityName = item.ModuleName;
                field.FieldName = item.FieldName;
                field.Label = item.Label;
                field.FieldType = item.FieldType;
                field.Options = item.Options;
                field.IsRequired = item.IsRequired;
                field.Active = item.Active;
                field.SortOrder = item.SortOrder;
                field.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return await GetSettings();
        }

        public async Task<List<CustomFieldDefinitionDto>> GetDefinitions(string moduleName)
        {
            var normalizedModule = NormalizeModuleName(moduleName);
            if (!await IsModuleVisible(normalizedModule))
                return new List<CustomFieldDefinitionDto>();

            return await _db.CustomFields
                .AsNoTracking()
                .Where(field => field.EntityName == normalizedModule && field.Active)
                .OrderBy(field => field.SortOrder)
                .ThenBy(field => field.Label)
                .Select(field => ToDefinition(field))
                .ToListAsync();
        }

        public async Task<Dictionary<string, string?>> GetValues(string moduleName, int entityId)
        {
            var definitions = await GetDefinitions(moduleName);
            if (definitions.Count == 0)
                return new Dictionary<string, string?>();

            var fieldIds = definitions.Select(field => field.Id).ToList();
            var values = await _db.CustomFieldValues
                .AsNoTracking()
                .Where(value => value.EntityId == entityId && fieldIds.Contains(value.CustomFieldId))
                .ToListAsync();

            return definitions.ToDictionary(
                field => field.FieldName,
                field => values.FirstOrDefault(value => value.CustomFieldId == field.Id)?.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        public async System.Threading.Tasks.Task SaveValues(string moduleName, int entityId, Dictionary<string, string?>? customFields)
        {
            var definitions = await GetDefinitions(moduleName);
            if (definitions.Count == 0)
                return;

            customFields ??= new Dictionary<string, string?>();
            var fieldIds = definitions.Select(field => field.Id).ToList();
            var existingValues = await _db.CustomFieldValues
                .Where(value => value.EntityId == entityId && fieldIds.Contains(value.CustomFieldId))
                .ToListAsync();

            foreach (var definition in definitions)
            {
                customFields.TryGetValue(definition.FieldName, out var rawValue);
                var value = NormalizeValue(definition, rawValue);

                if (definition.IsRequired && string.IsNullOrWhiteSpace(value))
                    throw new Exception($"Il campo {definition.Label} e obbligatorio");

                var existing = existingValues.FirstOrDefault(item => item.CustomFieldId == definition.Id);

                if (string.IsNullOrWhiteSpace(value) && definition.FieldType != "checkbox")
                {
                    if (existing != null)
                        _db.CustomFieldValues.Remove(existing);
                    continue;
                }

                if (existing == null)
                {
                    existing = new CustomFieldValue
                    {
                        CustomFieldId = definition.Id,
                        EntityId = entityId,
                        EntityName = definition.ModuleName
                    };
                    _db.CustomFieldValues.Add(existing);
                }

                existing.EntityName = definition.ModuleName;
                existing.Value = value;
            }

            await _db.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteValues(string moduleName, int entityId)
        {
            var normalizedModule = NormalizeModuleName(moduleName);
            var values = await _db.CustomFieldValues
                .Where(value => value.EntityName == normalizedModule && value.EntityId == entityId)
                .ToListAsync();

            if (values.Count == 0)
                return;

            _db.CustomFieldValues.RemoveRange(values);
            await _db.SaveChangesAsync();
        }

        private async Task<List<CustomFieldModuleDto>> GetVisibleModules()
        {
            var modules = await _db.Modules
                .AsNoTracking()
                .OrderBy(module => module.Description)
                .ThenBy(module => module.Name)
                .ToListAsync();

            var activeModules = await GetActiveModuleNameSet(modules.Select(module => module.Name));
            return modules
                .Where(module => SupportedModules.Contains(module.Name) && activeModules.Contains(module.Name))
                .Select(module => new CustomFieldModuleDto
                {
                    Name = module.Name,
                    Description = module.Description ?? module.Name,
                    Active = true
                })
                .ToList();
        }

        private async Task<HashSet<string>> GetActiveModuleNameSet(IEnumerable<string> fallback)
        {
            var setting = await _db.GeneralSettings.AsNoTracking().FirstOrDefaultAsync(item => item.Key == "activeModules");
            if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
                return fallback.ToHashSet(StringComparer.OrdinalIgnoreCase);

            try
            {
                var modules = JsonSerializer.Deserialize<List<string>>(setting.Value) ?? new List<string>();
                return modules.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return setting.Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<bool> IsModuleVisible(string moduleName)
        {
            if (!SupportedModules.Contains(moduleName))
                return false;

            var modules = await _db.Modules.AsNoTracking().Select(module => module.Name).ToListAsync();
            if (!modules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                return false;

            var activeModules = await GetActiveModuleNameSet(modules);
            return activeModules.Contains(moduleName);
        }

        private static CustomFieldDefinitionDto NormalizeDefinition(CustomFieldDefinitionDto field, int index)
        {
            var label = CleanText(field.Label);
            var fieldName = CleanKey(field.FieldName);
            if (string.IsNullOrWhiteSpace(fieldName))
                fieldName = CleanKey(label);

            return new CustomFieldDefinitionDto
            {
                Id = field.Id,
                ModuleName = NormalizeModuleName(field.ModuleName),
                FieldName = fieldName,
                Label = label,
                FieldType = NormalizeFieldType(field.FieldType),
                Options = CleanOptions(field.Options),
                IsRequired = field.IsRequired,
                Active = field.Active,
                SortOrder = field.SortOrder > 0 ? field.SortOrder : index + 1
            };
        }

        private static CustomFieldDefinitionDto ToDefinition(CustomField field)
        {
            return new CustomFieldDefinitionDto
            {
                Id = field.Id,
                ModuleName = field.EntityName,
                FieldName = field.FieldName,
                Label = string.IsNullOrWhiteSpace(field.Label) ? field.FieldName : field.Label,
                FieldType = NormalizeFieldType(field.FieldType),
                Options = field.Options ?? "",
                IsRequired = field.IsRequired,
                Active = field.Active,
                SortOrder = field.SortOrder
            };
        }

        private static string NormalizeValue(CustomFieldDefinitionDto definition, string? value)
        {
            var cleanValue = value?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(cleanValue) && definition.FieldType != "checkbox")
                return "";

            return definition.FieldType switch
            {
                "checkbox" => ParseBool(cleanValue) ? "true" : "false",
                "number" => NormalizeNumber(cleanValue, definition.Label),
                "date" => NormalizeDate(cleanValue, definition.Label),
                "select" => NormalizeSelect(cleanValue, definition),
                _ => cleanValue
            };
        }

        private static string NormalizeNumber(string value, string label)
        {
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
                return invariantValue.ToString(CultureInfo.InvariantCulture);

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentValue))
                return currentValue.ToString(CultureInfo.InvariantCulture);

            throw new Exception($"Il campo {label} deve essere numerico");
        }

        private static string NormalizeDate(string value, string label)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            throw new Exception($"Il campo {label} deve essere una data valida");
        }

        private static string NormalizeSelect(string value, CustomFieldDefinitionDto definition)
        {
            var options = ParseOptions(definition.Options);
            if (string.IsNullOrWhiteSpace(value) || options.Count == 0)
                return value;

            if (!options.Contains(value, StringComparer.OrdinalIgnoreCase))
                throw new Exception($"Il valore selezionato per {definition.Label} non e valido");

            return options.First(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> ParseOptions(string? options)
        {
            return (options ?? "")
                .Split(new[] { "\r\n", "\n", ";", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string CleanOptions(string? value)
        {
            return string.Join("\n", ParseOptions(value));
        }

        private static string NormalizeModuleName(string? value)
        {
            return CleanText(value).ToLowerInvariant();
        }

        private static string NormalizeFieldType(string? value)
        {
            var type = CleanText(value).ToLowerInvariant();
            return SupportedFieldTypes.Contains(type) ? type : "text";
        }

        private static string CleanText(string? value)
        {
            return value?.Trim() ?? "";
        }

        private static string CleanKey(string? value)
        {
            var normalized = Regex.Replace(CleanText(value).ToLowerInvariant(), @"[^a-z0-9_]+", "_").Trim('_');
            return Regex.Replace(normalized, "_+", "_");
        }

        private static bool ParseBool(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}

