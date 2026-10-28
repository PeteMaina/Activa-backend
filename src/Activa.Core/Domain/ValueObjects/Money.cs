using System;
using Activa.Core.Domain.Exceptions;

namespace Activa.Core.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainValidationException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainValidationException("Currency must be a valid 3-letter ISO code.");

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => Create(0, currency);
}
