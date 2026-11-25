using System.Globalization;

namespace SolidApi.Helpers;

/// <summary>
/// Datas no formato do enunciado (secção 2.4): yyyy-MM-ddTHH:mm:ss, hora local.
/// </summary>
public static class Clock
{
    public const string Format = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>Data e hora de agora, para o dt-creation.</summary>
    public static string NowIso() =>
        DateTime.Now.ToString(Format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Valida o dt-expiration. Vazio devolve null (não expira);
    /// data inválida ou no passado lança ArgumentException (400).
    /// </summary>
    public static string? NormalizeExpiration(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (!DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new ArgumentException("dt-expiration não é uma data ISO 8601 válida.");

        if (date <= DateTime.Now)
            throw new ArgumentException("dt-expiration não pode estar no passado.");

        return date.ToString(Format, CultureInfo.InvariantCulture);
    }
}