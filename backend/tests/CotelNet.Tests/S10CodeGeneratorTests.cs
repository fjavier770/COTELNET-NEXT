using CotelNet.Domain.Sales;

namespace CotelNet.Tests;

public sealed class S10CodeGeneratorTests
{
    [Fact]
    public void Generate_CreatesUpuS10CodeWithExpectedCheckDigit()
    {
        var code = S10CodeGenerator.Generate("RT", "0426", 58);

        Assert.Equal("RT042600589PA", code);
    }

    [Theory]
    [InlineData("04260058", 9)]
    [InlineData("00000000", 5)]
    public void CalculateCheckDigit_UsesWeightedModulus11(string serial, int expected)
    {
        Assert.Equal(expected, S10CodeGenerator.CalculateCheckDigit(serial));
    }

    [Fact]
    public void FormatHumanReadable_GroupsIdentifierForPostalLabel()
    {
        Assert.Equal("RT 042 600 589 PA", S10CodeGenerator.FormatHumanReadable("RT042600589PA"));
    }

    [Fact]
    public void TryFormatHumanReadable_PreservesLegacyIdentifier()
    {
        Assert.False(S10CodeGenerator.TryFormatHumanReadable("LEGACY-0001", out var formatted));
        Assert.Equal("LEGACY-0001", formatted);
    }
}
