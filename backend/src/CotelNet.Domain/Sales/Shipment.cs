using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Shipment : Entity
{
    private Shipment() { }
    public Shipment(int saleId, string trackingNumber, int postalServiceId, int destinationId, int weightGrams, decimal basePrice,
        string senderName, string senderDocument, string senderPhone, string senderEmail, string senderAddress,
        string recipientName, string recipientPhone, string recipientAddress, int? senderProfileId = null,
        string? recipientTitle = null, string? recipientFirstName = null, string? recipientMiddleName = null, string? recipientFirstLastName = null, string? recipientSecondLastName = null,
        string? recipientSecondaryPhone = null, string? recipientEmail = null, string? recipientProvince = null, string? recipientCity = null,
        string? recipientPostalCode = null, string? recipientStreet = null, string? recipientHouseNumber = null, string? recipientFax = null)
    {
        SaleId = saleId; TrackingNumber = trackingNumber; PostalServiceId = postalServiceId; DestinationId = destinationId; WeightGrams = weightGrams; BasePrice = basePrice;
        SenderName = senderName.Trim(); SenderDocument = senderDocument.Trim(); SenderPhone = senderPhone.Trim(); SenderEmail = senderEmail.Trim(); SenderAddress = senderAddress.Trim();
        RecipientName = recipientName.Trim(); RecipientPhone = recipientPhone.Trim(); RecipientAddress = recipientAddress.Trim(); SenderProfileId = senderProfileId;
        RecipientTitle = recipientTitle?.Trim() ?? string.Empty; RecipientFirstName = recipientFirstName?.Trim() ?? string.Empty; RecipientMiddleName = recipientMiddleName?.Trim() ?? string.Empty;
        RecipientFirstLastName = recipientFirstLastName?.Trim() ?? string.Empty; RecipientSecondLastName = recipientSecondLastName?.Trim() ?? string.Empty;
        RecipientSecondaryPhone = recipientSecondaryPhone?.Trim() ?? string.Empty; RecipientEmail = recipientEmail?.Trim() ?? string.Empty;
        RecipientProvince = recipientProvince?.Trim() ?? string.Empty; RecipientCity = recipientCity?.Trim() ?? string.Empty; RecipientPostalCode = recipientPostalCode?.Trim() ?? string.Empty;
        RecipientStreet = recipientStreet?.Trim() ?? string.Empty; RecipientHouseNumber = recipientHouseNumber?.Trim() ?? string.Empty; RecipientFax = recipientFax?.Trim() ?? string.Empty;
    }
    public int SaleId { get; private set; }
    public Sale Sale { get; private set; } = null!;
    public string TrackingNumber { get; private set; } = string.Empty;
    public int PostalServiceId { get; private set; }
    public PostalService PostalService { get; private set; } = null!;
    public int DestinationId { get; private set; }
    public Destination Destination { get; private set; } = null!;
    public int WeightGrams { get; private set; }
    public decimal BasePrice { get; private set; }
    public int? SenderProfileId { get; private set; }
    public SenderProfile? SenderProfile { get; private set; }
    public string SenderName { get; private set; } = string.Empty;
    public string SenderDocument { get; private set; } = string.Empty;
    public string SenderPhone { get; private set; } = string.Empty;
    public string SenderEmail { get; private set; } = string.Empty;
    public string SenderAddress { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string RecipientTitle { get; private set; } = string.Empty;
    public string RecipientFirstName { get; private set; } = string.Empty;
    public string RecipientMiddleName { get; private set; } = string.Empty;
    public string RecipientFirstLastName { get; private set; } = string.Empty;
    public string RecipientSecondLastName { get; private set; } = string.Empty;
    public string RecipientPhone { get; private set; } = string.Empty;
    public string RecipientSecondaryPhone { get; private set; } = string.Empty;
    public string RecipientEmail { get; private set; } = string.Empty;
    public string RecipientProvince { get; private set; } = string.Empty;
    public string RecipientCity { get; private set; } = string.Empty;
    public string RecipientPostalCode { get; private set; } = string.Empty;
    public string RecipientStreet { get; private set; } = string.Empty;
    public string RecipientHouseNumber { get; private set; } = string.Empty;
    public string RecipientAddress { get; private set; } = string.Empty;
    public string RecipientFax { get; private set; } = string.Empty;
    public ICollection<ShipmentSupplementaryService> SupplementaryServices { get; private set; } = new List<ShipmentSupplementaryService>();
}
