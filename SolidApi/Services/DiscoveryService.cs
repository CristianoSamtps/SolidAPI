using Microsoft.Data.Sqlite;
using SolidApi.Database;

namespace SolidApi.Services;

/// <summary>
/// Discovery (header solid-discovery): devolve os paths dos recursos do tipo pedido
/// dentro do âmbito do URI (raiz, aplicação ou canal).
/// É o ponto único de discovery: os controladores de cada nível chamam-no
/// quando o pedido GET traz o header, em vez de terem rotas próprias.
/// </summary>
public class DiscoveryService
{
    public static readonly string[] ValidTypes = { "application", "channel", "data-record", "subscription" };

    private readonly SolidDatabase _database;
    private readonly SubscriptionService _subscriptions;

    public DiscoveryService(SolidDatabase database, SubscriptionService subscriptions)
    {
        _database = database;
        _subscriptions = subscriptions;
    }

    /// <summary>
    /// Procura recursos do tipo <paramref name="type"/> dentro do âmbito dado.
    /// app = null: todo o middleware; channel = null: toda a aplicação.
    /// Tipo inválido lança ArgumentException (400); aplicação ou canal inexistente
    /// lança KeyNotFoundException (404).
    /// </summary>
    public List<string> Find(string? type, string? app = null, string? channel = null)
    {
        if (type == null || !ValidTypes.Contains(type))
            throw new ArgumentException(
                "solid-discovery inválido: use application, channel, data-record ou subscription.");

        if (app != null && !ApplicationExists(app))
            throw new KeyNotFoundException("Aplicação não encontrada.");

        if (channel != null && !ChannelExists(app!, channel))
            throw new KeyNotFoundException("Canal não encontrado.");

        // Um recurso nunca contém recursos do seu nível ou de níveis acima:
        // nesse caso o enunciado pede uma lista vazia com 200 OK
        return type switch
        {
            "application" => app == null ? FindApplications() : new List<string>(),
            "channel" => channel == null ? FindChannels(app) : new List<string>(),
            "data-record" => FindDataRecords(app, channel),
            _ => FindSubscriptions(app, channel)
        };
    }

    public bool ApplicationExists(string app)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Applications WHERE ResourceName = $app;";
        command.Parameters.AddWithValue("$app", app);
        return (long)command.ExecuteScalar()! > 0;
    }

    public bool ChannelExists(string app, string channel) => _subscriptions.ChannelExists(app, channel);

    private List<string> FindApplications() =>
        Query("SELECT ResourceName FROM Applications;",
              null, null,
              r => $"/api/solid/{r.GetString(0)}");

    private List<string> FindChannels(string? app) =>
        Query(@"SELECT a.ResourceName, c.ResourceName
                FROM Channels c
                JOIN Applications a ON a.Id = c.ApplicationId
                WHERE ($app IS NULL OR a.ResourceName = $app);",
              app, null,
              r => $"/api/solid/{r.GetString(0)}/{r.GetString(1)}");

    private List<string> FindDataRecords(string? app, string? channel) =>
        Query(@"SELECT a.ResourceName, c.ResourceName, d.ResourceName
                FROM DataRecords d
                JOIN Channels c ON c.Id = d.ChannelId
                JOIN Applications a ON a.Id = c.ApplicationId
                WHERE ($app IS NULL OR a.ResourceName = $app)
                  AND ($channel IS NULL OR c.ResourceName = $channel);",
              app, channel,
              r => $"/api/solid/{r.GetString(0)}/{r.GetString(1)}/{r.GetString(2)}");

    // As subscrições usam os métodos de discovery do SubscriptionService (inclui o nó "subs")
    private List<string> FindSubscriptions(string? app, string? channel)
    {
        if (app == null)
            return _subscriptions.DiscoverAll();

        if (channel == null)
            return _subscriptions.DiscoverByApplication(app);

        return _subscriptions.DiscoverByChannel(app, channel);
    }

    private List<string> Query(string sql, string? app, string? channel, Func<SqliteDataReader, string> toPath)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$app", (object?)app ?? DBNull.Value);
        command.Parameters.AddWithValue("$channel", (object?)channel ?? DBNull.Value);

        var paths = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            paths.Add(toPath(reader));

        return paths;
    }
}
