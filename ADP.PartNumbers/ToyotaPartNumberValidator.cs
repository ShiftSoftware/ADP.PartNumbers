using System;

namespace ShiftSoftware.ADP.PartNumbers;

internal static class ToyotaPartNumberValidator
{
    public static bool Validate(string partNumber)
    {
        return Validate(partNumber.AsSpan());
    }

    public static bool Validate(ReadOnlySpan<char> partNumber)
    {
        // Remove hyphens for validation
        Span<char> cleaned = stackalloc char[partNumber.Length];
        int cleanedLength = 0;

        for (int i = 0; i < partNumber.Length; i++)
        {
            if (partNumber[i] != '-')
                cleaned[cleanedLength++] = partNumber[i];
        }

        var cleanedSpan = cleaned.Slice(0, cleanedLength);

        // Toyota part numbers must be exactly 10 or 12 characters without hyphens
        // 10 chars: PartNumberCategory (5) + VehicleUsageCode (5)
        // 12 chars: PartNumberCategory (5) + VehicleUsageCode (5) + PartSuffix (2)
        if (cleanedLength != 10 && cleanedLength != 12)
            return false;

        // All characters must be alphanumeric
        for (int i = 0; i < cleanedLength; i++)
        {
            char c = cleanedSpan[i];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')))
                return false;
        }

        return true;
    }
}