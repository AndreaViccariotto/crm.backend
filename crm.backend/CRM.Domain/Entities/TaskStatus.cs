namespace crm.backend.CRM.Domain.Entities
{
    public class TaskStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Is_Default { get; set; }
        public int Position { get; set; }
    }
}
