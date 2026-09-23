namespace MauiApp1.Models
{
    /// <summary>
    /// Representa un registro de <c>Respuestas_IA</c>: el resultado devuelto por la IA
    /// para una consulta. Contiene el texto plano de la respuesta (el buscador solo
    /// devuelve texto plano) y, si aplica, referencias o errores.
    /// </summary>
    [SQLite.Table("Respuestas_IA")]
    public class RespuestaIA
    {
        /// <summary>Identificador unico (autoincremental).</summary>
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        /// <summary>Identificador del registro padre en <c>Historial_Prompts</c>.</summary>
        public int HistorialPromptId { get; set; }

        /// <summary>
        /// Identificador del detalle en <c>Detalle_Prompts</c> asociado. Puede ser nulo
        /// si la respuesta no proviene de un detalle registrado.
        /// </summary>
        public int? DetallePromptId { get; set; }

        /// <summary>Texto plano devuelto por la IA como respuesta a la consulta.</summary>
        public string? RespuestaTexto { get; set; }

        /// <summary>Referencias o fuentes citadas por la IA (serializadas como texto/JSON).</summary>
        public string? Referencias { get; set; }

        /// <summary>Mensaje de error si la llamada a la IA fallo. Nulo si fue exitosa.</summary>
        public string? Error { get; set; }

        /// <summary>Fecha y hora en que se recibio la respuesta.</summary>
        public DateTime FechaRespuesta { get; set; } = DateTime.Now;
    }
}