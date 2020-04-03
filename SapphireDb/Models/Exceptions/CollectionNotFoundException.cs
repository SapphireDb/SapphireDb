using System;

namespace SapphireDb.Models.Exceptions
{
    public class CollectionNotFoundException : Exception
    {
        public CollectionNotFoundException() : base("No collection was found for given collection name")
        {
            
        }
    }
}