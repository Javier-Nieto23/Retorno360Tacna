using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
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
        private sealed class BaseDatosComboItem
        {
            public string NombreReal { get; set; } = string.Empty;
            public string NombreVisible { get; set; } = string.Empty;
        }

        private ConexionInfo? conexionActual;
        private RetornoService? retornoService;
        private int idRazonSeleccionada = 0;
        private ResultadoRetorno? ultimoResultado;
        private Dictionary<Button, bool>? estadoBotonesAntesCarga;
        private MODELS.Usuario? usuarioActual;
        private SERVICES.PerfilUsuarioService? perfilService;

        public FrmRetorno()
        {
            InitializeComponent();
        }

        public FrmRetorno(ConexionInfo conexion) : this(conexion, null) { }

        public FrmRetorno(ConexionInfo conexion, MODELS.Usuario? usuario)
        {
            InitializeComponent();
            conexionActual = conexion;
            retornoService = new RetornoService(conexion);
            usuarioActual = usuario;

            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            // Configurar eventos de redimensionamiento
            this.Resize += FrmRetorno_Resize;
        }

        private static List<BaseDatosComboItem> CrearItemsBaseDatos(IEnumerable<string> basesDatos)
        {
            return basesDatos
                .Select(baseDatos => new BaseDatosComboItem
                {
                    NombreReal = baseDatos,
                    NombreVisible = LimpiarNombreBaseDatosVisible(baseDatos)
                })
                .ToList();
        }

        private static string LimpiarNombreBaseDatosVisible(string nombreBaseDatos)
        {
            return nombreBaseDatos
                .Replace("SEERT_", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim(' ', '_', '-');
        }

        private void FrmRetorno_Resize(object sender, EventArgs e)
        {
            AjustarControles();
        }

        private void EstablecerEstadoBotonesDuranteCarga(bool cargando)
        {
            if (FindForm() is MainMenu mainMenu)
            {
                mainMenu.EstablecerNavegacionLateralHabilitada(!cargando);
            }

            var botones = ObtenerBotonesRecursivamente(this)
                .Where(b => b != null)
                .ToList();

            if (cargando)
            {
                estadoBotonesAntesCarga = botones.ToDictionary(b => b, b => b.Enabled);
                foreach (var boton in botones)
                {
                    boton.Enabled = false;
                }
            }
            else if (estadoBotonesAntesCarga != null)
            {
                foreach (var item in estadoBotonesAntesCarga)
                {
                    if (!item.Key.IsDisposed)
                    {
                        item.Key.Enabled = item.Value;
                    }
                }

                estadoBotonesAntesCarga = null;
            }
        }

        private static IEnumerable<Button> ObtenerBotonesRecursivamente(Control control)
        {
            foreach (Control hijo in control.Controls)
            {
                if (hijo is Button boton)
                    yield return boton;

                foreach (var botonHijo in ObtenerBotonesRecursivamente(hijo))
                    yield return botonHijo;
            }
        }

        private void AjustarControles()
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;

            try
            {
                if (!this.IsHandleCreated)
                    return;

                this.SuspendLayout();

                const int baseWidth = 1230;
                const int baseHeight = 618;
                const int baseChartWidth = 428;
                const int baseChartHeight = 396;

                int margen = 12;
                int separacion = 15;
                int topGraficas = 170;

                float factorAncho = (float)this.ClientSize.Width / baseWidth;
                float factorAlto = (float)this.ClientSize.Height / baseHeight;
                float factorEscala = Math.Min(factorAncho, factorAlto);
                factorEscala = Math.Max(0.95f, Math.Min(1.12f, factorEscala));

                int anchoResultados = 310;
                int altoResultados = Math.Max(298, Math.Min(360, this.ClientSize.Height - topGraficas - margen));
                int altoGrafica = (int)(baseChartHeight * factorEscala);
                int anchoGraficaObjetivo = (int)(baseChartWidth * factorEscala);

                groupBoxResultados.Location = new Point(margen, 172);
                groupBoxResultados.Size = new Size(anchoResultados, altoResultados);

                int areaIzquierda = groupBoxResultados.Right + separacion;
                int anchoDisponibleGrafica = Math.Max(360, this.ClientSize.Width - areaIzquierda - margen);
                int anchoGrafica = Math.Min(anchoDisponibleGrafica, Math.Max(390, Math.Min(520, anchoGraficaObjetivo)));
                int leftGraficaCentrado = (this.ClientSize.Width - anchoGrafica) / 2;
                int leftGrafica = Math.Max(areaIzquierda, leftGraficaCentrado);
                int topGrafica = topGraficas + 45 + Math.Max(0, (altoResultados - altoGrafica) / 2);

                panelGraficaColumnas.Location = new Point(leftGrafica, topGrafica);
                panelGraficaColumnas.Size = new Size(anchoGrafica, altoGrafica);

                panelGraficaPie.Location = new Point(leftGrafica, topGrafica);
                panelGraficaPie.Size = new Size(anchoGrafica, altoGrafica);

                AjustarAreaGrafica(panelGraficaColumnas, cartesianChartView, lblTituloColumnas, btnAnteriorColumnas, btnSiguienteColumnas);
                AjustarAreaGrafica(panelGraficaPie, pieChartView, lblTituloPie, btnAnteriorPie, btnSiguientePie);

                this.ResumeLayout(false);
                this.PerformLayout();
            }
            catch
            {
                // Evitar errores durante el redimensionamiento
            }
        }

        private void AjustarAreaGrafica(Control panelContenedor, Control grafica, Label titulo, Button btnAnterior, Button btnSiguiente)
        {
            if (panelContenedor == null || grafica == null)
                return;

            int margen = 8;
            int topGrafica = 42;
            int anchoGrafica = Math.Max(200, panelContenedor.ClientSize.Width - (margen * 2));
            int altoGrafica = Math.Max(200, panelContenedor.ClientSize.Height - topGrafica - margen);

            grafica.Location = new Point(margen, topGrafica);
            grafica.Size = new Size(anchoGrafica, altoGrafica);
            grafica.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grafica.SendToBack();

            btnAnterior.Location = new Point(5, 5);
            btnSiguiente.Location = new Point(Math.Max(5, panelContenedor.ClientSize.Width - btnSiguiente.Width - 5), 5);

            titulo.AutoSize = false;
            titulo.TextAlign = ContentAlignment.MiddleCenter;
            titulo.Location = new Point(btnAnterior.Right + 10, 8);
            titulo.Size = new Size(Math.Max(120, panelContenedor.ClientSize.Width - btnAnterior.Width - btnSiguiente.Width - 30), 24);

            titulo.BringToFront();
            btnAnterior.BringToFront();
            btnSiguiente.BringToFront();
        }

        private void FrmRetorno_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            InicializarFechas();
            ConfigurarGrafica();
            AjustarControles();
        }

        private void InicializarFechas()
        {
            dtpFechaInicio.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpFechaFin.Value = DateTime.Now;
        }

        private void ConfigurarGrafica()
        {
            cartesianChartView = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                ZoomingSpeed = 1.1
            };

            panelGraficaColumnas.Controls.Add(cartesianChartView);
            cartesianChartView.SendToBack();

            btnAnteriorColumnas.Cursor = Cursors.Hand;
            btnSiguienteColumnas.Cursor = Cursors.Hand;

            panelGraficaColumnas.Controls.Add(lblTituloColumnas);
            panelGraficaColumnas.Controls.Add(btnAnteriorColumnas);
            panelGraficaColumnas.Controls.Add(btnSiguienteColumnas);
            lblTituloColumnas.BringToFront();
            btnAnteriorColumnas.BringToFront();
            btnSiguienteColumnas.BringToFront();

            pieChartView = new LiveChartsCore.SkiaSharpView.WinForms.PieChart
            {
                Dock = DockStyle.Fill
            };

            panelGraficaPie.Controls.Add(pieChartView);
            pieChartView.SendToBack();

            btnAnteriorPie.Cursor = Cursors.Hand;
            btnSiguientePie.Cursor = Cursors.Hand;

            panelGraficaPie.Controls.Add(lblTituloPie);
            panelGraficaPie.Controls.Add(btnAnteriorPie);
            panelGraficaPie.Controls.Add(btnSiguientePie);
            lblTituloPie.BringToFront();
            btnAnteriorPie.BringToFront();
            btnSiguientePie.BringToFront();

            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnAnteriorColumnas, "Gráfica anterior");
            tooltip.SetToolTip(btnSiguienteColumnas, "Gráfica siguiente");
            tooltip.SetToolTip(btnAnteriorPie, "Gráfica anterior");
            tooltip.SetToolTip(btnSiguientePie, "Gráfica siguiente");

            pieChartView.Series = Array.Empty<ISeries>();
            cartesianChartView.Series = Array.Empty<ISeries>();

            AjustarAreaGrafica(panelGraficaColumnas, cartesianChartView, lblTituloColumnas, btnAnteriorColumnas, btnSiguienteColumnas);
            AjustarAreaGrafica(panelGraficaPie, pieChartView, lblTituloPie, btnAnteriorPie, btnSiguientePie);
        }

        private void CargarRazonesSociales()
        {
            try
            {
                if (retornoService == null)
                {
                    ErrorMessageHelper.ShowError("El servicio de retorno no está disponible.",
                        "Error", contexto: "Carga de razones sociales en retorno");
                    return;
                }

                List<RazonSocial> razones = (chkPrecargarDatos.Checked && usuarioActual != null && perfilService != null)
                    ? perfilService.ObtenerRazonesSocialesDePerfil(usuarioActual.IdUsuario)
                    : retornoService.ObtenerRazonesSociales();

                cmbRazonSocial.DataSource = razones;
                cmbRazonSocial.DisplayMember = "NombreRazon";
                cmbRazonSocial.ValueMember = "IdRazon";
                cmbRazonSocial.SelectedIndex = -1;

                cmbBaseDatos.DataSource = null;
                cmbBaseDatos.Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar razones sociales: {ex.Message}",
                    "Error", ex, "Carga de razones sociales en retorno");
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

                btnPDF.Enabled = false;
                ultimoResultado = null;

                if (!chkCalRazon.Checked)
                {
                    CargarBasesDatosRazon(razonSeleccionada.IdRazon);
                }
            }
        }

        private void chkCalRazon_CheckedChanged(object sender, EventArgs e)
        {
            btnPDF.Enabled = false;
            ultimoResultado = null;

            if (chkCalRazon.Checked)
            {
                cmbBaseDatos.Enabled = false;
                cmbBaseDatos.SelectedIndex = -1;
            }
            else
            {
                if (cmbRazonSocial.SelectedIndex != -1 && cmbRazonSocial.SelectedItem is RazonSocial razon)
                {
                    CargarBasesDatosRazon(razon.IdRazon);
                }
            }
        }

        private void chkPrecargarDatos_CheckedChanged(object sender, EventArgs e)
        {
            btnPDF.Enabled = false;
            ultimoResultado = null;

            if (chkPrecargarDatos.Checked && usuarioActual == null)
            {
                MessageBox.Show("No hay usuario activo para usar el perfil de empresas.",
                    "Perfil no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkPrecargarDatos.Checked = false;
                return;
            }

            // Recargar razones sociales según modo activo
            CargarRazonesSociales();
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                if (retornoService == null)
                    return;

                List<string> basesDatos = (chkPrecargarDatos.Checked && usuarioActual != null && perfilService != null)
                    ? perfilService.ObtenerBasesDatosDePerfilPorRazon(usuarioActual.IdUsuario, idRazon)
                    : retornoService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cmbBaseDatos.DataSource = CrearItemsBaseDatos(basesDatos);
                    cmbBaseDatos.DisplayMember = nameof(BaseDatosComboItem.NombreVisible);
                    cmbBaseDatos.ValueMember = nameof(BaseDatosComboItem.NombreReal);
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
                ErrorMessageHelper.ShowError($"Error al cargar bases de datos: {ex.Message}",
                    "Error", ex, "Carga de bases de datos en retorno");
                cmbBaseDatos.DataSource = null;
                cmbBaseDatos.Enabled = false;
            }
        }

        private void cmbBaseDatos_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Lógica adicional si se requiere
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
                    ErrorMessageHelper.ShowError("El servicio de retorno no está disponible.",
                        "Error", contexto: "Cálculo de retorno sin servicio disponible");
                    return;
                }

                MostrarPanelCargando(true);
                EstablecerEstadoBotonesDuranteCarga(true);

                btnCalcular.Enabled = false;
                btnCalcular.Text = "Calculando...";

                ResultadoRetorno? resultado = null;

                if (chkCalRazon.Checked)
                {
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
                    string baseDatosSeleccionada = cmbBaseDatos.SelectedValue?.ToString() ?? string.Empty;

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
                }

                if (resultado != null)
                {
                    MostrarResultados(resultado);
                    ActualizarGrafica(resultado);

                    try
                    {
                        _ = await Retorno360Tacna.SERVICES.PortalWebService.GuardarResultadoRetornoAsync(resultado);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[FrmRetorno] Error guardando resultado en portal web: {ex}");
                    }

                    ultimoResultado = resultado;
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al calcular el porcentaje de retorno: {ex.Message}",
                    "Error", ex, "Cálculo de porcentaje de retorno");
            }
            finally
            {
                bool estadoPdf = btnPDF.Enabled;

                MostrarPanelCargando(false);
                EstablecerEstadoBotonesDuranteCarga(false);
                btnPDF.Enabled = estadoPdf;
                btnCalcular.Text = "Calcular Retorno";
            }
        }

        private void MostrarPanelCargando(bool mostrar)
        {
            panelCargando.Visible = mostrar;
            if (mostrar)
            {
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

            lblCantPedimentosImp.Text = $"Pedimentos Importación: {resultado.CantidadPedimentosImportacion}";
            lblCantPedimentosExp.Text = $"Pedimentos Exportación: {resultado.CantidadPedimentosExportacion}";
            lblTotalPedimentos.Text = $"Total Pedimentos: {resultado.TotalPedimentosValidados}";

            btnPDF.Enabled = true;
        }

        private void ActualizarGrafica(ResultadoRetorno resultado)
        {
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

                using SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
                saveDialog.Title = "Guardar Reporte PDF";
                saveDialog.FileName = $"Reporte_Retorno_{ultimoResultado.RazonSocial.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                Cursor = Cursors.WaitCursor;

                PdfGeneradorService pdfService = new PdfGeneradorService();
                pdfService.GenerarReportePDF(ultimoResultado, saveDialog.FileName);

                Cursor = Cursors.Default;

                DialogResult resultado = MessageBox.Show(
                    "PDF generado exitosamente.\n\n¿Desea abrir el archivo?",
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
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                ErrorMessageHelper.ShowError($"Error al generar el PDF: {ex.Message}",
                    "Error", ex, "Generación de PDF de retorno");
            }
        }
    }
}