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
}
