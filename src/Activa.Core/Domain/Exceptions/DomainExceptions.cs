using System;

namespace Activa.Core.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string message) : base(message) { }
}
