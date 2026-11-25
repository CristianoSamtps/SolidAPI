using Microsoft.AspNetCore.Mvc;
using SolidApi.Services;

namespace SolidApi.Controllers;

[ApiController]
[Route("api/solid")]
public class SubscriptionDiscoveryController : ControllerBase
{
    private readonly SubscriptionService _service;

    public SubscriptionDiscoveryController(SubscriptionService service)
    {
        _service = service;
    }

    [HttpGet]
    [HttpGet("{app}")]
    [HttpGet("{app}/{channel}")]
    public IActionResult Discover(
        string? app,
        string? channel,
        [FromHeader(Name = "solid-discovery")] string? discovery)
    {
        if (discovery != "subscription")
            return BadRequest("Invalid or missing solid-discovery header.");

        if (app == null)
        {
            return Ok(_service.DiscoverAll());
        }

        if (channel == null)
        {
            return Ok(_service.DiscoverByApplication(app));
        }

        if (!_service.ChannelExists(app, channel))
            return NotFound("Channel not found.");

        return Ok(_service.DiscoverByChannel(app, channel));
    }
}