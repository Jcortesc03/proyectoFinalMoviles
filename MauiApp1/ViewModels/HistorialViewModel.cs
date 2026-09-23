using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Storage;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana <c>Consultar historial</c>: lista las consultas guardadas,
    /// permite filtrarlas por fecha, descargarlas en PDF o eliminarlas de la base de datos.
    /// </summary>
    public partial class HistorialViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly PdfExportService _pdfExportService;

        private IReadOnlyList<ConsultaDto> _todas = Array.Empty<ConsultaDto>();

        /// <summary>Consultas visibles (aplicando el filtro actual).</summary>
        public ObservableCollection<ConsultaDto> Consultas { get; } = new();

        [ObservableProperty]
        public partial DateTime FechaDesde { get; set; } = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        public partial DateTime FechaHasta { get; set; } = DateTime.Today;

        [ObservableProperty]
        public partial bool Filtrando { get; set; }

        [ObservableProperty]
        public partial bool EstaCargando { get; set; }

        /// <summary>Indica si hay consultas desplegadas.</summary>
        public bool TieneConsultas => Consultas.Count > 0;

        public HistorialViewModel(DatabaseService databaseService, PdfExportService pdfExportService)
        {
            _databaseService = databaseService;
            _pdfExportService = pdfExportService;
        }

        /// <summary>Carga todas las consultas desde la base de datos y aplica el filtro por fecha.</summary>
        public async Task CargarAsync()
        {
            EstaCargando = true;
            try
            {
                await _databaseService.InitializeAsync();
                var historiales = await _databaseService.GetHistorialesAsync();

                var lista = new List<ConsultaDto>(historiales.Count);
                foreach (var historial in historiales)
                {
                    var detalles = await _databaseService.GetDetallesByHistorialAsync(historial.Id);
                    var respuestas = await _databaseService.GetRespuestasByHistorialAsync(historial.Id);
                    var respuesta = respuestas.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RespuestaTexto))
                                    ?? respuestas.FirstOrDefault();

                    lista.Add(new ConsultaDto
                    {
                        Id = historial.Id,
                        Consulta = historial.Consulta,
                        Fecha = historial.FechaConsulta,
                        PromptEnviado = detalles.FirstOrDefault()?.PromptCompleto,
                        RespuestaTexto = respuesta?.RespuestaTexto,
                        Error = respuesta?.Error,
                        Estado = EsRespondida(respuesta)
                            ? "Respondido con exito"
                            : "Sin respuesta",
                    });
                }

                _todas = lista;
                AplicarFiltro();
            }
            finally
            {
                EstaCargando = false;
            }
        }

        /// <summary>Filtra las consultas por el rango de fechas seleccionado.</summary>
        private void AplicarFiltro()
        {
            var desde = FechaDesde.Date;
            var hasta = FechaHasta.Date.AddDays(1).AddTicks(-1);

            Aplicar(_todas.Where(c => c.Fecha >= desde && c.Fecha <= hasta).ToList());
            Filtrando = true;
        }

        private static bool EsRespondida(RespuestaIA? respuesta) =>
            respuesta is { RespuestaTexto: not null } && !string.IsNullOrWhiteSpace(respuesta.RespuestaTexto);

        private void Aplicar(IReadOnlyCollection<ConsultaDto> consultas)
        {
            Consultas.Clear();
            foreach (var consulta in consultas)
                Consultas.Add(consulta);

            OnPropertyChanged(nameof(TieneConsultas));
        }

        [RelayCommand]
        private async Task CargarConsultaAsync()
        {
            await CargarAsync();
        }

        [RelayCommand]
        private void Filtrar()
        {
            AplicarFiltro();
        }

        [RelayCommand]
        private void LimpiarFiltro()
        {
            Aplicar(_todas);
            Filtrando = false;
        }

        [RelayCommand]
        private async Task VerDetalleAsync(ConsultaDto consulta) =>
            await Shell.Current.GoToAsync($"DetallePage?id={consulta.Id}");

        [RelayCommand]
        private async Task EliminarAsync(ConsultaDto consulta)
        {
            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Eliminar consulta",
                $"¿Eliminar la consulta \"{consulta.Consulta}\"? Se borrara junto con su detalle y respuesta.",
                "Eliminar",
                "Cancelar");

            if (!confirmar)
                return;

            var historial = await _databaseService.GetHistorialAsync(consulta.Id);
            if (historial is not null)
            {
                await _databaseService.DeleteHistorialAsync(historial);
                await CargarAsync();
            }
        }

        [RelayCommand]
        private async Task DescargarPdfAsync(ConsultaDto consulta)
        {
            var bytes = _pdfExportService.GenerarDetallePdf(consulta);
            using var stream = new MemoryStream(bytes);

            var result = await FileSaver.Default.SaveAsync(
                $"consulta_{consulta.Id}.pdf",
                stream,
                CancellationToken.None);

            if (result.IsSuccessful)
                await Shell.Current.DisplayAlertAsync(
                    "PDF guardado",
                    $"El archivo se guardo en:\n{result.FilePath}",
                    "OK");
            else
                await Shell.Current.DisplayAlertAsync(
                    "PDF no guardado",
                    "No se pudo guardar el archivo PDF.",
                    "OK");
        }

        [RelayCommand]
        private async Task DescargarHistorialPdfAsync()
        {
            if (!Consultas.Any())
                return;

            var bytes = _pdfExportService.GenerarHistorialPdf(Consultas.ToList(), Filtrando ? FechaDesde.ToString("dd/MM/yyyy") : null, Filtrando ? FechaHasta.ToString("dd/MM/yyyy") : null);
            using var stream = new MemoryStream(bytes);

            var result = await FileSaver.Default.SaveAsync(
                $"historial_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                stream,
                CancellationToken.None);

            if (result.IsSuccessful)
                await Shell.Current.DisplayAlertAsync("PDF guardado", $"El archivo se guardo en:\n{result.FilePath}", "OK");
            else
                await Shell.Current.DisplayAlertAsync("PDF no guardado", "No se pudo guardar el archivo PDF.", "OK");
        }
    }
}