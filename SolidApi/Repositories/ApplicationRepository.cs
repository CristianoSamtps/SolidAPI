using SolidApi.Database;
using SolidApi.Models;

namespace SolidApi.Repositories;

/// <summary>
/// Acesso à tabela Applications.
/// </summary>
public class ApplicationRepository
{
    private readonly SolidDatabase _database;

    public ApplicationRepository(SolidDatabase database)
    {
        _database = database;
    }

    // Consultar uma application pelo nome
    public ApplicationResource? GetByName(string resourceName)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT ResourceName, ResourceDesc, DtCreation
            FROM Applications
            WHERE ResourceName = $name;
        ";

        command.Parameters.AddWithValue("$name", resourceName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new ApplicationResource
        {
            ResourceName = reader.GetString(0),
            ResourceDesc = reader.IsDBNull(1) ? null : reader.GetString(1),
            DtCreation = reader.GetString(2)
        };
    }
}
