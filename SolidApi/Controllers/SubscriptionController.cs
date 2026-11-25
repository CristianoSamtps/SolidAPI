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

    // O POST /api/solid/{app}/{channel} (criar subscription) está no ChannelItemsController,
    // porque o mesmo URI também cria data-records. Continua a usar o SubscriptionService.Create.

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

    //DELETE /api/solid/{app}/{channel}/subs/{subscription}
    [HttpDelete("subs/{subscription}")]
    public IActionResult Delete(string app, string channel, string subscription)
    {
        _service.Delete(app, channel, subscription);

        return NoContent();
    }


}
