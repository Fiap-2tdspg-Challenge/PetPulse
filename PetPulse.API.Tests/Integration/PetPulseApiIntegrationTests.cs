using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static PetPulse.API.Tests.Integration.ApiTestHelper;

namespace PetPulse.API.Tests.Integration;

/// <summary>
/// Testes de integração ponta a ponta: requisições HTTP reais contra a API em memória
/// (<see cref="PetPulseApiFactory"/>), passando por roteamento, validação, JWT, controllers,
/// repositórios e EF Core. Padrão AAA (Arrange / Act / Assert).
/// </summary>
public class PetPulseApiIntegrationTests(PetPulseApiFactory factory) : IClassFixture<PetPulseApiFactory>
{
    [Fact]
    public async Task Health_DeveResponderHealthyComChecagemDoBanco()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
        Assert.Contains(json.GetProperty("checks").EnumerateArray(), check => check.GetProperty("name").GetString() == "Oracle");
    }

    [Fact]
    public async Task Swagger_DevePublicarDocumentoOpenApiComOsEndpoints()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var conteudo = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/Pet", conteudo);
        Assert.Contains("/api/auth/login", conteudo);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornar401()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Pet");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenInvalido_DeveRetornar401()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token.invalido.qualquer");

        // Act
        var response = await client.GetAsync("/api/Pet");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CadastroELogin_DeveEmitirTokenJwtQueLiberaEndpointsProtegidos()
    {
        // Arrange
        var client = factory.CreateClient();
        var usuario = await CriarUsuarioAsync(client);

        // Act
        var token = await ObterTokenAsync(client, usuario.Email, usuario.Senha);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync($"/api/Pet?usuarioId={usuario.Id}");

        // Assert
        Assert.Equal(3, token.Split('.').Length);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_DeveRetornar401()
    {
        // Arrange
        var client = factory.CreateClient();
        var usuario = await CriarUsuarioAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = usuario.Email, senha = "SenhaErrada999" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CadastroDeUsuario_ComDadosInvalidos_DeveRetornar400()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Usuario", new
        {
            nome = "A",
            cpf = "123",
            email = "email-invalido",
            senha = "curta"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Pet_CrudCompleto_DeveCriarConsultarAtualizarERemover()
    {
        // Arrange
        var (client, usuario) = await CriarClienteAutenticadoAsync(factory);

        // Act — criar
        var criado = await CriarPetAsync(client, usuario.Id, "Thor", "Cachorro", 30m);
        var petId = criado.GetProperty("id").GetGuid();

        // Act — consultar
        var consulta = await client.GetAsync($"/api/Pet/{petId}");
        var consultado = await consulta.Content.ReadFromJsonAsync<JsonElement>();

        // Act — atualizar
        var atualizacao = await client.PutAsJsonAsync($"/api/Pet/{petId}", NovoPet(usuario.Id, "Thor Atualizado", "Cachorro", 31.5m));
        var atualizado = await atualizacao.Content.ReadFromJsonAsync<JsonElement>();

        // Act — remover
        var remocao = await client.DeleteAsync($"/api/Pet/{petId}");
        var consultaAposRemocao = await client.GetAsync($"/api/Pet/{petId}");

        // Assert
        Assert.Equal("Thor", criado.GetProperty("nome").GetString());
        Assert.Contains("self", Rels(criado));

        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal(petId, consultado.GetProperty("id").GetGuid());
        Assert.Contains("usuario", Rels(consultado));

        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        Assert.Equal("Thor Atualizado", atualizado.GetProperty("nome").GetString());
        Assert.Equal(31.5m, atualizado.GetProperty("peso").GetDecimal());

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, consultaAposRemocao.StatusCode);
    }

    [Fact]
    public async Task Pet_ComUsuarioInexistente_DeveRetornar404()
    {
        // Arrange
        var (client, _) = await CriarClienteAutenticadoAsync(factory);

        // Act
        var response = await client.PostAsJsonAsync("/api/Pet", NovoPet(Guid.NewGuid(), "Rex", "Cachorro", 10m));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListagemDePets_ComFiltroOrdenacaoEPaginacao_DeveRetornarPaginaComLinksHateoas()
    {
        // Arrange
        var (client, usuario) = await CriarClienteAutenticadoAsync(factory);
        await CriarPetAsync(client, usuario.Id, "Bob", "Cachorro", 12m);
        await CriarPetAsync(client, usuario.Id, "Thor", "Cachorro", 30m);
        await CriarPetAsync(client, usuario.Id, "Luna", "Cachorro", 8m);
        await CriarPetAsync(client, usuario.Id, "Mel", "Gato", 4m);

        // Act
        var response = await client.GetAsync(
            $"/api/Pet?usuarioId={usuario.Id}&especie=cachorro&sortBy=peso&sortDirection=desc&page=1&pageSize=2");
        var pagina = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, pagina.GetProperty("totalItems").GetInt32());
        Assert.Equal(2, pagina.GetProperty("totalPages").GetInt32());
        Assert.True(pagina.GetProperty("hasNext").GetBoolean());
        Assert.False(pagina.GetProperty("hasPrevious").GetBoolean());

        var nomes = pagina.GetProperty("items").EnumerateArray().Select(pet => pet.GetProperty("nome").GetString()).ToList();
        Assert.Equal(new[] { "Thor", "Bob" }, nomes);

        var next = pagina.GetProperty("links").EnumerateArray().Single(link => link.GetProperty("rel").GetString() == "next");
        Assert.Contains("page=2", next.GetProperty("href").GetString());
        Assert.Contains("sortBy=peso", next.GetProperty("href").GetString());
        Assert.DoesNotContain("prev", Rels(pagina));
    }

    [Theory]
    [InlineData("/api/Pet?pageSize=500")]
    [InlineData("/api/Pet?page=0")]
    [InlineData("/api/Pet?sortBy=cpf")]
    [InlineData("/api/HistoricoClinico?dataInicio=2026-06-01&dataFim=2026-01-01")]
    public async Task Listagem_ComParametroInvalido_DeveRetornar400(string url)
    {
        // Arrange
        var (client, _) = await CriarClienteAutenticadoAsync(factory);

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
