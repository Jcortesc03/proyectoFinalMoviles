using MauiApp1.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ColorPalette = QuestPDF.Helpers.Colors;

namespace MauiApp1.Services
{
    /// <summary>
    /// Genera documentos PDF (QuestPDF) con la informacion de las consultas,
    /// tanto el resumen del historial como el detalle de una consulta.
    /// </summary>
    public sealed class PdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>Genera un PDF con la lista de consultas del historial.</summary>
        public byte[] GenerarHistorialPdf(IReadOnlyList<ConsultaDto> consultas, string? desde = null, string? hasta = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(ColorPalette.Grey.Darken3));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Historial de consultas")
                            .FontSize(18).Bold().FontColor(ColorPalette.Blue.Darken3);
                        header.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        if (!string.IsNullOrWhiteSpace(desde) || !string.IsNullOrWhiteSpace(hasta))
                            header.Item().Text($"Filtro: {desde} - {hasta}").FontSize(10);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Fecha").Bold();
                            header.Cell().Element(CellStyle).Text("Consulta").Bold();
                            header.Cell().Element(CellStyle).Text("Estado").Bold();
                        });

                        foreach (var consulta in consultas)
                        {
                            table.Cell().Element(CellStyle).Text(consulta.Fecha.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Element(CellStyle).Text(consulta.Consulta).BreakAnywhere();
                            table.Cell().Element(CellStyle).Text(consulta.Estado);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(9).FontColor(ColorPalette.Grey.Medium));
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });

                    static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container) =>
                        container.BorderBottom(1).BorderColor(ColorPalette.Grey.Lighten2).PaddingVertical(6);
                });
            }).GeneratePdf();
        }

        /// <summary>Genera un PDF con el detalle completo de una consulta.</summary>
        public byte[] GenerarDetallePdf(ConsultaDto consulta)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(ColorPalette.Grey.Darken3));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Detalle de la consulta")
                            .FontSize(18).Bold().FontColor(ColorPalette.Blue.Darken3);
                        header.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Text($"Fecha: {consulta.Fecha:dd/MM/yyyy HH:mm}").SemiBold();
                        column.Item().Text($"Estado: {consulta.Estado}").SemiBold();

                        column.Item().Text("Consulta")
                            .FontSize(13).Bold().FontColor(ColorPalette.Blue.Darken3);
                        column.Item().Text(consulta.Consulta).BreakAnywhere();

                        if (!string.IsNullOrWhiteSpace(consulta.PromptEnviado))
                        {
                            column.Item().Text("Prompt enviado a la IA")
                                .FontSize(13).Bold().FontColor(ColorPalette.Blue.Darken3);
                            column.Item().Text(consulta.PromptEnviado).BreakAnywhere();
                        }

                        column.Item().Text("Respuesta de la IA")
                            .FontSize(13).Bold().FontColor(ColorPalette.Blue.Darken3);
                        column.Item().Text(string.IsNullOrWhiteSpace(consulta.RespuestaTexto)
                                ? "(Sin respuesta)"
                                : consulta.RespuestaTexto)
                            .BreakAnywhere();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(9).FontColor(ColorPalette.Grey.Medium));
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}