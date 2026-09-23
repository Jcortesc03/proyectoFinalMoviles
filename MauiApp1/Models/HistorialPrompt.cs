namespace MauiApp1.Models
{
    /// <summary>
    /// Representa un registro de <c>Historial_Prompts</c>: cada consulta que el usuario
    /// realiza al buscador de IA queda registrada aqui como cabecera del historial.
    /// </summary>
    [SQLite.Table("Historial_Prompts")]
    public class HistorialPrompt
    {
        /// <summary>Identificador unico (autoincremental).</summary>
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        /// <summary>Texto de la consulta/pregunta realizada por el usuario.</summary>
        public string Consulta { get; set; } = string.Empty;

        /// <summary>Fecha y hora en que se realizo la consulta.</summary>
        public DateTime FechaConsulta { get; set; } = DateTime.Now;

        /// <summary>Modelo de IA utilizado para responder (ej. configurado en <c>AiApi.DefaultModel</c>).</summary>
        public string? ModeloIA { get; set; }

        /// <summary>
        /// Estado de la consulta. Valores sugeridos:
        /// <c>Pendiente</c>, <c>Procesado</c>, <c>Error</c> o <c>Cancelado</c>.
        /// </summary>
        public string? Estado { get; set; }

        /// <summary>Cantidad de tokens consumidos por la IA (si la API los reporta).</summary>
        public int? TokenConsumidos { get; set; }

        /// <summary>Duracion total del procesamiento en milisegundos.</summary>
        public long? DuracionMs { get; set; }
    }
}