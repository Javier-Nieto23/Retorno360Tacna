using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Globalization;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.MODELS
{
    public class ReporteRetornoPDF
    {
        private readonly ResultadoRetorno _resultado;
        private SKBitmap? _graficaLineal;
        private SKBitmap? _graficaCircular;

        public ReporteRetornoPDF(ResultadoRetorno resultado)
        {
            _resultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
            ConfigurarLicencia();
        }

        public ReporteRetornoPDF(ResultadoRetorno resultado, SKBitmap? graficaLineal, SKBitmap? graficaCircular)
        {
            _resultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
            _graficaLineal = graficaLineal;
            _graficaCircular = graficaCircular;
            ConfigurarLicencia();
        }

        private void ConfigurarLicencia()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GenerarPDF(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo no puede estar vacía.", nameof(rutaArchivo));

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        private void ConfigurarPagina(PageDescriptor page)
        {
            page.Size(PageSizes.Letter);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Reporte de Porcentaje de Retorno")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        col.Item().PaddingTop(5).Text(txt =>
                        {
                            txt.Span("Razón Social: ").SemiBold().FontSize(11);
                            txt.Span(_resultado.RazonSocial).FontSize(11);
                        });

                        col.Item().PaddingTop(2).Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                    });
                });

                column.Item().PaddingTop(10).BorderBottom(2).BorderColor(Colors.Blue.Darken2);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(15).Column(column =>
            {
                column.Spacing(12);

                // Información general
                column.Item().Element(ComposeInformacionGeneral);

                // Resultados financieros
                column.Item().Element(ComposeResultados);

                // Gráficas
                if (_graficaLineal != null || _graficaCircular != null)
                {
                    column.Item().Element(ComposeGraficas);
                }

                // Pedimentos
                column.Item().Element(ComposePedimentos);

                // Resumen ejecutivo
                column.Item().Element(ComposeResumen);
            });
        }

        private void ComposeInformacionGeneral(IContainer container)
        {
            container.Background(Colors.Grey.Lighten4)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Text("Información General")
                        .FontSize(13)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    AgregarCampoInfo(column, "Base(s) de Datos", _resultado.BaseDatos);
                    AgregarCampoInfo(column, "Período", $"{_resultado.FechaInicio:dd/MM/yyyy} - {_resultado.FechaFin:dd/MM/yyyy}");
                    AgregarCampoInfo(column, "Materia Prima", _resultado.IncluyeMateriaPrima ? "Incluida" : "No Incluida");
                });
        }

        private void AgregarCampoInfo(ColumnDescriptor column, string label, string valor)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(txt =>
                {
                    txt.Span($"{label}: ").SemiBold().FontSize(9);
                    txt.Span(valor).FontSize(9);
                });
            });
        }

        private void ComposeResultados(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(12).Column(column =>
            {
                column.Spacing(8);

                column.Item().Text("Resultados Financieros")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2)
                            .Padding(6).Text("Concepto").FontColor(Colors.White).SemiBold().FontSize(10);

                        header.Cell().Background(Colors.Blue.Darken2)
                            .Padding(6).Text("Valor").FontColor(Colors.White).SemiBold().FontSize(10);
                    });

                    // Filas
                    AgregarFilaResultado(table, "Valor Comercial Importado", 
                        $"${_resultado.ValorImportado:N2}", Colors.Grey.Lighten4, Colors.Red.Darken1);
                    
                    AgregarFilaResultado(table, "Valor Comercial Exportado", 
                        $"${_resultado.ValorExportado:N2}", Colors.White, Colors.Blue.Medium);
                    
                    AgregarFilaResultado(table, "Porcentaje de Retorno", 
                        $"{_resultado.PorcentajeRetorno:N2}%", Colors.Green.Lighten4, Colors.Green.Darken2, true);
                });
            });
        }

        private void AgregarFilaResultado(TableDescriptor table, string concepto, string valor, 
            string colorFondo, string colorTexto, bool destacado = false)
        {
            table.Cell().Background(colorFondo)
                .Padding(6).Text(concepto).FontSize(9);
            
            table.Cell().Background(colorFondo)
                .Padding(6).Text(valor)
                .FontColor(colorTexto)
                .SemiBold()
                .FontSize(destacado ? 12 : 9);
        }

        private void ComposeGraficas(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item().Text("Representación Gráfica")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().Row(row =>
                {
                    row.Spacing(10);

                    // Gráfica lineal
                    if (_graficaLineal != null)
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Comparación de Valores")
                                .FontSize(10)
                                .SemiBold()
                                .AlignCenter();

                            col.Item().Border(1).BorderColor(Colors.Grey.Medium)
                                .Padding(5)
                                .Image(ConvertirSKBitmapABytes(_graficaLineal))
                                .FitArea();
                        });
                    }

                    // Gráfica circular
                    if (_graficaCircular != null)
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Distribución Porcentual")
                                .FontSize(10)
                                .SemiBold()
                                .AlignCenter();

                            col.Item().Border(1).BorderColor(Colors.Grey.Medium)
                                .Padding(5)
                                .Image(ConvertirSKBitmapABytes(_graficaCircular))
                                .FitArea();
                        });
                    }
                });
            });
        }

        private void ComposePedimentos(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(12).Column(column =>
            {
                column.Spacing(8);

                column.Item().Text("Detalle de Pedimentos")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().Row(row =>
                {
                    row.Spacing(10);

                    AgregarCardPedimento(row, "Importación", 
                        _resultado.CantidadPedimentosImportacion, Colors.Red.Darken1);
                    
                    AgregarCardPedimento(row, "Exportación", 
                        _resultado.CantidadPedimentosExportacion, Colors.Blue.Medium);
                    
                    AgregarCardPedimento(row, "Total", 
                        _resultado.TotalPedimentosValidados, Colors.Green.Darken2);
                });
            });
        }

        private void AgregarCardPedimento(RowDescriptor row, string titulo, int cantidad, string color)
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(titulo)
                    .FontSize(9)
                    .SemiBold();
                
                col.Item().Text(cantidad.ToString())
                    .FontSize(20)
                    .Bold()
                    .FontColor(color);
            });
        }

        private void ComposeResumen(IContainer container)
        {
            container.Background(Colors.Blue.Lighten5).Padding(12).Column(column =>
            {
                column.Spacing(6);

                column.Item().Text("Resumen Ejecutivo")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken3);

                var (estado, colorEstado) = ObtenerEstadoRetorno();
                decimal diferencia = _resultado.ValorExportado - _resultado.ValorImportado;

                // Estado
                column.Item().Text(txt =>
                {
                    txt.Span("Estado del Retorno: ").FontSize(10);
                    txt.Span(estado).SemiBold().FontSize(11).FontColor(colorEstado);
                });

                // Diferencia
                column.Item().Text(txt =>
                {
                    txt.Span("Diferencia: ").FontSize(10);
                    txt.Span($"${Math.Abs(diferencia):N2}").SemiBold()
                        .FontColor(diferencia >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken1);
                    txt.Span(diferencia >= 0 ? " (Superávit)" : " (Déficit)").FontSize(9);
                });

                // Porcentaje faltante
                if (_resultado.PorcentajeRetorno < 100)
                {
                    decimal faltante = 100 - _resultado.PorcentajeRetorno;
                    column.Item().Text(txt =>
                    {
                        txt.Span("Porcentaje Faltante: ").FontSize(10);
                        txt.Span($"{faltante:N2}%").SemiBold().FontColor(Colors.Orange.Darken1);
                    });
                }

                // Fecha de cálculo
                column.Item().PaddingTop(5).Text($"Fecha de Cálculo: {_resultado.FechaCalculo:dd/MM/yyyy HH:mm:ss}")
                    .FontSize(8)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }

        private (string estado, string color) ObtenerEstadoRetorno()
        {
            return _resultado.PorcentajeRetorno switch
            {
                >= 100 => ("COMPLETO", Colors.Green.Darken2),
                >= 80 => ("BUEN AVANCE", Colors.Green.Medium),
                >= 50 => ("AVANCE MODERADO", Colors.Orange.Darken1),
                > 0 => ("BAJO", Colors.Red.Medium),
                _ => ("SIN RETORNO", Colors.Red.Darken1)
            };
        }

        private byte[] ConvertirSKBitmapABytes(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public static string GenerarNombreArchivo(ResultadoRetorno resultado)
        {
            string razonLimpia = resultado.RazonSocial;
            razonLimpia = razonLimpia.Replace(" ", "_");
            razonLimpia = razonLimpia.Replace("/", "_");
            razonLimpia = razonLimpia.Replace("\\", "_");

            return $"Reporte_Retorno_{razonLimpia}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        }
    }
}
