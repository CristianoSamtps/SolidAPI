using Microsoft.AspNetCore.Mvc;
using SolidApi.Database;
using SolidApi.Models;
using SolidApi.Services;
using SolidApi.Helpers;

namespace SolidApi.Controllers;

[ApiController]
[Route("api/solid")]
public class ChannelController : ControllerBase
{
    private readonly SolidDatabase _database;
    private readonly DiscoveryService _discovery;

    public ChannelController(SolidDatabase database, DiscoveryService discovery)
    {
        _database = database;
        _discovery = discovery;
    }

    /// <summary>
    /// Creates a new channel inside an application.
    /// </summary>
    [HttpPost("{application}")]
    [ProducesResponseType(typeof(ChannelResource), 201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public ActionResult<ChannelResource> CreateChannel(
        string application,
        [FromBody] ChannelResource channel)
    {
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Channels
            (
                ApplicationId,
                ResourceName,
                ResourceDesc,
                DtCreation
            )
            VALUES
            (
                (
                    SELECT Id
                    FROM Applications
                    WHERE ResourceName = $application
                ),
                $resourceName,
                $resourceDesc,
                $dtCreation
            );";

        command.Parameters.AddWithValue(
            "$application",
            application);

        command.Parameters.AddWithValue(
            "$resourceName",
            channel.ResourceName);

        command.Parameters.AddWithValue(
            "$resourceDesc",
            channel.ResourceDesc ?? "");

        command.Parameters.AddWithValue(
            "$dtCreation",
            Clock.NowIso());

        command.ExecuteNonQuery();

        return StatusCode(201, channel);
    }

    // O discovery de canais (GET /api/solid/{app} com solid-discovery) passou para
    // o DiscoveryService, chamado pelo ApplicationController, dono desse URI.

    /// <summary>
    /// Retrieves a channel. With the solid-discovery header, returns the paths found inside it.
    /// </summary>
    [HttpGet("{application}/{channel}")]
    [ProducesResponseType(typeof(ChannelResource), 200)]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetChannel(
        string application,
        string channel,
        [FromHeader(Name = "solid-discovery")] string? discovery)
    {
        if (discovery != null)
            return Ok(_discovery.Find(discovery, application, channel));

        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT
                c.ResourceName,
                c.ResourceDesc,
                c.DtCreation
            FROM Channels c
            JOIN Applications a
                ON a.Id = c.ApplicationId
            WHERE a.ResourceName = $application
              AND c.ResourceName = $channel;";

        command.Parameters.AddWithValue(
            "$application",
            application);

        command.Parameters.AddWithValue(
            "$channel",
            channel);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return NotFound();

        return Ok(new ChannelResource
        {
            ResourceName = reader.GetString(0),
            ResourceDesc = reader.IsDBNull(1)
                ? null
                : reader.GetString(1),
            DtCreation = reader.GetString(2)
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
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Channels
            SET ResourceDesc = $resourceDesc
            WHERE Id =
            (
                SELECT c.Id
                FROM Channels c
                JOIN Applications a
                    ON a.Id = c.ApplicationId
                WHERE a.ResourceName = $application
                  AND c.ResourceName = $channel
            );";

        command.Parameters.AddWithValue(
            "$application",
            application);

        command.Parameters.AddWithValue(
            "$channel",
            channel);

        command.Parameters.AddWithValue(
            "$resourceDesc",
            updatedChannel.ResourceDesc ?? "");

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            return NotFound();

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
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            DELETE FROM Channels
            WHERE Id =
            (
                SELECT c.Id
                FROM Channels c
                JOIN Applications a
                    ON a.Id = c.ApplicationId
                WHERE a.ResourceName = $application
                  AND c.ResourceName = $channel
            );";

        command.Parameters.AddWithValue(
            "$application",
            application);

        command.Parameters.AddWithValue(
            "$channel",
            channel);

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            return NotFound();

        return NoContent();
    }
}