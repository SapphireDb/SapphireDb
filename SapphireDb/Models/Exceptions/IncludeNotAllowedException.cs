using System;

namespace SapphireDb.Models.Exceptions
{
    public class IncludeNotAllowedException : Exception
    {
        public IncludeNotAllowedException() : base("Include prefilters are disabled")
        {
            
        }
    }
}