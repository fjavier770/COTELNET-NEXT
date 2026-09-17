using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class SenderProfile : Entity
{
    private SenderProfile() { }

    public SenderProfile(string documentNumber, string? title, bool isMinor, string? countryCode, string firstName, string? middleName,
        string firstLastName, string? secondLastName, string? primaryPhone, string? secondaryPhone, string? email,
        string? province, string? city, string? postalCode, string? street, string? houseNumber, string address, string? fax)
    {
        DocumentKey = NormalizeDocument(documentNumber);
        Update(documentNumber, title, isMinor, countryCode, firstName, middleName, firstLastName, secondLastName, primaryPhone,
            secondaryPhone, email, province, city, postalCode, street, houseNumber, address, fax);
    }

    public string DocumentKey { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = "PA";
    public string Title { get; private set; } = string.Empty;
    public bool IsMinor { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string FirstLastName { get; private set; } = string.Empty;
    public string SecondLastName { get; private set; } = string.Empty;
    public string PrimaryPhone { get; private set; } = string.Empty;
    public string SecondaryPhone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string HouseNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Fax { get; private set; } = string.Empty;

    public string FullName => string.Join(' ', new[] { FirstName, MiddleName, FirstLastName, SecondLastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public void Update(string documentNumber, string? title, bool isMinor, string? countryCode, string firstName, string? middleName,
        string firstLastName, string? secondLastName, string? primaryPhone, string? secondaryPhone, string? email,
        string? province, string? city, string? postalCode, string? street, string? houseNumber, string address, string? fax)
    {
        var key = NormalizeDocument(documentNumber);
        if (DocumentKey.Length > 0 && DocumentKey != key)
            throw new InvalidOperationException("El documento de un remitente no puede cambiarse.");
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("El primer nombre del remitente es obligatorio.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(firstLastName)) throw new ArgumentException("El primer apellido del remitente es obligatorio.", nameof(firstLastName));
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("La dirección del remitente es obligatoria.", nameof(address));

        DocumentKey = key;
        DocumentNumber = documentNumber.Trim().ToUpperInvariant();
        CountryCode = NormalizeCountryCode(countryCode);
        Title = Clean(title);
        IsMinor = isMinor;
        FirstName = firstName.Trim();
        MiddleName = Clean(middleName);
        FirstLastName = firstLastName.Trim();
        SecondLastName = Clean(secondLastName);
        PrimaryPhone = Clean(primaryPhone);
        SecondaryPhone = Clean(secondaryPhone);
        Email = Clean(email).ToLowerInvariant();
        Province = Clean(province);
        City = Clean(city);
        PostalCode = Clean(postalCode);
        Street = Clean(street);
        HouseNumber = Clean(houseNumber);
        Address = address.Trim();
        Fax = Clean(fax);
        MarkUpdated();
    }

    public static string NormalizeDocument(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("La cédula o pasaporte del remitente es obligatorio.", nameof(value));
        var normalized = new string(value.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (normalized.Length < 4) throw new ArgumentException("La cédula o pasaporte del remitente no es válido.", nameof(value));
        return normalized;
    }

    public static string NormalizeCountryCode(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "PA" : value.Trim().ToUpperInvariant();
        if (normalized.Length != 2 || normalized.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("El país del remitente debe ser un código ISO de dos letras.", nameof(value));
        return normalized;
    }

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;
}
