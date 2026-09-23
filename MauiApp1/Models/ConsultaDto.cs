namespace MauiApp1.Models
{
    /// <summary>
    /// DTO de una consulta del historial, preparado para mostrar en la lista
    /// del historial y para exportar a PDF. Agrega el estado calculado de la consulta.
    /// </summary>
    public class ConsultaDto
    {
        /// <summary>Identificador del registro en <c>Historial_Prompts</c>.</summary>
        public int Id { get; init; }

        /// <summary>Texto de la consulta realizada.</summary>
        public string Consulta { get; init; } = string.Empty;

        /// <summary>Fecha de la consulta.</summary>
        public DateTime Fecha { get; init; }

        /// <summary>Estado legible: "Respondido con exito" o "Sin respuesta".</summary>
        public string Estado { get; init; } = "Sin respuesta";

        /// <summary>Prompt completo enviado a la IA (si existe detalle).</summary>
        public string? PromptEnviado { get; init; }

        /// <summary>Respuesta en texto plano obtenida de la IA (si existe).</summary>
        public string? RespuestaTexto { get; init; }

        /// <summary>Mensaje de error de la llamada (si la hubo).</summary>
        public string? Error { get; init; }
    }
}