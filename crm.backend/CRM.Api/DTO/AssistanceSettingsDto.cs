using System.Text.Json.Serialization;

namespace crm.backend.CRM.Api.DTO
{
    public class AssistanceSettingsDto
    {
        [JsonPropertyName("interventionReminderEnabled")]
        public bool InterventionReminderEnabled { get; set; } = true;

        [JsonPropertyName("interventionReminderDays")]
        public int InterventionReminderDays { get; set; } = 3;

        [JsonPropertyName("publicActivityTypes")]
        public List<string> PublicActivityTypes { get; set; } = new();

        [JsonPropertyName("autoCloseTicketWhenAllTasksCompleted")]
        public bool AutoCloseTicketWhenAllTasksCompleted { get; set; }

        [JsonPropertyName("interventionTemplateCompanyName")]
        public string InterventionTemplateCompanyName { get; set; } = "";

        [JsonPropertyName("interventionTemplateLogoUrl")]
        public string InterventionTemplateLogoUrl { get; set; } = "";

        [JsonPropertyName("interventionTemplateLogoFileName")]
        public string InterventionTemplateLogoFileName { get; set; } = "";

        [JsonPropertyName("interventionTemplateLogoDataUrl")]
        public string InterventionTemplateLogoDataUrl { get; set; } = "";

        [JsonPropertyName("interventionTemplateBrandColor")]
        public string InterventionTemplateBrandColor { get; set; } = "#0f766e";

        [JsonPropertyName("interventionTemplateFooterNotes")]
        public string InterventionTemplateFooterNotes { get; set; } = "";

        [JsonPropertyName("interventionTemplateSignatureLabel")]
        public string InterventionTemplateSignatureLabel { get; set; } = "Firma del cliente";

        [JsonPropertyName("interventionTemplateShowSignature")]
        public bool InterventionTemplateShowSignature { get; set; } = true;

        [JsonPropertyName("interventionTemplateIncludeInternalNotes")]
        public bool InterventionTemplateIncludeInternalNotes { get; set; }
    }
}
