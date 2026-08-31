using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

public class AlertaInteligenteRepositoryTests
{
    private static PetPulseContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PetPulseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PetPulseContext(options);
    }

    private static Usuario CriarUsuarioValido() =>
        new(
            nome: "Maria Silva",
            cpf: "12345678900",
            email: "maria@teste.com",
            senha: "senha123",
            telefone: "11999999999",
            endereco: "Rua Teste, 123");

    private static Pet CriarPetValido(Guid usuarioId) =>
        new(
            usuarioId: usuarioId,
            nome: "Rex",
            especie: "Cachorro",
            raca: "Labrador",
            dataNascimento: new DateOnly(2020, 1, 1),
            peso: 20.5m,
            sexo: SexoPetEnum.Macho,
            castrado: true,
            porte: PortePetEnum.Grande);

    private static AlertaInteligente CriarAlertaValido(Guid petId) =>
        new(
            petId: petId,
            tipoAlerta: TipoAlertaEnum.FrequenciaCardiaca,
            nivelRisco: NivelRiscoEnum.Alto,
            origemAlerta: OrigemAlertaEnum.DispositivoIot,
            mensagem: "Frequência cardíaca acima do normal",
            recomendacao: "Procurar um veterinário");

    private static Guid CriarPetPersistido(PetPulseContext context)
    {
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var pet = CriarPetValido(usuario.Id);
        context.Pets.Add(pet);
        context.SaveChanges();

        return pet.Id;
    }

    [Fact]
    public void Add_DevePersistirComStatusAbertoEPermitirBuscarPorId()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);
        var petId = CriarPetPersistido(context);
        var alerta = CriarAlertaValido(petId);

        // Act
        var alertaAdicionado = repository.Add(alerta);
        var alertaEncontrado = repository.GetById(alertaAdicionado.Id);

        // Assert
        Assert.NotNull(alertaEncontrado);
        Assert.Equal(StatusAlertaEnum.Aberto, alertaEncontrado!.Status);
    }

    [Fact]
    public void GetByPetId_DeveRetornarApenasAlertasDoPetInformado()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);
        var petId1 = CriarPetPersistido(context);
        var petId2 = CriarPetPersistido(context);

        var alertaDoPet1 = CriarAlertaValido(petId1);
        var alertaDoPet2 = CriarAlertaValido(petId2);
        context.AlertasInteligentes.AddRange(alertaDoPet1, alertaDoPet2);
        context.SaveChanges();

        // Act
        var resultado = repository.GetByPetId(petId1);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(alertaDoPet1.Id, resultado[0].Id);
    }

    [Fact]
    public void GetByStatus_DeveRetornarApenasAlertasComOStatusInformado()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);
        var petId = CriarPetPersistido(context);

        var alertaAberto = CriarAlertaValido(petId);
        var alertaResolvido = CriarAlertaValido(petId);
        alertaResolvido.Resolver();

        context.AlertasInteligentes.AddRange(alertaAberto, alertaResolvido);
        context.SaveChanges();

        // Act
        var resultado = repository.GetByStatus(StatusAlertaEnum.Resolvido);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(alertaResolvido.Id, resultado[0].Id);
    }

    [Fact]
    public void Update_DeveAtualizarStatusAposVisualizarERecuperarValorPersistido()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);
        var petId = CriarPetPersistido(context);
        var alerta = CriarAlertaValido(petId);
        context.AlertasInteligentes.Add(alerta);
        context.SaveChanges();

        // Act
        alerta.Visualizar();
        repository.Update(alerta);
        var alertaAtualizado = repository.GetById(alerta.Id);

        // Assert
        Assert.Equal(StatusAlertaEnum.Visualizado, alertaAtualizado!.Status);
    }

    [Fact]
    public void Delete_QuandoExiste_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);
        var petId = CriarPetPersistido(context);
        var alerta = CriarAlertaValido(petId);
        context.AlertasInteligentes.Add(alerta);
        context.SaveChanges();

        // Act
        var resultado = repository.Delete(alerta.Id);

        // Assert
        Assert.True(resultado);
        Assert.Null(repository.GetById(alerta.Id));
    }

    [Fact]
    public void Delete_QuandoNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new AlertaInteligenteRepository(context);

        // Act
        var resultado = repository.Delete(Guid.NewGuid());

        // Assert
        Assert.False(resultado);
    }
}
