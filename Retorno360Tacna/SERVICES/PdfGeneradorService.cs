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
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 12, IsAntialias = true })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = baseY - (maxBarHeight * i / 4);
                    canvas.DrawLine(40, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 4;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" : $"${valorEje / 1000:N0}K";
                    canvas.DrawText(label, 5, y + 4, textPaint);
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

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true })
            {
                canvas.DrawText("Valores Comerciales", width / 2, height - 10, textPaint);
            }

            using (var valuePaint = new SKPaint { TextSize = 13, TextAlign = SKTextAlign.Center, IsAntialias = true })
            {
                valuePaint.Color = colorImportacion;
                canvas.DrawText($"${resultado.ValorImportado:N0}", x1 + barWidth / 2, baseY - altura1 - 8, valuePaint);

                valuePaint.Color = colorExportacion;
                canvas.DrawText($"${resultado.ValorExportado:N0}", x2 + barWidth / 2, baseY - altura2 - 8, valuePaint);
            }

            using (var legendPaint = new SKPaint { TextSize = 12, IsAntialias = true })
            {
                float legendY = 20;
                using (var rectPaint = new SKPaint { Color = colorImportacion, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(width - 120, legendY - 8, 12, 12, rectPaint);
                }
                legendPaint.Color = SKColors.Black;
                canvas.DrawText("Importación", width - 100, legendY, legendPaint);

                using (var rectPaint = new SKPaint { Color = colorExportacion, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(width - 120, legendY + 12, 12, 12, rectPaint);
                }
                canvas.DrawText("Exportación", width - 100, legendY + 20, legendPaint);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
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

            using (var textPaint = new SKPaint { Color = SKColors.White, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                float angle1 = (startAngle + sweepAngleImport / 2) * (float)Math.PI / 180;
                float labelX1 = centerX + (radius * 0.6f) * (float)Math.Cos(angle1);
                float labelY1 = centerY + (radius * 0.6f) * (float)Math.Sin(angle1);
                canvas.DrawText($"${resultado.ValorImportado:N0}", labelX1, labelY1, textPaint);

                float angle2 = (startAngle + sweepAngleImport + sweepAngleExport / 2) * (float)Math.PI / 180;
                float labelX2 = centerX + (radius * 0.6f) * (float)Math.Cos(angle2);
                float labelY2 = centerY + (radius * 0.6f) * (float)Math.Sin(angle2);
                canvas.DrawText($"${resultado.ValorExportado:N0}", labelX2, labelY2, textPaint);
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
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 10, IsAntialias = true })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = baseY - (maxBarHeight * i / 4);
                    canvas.DrawLine(30, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 4;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" : $"${valorEje / 1000:N0}K";
                    canvas.DrawText(label, 5, y + 3, textPaint);
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

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("IGI Comparativo", width / 2, 20, textPaint);
            }

            using (var valuePaint = new SKPaint { TextSize = 11, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                valuePaint.Color = colorPagado;
                canvas.DrawText($"${resumen.TotalIGI_Pagado:N0}", x1 + barWidth / 2, baseY - altura1 - 8, valuePaint);

                valuePaint.Color = colorCalculado;
                canvas.DrawText($"${resumen.TotalIGI_Calculado:N0}", x2 + barWidth / 2, baseY - altura2 - 8, valuePaint);
            }

            using (var legendPaint = new SKPaint { TextSize = 11, IsAntialias = true })
            {
                float legendX = x1 + barWidth / 2 - 40;
                float legendY = baseY + 20;

                using (var rectPaint = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 15, 15, rectPaint);
                }
                legendPaint.Color = SKColors.Black;
                canvas.DrawText("IGI Pagado", legendX + 20, legendY + 12, legendPaint);

                using (var rectPaint = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 120, legendY, 15, 15, rectPaint);
                }
                canvas.DrawText("IGI Calculado", legendX + 140, legendY + 12, legendPaint);
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
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9, IsAntialias = true })
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = baseY - (maxBarHeight * i / 5);
                    canvas.DrawLine(80, y, width - 10, y, gridPaint);

                    decimal valorEje = maxValor * i / 5;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M" 
                                 : valorEje >= 1000 ? $"${valorEje / 1000:N0}K" 
                                 : $"${valorEje:N0}";
                    canvas.DrawText(label, 5, y + 3, textPaint);
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
            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("Comparativa IGI e IVA por Forma de Pago", width / 2, 20, textPaint);
            }

            // Etiquetas de grupos
            using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                float group1Center = group1X + (barWidth * 3 + barSpacing * 2) / 2;
                float group2Center = group2X + (barWidth * 3 + barSpacing * 2) / 2;

                canvas.DrawText("Forma Pago 5", group1Center, baseY + 20, labelPaint);
                canvas.DrawText("Forma Pago 0", group2Center, baseY + 20, labelPaint);
            }

            // Leyenda
            using (var legendPaint = new SKPaint { TextSize = 10, IsAntialias = true })
            {
                float legendX = width / 2 - 150;
                float legendY = baseY + 40;

                // IGI Pagado
                using (var rectPaint = new SKPaint { Color = colorIGIPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                legendPaint.Color = SKColors.Black;
                canvas.DrawText("IGI Pagado", legendX + 17, legendY + 10, legendPaint);

                // IGI Calculado
                using (var rectPaint = new SKPaint { Color = colorIGICalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 100, legendY, 12, 12, rectPaint);
                }
                canvas.DrawText("IGI Calculado", legendX + 117, legendY + 10, legendPaint);

                // IVA Pagado
                using (var rectPaint = new SKPaint { Color = colorIVAPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 210, legendY, 12, 12, rectPaint);
                }
                canvas.DrawText("IVA Pagado", legendX + 227, legendY + 10, legendPaint);
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Genera un gráfico con IGI (FP-0 y FP-5) e IVA (FP-0 y FP-21) con diferencias
        /// Orden: IGI FP-0, IGI FP-5, IVA FP-0, IVA FP-21
        /// </summary>
        private byte[] GenerarGraficoCompletoPorFormaPago(
            decimal igiPagado0, decimal igiCalculado0, decimal diferenciaIGI0,
            decimal igiPagado5, decimal igiCalculado5, decimal diferenciaIGI5,
            decimal ivaPagado0, decimal ivaPagado21)
        {
            int width = 800;
            int height = 450;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Colores
            var colorPagado = new SKColor(52, 152, 219);      // Azul
            var colorCalculado = new SKColor(46, 204, 113);   // Verde
            var colorIVA0 = new SKColor(241, 196, 15);        // Amarillo
            var colorIVA21 = new SKColor(155, 89, 182);       // Púrpura
            var colorDiferencia = new SKColor(231, 76, 60);   // Rojo

            float barWidth = 60;
            float baseY = height - 80;
            float maxBarHeight = 280;

            // Encontrar máximo para escalar (considerando valores absolutos de diferencia)
            decimal maxValor = Math.Max(
                Math.Max(Math.Max(igiPagado0, igiCalculado0), Math.Max(igiPagado5, igiCalculado5)),
                Math.Max(Math.Max(ivaPagado0, ivaPagado21), Math.Max(Math.Abs(diferenciaIGI0), Math.Abs(diferenciaIGI5)))
            );
            if (maxValor == 0) maxValor = 1;

            // Posiciones X para cada grupo (4 grupos con 3 posiciones cada uno)
            float startX = 80;
            float groupWidth = 180;

            float x_IGI_FP0 = startX;
            float x_IGI_FP5 = startX + groupWidth;
            float x_IVA_FP0 = startX + groupWidth * 2;
            float x_IVA_FP21 = startX + groupWidth * 3;

            // Dibujar cuadrícula y eje Y
            using (var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 })
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9, IsAntialias = true })
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = baseY - (maxBarHeight * i / 5);
                    canvas.DrawLine(60, y, width - 20, y, gridPaint);
                    decimal valorEje = maxValor * i / 5;
                    string label = valorEje >= 1000000 ? $"${valorEje / 1000000:N1}M"
                                 : valorEje >= 1000 ? $"${valorEje / 1000:N0}K"
                                 : $"${valorEje:N0}";
                    canvas.DrawText(label, 5, y + 3, textPaint);
                }
            }

            // Función helper para dibujar una barra
            void DibujarBarra(float x, decimal valor, SKColor color, bool esDiferencia = false)
            {
                if (valor == 0 && !esDiferencia) return;

                float altura = (float)(Math.Abs(valor) / maxValor) * maxBarHeight;
                float yInicio = esDiferencia && valor < 0 ? baseY : baseY - altura;

                using (var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true })
                {
                    canvas.DrawRect(x, yInicio, barWidth, altura, paint);
                }

                // Etiqueta del valor (omitir ceros)
                if (valor != 0)
                {
                    using (var valuePaint = new SKPaint
                    {
                        Color = color,
                        TextSize = 9,
                        TextAlign = SKTextAlign.Center,
                        IsAntialias = true,
                        FakeBoldText = true
                    })
                    {
                        float labelY = esDiferencia && valor < 0 ? yInicio + altura + 12 : yInicio - 5;
                        string valorTexto = Math.Abs(valor) >= 1000000 ? $"${Math.Abs(valor) / 1000000:N1}M"
                                          : Math.Abs(valor) >= 1000 ? $"${Math.Abs(valor) / 1000:N0}K"
                                          : $"${Math.Abs(valor):N0}";
                        canvas.DrawText(valorTexto, x + barWidth / 2, labelY, valuePaint);
                    }
                }
            }

            // GRUPO 1: IGI FP-0
            DibujarBarra(x_IGI_FP0, igiPagado0, colorPagado);
            DibujarBarra(x_IGI_FP0 + barWidth + 5, igiCalculado0, colorCalculado);
            DibujarBarra(x_IGI_FP0 + (barWidth + 5) * 2, diferenciaIGI0, colorDiferencia, true);

            // GRUPO 2: IGI FP-5
            DibujarBarra(x_IGI_FP5, igiPagado5, colorPagado);
            DibujarBarra(x_IGI_FP5 + barWidth + 5, igiCalculado5, colorCalculado);
            DibujarBarra(x_IGI_FP5 + (barWidth + 5) * 2, diferenciaIGI5, colorDiferencia, true);

            // GRUPO 3: IVA FP-0
            DibujarBarra(x_IVA_FP0 + barWidth + 5, ivaPagado0, colorIVA0);

            // GRUPO 4: IVA FP-21
            DibujarBarra(x_IVA_FP21 + barWidth + 5, ivaPagado21, colorIVA21);

            // Título
            using (var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 16, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("Comparativa IGI e IVA por Forma de Pago", width / 2, 25, titlePaint);
            }

            // Etiquetas de categoría
            using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 10, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("IGI FP-0", x_IGI_FP0 + (barWidth * 3 + 10) / 2, baseY + 20, labelPaint);
                canvas.DrawText("IGI FP-5", x_IGI_FP5 + (barWidth * 3 + 10) / 2, baseY + 20, labelPaint);
                canvas.DrawText("IVA FP-0", x_IVA_FP0 + (barWidth * 3 + 10) / 2, baseY + 20, labelPaint);
                canvas.DrawText("IVA FP-21", x_IVA_FP21 + (barWidth * 3 + 10) / 2, baseY + 20, labelPaint);
            }

            // Leyenda
            using (var legendPaint = new SKPaint { TextSize = 9, IsAntialias = true })
            {
                float legendX = 100;
                float legendY = baseY + 45;
                float spacing = 120;

                void DibujarLeyenda(float x, SKColor color, string texto)
                {
                    using (var rectPaint = new SKPaint { Color = color, Style = SKPaintStyle.Fill })
                    {
                        canvas.DrawRect(x, legendY, 12, 12, rectPaint);
                    }
                    legendPaint.Color = SKColors.Black;
                    canvas.DrawText(texto, x + 16, legendY + 10, legendPaint);
                }

                DibujarLeyenda(legendX, colorPagado, "IGI Pagado");
                DibujarLeyenda(legendX + spacing, colorCalculado, "IGI Calculado");
                DibujarLeyenda(legendX + spacing * 2, colorDiferencia, "Diferencia");
                DibujarLeyenda(legendX + spacing * 3, colorIVA0, "IVA FP-0");
                DibujarLeyenda(legendX + spacing * 4, colorIVA21, "IVA FP-21");
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
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 8, IsAntialias = true })
            {
                for (int i = 0; i <= 5; i++)
                {
                    decimal valorEje = maxValor * i / 5;
                    float x = leftMargin + (maxBarWidth * i / 5);
                    canvas.DrawLine(x, 50, x, height - 50, gridPaint);
                    string label = valorEje >= 1000000 ? $"{valorEje / 1000000:N0}M"
                                 : valorEje >= 1000 ? $"{valorEje / 1000:N0}K"
                                 : $"{valorEje:N0}";
                    canvas.DrawText(label, x, height - 35, textPaint);
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
                        using (var labelPaint = new SKPaint { Color = SKColors.White, TextSize = 7, IsAntialias = true, FakeBoldText = true })
                        {
                            string texto = datos.Pagado >= 1000 ? $"{datos.Pagado / 1000:N0}K" : $"{datos.Pagado:N0}";
                            float textWidth = labelPaint.MeasureText(texto);
                            if (anchoPagado > textWidth + 5)
                                canvas.DrawText(texto, xInicio + anchoPagado / 2 - textWidth / 2, yPos + barHeight / 2 + 3, labelPaint);
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
                        using (var labelPaint = new SKPaint { Color = new SKColor(64, 64, 64), TextSize = 7, IsAntialias = true, FakeBoldText = true })
                        {
                            string texto = datos.Calculado >= 1000 ? $"{datos.Calculado / 1000:N0}K" : $"{datos.Calculado:N0}";
                            float textWidth = labelPaint.MeasureText(texto);
                            if (anchoCalculado > textWidth + 5)
                                canvas.DrawText(texto, xInicio + anchoCalculado / 2 - textWidth / 2, yPos + barHeight / 2 + 3, labelPaint);
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
                        using (var labelPaint = new SKPaint { Color = new SKColor(64, 64, 64), TextSize = 7, IsAntialias = true, FakeBoldText = true })
                        {
                            string texto = Math.Abs(datos.Diferencia) >= 1000 ? $"{Math.Abs(datos.Diferencia) / 1000:N0}K" : $"{Math.Abs(datos.Diferencia):N0}";
                            float textWidth = labelPaint.MeasureText(texto);
                            if (anchoDiferencia > textWidth + 5)
                                canvas.DrawText(texto, xInicio + anchoDiferencia / 2 - textWidth / 2, yPos + barHeight / 2 + 3, labelPaint);
                        }
                    }

                    // Etiqueta del mes y forma de pago (a la izquierda)
                    using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true })
                    {
                        string etiqueta = $"{mes} FP-{fp}";
                        canvas.DrawText(etiqueta, 5, yPos + barHeight / 2 + 4, labelPaint);
                    }

                    barIndex++;
                }
            }

            // Título
            using (var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("IGI por Mes y Forma de Pago", width / 2, 25, titlePaint);
            }

            // Leyenda
            using (var legendPaint = new SKPaint { TextSize = 9, IsAntialias = true })
            {
                float legendX = leftMargin;
                float legendY = height - 20;

                using (var rectPaint = new SKPaint { Color = colorPagado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                legendPaint.Color = SKColors.Black;
                canvas.DrawText("IGI pagado", legendX + 17, legendY + 10, legendPaint);

                using (var rectPaint = new SKPaint { Color = colorCalculado, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 100, legendY, 12, 12, rectPaint);
                }
                canvas.DrawText("IGI calculado", legendX + 117, legendY + 10, legendPaint);

                using (var rectPaint = new SKPaint { Color = colorDiferencia, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX + 210, legendY, 12, 12, rectPaint);
                }
                canvas.DrawText("Diferencia", legendX + 227, legendY + 10, legendPaint);
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
            using (var textPaint = new SKPaint { Color = SKColors.Gray, TextSize = 8, IsAntialias = true })
            {
                for (int i = 0; i <= 5; i++)
                {
                    decimal valorEje = maxValor * i / 5;
                    float x = leftMargin + (maxBarWidth * i / 5);
                    canvas.DrawLine(x, 50, x, height - 50, gridPaint);
                    string label = valorEje >= 1000000 ? $"{valorEje / 1000000:N0}M"
                                 : valorEje >= 1000 ? $"{valorEje / 1000:N0}K"
                                 : $"{valorEje:N0}";
                    canvas.DrawText(label, x, height - 35, textPaint);
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
                        using (var labelPaint = new SKPaint { Color = SKColors.White, TextSize = 11, IsAntialias = true, FakeBoldText = true })
                        {
                            string texto = datos.IVAPagado >= 1000 ? $"{datos.IVAPagado / 1000:N0}K" : $"{datos.IVAPagado:N0}";
                            float textWidth = labelPaint.MeasureText(texto);
                            if (anchoIVA > textWidth + 5)
                                canvas.DrawText(texto, xInicio + anchoIVA / 2 - textWidth / 2, yPos + barHeight / 2 + 4, labelPaint);
                        }
                    }

                    // Etiqueta del mes y forma de pago (a la izquierda)
                    using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true })
                    {
                        string etiqueta = $"{mes} FP-{fp}";
                        canvas.DrawText(etiqueta, 5, yPos + barHeight / 2 + 4, labelPaint);
                    }

                    barIndex++;
                }
            }

            // Título
            using (var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText("IVA por Mes y Forma de Pago", width / 2, 25, titlePaint);
            }

            // Leyenda
            using (var legendPaint = new SKPaint { TextSize = 9, IsAntialias = true })
            {
                float legendX = leftMargin;
                float legendY = height - 20;

                using (var rectPaint = new SKPaint { Color = colorIVA, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(legendX, legendY, 12, 12, rectPaint);
                }
                legendPaint.Color = SKColors.Black;
                canvas.DrawText("IVA pagado", legendX + 17, legendY + 10, legendPaint);
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
