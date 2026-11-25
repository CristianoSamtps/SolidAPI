using Microsoft.AspNetCore.Mvc;
using SolidApi.Database;
using SolidApi.Models;

namespace SolidApi.Controllers;

[ApiController]
[Route("api/solid")]
public class ChannelController : ControllerBase
{
    private readonly SolidDatabase _database;

    public ChannelController(SolidDatabase database)
    {
        _database = database;
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
            DateTime.UtcNow.ToString("s"));

        command.ExecuteNonQuery();

        return StatusCode(201, channel);
    }

    /// <summary>
    /// Discovers all channels belonging to an application.
    /// </summary>
    [HttpGet("{application}")]
    [ProducesResponseType(typeof(string[]), 200)]
    public IActionResult DiscoverChannels(
        string application,
        [FromHeader(Name = "solid-discovery")]
        string discovery)
    {
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT c.ResourceName
            FROM Channels c
            JOIN Applications a
                ON a.Id = c.ApplicationId
            WHERE a.ResourceName = $application;";

        command.Parameters.AddWithValue(
            "$application",
            application);

        var results = new List<string>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            results.Add(
                $"/api/solid/{application}/{reader.GetString(0)}");
        }

        return Ok(results);
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