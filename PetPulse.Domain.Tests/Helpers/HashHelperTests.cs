using PetPulse.Domain.Helpers;

namespace PetPulse.Domain.Tests.Helpers;

public class HashHelperTests
{
    [Fact]
    public void Verify_ComHashValidoESenhaCorreta_DeveRetornarTrue()
    {
        // Arrange
        var hash = HashHelper.Hash("Senha123456");

        // Act
        var resultado = HashHelper.Verify("Senha123456", hash);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void Verify_ComHashValidoESenhaIncorreta_DeveRetornarFalse()
    {
        // Arrange
        var hash = HashHelper.Hash("Senha123456");

        // Act
        var resultado = HashHelper.Verify("SenhaErrada999", hash);

        // Assert
        Assert.False(resultado);
    }

    [Theory]
    [InlineData("Senha123456")] // registro "legado": senha gravada em texto puro, antes desta implementação
    [InlineData("qualquer-texto-que-nao-seja-um-hash-bcrypt")]
    public void Verify_ComHashForaDoFormatoBCrypt_DeveRetornarFalseSemLancarExcecao(string hashInvalido)
    {
        // Act
        var resultado = HashHelper.Verify("Senha123456", hashInvalido);

        // Assert
        Assert.False(resultado);
    }
}
