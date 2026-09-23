# Configuration

Configuración de la aplicación: base de datos SQLite y conexión con la API de **Claude** (Anthropic).

| Archivo | Descripción |
| ------- | ----------- |
| `appsettings.json` | Valores por defecto (embebido en el ensamblado). **No contiene secretos.** |
| `AppSettings.cs` | Clases tipadas: `AppSettings`, `DatabaseSettings`, `ClaudeSettings`. |
| `EnvironmentVariables.cs` | Lee las variables de entorno `ANTHROPIC_API_KEY` y `CLAUDE_*` (del sistema o de `.env`) y las traduce a claves de configuración. |
| `env_example` | Plantilla del archivo `.env`. Sí se versiona. |
| `.env` | Tus valores locales (clave de API). **No se versiona** (`.gitignore`). |

---

## Variables de entorno de Claude

| Variable | Clave de configuración | Obligatoria | Valor por defecto |
| -------- | ---------------------- | ----------- | ----------------- |
| `ANTHROPIC_API_KEY` | `Claude:ApiKey` | **Sí** | *(vacío)* |
| `CLAUDE_MODEL` | `Claude:Model` | No | `claude-opus-5` |
| `CLAUDE_FALLBACK_MODEL` | `Claude:FallbackModel` | No | `claude-opus-4-8` (responde si el principal rechaza la consulta) |
| `CLAUDE_MAX_TOKENS` | `Claude:MaxTokens` | No | `16000` |
| `CLAUDE_TIMEOUT_SECONDS` | `Claude:TimeoutSeconds` | No | `120` |

> Las variables `GEMINI_*` ya no se usan. Si siguen en tu `.env`, se ignoran.

`Claude:SystemInstruction` solo se configura en `appsettings.json` (instrucción de sistema
que pide respuestas en texto plano y en español).

### Orden de prioridad

De menor a mayor (la última gana):

1. `appsettings.json`
2. `Configuration/.env` (embebido al compilar, si existe)
3. Variables de entorno del proceso (`Environment.GetEnvironmentVariable`)

Todo se combina en `Services/AppConfigurationService.cs` con
`ConfigurationBuilder().AddJsonStream(...).AddInMemoryCollection(EnvironmentVariables.Load())`.

---

## Cómo configurar la clave

1. Obtener una clave en <https://platform.claude.com/settings/keys> (empieza por `sk-ant-`).
2. Elegir **una** de estas opciones:

**Opción A — archivo `.env` (funciona en todas las plataformas, incluido Android/iOS):**

```bash
cp Configuration/env_example Configuration/.env
# editar Configuration/.env y completar ANTHROPIC_API_KEY=...
```

El `.csproj` lo embebe como recurso (`LogicalName="MauiApp1.Configuration.env"`) solo si
el archivo existe, así que el proyecto compila igual sin él. Recompilar tras editarlo.

> ⚠️ No usar **"Incluir en el proyecto"** sobre `.env` ni cambiar su *Acción de compilación*
> a **Compilar**: Visual Studio agrega `<Compile Include="Configuration\.env" />` y el
> compilador intenta leerlo como C#, dando errores como *CS0103: El nombre 'ANTHROPIC_API_KEY'
> no existe en el contexto actual*. Debe quedar como **Recurso incrustado** (ya lo configura
> el `.csproj`).

**Opción B — variable de entorno del sistema (Windows / Mac Catalyst):**

```powershell
# PowerShell (sesión actual)
$env:ANTHROPIC_API_KEY = "sk-ant-..."
# Permanente para el usuario (reiniciar Visual Studio después)
[Environment]::SetEnvironmentVariable("ANTHROPIC_API_KEY", "sk-ant-...", "User")
```

> En Android/iOS la app no hereda variables de entorno del equipo de desarrollo, por eso
> allí se debe usar la opción A.

### Formato del `.env`

- Una variable por línea: `CLAVE=valor`.
- Líneas vacías y las que empiezan por `#` se ignoran.
- Las comillas alrededor del valor (`"..."` o `'...'`) se eliminan.

> ⚠️ **Seguridad:** el `.env` queda embebido en el binario; cualquiera con el APK/ejecutable
> podría extraer la clave. Es aceptable para desarrollo y el proyecto académico, pero en
> producción la clave debería vivir en un backend propio que haga de intermediario.

---

## Acceso desde el código

```csharp
var config = ServiceHelper.GetService<AppConfigurationService>();
var modelo = config.Settings.Claude.Model;
```
