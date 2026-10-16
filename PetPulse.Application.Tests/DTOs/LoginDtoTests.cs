using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PetPulse.Application.DTOs.Login;

namespace PetPulse.Application.Tests;

public class LoginDtoTests
{
    [Fact]
    public void LoginRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Arrange
        var request = new LoginRequest("ana.souza@email.com", "Senha123456");

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("", "Senha123456", "Email")]
    [InlineData("email-invalido", "Senha123456", "Email")]
    [InlineData("ana.souza@email.com", "", "Senha")]
    public void LoginRequest_ComDadosInvalidos_DeveFalharNaValidacao(
        string email, string senha, string campoComErro)
    {
        // Arrange
        var request = new LoginRequest(email, senha);

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(campoComErro));
    }

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        // Mesma estratégia usada nos demais DTOs (ver UsuarioDtoTests): os atributos de
        // validação ficam no parâmetro do construtor primário do record, então a leitura
        // precisa ser via reflection sobre o construtor, não sobre as properties.
        var results = new List<ValidationResult>();
        var type = dto.GetType();
        var constructor = type.GetConstructors()[0];
        var context = new ValidationContext(dto);

        foreach (var parameter in constructor.GetParameters())
        {
            var property = type.GetProperty(parameter.Name!);
            if (property is null)
                continue;

            context.MemberName = property.Name;
            var value = property.GetValue(dto);

            foreach (var attribute in parameter.GetCustomAttributes<ValidationAttribute>(inherit: true))
            {
                var result = attribute.GetValidationResult(value, context);
                if (result is not null && result != ValidationResult.Success)
                    results.Add(result);
            }
        }

        return (results.Count == 0, results);
    }
}
