using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs.Login;
using PetPulse.Application.Services;
using PetPulse.Controllers;

namespace PetPulse.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<ILoginService> _loginService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_loginService.Object);
    }

    [Fact]
    public void Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        // Arrange
        var request = new LoginRequest("ana.souza@email.com", "Senha123456");
        var response = new LoginResponse(
            "token-fake",
            DateTime.UtcNow.AddHours(2),
            request.Email,
            "Ana Souza");

        _loginService
            .Setup(s => s.Login(request.Email, request.Senha))
            .Returns(response);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("token-fake", body.AccessToken);
    }

    [Fact]
    public void Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("ana.souza@email.com", "senhaErrada");

        _loginService
            .Setup(s => s.Login(request.Email, request.Senha))
            .Returns((LoginResponse?)null);

        // Act
        var result = _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void Login_ComModelStateInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new LoginRequest("", "");
        _controller.ModelState.AddModelError("Email", "O e-mail é obrigatório");

        // Act
        var result = _controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _loginService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
