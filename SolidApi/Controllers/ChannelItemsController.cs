using Microsoft.AspNetCore.Mvc;
using SolidApi.Models;
using SolidApi.Services;

namespace SolidApi.Controllers;

/// <summary>
/// POST /api/solid/{app}/{channel}: cria um filho do canal.
/// O enunciado usa o mesmo URI para criar data-records e subscrições, por isso
/// existe só esta ação, que decide pelo res-type e chama o serviço certo.
/// </summary>
[ApiController]
[Route("api/solid/{app}/{channel}")]
public class ChannelItemsController : ControllerBase
{
    private readonly SubscriptionService _subscriptions;

    public ChannelItemsController(SubscriptionService subscriptions)
    {
        _subscriptions = subscriptions;
    }

    /// <summary>
    /// Cria um data-record ou uma subscription no canal, conforme o res-type do pedido.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionResource), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult Create(string app, string channel, [FromBody] ResourceRequest body)
    {
        switch (body.ResType)
        {
            case "subscription":
                var subscription = new SubscriptionResource
                {
                    ResourceName = body.ResourceName ?? "",
                    Evt = body.Evt ?? 0,
                    Endpoint = body.Endpoint,
                    DtExpiration = body.DtExpiration ?? ""
                };
                var created = _subscriptions.Create(app, channel, subscription);
                return Created(created.Path, created.Resource);

            case "data-record":
                // TODO (data-records): chamar aqui o DataRecordService.Create
                return StatusCode(501, new { error = "Criação de data-records ainda não implementada." });

            default:
                return BadRequest(new { error = "res-type deve ser data-record ou subscription neste nível." });
        }
    }
}
