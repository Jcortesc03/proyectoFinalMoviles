namespace MauiApp1.Models
{
    /// <summary>
    /// Resultado de una consulta a Claude ya procesado por <c>ClaudeApiService</c>,
    /// listo para mostrar y guardar en la base de datos.
    /// </summary>
    /// <param name="Texto">Respuesta en texto plano.</param>
    /// <param name="Modelo">Modelo que respondio (el principal o el de respaldo).</param>
    /// <param name="TokensConsumidos">Total de tokens reportados por la API (si los reporta).</param>
    /// <param name="Parametros">Parametros de la llamada en JSON (para <c>Detalle_Prompts.Parametros</c>).</param>
    public sealed record ClaudeResultado(
        string Texto,
        string Modelo,
        int? TokensConsumidos,
        string Parametros);
}
