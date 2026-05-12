namespace crm.backend.CRM.Api.DTO
{
    public class FileRequest
    {
        public int id { get; set; } = 0;
        public string content { get; set; }
        public string file_name { get; set; }
        public string entity_name { get; set; }
        public int entity_id { get; set; }
        public string uploaded_by { get; set; }
    }
}
