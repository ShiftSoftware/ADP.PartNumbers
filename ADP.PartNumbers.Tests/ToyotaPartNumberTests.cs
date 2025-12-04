using ShiftSoftware.ADP.PartNumbers;
using Xunit.Abstractions;
using System.IO.Compression;

namespace ADP.PartNumbers.Tests;

public class ToyotaPartNumberTests
{
    private readonly ITestOutputHelper output;
    public ToyotaPartNumberTests(ITestOutputHelper testOutputHelper)
    {
        output = testOutputHelper;
    }

    [Fact(DisplayName = "01. Equality")]
    public void Equality()
    {
        var partString1 = "90915-YZZJ3";
        var partString2 = "90915-yzzj3";
        var partString3 = "90915YZZJ3";

        var part1IsValid = ToyotaPartNumber.TryParse(partString1, out ToyotaPartNumber part1);
        var part2IsValid = ToyotaPartNumber.TryParse(partString2, out ToyotaPartNumber part2);
        var part3IsValid = ToyotaPartNumber.TryParse(partString3, out ToyotaPartNumber part3);

        Assert.True(part1IsValid);
        Assert.True(part2IsValid);
        Assert.True(part3IsValid);

        Assert.Equal(part1, part2);
        Assert.Equal(part1, part3);
        Assert.Equal(part2, part3);

        Assert.Equal(part1.GetHashCode(), part2.GetHashCode());
        Assert.Equal(part1.GetHashCode(), part3.GetHashCode());
        Assert.Equal(part2.GetHashCode(), part3.GetHashCode());

        // Check the implicit string converter
        Assert.Equal(partString1.Replace("-", "").ToUpperInvariant(), part1.ToString(false));
        Assert.Equal(partString2.Replace("-", "").ToUpperInvariant(), part2.ToString(false));
        Assert.Equal(partString3.ToUpperInvariant(), part3.ToString(false));
    }

    [Fact(DisplayName = "02. Components")]
    public void Components()
    {
        ToyotaPartNumber.TryParse("90915-YZZJ3", out ToyotaPartNumber part);

        Assert.Equal("90915", part.PartNumberCategory);
        Assert.Equal("YZZJ3", part.VehicleUsageCode);
        Assert.Equal(string.Empty, part.PartSuffix);
    }

    [Fact(DisplayName = "03. Components with Supersession")]
    public void ComponentsWithSupersession()
    {
        ToyotaPartNumber.TryParse("90915-YZZJ3-01", out ToyotaPartNumber part);

        Assert.Equal("90915", part.PartNumberCategory);
        Assert.Equal("YZZJ3", part.VehicleUsageCode);
        Assert.Equal("01", part.PartSuffix);
    }

    [Fact(DisplayName = "04. Invalid Part Numbers")]
    public void InvalidPartNumbers()
    {
        Assert.False(ToyotaPartNumber.TryParse("123", out _)); // Too short
        Assert.False(ToyotaPartNumber.TryParse("12345-ABCDE-123456", out _)); // Too long
        Assert.False(ToyotaPartNumber.TryParse("12345-ABC*EE", out _)); // Invalid character
        Assert.False(ToyotaPartNumber.TryParse("", out _)); // Empty
        Assert.False(ToyotaPartNumber.TryParse(null, out _)); // Null
    }

    [Fact(DisplayName = "05. Valid Toyota Part Number Formats")]
    public void ValidFormats()
    {
        Assert.True(ToyotaPartNumber.TryParse("90915-YZZJ3", out _));
        Assert.True(ToyotaPartNumber.TryParse("04152-YZZA1", out _));
        Assert.True(ToyotaPartNumber.TryParse("04152YZZA1", out _));
        Assert.True(ToyotaPartNumber.TryParse("90915-10003", out _));
        Assert.True(ToyotaPartNumber.TryParse("90915-YZZJ3-01", out _));
    }

    [Fact(DisplayName = "06. Parse Exception")]
    public void ParseException()
    {
        var exception = Assert.Throws<ArgumentException>(() => ToyotaPartNumber.Parse("INVALID"));
        Assert.Equal("Invalid Toyota Part Number (Parameter 'partNumber')", exception.Message);
    }

