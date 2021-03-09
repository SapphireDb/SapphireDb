namespace SapphireDb.Models
{
    public class SapphireAuthResource
    {
        public object RequestedResource { get; set; }
        
        public OperationTypeEnum OperationType { get; set; }
        
        public enum OperationTypeEnum
        {
            Create, Query, Update, Delete, Execute
        }
    }
}