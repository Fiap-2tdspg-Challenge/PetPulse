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
    public void GetAll_DeveRetornarPaginaComPetsELinksHateoas()
    {
        // Arrange
        var pet = CriarPetValido();
        pet.Id = Guid.NewGuid();
        IReadOnlyList<Pet> pets = new List<Pet> { pet };
        _petRepository.Setup(r => r.GetPaged(It.IsAny<PetQuery>())).Returns((pets, 1));

        // Act
        var result = _controller.GetAll(new PetQuery());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<PagedResponse<PetResponse>>(okResult.Value);
        Assert.Single(pagina.Items);
        Assert.Equal(1, pagina.TotalItems);
        Assert.Contains(pagina.Links, link => link.Rel == "self");
        Assert.Contains(pagina.Items[0].Links, link => link.Rel == "self" && link.Href == $"/api/Pet/{pet.Id}");
        Assert.Contains(pagina.Items[0].Links, link => link.Rel == "usuario" && link.Href == $"/api/Usuario/{pet.UsuarioId}");
    }

    [Fact]
    public void GetAll_ComFiltrosEOrdenacao_DeveRepassarAQueryAoRepositorio()
    {
        // Arrange
        IReadOnlyList<Pet> vazio = new List<Pet>();
        _petRepository.Setup(r => r.GetPaged(It.IsAny<PetQuery>())).Returns((vazio, 0));
        var query = new PetQuery { Page = 2, PageSize = 5, SortBy = "peso", SortDirection = "desc", Especie = "Cachorro" };

        // Act
        var result = _controller.GetAll(query);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _petRepository.Verify(r => r.GetPaged(It.Is<PetQuery>(q =>
            q.Page == 2 && q.PageSize == 5 && q.SortBy == "peso" && q.Especie == "Cachorro")), Times.Once);
    }

    [Theory]
    [InlineData(0, 20, null)]
    [InlineData(1, 101, null)]
    [InlineData(1, 20, "campoQueNaoExiste")]
    public void GetAll_ComParametrosInvalidos_DeveRetornarBadRequest(int page, int pageSize, string? sortBy)
    {
        // Arrange
        var query = new PetQuery { Page = page, PageSize = pageSize, SortBy = sortBy };

        // Act
        var result = _controller.GetAll(query);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _petRepository.Verify(r => r.GetPaged(It.IsAny<PetQuery>()), Times.Never);
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
