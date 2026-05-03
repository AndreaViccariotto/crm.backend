namespace crm.backend.CRM.Api.DTO
{
    public class TaskStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool is_default { get; set; }
        public int position { get; set; }
    }
}
