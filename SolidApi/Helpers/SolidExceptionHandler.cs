using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.Sqlite;

namespace SolidApi.Helpers;

/// <summary>
/// Converte as exceções lançadas pelos serviços em respostas HTTP:
/// ArgumentException -> 400, KeyNotFoundException -> 404,
/// violação de UNIQUE/CHECK/FOREIGN KEY no SQLite -> 409, resto -> 500.
/// </summary>
public class SolidExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        int status = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            SqliteException { SqliteErrorCode: 19 } => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        // Num erro inesperado não se mostra o detalhe técnico ao cliente
        string message = status == StatusCodes.Status500InternalServerError
            ? "Erro interno do servidor."
            : exception.Message;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
