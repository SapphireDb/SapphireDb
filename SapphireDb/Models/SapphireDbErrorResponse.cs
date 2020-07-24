using System;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Models
{
    public class SapphireDbErrorResponse
    {
        public SapphireDbErrorResponse(SapphireDbException exception)
        {
            Type = exception.GetType().Name;
            Message = exception.Message;
            Id = exception.Id;
        }

        public string Type { get; }

        public string Message { get; }

        public Guid Id { get; set; }
    }
}