using CotelNet.Domain.Sales;

namespace CotelNet.Tests;

public sealed class SenderProfileTests
{
    [Fact]
    public void Normalizes_document_for_lookup_without_losing_display_value()
    {
        var sender = Create("8-772-1923");

        Assert.Equal("87721923", sender.DocumentKey);
        Assert.Equal("8-772-1923", sender.DocumentNumber);
        Assert.Equal("PA", sender.CountryCode);
        Assert.Equal("Francisco Javier Contreras", sender.FullName);
    }

    [Fact]
    public void Updates_profile_but_rejects_a_different_document()
    {
        var sender = Create("PA-123456");
        sender.Update("pa 123456", "Sr.", false, "PA", "Francisco", "Javier", "Contreras", "", "6000-0000", "", "USER@EXAMPLE.COM", "Panamá", "Panamá", "", "", "", "Nueva dirección", "");

        Assert.Equal("user@example.com", sender.Email);
        Assert.Equal("Nueva dirección", sender.Address);
        Assert.Throws<InvalidOperationException>(() => sender.Update("PA-999999", "Sr.", false, "PA", "Francisco", "", "Contreras", "", "", "", "", "", "", "", "", "", "Dirección", ""));
    }

    private static SenderProfile Create(string document) => new(document, "Sr.", false, "PA", "Francisco", "Javier", "Contreras", "", "6000-0000", "", "user@example.com", "Panamá", "Panamá", "", "", "", "Dirección", "");
}
