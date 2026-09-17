namespace CotelNet.Domain.Sales;

public static class S10CodeGenerator
{
    private static readonly int[] Weights = [8, 6, 4, 2, 3, 5, 9, 7];

    public static string Generate(string serviceIndicator, string officeCode, int consecutive, string countryCode = "PA")
    {
        var indicator = NormalizeLetters(serviceIndicator, 2, "indicador de servicio");
        var country = NormalizeLetters(countryCode, 2, "código de país");
        var office = new string((officeCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (office.Length > 4) office = office[^4..];
        office = office.PadLeft(4, '0');
        if (consecutive is < 1 or > 9999) throw new ArgumentOutOfRangeException(nameof(consecutive), "El consecutivo S10 de la estafeta debe estar entre 1 y 9999.");

        var serial = office + consecutive.ToString("D4");
        return indicator + serial + CalculateCheckDigit(serial) + country;
    }

    public static int CalculateCheckDigit(string serial)
    {
        if (serial?.Length != 8 || serial.Any(character => !char.IsDigit(character)))
            throw new ArgumentException("El número de serie S10 debe contener ocho dígitos.", nameof(serial));

        var remainder = serial.Select((character, index) => (character - '0') * Weights[index]).Sum() % 11;
        var result = 11 - remainder;
        return result switch { 10 => 0, 11 => 5, _ => result };
    }

    public static string FormatHumanReadable(string identifier)
    {
        var normalized = identifier?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalized.Length != 13 || normalized.Take(2).Any(character => character is < 'A' or > 'Z') ||
            normalized.Skip(2).Take(9).Any(character => character is < '0' or > '9') ||
            normalized.Skip(11).Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("El identificador S10 debe tener formato AANNNNNNNNNAA.", nameof(identifier));

        return $"{normalized[..2]} {normalized.Substring(2, 3)} {normalized.Substring(5, 3)} {normalized.Substring(8, 3)} {normalized[^2..]}";
    }

    public static bool TryFormatHumanReadable(string? identifier, out string formatted)
    {
        try
        {
            formatted = FormatHumanReadable(identifier ?? string.Empty);
            return true;
        }
        catch (ArgumentException)
        {
            formatted = identifier?.Trim().ToUpperInvariant() ?? string.Empty;
            return false;
        }
    }

    public static string? ResolvePostalFormCode(string? identifier)
    {
        var prefix = identifier?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(prefix)) return null;

        return prefix[0] switch
        {
            'R' => "CN-04",
            'C' => "CP-73",
            'E' => null,
            _ => null
        };
    }

    private static string NormalizeLetters(string value, int length, string field)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalized.Length != length || normalized.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException($"El {field} debe contener {length} letras.", nameof(value));
        return normalized;
    }
}
