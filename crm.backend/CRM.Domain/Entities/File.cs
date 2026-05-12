namespace crm.backend.CRM.Domain.Entities
{
    public class File
    {
        public int id { get; set; }
        public string file_name { get; set; }
        public string file_path { get; set; }
        public string entity_name { get; set; }
        public int entity_id { get; set; }
        public int uploaded_by { get; set; }
        public DateTime created_at { get; set; }
    }
}
