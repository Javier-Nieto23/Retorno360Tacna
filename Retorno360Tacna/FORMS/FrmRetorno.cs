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
                return;
            }

            if (cmbRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                idRazonSeleccionada = razonSeleccionada.IdRazon;

                // Si el checkbox está activado, no cargar bases de datos
                if (!chkCalRazon.Checked)
                {
                    CargarBasesDatosRazon(razonSeleccionada.IdRazon);
                }
            }
        }

        private void chkCalRazon_CheckedChanged(object sender, EventArgs e)
        {
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

                List<BaseDatosRazon> bases = retornoService.ObtenerBasesDatosRazon(idRazon);

                if (bases.Count > 0)
                {
                    // Filtrar por servidor si es necesario
                    string servidor = conexionActual?.NombreConexion ?? string.Empty;
                    
                    // Extraer solo los nombres de las bases de datos
                    List<string> nombresBases = bases.Select(b => b.NombreTabla).ToList();
                    
                    // Aplicar filtro de ValidadorBasesDatos si el servidor tiene restricciones
                    List<string> basesFiltradas = ValidadorBasesDatos.FiltrarBasesDatos(servidor, nombresBases);

                    if (basesFiltradas.Count > 0)
                    {
                        cmbBaseDatos.DataSource = basesFiltradas;
                        cmbBaseDatos.Enabled = true;
                        cmbBaseDatos.SelectedIndex = -1;
                    }
                    else
                    {
                        cmbBaseDatos.DataSource = null;
                        cmbBaseDatos.Enabled = false;
                        
                        if (ValidadorBasesDatos.ServidorTieneRestricciones(servidor))
                        {
                            MessageBox.Show($"No se encontraron bases de datos permitidas para el servidor '{servidor}'.",
                                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
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

        private void btnCalcular_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos())
                return;

            CalcularPorcentajeRetorno();
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

        private void CalcularPorcentajeRetorno()
        {
            try
            {
                if (retornoService == null)
                {
                    MessageBox.Show("El servicio de retorno no está disponible.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                btnCalcular.Enabled = false;
                btnCalcular.Text = "Calculando...";
                Cursor = Cursors.WaitCursor;

                ResultadoRetorno resultado;

                if (chkCalRazon.Checked)
                {
                    // Cálculo por razón social general (sin validación de pedimentos)
                    resultado = retornoService.CalcularRetornoPorRazonSocial(
                        idRazonSeleccionada,
                        dtpFechaInicio.Value,
                        dtpFechaFin.Value,
                        chkMateriaPrima.Checked
                    );

                    MessageBox.Show("Cálculo por razón social completado exitosamente.\n\n" +
                        "Nota: Este cálculo utiliza todos los pedimentos de TR_Glosa sin validación cruzada.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Cálculo con validación de pedimentos (modo normal)
                    string baseDatosSeleccionada = cmbBaseDatos.SelectedItem?.ToString() ?? string.Empty;

                    resultado = retornoService.CalcularRetorno(
                        idRazonSeleccionada,
                        baseDatosSeleccionada,
                        dtpFechaInicio.Value,
                        dtpFechaFin.Value,
                        chkMateriaPrima.Checked
                    );

                    MessageBox.Show("Cálculo completado exitosamente.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                btnCalcular.Enabled = true;
                btnCalcular.Text = "Calcular Retorno";
                Cursor = Cursors.Default;
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
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"${point.Model:N0}"
                },
                new ColumnSeries<double>
                {
                    Name = "Exportaciones",
                    Values = new double[] { (double)resultado.ValorExportado },
                    Fill = new SolidColorPaint(new SKColor(41, 128, 185)),
                    Stroke = new SolidColorPaint(new SKColor(21, 67, 96)) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"${point.Model:N0}"
                }
            };

            // Configurar ejes de la gráfica lineal
            cartesianChartView.XAxes = new[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Labels = new[] { "Valores Comerciales" },
                    LabelsRotation = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
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
                    Labeler = value => $"${value:N0}"
                }
            };
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
                    saveDialog.FileName = ReporteRetornoPDF.GenerarNombreArchivo(ultimoResultado);

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;

                        try
                        {
                            // Capturar las gráficas
                            SKBitmap? graficaLineal = ChartHelper.CapturarControl(cartesianChartView);
                            SKBitmap? graficaCircular = ChartHelper.CapturarControl(pieChartView);

                            // Generar PDF con gráficas
                            ReporteRetornoPDF reporte = new ReporteRetornoPDF(
                                ultimoResultado, 
                                graficaLineal, 
                                graficaCircular
                            );

                            reporte.GenerarPDF(saveDialog.FileName);

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
                        catch (Exception ex)
                        {
                            Cursor = Cursors.Default;
                            MessageBox.Show($"Error al generar el PDF: {ex.Message}\n\n{ex.StackTrace}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al preparar el PDF: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
