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
        PartNumberCategory = partNumber.Length >=5 ? partNumber.Slice(0, 5).ToString() : string.Empty;
        VehicleUsageCode = partNumber.Length >= 10 ? partNumber.Slice(5, 5).ToString() : string.Empty;
        PartSuffix = partNumber.Length > 10 ? partNumber.Slice(10).ToString() : string.Empty;

        this.partNumber = $"{this.PartNumberCategory}{this.VehicleUsageCode}{this.PartSuffix}".ToUpperInvariant();
    }

    /// <summary>
    /// Tries to parse a Toyota Part Number. Returns true if successful, otherwise false.
    /// </summary>
    public static bool TryParse(string partNumber, out ToyotaPartNumber result, bool removeNonAlphanumericCharacters = false)
        => TryParse(partNumber, out result, removeNonAlphanumericCharacters, recoverMissingLeadingZero: false);

    /// <summary>
    /// Tries to parse a Toyota Part Number. When <paramref name="recoverMissingLeadingZero"/> is enabled, an
    /// otherwise invalid value containing exactly nine numeric characters is treated as a ten-character part
    /// whose significant leading zero was removed by spreadsheet numeric-cell handling.
    /// </summary>
    public static bool TryParse(
        string partNumber,
        out ToyotaPartNumber result,
        bool removeNonAlphanumericCharacters,
        bool recoverMissingLeadingZero)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(partNumber))
            return false;

        // Check for trailing hyphen which suggests incomplete suffix
        if (partNumber.Length > 0 && partNumber[partNumber.Length - 1] == '-')
            return false;

        Span<char> buffer = stackalloc char[partNumber.Length];
        Span<char> cleanedBuffer = stackalloc char[partNumber.Length + (recoverMissingLeadingZero ? 1 : 0)];

        int index = 0;
        var cleandedIndex = 0;

        var containsInvalidCharacters = false;

        foreach (var ch in partNumber)
        {
            if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '-')
            {
                buffer[index++] = ch; //Valid Character

                if (ch != '-')
                    cleanedBuffer[cleandedIndex++] = ch;
            }
            else
            {
                if (!removeNonAlphanumericCharacters)
                {
                    containsInvalidCharacters = true;
                }
            }
        }

        if (containsInvalidCharacters)
            return false;

        if (recoverMissingLeadingZero && cleandedIndex == 9)
        {
            var containsOnlyDigits = true;

            for (var i = 0; i < cleandedIndex; i++)
            {
                if (cleanedBuffer[i] < '0' || cleanedBuffer[i] > '9')
                {
                    containsOnlyDigits = false;
                    break;
                }
            }

            if (containsOnlyDigits)
            {
                for (var i = cleandedIndex; i > 0; i--)
                    cleanedBuffer[i] = cleanedBuffer[i - 1];

                cleanedBuffer[0] = '0';
                cleandedIndex++;
            }
        }

        result = new ToyotaPartNumber(cleanedBuffer.Slice(0, cleandedIndex));

        return result.PartNumberCategory.Length == 5 && result.VehicleUsageCode.Length == 5 && (result.PartSuffix.Length == 0 || result.PartSuffix.Length == 2);
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
            return $"{PartNumberCategory}-{VehicleUsageCode}".ToUpperInvariant();

        return $"{PartNumberCategory}-{VehicleUsageCode}-{PartSuffix}".ToUpperInvariant();
    }

    public override string ToString() => ToString(true);
    public override bool Equals(object obj) => obj is ToyotaPartNumber other && partNumber.Equals(other.partNumber, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => partNumber.GetHashCode();

    public static implicit operator string(ToyotaPartNumber partNumber) => partNumber.ToString();
}
