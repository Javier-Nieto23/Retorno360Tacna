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
        private List<MateriaPrimaBOM> datosConsultadosMP;
        private List<MateriaPrimaBOM> datosConsultadosOtros;
        private Control chartControl; // Control genérico para manejar ambos tipos de gráficas
        private int vistaActual = 0; // 0 = MP, 1 = Otros tipos

        public FrmCatalogoPartes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
            datosConsultadosMP = new List<MateriaPrimaBOM>();
            datosConsultadosOtros = new List<MateriaPrimaBOM>();
            chartControl = chartEstatus; // Inicialmente es PieChart
        }

        private void FrmCatalogoPartes_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            lblTotalPartes.Text = "Total de partes: 0";
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

            string baseDatos = cboBaseDatos.SelectedItem.ToString();
            DateTime fechaInicio = dtpFechaInicio.Value;
            DateTime fechaFin = dtpFechaFin.Value;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                MostrarPanelCargando(true);
                btnConsultar.Enabled = false;

                // Pequeño delay para asegurar que la UI se actualice
                await Task.Delay(50);

                // Ejecutar ambas consultas en paralelo
                var tareaMP = Task.Run(() =>
                    catalogoService.ObtenerMateriaPrimaBOM(baseDatos, "MP", fechaInicio, fechaFin));

                var tareaOtros = Task.Run(() =>
                    catalogoService.ObtenerMateriaPrimaBOMMultiple(baseDatos, fechaInicio, fechaFin));

                await Task.WhenAll(tareaMP, tareaOtros);

                datosConsultadosMP = tareaMP.Result;
                datosConsultadosOtros = tareaOtros.Result;

                // Mostrar vista de MP por defecto
                vistaActual = 0;
                MostrarVistaMP();

                // Habilitar botones de navegación
                btnGraficaIndividual.Enabled = true;
                btnGraficaTodos.Enabled = true;
                btnExportarPdf.Enabled = datosConsultadosMP.Count > 0 || datosConsultadosOtros.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar datos:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MostrarPanelCargando(false);
                btnConsultar.Enabled = true;
            }
        }

        private void MostrarVistaMP()
        {
            vistaActual = 0;

            // Restaurar gráfica de pie
            if (chartControl != chartEstatus)
            {
                panelGrafico.Controls.Remove(chartControl);
                chartControl = chartEstatus;
                panelGrafico.Controls.Add(chartEstatus);
                chartEstatus.Dock = DockStyle.Fill;
                chartEstatus.BringToFront();
            }

            // Actualizar grid
            dgvMateriaPrima.DataSource = datosConsultadosMP;

            if (dgvMateriaPrima.Columns.Count > 0)
            {
                dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
                dgvMateriaPrima.Columns["Clave"].Visible = false;
                dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            // Actualizar gráfico
            ActualizarGrafico(datosConsultadosMP);

            // Actualizar total
            lblTotalPartes.Text = $"Total de partes MP: {datosConsultadosMP.Count:N0}";
        }

        private void MostrarVistaOtros()
        {
            vistaActual = 1;

            // Cambiar a gráfica de barras si es necesario
            if (chartControl == chartEstatus)
            {
                panelGrafico.Controls.Remove(chartEstatus);

                var cartesianChart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
                {
                    Dock = DockStyle.Fill
                };

                panelGrafico.Controls.Add(cartesianChart);
                chartControl = cartesianChart;
                cartesianChart.BringToFront();
            }

            // Actualizar grid
            dgvMateriaPrima.DataSource = datosConsultadosOtros;

            if (dgvMateriaPrima.Columns.Count > 0)
            {
                dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
                dgvMateriaPrima.Columns["Clave"].Visible = true; // Mostrar columna de tipo
                dgvMateriaPrima.Columns["Clave"].HeaderText = "Tipo";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            // Actualizar gráfico de barras
            ActualizarGraficoBarras(datosConsultadosOtros);

            // Actualizar total
            lblTotalPartes.Text = $"Total de partes (EQ, MAQ, SUB, RT): {datosConsultadosOtros.Count:N0}";
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

        private void ActualizarGraficoBarras(List<MateriaPrimaBOM> datos)
        {
            // Agrupar por tipo de clave y contar
            var agrupado = datos.GroupBy(d => d.Clave)
                                .Select(g => new { Tipo = g.Key, Total = g.Count() })
                                .OrderBy(x => x.Tipo)
                                .ToList();

            // Remover el chart actual si existe
            if (chartControl != null)
            {
                panelGrafico.Controls.Remove(chartControl);
            }

            // Crear nuevo CartesianChart para gráfica de barras
            var cartesianChart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                Name = "chartEstatus"
            };

            // Crear series de barras con colores diferentes para cada tipo
            var colors = new[] { "#9b59b6", "#e74c3c", "#3498db", "#2ecc71", "#f39c12" }; // Morado, Rojo, Azul, Verde, Naranja
            var series = new List<ISeries>();

            for (int i = 0; i < agrupado.Count; i++)
            {
                var item = agrupado[i];
                var color = colors[i % colors.Length];

                series.Add(new ColumnSeries<int>
                {
                    Name = item.Tipo,
                    Values = new[] { item.Total },
                    Fill = new SolidColorPaint(SKColor.Parse(color)),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"{item.Total}",
                    MaxBarWidth = 50
                });
            }

            cartesianChart.Series = series;

            // Configurar ejes X
            cartesianChart.XAxes = new LiveChartsCore.SkiaSharpView.Axis[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Labels = agrupado.Select(x => x.Tipo).ToArray(),
                    LabelsRotation = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

            // Configurar ejes Y
            cartesianChart.YAxes = new LiveChartsCore.SkiaSharpView.Axis[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    MinLimit = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

            cartesianChart.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = "Total de Partes por Tipo de Clave",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };

            cartesianChart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            cartesianChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;

            // Agregar al panel
            panelGrafico.Controls.Add(cartesianChart);
            panelGrafico.Controls.SetChildIndex(cartesianChart, 0);
            chartControl = cartesianChart;
        }

        private async void btnExportarPdf_Click(object sender, EventArgs e)
        {
            if (datosConsultadosMP == null || datosConsultadosMP.Count == 0)
            {
                if (datosConsultadosOtros == null || datosConsultadosOtros.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Guardar Catálogo como PDF",
                FileName = $"Catalogo_Partes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    btnExportarPdf.Enabled = false;
                    GenerarPdfCatalogo(saveDialog.FileName);

                    var result = MessageBox.Show("PDF generado correctamente.\n¿Desea abrirlo ahora?",
                        "Exportación Exitosa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar el PDF:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnExportarPdf.Enabled = true;
                }
            }
        }

        private void GenerarPdfCatalogo(string rutaArchivo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Generar imágenes de ambos gráficos
            var vigentesMP = datosConsultadosMP.Count(d => d.EstatusComponente == "VIGENTE EN BOM");
            var noVigentesMP = datosConsultadosMP.Count(d => d.EstatusComponente == "NO ESTA EN BOM");
            byte[] imagenGraficoMP = GenerarImagenGrafico(vigentesMP, noVigentesMP);
            byte[] imagenGraficoBarras = GenerarImagenGraficoBarras(datosConsultadosOtros);

            var agrupado = datosConsultadosOtros.GroupBy(d => d.Clave)
                                      .Select(g => new { Tipo = g.Key, Total = g.Count() })
                                      .OrderBy(x => x.Tipo)
                                      .ToList();

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
                        column.Item().Text("Catálogo de Partes - Reporte Completo")
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
                                txt.Span("Total General: ").Bold();
                                txt.Span((datosConsultadosMP.Count + datosConsultadosOtros.Count).ToString("N0"));
                            });
                        });

                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Content - PRIMERA PÁGINA CON AMBOS GRÁFICOS
                    page.Content().PaddingTop(5).Column(column =>
                    {
                        // AMBOS GRÁFICOS EN LA MISMA PÁGINA - LADO A LADO
                        column.Item().Row(row =>
                        {
                            // Sección Materia Prima - Izquierda
                            row.RelativeItem().Padding(3).Column(seccionMP =>
                            {
                                seccionMP.Item().Text("MATERIA PRIMA (MP)")
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3)
                                    .AlignCenter();

                                seccionMP.Item().PaddingTop(2).PaddingBottom(2).Row(statsRow =>
                                {
                                    statsRow.RelativeItem().AlignCenter().Text(txt =>
                                    {
                                        txt.Span("Total: ").Bold().FontSize(8);
                                        txt.Span($"{datosConsultadosMP.Count:N0}").FontSize(8);
                                    });

                                    statsRow.RelativeItem().AlignCenter().Text(txt =>
                                    {
                                        txt.Span("Vigentes: ").Bold().FontColor(Colors.Green.Darken2).FontSize(8);
                                        txt.Span($"{vigentesMP:N0}").FontSize(8);
                                    });

                                    statsRow.RelativeItem().AlignCenter().Text(txt =>
                                    {
                                        txt.Span("No vigentes: ").Bold().FontColor(Colors.Red.Darken2).FontSize(8);
                                        txt.Span($"{noVigentesMP:N0}").FontSize(8);
                                    });
                                });

                                // Gráfico MP - tamaño fijo reducido
                                seccionMP.Item().PaddingTop(3).AlignCenter().Width(280).Image(imagenGraficoMP);
                            });

                            // Sección Otros Tipos - Derecha
                            row.RelativeItem().Padding(3).Column(seccionOtros =>
                            {
                                seccionOtros.Item().Text("OTROS TIPOS (EQ, MAQ, SUB, RT)")
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3)
                                    .AlignCenter();

                                seccionOtros.Item().PaddingTop(2).PaddingBottom(2).Row(statsRow =>
                                {
                                    statsRow.RelativeItem().AlignCenter().Text(txt =>
                                    {
                                        txt.Span("Total: ").Bold().FontSize(8);
                                        txt.Span($"{datosConsultadosOtros.Count:N0}").FontSize(8);
                                    });

                                    foreach (var grupo in agrupado.Take(3))
                                    {
                                        statsRow.RelativeItem().AlignCenter().Text(txt =>
                                        {
                                            txt.Span($"{grupo.Tipo}: ").Bold().FontColor(Colors.Blue.Darken1).FontSize(8);
                                            txt.Span($"{grupo.Total:N0}").FontSize(8);
                                        });
                                    }
                                });

                                // Segunda fila de estadísticas si hay más de 3 tipos
                                if (agrupado.Count > 3)
                                {
                                    seccionOtros.Item().PaddingBottom(2).Row(statsRow2 =>
                                    {
                                        foreach (var grupo in agrupado.Skip(3))
                                        {
                                            statsRow2.RelativeItem().AlignCenter().Text(txt =>
                                            {
                                                txt.Span($"{grupo.Tipo}: ").Bold().FontColor(Colors.Blue.Darken1).FontSize(8);
                                                txt.Span($"{grupo.Total:N0}").FontSize(8);
                                            });
                                        }
                                    });
                                }

                                // Gráfico Barras - tamaño fijo reducido
                                seccionOtros.Item().PaddingTop(3).AlignCenter().Width(280).Image(imagenGraficoBarras);
                            });
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

                // SEGUNDA PÁGINA - TABLA DE MATERIA PRIMA
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().Text("MATERIA PRIMA (MP) - Detalle")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Total: ").Bold();
                                txt.Span($"{datosConsultadosMP.Count:N0}");
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Vigentes: ").Bold().FontColor(Colors.Green.Darken2);
                                txt.Span($"{vigentesMP:N0}");
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("No vigentes: ").Bold().FontColor(Colors.Red.Darken2);
                                txt.Span($"{noVigentesMP:N0}");
                            });
                        });

                        column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Content - Tabla MP
                    page.Content().PaddingTop(10).Table(table =>
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

                        // Data rows - Ordenados por Número de Parte
                        foreach (var item in datosConsultadosMP.OrderBy(x => x.Par_NoParte))
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_NoParte).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_DescripcionEsp).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_InsercionFecha?.ToString("dd/MM/yyyy") ?? "N/A").FontSize(8);

                            var estatusColor = item.EstatusComponente == "VIGENTE EN BOM" 
                                ? Colors.Green.Darken2 
                                : Colors.Red.Darken2;

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.EstatusComponente).FontColor(estatusColor).Bold().FontSize(8);
                        }
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

                // TERCERA PÁGINA (o más) - TABLA DE OTROS TIPOS
                if (datosConsultadosOtros.Count > 0)
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
                            column.Item().Text("OTROS TIPOS (EQ, MAQ, SUB, RT) - Detalle")
                                .FontSize(16)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text(txt =>
                                {
                                    txt.Span("Total: ").Bold();
                                    txt.Span($"{datosConsultadosOtros.Count:N0}");
                                });

                                foreach (var grupo in agrupado)
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span($"{grupo.Tipo}: ").Bold().FontColor(Colors.Blue.Darken1);
                                        txt.Span($"{grupo.Total:N0}");
                                    });
                                }
                            });

                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });

                        // Content - Tabla Otros
                        page.Content().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);  // Tipo
                                columns.RelativeColumn(2);     // Número de Parte
                                columns.RelativeColumn(3.5f);  // Descripción
                                columns.RelativeColumn(1.5f);  // Fecha Inserción
                                columns.RelativeColumn(1.5f);  // Estatus
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tipo").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                            });

                            // Data rows - Ordenados por Tipo y luego por Número de Parte
                            foreach (var item in datosConsultadosOtros.OrderBy(x => x.Clave).ThenBy(x => x.Par_NoParte))
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Clave).Bold().FontSize(8);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_NoParte).FontSize(8);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_DescripcionEsp).FontSize(8);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_InsercionFecha?.ToString("dd/MM/yyyy") ?? "N/A").FontSize(8);

                                var estatusColor = item.EstatusComponente == "VIGENTE EN BOM" 
                                    ? Colors.Green.Darken2 
                                    : Colors.Red.Darken2;

                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.EstatusComponente).FontColor(estatusColor).Bold().FontSize(8);
                            }
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
                }
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

        private byte[] GenerarImagenGraficoBarras(List<MateriaPrimaBOM> datos)
        {
            int width = 400;
            int height = 350;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            // Agrupar por tipo
            var agrupado = datos.GroupBy(d => d.Clave)
                               .Select(g => new { Tipo = g.Key, Total = g.Count() })
                               .OrderBy(x => x.Tipo)
                               .ToList();

            if (agrupado.Count == 0)
            {
                using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, TextAlign = SKTextAlign.Center, IsAntialias = true })
                {
                    canvas.DrawText("Sin datos", width / 2, height / 2, textPaint);
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }

            // Colores para cada tipo
            SKColor[] colores = new SKColor[]
            {
                SKColor.Parse("#3498db"), // Azul
                SKColor.Parse("#2ecc71"), // Verde
                SKColor.Parse("#e74c3c"), // Rojo
                SKColor.Parse("#f39c12"), // Naranja
                SKColor.Parse("#9b59b6")  // Púrpura
            };

            // Configuración del gráfico
            float marginLeft = 60;
            float marginRight = 30;
            float marginTop = 40;
            float marginBottom = 80;

            float chartWidth = width - marginLeft - marginRight;
            float chartHeight = height - marginTop - marginBottom;

            // Encontrar valor máximo
            int maxValue = agrupado.Max(x => x.Total);

            // Dibujar ejes
            using (var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true })
            {
                // Eje Y
                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + chartHeight, axisPaint);
                // Eje X
                canvas.DrawLine(marginLeft, marginTop + chartHeight, marginLeft + chartWidth, marginTop + chartHeight, axisPaint);
            }

            // Calcular ancho de barras
            float barWidth = chartWidth / agrupado.Count * 0.7f;
            float barSpacing = chartWidth / agrupado.Count;

            // Dibujar barras
            for (int i = 0; i < agrupado.Count; i++)
            {
                var item = agrupado[i];
                float barHeight = (float)item.Total / maxValue * chartHeight;
                float x = marginLeft + (i * barSpacing) + (barSpacing - barWidth) / 2;
                float y = marginTop + chartHeight - barHeight;

                SKColor barColor = colores[i % colores.Length];

                using (var barPaint = new SKPaint { Color = barColor, Style = SKPaintStyle.Fill, IsAntialias = true })
                {
                    canvas.DrawRect(x, y, barWidth, barHeight, barPaint);
                }

                // Dibujar valor encima de la barra
                using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
                {
                    canvas.DrawText(item.Total.ToString(), x + barWidth / 2, y - 5, textPaint);
                }

                // Dibujar etiqueta del tipo
                using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, TextAlign = SKTextAlign.Center, IsAntialias = true })
                {
                    canvas.DrawText(item.Tipo, x + barWidth / 2, marginTop + chartHeight + 20, labelPaint);
                }
            }

            // Dibujar escala del eje Y
            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 10, TextAlign = SKTextAlign.Right, IsAntialias = true })
            {
                int steps = 5;
                for (int i = 0; i <= steps; i++)
                {
                    int value = (maxValue / steps) * i;
                    float y = marginTop + chartHeight - (chartHeight / steps * i);
                    canvas.DrawText(value.ToString(), marginLeft - 10, y + 4, textPaint);
                }
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }

        private void btnGraficaIndividual_Click(object sender, EventArgs e)
        {
            if (datosConsultadosMP == null || datosConsultadosMP.Count == 0)
            {
                MessageBox.Show("Primero debe realizar una consulta.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MostrarVistaMP();
        }

        private void btnGraficaTodos_Click(object sender, EventArgs e)
        {
            if (datosConsultadosOtros == null || datosConsultadosOtros.Count == 0)
            {
                MessageBox.Show("Primero debe realizar una consulta.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MostrarVistaOtros();
        }
    }
}
