using System;

namespace SapphireDb.Models.Exceptions
{
    public class SapphireDbException : Exception
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ExceptionSeverity Severity { get; }
        
        public SapphireDbException(ExceptionSeverity severity = ExceptionSeverity.Warning)
        {
            Severity = severity;
        }

        public SapphireDbException(string message, ExceptionSeverity severity = ExceptionSeverity.Warning) : base(message)
        {
            Severity = severity;
        }
        
        public SapphireDbException(string message, Exception innerException, ExceptionSeverity severity = ExceptionSeverity.Error) : base(message, innerException)
        {
            Severity = severity;
        }
    }

    public enum ExceptionSeverity
    {
        Error, Warning
    }
}