using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.API.Tests.Controllers;

public class PetControllerTests
{
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly PetController _controller;

    public PetControllerTests()
    {
        _controller = new PetController(_petRepository.Object, _usuarioRepository.Object);
    }

    [Fact]
    public void GetAll_DeveRetornarOkComListaDePets()
    {
        // Arrange
        var pets = new List<Pet> { CriarPetValido() };
        _petRepository.Setup(r => r.GetAll()).Returns(pets);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IReadOnlyList<PetResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public void GetById_QuandoPetNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _petRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((Pet?)null);

        // Act
        var result = _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetByUsuarioId_QuandoUsuarioNaoExiste_DeveRetornarNotFoundENaoConsultarPets()
    {
        // Arrange
        _usuarioRepository.Setup(r => r.ExistsById(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.GetByUsuarioId(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _petRepository.Verify(r => r.GetByUsuarioId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Create_QuandoUsuarioNaoExiste_DeveRetornarNotFoundENaoPersistir()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());
        _usuarioRepository.Setup(r => r.ExistsById(request.UsuarioId)).Returns(false);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _petRepository.Verify(r => r.Add(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarCreatedEChamarAdd()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = CriarRequestValido(usuarioId);
        _usuarioRepository.Setup(r => r.ExistsById(usuarioId)).Returns(true);
        _petRepository.Setup(r => r.Add(It.IsAny<Pet>())).Returns((Pet p) => p);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        _petRepository.Verify(r => r.Add(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public void Update_QuandoPetNaoExiste_DeveRetornarNotFoundENaoAtualizar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = CriarRequestValido(usuarioId);
        _usuarioRepository.Setup(r => r.ExistsById(usuarioId)).Returns(true);
        _petRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((Pet?)null);

        // Act
        var result = _controller.Update(Guid.NewGuid(), request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _petRepository.Verify(r => r.Update(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public void Delete_QuandoPetNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _petRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static PetRequest CriarRequestValido(Guid usuarioId) =>
        new(
            usuarioId,
            "Thor",
            "Cachorro",
            "Golden Retriever",
            new DateOnly(2021, 4, 10),
            28.5m,
            SexoPetEnum.Macho,
            true,
            PortePetEnum.Grande);

    private static Pet CriarPetValido() =>
        new(
            Guid.NewGuid(),
            "Thor",
            "Cachorro",
            "Golden Retriever",
            new DateOnly(2021, 4, 10),
            28.5m,
            SexoPetEnum.Macho,
            true,
            PortePetEnum.Grande);
}
