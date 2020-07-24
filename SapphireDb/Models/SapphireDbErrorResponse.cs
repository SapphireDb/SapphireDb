using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            Data = exception.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(property => property.Name, property => property.GetValue(exception));
        }

        public string Type { get; }

        public string Message { get; }

        public Guid Id { get; }

        public Dictionary<string, object> Data { get; }
    }
}