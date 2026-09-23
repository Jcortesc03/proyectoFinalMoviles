using MauiApp1.Configuration;
using MauiApp1.Models;
using SQLite;

namespace MauiApp1.Services
{
    /// <summary>
    /// Servicio de acceso a la base de datos SQLite.
    /// Crea el esquema (tablas, indices y claves foraneas) y expone operaciones CRUD
    /// tipadas sobre <see cref="HistorialPrompt"/>, <see cref="DetallePrompt"/> y <see cref="RespuestaIA"/>.
    /// Se registra como singleton en <see cref="MauiProgram"/>.
    /// </summary>
    public sealed class DatabaseService
    {
        private const string EnableForeignKeysSql = "PRAGMA foreign_keys = ON;";

        /// <summary>Esquema creado al inicializar. Las claves foraneas garantizan integridad referencial.</summary>
        private static readonly string[] SchemaStatements =
        {
            """
            CREATE TABLE IF NOT EXISTS Historial_Prompts (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Consulta        TEXT    NOT NULL,
                FechaConsulta   INTEGER NOT NULL,
                ModeloIA        TEXT,
                Estado          TEXT,
                TokenConsumidos INTEGER,
                DuracionMs      INTEGER
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Detalle_Prompts (
                Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                HistorialPromptId INTEGER NOT NULL,
                PromptCompleto    TEXT,
                Contexto          TEXT,
                Parametros        TEXT,
                FechaProceso      INTEGER NOT NULL,
                FOREIGN KEY (HistorialPromptId)
                    REFERENCES Historial_Prompts (Id) ON DELETE CASCADE
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Respuestas_IA (
                Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                HistorialPromptId INTEGER NOT NULL,
                DetallePromptId   INTEGER,
                RespuestaTexto    TEXT,
                Referencias       TEXT,
                Error             TEXT,
                FechaRespuesta    INTEGER NOT NULL,
                FOREIGN KEY (HistorialPromptId)
                    REFERENCES Historial_Prompts (Id) ON DELETE CASCADE,
                FOREIGN KEY (DetallePromptId)
                    REFERENCES Detalle_Prompts (Id) ON DELETE SET NULL
            );
            """,
            """
            CREATE INDEX IF NOT EXISTS IX_Historial_Prompts_FechaConsulta
                ON Historial_Prompts (FechaConsulta);
            """,
            """
            CREATE INDEX IF NOT EXISTS IX_Detalle_Prompts_HistorialPromptId
                ON Detalle_Prompts (HistorialPromptId);
            """,
            """
            CREATE INDEX IF NOT EXISTS IX_Respuestas_IA_HistorialPromptId
                ON Respuestas_IA (HistorialPromptId);
            """,
            """
            CREATE INDEX IF NOT EXISTS IX_Respuestas_IA_DetallePromptId
                ON Respuestas_IA (DetallePromptId);
            """,
        };

        private readonly DatabaseSettings _settings;
        private SQLiteAsyncConnection? _connection;

        public DatabaseService(AppConfigurationService configuration)
        {
            _settings = configuration.Settings.Database;
        }

        /// <summary>Ruta completa del archivo de la base de datos en el almacenamiento local de la app.</summary>
        public string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, _settings.DatabaseFileName);

        /// <summary>Conexion activa. Solo disponible despues de llamar a <see cref="InitializeAsync"/>.</summary>
        public SQLiteAsyncConnection Connection =>
            _connection ?? throw new InvalidOperationException(
                "La base de datos no ha sido inicializada. Llama a InitializeAsync() antes de usarla.");

        /// <summary>
        /// Abre la conexion, aplica la configuracion (borrado en desarrollo, claves foraneas)
        /// y crea el esquema. Es idempotente y seguro de llamar varias veces.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_connection is not null)
                return;

            if (_settings.DeleteOnStartup && File.Exists(DatabasePath))
                File.Delete(DatabasePath);

            _connection = new SQLiteAsyncConnection(DatabasePath);

            if (_settings.EnforceForeignKeys)
                await _connection.ExecuteAsync(EnableForeignKeysSql).ConfigureAwait(false);

            foreach (var statement in SchemaStatements)
                await _connection.ExecuteAsync(statement).ConfigureAwait(false);
        }

        // -----------------------------------------------------------------
        //  Historial_Prompts
        // -----------------------------------------------------------------

        public Task<List<HistorialPrompt>> GetHistorialesAsync() =>
            Connection.Table<HistorialPrompt>()
                .OrderByDescending(h => h.FechaConsulta)
                .ToListAsync();

        public Task<HistorialPrompt?> GetHistorialAsync(int id) =>
            Connection.Table<HistorialPrompt>().Where(h => h.Id == id).FirstOrDefaultAsync()!;

        public Task InsertHistorialAsync(HistorialPrompt historial) =>
            Connection.InsertAsync(historial);

        public Task UpdateHistorialAsync(HistorialPrompt historial) =>
            Connection.UpdateAsync(historial);

        public Task DeleteHistorialAsync(HistorialPrompt historial) =>
            Connection.DeleteAsync(historial);

        // -----------------------------------------------------------------
        //  Detalle_Prompts
        // -----------------------------------------------------------------

        public Task<List<DetallePrompt>> GetDetallesByHistorialAsync(int historialId) =>
            Connection.Table<DetallePrompt>()
                .Where(d => d.HistorialPromptId == historialId)
                .OrderByDescending(d => d.FechaProceso)
                .ToListAsync();

        public Task<DetallePrompt?> GetDetalleAsync(int id) =>
            Connection.Table<DetallePrompt>().Where(d => d.Id == id).FirstOrDefaultAsync()!;

        public Task InsertDetalleAsync(DetallePrompt detalle) =>
            Connection.InsertAsync(detalle);

        public Task UpdateDetalleAsync(DetallePrompt detalle) =>
            Connection.UpdateAsync(detalle);

        public Task DeleteDetalleAsync(DetallePrompt detalle) =>
            Connection.DeleteAsync(detalle);

        // -----------------------------------------------------------------
        //  Respuestas_IA
        // -----------------------------------------------------------------

        public Task<List<RespuestaIA>> GetRespuestasByHistorialAsync(int historialId) =>
            Connection.Table<RespuestaIA>()
                .Where(r => r.HistorialPromptId == historialId)
                .OrderByDescending(r => r.FechaRespuesta)
                .ToListAsync();

        public Task<RespuestaIA?> GetRespuestaAsync(int id) =>
            Connection.Table<RespuestaIA>().Where(r => r.Id == id).FirstOrDefaultAsync()!;

        public Task InsertRespuestaAsync(RespuestaIA respuesta) =>
            Connection.InsertAsync(respuesta);

        public Task UpdateRespuestaAsync(RespuestaIA respuesta) =>
            Connection.UpdateAsync(respuesta);

        public Task DeleteRespuestaAsync(RespuestaIA respuesta) =>
            Connection.DeleteAsync(respuesta);

        // -----------------------------------------------------------------
        //  Todas las tablas
        // -----------------------------------------------------------------

        /// <summary>
        /// Elimina todos los registros de <c>Respuestas_IA</c>, <c>Detalle_Prompts</c> e
        /// <c>Historial_Prompts</c> en una sola transaccion (primero las tablas hijas
        /// para respetar las claves foraneas).
        /// </summary>
        public Task DeleteAllAsync() =>
            Connection.RunInTransactionAsync(conn =>
            {
                conn.DeleteAll<RespuestaIA>();
                conn.DeleteAll<DetallePrompt>();
                conn.DeleteAll<HistorialPrompt>();
            });
    }
}