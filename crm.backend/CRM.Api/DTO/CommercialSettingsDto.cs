using System.Text.Json.Serialization;

namespace crm.backend.CRM.Api.DTO
{
    public class CommercialSettingsDto
    {
        [JsonPropertyName("articleCategories")]
        public List<string> ArticleCategories { get; set; } = new();

        [JsonPropertyName("articleUnits")]
        public List<string> ArticleUnits { get; set; } = new();

        [JsonPropertyName("quoteReminderEnabled")]
        public bool QuoteReminderEnabled { get; set; } = true;

        [JsonPropertyName("quoteReminderDays")]
        public int QuoteReminderDays { get; set; } = 3;

        [JsonPropertyName("quoteTemplateCompanyName")]
        public string QuoteTemplateCompanyName { get; set; } = "";

        [JsonPropertyName("quoteTemplateLogoUrl")]
        public string QuoteTemplateLogoUrl { get; set; } = "";

        [JsonPropertyName("quoteTemplateLogoFileName")]
        public string QuoteTemplateLogoFileName { get; set; } = "";

        [JsonPropertyName("quoteTemplateLogoContentType")]
        public string QuoteTemplateLogoContentType { get; set; } = "";

        [JsonPropertyName("quoteTemplateLogoDataUrl")]
        public string QuoteTemplateLogoDataUrl { get; set; } = "";

        [JsonPropertyName("quoteTemplateBrandColor")]
        public string QuoteTemplateBrandColor { get; set; } = "#14b8a6";

        [JsonPropertyName("quoteTemplatePaymentTerms")]
        public string QuoteTemplatePaymentTerms { get; set; } = "";

        [JsonPropertyName("quoteTemplateFooterNotes")]
        public string QuoteTemplateFooterNotes { get; set; } = "";

        [JsonPropertyName("quoteTemplateSignatureLabel")]
        public string QuoteTemplateSignatureLabel { get; set; } = "Firma per accettazione";

        [JsonPropertyName("quoteTemplateShowSignature")]
        public bool QuoteTemplateShowSignature { get; set; } = true;
    }

    public class QuoteLogoUploadRequest
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = "";

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }
}
