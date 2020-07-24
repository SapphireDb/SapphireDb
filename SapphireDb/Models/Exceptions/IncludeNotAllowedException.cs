namespace SapphireDb.Models.Exceptions
{
    public class IncludeNotAllowedException : SapphireDbException
    {
        public IncludeNotAllowedException() : base("Include prefilters are disabled")
        {
            
        }
    }
}