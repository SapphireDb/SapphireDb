namespace SapphireDb.Models.Exceptions
{
    public class ValueNotFoundException : SapphireDbException
    {
        public ValueNotFoundException() : base("The value was not found")
        {
            
        }
    }
}