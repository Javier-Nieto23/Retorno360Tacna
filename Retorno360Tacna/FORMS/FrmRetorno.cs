using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Defaults;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmRetorno : Form
    {
        private ConexionInfo? conexionActual;
        private RetornoService? retornoService;
        private int idRazonSeleccionada = 0;
        private ResultadoRetorno? ultimoResultado;

        public FrmRetorno()
        {
            InitializeComponent();
        }

        public FrmRetorno(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            retornoService = new RetornoService(conexion);

            // Configurar eventos de redimensionamiento
            this.Resize += FrmRetorno_Resize;
        }

        private void FrmRetorno_Resize(object sender, EventArgs e)
        {
            AjustarControles();
        }

        private void AjustarControles()
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;

            try
            {
                // Solo ajustar si el formulario está cargado
                if (!this.IsHandleCreated)
                    return;

                this.SuspendLayout();

                int anchoDisponible = this.ClientSize.Width;
                int altoDisponible = this.ClientSize.Height;

                // Posiciones y tamaños base del diseño original
                const int BASE_WIDTH = 1230;
                const int BASE_HEIGHT = 618;
                const int CHART_LEFT = 340;
                const int PIE_LEFT = 799;
                const int CHART_TOP = 172;
                const int CHART_BASE_WIDTH = 450;
                const int PIE_BASE_WIDTH = 400;
                const int CHART_BASE_HEIGHT = 410;

                // Calcular factores de escala
                float factorAncho = (float)anchoDisponible / BASE_WIDTH;
                float factorAlto = (float)altoDisponible / BASE_HEIGHT;

                // Usar el factor menor para mantener proporciones
                float factorEscala = Math.Min(factorAncho, factorAlto);

                // No reducir más allá del tamaño original
                if (factorEscala > 1.0f)
                {
                    factorEscala = 1.0f + ((factorEscala - 1.0f) * 0.7f); // Crecer solo 70% del exceso
                }

                // Ajustar panel de gráfica de columnas
                if (panelGraficaColumnas != null)
                {
                    int nuevoAncho = (int)(CHART_BASE_WIDTH * factorEscala);
                    int nuevoAlto = (int)(CHART_BASE_HEIGHT * factorEscala);

                    panelGraficaColumnas.Location = new Point(CHART_LEFT, CHART_TOP);
                    panelGraficaColumnas.Width = Math.Max(350, nuevoAncho);
                    panelGraficaColumnas.Height = Math.Max(300, nuevoAlto);
                }

                // Ajustar panel de gráfica de pie
                if (panelGraficaPie != null)
                {
                    int nuevoAncho = (int)(CHART_BASE_WIDTH * factorEscala);
                    int nuevoAlto = (int)(CHART_BASE_HEIGHT * factorEscala);

                    panelGraficaPie.Location = new Point(CHART_LEFT, CHART_TOP);
                    panelGraficaPie.Width = Math.Max(350, nuevoAncho);
                    panelGraficaPie.Height = Math.Max(300, nuevoAlto);
                }

                this.ResumeLayout(false);
                this.PerformLayout();
            }
            catch
            {
                // Evitar errores durante el redimensionamiento
            }
        }

        private void FrmRetorno_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            InicializarFechas();
            ConfigurarGrafica();
        }

        private void InicializarFechas()
        {
            dtpFechaInicio.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpFechaFin.Value = DateTime.Now;
        }

        private void ConfigurarGrafica()
        {
            // Configurar gráfica de columnas con zoom completo
            cartesianChartView = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                ZoomingSpeed = 1.1
            };

            // Agregar gráfica de columnas al panel
            panelGraficaColumnas.Controls.Add(cartesianChartView);
            cartesianChartView.SendToBack(); // Enviar la gráfica atrás

            // Asegurar que los botones de navegación estén visibles y configurados
            btnAnteriorColumnas.Cursor = Cursors.Hand;
            btnSiguienteColumnas.Cursor = Cursors.Hand;

            // Agregar los controles de navegación AL FRENTE
            panelGraficaColumnas.Controls.Add(lblTituloColumnas);
            panelGraficaColumnas.Controls.Add(btnAnteriorColumnas);
            panelGraficaColumnas.Controls.Add(btnSiguienteColumnas);
            lblTituloColumnas.BringToFront();
            btnAnteriorColumnas.BringToFront();
            btnSiguienteColumnas.BringToFront();

            // Configurar gráfica de pie
            pieChartView = new LiveChartsCore.SkiaSharpView.WinForms.PieChart
            {
                Dock = DockStyle.Fill
            };

            // Agregar gráfica de pie al panel
            panelGraficaPie.Controls.Add(pieChartView);
            pieChartView.SendToBack(); // Enviar la gráfica atrás

            // Asegurar que los botones de navegación estén visibles y configurados
            btnAnteriorPie.Cursor = Cursors.Hand;
            btnSiguientePie.Cursor = Cursors.Hand;

            // Agregar los controles de navegación AL FRENTE
            panelGraficaPie.Controls.Add(lblTituloPie);
            panelGraficaPie.Controls.Add(btnAnteriorPie);
            panelGraficaPie.Controls.Add(btnSiguientePie);
            lblTituloPie.BringToFront();
            btnAnteriorPie.BringToFront();
            btnSiguientePie.BringToFront();

            // Configurar tooltips para los botones
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnAnteriorColumnas, "Gráfica anterior");
            tooltip.SetToolTip(btnSiguienteColumnas, "Gráfica siguiente");
            tooltip.SetToolTip(btnAnteriorPie, "Gráfica anterior");
            tooltip.SetToolTip(btnSiguientePie, "Gráfica siguiente");

            // Inicializar series vacías
            pieChartView.Series = Array.Empty<ISeries>();
            cartesianChartView.Series = Array.Empty<ISeries>();
        }

        private void CargarRazonesSociales()
        {
            try
            {
                if (retornoService == null)
                {
                    MessageBox.Show("El servicio de retorno no está disponible.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                List<RazonSocial> razones = retornoService.ObtenerRazonesSociales();

                cmbRazonSocial.DataSource = razones;
                cmbRazonSocial.DisplayMember = "NombreRazon";
                cmbRazonSocial.ValueMember = "IdRazon";
                cmbRazonSocial.SelectedIndex = -1;

                cmbBaseDatos.DataSource = null;
                cmbBaseDatos.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedIndex == -1)
            {
                cmbBaseDatos.DataSource = null;
                cmbBaseDatos.Enabled = false;
                btnPDF.Enabled = false;
                return;
            }

            if (cmbRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                idRazonSeleccionada = razonSeleccionada.IdRazon;

                // Deshabilitar el botón PDF al cambiar la selección
                btnPDF.Enabled = false;
                ultimoResultado = null;

                // Si el checkbox está activado, no cargar bases de datos
                if (!chkCalRazon.Checked)
                {
                    CargarBasesDatosRazon(razonSeleccionada.IdRazon);
                }
            }
        }

        private void chkCalRazon_CheckedChanged(object sender, EventArgs e)
        {
            // Deshabilitar el botón PDF al cambiar la selección
            btnPDF.Enabled = false;
            ultimoResultado = null;

            if (chkCalRazon.Checked)
            {
                // Deshabilitar combo de bases de datos
                cmbBaseDatos.Enabled = false;
                cmbBaseDatos.SelectedIndex = -1;
            }
            else
            {
                // Reactivar y cargar bases de datos si hay razón social seleccionada
                if (cmbRazonSocial.SelectedIndex != -1 && cmbRazonSocial.SelectedItem is RazonSocial razon)
                {
                    CargarBasesDatosRazon(razon.IdRazon);
                }
            }
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                if (retornoService == null)
                    return;

                List<string> basesDatos = retornoService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cmbBaseDatos.DataSource = basesDatos;
                    cmbBaseDatos.Enabled = true;
                    cmbBaseDatos.SelectedIndex = -1;
                }
                else
                {
                    cmbBaseDatos.DataSource = null;
                    cmbBaseDatos.Enabled = false;
                    MessageBox.Show("No se encontraron bases de datos asociadas a esta razón social.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar bases de datos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbBaseDatos.DataSource = null;
                cmbBaseDatos.Enabled = false;
            }
        }

        private void cmbBaseDatos_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Este evento se mantiene por si se necesita lógica adicional
        }

        private async void btnCalcular_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos())
                return;

            await CalcularPorcentajeRetornoAsync();
        }

        private bool ValidarDatos()
        {
            if (cmbRazonSocial.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor seleccione una razón social.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Solo validar base de datos si NO está calculando por razón social
            if (!chkCalRazon.Checked && cmbBaseDatos.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor seleccione una base de datos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtpFechaInicio.Value > dtpFechaFin.Value)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async Task CalcularPorcentajeRetornoAsync()
        {
            try
            {
                if (retornoService == null)
                {
                    MessageBox.Show("El servicio de retorno no está disponible.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Mostrar panel de carga
                MostrarPanelCargando(true);

                btnCalcular.Enabled = false;
                btnCalcular.Text = "Calculando...";

                ResultadoRetorno resultado = null;

                if (chkCalRazon.Checked)
                {
                    // Cálculo por razón social general (sin validación de pedimentos)
                    resultado = await Task.Run(() => retornoService.CalcularRetornoPorRazonSocial(
                        idRazonSeleccionada,
                        dtpFechaInicio.Value,
                        dtpFechaFin.Value,
                        chkMateriaPrima.Checked
                    ));

                    MessageBox.Show("Cálculo por razón social completado exitosamente.\n\n" +
                        "Nota: Este cálculo utiliza todos los pedimentos de TR_Glosa sin validación cruzada.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Cálculo con validación de pedimentos (modo normal)
                    string baseDatosSeleccionada = cmbBaseDatos.SelectedItem?.ToString() ?? string.Empty;

                    resultado = await Task.Run(() => retornoService.CalcularRetorno(
                        idRazonSeleccionada,
                        baseDatosSeleccionada,
                        dtpFechaInicio.Value,
                        dtpFechaFin.Value,
                        chkMateriaPrima.Checked,
                        chkForzarCalculo.Checked
                    ));

                    if (chkForzarCalculo.Checked)
                    {
                        MessageBox.Show("Cálculo completado exitosamente.\n\n" +
                            "NOTA: Se omitieron las validaciones de pedimentos. El cálculo se realizó con los datos disponibles.",
                            "Éxito - Cálculo Forzado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                    }
                }

                MostrarResultados(resultado);
                ActualizarGrafica(resultado);

                // Guardar el último resultado para el PDF
                ultimoResultado = resultado;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular el porcentaje de retorno: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Ocultar panel de carga
                MostrarPanelCargando(false);

                btnCalcular.Enabled = true;
                btnCalcular.Text = "Calcular Retorno";
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

        private void MostrarResultados(ResultadoRetorno resultado)
        {
            lblImportadoValor.Text = $"${resultado.ValorImportado:N2}";
            lblExportadoValor.Text = $"${resultado.ValorExportado:N2}";
            lblPorcentajeValor.Text = $"{resultado.PorcentajeRetorno:N2}%";

            // Mostrar cantidades de pedimentos
            lblCantPedimentosImp.Text = $"Pedimentos Importación: {resultado.CantidadPedimentosImportacion}";
            lblCantPedimentosExp.Text = $"Pedimentos Exportación: {resultado.CantidadPedimentosExportacion}";
            lblTotalPedimentos.Text = $"Total Pedimentos: {resultado.TotalPedimentosValidados}";

            // Habilitar el botón de generar PDF
            btnPDF.Enabled = true;
        }

        private void ActualizarGrafica(ResultadoRetorno resultado)
        {
            // Gráfica de pastel (circular)
            pieChartView.Series = new ISeries[]
            {
                new PieSeries<double>
                {
                    Name = "Importado",
                    Values = new double[] { (double)resultado.ValorImportado },
                    Fill = new SolidColorPaint(SKColors.Crimson),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.Model:N0}"
                },
                new PieSeries<double>
                {
                    Name = "Exportado",
                    Values = new double[] { (double)resultado.ValorExportado },
                    Fill = new SolidColorPaint(new SKColor(41, 128, 185)),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.Model:N0}"
                }
            };

            // Gráfica lineal (comparación de valores)
            cartesianChartView.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Importaciones",
                    Values = new double[] { (double)resultado.ValorImportado },
                    Fill = new SolidColorPaint(SKColors.Crimson),
                    Stroke = new SolidColorPaint(SKColors.DarkRed) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"${point.Model:N0}",
                    MaxBarWidth = 100
                },
                new ColumnSeries<double>
                {
                    Name = "Exportaciones",
                    Values = new double[] { (double)resultado.ValorExportado },
                    Fill = new SolidColorPaint(new SKColor(41, 128, 185)),
                    Stroke = new SolidColorPaint(new SKColor(21, 67, 96)) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"${point.Model:N0}",
                    MaxBarWidth = 100
                }
            };

            // Configurar ejes de la gráfica de columnas con soporte completo para zoom
            cartesianChartView.XAxes = new[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Labels = new[] { "Valores Comerciales" },
                    LabelsRotation = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    MinLimit = null,
                    MaxLimit = null,
                    MinStep = 1,
                    ForceStepToMin = false
                }
            };

            cartesianChartView.YAxes = new[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Name = "USD",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    Labeler = value => $"${value:N0}",
                    MinLimit = null,
                    MaxLimit = null,
                    ForceStepToMin = false
                }
            };
        }

        private void btnCambiarGrafica_Click(object sender, EventArgs e)
        {
            // Alternar entre paneles de gráficas
            if (panelGraficaColumnas.Visible)
            {
                panelGraficaColumnas.Visible = false;
                panelGraficaPie.Visible = true;
            }
            else
            {
                panelGraficaColumnas.Visible = true;
                panelGraficaPie.Visible = false;
            }
        }

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultimoResultado == null)
                {
                    MessageBox.Show("Primero debe calcular el porcentaje de retorno antes de generar el PDF.",
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Crear diálogo para guardar archivo
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
                    saveDialog.Title = "Guardar Reporte PDF";
                    saveDialog.FileName = $"Reporte_Retorno_{ultimoResultado.RazonSocial.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;

                        // Generar PDF
                        PdfGeneradorService pdfService = new PdfGeneradorService();
                        pdfService.GenerarReportePDF(ultimoResultado, saveDialog.FileName);

                        Cursor = Cursors.Default;

                        DialogResult resultado = MessageBox.Show(
                            $"PDF generado exitosamente.\n\n¿Desea abrir el archivo?",
                            "Éxito",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (resultado == DialogResult.Yes)
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
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al generar el PDF: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
