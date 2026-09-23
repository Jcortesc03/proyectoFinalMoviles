# MauiApp1 — Buscador con IA

Aplicación .NET MAUI (net10.0) que funciona como un buscador sencillo: recibe una
pregunta del usuario, la envía a una **API de IA externa** y devuelve la respuesta en
**texto plano**. Cada consulta, su detalle técnico y la respuesta quedan persistidos en
una base de datos **SQLite** local.

La IA utilizada es **Claude** (Anthropic), a través del SDK oficial `Anthropic`; la clave se
configura mediante la variable de entorno `ANTHROPIC_API_KEY` (ver [Configuración](#configuración)).

---

## Estructura del proyecto

```
MauiApp1/
├── Configuration/                   # Ver Configuration/README.md
│   ├── appsettings.json        # Configuración embebida (BD + Claude, sin secretos)
│   ├── AppSettings.cs          # Clases fuertemente tipadas para la configuración
│   ├── EnvironmentVariables.cs # Lectura de ANTHROPIC_API_KEY y CLAUDE_*
│   ├── env_example             # Plantilla de variables de entorno (versionada)
│   └── .env                    # Variables locales con la clave (NO versionado)
├── Models/                          # Ver Models/README.md
│   ├── HistorialPrompt.cs      # Tabla Historial_Prompts
│   ├── DetallePrompt.cs        # Tabla Detalle_Prompts
│   ├── RespuestaIA.cs          # Tabla Respuestas_IA
│   ├── ConsultaDto.cs          # DTO para listar/exportar consultas
│   └── ClaudeResultado.cs      # Resultado procesado de una consulta a Claude
├── Services/                        # Ver Services/README.md
│   ├── AppConfigurationService.cs   # Carga appsettings.json + variables de entorno
│   ├── DatabaseService.cs           # Conexión SQLite, esquema y operaciones CRUD
│   ├── ClaudeApiService.cs          # Cliente de la API de Claude (SDK Anthropic)
│   ├── PdfExportService.cs          # Generación de PDFs (QuestPDF)
│   └── ServiceHelper.cs             # Localizador estático de servicios (DI)
├── ViewModels/                      # Ver ViewModels/README.md
│   ├── InicioViewModel.cs           # Ventana Inicio
│   ├── ConsultarIaViewModel.cs      # Ventana Consultar IA
│   ├── HistorialViewModel.cs        # Ventana Consultar historial
│   ├── DetalleViewModel.cs          # Ventana Detalle
│   └── AcercaDeViewModel.cs         # Ventana Acerca de
├── Views/                           # Páginas (Inicio/ConsultarIa/Historial/Detalle/AcercaDe)
├── MauiProgram.cs              # Registro de servicios de DI
└── MauiApp1.csproj             # Dependencias de NuGet
```

---

## Ventanas de la aplicación

La aplicación tiene 5 ventanas organizadas con `Shell` (4 pestañas + detalle):

| Ventana | Página / ViewModel | Descripción |
| ------- | ------------------ | ----------- |
| **Inicio** | `Views/InicioPage` | Propósito del proyecto, creadores y acceso rápido al resto de ventanas. |
| **Consultar IA** | `Views/ConsultarIaPage` | Barra de búsqueda extendible (límite 500 caracteres) y cuadro con la respuesta en texto plano de la IA. |
| **Consultar historial** | `Views/HistorialPage` | Lista las consultas guardadas, filtra por fecha, descarga en PDF o elimina de la base de datos. |
| **Detalle** | `Views/DetallePage` | Muestra el prompt enviado, la respuesta, la fecha y el estado ("Respondido con éxito" / "Sin respuesta"). Se abre con la ruta `DetallePage?id=X`. |
| **Acerca de** | `Views/AcercaDePage` | Detalles del proyecto y tecnologías usadas. |

> La ruta `DetallePage?id=X` se registra en `AppShell` (`Routing.RegisterRoute`). Una consulta,
> su detalle técnico y su respuesta se guardan juntos en SQLite al hacer una consulta a la IA.

---

## Dependencias

Paquetes agregados al `.csproj`:

| Paquete | Versión | Propósito |
| ------- | ------- | --------- |
| `sqlite-net-pcl` | 1.11.285 | ORM liviano para SQLite en MAUI |
| `Microsoft.Extensions.Configuration.Json` | 10.0.12 | Lectura de `appsettings.json` |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.12 | Mapeo de configuración a clases tipadas |
| `Microsoft.Maui.Controls` | 10.0.90 | Requerido por CommunityToolkit.Maui |
| `CommunityToolkit.Maui` | 15.0.1 | `FileSaver` (guardar PDFs) y funcionalidades del toolkit |
| `CommunityToolkit.Mvvm` | 8.4.2 | MVVM (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) |
| `QuestPDF` | 2026.9.0 | Generación de documentos PDF |
| `Anthropic` | 12.50.0 | SDK oficial de la API de Claude |

Para instalar/restaurar las dependencias:

```bash
dotnet restore
```

Para compilar:

```bash
dotnet build -f net10.0-windows10.0.19041.0   # Windows
dotnet build -f net10.0-android               # Android
```

> `sqlite-net-pcl` trae `SQLitePCLRaw.bundle_green`, que incluye el proveedor nativo de
> SQLite para todas las plataformas de MAUI.

---

## Configuración

La configuración se arma en `AppConfigurationService` combinando, de menor a mayor prioridad:

1. `Configuration/appsettings.json` (embebido como recurso, ver `EmbeddedResource` en el `.csproj`).
2. `Configuration/.env` (embebido solo si existe; no se versiona).
3. Variables de entorno del proceso.

Detalle completo en [`Configuration/README.md`](Configuration/README.md).

### Puesta en marcha rápida (Claude)

```bash
cp Configuration/env_example Configuration/.env
# editar Configuration/.env -> ANTHROPIC_API_KEY=sk-ant-...   (https://platform.claude.com/settings/keys)
dotnet build -f net10.0-windows10.0.19041.0
```

En Windows también se puede usar una variable de entorno real:

```powershell
$env:ANTHROPIC_API_KEY = "sk-ant-..."
```

### `Database`

| Clave | Valor por defecto | Descripción |
| ----- | ----------------- | ----------- |
| `DatabaseFileName` | `mauiapp1.db3` | Nombre del archivo SQLite en `AppDataDirectory`. |
| `DeleteOnStartup` | `false` | Si es `true`, borra la BD al iniciar (recomendado solo en desarrollo). |
| `EnforceForeignKeys` | `true` | Activa `PRAGMA foreign_keys = ON` para respetar las relaciones. |

### `Claude`

| Clave | Variable de entorno | Valor por defecto | Descripción |
| ----- | ------------------- | ----------------- | ----------- |
| `ApiKey` | `ANTHROPIC_API_KEY` | *(vacío)* | Clave de la API. **Obligatoria. No versionar.** |
| `Model` | `CLAUDE_MODEL` | `claude-opus-5` | Modelo usado en las consultas. |
| `FallbackModel` | `CLAUDE_FALLBACK_MODEL` | `claude-opus-4-8` | Responde si el modelo principal rechaza la consulta. |
| `MaxTokens` | `CLAUDE_MAX_TOKENS` | `16000` | Máximo de tokens de la respuesta. |
| `TimeoutSeconds` | `CLAUDE_TIMEOUT_SECONDS` | `120` | Timeout de las llamadas HTTP. |
| `SystemInstruction` | — | *(texto plano, en español)* | Instrucción de sistema enviada en cada consulta. |

Acceso desde el código:

```csharp
var config = ServiceHelper.GetService<AppConfigurationService>();
var modelo = config.Settings.Claude.Model;
```

---

## Base de datos SQLite

La BD se almacena en `FileSystem.AppDataDirectory` (ruta propia de cada plataforma).
El esquema se crea automáticamente al llamar a `DatabaseService.InitializeAsync()`
(tablas, índices y claves foráneas).

### Diagrama de relaciones

```
Historial_Prompts 1 ──── * Detalle_Prompts   (HistorialPromptId)
Historial_Prompts 1 ──── * Respuestas_IA     (HistorialPromptId)
Detalle_Prompts   1 ──── 0..1 Respuestas_IA  (DetallePromptId, opcional)
```

### Tabla `Historial_Prompts`

Cabecera del historial: cada pregunta del usuario.

| Columna | Tipo | Descripción |
| ------- | ---- | ----------- |
| `Id` | INTEGER PK | Identificador (autoincremental). |
| `Consulta` | TEXT | Texto de la consulta/pregunta del usuario. |
| `FechaConsulta` | INTEGER | Fecha/hora de la consulta (ticks). |
| `ModeloIA` | TEXT | Modelo de IA utilizado. |
| `Estado` | TEXT | `Pendiente`, `Procesado`, `Error` o `Cancelado`. |
| `TokenConsumidos` | INTEGER | Tokens consumidos (si la API los reporta). |
| `DuracionMs` | INTEGER | Duración del procesamiento, en ms. |

### Tabla `Detalle_Prompts`

Detalle técnico de cada prompt enviado a la IA.

| Columna | Tipo | Descripción |
| ------- | ---- | ----------- |
| `Id` | INTEGER PK | Identificador. |
| `HistorialPromptId` | INTEGER FK | → `Historial_Prompts.Id` (CASCADE). |
| `PromptCompleto` | TEXT | Prompt final construido (plantilla + consulta). |
| `Contexto` | TEXT | Contexto adicional (historial, resultados previos). |
| `Parametros` | TEXT | Parámetros de la llamada API en JSON. |
| `FechaProceso` | INTEGER | Fecha/hora del procesamiento (ticks). |

### Tabla `Respuestas_IA`

Respuestas en texto plano devueltas por la IA.

| Columna | Tipo | Descripción |
| ------- | ---- | ----------- |
| `Id` | INTEGER PK | Identificador. |
| `HistorialPromptId` | INTEGER FK | → `Historial_Prompts.Id` (CASCADE). |
| `DetallePromptId` | INTEGER FK | → `Detalle_Prompts.Id` (nulo, SET NULL). |
| `RespuestaTexto` | TEXT | Texto plano devuelto por la IA. |
| `Referencias` | TEXT | Referencias/fuentes citadas (texto o JSON). |
| `Error` | TEXT | Mensaje de error, en caso de fallo. |
| `FechaRespuesta` | INTEGER | Fecha/hora de la respuesta (ticks). |

---

## Uso del `DatabaseService`

El servicio se registra como singleton en `MauiProgram.cs` mediante inyección de
dependencias:

```csharp
builder.Services.AddSingleton<AppConfigurationService>();
builder.Services.AddSingleton<DatabaseService>();
```

### Inicialización

Hay que inicializar la base de datos una vez antes de usarla (idempotente). Lo más
sencillo es inyectarla en el constructor de la clase (View, ViewModel o página):

```csharp
public class MiPageViewModel
{
    private readonly DatabaseService _db;

    public MiPageViewModel(AppConfigurationService config, DatabaseService db)
    {
        _db = db;
        _ = db.InitializeAsync();   // o await desde un método async
    }
}
```

### Ejemplo de CRUD

```csharp
// Suponiendo "_db" (DatabaseService) y "config" (AppConfigurationService) inyectados por DI.

// 1) Registrar la consulta y persistirla
var historial = new HistorialPrompt
{
    Consulta = "¿Qué es SQLite?",
    ModeloIA  = config.Settings.Claude.Model,
    Estado    = "Procesado",
};
await _db.InsertHistorialAsync(historial);

// 2) Guardar el detalle del prompt enviado
var detalle = new DetallePrompt
{
    HistorialPromptId = historial.Id,
    PromptCompleto    = "Responde en texto plano claro: ¿Qué es SQLite?",
    Parametros        = "{\"temperature\":0.7}",
};
await _db.InsertDetalleAsync(detalle);

// 3) Guardar la respuesta de la IA
var respuesta = new RespuestaIA
{
    HistorialPromptId = historial.Id,
    DetallePromptId   = detalle.Id,
    RespuestaTexto    = "SQLite es un sistema de gestión de bases de datos relacional embebido…",
};
await _db.InsertRespuestaAsync(respuesta);

// 4) Recuperar el historial con sus respuestas
var historialCompleto = await _db.GetHistorialesAsync();
var respuestas        = await _db.GetRespuestasByHistorialAsync(historial.Id);
```

---

## Próximos pasos sugeridos

1. Crear `Configuration/.env` con `ANTHROPIC_API_KEY` (ver `Configuration/README.md`).
2. Para producción, mover la clave de Claude a un backend propio (el `.env` queda embebido
   en el binario).
3. Ampliar filtros del historial (por texto, por estado, etc.).
4. Agregar pruebas unitarias para los ViewModels, `ClaudeApiService` y `PdfExportService`.
