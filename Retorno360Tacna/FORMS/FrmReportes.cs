using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.WinForms;
using System.Diagnostics;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportes : Form
    {
        private readonly ConexionInfo conexionActual;
        private readonly ReporteIGIService reporteService;
        private List<RazonSocial> razonesSociales = new();
        private List<ReporteIGIPagado> reporteActual = new();
        private CartesianChart? chartIGI;

        public FrmReportes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            reporteService = new ReporteIGIService(conexion);

            // Inicializar gráfica
            InicializarGrafica();

            // Configurar eventos de redimensionamiento
            this.Load += FrmReportes_Load;
            this.Resize += FrmReportes_Resize;
            this.SizeChanged += FrmReportes_SizeChanged;
        }

        private void FrmReportes_Resize(object sender, EventArgs e)
        {
            AjustarControles();
        }

        private void FrmReportes_SizeChanged(object sender, EventArgs e)
        {
            AjustarControles();
        }

        private void AjustarControles()
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;

            try
            {
                this.SuspendLayout();

                // El panelResumen ahora usa Dock = DockStyle.Bottom
                // por lo que no necesitamos ajustar su posición manualmente

                // Ajustar la gráfica interna si existe
                if (chartIGI != null && panelGrafica != null)
                {
                    chartIGI.Width = panelGrafica.Width - 20;
                    chartIGI.Height = panelGrafica.Height - lblTituloGrafica.Height - 30;
                }

                this.ResumeLayout(true);
            }
            catch
            {
                // Evitar errores durante el redimensionamiento
            }
        }

        private void FrmReportes_Load(object sender, EventArgs e)
        {
            // Configurar fechas por defecto (mes actual)
            dtpFechaInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpFechaFin.Value = DateTime.Now;

            // Cargar razones sociales
            CargarRazonesSociales();

            // Configurar DataGridView
            ConfigurarDataGridView();

            // Deshabilitar botón PDF al inicio
            btnGenerarPDF.Enabled = false;
        }

        private async void CargarRazonesSociales()
        {
            try
            {
                lblProgreso.Text = "Cargando razones sociales...";

                await Task.Run(() =>
                {
                    razonesSociales = reporteService.ObtenerRazonesSociales();
                });

                cmbRazonSocial.DataSource = razonesSociales;
                cmbRazonSocial.DisplayMember = "NombreRazon";
                cmbRazonSocial.ValueMember = "IdRazon";
                cmbRazonSocial.SelectedIndex = -1;

                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;

                lblProgreso.Text = $"{razonesSociales.Count} razones sociales cargadas";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar razones sociales:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                lblProgreso.Text = "Error al cargar razones sociales";
            }
        }

        private void cmbRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedIndex == -1)
            {
                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
                return;
            }

            if (cmbRazonSocial.SelectedItem is not RazonSocial razonSeleccionada)
                return;

            // Si el checkbox está activado, no cargar bases de datos
            if (!chkSinGlosa.Checked)
            {
                CargarBasesDatosRazon(razonSeleccionada.IdRazon);
            }
            else
            {
                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
            }
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                lblProgreso.Text = "Cargando clientes...";
                cmbCliente.Enabled = false;
                cmbCliente.DataSource = null;

                var basesDatos = reporteService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cmbCliente.DataSource = basesDatos;
                    cmbCliente.Enabled = true;
                    cmbCliente.SelectedIndex = -1;
                    lblProgreso.Text = $"{basesDatos.Count} clientes encontrados";
                }
                else
                {
                    cmbCliente.DataSource = null;
                    cmbCliente.Enabled = false;
                    lblProgreso.Text = "No se encontraron clientes para esta razón social";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar clientes:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
                lblProgreso.Text = "Error al cargar clientes";
            }
        }

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (cmbRazonSocial.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una razón social", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Solo validar cliente si NO está usando el modo sin validación de glosa
            if (!chkSinGlosa.Checked && cmbCliente.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un cliente", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpFechaInicio.Value > dtpFechaFin.Value)
            {
                MessageBox.Show("La fecha inicial no puede ser mayor a la fecha final", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await GenerarReporte();
        }

        private async Task GenerarReporte()
        {
            try
            {
                // Mostrar indicador de carga
                MostrarPanelCargando(true);

                // Deshabilitar controles
                btnConsultar.Enabled = false;
                cmbRazonSocial.Enabled = false;
                cmbCliente.Enabled = false;
                dtpFechaInicio.Enabled = false;
                dtpFechaFin.Enabled = false;
                chkSinGlosa.Enabled = false;

                // Limpiar resultados anteriores
                dgvReporte.DataSource = null;
                reporteActual.Clear();

                DateTime fechaInicio = dtpFechaInicio.Value.Date;
                DateTime fechaFin = dtpFechaFin.Value.Date;
                bool sinValidacionGlosa = chkSinGlosa.Checked;

                if (sinValidacionGlosa)
                {
                    // Consultar todas las bases de datos de la razón social
                    var razonSeleccionada = (RazonSocial)cmbRazonSocial.SelectedItem;
                    lblProgreso.Text = $"Consultando todas las bases de {razonSeleccionada.NombreRazon} para generar reporte...";
                    lblResumenInfo.Text = "Generando reporte...";

                    // Ejecutar consulta en background
                    reporteActual = await Task.Run(() =>
                        reporteService.GenerarReporteIGIPorRazonSocial(razonSeleccionada.IdRazon, fechaInicio, fechaFin)
                    );
                }
                else
                {
                    // Consultar una base de datos específica CON validación
                    string baseDatos = cmbCliente.SelectedItem?.ToString() ?? string.Empty;
                    lblProgreso.Text = $"Consultando {baseDatos} para generar reporte...";
                    lblResumenInfo.Text = "Generando reporte...";

                    // Ejecutar consulta en background
                    reporteActual = await Task.Run(() =>
                        reporteService.GenerarReporteIGI(baseDatos, fechaInicio, fechaFin, false)
                    );
                }

                // Mostrar resultados
                MostrarResultados();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar el reporte:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                lblProgreso.Text = "Error al generar reporte";
                lblResumenInfo.Text = "Error en la consulta";
            }
            finally
            {
                // Ocultar indicador de carga
                MostrarPanelCargando(false);

                // Rehabilitar controles
                btnConsultar.Enabled = true;
                cmbRazonSocial.Enabled = true;
                cmbCliente.Enabled = true;
                dtpFechaInicio.Enabled = true;
                dtpFechaFin.Enabled = true;
                chkSinGlosa.Enabled = true;
            }
        }

        private void MostrarPanelCargando(bool mostrar)
        {
            panelCargando.Visible = mostrar;
            if (mostrar)
            {
                // Centrar el panel en el formulario
                panelCargando.Left = (this.ClientSize.Width - panelCargando.Width) / 2;
                panelCargando.Top = (this.ClientSize.Height - panelCargando.Height) / 2;
                panelCargando.BringToFront();
            }
        }

        private void MostrarResultados()
        {
            if (!reporteActual.Any())
            {
                lblProgreso.Text = "No se encontraron registros";
                lblResumenInfo.Text = "Sin resultados para los filtros seleccionados";
                btnGenerarPDF.Enabled = false;
                return;
            }

            // Convertir a DataTable y mostrar
            var dataTable = reporteService.ConvertirADataTable(reporteActual);
            dgvReporte.DataSource = dataTable;

            // Formatear columnas
            FormatearColumnas();

            // Generar resumen
            var resumen = reporteService.GenerarResumen(reporteActual);
            MostrarResumen(resumen);

            lblProgreso.Text = $"Consulta completada: {reporteActual.Count} registros encontrados";
            btnGenerarPDF.Enabled = true;
        }

        private void chkSinGlosa_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSinGlosa.Checked)
            {
                // Deshabilitar combo de clientes
                cmbCliente.Enabled = false;
                cmbCliente.SelectedIndex = -1;
            }
            else
            {
                // Reactivar y cargar clientes si hay razón social seleccionada
                if (cmbRazonSocial.SelectedIndex != -1 && cmbRazonSocial.SelectedItem is RazonSocial razon)
                {
                    CargarBasesDatosRazon(razon.IdRazon);
                }
            }
        }

        private void ConfigurarDataGridView()
        {
            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvReporte.AllowUserToAddRows = false;
            dgvReporte.AllowUserToDeleteRows = false;
            dgvReporte.ReadOnly = true;
            dgvReporte.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporte.MultiSelect = false;
            dgvReporte.RowHeadersVisible = false;
            dgvReporte.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);
        }

        private void FormatearColumnas()
        {
            if (dgvReporte.Columns.Count == 0)
                return;

            // Ocultar columnas no deseadas
            if (dgvReporte.Columns["Base Datos"] != null)
                dgvReporte.Columns["Base Datos"].Visible = false;

            if (dgvReporte.Columns["Estatus Glosa"] != null)
                dgvReporte.Columns["Estatus Glosa"].Visible = false;

            // Formatear columnas de moneda
            if (dgvReporte.Columns["IGI Pagado"] != null)
                dgvReporte.Columns["IGI Pagado"].DefaultCellStyle.Format = "C2";

            if (dgvReporte.Columns["IGI Calculado"] != null)
                dgvReporte.Columns["IGI Calculado"].DefaultCellStyle.Format = "C2";

            if (dgvReporte.Columns["Diferencia IGI"] != null)
            {
                dgvReporte.Columns["Diferencia IGI"].DefaultCellStyle.Format = "C2";
                dgvReporte.Columns["Diferencia IGI"].DefaultCellStyle.ForeColor = Color.FromArgb(192, 57, 43);
            }

            if (dgvReporte.Columns["IVA Pagado"] != null)
                dgvReporte.Columns["IVA Pagado"].DefaultCellStyle.Format = "C2";

            // Formatear fecha
            if (dgvReporte.Columns["Fecha Pago"] != null)
                dgvReporte.Columns["Fecha Pago"].DefaultCellStyle.Format = "dd/MM/yyyy";

            // Ajustar anchos
            if (dgvReporte.Columns["Pedimento"] != null)
                dgvReporte.Columns["Pedimento"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void MostrarResumen(ResumenIGI resumen)
        {
            lblResumenInfo.Text = $"📊 Total Pedimentos: {resumen.TotalPedimentos} | " +
                                  $"💰 IGI Pagado: {resumen.TotalIGI_Pagado:C2} | " +
                                  $"🧮 IGI Calculado: {resumen.TotalIGI_Calculado:C2} | " +
                                  $"📈 Diferencia: {resumen.DiferenciaTotal:C2} | " +
                                  $"💵 IVA Pagado: {resumen.TotalIVA_Pagado:C2}";

            // Actualizar gráfica
            ActualizarGrafica(resumen);
        }

        private void InicializarGrafica()
        {
            // Crear control de gráfica
            chartIGI = new CartesianChart
            {
                Dock = DockStyle.Fill,
                Location = new Point(10, 50),
                Size = new Size(380, 350)
            };

            // Agregar al panel de gráfica (después del título)
            panelGrafica.Controls.Add(chartIGI);
            chartIGI.BringToFront();

            // Configuración inicial vacía
            chartIGI.Series = Array.Empty<ISeries>();
            chartIGI.XAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { "IGI Pagado", "IGI Calculado" },
                    LabelsRotation = 0,
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200))
                }
            };

            chartIGI.YAxes = new[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    Labeler = value => value.ToString("C0")
                }
            };
        }

        private void ActualizarGrafica(ResumenIGI resumen)
        {
            if (chartIGI == null) return;

            // Crear series de barras
            var series = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "IGI Pagado",
                    Values = new[] { resumen.TotalIGI_Pagado },
                    Fill = new SolidColorPaint(new SKColor(52, 152, 219)),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                },
                new ColumnSeries<decimal>
                {
                    Name = "IGI Calculado",
                    Values = new[] { resumen.TotalIGI_Calculado },
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                }
            };

            chartIGI.Series = series;
        }

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            if (!reporteActual.Any())
            {
                MessageBox.Show(
                    "No hay datos de reporte para exportar.\nPor favor, genere un reporte primero.",
                    "Sin Datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    string nombreArchivo = $"Reporte_IGI_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    saveDialog.Filter = "Archivos PDF|*.pdf";
                    saveDialog.Title = "Guardar Reporte IGI en PDF";
                    saveDialog.FileName = nombreArchivo;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        btnGenerarPDF.Enabled = false;
                        lblProgreso.Text = "Generando PDF...";

                        var razonSocial = cmbRazonSocial.SelectedItem as RazonSocial;
                        string nombreRazon = razonSocial?.NombreRazon ?? "N/A";

                        string baseDatos = string.Empty;
                        if (!chkSinGlosa.Checked && cmbCliente.SelectedItem != null)
                        {
                            baseDatos = cmbCliente.SelectedItem.ToString();
                        }
                        else if (chkSinGlosa.Checked)
                        {
                            var basesDatos = reporteActual
                                .Select(r => r.BaseDatos)
                                .Distinct()
                                .OrderBy(b => b)
                                .ToList();
                            baseDatos = string.Join(", ", basesDatos);
                        }

                        var resumen = reporteService.GenerarResumen(reporteActual);

                        var pdfService = new PdfGeneradorService();
                        pdfService.GenerarReporteIGIPDF(
                            reporteActual,
                            resumen,
                            nombreRazon,
                            baseDatos,
                            dtpFechaInicio.Value,
                            dtpFechaFin.Value,
                            saveDialog.FileName
                        );

                        lblProgreso.Text = "PDF generado exitosamente";

                        var result = MessageBox.Show(
                            $"El archivo PDF se ha generado correctamente.\n\n¿Desea abrir el archivo?",
                            "Éxito",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information
                        );

                        if (result == DialogResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
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
                MessageBox.Show(
                    $"Error al generar el PDF:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                lblProgreso.Text = "Error al generar PDF";
            }
            finally
            {
                btnGenerarPDF.Enabled = reporteActual.Any();
            }
        }


    }
}
