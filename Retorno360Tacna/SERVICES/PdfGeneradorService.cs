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
                                    .FontColor(resumen.DiferenciaTotal > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2).Bold();

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
                                            .FontColor(item.DiferenciaIGI != 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);

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
