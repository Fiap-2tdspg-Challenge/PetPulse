using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

public class UsuarioRepositoryTests
{
    private static PetPulseContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PetPulseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PetPulseContext(options);
    }

    private static Usuario CriarUsuarioValido(string email = "maria@teste.com", string cpf = "12345678900") =>
        new(
            nome: "Maria Silva",
            cpf: cpf,
            email: email,
            senha: "senha123",
            telefone: "11999999999",
            endereco: "Rua Teste, 123");

    [Fact]
    public void Add_DevePersistirEPermitirBuscarPorId()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new UsuarioRepository(context);
        var usuario = CriarUsuarioValido();

        // Act
        var usuarioAdicionado = repository.Add(usuario);
        var usuarioEncontrado = repository.GetById(usuarioAdicionado.Id);

        // Assert
        Assert.NotNull(usuarioEncontrado);
        Assert.Equal("Maria Silva", usuarioEncontrado!.Nome);
    }

    [Fact]
    public void ExistsByEmail_ComEmailCadastrado_DeveRetornarTrueIndependenteDeMaiusculas()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new UsuarioRepository(context);
        var usuario = CriarUsuarioValido(email: "maria@teste.com");
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        // Act
        var resultado = repository.ExistsByEmail("MARIA@TESTE.COM");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void ExistsByEmail_ComEmailNaoCadastrado_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new UsuarioRepository(context);

        // Act
        var resultado = repository.ExistsByEmail("naoexiste@teste.com");

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void ExistsByCpf_ComCpfCadastrado_DeveRetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new UsuarioRepository(context);
        var usuario = CriarUsuarioValido(cpf: "98765432100");
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        // Act
        var resultado = repository.ExistsByCpf("98765432100");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void Delete_QuandoExiste_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new UsuarioRepository(context);
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        // Act
        var resultado = repository.Delete(usuario.Id);

        // Assert
        Assert.True(resultado);
        Assert.Null(repository.GetById(usuario.Id));
    }
}
