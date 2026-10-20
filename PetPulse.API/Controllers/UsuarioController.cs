using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Hateoas;
using PetPulse.Domain.entites;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de usuários/tutores na API.
/// Expõe endpoints para criar, listar, buscar, atualizar e remover usuários.
/// </summary>
/// <remarks>
/// Base URL: /api/usuario
/// Exemplo: http://localhost:5292/api/usuario
///
/// O cadastro (<c>POST</c>) é público — é o único jeito de criar a própria conta.
/// Os demais endpoints exigem um token JWT válido (ver <c>POST /api/auth/login</c>).
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    /// <summary>
    /// Inicializa o controller com o repositório de usuários.
    /// </summary>
    /// <param name="usuarioRepository">Implementação do repositório de usuários injetada via DI.</param>
    public UsuarioController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Lista os usuários/tutores com paginação, ordenação e filtros.
    /// </summary>
    /// <remarks>
    /// Ordenação: sortBy = nome (padrão), email ou dataCadastro; sortDirection = asc ou desc.
    /// Filtros: nome e email (contém o texto).
    /// Exemplo: GET /api/Usuario?page=1&amp;pageSize=10&amp;sortBy=nome&amp;nome=ana
    /// </remarks>
    /// <param name="query">Paginação (page, pageSize), ordenação (sortBy, sortDirection) e filtros.</param>
    /// <returns>Página de resultados com links HATEOAS de navegação.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação, ordenação ou filtro inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] UsuarioQuery query)
    {
        if (query.TryGetError(out var erro))
            return BadRequest(erro);

        var (usuarios, total) = _usuarioRepository.GetPaged(query);

        var pagina = new PagedResponse<UsuarioResponse>(
            usuarios.Select(ComLinks).ToList(),
            query.Page,
            query.PageSize,
            total);

        return Ok(pagina with { Links = HateoasLinks.Paginacao(Rotas.Usuario, query, pagina) });
    }

    /// <summary>
    /// Busca um usuário pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <returns>Usuário encontrado ou 404 se não existir.</returns>
    /// <response code="200">Usuário encontrado.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var usuario = _usuarioRepository.GetById(id);

        if (usuario is null)
            return NotFound("Usuário não encontrado.");

        return Ok(ComLinks(usuario));
    }

    /// <summary>
    /// Cria um novo usuário/tutor.
    /// </summary>
    /// <param name="request">Dados para criação do usuário.</param>
    /// <returns>Usuário criado.</returns>
    /// <response code="201">Usuário criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] UsuarioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (_usuarioRepository.ExistsByEmail(request.Email))
            return BadRequest("Já existe um usuário cadastrado com este e-mail.");

        if (_usuarioRepository.ExistsByCpf(request.Cpf))
            return BadRequest("Já existe um usuário cadastrado com este CPF.");

        var usuario = request.ToDomain();

        _usuarioRepository.Add(usuario);

        var response = ComLinks(usuario);

        return CreatedAtAction(
            nameof(GetById),
            new { id = usuario.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza os dados de um usuário existente.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <param name="request">Novos dados do usuário.</param>
    /// <returns>Usuário atualizado.</returns>
    /// <response code="200">Usuário atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public IActionResult Update(Guid id, [FromBody] UsuarioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = _usuarioRepository.GetById(id);

        if (usuario is null)
            return NotFound("Usuário não encontrado.");

        usuario.AtualizarNome(request.Nome);
        usuario.AtualizarCpf(request.Cpf);
        usuario.AtualizarEmail(request.Email);
        usuario.AtualizarSenha(request.Senha);
        usuario.AtualizarTelefone(request.Telefone);
        usuario.AtualizarEndereco(request.Endereco);

        _usuarioRepository.Update(usuario);

        return Ok(ComLinks(usuario));
    }

    /// <summary>
    /// Remove um usuário pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <returns>NoContent se removido ou 404 se não existir.</returns>
    /// <response code="204">Usuário removido com sucesso.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public IActionResult Delete(Guid id)
    {
        var removed = _usuarioRepository.Delete(id);

        if (!removed)
            return NotFound("Usuário não encontrado.");

        return NoContent();
    }

    /// <summary>
    /// Converte a entidade para DTO e anexa os links HATEOAS do recurso.
    /// </summary>
    private static UsuarioResponse ComLinks(Usuario usuario) =>
        UsuarioResponse.FromDomain(usuario) with { Links = HateoasLinks.Usuario(usuario.Id) };
}