    [Fact(DisplayName = "07. Validate All Part Numbers from ZIP")]
    public void ValidatePartNumbersFromZip()
    {
        var zipPath = Path.Combine(AppContext.BaseDirectory, "Toyota Parts.zip");
        Assert.True(File.Exists(zipPath), $"ZIP file not found at: {zipPath}");

        using var zipArchive = ZipFile.OpenRead(zipPath);
        var csvEntry = zipArchive.Entries.FirstOrDefault(e => e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
        
        Assert.NotNull(csvEntry);

        using var streamReader = new StreamReader(csvEntry.Open());
        var partNumbers = new List<string>();
        var invalidPartNumbers = new List<string>();
        
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                partNumbers.Add(line.Trim());
                
                if (!ToyotaPartNumber.TryParse(line.Trim(), out var parsedPart))
                {
                    invalidPartNumbers.Add(line.Trim());
                    //output.WriteLine($"Invalid part number: {line.Trim()}");
                }
                else
                {
                    //output.WriteLine($"Valid: {line.Trim()} -> {parsedPart}");
                }
            }
        }

        output.WriteLine($"\nTotal part numbers: {partNumbers.Count}");
        output.WriteLine($"Valid part numbers: {partNumbers.Count - invalidPartNumbers.Count}");
        output.WriteLine($"Invalid part numbers: {invalidPartNumbers.Count}");

        Assert.Empty(invalidPartNumbers);
    }

    [Fact(DisplayName = "08. Validate Part Numbers from CSV")]
    public void ValidatePartNumbersFromCsv()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "Toyota Part Numbers.csv");
        Assert.True(File.Exists(csvPath), $"CSV file not found at: {csvPath}");

        var lines = File.ReadAllLines(csvPath);
        var partNumbers = new List<string>();
        var invalidPartNumbers = new List<string>();
        
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var partNumber = line.Trim();
                partNumbers.Add(partNumber);
                
                if (!ToyotaPartNumber.TryParse(partNumber, out var parsedPart))
                {
                    invalidPartNumbers.Add(partNumber);
                    output.WriteLine($"Invalid part number: {partNumber}");
                }
                else
                {
                    output.WriteLine($"Valid: {partNumber} -> {parsedPart}");
                }
            }
        }

        output.WriteLine($"\nTotal part numbers: {partNumbers.Count}");
        output.WriteLine($"Valid part numbers: {partNumbers.Count - invalidPartNumbers.Count}");
        output.WriteLine($"Invalid part numbers: {invalidPartNumbers.Count}");

        if (invalidPartNumbers.Count > 0)
        {
            output.WriteLine("\nInvalid part numbers list:");
            foreach (var invalid in invalidPartNumbers)
            {
                output.WriteLine($"  - {invalid}");
            }
        }

        Assert.Empty(invalidPartNumbers);
    }

    [Fact(DisplayName = "09. Parse with Non-Alphanumeric Characters")]
    public void ParseWithNonAlphanumericCharacters()
    {
        Assert.True(ToyotaPartNumber.TryParse("'0400553242'", out var part1));
        Assert.Equal("0400553242", part1.ToString(false));

        Assert.True(ToyotaPartNumber.TryParse("\"0400553242\"", out var part2));
        Assert.Equal("0400553242", part2.ToString(false));

        Assert.True(ToyotaPartNumber.TryParse(" 90915-YZZJ3 ", out var part3));
        Assert.Equal("90915YZZJ3", part3.ToString(false));

        Assert.True(ToyotaPartNumber.TryParse("  '90915-YZZJ3'  ", out var part4));
        Assert.Equal("90915YZZJ3", part4.ToString(false));

        Assert.True(ToyotaPartNumber.TryParse("04152@YZZA1!", out var part5));
        Assert.Equal("04152YZZA1", part5.ToString(false));

        Assert.Equal(part1, part1);
        Assert.Equal(part2, part2);
        Assert.Equal(part3, part4);
    }
}