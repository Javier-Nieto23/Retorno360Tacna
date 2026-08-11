using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Retorno360Tacna.MODELS;
using SkiaSharp;

namespace Retorno360Tacna.SERVICES
{
    public class PdfGeneradorService
    {
        public PdfGeneradorService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GenerarReportePDF(ResultadoRetorno resultado, string rutaArchivo)
        {
            byte[] imagenBarras = GenerarGraficoBarras(resultado);
            byte[] imagenPie = GenerarGraficoPie(resultado);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Reporte de Porcentaje de Retorno")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Razón Social: ").Bold();
                                    txt.Span(resultado.RazonSocial);
                                });
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Fecha de generación: ").FontSize(9);
                                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(9);
                                });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(10)

                        .Column(column =>
                        {
                            // PRIMERA PÁGINA - Información General
                            column.Item().PaddingBottom(10).Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("Información General")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                if (!string.IsNullOrEmpty(resultado.BaseDatos))
                                {
                                    col.Item().PaddingTop(5).Text(txt =>
                                    {
                                        txt.Span("Base(s) de Datos: ").Bold();
                                        txt.Span(resultado.BaseDatos);
                                    });
                                }

                                col.Item().Text(txt =>
                                {
                                    txt.Span("Período: ").Bold();
                                    txt.Span($"{resultado.FechaInicio:dd/MM/yyyy} - {resultado.FechaFin:dd/MM/yyyy}");
                                });

                                col.Item().Text(txt =>
                                {
                                    txt.Span("Materia Prima: ").Bold();
                                    txt.Span("No Incluida");
                                });
                            });

                            // Resultados Financieros
                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Resultados Financieros")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8)
                                        .Text("Concepto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8).AlignRight()
                                        .Text("Valor").FontColor(Colors.White).Bold();
                                });

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Valor Comercial Importado");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resultado.ValorImportado:N2}")
                                    .FontColor(Colors.Red.Darken1).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Valor Comercial Exportado");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resultado.ValorExportado:N2}")
                                    .FontColor(Colors.Blue.Darken1).Bold();

                                table.Cell().Background(Colors.Green.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(8).Text("Porcentaje de Retorno").Bold();
                                table.Cell().Background(Colors.Green.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(8).AlignRight()
                                    .Text($"{resultado.PorcentajeRetorno:N2}%")
                                    .FontSize(14)
                                    .FontColor(Colors.Green.Darken2).Bold();
                            });

                            // Tabla de Cantidad de Pedimentos
                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Detalle de Pedimentos")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .Text("Importación").FontSize(11).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .Text("Exportación").FontSize(11).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .Text("Total").FontSize(11).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .AlignCenter()
                                    .Text(resultado.CantidadPedimentosImportacion.ToString())
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Red.Darken1);

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .AlignCenter()
                                    .Text(resultado.CantidadPedimentosExportacion.ToString())
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken1);

                                table.Cell().Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                                    .AlignCenter()
                                    .Text(resultado.TotalPedimentosValidados.ToString())
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken1);
                            });

                            // Gráficos - Apilados verticalmente
                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Representación Gráfica")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            // Gráfico de Barras (más grande)
                            column.Item().PaddingBottom(15).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                            {
                                col.Item().AlignCenter().Text("Comparación de Valores Comerciales")
                                    .FontSize(12)
                                    .Bold();
                                col.Item().PaddingTop(5).Image(imagenBarras).FitWidth();
                            });

                            // SEGUNDA PÁGINA - Gráfico Circular y Resumen
                            column.Item().PageBreak();

                            // Gráfico Circular (más grande)
                            column.Item().PaddingBottom(15).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                            {
                                col.Item().AlignCenter().Text("Distribución Porcentual")
                                    .FontSize(12)
                                    .Bold();
                                col.Item().PaddingTop(5).Image(imagenPie).FitWidth();
                            });

                            column.Item().PaddingTop(10).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                            {
                                col.Item().Text("Resumen Ejecutivo")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(8).Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Estado del Retorno: ").Bold();
                                        decimal porcentaje = resultado.PorcentajeRetorno;
                                        string estado = porcentaje >= 100 ? "BUEN AVANCE" : 
                                                       porcentaje >= 75 ? "EN PROCESO" : "REQUIERE ATENCIÓN";
                                        txt.Span(estado).FontColor(porcentaje >= 100 ? Colors.Green.Darken1 : 
                                                                   porcentaje >= 75 ? Colors.Orange.Darken1 : Colors.Red.Darken1);
                                    });
                                });

                                decimal diferencia = resultado.ValorImportado - resultado.ValorExportado;
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Diferencia: ").Bold();
                                        txt.Span($"${Math.Abs(diferencia):N2} ({(diferencia > 0 ? "Déficit" : "Superávit")})")
                                           .FontColor(diferencia > 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);
                                    });
                                });

                                decimal porcentajeFaltante = 100 - resultado.PorcentajeRetorno;
                                if (porcentajeFaltante > 0)
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text(txt =>
                                        {
                                            txt.Span("Porcentaje Faltante: ").Bold();
                                            txt.Span($"{porcentajeFaltante:N2}%").FontColor(Colors.Orange.Darken2);
                                        });
                                    });
                                }

                                col.Item().PaddingTop(5).Text(txt =>
                                {
                                    txt.Span("Fecha de Cálculo: ").FontSize(8);
                                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(8);
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(9))
                        .Text(txt =>
                        {
                            txt.Span("Página ");
                            txt.CurrentPageNumber();
                            txt.Span(" de ");
                            txt.TotalPages();
                        });
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        private byte[] GenerarGraficoBarras(ResultadoRetorno resultado)
        {
            int width = 600;
            int height = 350;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            SKColor colorImportacion = new SKColor(220, 53, 69);
            SKColor colorExportacion = new SKColor(30, 136, 229);

            decimal maxValor = Math.Max(resultado.ValorImportado, resultado.ValorExportado);
            if (maxValor == 0) maxValor = 1;

            float barWidth = 120;
            float spacing = 80;
            float maxBarHeight = height - 100;
            float baseY = height - 60;

            float x1 = (width / 2) - barWidth - spacing / 2;
            float x2 = (width / 2) + spacing / 2;

            float altura1 = (float)(resultado.ValorImportado / maxValor) * maxBarHeight;
            float altura2 = (float)(resultado.ValorExportado / maxValor) * maxBarHeight;

            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = baseY - (maxBarHeight * i / 4);
                    canvas.DrawLine(40, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 4;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" : $"${valorEje / 1000:N0}K";
                    DrawTextBlob(canvas, label, 5, y + 4, 12, SKColors.Gray, SKTextAlign.Left);
                }
            }

            using (var paint1 = new SKPaint { Color = colorImportacion, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(x1, baseY - altura1, barWidth, altura1, paint1);
            }

            using (var paint2 = new SKPaint { Color = colorExportacion, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(x2, baseY - altura2, barWidth, altura2, paint2);
            }

            DrawTextBlob(canvas, "Valores Comerciales", width / 2, height - 10, 14, SKColors.Black, SKTextAlign.Center);

            DrawTextBlob(canvas, $"${resultado.ValorImportado:N0}", x1 + barWidth / 2, baseY - altura1 - 8, 13, colorImportacion, SKTextAlign.Center);
            DrawTextBlob(canvas, $"${resultado.ValorExportado:N0}", x2 + barWidth / 2, baseY - altura2 - 8, 13, colorExportacion, SKTextAlign.Center);

            {
                float legendY = 20;
                using (var rectPaint = new SKPaint { Color = colorImportacion, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(width - 120, legendY - 8, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "Importación", width - 100, legendY, 12, SKColors.Black, SKTextAlign.Left);

                using (var rectPaint = new SKPaint { Color = colorExportacion, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(width - 120, legendY + 12, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "Exportación", width - 100, legendY + 20, 12, SKColors.Black, SKTextAlign.Left);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public static void DrawTextBlob(SKCanvas canvas, string text, float x, float y, float size, SKColor color, SKTextAlign align = SKTextAlign.Left)
        {
            using var paint = new SKPaint { Color = color, IsAntialias = true };
            using var font = new SKFont(SKTypeface.Default, size);
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            float width = font.MeasureText(bytes, SKTextEncoding.Utf8, out SKRect bounds);
            using var blob = SKTextBlob.Create(text, font);
            float drawX = x;
            if (align == SKTextAlign.Center) drawX = x - width / 2;
            else if (align == SKTextAlign.Right) drawX = x - width;
            canvas.DrawText(blob, drawX, y, paint);
        }

        private byte[] GenerarGraficoPie(ResultadoRetorno resultado)
        {
            int width = 600;
            int height = 400;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            float centerX = width / 2;
            float centerY = height / 2.2f;
            float radius = Math.Min(width, height) / 3.2f;

            decimal total = resultado.ValorImportado + resultado.ValorExportado;
            if (total == 0) total = 1;

            float importadoPorcentaje = (float)(resultado.ValorImportado / total);
            float exportadoPorcentaje = (float)(resultado.ValorExportado / total);

            SKColor colorImportacion = new SKColor(220, 53, 69);
            SKColor colorExportacion = new SKColor(30, 136, 229);

            float startAngle = -90;
            float sweepAngleImport = 360 * importadoPorcentaje;

            using (var paint = new SKPaint { Color = colorImportacion, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle, sweepAngleImport, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            float sweepAngleExport = 360 * exportadoPorcentaje;
            using (var paint = new SKPaint { Color = colorExportacion, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle + sweepAngleImport, sweepAngleExport, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            {
                float angle1 = (startAngle + sweepAngleImport / 2) * (float)Math.PI / 180;
                float labelX1 = centerX + (radius * 0.6f) * (float)Math.Cos(angle1);
                float labelY1 = centerY + (radius * 0.6f) * (float)Math.Sin(angle1);
                DrawTextBlob(canvas, $"${resultado.ValorImportado:N0}", labelX1, labelY1, 14, SKColors.White, SKTextAlign.Center);

                float angle2 = (startAngle + sweepAngleImport + sweepAngleExport / 2) * (float)Math.PI / 180;
                float labelX2 = centerX + (radius * 0.6f) * (float)Math.Cos(angle2);
                float labelY2 = centerY + (radius * 0.6f) * (float)Math.Sin(angle2);
                DrawTextBlob(canvas, $"${resultado.ValorExportado:N0}", labelX2, labelY2, 14, SKColors.White, SKTextAlign.Center);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public void GenerarReporteIGIPDF(List<ReporteIGIPagado> reporteIGI, ResumenIGI resumen, string razonSocial, string baseDatos, DateTime fechaInicio, DateTime fechaFin, string rutaArchivo)
        {
            byte[] imagenBarras = GenerarGraficoBarrasIGI(resumen);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Reporte de IGI Pagado vs Calculado")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Razón Social: ").Bold();
                                    txt.Span(razonSocial);
                                });
                            });

                            if (!string.IsNullOrEmpty(baseDatos))
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Base(s) de Datos: ").Bold();
                                        txt.Span(baseDatos);
                                    });
                                });
                            }

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Fecha de generación: ").FontSize(9);
                                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(9);
                                });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("Información General")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().Text(txt =>
                                {
                                    txt.Span("Período: ").Bold();
                                    txt.Span($"{fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}");
                                });
                            });

                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Resumen Financiero")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8)
                                        .Text("Concepto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8).AlignRight()
                                        .Text("Valor").FontColor(Colors.White).Bold();
                                });

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Total Pedimentos");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text(resumen.TotalPedimentos.ToString())
                                    .FontColor(Colors.Blue.Darken1).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("IGI Pagado");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIGI_Pagado:N2}")
                                    .FontColor(new SKColor(52, 152, 219).ToQuestColor()).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("IGI Calculado");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIGI_Calculado:N2}")
                                    .FontColor(new SKColor(46, 204, 113).ToQuestColor()).Bold();

                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(8).Text("Diferencia IGI").Bold();
                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(8).AlignRight()
                                    .Text($"${resumen.DiferenciaTotal:N2}")
                                    .FontSize(14)
                                    .FontColor(resumen.DiferenciaTotal > 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).Bold();

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text("IVA Pagado");
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIVA_Pagado:N2}")
                                    .FontColor(Colors.Purple.Darken1).Bold();
                            });

                            column.Item().PaddingTop(20).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Representación Gráfica")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().AlignCenter().Text("IGI Pagado vs Calculado")
                                        .FontSize(11)
                                        .Bold();
                                    col.Item().PaddingTop(5).Image(imagenBarras).FitArea();
                                });
                            });

                            if (reporteIGI.Any())
                            {
                                column.Item().PageBreak();

                                column.Item().PaddingBottom(10).Text("Detalle de Pedimentos")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken1).Padding(5)
                                            .Text("Pedimento").FontColor(Colors.White).Bold().FontSize(9);
                                        header.Cell().Background(Colors.Blue.Darken1).Padding(5)
                                            .Text("Fecha Pago").FontColor(Colors.White).Bold().FontSize(9);
                                        header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                            .Text("IGI Pagado").FontColor(Colors.White).Bold().FontSize(9);
                                        header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                            .Text("IGI Calc.").FontColor(Colors.White).Bold().FontSize(9);
                                        header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                            .Text("Diferencia").FontColor(Colors.White).Bold().FontSize(9);
                                    });

                                    int contador = 0;
                                    foreach (var item in reporteIGI.Take(50))
                                    {
                                        var bgColor = contador % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                        string fechaTexto = item.FechaPago.HasValue ? string.Format("{0:dd/MM/yyyy}", item.FechaPago.Value) : "N/A";

                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(item.Pedimento).FontSize(8);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(fechaTexto).FontSize(8);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                            .Text($"${item.IGI_Pagado:N2}").FontSize(8);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                            .Text($"${item.IGI_Calculado:N2}").FontSize(8);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                            .Text($"${item.DiferenciaIGI:N2}")
                                            .FontSize(8)
                                            .FontColor(item.DiferenciaIGI > 0 ? Colors.Green.Darken1 : item.DiferenciaIGI < 0 ? Colors.Red.Darken1 : Colors.Grey.Darken1);

                                        contador++;
                                    }

                                    if (reporteIGI.Count > 50)
                                    {
                                        table.Cell().ColumnSpan(5).Background(Colors.Yellow.Lighten3).Padding(8).AlignCenter()
                                            .Text($"Nota: Se muestran los primeros 50 registros de {reporteIGI.Count} totales")
                                            .FontSize(9).Italic();
                                    }
                                });
                            }

                            column.Item().PaddingTop(20).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                            {
                                col.Item().Text("Nota Importante")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(5).Text("Este reporte muestra la comparación entre el IGI pagado registrado en los pedimentos y el IGI calculado según los datos de glosa. Las diferencias pueden deberse a ajustes, rectificaciones o errores en la captura.")
                                    .FontSize(9)
                                    .Italic();
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(9))
                        .Text(txt =>
                        {
                            txt.Span("Página ");
                            txt.CurrentPageNumber();
                            txt.Span(" de ");
                            txt.TotalPages();
                        });
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        /// <summary>
        /// Genera el reporte de IGI en PDF organizado por forma de pago (5 y 0)
        /// </summary>
        public void GenerarReporteIGIPDFPorFormaPago(List<ReporteIGIPagado> reporteIGI, ResumenIGI resumen, string razonSocial, string baseDatos, DateTime fechaInicio, DateTime fechaFin, string rutaArchivo)
        {
            // Separar reportes por forma de pago
            var reportesFormaPago5 = reporteIGI.Where(r => r.FormaPago_IGI == "5").OrderBy(r => r.FechaPago).ToList();
            var reportesFormaPago0 = reporteIGI.Where(r => r.FormaPago_IGI == "0" || (r.FormaPago_IGI != "5" && r.FormaPago_IGI != "21")).OrderBy(r => r.FechaPago).ToList();

            // Calcular totales por forma de pago
            var totalIGI_Pagado5 = reportesFormaPago5.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado5 = reportesFormaPago5.Sum(r => r.IGI_Calculado);
            var totalIVA_Pagado5 = reportesFormaPago5.Sum(r => r.IVA_Pagado);
            var totalIGI_Pagado0 = reportesFormaPago0.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado0 = reportesFormaPago0.Sum(r => r.IGI_Calculado);
            var totalIVA_Pagado0 = reportesFormaPago0.Sum(r => r.IVA_Pagado);

            byte[] imagenBarras = GenerarGraficoBarrasPorFormaPago(totalIGI_Pagado5, totalIGI_Calculado5, totalIVA_Pagado5, totalIGI_Pagado0, totalIGI_Calculado0, totalIVA_Pagado0);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Reporte de IGI por Forma de Pago")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Razón Social: ").Bold();
                                    txt.Span(razonSocial);
                                });
                            });

                            if (!string.IsNullOrEmpty(baseDatos))
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Base(s) de Datos: ").Bold();
                                        txt.Span(baseDatos);
                                    });
                                });
                            }

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Fecha de generación: ").FontSize(9);
                                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(9);
                                });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("Información General")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().Text(txt =>
                                {
                                    txt.Span("Período: ").Bold();
                                    txt.Span($"{fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}");
                                });

                                col.Item().Text(txt =>
                                {
                                    txt.Span("Total Registros: ").Bold();
                                    txt.Span(resumen.TotalPedimentos.ToString());
                                });
                            });

                            // ===== RESUMEN POR FORMA DE PAGO =====
                            column.Item().PaddingTop(15).PaddingBottom(10).Text("Resumen por Forma de Pago")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            // Tabla comparativa de formas de pago
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.2f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8)
                                        .Text("Forma de Pago").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8).AlignRight()
                                        .Text("IGI Pagado").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8).AlignRight()
                                        .Text("IGI Calculado").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(8).AlignRight()
                                        .Text("IVA Pagado").FontColor(Colors.White).Bold();
                                });

                                // Forma de Pago 5
                                table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text($"Forma de Pago 5 ({reportesFormaPago5.Count} registros)").Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIGI_Pagado5:N2}")
                                    .FontColor(new SKColor(52, 152, 219).ToQuestColor()).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIGI_Calculado5:N2}")
                                    .FontColor(new SKColor(46, 204, 113).ToQuestColor()).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIVA_Pagado5:N2}")
                                    .FontColor(new SKColor(241, 196, 15).ToQuestColor()).Bold();

                                // Forma de Pago 0
                                table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text($"Forma de Pago 0 ({reportesFormaPago0.Count} registros)").Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIGI_Pagado0:N2}")
                                    .FontColor(new SKColor(52, 152, 219).ToQuestColor()).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIGI_Calculado0:N2}")
                                    .FontColor(new SKColor(46, 204, 113).ToQuestColor()).Bold();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${totalIVA_Pagado0:N2}")
                                    .FontColor(new SKColor(241, 196, 15).ToQuestColor()).Bold();

                                // Total General
                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text($"TOTAL GENERAL ({resumen.TotalPedimentos} registros)").Bold().FontSize(11);
                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIGI_Pagado:N2}")
                                    .FontSize(11).FontColor(Colors.Blue.Darken2).Bold();
                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIGI_Calculado:N2}")
                                    .FontSize(11).FontColor(Colors.Green.Darken2).Bold();
                                table.Cell().Background(Colors.Orange.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight()
                                    .Text($"${resumen.TotalIVA_Pagado:N2}")
                                    .FontSize(11).FontColor(Colors.Orange.Darken2).Bold();
                            });

                            // Gráfica comparativa
                            column.Item().PaddingTop(20).PaddingBottom(10).Text("Representación Gráfica")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().AlignCenter().Text("Comparativa por Forma de Pago")
                                        .FontSize(11)
                                        .Bold();
                                    col.Item().PaddingTop(5).Image(imagenBarras).FitArea();
                                });
                            });

                            // Detalle de pedimentos por forma de pago
                            if (reporteIGI.Any())
                            {
                                column.Item().PageBreak();

                                // FORMA DE PAGO 5
                                if (reportesFormaPago5.Any())
                                {
                                    column.Item().PaddingBottom(10).Background(Colors.Blue.Lighten4).Padding(8).Text("Detalle - Forma de Pago 5")
                                        .FontSize(13)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken2);

                                    GenerarTablaDetalle(column, reportesFormaPago5, 30);
                                }

                                // FORMA DE PAGO 0
                                if (reportesFormaPago0.Any())
                                {
                                    column.Item().PaddingTop(15).PaddingBottom(10).Background(Colors.Red.Lighten4).Padding(8).Text("Detalle - Forma de Pago 0")
                                        .FontSize(13)
                                        .Bold()
                                        .FontColor(Colors.Red.Darken2);

                                    GenerarTablaDetalle(column, reportesFormaPago0, 30);
                                }
                            }

                            column.Item().PaddingTop(20).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                            {
                                col.Item().Text("Nota Importante")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(5).Text("Este reporte muestra el desglose de IGI e IVA pagado por forma de pago. Forma de Pago 5 corresponde a pagos efectuados, mientras que Forma de Pago 0 corresponde a otros métodos de pago.")
                                    .FontSize(9)
                                    .Italic();
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(9))
                        .Text(txt =>
                        {
                            txt.Span("Página ");
                            txt.CurrentPageNumber();
                            txt.Span(" de ");
                            txt.TotalPages();
                        });
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        /// <summary>
        /// Método auxiliar para generar tablas de detalle
        /// </summary>
        private void GenerarTablaDetalle(ColumnDescriptor column, List<ReporteIGIPagado> reportes, int maxRegistros)
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5)
                        .Text("Fecha Pago").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).AlignRight()
                        .Text("IGI Pagado").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).AlignRight()
                        .Text("IGI Calc.").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).AlignRight()
                        .Text("Diferencia").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).AlignRight()
                        .Text("IVA Pagado").FontColor(Colors.White).Bold().FontSize(9);
                });

                int contador = 0;
                foreach (var item in reportes.Take(maxRegistros))
                {
                    var bgColor = contador % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                    string fechaTexto = item.FechaPago.HasValue ? string.Format("{0:dd/MM/yyyy}", item.FechaPago.Value) : "N/A";

                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(fechaTexto).FontSize(8);
                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text($"${item.IGI_Pagado:N2}").FontSize(8);
                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text($"${item.IGI_Calculado:N2}").FontSize(8);
                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text($"${item.DiferenciaIGI:N2}")
                        .FontSize(8)
                        .FontColor(item.DiferenciaIGI != 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);
                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text($"${item.IVA_Pagado:N2}").FontSize(8);

                    contador++;
                }

                if (reportes.Count > maxRegistros)
                {
                    table.Cell().ColumnSpan(5).Background(Colors.Yellow.Lighten3).Padding(8).AlignCenter()
                        .Text($"Nota: Se muestran los primeros {maxRegistros} registros de {reportes.Count} totales")
                        .FontSize(9).Italic();
                }
            });
        }

        private byte[] GenerarGraficoBarrasIGI(ResumenIGI resumen)
        {
            int width = 400;
            int height = 300;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            SKColor colorPagado = new SKColor(52, 152, 219);
            SKColor colorCalculado = new SKColor(46, 204, 113);

            decimal maxValor = Math.Max(resumen.TotalIGI_Pagado, resumen.TotalIGI_Calculado);
            if (maxValor == 0) maxValor = 1;

            float barWidth = 100;
            float spacing = 60;
            float maxBarHeight = height - 100;
            float baseY = height - 50;

            float x1 = (width / 2) - barWidth - spacing / 2;
            float x2 = (width / 2) + spacing / 2;

            float altura1 = (float)(resumen.TotalIGI_Pagado / maxValor) * maxBarHeight;
            float altura2 = (float)(resumen.TotalIGI_Calculado / maxValor) * maxBarHeight;

            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = baseY - (maxBarHeight * i / 4);
                    canvas.DrawLine(30, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 4;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" : $"${valorEje / 1000:N0}K";
                    DrawTextBlob(canvas, label, 5, y + 3, 10, SKColors.Gray, SKTextAlign.Left);
                }
            }

            using (var paint1 = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(x1, baseY - altura1, barWidth, altura1, paint1);
            }

            using (var paint2 = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(x2, baseY - altura2, barWidth, altura2, paint2);
            }

            DrawTextBlob(canvas, "IGI Comparativo", width / 2, 20, 12, SKColors.Black, SKTextAlign.Center);

            DrawTextBlob(canvas, $"${resumen.TotalIGI_Pagado:N0}", x1 + barWidth / 2, baseY - altura1 - 8, 11, colorPagado, SKTextAlign.Center);
            DrawTextBlob(canvas, $"${resumen.TotalIGI_Calculado:N0}", x2 + barWidth / 2, baseY - altura2 - 8, 11, colorCalculado, SKTextAlign.Center);

            {
                float legendX = x1 + barWidth / 2 - 40;
                float legendY = baseY + 20;

                using (var rectPaint = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 15, 15, rectPaint);
                }
                DrawTextBlob(canvas, "IGI Pagado", legendX + 20, legendY + 12, 11, SKColors.Black, SKTextAlign.Left);

                using (var rectPaint = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 120, legendY, 15, 15, rectPaint);
                }
                DrawTextBlob(canvas, "IGI Calculado", legendX + 140, legendY + 12, 11, SKColors.Black, SKTextAlign.Left);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Genera un gráfico de barras agrupadas comparando formas de pago 5 y 0
        /// </summary>
        private byte[] GenerarGraficoBarrasPorFormaPago(decimal igiPagado5, decimal igiCalculado5, decimal ivaPagado5, 
                                                         decimal igiPagado0, decimal igiCalculado0, decimal ivaPagado0)
        {
            int width = 500;
            int height = 350;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            // Colores
            var colorIGIPagado = new SKColor(52, 152, 219);      // Azul
            var colorIGICalculado = new SKColor(46, 204, 113);   // Verde
            var colorIVAPagado = new SKColor(241, 196, 15);      // Amarillo

            // Configuración del gráfico
            float barWidth = 50;
            float groupSpacing = 80;
            float barSpacing = 8;
            float maxBarHeight = 220;

            // Encontrar el valor máximo para escalar
            decimal maxValor = Math.Max(
                Math.Max(Math.Max(igiPagado5, igiCalculado5), ivaPagado5),
                Math.Max(Math.Max(igiPagado0, igiCalculado0), ivaPagado0)
            );

            if (maxValor == 0) maxValor = 1;

            float baseY = height - 70;

            // Posiciones de los grupos
            float group1X = 100;  // Grupo Forma de Pago 5
            float group2X = group1X + groupSpacing + (barWidth + barSpacing) * 3;  // Grupo Forma de Pago 0

            // Calcular alturas
            float altura_IGIPagado5 = (float)(igiPagado5 / maxValor) * maxBarHeight;
            float altura_IGICalculado5 = (float)(igiCalculado5 / maxValor) * maxBarHeight;
            float altura_IVAPagado5 = (float)(ivaPagado5 / maxValor) * maxBarHeight;

            float altura_IGIPagado0 = (float)(igiPagado0 / maxValor) * maxBarHeight;
            float altura_IGICalculado0 = (float)(igiCalculado0 / maxValor) * maxBarHeight;
            float altura_IVAPagado0 = (float)(ivaPagado0 / maxValor) * maxBarHeight;

            // Dibujar líneas de cuadrícula y etiquetas del eje Y
            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = baseY - (maxBarHeight * i / 5);
                    canvas.DrawLine(80, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 5;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" 
                                 : valorEje >= 1000 ? $"${valorEje / 1000:N0}K" 
                                 : $"${valorEje:N0}";
                    DrawTextBlob(canvas, label, 5, y + 3, 9, SKColors.Gray, SKTextAlign.Left);
                }
            }

            // GRUPO 1: Forma de Pago 5
            using (var paint = new SKPaint { Color = colorIGIPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group1X, baseY - altura_IGIPagado5, barWidth, altura_IGIPagado5, paint);
            }
            using (var paint = new SKPaint { Color = colorIGICalculado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group1X + barWidth + barSpacing, baseY - altura_IGICalculado5, barWidth, altura_IGICalculado5, paint);
            }
            using (var paint = new SKPaint { Color = colorIVAPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group1X + (barWidth + barSpacing) * 2, baseY - altura_IVAPagado5, barWidth, altura_IVAPagado5, paint);
            }

            // GRUPO 2: Forma de Pago 0
            using (var paint = new SKPaint { Color = colorIGIPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group2X, baseY - altura_IGIPagado0, barWidth, altura_IGIPagado0, paint);
            }
            using (var paint = new SKPaint { Color = colorIGICalculado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group2X + barWidth + barSpacing, baseY - altura_IGICalculado0, barWidth, altura_IGICalculado0, paint);
            }
            using (var paint = new SKPaint { Color = colorIVAPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(group2X + (barWidth + barSpacing) * 2, baseY - altura_IVAPagado0, barWidth, altura_IVAPagado0, paint);
            }

            // Título
            DrawTextBlob(canvas, "Comparativa IGI e IVA por Forma de Pago", width / 2, 20, 14, SKColors.Black, SKTextAlign.Center);

            // Etiquetas de grupos
            {
                float group1Center = group1X + (barWidth * 3 + barSpacing * 2) / 2;
                float group2Center = group2X + (barWidth * 3 + barSpacing * 2) / 2;

                DrawTextBlob(canvas, "Forma Pago 5", group1Center, baseY + 20, 11, SKColors.Black, SKTextAlign.Center);
                DrawTextBlob(canvas, "Forma Pago 0", group2Center, baseY + 20, 11, SKColors.Black, SKTextAlign.Center);
            }

            // Leyenda
            {
                float legendX = width / 2 - 150;
                float legendY = baseY + 40;

                // IGI Pagado
                using (var rectPaint = new SKPaint { Color = colorIGIPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IGI Pagado", legendX + 17, legendY + 10, 10, SKColors.Black, SKTextAlign.Left);

                // IGI Calculado
                using (var rectPaint = new SKPaint { Color = colorIGICalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 100, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IGI Calculado", legendX + 117, legendY + 10, 10, SKColors.Black, SKTextAlign.Left);

                // IVA Pagado
                using (var rectPaint = new SKPaint { Color = colorIVAPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 210, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IVA Pagado", legendX + 227, legendY + 10, 10, SKColors.Black, SKTextAlign.Left);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

   
        /// <summary>
        /// Genera gráfico de barras apiladas horizontales por mes y forma de pago para IGI
        /// </summary>
        private byte[] GenerarGraficoIGIPorMes(List<ReporteIGIPagado> reporteCompleto)
        {
            int width = 700;
            int height = 500;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Agrupar datos por mes y forma de pago
            var datosPorMes = reporteCompleto
                .Where(r => r.FechaPago.HasValue)
                .GroupBy(r => new {
                    Año = r.FechaPago.Value.Year,
                    Mes = r.FechaPago.Value.Month,
                    FormaPago = r.FormaPago_IGI
                })
                .Select(g => new {
                    Periodo = new DateTime(g.Key.Año, g.Key.Mes, 1),
                    MesNombre = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM").ToLower(),
                    FormaPago = g.Key.FormaPago,
                    Pagado = g.Sum(x => x.IGI_Pagado),
                    Calculado = g.Sum(x => x.IGI_Calculado),
                    Diferencia = g.Sum(x => x.DiferenciaIGI)
                })
                .OrderByDescending(x => x.Periodo)
                .ToList();

            if (datosPorMes.Count == 0) return Array.Empty<byte>();

            // Colores
            var colorPagado = new SKColor(79, 129, 189);    // Azul
            var colorCalculado = new SKColor(192, 192, 192); // Gris
            var colorDiferencia = new SKColor(155, 194, 230); // Azul claro

            // Obtener meses únicos y formas de pago
            var mesesUnicos = datosPorMes.Select(x => x.MesNombre).Distinct().ToList();
            var formasPago = datosPorMes.Select(x => x.FormaPago).Distinct().OrderBy(x => x).ToList();

            // Calcular número total de barras
            int totalBarras = mesesUnicos.Count * formasPago.Count;
            float barHeight = Math.Min(30, (height - 150) / (totalBarras * 1.3f));
            float leftMargin = 120;
            float rightMargin = 50;
            float maxBarWidth = width - leftMargin - rightMargin;

            // Calcular máximo para escalar
            decimal maxValor = datosPorMes.Any() ? datosPorMes.Max(x => x.Pagado + x.Calculado + Math.Abs(x.Diferencia)) : 1;
            if (maxValor == 0) maxValor = 1;

            // Dibujar eje vertical (línea base)
            using (var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2 })
            {
                canvas.DrawLine(leftMargin, 50, leftMargin, height - 50, axisPaint);
            }

            // Dibujar líneas de cuadrícula y etiquetas del eje X
            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            {
                for (int i = 0; i <= 5; i++)
                {
                    decimal valorEje = maxValor * i / 5;
                    float x = leftMargin + (maxBarWidth * i / 5);
                    canvas.DrawLine(x, 50, x, height - 50, gridPaint);
                    string label = valorEje >= 1000000 ? $"{valorEje / 1000000:N0}M"
                                 : valorEje >= 1000 ? $"{valorEje / 1000:N0}K"
                                 : $"{valorEje:N0}";
                    DrawTextBlob(canvas, label, x, height - 35, 8, SKColors.Gray, SKTextAlign.Center);
                }
            }

            // Dibujar barras horizontales apiladas por mes y forma de pago
            float startY = 60;
            float spacing = Math.Min(60, (height - 110) / totalBarras);
            int barIndex = 0;

            foreach (var mes in mesesUnicos)
            {
                foreach (var fp in formasPago)
                {
                    var datos = datosPorMes.FirstOrDefault(x => x.MesNombre == mes && x.FormaPago == fp);

                    if (datos == null)
                    {
                        barIndex++;
                        continue;
                    }

                    float yPos = startY + (barIndex * spacing);

                    // Calcular anchos
                    float anchoPagado = (float)(datos.Pagado / maxValor) * maxBarWidth;
                    float anchoCalculado = (float)(datos.Calculado / maxValor) * maxBarWidth;
                    float anchoDiferencia = (float)(Math.Abs(datos.Diferencia) / maxValor) * maxBarWidth;

                    float xInicio = leftMargin;

                    // Barra IGI Pagado (azul)
                    if (datos.Pagado > 0)
                    {
                        using (var paint = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill, IsAntialias = true })
                        {
                            canvas.DrawRect(xInicio, yPos, anchoPagado, barHeight, paint);
                        }
                        // Etiqueta Pagado
                        {
                            string texto = datos.Pagado >= 1000 ? $"{datos.Pagado / 1000:N0}K" : $"{datos.Pagado:N0}";
                            if (anchoPagado > 30)
                                DrawTextBlob(canvas, texto, xInicio + anchoPagado / 2, yPos + barHeight / 2 + 3, 7, SKColors.White, SKTextAlign.Center);
                        }
                    }
                    xInicio += anchoPagado;

                    // Barra IGI Calculado (gris)
                    if (datos.Calculado > 0)
                    {
                        using (var paint = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill, IsAntialias = true })
                        {
                            canvas.DrawRect(xInicio, yPos, anchoCalculado, barHeight, paint);
                        }
                        // Etiqueta Calculado
                        {
                            string texto = datos.Calculado >= 1000 ? $"{datos.Calculado / 1000:N0}K" : $"{datos.Calculado:N0}";
                            if (anchoCalculado > 30)
                                DrawTextBlob(canvas, texto, xInicio + anchoCalculado / 2, yPos + barHeight / 2 + 3, 7, new SKColor(64, 64, 64), SKTextAlign.Center);
                        }
                    }
                    xInicio += anchoCalculado;

                    // Barra Diferencia (azul claro)
                    if (datos.Diferencia != 0)
                    {
                        using (var paint = new SKPaint { Color = colorDiferencia, Style = SKPaintStyle.Fill, IsAntialias = true })
                        {
                            canvas.DrawRect(xInicio, yPos, anchoDiferencia, barHeight, paint);
                        }
                        // Etiqueta Diferencia
                        {
                            string texto = Math.Abs(datos.Diferencia) >= 1000 ? $"{Math.Abs(datos.Diferencia) / 1000:N0}K" : $"{Math.Abs(datos.Diferencia):N0}";
                            if (anchoDiferencia > 30)
                                DrawTextBlob(canvas, texto, xInicio + anchoDiferencia / 2, yPos + barHeight / 2 + 3, 7, new SKColor(64, 64, 64), SKTextAlign.Center);
                        }
                    }

                    // Etiqueta del mes y forma de pago (a la izquierda)
                    {
                        string etiqueta = $"{mes} FP-{fp}";
                        DrawTextBlob(canvas, etiqueta, 5, yPos + barHeight / 2 + 4, 11, SKColors.Black, SKTextAlign.Left);
                    }

                    barIndex++;
                }
            }

            // Título
            DrawTextBlob(canvas, "IGI por Mes y Forma de Pago", width / 2, 25, 14, SKColors.Black, SKTextAlign.Center);

            // Leyenda
            {
                float legendX = leftMargin;
                float legendY = height - 20;

                using (var rectPaint = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IGI pagado", legendX + 17, legendY + 10, 9, SKColors.Black, SKTextAlign.Left);

                using (var rectPaint = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 100, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IGI calculado", legendX + 117, legendY + 10, 9, SKColors.Black, SKTextAlign.Left);

                using (var rectPaint = new SKPaint { Color = colorDiferencia, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 210, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "Diferencia", legendX + 227, legendY + 10, 9, SKColors.Black, SKTextAlign.Left);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Genera gráfico de barras horizontales por mes y forma de pago para IVA
        /// </summary>
        private byte[] GenerarGraficoIVAPorMes(List<ReporteIGIPagado> reporteCompleto)
        {
            int width = 800;
            int height = 700;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Agrupar datos por mes y forma de pago IVA
            var datosPorMes = reporteCompleto
                .Where(r => r.FechaPago.HasValue && r.FormaPago_IVA != null)
                .GroupBy(r => new {
                    Año = r.FechaPago.Value.Year,
                    Mes = r.FechaPago.Value.Month,
                    FormaPago = r.FormaPago_IVA
                })
                .Select(g => new {
                    Periodo = new DateTime(g.Key.Año, g.Key.Mes, 1),
                    MesNombre = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM").ToLower(),
                    FormaPago = g.Key.FormaPago,
                    IVAPagado = g.Sum(x => x.IVA_Pagado)
                })
                .OrderByDescending(x => x.Periodo)
                .ToList();

            if (datosPorMes.Count == 0) return Array.Empty<byte>();

            // Color verde para IVA
            var colorIVA = new SKColor(46, 204, 113);

            // Obtener meses únicos y formas de pago
            var mesesUnicos = datosPorMes.Select(x => x.MesNombre).Distinct().ToList();
            var formasPago = datosPorMes.Select(x => x.FormaPago).Distinct().OrderBy(x => x).ToList();

            // Calcular número total de barras con mayor altura
            int totalBarras = mesesUnicos.Count * formasPago.Count;
            float barHeight = Math.Min(50, (height - 150) / (totalBarras * 1.1f));
            float leftMargin = 120;
            float rightMargin = 50;
            float maxBarWidth = width - leftMargin - rightMargin;

            // Calcular máximo para escalar
            decimal maxValor = datosPorMes.Any() ? datosPorMes.Max(x => x.IVAPagado) : 1;
            if (maxValor == 0) maxValor = 1;

            // Dibujar eje vertical (línea base)
            using (var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2 })
            {
                canvas.DrawLine(leftMargin, 50, leftMargin, height - 50, axisPaint);
            }

            // Dibujar líneas de cuadrícula y etiquetas del eje X
            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            {
                for (int i = 0; i <= 5; i++)
                {
                    decimal valorEje = maxValor * i / 5;
                    float x = leftMargin + (maxBarWidth * i / 5);
                    canvas.DrawLine(x, 50, x, height - 50, gridPaint);
                    string label = valorEje >= 1000000 ? $"{valorEje / 1000000:N0}M"
                                 : valorEje >= 1000 ? $"{valorEje / 1000:N0}K"
                                 : $"{valorEje:N0}";
                    DrawTextBlob(canvas, label, x, height - 35, 8, SKColors.Gray, SKTextAlign.Center);
                }
            }

            // Dibujar barras horizontales por mes y forma de pago
            float startY = 60;
            float spacing = Math.Min(60, (height - 110) / totalBarras);
            int barIndex = 0;

            foreach (var mes in mesesUnicos)
            {
                foreach (var fp in formasPago)
                {
                    var datos = datosPorMes.FirstOrDefault(x => x.MesNombre == mes && x.FormaPago == fp);

                    if (datos == null)
                    {
                        barIndex++;
                        continue;
                    }

                    float yPos = startY + (barIndex * spacing);

                    // Calcular ancho
                    float anchoIVA = (float)(datos.IVAPagado / maxValor) * maxBarWidth;

                    float xInicio = leftMargin;

                    // Barra IVA Pagado (verde)
                    if (datos.IVAPagado > 0)
                    {
                        using (var paint = new SKPaint { Color = colorIVA, Style = SKPaintStyle.Fill, IsAntialias = true })
                        {
                            canvas.DrawRect(xInicio, yPos, anchoIVA, barHeight, paint);
                        }
                        // Etiqueta IVA
                        {
                            string texto = datos.IVAPagado >= 1000 ? $"{datos.IVAPagado / 1000:N0}K" : $"{datos.IVAPagado:N0}";
                            if (anchoIVA > 30)
                                DrawTextBlob(canvas, texto, xInicio + anchoIVA / 2, yPos + barHeight / 2 + 4, 11, SKColors.White, SKTextAlign.Center);
                        }
                    }

                    // Etiqueta del mes y forma de pago (a la izquierda)
                    {
                        string etiqueta = $"{mes} FP-{fp}";
                        DrawTextBlob(canvas, etiqueta, 5, yPos + barHeight / 2 + 4, 11, SKColors.Black, SKTextAlign.Left);
                    }

                    barIndex++;
                }
            }

            // Título
            DrawTextBlob(canvas, "IVA por Mes y Forma de Pago", width / 2, 25, 14, SKColors.Black, SKTextAlign.Center);

            // Leyenda
            {
                float legendX = leftMargin;
                float legendY = height - 20;

                using (var rectPaint = new SKPaint { Color = colorIVA, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                DrawTextBlob(canvas, "IVA pagado", legendX + 17, legendY + 10, 9, SKColors.Black, SKTextAlign.Left);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Genera el PDF del reporte IGI con tablas separadas, gráfico y resumen por forma de pago
        /// </summary>
        public void GenerarReporteIGIConFormasPagoPDF(
            List<ReporteIGIPagado> reporteCompleto,
            System.Data.DataTable tablaIGI,
            System.Data.DataTable tablaIVA,
            System.Data.DataTable tablaDetalleCompleto,
            ResumenIGI resumen,
            string razonSocial,
            string baseDatos,
            DateTime fechaInicio,
            DateTime fechaFin,
            string rutaArchivo)
        {
            // Calcular datos para gráfico y resumen separados por forma de pago
            var reportesIGI_FormaPago5 = reporteCompleto.Where(r => r.FormaPago_IGI == "5").ToList();
            var reportesIGI_FormaPago0 = reporteCompleto.Where(r => r.FormaPago_IGI == "0").ToList();

            var totalIGI_Pagado5 = reportesIGI_FormaPago5.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado5 = reportesIGI_FormaPago5.Sum(r => r.IGI_Calculado);
            var diferenciaIGI_5 = totalIGI_Pagado5 - totalIGI_Calculado5;

            var totalIGI_Pagado0 = reportesIGI_FormaPago0.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado0 = reportesIGI_FormaPago0.Sum(r => r.IGI_Calculado);
            var diferenciaIGI_0 = totalIGI_Pagado0 - totalIGI_Calculado0;

            var reportesIVA_FormaPago21 = reporteCompleto.Where(r => r.FormaPago_IVA == "21").ToList();
            var reportesIVA_FormaPago0 = reporteCompleto.Where(r => r.FormaPago_IVA == "0").ToList();

            var totalIVA_Pagado21 = reportesIVA_FormaPago21.Sum(r => r.IVA_Pagado);
            var totalIVA_Pagado0 = reportesIVA_FormaPago0.Sum(r => r.IVA_Pagado);



            // Generar gráfico de barras apiladas por mes
            byte[] imagenGraficoIGI = GenerarGraficoIGIPorMes(reporteCompleto);
            byte[] imagenGraficoIVA = GenerarGraficoIVAPorMes(reporteCompleto);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Reporte de IGI e IVA por Forma de Pago")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Razón Social: ").Bold();
                                    txt.Span(razonSocial);
                                });
                            });

                            if (!string.IsNullOrEmpty(baseDatos))
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Base(s) de Datos: ").Bold();
                                        txt.Span(baseDatos);
                                    });
                                });
                            }

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Período: ").Bold();
                                    txt.Span($"{fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}");
                                });
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Fecha de generación: ").FontSize(9);
                                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(9);
                                });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            // Resumen por forma de pago
                            column.Item().PaddingBottom(10).Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("Resumen Financiero por Forma de Pago")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().PaddingTop(5).Text(txt =>
                                {
                                    txt.Span("📊 Total: ").Bold();
                                    txt.Span($"{resumen.TotalPedimentos} registros");
                                });

                                col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().PaddingTop(5).Text(txt =>
                                {
                                    txt.Span("💳 IGI FP-5:   ").Bold().FontColor(Colors.Blue.Darken1);
                                    txt.Span($"Pagado: {totalIGI_Pagado5:C2}  |  Calculado: {totalIGI_Calculado5:C2}  |  Diferencia: {diferenciaIGI_5:C2}");
                                });

                                col.Item().Text(txt =>
                                {
                                    txt.Span("💰 IGI FP-0:   ").Bold().FontColor(Colors.Green.Darken1);
                                    txt.Span($"Pagado: {totalIGI_Pagado0:C2}  |  Calculado: {totalIGI_Calculado0:C2}  |  Diferencia: {diferenciaIGI_0:C2}");
                                });

                                col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().PaddingTop(5).Text(txt =>
                                {
                                    txt.Span("💵 IVA FP-21:  ").Bold().FontColor(Colors.Purple.Darken1);
                                    txt.Span($"Pagado: {totalIVA_Pagado21:C2}");
                                });

                                col.Item().Text(txt =>
                                {
                                    txt.Span("💵 IVA FP-0:   ").Bold().FontColor(Colors.Orange.Darken1);
                                    txt.Span($"Pagado: {totalIVA_Pagado0:C2}");
                                });
                            });

                            // Gráfico
                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Representación Gráfica - IGI por Mes")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                                .Image(imagenGraficoIGI).FitArea();

                            // Salto de página antes de las tablas
                            column.Item().PageBreak();

                            // Tabla IGI
                            column.Item().PaddingBottom(10).Text("Detalle IGI por Mes y Forma de Pago")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);      // MES
                                    columns.RelativeColumn(1.5f);   // IGI PAGADO
                                    columns.RelativeColumn(1.5f);   // IGI CALCULADO
                                    columns.RelativeColumn(1.5f);   // DIFERENCIA
                                    columns.RelativeColumn(1);      // FORMA DE PAGO IGI
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(5)
                                        .Text("MES").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                        .Text("IGI PAGADO").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                        .Text("IGI CALCULADO").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(5).AlignRight()
                                        .Text("DIFERENCIA").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Darken1).Padding(5)
                                        .Text("FORMA DE PAGO").FontColor(Colors.White).Bold().FontSize(9);
                                });

                                int contador = 0;
                                foreach (System.Data.DataRow row in tablaIGI.Rows)
                                {
                                    var bgColor = contador % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(row["MES"].ToString()).FontSize(8);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                        .Text(Convert.ToDecimal(row["IGI PAGADO"]).ToString("C2")).FontSize(8);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                        .Text(Convert.ToDecimal(row["IGI CALCULADO"]).ToString("C2")).FontSize(8);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                        .Text(Convert.ToDecimal(row["DIFERENCIA"]).ToString("C2")).FontSize(8)
                                        .FontColor(Convert.ToDecimal(row["DIFERENCIA"]) != 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(row["FORMA DE PAGO IGI"].ToString()).FontSize(8);

                                    contador++;
                                }
                            });

                            // Salto de página antes de la tabla IVA
                            column.Item().PageBreak();

                            // Gráfico IVA
                            column.Item().PaddingTop(15).PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Representación Gráfica - IVA por Mes y Forma de Pago")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Purple.Darken2);
                            });

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                                .Image(imagenGraficoIVA).FitArea();

                            // Tabla IVA
                            column.Item().PaddingTop(20).PaddingBottom(10).Text("Detalle IVA por Mes y Forma de Pago")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);      // MES
                                    columns.RelativeColumn(2);      // IVA PAGADO
                                    columns.RelativeColumn(1.5f);   // FORMA DE PAGO IVA
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Purple.Darken1).Padding(5)
                                        .Text("MES").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Purple.Darken1).Padding(5).AlignRight()
                                        .Text("IVA PAGADO").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Purple.Darken1).Padding(5)
                                        .Text("FORMA DE PAGO").FontColor(Colors.White).Bold().FontSize(9);
                                });

                                int contadorIVA = 0;
                                foreach (System.Data.DataRow row in tablaIVA.Rows)
                                {
                                    var bgColor = contadorIVA % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(row["MES"].ToString()).FontSize(8);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                        .Text(Convert.ToDecimal(row["IVA PAGADO"]).ToString("C2")).FontSize(8);
                                    table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(row["FORMA DE PAGO IVA"].ToString()).FontSize(8);

                                    contadorIVA++;
                                }
                            });

                            if (tablaDetalleCompleto != null && tablaDetalleCompleto.Rows.Count > 0)
                            {
                                column.Item().PageBreak();

                                column.Item().PaddingBottom(10).Text("Detalle Completo de Pedimentos")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("Base Datos").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignRight().Text("ID").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("Pedimento").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("Fecha").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignRight().Text("IGI Pagado").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignRight().Text("IGI Calc.").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignRight().Text("Dif. IGI").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignRight().Text("IVA Pagado").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("FP IGI").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("FP IVA").FontColor(Colors.White).Bold().FontSize(7);
                                        header.Cell().Background(Colors.Grey.Darken2).Padding(4).Text("Estatus").FontColor(Colors.White).Bold().FontSize(7);
                                    });

                                    int contadorDetalle = 0;
                                    foreach (System.Data.DataRow row in tablaDetalleCompleto.Rows)
                                    {
                                        var bgColor = contadorDetalle % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Base Datos") ? row["Base Datos"].ToString() : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                                            .Text(row.Table.Columns.Contains("ID Pedimento") ? row["ID Pedimento"].ToString() : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Pedimento") ? row["Pedimento"].ToString() : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Fecha Pago") && row["Fecha Pago"] != DBNull.Value ? Convert.ToDateTime(row["Fecha Pago"]).ToString("dd/MM/yyyy") : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                                            .Text(row.Table.Columns.Contains("IGI Pagado") ? Convert.ToDecimal(row["IGI Pagado"]).ToString("C2") : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                                            .Text(row.Table.Columns.Contains("IGI Calculado") ? Convert.ToDecimal(row["IGI Calculado"]).ToString("C2") : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                                            .Text(row.Table.Columns.Contains("Diferencia IGI") ? Convert.ToDecimal(row["Diferencia IGI"]).ToString("C2") : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                                            .Text(row.Table.Columns.Contains("IVA Pagado") ? Convert.ToDecimal(row["IVA Pagado"]).ToString("C2") : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Forma Pago IGI") ? row["Forma Pago IGI"].ToString() : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Forma Pago IVA") ? row["Forma Pago IVA"].ToString() : string.Empty).FontSize(6);
                                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                            .Text(row.Table.Columns.Contains("Estatus Glosa") ? row["Estatus Glosa"].ToString() : string.Empty).FontSize(6);

                                        contadorDetalle++;
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            })
            .GeneratePdf(rutaArchivo);
        }
    }
}

// Extension method para convertir SKColor a QuestPDF Color
public static class SKColorExtensions
{
    public static string ToQuestColor(this SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }
}
