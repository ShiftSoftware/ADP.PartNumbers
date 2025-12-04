using System;

namespace ShiftSoftware.ADP.PartNumbers;

public class ToyotaPartNumber
{
    private readonly string partNumber;
    public string PartNumberCategory { get; }
    public string VehicleUsageCode { get; }
    public string PartSuffix { get; }

    private ToyotaPartNumber(string partNumber)
    {
        this.partNumber = partNumber.Replace("-", "").ToUpperInvariant();
        
        PartNumberCategory = this.partNumber.Substring(0, 5);
        VehicleUsageCode = this.partNumber.Length >= 10 ? this.partNumber.Substring(5, 5) : string.Empty;
        PartSuffix = this.partNumber.Length > 10 ? this.partNumber.Substring(10) : string.Empty;
    }

    private ToyotaPartNumber(ReadOnlySpan<char> partNumber)
    {
        this.partNumber = RemoveHyphens(partNumber);
        ReadOnlySpan<char> cleaned = this.partNumber.AsSpan();

        PartNumberCategory = cleaned.Slice(0, 5).ToString();
        VehicleUsageCode = cleaned.Length >= 10 ? cleaned.Slice(5, 5).ToString() : string.Empty;
        PartSuffix = cleaned.Length > 10 ? cleaned.Slice(10).ToString() : string.Empty;
    }

    /// <summary>
    /// Tries to parse a Toyota Part Number. Returns true if successful, otherwise false.
    /// </summary>
    public static bool TryParse(string partNumber, out ToyotaPartNumber result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(partNumber))
            return false;

        var cleaned = ExtractAlphanumeric(partNumber);
        
        if (string.IsNullOrEmpty(cleaned))
            return false;

        cleaned = cleaned.ToUpperInvariant();

        if (!ToyotaPartNumberValidator.Validate(cleaned))
            return false;

        result = new ToyotaPartNumber(cleaned);
        return true;
    }

    /// <summary>
    /// Tries to parse a Toyota Part Number. Returns true if successful, otherwise false.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> partNumber, out ToyotaPartNumber result)
    {
        result = null;

        Span<char> alphanumericOnly = stackalloc char[partNumber.Length];
        int alphanumericLength = 0;

        for (int i = 0; i < partNumber.Length; i++)
        {
            char c = partNumber[i];

            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-')
            {
                if (c >= 'a' && c <= 'z')
                    c = (char)(c - 32);
                
                alphanumericOnly[alphanumericLength++] = c;
            }
        }

        if (alphanumericLength == 0)
            return false;

        var cleanedSpan = alphanumericOnly.Slice(0, alphanumericLength);

        if (!ToyotaPartNumberValidator.Validate(cleanedSpan))
        {
            result = null;
            return false;
        }

        result = new ToyotaPartNumber(cleanedSpan);
        return true;
    }

    /// <summary>
    /// Parses a Toyota Part Number. Throws an exception if invalid.
    /// </summary>
    public static ToyotaPartNumber Parse(string partNumber)
    {
        var cleaned = ExtractAlphanumeric(partNumber);
        
        if (string.IsNullOrEmpty(cleaned))
            throw new ArgumentException("Invalid Toyota Part Number", nameof(partNumber));

        cleaned = cleaned.ToUpperInvariant();

        if (!ToyotaPartNumberValidator.Validate(cleaned))
            throw new ArgumentException("Invalid Toyota Part Number", nameof(partNumber));

        return new ToyotaPartNumber(cleaned);
    }

    private static string ExtractAlphanumeric(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        Span<char> buffer = stackalloc char[input.Length];
        int index = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-')
            {
                buffer[index++] = c;
            }
        }

        return buffer.Slice(0, index).ToString();
    }

    private static string RemoveHyphens(ReadOnlySpan<char> input)
    {
        if (input.IndexOf('-') == -1)
            return input.ToString();

        Span<char> buffer = stackalloc char[input.Length];
        int index = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] != '-')
                buffer[index++] = input[i];
        }

        return buffer.Slice(0, index).ToString();
    }

    public string ToString(bool includeHyphens = true)
    {
        if (!includeHyphens)
            return partNumber;

        if (string.IsNullOrEmpty(PartSuffix))
            return $"{PartNumberCategory}-{VehicleUsageCode}";

        return $"{PartNumberCategory}-{VehicleUsageCode}-{PartSuffix}";
    }

    public override string ToString() => ToString(true);
    public override bool Equals(object obj) => obj is ToyotaPartNumber other && partNumber.Equals(other.partNumber, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => partNumber.GetHashCode();

    public static implicit operator string(ToyotaPartNumber partNumber) => partNumber.ToString();
}