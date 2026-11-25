using Microsoft.AspNetCore.Mvc;
using SolidApi.Models;

namespace SolidApi.Controllers;

[ApiController]
[Route("api/solid")]
public class ChannelController : ControllerBase
{
    /// <summary>
    /// Creates a new channel inside an application.
    /// </summary>
    [HttpPost("{application}")]
    [ProducesResponseType(typeof(ChannelResource), 201)]
    [ProducesResponseType(400)]
    public ActionResult<ChannelResource> CreateChannel(
        string application,
        [FromBody] ChannelResource channel)
    {
        return StatusCode(201, channel);
    }

    /// <summary>
    /// Discovers channels belonging to an application.
    /// </summary>
    [HttpGet("{application}")]
    [ProducesResponseType(typeof(string[]), 200)]
    public IActionResult DiscoverChannels(
        string application,
        [FromHeader(Name = "solid-discovery")] string discovery)
    {
        return Ok(new[]
        {
            $"/api/solid/{application}/humidity",
            $"/api/solid/{application}/temperature"
        });
    }

    /// <summary>
    /// Retrieves a channel.
    /// </summary>
    [HttpGet("{application}/{channel}")]
    [ProducesResponseType(typeof(ChannelResource), 200)]
    [ProducesResponseType(404)]
    public ActionResult<ChannelResource> GetChannel(
        string application,
        string channel)
    {
        return Ok(new ChannelResource
        {
            ResourceName = channel,
            ResourceDesc = "Test channel"
        });
    }

    /// <summary>
    /// Updates a channel.
    /// </summary>
    [HttpPut("{application}/{channel}")]
    [ProducesResponseType(typeof(ChannelResource), 200)]
    [ProducesResponseType(404)]
    public ActionResult<ChannelResource> UpdateChannel(
        string application,
        string channel,
        [FromBody] ChannelResource updatedChannel)
    {
        return Ok(updatedChannel);
    }

    /// <summary>
    /// Deletes a channel.
    /// </summary>
    [HttpDelete("{application}/{channel}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult DeleteChannel(
        string application,
        string channel)
    {
        return NoContent();
    }
}