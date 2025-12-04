using System;

namespace ShiftSoftware.ADP.PartNumbers;

public class ToyotaPartNumber
{
    private readonly string partNumber;
    public string PartNumberCategory { get; }
    public string VehicleUsageCode { get; }
    public string PartSuffix { get; }

    private ToyotaPartNumber(ReadOnlySpan<char> partNumber)
    {
        PartNumberCategory = partNumber.Slice(0, 5).ToString();
        VehicleUsageCode = partNumber.Length >= 10 ? partNumber.Slice(5, 5).ToString() : string.Empty;
        PartSuffix = partNumber.Length > 10 ? partNumber.Slice(10).ToString() : string.Empty;

        this.partNumber = $"{this.PartNumberCategory}{this.VehicleUsageCode}{this.PartSuffix}";
    }

    /// <summary>
    /// Tries to parse a Toyota Part Number. Returns true if successful, otherwise false.
    /// </summary>
    public static bool TryParse(string partNumber, out ToyotaPartNumber result, bool removeNonAlphanumericCharacters = false)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(partNumber))
            return false;

        if (removeNonAlphanumericCharacters)
        {
            Span<char> buffer = stackalloc char[partNumber.Length];

            int index = 0;

            foreach (var ch in partNumber)
            {
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '-')
                    buffer[index++] = ch;
            }

            partNumber = buffer.Slice(0, index).ToString();
        }

        var cleaned = RemoveHyphens(partNumber);

        if (string.IsNullOrEmpty(cleaned))
            return false;

        cleaned = cleaned.ToUpperInvariant();

        if (!ToyotaPartNumberValidator.Validate(cleaned))
            return false;

        result = new ToyotaPartNumber(cleaned);

        return true;
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