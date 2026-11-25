using Microsoft.AspNetCore.Mvc;
using SolidApi.Models;
using SolidApi.Repositories;
using SolidApi.Services;

namespace SolidApi.Controllers;

/// <summary>
/// Nível 0 (/api/solid) e nível 1 (/api/solid/{app}).
/// Regra de rotas do grupo: cada URI + método existe num só controlador.
/// O GET destes URIs também responde ao discovery (header solid-discovery).
/// O POST em /api/solid/{app} (criar channel) fica no ChannelController.
/// </summary>
[ApiController]
[Route("api/solid")]
public class ApplicationController : ControllerBase
{
    private readonly ApplicationRepository _applications;
    private readonly DiscoveryService _discovery;

    public ApplicationController(ApplicationRepository applications, DiscoveryService discovery)
    {
        _applications = applications;
        _discovery = discovery;
    }

    /// <summary>
    /// Discovery em todo o middleware. Não existe "get all": o header solid-discovery é obrigatório.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(400)]
    public IActionResult Discover([FromHeader(Name = "solid-discovery")] string? discovery)
    {
        if (discovery == null)
            return BadRequest(new { error = "Use o header solid-discovery: não existe 'get all'." });

        return Ok(_discovery.Find(discovery));
    }

    /// <summary>
    /// Devolve a application; com o header solid-discovery devolve os paths encontrados dentro dela.
    /// </summary>
    [HttpGet("{app}")]
    [ProducesResponseType(typeof(ApplicationResource), 200)]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult Get(string app, [FromHeader(Name = "solid-discovery")] string? discovery)
    {
        if (discovery != null)
            return Ok(_discovery.Find(discovery, app));

        var application = _applications.GetByName(app);

        if (application == null)
            return NotFound(new { error = "Aplicação não encontrada." });

        return Ok(application);
    }
}
