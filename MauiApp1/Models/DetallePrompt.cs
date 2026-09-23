namespace MauiApp1.Models
{
    /// <summary>
    /// Representa un registro de <c>Detalle_Prompts</c>: informacion tecnica y ampliada
    /// de cada consulta (prompt completo enviado a la IA, contexto y parametros usados).
    /// Un <see cref="HistorialPrompt"/> puede tener uno o varios detalles asociados.
    /// </summary>
    [SQLite.Table("Detalle_Prompts")]
    public class DetallePrompt
    {
        /// <summary>Identificador unico (autoincremental).</summary>
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        /// <summary>Identificador del registro padre en <c>Historial_Prompts</c>.</summary>
        public int HistorialPromptId { get; set; }

        /// <summary>Prompt completo (con plantilla, instrucciones y consulta) enviado a la IA.</summary>
        public string? PromptCompleto { get; set; }

        /// <summary>Contexto adicional (resultados previos, historial de la sesion, etc.).</summary>
        public string? Contexto { get; set; }

        /// <summary>Parametros de la llamada a la API en formato JSON (temperatura, max_tokens, etc.).</summary>
        public string? Parametros { get; set; }

        /// <summary>Fecha y hora en que se proceso el prompt.</summary>
        public DateTime FechaProceso { get; set; } = DateTime.Now;
    }
}