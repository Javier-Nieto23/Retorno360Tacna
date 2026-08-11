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
        private sealed class BaseDatosComboItem
        {
            public string NombreReal { get; set; } = string.Empty;
            public string NombreVisible { get; set; } = string.Empty;
        }

        private sealed class ResumenTipoParte
        {
            public string Tipo { get; set; } = string.Empty;
            public int Total { get; set; }
        }

        private sealed class ResumenCumplimiento
        {
            public string Etiqueta { get; set; } = string.Empty;
            public int TotalSubidos { get; set; }
            public int EnBom { get; set; }
            public int EnPedimento { get; set; }
            public int Cumplen { get; set; }
            public int NoCumplen => Math.Max(0, TotalSubidos - Cumplen);
            public double PorcentajeCumplimiento => TotalSubidos == 0 ? 0D : Math.Round((double)Cumplen * 100D / TotalSubidos, 2);
            public double PorcentajeIncumplimiento => Math.Max(0D, Math.Round(100D - PorcentajeCumplimiento, 2));
        }

        private readonly ConexionInfo conexionActual;
        private CatalogoPartesService catalogoService;
        private List<MateriaPrimaBOM> datosConsultadosMP;
        private List<MateriaPrimaBOM> datosConsultadosOtros;
        private List<MateriaPrimaBOM> datosOtrosFiltrados;
        private bool consultarTodasEmpresasRazonSocial;
        private bool consultarTodasRazonesSociales;
        private List<ResumenCumplimiento> resumenCumplimientoPorRazon;
        private readonly LiveChartsCore.SkiaSharpView.WinForms.PieChart chartPedimentosMP;
        private Control chartControl; // Control genérico para manejar ambos tipos de gráficas
        private int vistaActual = 0; // 0 = MP cumplimiento, 1 = Otros tipos, 2 = MP Pedimentos
        private Dictionary<Button, bool>? estadoBotonesAntesCarga;
        public FrmCatalogoPartes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
            datosConsultadosMP = new List<MateriaPrimaBOM>();
            datosConsultadosOtros = new List<MateriaPrimaBOM>();
            datosOtrosFiltrados = new List<MateriaPrimaBOM>();
            resumenCumplimientoPorRazon = new List<ResumenCumplimiento>();
            chartPedimentosMP = CrearChartPedimentos();
            chartControl = chartEstatus;
            DataGridViewManualCopyHelper.ConfigurarControles(this);
            dgvMateriaPrima.CellDoubleClick += dgvMateriaPrima_CellDoubleClick;
            DataGridViewManualCopyHelper.Configurar(dgvMateriaPrima);
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

        private static bool EstaEnBom(MateriaPrimaBOM item)
        {
            return string.Equals(item.EstatusComponente, "VIGENTE EN BOM", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EstaEnPedimento(MateriaPrimaBOM item)
        {
            return string.Equals(item.DetallePedimentosGlosa, "SI", StringComparison.OrdinalIgnoreCase);
        }

        private static ResumenCumplimiento CrearResumenCumplimiento(string etiqueta, IEnumerable<MateriaPrimaBOM> datos)
        {
            List<MateriaPrimaBOM> datosLista = datos.ToList();

            return new ResumenCumplimiento
            {
                Etiqueta = etiqueta,
                TotalSubidos = datosLista.Count,
                EnBom = datosLista.Count(EstaEnBom),
                EnPedimento = datosLista.Count(EstaEnPedimento),
                Cumplen = datosLista.Count(item => EstaEnBom(item) && EstaEnPedimento(item))
            };
        }

        private List<ResumenCumplimiento> ConstruirResumenCumplimientoPorRazon(List<MateriaPrimaBOM> datos)
        {
            if (datos == null || datos.Count == 0)
                return new List<ResumenCumplimiento>();

            if (consultarTodasRazonesSociales)
            {
                return datos
                    .GroupBy(item => string.IsNullOrWhiteSpace(item.RazonSocialOrigen)
                        ? "Sin razón social"
                        : item.RazonSocialOrigen.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(group => CrearResumenCumplimiento(group.Key, group))
                    .OrderBy(item => item.Etiqueta, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return new List<ResumenCumplimiento>
            {
                CrearResumenCumplimiento(ObtenerNombreRazonConsultaActual(), datos)
            };
        }

        private void AsignarRazonSocialOrigen(IEnumerable<MateriaPrimaBOM> datos, string razonSocial)
        {
            foreach (var item in datos)
            {
                item.RazonSocialOrigen = razonSocial;
            }
        }

        private string ObtenerNombreRazonConsultaActual()
        {
            if (consultarTodasRazonesSociales)
                return "Todas las razones sociales";

            return ObtenerNombreRazonSocialSeleccionada();
        }

        private static string AcortarTexto(string texto, int longitudMaxima)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Length <= longitudMaxima)
                return texto;

            return texto[..Math.Max(0, longitudMaxima - 3)] + "...";
        }

        private LiveChartsCore.SkiaSharpView.WinForms.PieChart CrearChartPedimentos()
        {
            return new LiveChartsCore.SkiaSharpView.WinForms.PieChart
            {
                Dock = DockStyle.Fill,
                InitialRotation = 0D,
                IsClockwise = true,
                MaxAngle = 360D,
                MaxValue = double.NaN,
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
                    cboBaseDatos.DataSource = CrearItemsBaseDatos(basesDatos);
                    cboBaseDatos.DisplayMember = nameof(BaseDatosComboItem.NombreVisible);
                    cboBaseDatos.ValueMember = nameof(BaseDatosComboItem.NombreReal);
                    cboBaseDatos.Enabled = !chkPdfTodasEmpresas.Checked;
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

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            bool consultarTodasLasRazones = chkTodasRazonesSociales.Checked;

            if (!consultarTodasLasRazones && (cboRazonSocial.SelectedValue == null || cboRazonSocial.SelectedValue is not int))
            {
                MessageBox.Show("Por favor seleccione una razón social.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!consultarTodasLasRazones && !chkPdfTodasEmpresas.Checked && cboBaseDatos.SelectedItem == null)
            {
                MessageBox.Show("Por favor seleccione una base de datos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idRazon = consultarTodasLasRazones ? 0 : Convert.ToInt32(cboRazonSocial.SelectedValue);
            string baseDatos = cboBaseDatos.SelectedValue?.ToString() ?? string.Empty;
            string nombreRazonSeleccionada = cboRazonSocial.Text?.Trim() ?? string.Empty;
            DateTime fechaInicio = dtpFechaInicio.Value;
            DateTime fechaFin = dtpFechaFin.Value;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool consultaExitosa = false;

            try
            {
                MostrarPanelCargando(true);
                EstablecerEstadoBotonesDuranteCarga(true);
                btnConsultar.Enabled = false;

                // Pequeño delay para asegurar que la UI se actualice
                await Task.Delay(50);

                // Ejecutar ambas consultas en paralelo
                bool consultarTodasEmpresas = !consultarTodasLasRazones && chkPdfTodasEmpresas.Checked;
                consultarTodasEmpresasRazonSocial = consultarTodasEmpresas;
                consultarTodasRazonesSociales = consultarTodasLasRazones;

                var tareaMP = Task.Run(() => consultarTodasLasRazones
                    ? catalogoService.ObtenerMateriaPrimaBOMPorTodasLasRazones("MP", fechaInicio, fechaFin)
                    : consultarTodasEmpresas
                        ? catalogoService.ObtenerMateriaPrimaBOMPorRazonSocial(idRazon, "MP", fechaInicio, fechaFin)
                        : catalogoService.ObtenerMateriaPrimaBOM(baseDatos, "MP", fechaInicio, fechaFin));

                var tareaOtros = Task.Run(() => consultarTodasLasRazones
                    ? catalogoService.ObtenerMateriaPrimaBOMMultiplePorTodasLasRazones(fechaInicio, fechaFin)
                    : consultarTodasEmpresas
                        ? catalogoService.ObtenerMateriaPrimaBOMMultiplePorRazonSocial(idRazon, fechaInicio, fechaFin)
                        : catalogoService.ObtenerMateriaPrimaBOMMultiple(baseDatos, fechaInicio, fechaFin));

                await Task.WhenAll(tareaMP, tareaOtros);

                datosConsultadosMP = tareaMP.Result;
                datosConsultadosOtros = tareaOtros.Result;

                if (!consultarTodasLasRazones && !string.IsNullOrWhiteSpace(nombreRazonSeleccionada))
                {
                    AsignarRazonSocialOrigen(datosConsultadosMP, nombreRazonSeleccionada);
                    AsignarRazonSocialOrigen(datosConsultadosOtros, nombreRazonSeleccionada);
                }

                resumenCumplimientoPorRazon = ConstruirResumenCumplimientoPorRazon(datosConsultadosMP);
                datosOtrosFiltrados = new List<MateriaPrimaBOM>(datosConsultadosOtros);

                // Mostrar vista de MP por defecto
                vistaActual = 0;
                MostrarVistaMP();
                consultaExitosa = true;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al consultar datos:\n{ex.Message}",
                    "Error", ex, "Consulta de catálogo de partes");
            }
            finally
            {
                bool estadoExportarExcel = btnExportarExcel.Enabled;
                bool estadoExportarPdf = btnExportarPdf.Enabled;
                bool estadoGraficaIndividual = btnGraficaIndividual.Enabled;
                bool estadoGraficaTodos = btnGraficaTodos.Enabled;

                MostrarPanelCargando(false);
                EstablecerEstadoBotonesDuranteCarga(false);

                btnExportarExcel.Enabled = estadoExportarExcel;
                btnExportarPdf.Enabled = estadoExportarPdf;
                btnGraficaIndividual.Enabled = estadoGraficaIndividual;
                btnGraficaTodos.Enabled = estadoGraficaTodos;

                if (consultaExitosa)
                {
                    bool hayDatos = datosConsultadosMP.Count > 0 || datosConsultadosOtros.Count > 0;
                    btnGraficaIndividual.Enabled = hayDatos;
                    btnGraficaTodos.Enabled = hayDatos;
                    btnExportarExcel.Enabled = hayDatos;
                    btnExportarPdf.Enabled = hayDatos;
                }
            }
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            if ((datosConsultadosMP == null || datosConsultadosMP.Count == 0) && (datosConsultadosOtros == null || datosConsultadosOtros.Count == 0))
            {
                MessageBox.Show("No hay datos para exportar.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                Title = "Guardar Catálogo como Excel",
                FileName = $"Catalogo_Partes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var workbook = new XLWorkbook();
                ExportarHojaMateriaPrima(workbook);
                ExportarHojaGeneral(workbook);
                workbook.SaveAs(saveDialog.FileName);

                MessageBox.Show("Archivo exportado exitosamente.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al exportar a Excel: {ex.Message}",
                    "Error", ex, "Exportación a Excel de catálogo de partes");
            }
        }

        private void ExportarHojaMateriaPrima(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Materia Prima MP");
            worksheet.Cell(1, 1).Value = "Número de Parte";
            worksheet.Cell(1, 2).Value = "Descripción";
            worksheet.Cell(1, 3).Value = "Fecha Inserción";
            worksheet.Cell(1, 4).Value = "Estatus en BOM";
            worksheet.Cell(1, 5).Value = "En Pedimento";

            int fila = 2;
            foreach (var item in datosConsultadosMP)
            {
                worksheet.Cell(fila, 1).Value = item.Par_NoParte;
                worksheet.Cell(fila, 2).Value = item.Par_DescripcionEsp;
                worksheet.Cell(fila, 3).Value = item.Par_InsercionFecha;
                worksheet.Cell(fila, 4).Value = item.EstatusComponente;
                worksheet.Cell(fila, 5).Value = item.DetallePedimentosGlosa;
                fila++;
            }

            worksheet.Columns().AdjustToContents();
        }

        private void ExportarHojaGeneral(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("General");
            worksheet.Cell(1, 1).Value = "Tipo";
            worksheet.Cell(1, 2).Value = "Número de Parte";
            worksheet.Cell(1, 3).Value = "Descripción";
            worksheet.Cell(1, 4).Value = "Fecha Inserción";
            worksheet.Cell(1, 5).Value = "Estatus en BOM";

            int fila = 2;
            foreach (var item in ObtenerDatosOtrosParaExportacion())
            {
                worksheet.Cell(fila, 1).Value = item.Clave;
                worksheet.Cell(fila, 2).Value = item.Par_NoParte;
                worksheet.Cell(fila, 3).Value = item.Par_DescripcionEsp;
                worksheet.Cell(fila, 4).Value = item.Par_InsercionFecha;
                worksheet.Cell(fila, 5).Value = item.EstatusComponente;
                fila++;
            }

            worksheet.Columns().AdjustToContents();
        }

        private IEnumerable<MateriaPrimaBOM> ObtenerDatosOtrosParaExportacion()
        {
            return vistaActual == 1 ? datosOtrosFiltrados : datosConsultadosOtros;
        }

        private void ConfigurarColumnasMateriaPrima(bool mostrarTipo, bool mostrarPedimento, bool mostrarRazonSocial)
        {
            if (dgvMateriaPrima.Columns.Count == 0)
                return;

            dgvMateriaPrima.Columns["Par_NoParte"].HeaderText = "Número de Parte";
            dgvMateriaPrima.Columns["Par_DescripcionEsp"].HeaderText = "Descripción";
            dgvMateriaPrima.Columns["Par_InsercionFecha"].HeaderText = "Fecha Inserción";
            dgvMateriaPrima.Columns["EstatusComponente"].HeaderText = "Estatus en BOM";
            dgvMateriaPrima.Columns["Par_Consecutivo"].Visible = false;
            dgvMateriaPrima.Columns["BaseDatosOrigenConsulta"].Visible = false;
            dgvMateriaPrima.Columns["Par_InsercionFecha"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgvMateriaPrima.Columns.Contains("RazonSocialOrigen"))
            {
                dgvMateriaPrima.Columns["RazonSocialOrigen"].HeaderText = "Razón Social";
                dgvMateriaPrima.Columns["RazonSocialOrigen"].Visible = mostrarRazonSocial;
                dgvMateriaPrima.Columns["RazonSocialOrigen"].DisplayIndex = 0;
            }

            if (dgvMateriaPrima.Columns.Contains("Clave"))
            {
                dgvMateriaPrima.Columns["Clave"].Visible = mostrarTipo;
                dgvMateriaPrima.Columns["Clave"].HeaderText = "Tipo";
            }

            if (dgvMateriaPrima.Columns.Contains("DetallePedimentosGlosa"))
            {
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].HeaderText = "En Pedimento";
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].Visible = mostrarPedimento;
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvMateriaPrima.Columns["DetallePedimentosGlosa"].MinimumWidth = 80;
            }

            if (dgvMateriaPrima.Columns.Contains("DetallePedimentosInfo"))
            {
                dgvMateriaPrima.Columns["DetallePedimentosInfo"].Visible = false;
            }
        }

        private void MostrarVistaMP()
        {
            vistaActual = 0;
            cboTipoParte.Enabled = false;

            // Actualizar grid
            dgvMateriaPrima.DataSource = datosConsultadosMP;
            ConfigurarColumnasMateriaPrima(false, true, consultarTodasRazonesSociales);

            if (consultarTodasRazonesSociales)
            {
                ActualizarGraficoCumplimientoPorRazon(resumenCumplimientoPorRazon);
            }
            else
            {
                MostrarControlGrafico(chartEstatus);
                ActualizarGrafico(datosConsultadosMP);
            }

            ResumenCumplimiento resumenActual = CrearResumenCumplimiento("MP", datosConsultadosMP);
            lblTotalPartes.Text = $"Total de partes MP: {resumenActual.TotalSubidos:N0} | Cumplimiento: {resumenActual.PorcentajeCumplimiento:N2}%";
        }

        private void MostrarVistaPedimentosMP()
        {
            vistaActual = 2;
            cboTipoParte.Enabled = false;
            MostrarControlGrafico(chartPedimentosMP);

            dgvMateriaPrima.DataSource = datosConsultadosMP;
            ConfigurarColumnasMateriaPrima(false, true, consultarTodasRazonesSociales);

            ActualizarGraficoPedimentosMP(datosConsultadosMP);
            ResumenCumplimiento resumenActual = CrearResumenCumplimiento("MP", datosConsultadosMP);
            lblTotalPartes.Text = $"Total de partes MP: {resumenActual.TotalSubidos:N0} | En pedimento: {resumenActual.EnPedimento:N0}";
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
            ConfigurarColumnasMateriaPrima(true, false, consultarTodasRazonesSociales);

            // Actualizar gráfico de barras
            ActualizarGraficoBarras(datosOtrosFiltrados);

            // Actualizar total
            lblTotalPartes.Text = tipoSeleccionado == "Todos"
                ? $"Total de partes (EQ, MAQ, SUB, RT, AUX, PT): {datosOtrosFiltrados.Count:N0}"
                : $"Total de partes {tipoSeleccionado}: {datosOtrosFiltrados.Count:N0}";
        }

        private void ActualizarGrafico(List<MateriaPrimaBOM> datos)
        {
            ResumenCumplimiento resumen = CrearResumenCumplimiento("MP", datos);

            var serieCumplen = new PieSeries<int>
            {
                Values = new[] { resumen.Cumplen },
                Name = $"Cumplen: {resumen.Cumplen:N0} ({resumen.PorcentajeCumplimiento:N2}%)",
                Fill = new SolidColorPaint(SKColor.Parse("#2ecc71")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{resumen.Cumplen:N0}"
            };

            var serieNoCumplen = new PieSeries<int>
            {
                Values = new[] { resumen.NoCumplen },
                Name = $"No cumplen: {resumen.NoCumplen:N0} ({resumen.PorcentajeIncumplimiento:N2}%)",
                Fill = new SolidColorPaint(SKColor.Parse("#e74c3c")),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{resumen.NoCumplen:N0}"
            };

            chartEstatus.Series = new ISeries[] { serieCumplen, serieNoCumplen };

            chartEstatus.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = $"Cumplimiento MP\nTotal: {resumen.TotalSubidos:N0} | BOM: {resumen.EnBom:N0} | Pedimento: {resumen.EnPedimento:N0}",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };

            chartEstatus.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;

            // Ocultar tooltip para evitar duplicación
            chartEstatus.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        }

        private void ActualizarGraficoCumplimientoPorRazon(List<ResumenCumplimiento> resumenes)
        {
            var cartesianChart = chartControl as LiveChartsCore.SkiaSharpView.WinForms.CartesianChart;
            if (cartesianChart == null || chartControl == chartPedimentosMP || chartControl == chartEstatus)
            {
                cartesianChart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
                {
                    Dock = DockStyle.Fill,
                    Name = "chartCumplimientoRazones"
                };
            }

            MostrarControlGrafico(cartesianChart);

            List<ResumenCumplimiento> datos = resumenes ?? new List<ResumenCumplimiento>();
            string[] etiquetas = datos.Select(x => AcortarTexto(x.Etiqueta, 16)).ToArray();
            double[] valores = datos.Select(x => x.PorcentajeCumplimiento).ToArray();

            cartesianChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Cumplimiento",
                    Values = valores,
                    Fill = new SolidColorPaint(SKColor.Parse("#2ecc71")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:N2}%",
                    MaxBarWidth = 45
                }
            };

            cartesianChart.XAxes = new LiveChartsCore.SkiaSharpView.Axis[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Labels = etiquetas,
                    LabelsRotation = 12,
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

            cartesianChart.YAxes = new LiveChartsCore.SkiaSharpView.Axis[]
            {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    Labeler = value => $"{value:N0}%",
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

            cartesianChart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            cartesianChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
            cartesianChart.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = "Cumplimiento MP por razón social",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };
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
                    await Task.Run(() => GenerarPdfCatalogo(saveDialog.FileName));

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

            string nombreBaseDatos = ObtenerNombreBaseDatosSeleccionada();
            string nombreRazonSocial = ObtenerNombreRazonSocialSeleccionada();
            List<MateriaPrimaBOM> datosMpPdf = datosConsultadosMP;
            List<MateriaPrimaBOM> datosOtrosPdf = datosConsultadosOtros;

            if (consultarTodasRazonesSociales)
            {
                nombreRazonSocial = "Todas las razones sociales";
                nombreBaseDatos = "Todas las empresas";
            }
            else if (consultarTodasEmpresasRazonSocial)
            {
                nombreBaseDatos = "Todas las empresas de la razón social";
            }

            List<string> basesConsultadas = ObtenerBasesConsultadasPdf(datosMpPdf, datosOtrosPdf, nombreBaseDatos);
            List<ResumenCumplimiento> resumenesCumplimiento = ConstruirResumenCumplimientoPorRazon(datosMpPdf);
            ResumenCumplimiento resumenGeneral = CrearResumenCumplimiento(nombreRazonSocial, datosMpPdf);
            var mpVigentes = datosMpPdf
                .Where(EstaEnBom)
                .OrderBy(d => d.BaseDatosOrigenConsulta)
                .ThenBy(d => d.Par_NoParte)
                .ToList();
            var mpNoVigentes = datosMpPdf
                .Where(d => !EstaEnBom(d))
                .OrderBy(d => d.BaseDatosOrigenConsulta)
                .ThenBy(d => d.Par_NoParte)
                .ToList();
            var mpConPedimento = datosMpPdf
                .Where(EstaEnPedimento)
                .OrderBy(d => d.BaseDatosOrigenConsulta)
                .ThenBy(d => d.Par_NoParte)
                .ToList();
            var mpSinPedimento = datosMpPdf
                .Where(d => !EstaEnPedimento(d))
                .OrderBy(d => d.BaseDatosOrigenConsulta)
                .ThenBy(d => d.Par_NoParte)
                .ToList();
            var datosOtrosOrdenados = datosOtrosPdf
                .OrderBy(d => d.Clave)
                .ThenBy(d => d.BaseDatosOrigenConsulta)
                .ThenBy(d => d.Par_NoParte)
                .ToList();

            // Generar imágenes de ambos gráficos
            byte[] imagenGraficoMP = GenerarImagenGrafico(resumenGeneral.Cumplen, resumenGeneral.NoCumplen, "Cumplimiento MP", "Cumplen", "No cumplen", "#2ecc71", "#e74c3c");
            byte[] imagenGraficoPedimentosMP = GenerarImagenGrafico(resumenGeneral.EnPedimento, Math.Max(0, resumenGeneral.TotalSubidos - resumenGeneral.EnPedimento), "Partes en Pedimento", "En pedimento", "Sin pedimento", "#3498db", "#95a5a6");
            byte[] imagenGraficoBarras = GenerarImagenGraficoBarras(datosOtrosPdf);
            byte[] imagenGraficoCumplimientoRazones = GenerarImagenGraficoCumplimientoPorRazon(resumenesCumplimiento);

            List<ResumenTipoParte> agrupado = datosOtrosPdf.GroupBy(d => d.Clave)
                .Select(g => new ResumenTipoParte { Tipo = g.Key, Total = g.Count() })
                .OrderBy(x => x.Tipo)
                .ToList();

            List<string> resumenMp = new List<string>
            {
                $"Total subidos: {resumenGeneral.TotalSubidos:N0}",
                $"En BOM: {resumenGeneral.EnBom:N0}",
                $"En pedimento: {resumenGeneral.EnPedimento:N0}",
                $"Cumplen BOM + pedimento: {resumenGeneral.Cumplen:N0}",
                $"No cumplen: {resumenGeneral.NoCumplen:N0}",
                $"Porcentaje de cumplimiento: {resumenGeneral.PorcentajeCumplimiento:N2}%"
            };

            List<string> resumenPedimentos = new List<string>
            {
                $"Total subidos: {resumenGeneral.TotalSubidos:N0}",
                $"En pedimento: {resumenGeneral.EnPedimento:N0}",
                $"Sin pedimento: {Math.Max(0, resumenGeneral.TotalSubidos - resumenGeneral.EnPedimento):N0}",
                $"En BOM: {resumenGeneral.EnBom:N0}",
                $"Cumplen BOM + pedimento: {resumenGeneral.Cumplen:N0}"
            };

            Document.Create(container =>
            {
                AgregarPaginaGraficoResumen(
                    container,
                    "CUMPLIMIENTO DE MATERIA PRIMA (MP)",
                    nombreRazonSocial,
                    nombreBaseDatos,
                    basesConsultadas,
                    imagenGraficoMP,
                    resumenMp);

                AgregarPaginaGraficoResumen(
                    container,
                    "PARTES EN PEDIMENTO",
                    nombreRazonSocial,
                    nombreBaseDatos,
                    basesConsultadas,
                    imagenGraficoPedimentosMP,
                    resumenPedimentos);

                if (consultarTodasRazonesSociales)
                {
                    double promedio = resumenesCumplimiento.Count == 0
                        ? 0D
                        : resumenesCumplimiento.Average(x => x.PorcentajeCumplimiento);
                    ResumenCumplimiento? mejor = resumenesCumplimiento.OrderByDescending(x => x.PorcentajeCumplimiento).FirstOrDefault();
                    ResumenCumplimiento? menor = resumenesCumplimiento.OrderBy(x => x.PorcentajeCumplimiento).FirstOrDefault();

                    AgregarPaginaGraficoCumplimientoPorRazon(
                        container,
                        "CUMPLIMIENTO MP POR RAZÓN SOCIAL",
                        nombreRazonSocial,
                        nombreBaseDatos,
                        basesConsultadas,
                        imagenGraficoCumplimientoRazones,
                        new List<string>
                        {
                            $"Razones analizadas: {resumenesCumplimiento.Count:N0}",
                            $"Promedio de cumplimiento: {promedio:N2}%",
                            $"Mayor cumplimiento: {(mejor == null ? "N/D" : $"{mejor.Etiqueta} ({mejor.PorcentajeCumplimiento:N2}%)")}",
                            $"Menor cumplimiento: {(menor == null ? "N/D" : $"{menor.Etiqueta} ({menor.PorcentajeCumplimiento:N2}%)")}",
                            $"Total de partes MP consideradas: {resumenGeneral.TotalSubidos:N0}"
                        },
                        resumenesCumplimiento);
                }
                else if (datosOtrosOrdenados.Count > 0)
                {
                    AgregarPaginaGraficoResumen(
                        container,
                        "GENERAL DE OTROS TIPOS",
                        nombreRazonSocial,
                        nombreBaseDatos,
                        basesConsultadas,
                        imagenGraficoBarras,
                        new List<string> { $"Total general: {datosOtrosOrdenados.Count:N0}" }
                            .Concat(agrupado.Select(x => $"{x.Tipo}: {x.Total:N0}"))
                            .ToList());
                }
            }).GeneratePdf(rutaArchivo);
        }

        private void AgregarPaginaGraficoResumen(IDocumentContainer container, string titulo, string razonSocial, string nombreBaseDatos, List<string> basesConsultadas, byte[] imagen, List<string> resumenes)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Column(column =>
                {
                    column.Item().Text(titulo)
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

                        row.RelativeItem().AlignRight().Text(txt =>
                        {
                            txt.Span("Base de Datos: ").Bold();
                            txt.Span(nombreBaseDatos);
                        });
                    });

                    column.Item().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8));
                        txt.Span("Bases consultadas: ").Bold();
                        txt.Span(string.Join(", ", basesConsultadas));
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
                            txt.Span("Fecha de Generación: ").Bold();
                            txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });
                    });

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(8).Column(column =>
                {
                    column.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(resumen =>
                    {
                        resumen.Item().Text("Resumen general").Bold().FontColor(Colors.Blue.Darken2);

                        foreach (string linea in resumenes)
                        {
                            resumen.Item().Text(linea).FontSize(8);
                        }
                    });

                    column.Item().PaddingTop(10).MaxHeight(360).AlignCenter().Image(imagen).FitArea();
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        }

        private void AgregarPaginaGraficoCumplimientoPorRazon(IDocumentContainer container, string titulo, string razonSocial, string nombreBaseDatos, List<string> basesConsultadas, byte[] imagen, List<string> resumenes, List<ResumenCumplimiento> detalles)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Column(column =>
                {
                    column.Item().Text(titulo)
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

                        row.RelativeItem().AlignRight().Text(txt =>
                        {
                            txt.Span("Base de Datos: ").Bold();
                            txt.Span(nombreBaseDatos);
                        });
                    });

                    column.Item().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8));
                        txt.Span("Bases consultadas: ").Bold();
                        txt.Span(string.Join(", ", basesConsultadas));
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
                            txt.Span("Fecha de Generación: ").Bold();
                            txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });
                    });

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(8).Column(column =>
                {
                    column.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(resumen =>
                    {
                        resumen.Item().Text("Resumen general").Bold().FontColor(Colors.Blue.Darken2);

                        foreach (string linea in resumenes)
                        {
                            resumen.Item().Text(linea).FontSize(8);
                        }
                    });

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                            .MaxHeight(360).AlignCenter().Image(imagen).FitArea();

                        row.RelativeItem(2).PaddingLeft(10).Background(Colors.Grey.Lighten5).Padding(8).Column(detalle =>
                        {
                            detalle.Item().Text("Detalle por razón social")
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            for (int i = 0; i < detalles.Count; i++)
                            {
                                ResumenCumplimiento item = detalles[i];
                                string clave = $"R{i + 1}";

                                detalle.Item().Text(txt =>
                                {
                                    txt.DefaultTextStyle(x => x.FontSize(7));
                                    txt.Span($"{clave} ").Bold().FontColor(Colors.Blue.Darken2);
                                    txt.Span($"{item.Etiqueta}: ").Bold();
                                    txt.Span($"CV: {item.PorcentajeCumplimiento:N1}% ({item.Cumplen:N0}/{item.TotalSubidos:N0})");
                                });
                            }
                        });
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        }

        private void AgregarPaginaDetalleMp(IDocumentContainer container, string titulo, string razonSocial, List<string> basesConsultadas, List<MateriaPrimaBOM> datos)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Column(column =>
                {
                    column.Item().Text(titulo)
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().Text(txt =>
                    {
                        txt.Span("Razón Social: ").Bold();
                        txt.Span(razonSocial);
                    });

                    column.Item().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8));
                        txt.Span("Bases consultadas: ").Bold();
                        txt.Span(string.Join(", ", basesConsultadas));
                    });

                    column.Item().Text(txt =>
                    {
                        txt.Span("Total: ").Bold();
                        txt.Span($"{datos.Count:N0}");
                    });

                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte / Empresa").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("En Pedimento").FontColor(Colors.White).Bold();
                    });

                    foreach (var item in datos)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatearNumeroPartePdf(item)).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_DescripcionEsp).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_InsercionFecha?.ToString("dd/MM/yyyy") ?? "N/A").FontSize(8);

                        var estatusColor = string.Equals(item.EstatusComponente, "VIGENTE EN BOM", StringComparison.OrdinalIgnoreCase)
                            ? Colors.Green.Darken2
                            : Colors.Red.Darken2;

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.EstatusComponente).FontColor(estatusColor).Bold().FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.DetallePedimentosGlosa).FontSize(8);
                    }
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        }

        private void AgregarPaginaDetalleGeneral(IDocumentContainer container, string razonSocial, List<string> basesConsultadas, List<MateriaPrimaBOM> datos, List<ResumenTipoParte> agrupado)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Column(column =>
                {
                    column.Item().Text("GENERAL DE NÚMEROS DE PARTE Y OTROS TIPOS")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().Text(txt =>
                    {
                        txt.Span("Razón Social: ").Bold();
                        txt.Span(razonSocial);
                    });

                    column.Item().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8));
                        txt.Span("Bases consultadas: ").Bold();
                        txt.Span(string.Join(", ", basesConsultadas));
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Total general: ").Bold();
                            txt.Span($"{datos.Count:N0}");
                        });

                        foreach (var grupo in agrupado)
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.DefaultTextStyle(x => x.FontSize(8));
                                txt.Span($"{grupo.Tipo}: ").Bold().FontColor(Colors.Blue.Darken1);
                                txt.Span($"{grupo.Total:N0}");
                            });
                        }
                    });

                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(3.5f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tipo").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Número de Parte / Empresa").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha Inserción").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estatus en BOM").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Pedimento").FontColor(Colors.White).Bold();
                    });

                    foreach (var item in datos)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Clave).Bold().FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatearNumeroPartePdf(item)).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_DescripcionEsp).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Par_InsercionFecha?.ToString("dd/MM/yyyy") ?? "N/A").FontSize(8);

                        var estatusColor = string.Equals(item.EstatusComponente, "VIGENTE EN BOM", StringComparison.OrdinalIgnoreCase)
                            ? Colors.Green.Darken2
                            : Colors.Red.Darken2;

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.EstatusComponente).FontColor(estatusColor).Bold().FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.DetallePedimentosGlosa).FontSize(8);
                    }
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        }

        private List<string> ObtenerBasesConsultadasPdf(List<MateriaPrimaBOM> datosMpPdf, List<MateriaPrimaBOM> datosOtrosPdf, string nombreBaseDatosFallback)
        {
            var bases = datosMpPdf
                .Concat(datosOtrosPdf)
                .Select(x => x.BaseDatosOrigenConsulta)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(LimpiarNombreBaseDatosVisible)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (bases.Count == 0 && !string.IsNullOrWhiteSpace(nombreBaseDatosFallback))
            {
                bases.Add(nombreBaseDatosFallback);
            }

            return bases;
        }

        private string ObtenerNombreBaseDatosSeleccionada()
        {
            if (cboBaseDatos.InvokeRequired)
            {
                return (string)cboBaseDatos.Invoke(new Func<string>(ObtenerNombreBaseDatosSeleccionada));
            }

            return cboBaseDatos.Text?.Trim() ?? string.Empty;
        }

        private string ObtenerNombreRazonSocialSeleccionada()
        {
            if (cboRazonSocial.InvokeRequired)
            {
                return (string)cboRazonSocial.Invoke(new Func<string>(ObtenerNombreRazonSocialSeleccionada));
            }

            return cboRazonSocial.Text?.Trim() ?? string.Empty;
        }

        private string FormatearNumeroPartePdf(MateriaPrimaBOM item)
        {
            bool incluirRazon = consultarTodasRazonesSociales && !string.IsNullOrWhiteSpace(item.RazonSocialOrigen);
            bool incluirEmpresa = !string.IsNullOrWhiteSpace(item.BaseDatosOrigenConsulta);

            if (!incluirEmpresa && !incluirRazon)
                return item.Par_NoParte;

            string empresa = incluirEmpresa ? LimpiarNombreBaseDatosVisible(item.BaseDatosOrigenConsulta) : string.Empty;
            string razon = incluirRazon ? item.RazonSocialOrigen.Trim() : string.Empty;

            if (incluirEmpresa && incluirRazon)
                return $"{item.Par_NoParte} ({empresa} - {razon})";

            return incluirEmpresa
                ? $"{item.Par_NoParte} ({empresa})"
                : $"{item.Par_NoParte} ({razon})";
        }

        private void chkPdfTodasEmpresas_CheckedChanged(object sender, EventArgs e)
        {
            bool consultarTodas = chkPdfTodasEmpresas.Checked;
            cboBaseDatos.Enabled = !consultarTodas && cboBaseDatos.DataSource != null && !chkTodasRazonesSociales.Checked;

            if (consultarTodas)
            {
                cboBaseDatos.SelectedIndex = -1;
            }
        }

        private void chkTodasRazonesSociales_CheckedChanged(object sender, EventArgs e)
        {
            bool consultarTodas = chkTodasRazonesSociales.Checked;

            if (consultarTodas)
            {
                chkPdfTodasEmpresas.Checked = false;
            }

            cboRazonSocial.Enabled = !consultarTodas;
            chkPdfTodasEmpresas.Enabled = !consultarTodas;
            cboBaseDatos.Enabled = !consultarTodas && !chkPdfTodasEmpresas.Checked && cboBaseDatos.DataSource != null;
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

            PdfGeneradorService.DrawTextBlob(canvas, titulo, width / 2f, 28, 18, SKColor.Parse("#2c3e50"), SKTextAlign.Center);

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
            if (valorPrimario > 0)
            {
                float angle1 = (startAngle + sweepAnglePrimario / 2) * (float)Math.PI / 180;
                float labelX1 = centerX + (radius * 0.6f) * (float)Math.Cos(angle1);
                float labelY1 = centerY + (radius * 0.6f) * (float)Math.Sin(angle1);
                PdfGeneradorService.DrawTextBlob(canvas, $"{valorPrimario}", labelX1, labelY1, 16, SKColors.White, SKTextAlign.Center);
            }

            if (valorSecundario > 0)
            {
                float angle2 = (startAngle + sweepAnglePrimario + sweepAngleSecundario / 2) * (float)Math.PI / 180;
                float labelX2 = centerX + (radius * 0.6f) * (float)Math.Cos(angle2);
                float labelY2 = centerY + (radius * 0.6f) * (float)Math.Sin(angle2);
                PdfGeneradorService.DrawTextBlob(canvas, $"{valorSecundario}", labelX2, labelY2, 16, SKColors.White, SKTextAlign.Center);
            }

            // Dibujar leyenda
            float legendY = height - 60;
            float legendX = 50;
            float boxSize = 15;

            using (var paint = new SKPaint { Color = colorPrimario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            PdfGeneradorService.DrawTextBlob(canvas, $"{etiquetaPrimaria}: {valorPrimario:N0}", legendX + boxSize + 10, legendY + 12, 12, SKColors.Black, SKTextAlign.Left);

            legendY += 25;
            using (var paint = new SKPaint { Color = colorSecundario, Style = SKPaintStyle.Fill, IsAntialias = true })
            {
                canvas.DrawRect(legendX, legendY, boxSize, boxSize, paint);
            }

            PdfGeneradorService.DrawTextBlob(canvas, $"{etiquetaSecundaria}: {valorSecundario:N0}", legendX + boxSize + 10, legendY + 12, 12, SKColors.Black, SKTextAlign.Left);

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
                PdfGeneradorService.DrawTextBlob(canvas, "Sin datos", width / 2, height / 2, 14, SKColors.Black, SKTextAlign.Center);

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
                PdfGeneradorService.DrawTextBlob(canvas, item.Total.ToString(), x + barWidth / 2, y - 5, 12, SKColors.Black, SKTextAlign.Center);

                // Dibujar etiqueta del tipo
                PdfGeneradorService.DrawTextBlob(canvas, item.Tipo, x + barWidth / 2, marginTop + chartHeight + 20, 11, SKColors.Black, SKTextAlign.Center);
            }

            // Dibujar escala del eje Y
            {
                int steps = 5;
                for (int i = 0; i <= steps; i++)
                {
                    int value = (maxValue / steps) * i;
                    float y = marginTop + chartHeight - (chartHeight / steps * i);
                    PdfGeneradorService.DrawTextBlob(canvas, value.ToString(), marginLeft - 10, y + 4, 10, SKColors.Black, SKTextAlign.Right);
                }
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }

        private byte[] GenerarImagenGraficoCumplimientoPorRazon(List<ResumenCumplimiento> resumenes)
        {
            int width = 520;
            int height = 330;

            var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            List<ResumenCumplimiento> datos = resumenes ?? new List<ResumenCumplimiento>();

            PdfGeneradorService.DrawTextBlob(canvas, "Cumplimiento MP por razón social", width / 2f, 24, 16, SKColor.Parse("#2c3e50"), SKTextAlign.Center);

            if (datos.Count == 0)
            {
                PdfGeneradorService.DrawTextBlob(canvas, "Sin datos", width / 2f, height / 2f, 14, SKColors.Black, SKTextAlign.Center);

                using var imageVacio = surface.Snapshot();
                using var dataVacia = imageVacio.Encode(SKEncodedImageFormat.Png, 100);
                return dataVacia.ToArray();
            }

            float marginLeft = 60;
            float marginRight = 20;
            float marginTop = 45;
            float marginBottom = 70;
            float chartWidth = width - marginLeft - marginRight;
            float chartHeight = height - marginTop - marginBottom;
            float barWidth = chartWidth / datos.Count * 0.6f;
            float barSpacing = chartWidth / datos.Count;

            using (var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true })
            {
                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + chartHeight, axisPaint);
                canvas.DrawLine(marginLeft, marginTop + chartHeight, marginLeft + chartWidth, marginTop + chartHeight, axisPaint);
            }

            for (int i = 0; i <= 5; i++)
            {
                float y = marginTop + chartHeight - (chartHeight / 5 * i);
                int value = i * 20;
                PdfGeneradorService.DrawTextBlob(canvas, $"{value}%", marginLeft - 10, y + 4, 10, SKColors.Black, SKTextAlign.Right);
            }

            for (int i = 0; i < datos.Count; i++)
            {
                ResumenCumplimiento item = datos[i];
                float barHeight = (float)(item.PorcentajeCumplimiento / 100D) * chartHeight;
                float x = marginLeft + (i * barSpacing) + (barSpacing - barWidth) / 2;
                float y = marginTop + chartHeight - barHeight;

                SKColor colorBarra = item.PorcentajeCumplimiento > 50D
                    ? SKColor.Parse("#2ecc71")
                    : SKColor.Parse("#e74c3c");

                using (var barPaint = new SKPaint { Color = colorBarra, Style = SKPaintStyle.Fill, IsAntialias = true })
                {
                    canvas.DrawRect(x, y, barWidth, barHeight, barPaint);
                }

                string clave = $"R{i + 1}";
                PdfGeneradorService.DrawTextBlob(canvas, $"{item.PorcentajeCumplimiento:N1}%", x + barWidth / 2, y - 6, 11, SKColors.Black, SKTextAlign.Center);
                PdfGeneradorService.DrawTextBlob(canvas, clave, x + barWidth / 2, marginTop + chartHeight + 18, 10, SKColors.Black, SKTextAlign.Center);
                PdfGeneradorService.DrawTextBlob(canvas, $"{item.Cumplen:N0}/{item.TotalSubidos:N0}", x + barWidth / 2, marginTop + chartHeight + 34, 9, SKColor.Parse("#2c3e50"), SKTextAlign.Center);
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
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
