using Microsoft.Data.Sqlite;
using SolidApi.Database;
using SolidApi.Models;

namespace SolidApi.Repositories;

public class SubscriptionRepository
{
    private readonly SolidDatabase _database;

    public SubscriptionRepository(SolidDatabase database)
    {
        _database = database;
    }

    // Criar uma subscription
    public void Create(
        string app,
        string channel,
        SubscriptionResource resource)
    {
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Subscriptions
                (ChannelId, ResourceName, Evt, Endpoint,
                 DtCreation, DtExpiration)
            SELECT c.Id, $name, $evt, $endpoint, $creation, $expiration
            FROM Channels c
            INNER JOIN Applications a ON a.Id = c.ApplicationId
            WHERE a.ResourceName = $app
              AND c.ResourceName = $channel;
        ";

        command.Parameters.AddWithValue("$app", app);
        command.Parameters.AddWithValue("$channel", channel);
        command.Parameters.AddWithValue("$name", resource.ResourceName);
        command.Parameters.AddWithValue("$evt", resource.Evt);
        command.Parameters.AddWithValue("$endpoint", resource.Endpoint!);
        command.Parameters.AddWithValue("$creation", resource.DtCreation);

        command.Parameters.AddWithValue(
            "$expiration",
            string.IsNullOrWhiteSpace(resource.DtExpiration)
                ? DBNull.Value
                : resource.DtExpiration
        );

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            throw new InvalidOperationException("Channel not found.");
    }

    //Verificar se o channel existe
    public bool ChannelExists(string app, string channel)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
        SELECT COUNT(*)
        FROM Channels c
        INNER JOIN Applications a ON a.Id = c.ApplicationId
        WHERE a.ResourceName = $app
          AND c.ResourceName = $channel;
    ";

        command.Parameters.AddWithValue("$app", app);
        command.Parameters.AddWithValue("$channel", channel);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    //Verificar se já existe uma subscription com aquele nome no mesmo channel
    public bool ExistsByName(string app, string channel, string resourceName)
    {
        return GetByName(app, channel, resourceName) != null;
    }

    public List<(string ChannelName, string ResourceName)> GetAllByApplication(string app)
    {
        var subscriptions = new List<(string, string)>();

        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
        SELECT c.ResourceName, s.ResourceName
        FROM Subscriptions s
        INNER JOIN Channels c ON c.Id = s.ChannelId
        INNER JOIN Applications a ON a.Id = c.ApplicationId
        WHERE a.ResourceName = $app;
    ";

        command.Parameters.AddWithValue("$app", app);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            subscriptions.Add((
                reader.GetString(0),
                reader.GetString(1)
            ));
        }

        return subscriptions;
    }

    //Obter todas as subscriptions do middleware
    public List<(string AppName, string ChannelName, string ResourceName)> GetAll()
    {
        var subscriptions = new List<(string, string, string)>();

        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
        SELECT a.ResourceName, c.ResourceName, s.ResourceName
        FROM Subscriptions s
        INNER JOIN Channels c ON c.Id = s.ChannelId
        INNER JOIN Applications a ON a.Id = c.ApplicationId;
    ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            subscriptions.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }

        return subscriptions;
    }

    //Obter todas as subscriptions de um channel
    public List<string> GetAllByChannel(string app, string channel)
    {
        var subscriptions = new List<string>();

        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
        SELECT s.ResourceName
        FROM Subscriptions s
        INNER JOIN Channels c ON c.Id = s.ChannelId
        INNER JOIN Applications a ON a.Id = c.ApplicationId
        WHERE a.ResourceName = $app
          AND c.ResourceName = $channel;
    ";

        command.Parameters.AddWithValue("$app", app);
        command.Parameters.AddWithValue("$channel", channel);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            subscriptions.Add(reader.GetString(0));
        }

        return subscriptions;
    }


    // Consultar uma subscription
    public SubscriptionResource? GetByName(
        string app,
        string channel,
        string resourceName)
    {
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT s.ResourceName, s.Evt, s.Endpoint,
                   s.DtCreation, s.DtExpiration
            FROM Subscriptions s
            INNER JOIN Channels c ON c.Id = s.ChannelId
            INNER JOIN Applications a ON a.Id = c.ApplicationId
            WHERE a.ResourceName = $app
              AND c.ResourceName = $channel
              AND s.ResourceName = $name;
        ";

        command.Parameters.AddWithValue("$app", app);
        command.Parameters.AddWithValue("$channel", channel);
        command.Parameters.AddWithValue("$name", resourceName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new SubscriptionResource
        {
            ResourceName = reader.GetString(0),
            Evt = reader.GetInt32(1),
            Endpoint = reader.GetString(2),
            DtCreation = reader.GetString(3),
            DtExpiration = reader.IsDBNull(4)
                ? ""
                : reader.GetString(4)
        };
    }

    // Eliminar uma subscription
    public bool Delete(
        string app,
        string channel,
        string resourceName)
    {
        using var connection = _database.OpenConnection();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            DELETE FROM Subscriptions
            WHERE ResourceName = $name
              AND ChannelId = (
                  SELECT c.Id
                  FROM Channels c
                  INNER JOIN Applications a ON a.Id = c.ApplicationId
                  WHERE a.ResourceName = $app
                    AND c.ResourceName = $channel
              );
        ";

        command.Parameters.AddWithValue("$app", app);
        command.Parameters.AddWithValue("$channel", channel);
        command.Parameters.AddWithValue("$name", resourceName);

        return command.ExecuteNonQuery() > 0;
    }
}
