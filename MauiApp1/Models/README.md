# Models

Clases de datos de la aplicación.

## Tablas SQLite

| Archivo | Tabla | Descripción |
| ------- | ----- | ----------- |
| `HistorialPrompt.cs` | `Historial_Prompts` | Cabecera: cada consulta del usuario. `ModeloIA` y `TokenConsumidos` se llenan con los datos de Claude. |
| `DetallePrompt.cs` | `Detalle_Prompts` | Detalle técnico. `Parametros` guarda el JSON de la llamada a Claude. |
| `RespuestaIA.cs` | `Respuestas_IA` | Texto plano devuelto por Claude o mensaje de error. |

## DTOs

| Archivo | Descripción |
| ------- | ----------- |
| `ConsultaDto.cs` | Consulta del historial preparada para la lista y la exportación a PDF. |
| `ClaudeResultado.cs` | `record` que devuelve `ClaudeApiService.AskAsync`: `Texto`, `Modelo`, `TokensConsumidos`, `Parametros`. |

La petición y la respuesta a Claude usan los tipos del SDK oficial `Anthropic`
(`MessageCreateParams`, `BetaMessage`…), por eso ya no hay clases propias como las
`GeminiRequest` / `GeminiResponse` que se usaban con Gemini (eliminadas).

### Mapeo de `ClaudeResultado` a la base de datos

| `ClaudeResultado` | Columna |
| ----------------- | ------- |
| `Texto` | `Respuestas_IA.RespuestaTexto` |
| `Modelo` | `Historial_Prompts.ModeloIA` |
| `TokensConsumidos` | `Historial_Prompts.TokenConsumidos` |
| `Parametros` | `Detalle_Prompts.Parametros` |
