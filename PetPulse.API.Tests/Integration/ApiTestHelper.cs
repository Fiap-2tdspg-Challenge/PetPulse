using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PetPulse.API.Tests.Integration;

/// <summary>
/// Atalhos usados pelos testes de integração: cadastrar usuário, autenticar e criar pets pela própria API.
/// </summary>
internal static class ApiTestHelper
{
    public const string SenhaPadrao = "Senha123456";

    public sealed record UsuarioCriado(Guid Id, string Email, string Senha);

    /// <summary>Cadastra um usuário com e-mail e CPF únicos (POST /api/Usuario, endpoint público).</summary>
    public static async Task<UsuarioCriado> CriarUsuarioAsync(HttpClient client)
    {
        var email = $"tutor.{Guid.NewGuid():N}@petpulse.com";

        var response = await client.PostAsJsonAsync("/api/Usuario", new
        {
            nome = "Tutor Integração",
            cpf = GerarCpf(),
            email,
            senha = SenhaPadrao,
            telefone = "11999990000",
            endereco = "Rua dos Testes, 100"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return new UsuarioCriado(json.GetProperty("id").GetGuid(), email, SenhaPadrao);
    }

    /// <summary>Faz login (POST /api/auth/login) e devolve o accessToken.</summary>
    public static async Task<string> ObterTokenAsync(HttpClient client, string email, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, senha });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("accessToken").GetString()!;
    }

    /// <summary>Cria um usuário, autentica e devolve um cliente com o header Authorization: Bearer.</summary>
    public static async Task<(HttpClient Client, UsuarioCriado Usuario)> CriarClienteAutenticadoAsync(PetPulseApiFactory factory)
    {
        var client = factory.CreateClient();
        var usuario = await CriarUsuarioAsync(client);
        var token = await ObterTokenAsync(client, usuario.Email, usuario.Senha);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (client, usuario);
    }

    /// <summary>Cria um pet (POST /api/Pet) e devolve o JSON da resposta.</summary>
    public static async Task<JsonElement> CriarPetAsync(HttpClient client, Guid usuarioId, string nome, string especie, decimal peso)
    {
        var response = await client.PostAsJsonAsync("/api/Pet", NovoPet(usuarioId, nome, especie, peso));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static object NovoPet(Guid usuarioId, string nome, string especie, decimal peso) => new
    {
        usuarioId,
        nome,
        especie,
        raca = "SRD",
        dataNascimento = "2021-04-10",
        peso,
        sexo = 1,
        castrado = true,
        porte = 2
    };

    /// <summary>Lê os "rel" dos links HATEOAS de um JSON.</summary>
    public static IReadOnlyList<string> Rels(JsonElement json) =>
        json.GetProperty("links").EnumerateArray().Select(link => link.GetProperty("rel").GetString()!).ToList();

    private static string GerarCpf() =>
        string.Concat(Enumerable.Range(0, 11).Select(_ => Random.Shared.Next(0, 10)));
}
