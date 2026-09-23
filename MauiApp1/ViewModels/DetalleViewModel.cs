using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana <c>Detalle</c>: muestra el prompt enviado a la IA,
    /// la respuesta, la fecha y el estado de una consulta seleccionada del historial.
    /// </summary>
    public partial class DetalleViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        public partial int ConsultaId { get; set; }

        [ObservableProperty]
        public partial string? Consulta { get; set; }

        [ObservableProperty]
        public partial string? PromptEnviado { get; set; }

        [ObservableProperty]
        public partial string? Respuesta { get; set; }

        [ObservableProperty]
        public partial string? Fecha { get; set; }

        [ObservableProperty]
        public partial string? Estado { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneError))]
        public partial string? Error { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NoTieneDatos))]
        public partial bool TieneDatos { get; set; }

        /// <summary>Indica si hay un mensaje de error que mostrar.</summary>
        public bool TieneError => !string.IsNullOrWhiteSpace(Error);

        /// <summary>Indica el estado vacio (sin registro seleccionado).</summary>
        public bool NoTieneDatos => !TieneDatos;

        public DetalleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        private async Task VolverAsync() =>
            await Shell.Current.GoToAsync("..");

        /// <summary>
        /// Carga el detalle de la consulta indicada. Si <paramref name="id"/> es invalido,
        /// muestra el estado vacio (peticion directa sin registro seleccionado).
        /// </summary>
        public async Task CargarAsync(int id)
        {
            ConsultaId = id;
            TieneDatos = false;
            Consulta = null;
            PromptEnviado = null;
            Respuesta = null;
            Fecha = null;
            Estado = null;
            Error = null;

            if (id <= 0)
                return;

            try
            {
                await CargarDesdeBaseDeDatosAsync(id);
            }
            catch (Exception ex)
            {
                // La pagina llama a este metodo sin await: sin este catch el error se perderia.
                Error = $"No se pudo cargar el detalle de la consulta: {ex.Message}";
                TieneDatos = true;
            }
        }

        private async Task CargarDesdeBaseDeDatosAsync(int id)
        {
            await _databaseService.InitializeAsync();

            var historial = await _databaseService.GetHistorialAsync(id);
            if (historial is null)
                return;

            var detalle = (await _databaseService.GetDetallesByHistorialAsync(id)).FirstOrDefault();
            var respuestas = await _databaseService.GetRespuestasByHistorialAsync(id);
            var respuesta = respuestas.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RespuestaTexto))
                            ?? respuestas.FirstOrDefault();

            Consulta = historial.Consulta;
            PromptEnviado = detalle?.PromptCompleto;
            Respuesta = respuesta?.RespuestaTexto;
            Error = respuesta?.Error;
            Fecha = historial.FechaConsulta.ToString("dd/MM/yyyy HH:mm");
            Estado = EsRespondida(respuesta)
                ? "Respondido con exito"
                : "Sin respuesta";
            TieneDatos = true;
        }

        private static bool EsRespondida(RespuestaIA? respuesta) =>
            respuesta is { RespuestaTexto: not null } && !string.IsNullOrWhiteSpace(respuesta.RespuestaTexto);
    }
}