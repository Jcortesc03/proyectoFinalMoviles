# ViewModels

ViewModels MVVM (CommunityToolkit.Mvvm) de cada ventana. Se registran como singleton en
`MauiProgram.cs`.

| Archivo | Ventana |
| ------- | ------- |
| `InicioViewModel.cs` | Inicio |
| `ConsultarIaViewModel.cs` | Consultar IA |
| `HistorialViewModel.cs` | Consultar historial |
| `DetalleViewModel.cs` | Detalle |
| `AcercaDeViewModel.cs` | Acerca de |

---

## `ConsultarIaViewModel` y Claude

Recibe por inyección `ClaudeApiService` (antes `GeminiApiService` y, antes, `AiApiService`), `DatabaseService` y
`AppConfigurationService`.

Flujo del comando `ConsultarCommand`:

1. Valida que la consulta no esté vacía y la recorta a 500 caracteres.
2. Llama a `ClaudeApiService.AskAsync(texto)` y mide la duración.
3. **Éxito:** muestra `resultado.Texto` y guarda:
   - `Historial_Prompts`: `Estado = "Procesado"`, `ModeloIA` (el modelo que respondió realmente), `TokenConsumidos`, `DuracionMs`.
   - `Detalle_Prompts`: `PromptCompleto` y `Parametros` (JSON de la llamada).
   - `Respuestas_IA`: `RespuestaTexto`.
4. **Error** (sin `ANTHROPIC_API_KEY`, clave inválida, límite de uso, consulta rechazada, timeout…): muestra el mensaje en
   `MensajeError` y guarda `Estado = "Error"` y `Respuestas_IA.Error`.

Si falta la clave, el mensaje de error indica que se debe definir `ANTHROPIC_API_KEY`
(ver `Configuration/README.md`).

---

## `HistorialViewModel`: botón Limpiar

`LimpiarHistorialCommand` (antes `LimpiarFiltroCommand`, que solo quitaba el filtro de fechas):

1. Si no hay consultas, muestra el aviso "Historial vacío" y no hace nada.
2. Muestra un modal (`DisplayAlertAsync`) con la cantidad de consultas que se van a borrar
   y los botones **Eliminar todo** / **Cancelar**.
3. Al confirmar, llama a `DatabaseService.DeleteAllAsync()`, quita el filtro y recarga la lista.
4. Si falla la base de datos, muestra el error en otro modal.
