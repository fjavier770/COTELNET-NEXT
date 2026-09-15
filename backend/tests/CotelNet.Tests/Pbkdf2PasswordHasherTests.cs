using CotelNet.Infrastructure.Auth;

namespace CotelNet.Tests;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void HashAndVerify_WithCorrectPassword_ReturnsTrue()
    {
        var sut = new Pbkdf2PasswordHasher();
        var result = sut.Hash("ClaveSegura123!");

        Assert.True(sut.Verify("ClaveSegura123!", result.Hash, result.Salt, result.Iterations));
        Assert.Equal(Pbkdf2PasswordHasher.DefaultIterations, result.Iterations);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var sut = new Pbkdf2PasswordHasher();
        var result = sut.Hash("ClaveSegura123!");

        Assert.False(sut.Verify("OtraClave", result.Hash, result.Salt, result.Iterations));
    }
}
