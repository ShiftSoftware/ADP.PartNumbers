# ADP Part Numbers

A .NET library for parsing and validating automotive part numbers from various manufacturers.

## Supported Manufacturers

- **Toyota** - Fully implemented

> Additional manufacturers may be added in the future.

---

## Toyota Part Number Format

This section describes the Toyota part number format and provides examples of how the parser handles various input formats.

## Part Number Structure

A Toyota part number consists of three components:

| Component | Length | Description |
|-----------|--------|-------------|
| **Part Number Category** | 5 characters | Identifies the part category |
| **Vehicle Usage Code** | 5 characters | Identifies the vehicle application |
| **Part Suffix** (optional) | 2 characters | Supersession or revision identifier |

### Format Examples

- `90915-YZZJ3` - Basic format with hyphen
- `90915YZZJ3` - Basic format without hyphen
- `90915-YZZJ3-01` - With supersession suffix

## Parsing Behavior

The parser is flexible and handles various input formats:

### Case Insensitivity

All inputs are normalized to uppercase:

| Input | With Hyphens | Without Hyphens |
|-------|--------------|-----------------|
| `12345ABCDE` | `12345-ABCDE` | `12345ABCDE` |
| `12345abcde` | `12345-ABCDE` | `12345ABCDE` |
| `12345-ABCDE` | `12345-ABCDE` | `12345ABCDE` |
| `12345-abcde` | `12345-ABCDE` | `12345ABCDE` |

### With Supersession Suffix

| Input | With Hyphens | Without Hyphens |
|-------|--------------|-----------------|
| `12345ABCDEXY` | `12345-ABCDE-XY` | `12345ABCDEXY` |
| `12345abcdexy` | `12345-ABCDE-XY` | `12345ABCDEXY` |
| `12345-ABCDE-XY` | `12345-ABCDE-XY` | `12345ABCDEXY` |
| `12345-abcde-xy` | `12345-ABCDE-XY` | `12345ABCDEXY` |

### Non-Alphanumeric Character Removal

When `removeNonAlphanumericCharacters: true` is specified, invalid characters are stripped:

| Input | With Hyphens | Without Hyphens |
|-------|--------------|-----------------|
| `12345ABC'DE` | `12345-ABCDE` | `12345ABCDE` |
| `12@345ABCDE` | `12345-ABCDE` | `12345ABCDE` |
| `12345A#BCDE` | `12345-ABCDE` | `12345ABCDE` |
| `12345ABC%DE` | `12345-ABCDE` | `12345ABCDE` |
| `1&2345ABC%DE` | `12345-ABCDE` | `12345ABCDE` |
| `12345-ABCDE??` | `12345-ABCDE` | `12345ABCDE` |
| `12345-ABCDE??` | `12345-ABCDE` | `12345ABCDE` |

## Usage

### Parsing

```csharp
// Basic parsing
if (ToyotaPartNumber.TryParse("90915-YZZJ3", out var part))
{
    Console.WriteLine($"Category: {part.PartNumberCategory}"); // 90915
    Console.WriteLine($"Usage: {part.VehicleUsageCode}");      // YZZJ3
    Console.WriteLine($"Suffix: {part.PartSuffix}");           // (empty)
}

// Parsing with non-alphanumeric character removal
if (ToyotaPartNumber.TryParse("90915 YZZJ3", out var part, removeNonAlphanumericCharacters: true))
{
    Console.WriteLine(part.ToString()); // 90915-YZZJ3
}
```

### Formatting

```csharp
var part = ToyotaPartNumber.Parse("90915YZZJ3");

// With hyphens (default)
Console.WriteLine(part.ToString());      // 90915-YZZJ3
Console.WriteLine(part.ToString(true));  // 90915-YZZJ3

// Without hyphens
Console.WriteLine(part.ToString(false)); // 90915YZZJ3
```

### Equality

Part numbers are compared case-insensitively and hyphen-agnostically:

```csharp
var part1 = ToyotaPartNumber.Parse("90915-YZZJ3");
var part2 = ToyotaPartNumber.Parse("90915-yzzj3");
var part3 = ToyotaPartNumber.Parse("90915YZZJ3");

// All are equal
Console.WriteLine(part1.Equals(part2)); // true
Console.WriteLine(part1.Equals(part3)); // true
Console.WriteLine(part1.GetHashCode() == part2.GetHashCode()); // true
```

## Valid Formats

? **Valid:**
- `90915-YZZJ3` - Standard format with hyphen
- `90915YZZJ3` - No hyphen
- `04152-YZZA1` - Different category
- `04152YZZA1` - No hyphen
- `90915-10003` - Numeric usage code
- `90915-YZZJ3-01` - With supersession suffix

? **Invalid:**
- `123` - Too short
- `?????-?????` - Non-alphanumeric (without removal flag)
- `12345-ABCDE-123456` - Suffix too long
- `12345-ABC*EE` - Invalid character (without removal flag)
- `12345-123456` - Vehicle usage code is 6 characters (must be 5)
- `123456-12345` - Part category is 6 characters (must be 5)
- `12345-12345-1` - Suffix is 1 character (must be 0 or 2)
- `12345-12345-123` - Suffix is 3 characters (must be 0 or 2)
- `12345-12345-` - Trailing hyphen with no suffix

## Components

### Part Number Category (5 characters)
The first 5 characters identify the general category of the part.

Example: In `90915-YZZJ3`, the category is `90915`

### Vehicle Usage Code (5 characters)
Characters 6-10 identify the specific vehicle application or usage.

Example: In `90915-YZZJ3`, the usage code is `YZZJ3`

### Part Suffix (0 or 2 characters)
Optional characters after the 10th position indicate supersession or revision.

Example: In `90915-YZZJ3-01`, the suffix is `01`
