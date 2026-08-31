using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;

namespace PetPulse.API.Tests.Controllers;

public class UsuarioControllerTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly UsuarioController _controller;

    public UsuarioControllerTests()
    {
        _controller = new UsuarioController(_usuarioRepository.Object);
    }

    [Fact]
    public void Create_ComEmailJaCadastrado_DeveRetornarBadRequestENaoPersistir()
    {
        // Arrange
        var request = CriarRequestValido();
        _usuarioRepository.Setup(r => r.ExistsByEmail(request.Email)).Returns(true);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _usuarioRepository.Verify(r => r.Add(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public void Create_ComCpfJaCadastrado_DeveRetornarBadRequestENaoPersistir()
    {
        // Arrange
        var request = CriarRequestValido();
        _usuarioRepository.Setup(r => r.ExistsByEmail(request.Email)).Returns(false);
        _usuarioRepository.Setup(r => r.ExistsByCpf(request.Cpf)).Returns(true);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _usuarioRepository.Verify(r => r.Add(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarCreatedEChamarAdd()
    {
        // Arrange
        var request = CriarRequestValido();
        _usuarioRepository.Setup(r => r.ExistsByEmail(request.Email)).Returns(false);
        _usuarioRepository.Setup(r => r.ExistsByCpf(request.Cpf)).Returns(false);
        _usuarioRepository.Setup(r => r.Add(It.IsAny<Usuario>())).Returns((Usuario u) => u);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        _usuarioRepository.Verify(r => r.Add(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public void GetById_QuandoUsuarioNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _usuarioRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((Usuario?)null);

        // Act
        var result = _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_QuandoUsuarioNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _usuarioRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static UsuarioRequest CriarRequestValido() =>
        new(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");
}
