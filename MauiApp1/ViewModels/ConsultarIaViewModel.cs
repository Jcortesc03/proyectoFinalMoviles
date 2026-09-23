using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Configuration;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana <c>Consultar IA</c>: una barra de busqueda extendible
    /// (maximo 500 caracteres) y un cuadro con la respuesta en texto plano de la IA.
    /// Registra la consulta, su detalle y la respuesta en la base de datos.
    /// </summary>
    public partial class ConsultarIaViewModel : ObservableObject
    {
        private readonly AiApiService _aiApiService;
        private readonly DatabaseService _databaseService;
        private readonly AppConfigurationService _configuration;

        /// <summary>Cantidad maxima de caracteres permitida para la consulta.</summary>
        public int MaxLongitudPrompt => 500;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContadorCaracteres))]
        public partial string Prompt { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneRespuesta))]
        [NotifyPropertyChangedFor(nameof(MuestraAvisoSinRespuesta))]
        [NotifyPropertyChangedFor(nameof(PuedeConsultar))]
        public partial string? Respuesta { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneError))]
        [NotifyPropertyChangedFor(nameof(MuestraAvisoSinRespuesta))]
        public partial string? MensajeError { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PuedeConsultar))]
        [NotifyPropertyChangedFor(nameof(MuestraAvisoSinRespuesta))]
        public partial bool EstaConsultando { get; set; }

        /// <summary>Indica si el usuario puede lanzar una nueva consulta.</summary>
        public bool PuedeConsultar => !EstaConsultando;

        /// <summary>Indica si hay una respuesta mostrada.</summary>
        public bool TieneRespuesta => !string.IsNullOrWhiteSpace(Respuesta);

        /// <summary>Indica si hay un mensaje de error mostrado.</summary>
        public bool TieneError => !string.IsNullOrWhiteSpace(MensajeError);

        /// <summary>Indica si se debe mostrar el aviso de "aun no hay respuesta".</summary>
        public bool MuestraAvisoSinRespuesta =>
            !EstaConsultando && !TieneRespuesta && !TieneError;

        /// <summary>Contador de caracteres de la barra de busqueda (ej. "120 / 500").</summary>
        public string ContadorCaracteres => $"{Prompt.Length} / {MaxLongitudPrompt}";

        public ConsultarIaViewModel(
            AiApiService aiApiService,
            DatabaseService databaseService,
            AppConfigurationService configuration)
        {
            _aiApiService = aiApiService;
            _databaseService = databaseService;
            _configuration = configuration;
        }

        [RelayCommand]
        private async Task ConsultarAsync()
        {
            var texto = Prompt.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                MensajeError = "Escribe una consulta antes de preguntar.";
                return;
            }

            if (texto.Length > MaxLongitudPrompt)
                texto = texto[..MaxLongitudPrompt];

            EstaConsultando = true;
            Respuesta = null;
            MensajeError = null;

            var historial = new HistorialPrompt
            {
                Consulta = texto,
                ModeloIA = _configuration.Settings.AiApi.DefaultModel,
                Estado = "Procesado",
                FechaConsulta = DateTime.Now,
            };
            var detalle = new DetallePrompt { PromptCompleto = texto };
            RespuestaIA? respuesta = null;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var textoRespuesta = await _aiApiService.AskAsync(texto);
                stopwatch.Stop();

                Respuesta = textoRespuesta;
                historial.Estado = "Procesado";
                historial.DuracionMs = stopwatch.ElapsedMilliseconds;
                respuesta = new RespuestaIA { RespuestaTexto = textoRespuesta };
            }
            catch (Exception ex)
            {
                historial.Estado = "Error";
                MensajeError = $"No se pudo obtener respuesta de la IA: {ex.Message}";
                respuesta = new RespuestaIA { Error = ex.Message };
            }
            finally
            {
                EstaConsultando = false;
            }

            await PersistirAsync(historial, detalle, respuesta);
        }

        private async Task PersistirAsync(
            HistorialPrompt historial,
            DetallePrompt detalle,
            RespuestaIA? respuesta)
        {
            try
            {
                await _databaseService.InitializeAsync();
                await _databaseService.InsertHistorialAsync(historial);

                detalle.HistorialPromptId = historial.Id;
                await _databaseService.InsertDetalleAsync(detalle);

                if (respuesta is not null)
                {
                    respuesta.HistorialPromptId = historial.Id;
                    respuesta.DetallePromptId = detalle.Id;
                    await _databaseService.InsertRespuestaAsync(respuesta);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"No se pudo guardar la consulta en la base de datos: {ex.Message}";
            }
        }
    }
}