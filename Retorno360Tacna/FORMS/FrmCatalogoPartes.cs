using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmCatalogoPartes : Form
    {
        private readonly ConexionInfo conexionActual;
        private CatalogoPartesService catalogoService;
        private List<MateriaPrimaBOM> datosConsultados;

        public FrmCatalogoPartes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
            datosConsultados = new List<MateriaPrimaBOM>();
        }

        private void FrmCatalogoPartes_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            CargarTiposClave();
            lblTotalPartes.Text = "Total de partes: 0";
        }

        private void CargarTiposClave()
        {
            cboTipoClave.Items.Clear();
            cboTipoClave.Items.Add("MP");
            cboTipoClave.Items.Add("EQ");
            cboTipoClave.Items.Add("MAQ");
            cboTipoClave.Items.Add("SUB");
            cboTipoClave.Items.Add("RT");
            cboTipoClave.SelectedIndex = 0;
        }

        private void CargarRazonesSociales()
        {
            try
            {
                var razones = catalogoService.ObtenerRazonesSociales();

                cboRazonSocial.DataSource = razones;
                cboRazonSocial.DisplayMember = "NombreRazon";
                cboRazonSocial.ValueMember = "IdRazon";

                if (razones.Any())
                {
                    cboRazonSocial.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedValue != null && cboRazonSocial.SelectedValue is int idRazon)
            {
                CargarBasesDatos(idRazon);
            }
        }

        private void CargarBasesDatos(int idRazon)
        {
            try
            {
                List<string> basesDatos = catalogoService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cboBaseDatos.DataSource = basesDatos;
                    cboBaseDatos.Enabled = true;
                    cboBaseDatos.SelectedIndex = -1;
                }
                else
                {
                    cboBaseDatos.DataSource = null;
                    cboBaseDatos.Enabled = false;
                    MessageBox.Show("No se encontraron bases de datos asociadas a esta razón social.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar bases de datos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboBaseDatos.DataSource = null;
                cboBaseDatos.Enabled = false;
            }
        }

        private void MostrarPanelCargando(bool mostrar)
        {
            if (panelCargando.InvokeRequired)
            {
                panelCargando.Invoke(new Action(() => MostrarPanelCargando(mostrar)));
                return;
            }

            if (mostrar)
            {
                // Centrar el panel de carga en el formulario
                panelCargando.Location = new Point(
                    (this.ClientSize.Width - panelCargando.Width) / 2,
                    (this.ClientSize.Height - panelCargando.Height) / 2
                );
                panelCargando.BringToFront();
                panelCargando.Visible = true;

                // Asegurar que la animación del progress bar esté activa
                if (progressBarCargando.Style != ProgressBarStyle.Marquee)
                {
                    progressBarCargando.Style = ProgressBarStyle.Marquee;
                }
            }
            else
            {
                panelCargando.Visible = false;
            }

            // Refrescar la UI
            Application.DoEvents();
        }

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            if (cboBaseDatos.SelectedItem == null)
            {
                MessageBox.Show("Por favor seleccione una base de datos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboTipoClave.SelectedItem == null)
            {
                MessageBox.Show("Por favor seleccione un tipo de clave.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string baseDatos = cboBaseDatos.SelectedItem.ToString();
            string tipoClave = cboTipoClave.SelectedItem.ToString();
            DateTime fechaInicio = dtpFechaInicio.Value;
            DateTime fechaFin = dtpFechaFin.Value;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ConsultarMateriaPrimaAsync(baseDatos, tipoClave, fechaInicio, fechaFin);
        }

        private async Task ConsultarMateriaPrimaAsync(string baseDatos, string tipoClave, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                MostrarPanelCargando(true);
                btnConsultar.Enabled = false;

                // Pequeño delay para asegurar que la UI se actualice
                await Task.Delay(50);

                var resultado = await Task.Run(() =>
                    catalogoService.ObtenerMateriaPrimaBOM(baseDatos, tipoClave, fechaInicio, fechaFin));

                // Guardar los datos consultados para exportar
                datosConsultados = resultado;

                dgvMateriaPrima.DataSource = resultado;

                if (dgvMateriaPrima.Columns.Count > 0)
                {
                    dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                    dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                    dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                    dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";

                    // Ocultar la columna Clave
                    dgvMateriaPrima.Columns["Clave"].Visible = false;

                    dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }

                ActualizarGrafico(resultado);

                // Actualizar el total de partes
                lblTotalPartes.Text = $"Total de partes: {resultado.Count:N0}";

                // Habilitar el botón de exportar PDF
                btnExportarPdf.Enabled = resultado.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar materia prima:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MostrarPanelCargando(false);
                btnConsultar.Enabled = true;
            }
        }

        private void ActualizarGrafico(List<MateriaPrimaBOM> datos)
        {
            var vigentes = datos.Count(d => d.EstatusComponente == "VIGENTE EN BOM");
            var noVigentes = datos.Count(d => d.EstatusComponente == "NO ESTA EN BOM");

            var serieVigentes = new PieSeries<int>
            {
                Values = new[] { vigentes },
                Name = $"Vigentes en BOM: {vigentes}",
                Fill = new SolidColorPaint(SKColor.Parse("#2ecc71")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{vigentes}"
            };

            var serieNoVigentes = new PieSeries<int>
            {
                Values = new[] { noVigentes },
                Name = $"No vigentes en BOM: {noVigentes}",
                Fill = new SolidColorPaint(SKColor.Parse("#e74c3c")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{noVigentes}"
            };

            chartEstatus.Series = new ISeries[] { serieVigentes, serieNoVigentes };

            chartEstatus.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = "Estatus de Componentes en BOM",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };

            chartEstatus.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;

            // Ocultar tooltip para evitar duplicación
            chartEstatus.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        }

        private void btnExportarPdf_Click(object sender, EventArgs e)
        {
            if (datosConsultados == null || datosConsultados.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar. Por favor realice una consulta primero.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveDialog.Title = "Guardar Catálogo de Partes";
                    saveDialog.FileName = $"CatalogoPartes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        MostrarPanelCargando(true);
                        btnExportarPdf.Enabled = false;

                        GenerarPdfCatalogo(saveDialog.FileName);

                        MostrarPanelCargando(false);
                        btnExportarPdf.Enabled = true;

                        MessageBox.Show($"PDF generado exitosamente en:\n{saveDialog.FileName}",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Preguntar si desea abrir el archivo
                        if (MessageBox.Show("¿Desea abrir el archivo PDF?", "Abrir archivo",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarPanelCargando(false);
                btnExportarPdf.Enabled = true;
                MessageBox.Show($"Error al generar el PDF:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarPdfCatalogo(string rutaArchivo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var vigentes = datosConsultados.Count(d => d.EstatusComponente == "VIGENTE EN BOM");
            var noVigentes = datosConsultados.Count(d => d.EstatusComponente == "NO ESTA EN BOM");

            // Generar imagen del gráfico
            byte[] imagenGrafico = GenerarImagenGrafico(vigentes, noVigentes);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().Text("Catálogo de Materia Prima")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Base de Datos: ").Bold();
                                txt.Span(cboBaseDatos.SelectedItem?.ToString() ?? "N/A");
                            });

                            row.RelativeItem().AlignRight().Text(txt =>
                            {
                                txt.Span("Fecha de Generación: ").Bold();
                                txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Rango de Fechas: ").Bold();
                                txt.Span($"{dtpFechaInicio.Value:dd/MM/yyyy} - {dtpFechaFin.Value:dd/MM/yyyy}");
                            });

                            row.RelativeItem().AlignRight().Text(txt =>
                            {
                                txt.Span("Total de Partes: ").Bold();
                                txt.Span(datosConsultados.Count.ToString("N0"));
                            });
                        });

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Vigentes en BOM: ").Bold().FontColor(Colors.Green.Darken2);
                                txt.Span($"{vigentes:N0}");
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("No vigentes en BOM: ").Bold().FontColor(Colors.Red.Darken2);
                                txt.Span($"{noVigentes:N0}");
                            });
                        });

                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Content
                    page.Content().PaddingTop(10).Column(column =>
                    {
                        // Gráfico
                        column.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem(1).Column(col =>
                            {
                                col.Item().AlignCenter().Text("Estatus de Componentes en BOM")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Grey.Darken3);

                                col.Item().PaddingTop(5).AlignCenter().Image(imagenGrafico).FitWidth();
                            });

                            row.RelativeItem(2); // Espacio para la tabla
                        });

                        // Tabla
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);  // Número de Parte
                                columns.RelativeColumn(4);  // Descripción
                                columns.RelativeColumn(1.5f);  // Fecha Inserción
                                columns.RelativeColumn(1.5f);  // Estatus
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                            });

                            // Data rows
                            foreach (var item in datosConsultados)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_NoParte);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_DescripcionEsp);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_InsercionFecha?.ToString("dd/MM/yyyy") ?? "N/A");

                                var estatusColor = item.EstatusComponente == "VIGENTE EN BOM" 
                                    ? Colors.Green.Darken2 
                                    : Colors.Red.Darken2;

                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.EstatusComponente).FontColor(estatusColor).Bold();
                            }
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Página ");
                        txt.CurrentPageNumber();
                        txt.Span(" de ");
                        txt.TotalPages();
                    });
                });
            }).GeneratePdf(rutaArchivo);
        }

        private byte[] GenerarImagenGrafico(int vigentes, int noVigentes)
        {
            int width = 350;
            int height = 350;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            float centerX = width / 2;
            float centerY = height / 2.2f;
            float radius = Math.Min(width, height) / 3.2f;

            int total = vigentes + noVigentes;
            if (total == 0) total = 1;

            float vigentesPorcentaje = (float)vigentes / total;
            float noVigentesPorcentaje = (float)noVigentes / total;

            SKColor colorVigentes = SKColor.Parse("#2ecc71");
            SKColor colorNoVigentes = SKColor.Parse("#e74c3c");

            float startAngle = -90;
            float sweepAngleVigentes = 360 * vigentesPorcentaje;

            // Dibujar sector de vigentes
            using (var paint = new SKPaint { Color = colorVigentes, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle, sweepAngleVigentes, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            // Dibujar sector de no vigentes
            float sweepAngleNoVigentes = 360 * noVigentesPorcentaje;
            using (var paint = new SKPaint { Color = colorNoVigentes, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle + sweepAngleVigentes, sweepAngleNoVigentes, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            // Dibujar etiquetas de valores
            using (var textPaint = new SKPaint { Color = SKColors.White, TextSize = 16, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                if (vigentes > 0)
                {
                    float angle1 = (startAngle + sweepAngleVigentes / 2) * (float)Math.PI / 180;
                    float labelX1 = centerX + (radius * 0.6f) * (float)Math.Cos(angle1);
                    float labelY1 = centerY + (radius * 0.6f) * (float)Math.Sin(angle1);
                    canvas.DrawText($"{vigentes}", labelX1, labelY1, textPaint);
                }

                if (noVigentes > 0)
                {
                    float angle2 = (startAngle + sweepAngleVigentes + sweepAngleNoVigentes / 2) * (float)Math.PI / 180;
                    float labelX2 = centerX + (radius * 0.6f) * (float)Math.Cos(angle2);
                    float labelY2 = centerY + (radius * 0.6f) * (float)Math.Sin(angle2);
                    canvas.DrawText($"{noVigentes}", labelX2, labelY2, textPaint);
                }
            }

            // Dibujar leyenda
            float legendY = height - 60;
            float legendX = 50;
            float boxSize = 15;

            using (var paint = new SKPaint { Color = colorVigentes, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true })
            {
                canvas.DrawText($"Vigentes en BOM: {vigentes:N0}", legendX + boxSize + 10, legendY + 12, textPaint);
            }

            legendY += 25;
            using (var paint = new SKPaint { Color = colorNoVigentes, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true })
            {
                canvas.DrawText($"No vigentes en BOM: {noVigentes:N0}", legendX + boxSize + 10, legendY + 12, textPaint);
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }
}
