using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.WinForms;
using System.Diagnostics;
using ClosedXML.Excel;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportes : Form
    {
        private readonly ConexionInfo conexionActual;
        private readonly ReporteIGIService reporteService;
        private List<RazonSocial> razonesSociales = new();
        private List<ReporteIGIPagado> reporteActual = new();
        private CartesianChart? chartIGI;
        private CartesianChart? chartIVA;
        private int graficaActual = 0; // 0 = IGI, 1 = IVA
        private bool modalAbierto = false;
        private int ultimaFilaClickeada = -1;

        public FrmReportes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            reporteService = new ReporteIGIService(conexion);

            // Inicializar gráfica
            InicializarGrafica();
            InicializarGraficaIVA();

            // Configurar tooltips para botones de navegación
            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnAnteriorGrafica, "Gráfica anterior (IGI ⟷ IVA)");
            tooltip.SetToolTip(btnSiguienteGrafica, "Gráfica siguiente (IGI ⟷ IVA)");
            tooltip.SetToolTip(btnAnteriorGraficaIVA, "Gráfica anterior (IGI ⟷ IVA)");
            tooltip.SetToolTip(btnSiguienteGraficaIVA, "Gráfica siguiente (IGI ⟷ IVA)");

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

                // chartIGI usa Dock.Fill, se ajustará automáticamente

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
            btnExportarExcel.Enabled = false;
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
                dgvReporteIGI.DataSource = null;
                dgvReporteIVA.DataSource = null;
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
                btnExportarExcel.Enabled = false;
                return;
            }

            // Convertir a DataTable separados: uno para IGI y otro para IVA
            var dataTableIGI = reporteService.ConvertirADataTableIGI(reporteActual);
            var dataTableIVA = reporteService.ConvertirADataTableIVA(reporteActual);

            dgvReporteIGI.DataSource = dataTableIGI;
            dgvReporteIVA.DataSource = dataTableIVA;

            // Formatear columnas para cada grid
            FormatearGridIGI();
            FormatearGridIVA();

            // Generar resumen
            var resumen = reporteService.GenerarResumen(reporteActual);
            MostrarResumenPorFormaPago(resumen);

            lblProgreso.Text = $"Consulta completada: {reporteActual.Count} registros encontrados";
            btnGenerarPDF.Enabled = true;
            btnExportarExcel.Enabled = true;
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
            // Configurar grid IGI
            dgvReporteIGI.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvReporteIGI.AllowUserToAddRows = false;
            dgvReporteIGI.AllowUserToDeleteRows = false;
            dgvReporteIGI.ReadOnly = true;
            dgvReporteIGI.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporteIGI.MultiSelect = false;
            dgvReporteIGI.RowHeadersVisible = false;
            dgvReporteIGI.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);

            // Configurar grid IVA
            dgvReporteIVA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvReporteIVA.AllowUserToAddRows = false;
            dgvReporteIVA.AllowUserToDeleteRows = false;
            dgvReporteIVA.ReadOnly = true;
            dgvReporteIVA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporteIVA.MultiSelect = false;
            dgvReporteIVA.RowHeadersVisible = false;
            dgvReporteIVA.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);

            // Registrar eventos de doble clic para abrir detalle de pedimentos
            dgvReporteIGI.CellDoubleClick += DgvReporteIGI_CellDoubleClick;
            dgvReporteIVA.CellDoubleClick += DgvReporteIVA_CellDoubleClick;
        }

        private void FormatearGridIGI()
        {
            if (dgvReporteIGI.Columns.Count == 0)
                return;

            // Título del header
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.Font = new Font(dgvReporteIGI.Font.FontFamily, 10, FontStyle.Bold);
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvReporteIGI.EnableHeadersVisualStyles = false;

            // Formatear columnas de moneda
            if (dgvReporteIGI.Columns["IGI PAGADO"] != null)
                dgvReporteIGI.Columns["IGI PAGADO"].DefaultCellStyle.Format = "C2";

            if (dgvReporteIGI.Columns["IGI CALCULADO"] != null)
                dgvReporteIGI.Columns["IGI CALCULADO"].DefaultCellStyle.Format = "C2";

            if (dgvReporteIGI.Columns["DIFERENCIA"] != null)
            {
                dgvReporteIGI.Columns["DIFERENCIA"].DefaultCellStyle.Format = "C2";
                dgvReporteIGI.Columns["DIFERENCIA"].DefaultCellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                dgvReporteIGI.Columns["DIFERENCIA"].DefaultCellStyle.Font = new Font(dgvReporteIGI.Font.FontFamily, 9, FontStyle.Bold);
            }

            // Configurar ancho de columnas
            if (dgvReporteIGI.Columns["MES"] != null)
            {
                dgvReporteIGI.Columns["MES"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvReporteIGI.Columns["MES"].MinimumWidth = 120;
            }

            if (dgvReporteIGI.Columns["FORMA DE PAGO IGI"] != null)
            {
                dgvReporteIGI.Columns["FORMA DE PAGO IGI"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvReporteIGI.Columns["FORMA DE PAGO IGI"].MinimumWidth = 100;
            }
        }

        private void FormatearGridIVA()
        {
            if (dgvReporteIVA.Columns.Count == 0)
                return;

            // Título del header
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.Font = new Font(dgvReporteIVA.Font.FontFamily, 10, FontStyle.Bold);
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvReporteIVA.EnableHeadersVisualStyles = false;

            // Formatear columnas de moneda
            if (dgvReporteIVA.Columns["IVA PAGADO"] != null)
                dgvReporteIVA.Columns["IVA PAGADO"].DefaultCellStyle.Format = "C2";

            // Configurar ancho de columnas
            if (dgvReporteIVA.Columns["MES"] != null)
            {
                dgvReporteIVA.Columns["MES"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvReporteIVA.Columns["MES"].MinimumWidth = 120;
            }

            if (dgvReporteIVA.Columns["FORMA DE PAGO IVA"] != null)
            {
                dgvReporteIVA.Columns["FORMA DE PAGO IVA"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvReporteIVA.Columns["FORMA DE PAGO IVA"].MinimumWidth = 100;
            }
        }

        private void DgvReporteIGI_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Validar índice de fila válido
            if (e.RowIndex < 0) return;

            // Si ya hay un modal abierto, ignorar completamente
            if (modalAbierto) return;

            // Si es la misma fila que ya fue procesada, ignorar para evitar múltiples aperturas
            if (ultimaFilaClickeada == e.RowIndex) return;

            try
            {
                // Marcar como procesando
                modalAbierto = true;
                ultimaFilaClickeada = e.RowIndex;

                // Obtener datos de la fila seleccionada
                DataGridViewRow row = dgvReporteIGI.Rows[e.RowIndex];

                string mes = row.Cells["MES"]?.Value?.ToString() ?? "";
                string formaPago = row.Cells["FORMA DE PAGO IGI"]?.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(mes) || string.IsNullOrEmpty(formaPago))
                {
                    MessageBox.Show("No se pudo obtener la información del mes o forma de pago.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Abrir formulario de detalle
                var frmDetalle = new FrmDetallePedimentos(reporteActual, mes, formaPago, "IGI");
                frmDetalle.ShowDialog(this);
                frmDetalle.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar el detalle: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Resetear después de un pequeño delay para evitar clics duplicados inmediatos
                Task.Delay(300).ContinueWith(_ =>
                {
                    this.Invoke(() =>
                    {
                        modalAbierto = false;
                        ultimaFilaClickeada = -1;
                    });
                });
            }
        }

        private void DgvReporteIVA_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Validar índice de fila válido
            if (e.RowIndex < 0) return;

            // Si ya hay un modal abierto, ignorar completamente
            if (modalAbierto) return;

            // Si es la misma fila que ya fue procesada, ignorar para evitar múltiples aperturas
            if (ultimaFilaClickeada == e.RowIndex) return;

            try
            {
                // Marcar como procesando
                modalAbierto = true;
                ultimaFilaClickeada = e.RowIndex;

                // Obtener datos de la fila seleccionada
                DataGridViewRow row = dgvReporteIVA.Rows[e.RowIndex];

                string mes = row.Cells["MES"]?.Value?.ToString() ?? "";
                string formaPago = row.Cells["FORMA DE PAGO IVA"]?.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(mes) || string.IsNullOrEmpty(formaPago))
                {
                    MessageBox.Show("No se pudo obtener la información del mes o forma de pago.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Abrir formulario de detalle
                var frmDetalle = new FrmDetallePedimentos(reporteActual, mes, formaPago, "IVA");
                frmDetalle.ShowDialog(this);
                frmDetalle.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar el detalle: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Resetear después de un pequeño delay para evitar clics duplicados inmediatos
                Task.Delay(300).ContinueWith(_ =>
                {
                    this.Invoke(() =>
                    {
                        modalAbierto = false;
                        ultimaFilaClickeada = -1;
                    });
                });
            }
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

        private void MostrarResumenPorFormaPago(ResumenIGI resumen)
        {
            // Calcular totales separados por forma de pago IGI (0 y 5)
            var reportesIGI_FormaPago5 = reporteActual.Where(r => r.FormaPago_IGI == "5").ToList();
            var reportesIGI_FormaPago0 = reporteActual.Where(r => r.FormaPago_IGI == "0").ToList();

            var totalIGI_Pagado5 = reportesIGI_FormaPago5.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado5 = reportesIGI_FormaPago5.Sum(r => r.IGI_Calculado);
            var diferenciaIGI_5 = totalIGI_Calculado5 - totalIGI_Pagado5;

            var totalIGI_Pagado0 = reportesIGI_FormaPago0.Sum(r => r.IGI_Pagado);
            var totalIGI_Calculado0 = reportesIGI_FormaPago0.Sum(r => r.IGI_Calculado);
            var diferenciaIGI_0 = totalIGI_Calculado0 - totalIGI_Pagado0;

            // Calcular totales separados por forma de pago IVA (0 y 21)
            var reportesIVA_FormaPago21 = reporteActual.Where(r => r.FormaPago_IVA == "21").ToList();
            var reportesIVA_FormaPago0 = reporteActual.Where(r => r.FormaPago_IVA == "0").ToList();

            var totalIVA_Pagado21 = reportesIVA_FormaPago21.Sum(r => r.IVA_Pagado);
            var totalIVA_Pagado0 = reportesIVA_FormaPago0.Sum(r => r.IVA_Pagado);

            // Formato estructurado con alineación
            string linea1 = $"📊 Total: {resumen.TotalPedimentos} registros";
            string separador = new string('─', 100);

            string lineaIGI_FP5 = $"💳 IGI FP-5:   Pagado: {totalIGI_Pagado5,15:C2}  |  Calculado: {totalIGI_Calculado5,15:C2}  |  Diferencia: {diferenciaIGI_5,15:C2}";
            string lineaIGI_FP0 = $"💰 IGI FP-0:   Pagado: {totalIGI_Pagado0,15:C2}  |  Calculado: {totalIGI_Calculado0,15:C2}  |  Diferencia: {diferenciaIGI_0,15:C2}";

            string lineaIVA_FP21 = $"💵 IVA FP-21:  Pagado: {totalIVA_Pagado21,15:C2}";
            string lineaIVA_FP0 = $"💵 IVA FP-0:   Pagado: {totalIVA_Pagado0,15:C2}";

            lblResumenInfo.Text = $"{linea1}\n{separador}\n{lineaIGI_FP5}\n{lineaIGI_FP0}\n{separador}\n{lineaIVA_FP21}\n{lineaIVA_FP0}";

            // Actualizar gráfica IGI con datos por forma de pago
            ActualizarGraficaPorFormaPago(
                totalIGI_Pagado5, totalIGI_Calculado5, diferenciaIGI_5,
                totalIGI_Pagado0, totalIGI_Calculado0, diferenciaIGI_0,
                totalIVA_Pagado21, totalIVA_Pagado0
            );

            // Actualizar gráfica IVA
            ActualizarGraficaIVAPorFormaPago();
        }

        private void InicializarGrafica()
        {
            // Crear control de gráfica
            chartIGI = new CartesianChart
            {
                Dock = DockStyle.Fill,
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top
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
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    MinLimit = null,
                    MaxLimit = null
                }
            };

            chartIGI.YAxes = new[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    Labeler = value => value.ToString("C0"),
                    MinLimit = null,
                    MaxLimit = null
                }
            };
        }

        private void InicializarGraficaIVA()
        {
            // Crear control de gráfica IVA
            chartIVA = new CartesianChart
            {
                Dock = DockStyle.Fill,
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top
            };

            // Agregar al panel de gráfica IVA (después del título)
            panelGraficaIVA.Controls.Add(chartIVA);
            chartIVA.BringToFront();

            // Configuración inicial vacía
            chartIVA.Series = Array.Empty<ISeries>();
            chartIVA.XAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { "IVA Pagado" },
                    LabelsRotation = 0,
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    MinLimit = null,
                    MaxLimit = null
                }
            };

            chartIVA.YAxes = new[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    Labeler = value => value.ToString("C0"),
                    MinLimit = null,
                    MaxLimit = null
                }
            };
        }

        private void ActualizarGrafica(ResumenIGI resumen)
        {
            if (chartIGI == null) return;

            // Calcular la diferencia (ahorro: calculado - pagado)
            // Valor positivo = ahorro (se pagó menos de lo calculado)
            decimal diferencia = resumen.TotalIGI_Calculado - resumen.TotalIGI_Pagado;

            // Crear series de barras
            var series = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "IGI Pagado",
                    Values = new[] { resumen.TotalIGI_Pagado },
                    Fill = new SolidColorPaint(new SKColor(79, 129, 189)), // Azul
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                },
                new ColumnSeries<decimal>
                {
                    Name = "Diferencia",
                    Values = new[] { diferencia },
                    Fill = new SolidColorPaint(new SKColor(155, 194, 230)), // Azul claro
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                }
            };

            chartIGI.Series = series;
        }

        private void ActualizarGraficaPorFormaPago(
            decimal igiPagado5, decimal igiCalculado5, decimal diferenciaIGI5,
            decimal igiPagado0, decimal igiCalculado0, decimal diferenciaIGI0,
            decimal ivaPagado21, decimal ivaPagado0)
        {
            if (chartIGI == null) return;

            try
            {
                // Agrupar datos por mes y forma de pago (solo formas válidas: 0 y 5)
                var datosPorMes = reporteActual
                    .Where(r => r.FechaPago.HasValue && 
                                !string.IsNullOrWhiteSpace(r.FormaPago_IGI) &&
                                (r.FormaPago_IGI == "0" || r.FormaPago_IGI == "5"))
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

                if (datosPorMes.Count == 0)
                {
                    lblTituloGrafica.Text = "Sin datos para mostrar";
                    return;
                }

                // Obtener meses únicos ordenados cronológicamente (de más reciente a más antiguo)
                var mesesUnicos = datosPorMes
                    .GroupBy(x => new { x.MesNombre, x.Periodo })
                    .OrderByDescending(g => g.Key.Periodo)
                    .Select(g => g.Key.MesNombre)
                    .ToList();

                var formasPago = datosPorMes.Select(x => x.FormaPago).Distinct().OrderBy(x => x).ToList();

                // Crear etiquetas combinadas con porcentajes: "enero FP-0  ■ 76.9% ■ 23.1%"
                var labels = new List<string>();
                var pagadoValues = new List<decimal>();
                var calculadoValues = new List<decimal>();
                var diferenciaValues = new List<decimal>();
                var pagadoPorcentajes = new List<decimal>();
                var diferenciaPorcentajes = new List<decimal>();

                foreach (var mes in mesesUnicos)
                {
                    foreach (var fp in formasPago)
                    {
                        var dato = datosPorMes.FirstOrDefault(x => x.MesNombre == mes && x.FormaPago == fp);
                        decimal pagado = dato?.Pagado ?? 0;
                        decimal calculado = dato?.Calculado ?? 0;
                        decimal diferencia = dato?.Diferencia ?? 0;

                        pagadoValues.Add(pagado);
                        calculadoValues.Add(calculado);
                        diferenciaValues.Add(diferencia);

                        // Calcular porcentajes basados en IGI Calculado como 100%
                        decimal porcPagado = 0;
                        decimal porcDiferencia = 0;

                        if (calculado > 0)
                        {
                            porcPagado = (pagado / calculado) * 100;
                            porcDiferencia = (diferencia / calculado) * 100;
                        }

                        pagadoPorcentajes.Add(porcPagado);
                        diferenciaPorcentajes.Add(porcDiferencia);

                        // Crear etiqueta con porcentajes incluidos
                        string etiqueta = $"{mes} FP-{fp}    ■ {porcPagado:N1}%  ■ {porcDiferencia:N1}%";
                        labels.Add(etiqueta);
                    }
                }

                // Crear series de barras apiladas horizontales
                var series = new List<ISeries>();

                // Crear diccionario para mapeo confiable de valores a porcentajes
                var pagadoDictionary = new Dictionary<decimal, decimal>();
                var diferenciaDictionary = new Dictionary<decimal, decimal>();

                for (int i = 0; i < pagadoValues.Count; i++)
                {
                    var keyPagado = pagadoValues[i];
                    var keyDiferencia = diferenciaValues[i];

                    // Solo agregar si no existe (para evitar duplicados)
                    if (!pagadoDictionary.ContainsKey(keyPagado))
                    {
                        pagadoDictionary[keyPagado] = pagadoPorcentajes[i];
                    }
                    if (!diferenciaDictionary.ContainsKey(keyDiferencia))
                    {
                        diferenciaDictionary[keyDiferencia] = diferenciaPorcentajes[i];
                    }
                }

                // Serie 1: IGI Pagado (azul) - mostrar porcentaje en la etiqueta
                var seriePagado = new StackedRowSeries<decimal>
                {
                    Name = "IGI pagado",
                    Values = pagadoValues.ToArray(),
                    Fill = new SolidColorPaint(new SKColor(79, 129, 189)), // Azul
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    MaxBarWidth = 50,
                    DataLabelsFormatter = point => {
                        var valor = (decimal)point.Model;
                        if (valor > 0 && pagadoDictionary.TryGetValue(valor, out var porcentaje))
                        {
                            return $"{porcentaje:N1}%";
                        }
                        return "";
                    }
                };

                series.Add(seriePagado);

                // Serie 2: Diferencia (azul claro) - mostrar porcentaje en la etiqueta
                var serieDiferencia = new StackedRowSeries<decimal>
                {
                    Name = "Diferencia",
                    Values = diferenciaValues.ToArray(),
                    Fill = new SolidColorPaint(new SKColor(155, 194, 230)), // Azul claro
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    MaxBarWidth = 50,
                    DataLabelsFormatter = point => {
                        var valor = (decimal)point.Model;
                        if (valor > 0 && diferenciaDictionary.TryGetValue(valor, out var porcentaje))
                        {
                            return $"{porcentaje:N1}%";
                        }
                        return "";
                    }
                };

                series.Add(serieDiferencia);

                chartIGI.Series = series.ToArray();

                // Configurar eje X (valores horizontales)
                chartIGI.XAxes = new[]
                {
                    new Axis
                    {
                        TextSize = 9,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                        Labeler = value => value >= 1000000 ? $"{value/1000000:N0}M" 
                                         : value >= 1000 ? $"{value/1000:N0}K" 
                                         : $"{value:N0}",
                        ShowSeparatorLines = true
                    }
                };

                // Configurar eje Y (etiquetas de meses + forma de pago)
                var totalFilas = labels.Count;

                chartIGI.YAxes = new[]
                {
                    new Axis
                    {
                        Labels = labels.ToArray(),
                        TextSize = 9,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                        ShowSeparatorLines = false
                    }
                };

                // Habilitar zoom y pan en ambos ejes
                // Ctrl + Rueda del mouse = Zoom
                // Click y arrastrar = Pan (desplazamiento)
                chartIGI.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both;

                // Actualizar título de la gráfica
                lblTituloGrafica.Text = "IGI por Mes y Forma de Pago (1/2)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar gráfica: {ex.Message}");
                lblTituloGrafica.Text = "Error al cargar gráfica (1/2)";
            }
        }

        private void ActualizarGraficaIVAPorFormaPago()
        {
            if (chartIVA == null) return;

            try
            {
                // Agrupar datos por mes y forma de pago IVA (solo formas válidas: 0 y 21)
                var datosPorMes = reporteActual
                    .Where(r => r.FechaPago.HasValue && 
                                !string.IsNullOrWhiteSpace(r.FormaPago_IVA) &&
                                (r.FormaPago_IVA == "0" || r.FormaPago_IVA == "21"))
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

                if (datosPorMes.Count == 0)
                {
                    lblTituloGraficaIVA.Text = "Sin datos de IVA para mostrar";
                    return;
                }

                // Obtener meses únicos ordenados cronológicamente (de más reciente a más antiguo)
                var mesesOrdenados = datosPorMes
                    .GroupBy(x => new { x.MesNombre, x.Periodo })
                    .OrderByDescending(g => g.Key.Periodo)
                    .Select(g => g.Key.MesNombre)
                    .ToList();

                var formasPago = datosPorMes.Select(x => x.FormaPago).Distinct().OrderBy(x => x).ToList();

                // Construir etiquetas combinadas (mes + forma de pago + monto IVA)
                var labels = new List<string>();
                var ivaPagadoValues = new List<decimal>();

                foreach (var mes in mesesOrdenados)
                {
                    foreach (var fp in formasPago)
                    {
                        var dato = datosPorMes.FirstOrDefault(x => x.MesNombre == mes && x.FormaPago == fp);
                        decimal ivaPagado = dato?.IVAPagado ?? 0;

                        ivaPagadoValues.Add(ivaPagado);

                        // Crear etiqueta con monto incluido
                        string etiqueta = $"{mes} FP-{fp}    ■ {ivaPagado:C0}";
                        labels.Add(etiqueta);
                    }
                }

                // Crear serie de barras horizontales
                var series = new List<ISeries>();

                // Serie: IVA Pagado (verde)
                series.Add(new RowSeries<decimal>
                {
                    Name = "IVA pagado",
                    Values = ivaPagadoValues.ToArray(),
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113)), // Verde
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    MaxBarWidth = 50
                });

                chartIVA.Series = series.ToArray();

                // Configurar eje X (valores horizontales)
                chartIVA.XAxes = new[]
                {
                    new Axis
                    {
                        TextSize = 9,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                        Labeler = value => value >= 1000000 ? $"{value/1000000:N0}M" 
                                         : value >= 1000 ? $"{value/1000:N0}K" 
                                         : $"{value:N0}",
                        ShowSeparatorLines = true
                    }
                };

                // Configurar eje Y (etiquetas de meses + forma de pago)
                chartIVA.YAxes = new[]
                {
                    new Axis
                    {
                        Labels = labels.ToArray(),
                        TextSize = 9,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                        ShowSeparatorLines = false
                    }
                };

                // Habilitar zoom y pan en ambos ejes
                // Ctrl + Rueda del mouse = Zoom
                // Click y arrastrar = Pan (desplazamiento)
                chartIVA.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both;

                // Actualizar título de la gráfica
                lblTituloGraficaIVA.Text = "IVA por Mes y Forma de Pago (2/2)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar gráfica IVA: {ex.Message}");
                lblTituloGraficaIVA.Text = "Error al cargar gráfica IVA (2/2)";
            }
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

                        // Obtener las tablas de datos para el PDF
                        var tablaIGI = reporteService.ConvertirADataTableIGI(reporteActual);
                        var tablaIVA = reporteService.ConvertirADataTableIVA(reporteActual);

                        var pdfService = new PdfGeneradorService();
                        pdfService.GenerarReporteIGIConFormasPagoPDF(
                            reporteActual,
                            tablaIGI,
                            tablaIVA,
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

        private void btnExportarExcel_Click(object sender, EventArgs e)
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
                    string nombreArchivo = $"Reporte_IGI_IVA_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    saveDialog.Filter = "Archivos Excel|*.xlsx";
                    saveDialog.Title = "Guardar Reporte en Excel";
                    saveDialog.FileName = nombreArchivo;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        btnExportarExcel.Enabled = false;
                        lblProgreso.Text = "Generando archivo Excel...";

                        // Crear workbook de Excel
                        using (var workbook = new XLWorkbook())
                        {
                            // Hoja 1: Reporte IGI
                            var worksheetIGI = workbook.Worksheets.Add("Reporte IGI");
                            var tablaIGI = dgvReporteIGI.DataSource as System.Data.DataTable;

                            if (tablaIGI != null && tablaIGI.Rows.Count > 0)
                            {
                                // Agregar encabezados
                                for (int col = 0; col < tablaIGI.Columns.Count; col++)
                                {
                                    worksheetIGI.Cell(1, col + 1).Value = tablaIGI.Columns[col].ColumnName;
                                    worksheetIGI.Cell(1, col + 1).Style.Font.Bold = true;
                                    worksheetIGI.Cell(1, col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                                    worksheetIGI.Cell(1, col + 1).Style.Font.FontColor = XLColor.White;
                                    worksheetIGI.Cell(1, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                }

                                // Agregar datos
                                for (int row = 0; row < tablaIGI.Rows.Count; row++)
                                {
                                    for (int col = 0; col < tablaIGI.Columns.Count; col++)
                                    {
                                        var valor = tablaIGI.Rows[row][col];
                                        var cell = worksheetIGI.Cell(row + 2, col + 1);

                                        if (valor is decimal || valor is double || valor is float)
                                        {
                                            cell.Value = Convert.ToDecimal(valor);
                                            cell.Style.NumberFormat.Format = "$#,##0.00";
                                        }
                                        else
                                        {
                                            cell.Value = valor?.ToString() ?? "";
                                        }
                                    }
                                }

                                // Ajustar ancho de columnas
                                worksheetIGI.Columns().AdjustToContents();
                            }

                            // Hoja 2: Reporte IVA
                            var worksheetIVA = workbook.Worksheets.Add("Reporte IVA");
                            var tablaIVA = dgvReporteIVA.DataSource as System.Data.DataTable;

                            if (tablaIVA != null && tablaIVA.Rows.Count > 0)
                            {
                                // Agregar encabezados
                                for (int col = 0; col < tablaIVA.Columns.Count; col++)
                                {
                                    worksheetIVA.Cell(1, col + 1).Value = tablaIVA.Columns[col].ColumnName;
                                    worksheetIVA.Cell(1, col + 1).Style.Font.Bold = true;
                                    worksheetIVA.Cell(1, col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(39, 174, 96);
                                    worksheetIVA.Cell(1, col + 1).Style.Font.FontColor = XLColor.White;
                                    worksheetIVA.Cell(1, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                }

                                // Agregar datos
                                for (int row = 0; row < tablaIVA.Rows.Count; row++)
                                {
                                    for (int col = 0; col < tablaIVA.Columns.Count; col++)
                                    {
                                        var valor = tablaIVA.Rows[row][col];
                                        var cell = worksheetIVA.Cell(row + 2, col + 1);

                                        if (valor is decimal || valor is double || valor is float)
                                        {
                                            cell.Value = Convert.ToDecimal(valor);
                                            cell.Style.NumberFormat.Format = "$#,##0.00";
                                        }
                                        else
                                        {
                                            cell.Value = valor?.ToString() ?? "";
                                        }
                                    }
                                }

                                // Ajustar ancho de columnas
                                worksheetIVA.Columns().AdjustToContents();
                            }

                            // Guardar archivo
                            workbook.SaveAs(saveDialog.FileName);
                        }

                        lblProgreso.Text = "Archivo Excel generado exitosamente";

                        var result = MessageBox.Show(
                            $"El archivo Excel se ha generado correctamente.\n\n¿Desea abrir el archivo?",
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
                    $"Error al generar el archivo Excel:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                lblProgreso.Text = "Error al generar Excel";
            }
            finally
            {
                btnExportarExcel.Enabled = reporteActual.Any();
            }
        }

        private void btnAnteriorGrafica_Click(object sender, EventArgs e)
        {
            CambiarGrafica(-1);
        }

        private void btnSiguienteGrafica_Click(object sender, EventArgs e)
        {
            CambiarGrafica(1);
        }

        private void CambiarGrafica(int direccion)
        {
            // Cambiar índice de gráfica (ciclo entre 0 y 1)
            graficaActual = (graficaActual + direccion + 2) % 2;

            // Mostrar/ocultar paneles según la gráfica actual
            if (graficaActual == 0)
            {
                // Mostrar IGI
                panelGrafica.Visible = true;
                panelGrafica.BringToFront();
                panelGraficaIVA.Visible = false;
                lblTituloGrafica.Text = "IGI por Mes y Forma de Pago (1/2)";
            }
            else
            {
                // Mostrar IVA
                panelGraficaIVA.Visible = true;
                panelGraficaIVA.BringToFront();
                panelGrafica.Visible = false;
                lblTituloGraficaIVA.Text = "IVA por Mes y Forma de Pago (2/2)";
            }
        }


    }
}
