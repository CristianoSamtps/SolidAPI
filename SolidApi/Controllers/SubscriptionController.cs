using Microsoft.AspNetCore.Mvc;
using SolidApi.Models;
using SolidApi.Services;

namespace SolidApi.Controllers;

[ApiController]
[Route("api/solid/{app}/{channel}")]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionService _service;

    public SubscriptionController(SubscriptionService service)
    {
        _service = service;
    }

    // POST /api/solid/{app}/{channel}
    // Criar uma subscription
    [HttpPost]
    public IActionResult Create(
        string app,
        string channel,
        [FromBody] SubscriptionResource resource)
    {
        var result = _service.Create(app, channel, resource);

        return Created(result.Path, result.Resource);
    }


    //GET /api/solid/{app}/{channel}/subs/{subscription}
    //Consultar uma subscription
    [HttpGet("subs/{subscription}")]
    public IActionResult Get(
        string app,
        string channel,
        string subscription)
    {
        var result = _service.Get(app, channel, subscription);

        return Ok(result);
    }

}
