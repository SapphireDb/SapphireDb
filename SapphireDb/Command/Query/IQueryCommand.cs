namespace SapphireDb.Command.Query
{
    public interface IQueryCommand
    {
        public string ReferenceId { get; set; }
        
        string CollectionName { get; }
        
        string ContextName { get; }
    }
}