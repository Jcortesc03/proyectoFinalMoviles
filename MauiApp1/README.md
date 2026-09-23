# MauiApp1 — Buscador con IA

Aplicación .NET MAUI (net10.0) que funciona como un buscador sencillo: recibe una
pregunta del usuario, la envía a una **API de IA externa** y devuelve la respuesta en
**texto plano**. Cada consulta, su detalle técnico y la respuesta quedan persistidos en
una base de datos **SQLite** local.

---

## Estructura del proyecto

```
MauiApp1/
├── Configuration/
│   ├── appsettings.json        # Configuración embebida (BD + API IA)
│   └── AppSettings.cs          # Clases fuertemente tipadas para la configuración
├── Models/
│   ├── HistorialPrompt.cs      # Tabla Historial_Prompts
│   ├── DetallePrompt.cs        # Tabla Detalle_Prompts
│   └── RespuestaIA.cs          # Tabla Respuestas_IA
├── Services/
│   ├── AppConfigurationService.cs   # Carga appsettings.json desde el recurso embebido
│   └── DatabaseService.cs           # Conexión SQLite, esquema y operaciones CRUD
├── ViewModels/                 # (vacío por ahora) ViewModels MVVM
├── Views/                      # (vacío por ahora) Páginas MVVM
├── MauiProgram.cs              # Registro de servicios de DI
└── MauiApp1.csproj             # Dependencias de NuGet
```

---

## Dependencias

Paquetes agregados al `.csproj`:

| Paquete | Versión | Propósito |
| ------- | ------- | --------- |
| `sqlite-net-pcl` | 1.11.285 | ORM liviano para SQLite en MAUI |
| `Microsoft.Extensions.Configuration.Json` | 10.0.12 | Lectura de `appsettings.json` |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.12 | Mapeo de configuración a clases tipadas |

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

El archivo `Configuration/appsettings.json` está embebido como recurso en el ensamblado
(ver `EmbeddedResource` en el `.csproj`) y se carga en `AppConfigurationService`.

Dos secciones principales:

### `Database`

| Clave | Valor por defecto | Descripción |
| ----- | ----------------- | ----------- |
| `DatabaseFileName` | `mauiapp1.db3` | Nombre del archivo SQLite en `AppDataDirectory`. |
| `DeleteOnStartup` | `false` | Si es `true`, borra la BD al iniciar (recomendado solo en desarrollo). |
| `EnforceForeignKeys` | `true` | Activa `PRAGMA foreign_keys = ON` para respetar las relaciones. |

### `AiApi`

Configuración de la API de IA externa (aún no consumida por código, lista para el
servicio de IA):

| Clave | Valor por defecto | Descripción |
| ----- | ----------------- | ----------- |
| `BaseUrl` | `https://tu-api-ia.ejemplo.com/v1` | URL base de la API. **Cambiar por la real.** |
| `ApiKey` | *(vacío)* | Clave de autenticación. **No versionar**; usar `SecureStorage` o variables de entorno en producción. |
| `DefaultModel` | `modelo-por-defecto` | Modelo a usar en las consultas. |
| `TimeoutSeconds` | `60` | Timeout de las llamadas HTTP. |

Acceso desde el código:

```csharp
var config = ServiceHelper.GetService<AppConfigurationService>();
var baseUrl = config.Settings.AiApi.BaseUrl;
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
    ModeloIA  = config.Settings.AiApi.DefaultModel,
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

1. Crear un servicio `AiApiService` (cliente HTTP) que consuma `AiApiSettings`
   y devuelva el texto plano de la IA.
2. Orquestar en un ViewModel: insertar `HistorialPrompt` → `DetallePrompt` →
   llamar a la API → insertar `RespuestaIA`.
3. Crear las páginas en `Views/` para buscar y mostrar el historial.