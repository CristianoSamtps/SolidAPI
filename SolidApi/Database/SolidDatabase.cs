using Microsoft.Data.Sqlite;

namespace SolidApi.Database;

/// <summary>
/// Acesso à base de dados SQLite do SOLID.
/// Cria as tabelas no arranque (Data/schema.sql) e insere os dados de exemplo
/// (Data/seed.sql) quando a BD está vazia.
/// </summary>
public class SolidDatabase
{
    private readonly string _connectionString;

    public SolidDatabase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Solid") ?? "Data Source=solid.db";
        Initialize();
    }

    /// <summary>
    /// Abre uma ligação à BD com as chaves estrangeiras ativas.
    /// Todo o acesso à BD no projeto deve passar por aqui.
    /// </summary>
    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Sem isto o SQLite ignora as FOREIGN KEY e o ON DELETE CASCADE
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private void Initialize()
    {
        using var connection = OpenConnection();

        Execute(connection, ReadScript("schema.sql"));

        // Só insere os dados de exemplo se ainda não houver nenhuma aplicação
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Applications;";
        if ((long)count.ExecuteScalar()! == 0)
            Execute(connection, ReadScript("seed.sql"));
    }

    // Os scripts são copiados para bin/.../Sql/ pelo SolidApi.csproj (ponto 4.2)
    private static string ReadScript(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Sql", fileName));

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}