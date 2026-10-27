using System;
using Activa.Core.Domain.Exceptions;

namespace Activa.Core.Domain.ValueObjects;

public readonly record struct Location
{
    public string Facility { get; }
    public string Detail { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }

    private Location(string facility, string detail, double? latitude, double? longitude)
    {
        Facility = facility;
        Detail = detail;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Location Create(string facility, string detail = "", double? latitude = null, double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(facility))
            throw new DomainValidationException("Facility name is required.");

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new DomainValidationException("Latitude must be between -90 and 90.");

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new DomainValidationException("Longitude must be between -180 and 180.");

        return new Location(facility.Trim(), detail?.Trim() ?? string.Empty, latitude, longitude);
    }
}
