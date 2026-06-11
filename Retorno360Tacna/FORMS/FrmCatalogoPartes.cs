using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
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
        private List<MateriaPrimaBOM> datosOtrosFiltrados;
        private readonly LiveChartsCore.SkiaSharpView.WinForms.PieChart chartPedimentosMP;
        private Control chartControl; // Control genérico para manejar ambos tipos de gráficas
        private int vistaActual = 0; // 0 = MP BOM, 1 = Otros tipos, 2 = MP Pedimentos
        public FrmCatalogoPartes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
            datosConsultadosMP = new List<MateriaPrimaBOM>();
            datosConsultadosOtros = new List<MateriaPrimaBOM>();
            datosOtrosFiltrados = new List<MateriaPrimaBOM>();
            chartPedimentosMP = CrearChartPedimentos();
            chartControl = chartEstatus;
            DataGridViewManualCopyHelper.ConfigurarControles(this);
            dgvMateriaPrima.CellDoubleClick += dgvMateriaPrima_CellDoubleClick;
            DataGridViewManualCopyHelper.Configurar(dgvMateriaPrima);
        }

        private LiveChartsCore.SkiaSharpView.WinForms.PieChart CrearChartPedimentos()
        {
            return new LiveChartsCore.SkiaSharpView.WinForms.PieChart
            {
                Dock = DockStyle.Fill,
                InitialRotation = 0D,
                IsClockwise = true,
                MaxAngle = 360D,
                MaxValue = null,
                MinValue = 0D,
                Name = "chartPedimentosMP"
            };
        }

        private void MostrarControlGrafico(Control control)
        {
            if (chartControl == control && panelGrafico.Controls.Contains(control))
            {
                control.BringToFront();
                return;
            }

            if (chartControl != null && panelGrafico.Controls.Contains(chartControl))
            {
                panelGrafico.Controls.Remove(chartControl);
            }

            if (!panelGrafico.Controls.Contains(control))
            {
                panelGrafico.Controls.Add(control);
                panelGrafico.Controls.SetChildIndex(control, 0);
            }

            chartControl = control;
            control.Dock = DockStyle.Fill;
            control.BringToFront();
        }

        private void FrmCatalogoPartes_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            ConfigurarFiltroTipoParte();
            lblTotalPartes.Text = "Total de partes: 0";
        }

        private void ConfigurarFiltroTipoParte()
        {
            cboTipoParte.Items.Clear();
            cboTipoParte.Items.Add("Todos");
            cboTipoParte.Items.AddRange(new object[] { "EQ", "MAQ", "SUB", "RT", "AUX", "PT" });
            cboTipoParte.SelectedIndex = 0;
            cboTipoParte.Enabled = false;
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
                datosOtrosFiltrados = new List<MateriaPrimaBOM>(datosConsultadosOtros);

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
            cboTipoParte.Enabled = false;
            MostrarControlGrafico(chartEstatus);

            // Actualizar grid
            dgvMateriaPrima.DataSource = datosConsultadosMP;

            if (dgvMateriaPrima.Columns.Count > 0)
            {
                dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].HeaderText = "En Pedimento";
                dgvMateriaPrima.Columns["DetallePedimentosInfo"].Visible = false;
                dgvMateriaPrima.Columns["Par_Consecutivo"].Visible = false;
                dgvMateriaPrima.Columns["Clave"].Visible = false;
                dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].MinimumWidth = 80;
            }

            // Actualizar gráfico
            ActualizarGrafico(datosConsultadosMP);

            // Actualizar total
            lblTotalPartes.Text = $"Total de partes MP: {datosConsultadosMP.Count:N0}";
        }

        private void MostrarVistaPedimentosMP()
        {
            vistaActual = 2;
            cboTipoParte.Enabled = false;
            MostrarControlGrafico(chartPedimentosMP);

            dgvMateriaPrima.DataSource = datosConsultadosMP;

            if (dgvMateriaPrima.Columns.Count > 0)
            {
                dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].HeaderText = "En Pedimento";
                dgvMateriaPrima.Columns["DetallePedimentosInfo"].Visible = false;
                dgvMateriaPrima.Columns["Par_Consecutivo"].Visible = false;
                dgvMateriaPrima.Columns["Clave"].Visible = false;
                dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].MinimumWidth = 80;
            }

            ActualizarGraficoPedimentosMP(datosConsultadosMP);
            lblTotalPartes.Text = $"Total de partes MP: {datosConsultadosMP.Count:N0}";
        }

        private void MostrarVistaOtros()
        {
            vistaActual = 1;
            cboTipoParte.Enabled = true;
            AplicarFiltroTipoParte();
        }

        private void AplicarFiltroTipoParte()
        {
            string tipoSeleccionado = cboTipoParte.SelectedItem?.ToString() ?? "Todos";

            datosOtrosFiltrados = tipoSeleccionado == "Todos"
                ? new List<MateriaPrimaBOM>(datosConsultadosOtros)
                : datosConsultadosOtros
                    .Where(x => string.Equals(x.Clave, tipoSeleccionado, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Cambiar a gráfica de barras si es necesario
            if (chartControl == chartEstatus || chartControl == chartPedimentosMP)
            {
                var cartesianChart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
                {
                    Dock = DockStyle.Fill
                };

                MostrarControlGrafico(cartesianChart);
            }

            // Actualizar grid
            dgvMateriaPrima.DataSource = null;
            dgvMateriaPrima.DataSource = datosOtrosFiltrados;

            if (dgvMateriaPrima.Columns.Count > 0)
            {
                dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
                dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
                dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].Visible = false;
                dgvMateriaPrima.Columns["DetallePedimentosInfo"].Visible = false;
                dgvMateriaPrima.Columns["Par_Consecutivo"].Visible = false;
                dgvMateriaPrima.Columns["Clave"].Visible = true; // Mostrar columna de tipo
                dgvMateriaPrima.Columns["Clave"].HeaderText = "Tipo";
                dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            // Actualizar gráfico de barras
            ActualizarGraficoBarras(datosOtrosFiltrados);

            // Actualizar total
            lblTotalPartes.Text = tipoSeleccionado == "Todos"
                ? $"Total de partes (EQ, MAQ, SUB, RT, AUX, PT): {datosOtrosFiltrados.Count:N0}"
                : $"Total de partes {tipoSeleccionado}: {datosOtrosFiltrados.Count:N0}";
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

        private void ActualizarGraficoPedimentosMP(List<MateriaPrimaBOM> datos)
        {
            var enPedimento = datos.Count(d => string.Equals(d.DetallePedimentosGlosa, "SI", StringComparison.OrdinalIgnoreCase));
            var sinPedimento = datos.Count - enPedimento;

            var serieEnPedimento = new PieSeries<int>
            {
                Values = new[] { enPedimento },
                Name = $"En pedimento: {enPedimento}",
                Fill = new SolidColorPaint(SKColor.Parse("#3498db")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{enPedimento}"
            };

            var serieSinPedimento = new PieSeries<int>
            {
                Values = new[] { sinPedimento },
                Name = $"Sin pedimento: {sinPedimento}",
                Fill = new SolidColorPaint(SKColor.Parse("#95a5a6")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{sinPedimento}"
            };

            chartPedimentosMP.Series = new ISeries[] { serieEnPedimento, serieSinPedimento };

            chartPedimentosMP.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = "Partes en Pedimento",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };

            chartPedimentosMP.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartPedimentosMP.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
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
            var enPedimentoMP = datosConsultadosMP.Count(d => string.Equals(d.DetallePedimentosGlosa, "SI", StringComparison.OrdinalIgnoreCase));
            var sinPedimentoMP = datosConsultadosMP.Count - enPedimentoMP;
            byte[] imagenGraficoMP = GenerarImagenGrafico(vigentesMP, noVigentesMP, "Estatus de Componentes en BOM", "Vigentes en BOM", "No vigentes en BOM", "#2ecc71", "#e74c3c");
            byte[] imagenGraficoPedimentosMP = GenerarImagenGrafico(enPedimentoMP, sinPedimentoMP, "Partes en Pedimento", "En pedimento", "Sin pedimento", "#3498db", "#95a5a6");
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
                        // GRÁFICAS DE MP Y OTROS EN LA MISMA PÁGINA
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

                                seccionMP.Item().PaddingTop(3).Row(rowGraficasMP =>
                                {
                                    rowGraficasMP.RelativeItem().AlignCenter().Width(200).Image(imagenGraficoMP);
                                    rowGraficasMP.RelativeItem().AlignCenter().Width(200).Image(imagenGraficoPedimentosMP);
                                });
                            });

                            // Sección Otros Tipos - Derecha
                            row.RelativeItem().Padding(3).Column(seccionOtros =>
                            {
                                seccionOtros.Item().Text("OTROS TIPOS (EQ, MAQ, SUB, RT, AUX, PT)")
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
                            columns.RelativeColumn(4);  // Pedimentos en Glosa
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Pedimentos en Glosa").FontColor(Colors.White).Bold();
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
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.DetallePedimentosGlosa).FontSize(8);
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
                            column.Item().Text("OTROS TIPOS (EQ, MAQ, SUB, RT, AUX, PT) - Detalle")
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
                                columns.RelativeColumn(4);     // Pedimentos en Glosa
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tipo").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Pedimentos en Glosa").FontColor(Colors.White).Bold();
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
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.DetallePedimentosGlosa).FontSize(8);
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

        private byte[] GenerarImagenGrafico(int valorPrimario, int valorSecundario, string titulo, string etiquetaPrimaria, string etiquetaSecundaria, string colorPrimarioHex, string colorSecundarioHex)
        {
            int width = 350;
            int height = 350;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            float centerX = width / 2;
            float centerY = height / 2.2f;
            float radius = Math.Min(width, height) / 3.2f;

            int total = valorPrimario + valorSecundario;
            if (total == 0) total = 1;

            float porcentajePrimario = (float)valorPrimario / total;
            float porcentajeSecundario = (float)valorSecundario / total;

            SKColor colorPrimario = SKColor.Parse(colorPrimarioHex);
            SKColor colorSecundario = SKColor.Parse(colorSecundarioHex);

            float startAngle = -90;
            float sweepAnglePrimario = 360 * porcentajePrimario;

            using (var titlePaint = new SKPaint { Color = SKColor.Parse("#2c3e50"), TextSize = 18, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                canvas.DrawText(titulo, width / 2f, 28, titlePaint);
            }

            // Dibujar sector primario
            using (var paint = new SKPaint { Color = colorPrimario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle, sweepAnglePrimario, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            // Dibujar sector secundario
            float sweepAngleSecundario = 360 * porcentajeSecundario;
            using (var paint = new SKPaint { Color = colorSecundario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY);
                    path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                              startAngle + sweepAnglePrimario, sweepAngleSecundario, false);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }

            // Dibujar etiquetas de valores
            using (var textPaint = new SKPaint { Color = SKColors.White, TextSize = 16, TextAlign = SKTextAlign.Center, IsAntialias = true, FakeBoldText = true })
            {
                if (valorPrimario > 0)
                {
                    float angle1 = (startAngle + sweepAnglePrimario / 2) * (float)Math.PI / 180;
                    float labelX1 = centerX + (radius * 0.6f) * (float)Math.Cos(angle1);
                    float labelY1 = centerY + (radius * 0.6f) * (float)Math.Sin(angle1);
                    canvas.DrawText($"{valorPrimario}", labelX1, labelY1, textPaint);
                }

                if (valorSecundario > 0)
                {
                    float angle2 = (startAngle + sweepAnglePrimario + sweepAngleSecundario / 2) * (float)Math.PI / 180;
                    float labelX2 = centerX + (radius * 0.6f) * (float)Math.Cos(angle2);
                    float labelY2 = centerY + (radius * 0.6f) * (float)Math.Sin(angle2);
                    canvas.DrawText($"{valorSecundario}", labelX2, labelY2, textPaint);
                }
            }

            // Dibujar leyenda
            float legendY = height - 60;
            float legendX = 50;
            float boxSize = 15;

            using (var paint = new SKPaint { Color = colorPrimario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true })
            {
                canvas.DrawText($"{etiquetaPrimaria}: {valorPrimario:N0}", legendX + boxSize + 10, legendY + 12, textPaint);
            }

            legendY += 25;
            using (var paint = new SKPaint { Color = colorSecundario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true })
            {
                canvas.DrawText($"{etiquetaSecundaria}: {valorSecundario:N0}", legendX + boxSize + 10, legendY + 12, textPaint);
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
            if ((vistaActual == 1 || vistaActual == 2) && (datosConsultadosMP == null || datosConsultadosMP.Count == 0))
            {
                MessageBox.Show("Primero debe realizar una consulta.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (vistaActual == 0)
            {
                MostrarVistaOtros();
            }
            else if (vistaActual == 1)
            {
                MostrarVistaPedimentosMP();
            }
            else
            {
                MostrarVistaMP();
            }
        }

        private void btnGraficaTodos_Click(object sender, EventArgs e)
        {
            if ((vistaActual == 0 && (datosConsultadosOtros == null || datosConsultadosOtros.Count == 0))
                || (vistaActual == 1 && (datosConsultadosMP == null || datosConsultadosMP.Count == 0))
                || (vistaActual == 2 && (datosConsultadosMP == null || datosConsultadosMP.Count == 0)))
            {
                MessageBox.Show("Primero debe realizar una consulta.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (vistaActual == 0)
            {
                MostrarVistaPedimentosMP();
            }
            else if (vistaActual == 1)
            {
                MostrarVistaMP();
            }
            else
            {
                MostrarVistaOtros();
            }
        }

        private void cboTipoParte_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (vistaActual == 1)
            {
                AplicarFiltroTipoParte();
            }
        }

        private void dgvMateriaPrima_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dgvMateriaPrima.Rows[e.RowIndex].DataBoundItem is not MateriaPrimaBOM item)
                return;

            if (item.PedimentosRelacionados == null || item.PedimentosRelacionados.Count == 0)
            {
                MessageBox.Show("Este número de parte no tiene pedimentos encontrados en glosa.",
                    "Detalle de pedimentos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tablaDetalle = new DataTable();
            tablaDetalle.Columns.Add("Pedimento", typeof(string));
            tablaDetalle.Columns.Add("Tipo Operación", typeof(string));
            tablaDetalle.Columns.Add("Clave Pedimento", typeof(string));
            tablaDetalle.Columns.Add("Cantidad Igual", typeof(int));

            foreach (var detalle in item.PedimentosRelacionados.OrderBy(x => x.Pedimento))
            {
                tablaDetalle.Rows.Add(detalle.Pedimento, detalle.TipoOperacion, detalle.ClavePedimento, detalle.CantidadPartidasMismaParte);
            }

            using var frmDetalle = CrearVentanaDetallePedimentos(item.Par_NoParte, tablaDetalle);
            frmDetalle.ShowDialog(this);
        }

        private Form CrearVentanaDetallePedimentos(string numeroParte, DataTable tablaDetalle)
        {
            var frmDetalle = new Form
            {
                Text = $"Pedimentos del número de parte: {numeroParte}",
                StartPosition = FormStartPosition.CenterParent,
                Size = new System.Drawing.Size(980, 560),
                MinimumSize = new System.Drawing.Size(820, 420),
                BackColor = System.Drawing.Color.White,
                FormBorderStyle = FormBorderStyle.None,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = System.Drawing.Color.FromArgb(52, 73, 94)
            };

            var lblTituloDetalle = new Label
            {
                Dock = DockStyle.Left,
                Width = 620,
                Text = $"Pedimentos del número de parte: {numeroParte}",
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Padding = new Padding(20, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var panelAcciones = new Panel
            {
                Dock = DockStyle.Right,
                Width = 290
            };

            var btnExportar = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(39, 174, 96),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Exportar",
                Size = new System.Drawing.Size(110, 50),
                Location = new Point(20, 20),
                Cursor = Cursors.Hand
            };

            var btnCerrar = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(231, 76, 60),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Cerrar",
                Size = new System.Drawing.Size(110, 50),
                Location = new Point(150, 20),
                Cursor = Cursors.Hand
            };

            var dgvDetalle = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                DataSource = tablaDetalle
            };

            DataGridViewManualCopyHelper.Configurar(dgvDetalle);

            dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvDetalle.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDetalle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetalle.EnableHeadersVisualStyles = false;
            dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 240, 241);
            dgvDetalle.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            dgvDetalle.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            dgvDetalle.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvDetalle.RowTemplate.Height = 30;

            if (dgvDetalle.Columns.Contains("Cantidad Igual"))
            {
                dgvDetalle.Columns["Cantidad Igual"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvDetalle.Columns["Cantidad Igual"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            var panelFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = System.Drawing.Color.FromArgb(236, 240, 241)
            };

            var lblTotal = new Label
            {
                AutoSize = true,
                Location = new Point(20, 10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(52, 73, 94),
                Text = $"Total registros: {tablaDetalle.Rows.Count:N0}"
            };

            panelAcciones.Controls.Add(btnExportar);
            panelAcciones.Controls.Add(btnCerrar);
            panelHeader.Controls.Add(lblTituloDetalle);
            panelHeader.Controls.Add(panelAcciones);
            panelFooter.Controls.Add(lblTotal);

            btnCerrar.Click += (_, _) => frmDetalle.Close();
            btnExportar.Click += (_, _) => ExportarDetallePedimentos(numeroParte, tablaDetalle);

            frmDetalle.Controls.Add(dgvDetalle);
            frmDetalle.Controls.Add(panelFooter);
            frmDetalle.Controls.Add(panelHeader);

            return frmDetalle;
        }

        private void ExportarDetallePedimentos(string numeroParte, DataTable tablaDetalle)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar detalle de pedimentos",
                    FileName = $"Detalle_Pedimentos_{numeroParte}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Detalle Pedimentos");
                worksheet.Cell(1, 1).InsertTable(tablaDetalle);
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveDialog.FileName);

                MessageBox.Show("Archivo exportado exitosamente.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el detalle: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
