# Services

Servicios de la aplicación. Todos se registran como **singleton** en `MauiProgram.cs`.

| Archivo | Descripción |
| ------- | ----------- |
| `AppConfigurationService.cs` | Carga `appsettings.json` y lo sobreescribe con las variables de entorno `ANTHROPIC_API_KEY` / `CLAUDE_*`. |
| `ClaudeApiService.cs` | Cliente de la API de Claude (SDK oficial `Anthropic`). |
| `DatabaseService.cs` | Conexión SQLite, esquema y operaciones CRUD. |
| `PdfExportService.cs` | Generación de PDFs del historial (QuestPDF). |
| `ServiceHelper.cs` | Localizador estático de servicios (DI). |

> Historial de cambios: `AiApiService.cs` (cliente genérico) fue reemplazado por
> `GeminiApiService.cs`, y este a su vez por `ClaudeApiService.cs`.

---

## `DatabaseService.DeleteAllAsync`

Vacía `Respuestas_IA`, `Detalle_Prompts` e `Historial_Prompts` dentro de una transacción
(`RunInTransactionAsync`), empezando por las tablas hijas para respetar las claves foráneas.
Si algo falla, no se borra nada. Lo usa el botón **Limpiar** del historial.

---

## `AppConfigurationService`

Orden de carga (la última fuente gana):

```
appsettings.json  <  Configuration/.env  <  variables de entorno del proceso
```

Ver `Configuration/README.md` para la lista de variables.

---

## `ClaudeApiService`

Usa el paquete NuGet oficial **`Anthropic`** (ver `MauiApp1.csproj`). No hay clases propias de
petición/respuesta: se usan los tipos del SDK (`MessageCreateParams`, `BetaMessage`, ...).

### Petición

`AskAsync(prompt)` llama a `client.Beta.Messages.Create(...)` con:

| Campo | Valor |
| ----- | ----- |
| `Model` | `Claude.Model` / `CLAUDE_MODEL` (por defecto `claude-opus-5`). |
| `MaxTokens` | `Claude.MaxTokens` / `CLAUDE_MAX_TOKENS` (por defecto `16000`). |
| `System` | `Claude.SystemInstruction` (texto plano, en español). |
| `Messages` | Un único mensaje `user` con la consulta. |
| `Fallbacks` + beta `server-side-fallback-2026-06-01` | Si el modelo principal **rechaza** la consulta, responde `Claude.FallbackModel` / `CLAUDE_FALLBACK_MODEL` (por defecto `claude-opus-4-8`) en la misma llamada. |

Se usa el endpoint **beta** solo para poder activar `Fallbacks`.

> Claude Opus 5 **no acepta** `temperature`, `top_p` ni `top_k` (devuelve 400), por eso ya no
> existe la opción de temperatura que había con Gemini.

### Respuesta

`AskAsync` devuelve un `ClaudeResultado` (`Models/ClaudeResultado.cs`):

| Propiedad | Origen |
| --------- | ------ |
| `Texto` | Concatenación de los bloques `BetaTextBlock` de `Content`. |
| `Modelo` | `BetaMessage.Model`: el modelo que respondió (el principal o el de respaldo). |
| `TokensConsumidos` | `Usage.InputTokens + Usage.OutputTokens`. |
| `Parametros` | JSON con modelo, modelo de respaldo, `max_tokens` e instrucción de sistema. |

### Errores

| Situación | Excepción |
| --------- | --------- |
| Falta `ANTHROPIC_API_KEY` | `InvalidOperationException` (mensaje indica cómo configurarla). |
| Clave inválida (401) | `InvalidOperationException` ("La clave de Claude no es válida…"). |
| Límite de uso superado (429) | `InvalidOperationException` ("Se superó el límite…"). |
| `StopReason == "refusal"` (también rechazada por el modelo de respaldo) | `InvalidOperationException` con `StopDetails.Explanation`. |
| Respuesta sin texto | `InvalidOperationException` con el `StopReason`. |
| Otros errores de la API (400, 404, 5xx…) | `AnthropicApiException` del SDK. |
| Timeout (`CLAUDE_TIMEOUT_SECONDS`) | Excepción de timeout del SDK. |

El SDK reintenta automáticamente (2 veces) los errores 408/409/429/5xx y los de conexión.
`ConsultarIaViewModel` captura todas las excepciones y guarda el error en `Respuestas_IA.Error`.

### Uso

```csharp
public MiViewModel(ClaudeApiService claude) => _claude = claude;

var resultado = await _claude.AskAsync("¿Qué es SQLite?");
Console.WriteLine(resultado.Texto);
```
