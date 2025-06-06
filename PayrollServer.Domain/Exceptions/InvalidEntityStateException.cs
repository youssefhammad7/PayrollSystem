using System;
using System.Collections.Generic;

namespace PayrollServer.Domain.Exceptions
{
    public class InvalidEntityStateException : DomainException
    {
        public InvalidEntityStateException(string message) : base(message)
        {
            EntityName = string.Empty;
            EntityId = string.Empty;
            Errors = Array.Empty<string>();
        }

        public InvalidEntityStateException(string message, Exception innerException) : base(message, innerException)
        {
            EntityName = string.Empty;
            EntityId = string.Empty;
            Errors = Array.Empty<string>();
        }

        public InvalidEntityStateException(string entityName, object entityId)
            : base($"Entity \"{entityName}\" with ID {entityId} is in an invalid state.")
        {
            EntityName = entityName;
            EntityId = entityId;
            Errors = Array.Empty<string>();
        }

        public InvalidEntityStateException(string entityName, object entityId, IEnumerable<string> errors)
            : base($"Entity \"{entityName}\" with ID {entityId} is in an invalid state.")
        {
            EntityName = entityName;
            EntityId = entityId;
            Errors = errors;
        }

        public string EntityName { get; }
        public object EntityId { get; }
        public IEnumerable<string> Errors { get; }
    }
} 