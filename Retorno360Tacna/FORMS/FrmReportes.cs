using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using Retorno360Tacna.HELPERS;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.WinForms;
using System.Linq;
using System.Diagnostics;
using System.Data;
using ClosedXML.Excel;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportes : Form
    {
        private sealed class BaseDatosComboItem
        {
            public string NombreReal { get; set; } = string.Empty;
            public string NombreVisible { get; set; } = string.Empty;
        }

        private readonly ConexionInfo conexionActual;
        private readonly ReporteIGIService reporteService;
        private List<RazonSocial> razonesSociales = new();
        private List<ReporteIGIPagado> reporteActual = new();
        private CartesianChart? chartIGI;
        private CartesianChart? chartIVA;
        private int graficaActual = 0; // 0 = IGI, 1 = IVA
        private bool modalAbierto = false;
        private int ultimaFilaClickeada = -1;
        private Dictionary<Button, bool>? estadoBotonesAntesCarga;
        private MODELS.Usuario? usuarioActual;
        private SERVICES.PerfilUsuarioService? perfilService;

        // Tablas de detalle para mostrar al hacer doble clic
        private System.Data.DataTable? detalleIGIActual;
        private System.Data.DataTable? detalleIVAActual;

        public FrmReportes(ConexionInfo conexion) : this(conexion, null) { }

        public FrmReportes(ConexionInfo conexion, MODELS.Usuario? usuario)
        {
            InitializeComponent();
            conexionActual = conexion;
            reporteService = new ReporteIGIService(conexion);
            usuarioActual = usuario;

            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            if (SERVICES.ConfiguracionService.ObtenerAjusteVentanaPantallaLogica())
            {
                SERVICES.ConfiguracionService.AplicarPerfilPantallaLogica(this);
            }

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

        private void FrmReportes_Resize(object sender, EventArgs e)
        {
            AjustarControles();
        }

        private void FrmReportes_SizeChanged(object sender, EventArgs e)
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
                this.SuspendLayout();

                // El panelResumen ahora usa Dock = DockStyle.Bottom
                // por lo que no necesitamos ajustar su posición manualmente

                AjustarAreaGrafica(chartIGI, panelGrafica, lblTituloGrafica);
                AjustarAreaGrafica(chartIVA, panelGrafica, lblTituloGrafica);

                this.ResumeLayout(true);
            }
            catch
            {
                // Evitar errores durante el redimensionamiento
            }
        }

        private void AjustarAreaGrafica(CartesianChart? chart, Panel panelContenedor, Label titulo)
        {
            if (chart == null)
                return;

            int margen = 10;
            int top = titulo.Bottom + 8;
            int ancho = Math.Max(100, panelContenedor.ClientSize.Width - (margen * 2));
            int alto = Math.Max(120, panelContenedor.ClientSize.Height - top - margen);

            chart.Location = new Point(margen, top);
            chart.Size = new Size(ancho, alto);
            chart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart.BringToFront();
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

            panelGraficaIVA.Visible = false;
            MostrarGraficaActual();

            // Deshabilitar botón PDF al inicio
            btnGenerarPDF.Enabled = false;
            btnExportarExcel.Enabled = false;
        }

        private void MostrarGraficaActual()
        {
            panelGrafica.Visible = true;
            panelGrafica.BringToFront();
            panelGraficaIVA.Visible = false;

            if (chartIGI != null)
            {
                chartIGI.Visible = graficaActual == 0;
                if (chartIGI.Visible)
                    chartIGI.BringToFront();
            }

            if (chartIVA != null)
            {
                chartIVA.Visible = graficaActual == 1;
                if (chartIVA.Visible)
                    chartIVA.BringToFront();
            }

                // Forzar refresco y repintado al cambiar de gráfica para asegurar renderizado
            try
            {
                if (graficaActual == 0)
                {
                        try
                        {
                            chartIGI?.Update();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual] chartIGI.Update error: {ex}");
                        }
                        chartIGI?.Refresh();
                        chartIGI?.Invalidate();
                        try { panelGrafica?.Refresh(); } catch { }
                }
                else
                {
                        try
                        {
                            chartIVA?.Update();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual] chartIVA.Update error: {ex}");
                        }
                        chartIVA?.Refresh();
                        chartIVA?.Invalidate();
                        try { panelGrafica?.Refresh(); } catch { }
                }

                // Procesar eventos pendientes para asegurar que el control se repinte inmediatamente
                try { System.Windows.Forms.Application.DoEvents(); } catch { }
                System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual] Forzado Update/Refresh para graficaActual={graficaActual}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual] Error al forzar refresco: {ex.Message}");
            }

            // Información adicional de diagnóstico sobre series y ejes
            try
            {
                if (chartIGI != null)
                {
                    var seriesCount = chartIGI.Series?.Count() ?? 0;
                    var xLabels = chartIGI.XAxes?.FirstOrDefault()?.Labels?.Count ?? 0;
                    System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual-DBG] chartIGI series={seriesCount} xLabels={xLabels}");
                }

                if (chartIVA != null)
                {
                    var seriesCount = chartIVA.Series?.Count() ?? 0;
                    var xLabels = chartIVA.XAxes?.FirstOrDefault()?.Labels?.Count ?? 0;
                    System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual-DBG] chartIVA series={seriesCount} xLabels={xLabels}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual-DBG] Error leyendo propiedades: {ex.Message}");
            }

            lblTituloGrafica.Text = graficaActual == 0
                ? "IGI por Mes y Forma de Pago (1/2)"
                : "IVA por Mes y Forma de Pago (2/2)";

            System.Diagnostics.Debug.WriteLine($"[MostrarGraficaActual] graficaActual={graficaActual} chartIGI!=null:{chartIGI!=null} chartIVA!=null:{chartIVA!=null} chartIGI.Visible={chartIGI?.Visible} chartIVA.Visible={chartIVA?.Visible}");

            lblTituloGrafica.BringToFront();
            btnAnteriorGrafica.Visible = true;
            btnSiguienteGrafica.Visible = true;
            btnAnteriorGrafica.BringToFront();
            btnSiguienteGrafica.BringToFront();
        }

        // Nota: método auxiliar para mostrar tablas cuando hay discrepancias en los datos usados para graficar.
        private void MostrarDiagnosticoTablas(string tipo, System.Data.DataTable tabla)
        {
            try
            {
                using var frm = new Form();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Size = new Size(900, 500);
                frm.Text = $"Diagnóstico de datos para gráfica: {tipo}";
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;

                var dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                    DataSource = tabla.Copy()
                };

                var btnCerrar = new Button
                {
                    Text = "Cerrar",
                    Dock = DockStyle.Bottom,
                    Height = 36
                };
                btnCerrar.Click += (s, e) => frm.Close();

                frm.Controls.Add(dgv);
                frm.Controls.Add(btnCerrar);
                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en MostrarDiagnosticoTablas: {ex.Message}");
            }
        }

        private async void CargarRazonesSociales()
        {
            try
            {
                lblProgreso.Text = "Cargando razones sociales...";

                if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                {
                    int idUsuario = usuarioActual.IdUsuario;
                    razonesSociales = await Task.Run(() =>
                        perfilService.ObtenerRazonesSocialesDePerfil(idUsuario));
                }
                else
                {
                    await Task.Run(() =>
                    {
                        razonesSociales = reporteService.ObtenerRazonesSociales();
                    });
                }

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

                var basesDatos = (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                    ? perfilService.ObtenerBasesDatosDePerfilPorRazon(usuarioActual.IdUsuario, idRazon)
                    : reporteService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cmbCliente.DataSource = CrearItemsBaseDatos(basesDatos);
                    cmbCliente.DisplayMember = nameof(BaseDatosComboItem.NombreVisible);
                    cmbCliente.ValueMember = nameof(BaseDatosComboItem.NombreReal);
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
            // ── Modo perfil: consulta por empresas guardadas del usuario ───────
            if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
            {
                await ConsultarConPerfilAsync();
                return;
            }

            // ── Modo normal ────────────────────────────────────────────────────
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

        /// <summary>
        /// Flujo exclusivo del modo perfil: obtiene razones + empresas del usuario,
        /// pregunta por razón si hay más de una, y abre FrmResultadosPerfilIGI.
        /// </summary>
        private async Task ConsultarConPerfilAsync()
        {
            if (dtpFechaInicio.Value > dtpFechaFin.Value)
            {
                MessageBox.Show("La fecha inicial no puede ser mayor a la fecha final",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Obtener razones sociales del perfil
            List<MODELS.RazonSocial> razonesPerfil;
            try
            {
                razonesPerfil = await Task.Run(() =>
                    perfilService!.ObtenerRazonesSocialesDePerfil(usuarioActual!.IdUsuario));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el perfil del usuario:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (razonesPerfil.Count == 0)
            {
                MessageBox.Show("Tu perfil no tiene razones sociales configuradas.\n" +
                    "Ve a Configuración → Usuarios y empresas para asignarlas.",
                    "Perfil vacío", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. Si hay más de una razón social, preguntar cuál consultar primero
            MODELS.RazonSocial razonElegida;
            if (razonesPerfil.Count == 1)
            {
                razonElegida = razonesPerfil[0];
            }
            else
            {
                using var dlg = new FrmSeleccionRazonPerfil(razonesPerfil);
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.RazonSeleccionada == null)
                    return;
                razonElegida = dlg.RazonSeleccionada;
            }

            // 3. Obtener las empresas (bases de datos) del perfil para esa razón
            List<string> empresas;
            try
            {
                empresas = await Task.Run(() =>
                    perfilService!.ObtenerBasesDatosDePerfilPorRazon(
                        usuarioActual!.IdUsuario, razonElegida.IdRazon));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener las empresas del perfil:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (empresas.Count == 0)
            {
                MessageBox.Show($"No hay empresas configuradas en tu perfil para la razón social «{razonElegida.NombreRazon}».\n" +
                    "Ve a Configuración → Usuarios y empresas para asignarlas.",
                    "Sin empresas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 4. Abrir el form de resultados por perfil
            var frmResultados = new FrmResultadosPerfilIGI(
                reporteService,
                razonElegida,
                empresas,
                dtpFechaInicio.Value.Date,
                dtpFechaFin.Value.Date);

            // Mostrar embebido en el panel principal del MainMenu si es posible,
            // sino como formulario flotante independiente
            var mainMenu = this.FindForm() as MainMenu;
            if (mainMenu != null)
            {
                mainMenu.MostrarFormularioEnPanel(frmResultados, limpiarPanelPrimero: false);
            }
            else
            {
                frmResultados.FormBorderStyle = FormBorderStyle.Sizable;
                frmResultados.ShowDialog(this);
            }
        }

        private async Task GenerarReporte()
        {
            try
            {
                // Mostrar indicador de carga
                MostrarPanelCargando(true);
                EstablecerEstadoBotonesDuranteCarga(true);

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

                System.Data.DataTable tablaIGI;
                System.Data.DataTable tablaIVA;
                bool faltaGlosaIva = false;
                var basesSinGlosaIva = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (sinValidacionGlosa)
                {
                    // Consultar todas las bases de datos de la razón social y agregar resultados
                    var razonSeleccionada = (RazonSocial)cmbRazonSocial.SelectedItem;
                    lblProgreso.Text = $"Consultando todas las bases de {razonSeleccionada.NombreRazon} para generar reporte...";
                    lblResumenInfo.Text = "Generando reporte...";

                    var resultado = await Task.Run(() =>
                    {
                        var bases = reporteService.ObtenerBasesDatosRazon(razonSeleccionada.IdRazon);

                        // Agregadores en memoria para resúmenes
                        var aggIGI = new Dictionary<(int Año, int Mes, string Forma), (decimal IGI_Pagado, decimal IGI_Calculado, decimal Diferencia)>();
                        var aggIVA = new Dictionary<(int Año, int Mes, string Forma), decimal>();

                        // Agregador de detalles
                        var todosDetallesIGI = new System.Data.DataTable();
                        var todosDetallesIVA = new System.Data.DataTable();
                        bool primeraIteracion = true;

                        foreach (var baseDb in bases)
                        {
                            try
                            {
                                var conciliacion = reporteService.ObtenerConciliacionIGI(baseDb, fechaInicio, fechaFin);

                                if (conciliacion.FaltaGlosaIVA)
                                {
                                    faltaGlosaIva = true;
                                    basesSinGlosaIva.Add(string.IsNullOrWhiteSpace(conciliacion.BaseDatosGlosa) ? baseDb : conciliacion.BaseDatosGlosa);
                                }

                                // Agregar resúmenes IGI
                                foreach (System.Data.DataRow r in conciliacion.ResumenIGI.Rows)
                                {
                                    int año = Convert.ToInt32(r["Año"]);
                                    int mes = Convert.ToInt32(r["Mes"]);
                                    string forma = r["FormaPago_IGI"]?.ToString() ?? string.Empty;
                                    decimal igiPag = r["IGI_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IGI_Pagado"]);
                                    decimal igiCalc = r["IGI_Calculado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IGI_Calculado"]);
                                    decimal dif = r["Diferencia_IGI"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Diferencia_IGI"]);

                                    var key = (año, mes, forma);
                                    if (aggIGI.TryGetValue(key, out var cur))
                                    {
                                        aggIGI[key] = (cur.IGI_Pagado + igiPag, cur.IGI_Calculado + igiCalc, cur.Diferencia + dif);
                                    }
                                    else
                                    {
                                        aggIGI[key] = (igiPag, igiCalc, dif);
                                    }
                                }

                                // Agregar resúmenes IVA
                                foreach (System.Data.DataRow r in conciliacion.ResumenIVA.Rows)
                                {
                                    int año = Convert.ToInt32(r["Año"]);
                                    int mes = Convert.ToInt32(r["Mes"]);
                                    string forma = r["FormaPago_IVA"]?.ToString() ?? string.Empty;
                                    decimal ivaPag = r["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IVA_Pagado"]);

                                    var key = (año, mes, forma);
                                    if (aggIVA.TryGetValue(key, out var cur))
                                    {
                                        aggIVA[key] = cur + ivaPag;
                                    }
                                    else
                                    {
                                        aggIVA[key] = ivaPag;
                                    }
                                }

                                // Agregar detalles (merge de tablas)
                                if (primeraIteracion)
                                {
                                    todosDetallesIGI = conciliacion.DetalleIGI.Copy();
                                    todosDetallesIVA = conciliacion.DetalleIVA.Copy();
                                    primeraIteracion = false;
                                }
                                else
                                {
                                    todosDetallesIGI.Merge(conciliacion.DetalleIGI);
                                    todosDetallesIVA.Merge(conciliacion.DetalleIVA);
                                }
                            }
                            catch
                            {
                                // Ignorar bases con error localmente y continuar
                            }
                        }

                        // Construir DataTables resultantes de resumen
                        var resIGI = new System.Data.DataTable();
                        resIGI.Columns.Add("Año", typeof(int));
                        resIGI.Columns.Add("Mes", typeof(int));
                        resIGI.Columns.Add("IGI_Pagado", typeof(decimal));
                        resIGI.Columns.Add("IGI_Calculado", typeof(decimal));
                        resIGI.Columns.Add("Diferencia_IGI", typeof(decimal));
                        resIGI.Columns.Add("FormaPago_IGI", typeof(string));

                        var resIVA = new System.Data.DataTable();
                        resIVA.Columns.Add("Año", typeof(int));
                        resIVA.Columns.Add("Mes", typeof(int));
                        resIVA.Columns.Add("IVA_Pagado", typeof(decimal));
                        resIVA.Columns.Add("FormaPago_IVA", typeof(string));

                        foreach (var kv in aggIGI.OrderBy(k => k.Key.Año).ThenBy(k => k.Key.Mes).ThenBy(k => k.Key.Forma))
                        {
                            resIGI.Rows.Add(kv.Key.Año, kv.Key.Mes, kv.Value.IGI_Pagado, kv.Value.IGI_Calculado, kv.Value.Diferencia, kv.Key.Forma);
                        }

                        foreach (var kv in aggIVA.OrderBy(k => k.Key.Año).ThenBy(k => k.Key.Mes).ThenBy(k => k.Key.Forma))
                        {
                            resIVA.Rows.Add(kv.Key.Año, kv.Key.Mes, kv.Value, kv.Key.Forma);
                        }

                        return (resIGI, resIVA, todosDetallesIGI, todosDetallesIVA);
                    });

                    tablaIGI = resultado.Item1;
                    tablaIVA = resultado.Item2;
                    detalleIGIActual = resultado.Item3;
                    detalleIVAActual = resultado.Item4;

                    ReconstruirReporteActualDesdeDetalles();
                    CompletarReporteActualDesdeResumenes(tablaIGI, tablaIVA);
                }
                else
                {
                    // Consultar una base de datos específica CON validación
                    string baseDatos = cmbCliente.SelectedValue?.ToString() ?? string.Empty;
                    lblProgreso.Text = $"Consultando {baseDatos} para generar reporte...";
                    lblResumenInfo.Text = "Generando reporte...";

                    var resultado = await Task.Run(() => reporteService.ObtenerConciliacionIGI(baseDatos, fechaInicio, fechaFin));

                    if (resultado.FaltaGlosaIVA)
                    {
                        faltaGlosaIva = true;
                        basesSinGlosaIva.Add(string.IsNullOrWhiteSpace(resultado.BaseDatosGlosa) ? baseDatos : resultado.BaseDatosGlosa);
                    }

                    // Guardar las tablas de detalle para uso posterior (doble clic)
                    detalleIGIActual = resultado.DetalleIGI;
                    detalleIVAActual = resultado.DetalleIVA;

                    ReconstruirReporteActualDesdeDetalles(baseDatos);

                    // Mostrar solo los RESÚMENES en los grids
                    tablaIGI = resultado.ResumenIGI;
                    tablaIVA = resultado.ResumenIVA;
                    CompletarReporteActualDesdeResumenes(tablaIGI, tablaIVA, baseDatos);
                }

                PrepararColumnaPeriodo(tablaIGI);
                PrepararColumnaPeriodo(tablaIVA);

                // Mostrar resultados en los grids
                dgvReporteIGI.DataSource = tablaIGI;
                dgvReporteIVA.DataSource = tablaIVA;

                // Formatear columnas
                FormatearGridIGI();
                FormatearGridIVA();

                // Generar resumen (a partir de la tabla IGI)
                var resumen = new ResumenIGI();
                if (tablaIGI != null && tablaIGI.Rows.Count > 0)
                {
                    resumen.TotalIGI_Pagado = tablaIGI.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToDecimal(r["IGI_Pagado"]));
                    resumen.TotalIGI_Calculado = tablaIGI.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToDecimal(r["IGI_Calculado"]));
                    resumen.TotalIVA_Pagado = tablaIVA != null && tablaIVA.Rows.Count > 0 ? tablaIVA.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToDecimal(r["IVA_Pagado"])) : 0m;
                    resumen.TotalPedimentos = tablaIGI.Rows.Count;
                }

                MostrarResumenPorFormaPago(resumen);

                // Intentar guardar el reporte IGI/IVA en portal web (si está configurada la conexión)
                try
                {
                    var razon = cmbRazonSocial.SelectedItem is RazonSocial rz ? rz.NombreRazon : string.Empty;
                    var baseDb = cmbCliente.SelectedValue?.ToString() ?? string.Empty;
                    _ = await Retorno360Tacna.SERVICES.PortalWebService.GuardarReporteIGIAsync(reporteActual, resumen, razon, baseDb, fechaInicio, fechaFin);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FrmReportes] Error guardando reporte IGI en portal web: {ex}");
                }

                lblProgreso.Text = "Consulta completada";
                btnGenerarPDF.Enabled = true;
                btnExportarExcel.Enabled = true;

                if (faltaGlosaIva)
                {
                    string basesAviso = string.Join(", ", basesSinGlosaIva.OrderBy(x => x));
                    MessageBox.Show(
                        $"No está cargada la glosa para obtener el IVA pagado{(string.IsNullOrWhiteSpace(basesAviso) ? string.Empty : $" en: {basesAviso}") }.",
                        "Aviso de glosa IVA",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError(
                    $"Error al generar el reporte:\n{ex.Message}",
                    "Error",
                    ex,
                    "Generación de reporte IGI/IVA"
                );
                lblProgreso.Text = "Error al generar reporte";
                lblResumenInfo.Text = "Error en la consulta";
            }
            finally
            {
                bool estadoExportarExcel = btnExportarExcel.Enabled;
                bool estadoGenerarPdf = btnGenerarPDF.Enabled;

                // Ocultar indicador de carga
                MostrarPanelCargando(false);
                EstablecerEstadoBotonesDuranteCarga(false);

                btnExportarExcel.Enabled = estadoExportarExcel;
                btnGenerarPDF.Enabled = estadoGenerarPdf;

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

        private void ReconstruirReporteActualDesdeDetalles(string baseDatosPredeterminada = "")
        {
            reporteActual.Clear();

            var mapa = new Dictionary<string, ReporteIGIPagado>(StringComparer.OrdinalIgnoreCase);

            if (detalleIGIActual != null)
            {
                foreach (System.Data.DataRow row in detalleIGIActual.Rows)
                {
                    string pedimento = row["Pedimento"]?.ToString()?.Trim() ?? string.Empty;
                    DateTime? fechaPago = row["FechaPago"] == DBNull.Value ? null : Convert.ToDateTime(row["FechaPago"]);
                    string formaPagoIGI = row["FormaPago_IGI"]?.ToString()?.Trim() ?? string.Empty;
                    string llave = $"{pedimento}|{fechaPago:yyyyMMdd}|{formaPagoIGI}";

                    var item = new ReporteIGIPagado
                    {
                        BaseDatos = baseDatosPredeterminada,
                        Clave = row.Table.Columns.Contains("Clave") ? row["Clave"]?.ToString()?.Trim() ?? string.Empty : string.Empty,
                        Pedimento = pedimento,
                        FechaPago = fechaPago,
                        FormaPago_IGI = formaPagoIGI,
                        IGI_Pagado = row["IGI_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Pagado"]),
                        IGI_Calculado = row["IGI_Calculado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Calculado"]),
                        EstatusGlosa = row.Table.Columns.Contains("Estatus") ? row["Estatus"]?.ToString() ?? string.Empty : string.Empty
                    };



                    mapa[llave] = item;
                }
            }

            if (detalleIVAActual != null)
            {
                foreach (System.Data.DataRow row in detalleIVAActual.Rows)
                {
                    string pedimento = row["Pedimento"]?.ToString()?.Trim() ?? string.Empty;
                    DateTime? fechaPago = row["FechaPago"] == DBNull.Value ? null : Convert.ToDateTime(row["FechaPago"]);
                    string formaPagoIVA = row["FormaPago_IVA"]?.ToString()?.Trim() ?? string.Empty;

                    var coincidencia = mapa.Values.FirstOrDefault(x =>
                        string.Equals(x.Pedimento, pedimento, StringComparison.OrdinalIgnoreCase)
                        && x.FechaPago == fechaPago
                        && string.IsNullOrWhiteSpace(x.FormaPago_IVA));

                    if (coincidencia != null)
                    {
                        coincidencia.FormaPago_IVA = formaPagoIVA;
                        coincidencia.IVA_Pagado = row["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IVA_Pagado"]);
                    }
                    else
                    {
                        var item = new ReporteIGIPagado
                        {
                            BaseDatos = baseDatosPredeterminada,
                            Pedimento = pedimento,
                            FechaPago = fechaPago,
                            FormaPago_IVA = formaPagoIVA,
                            IVA_Pagado = row["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IVA_Pagado"])
                        };

                        mapa[$"IVA|{pedimento}|{fechaPago:yyyyMMdd}|{formaPagoIVA}"] = item;
                    }
                }
            }

            reporteActual = mapa.Values
                .OrderBy(x => x.FechaPago)
                .ThenBy(x => x.Pedimento)
                .ToList();
        }

        private void CompletarReporteActualDesdeResumenes(System.Data.DataTable? tablaIGI, System.Data.DataTable? tablaIVA, string baseDatosPredeterminada = "")
        {
            if (tablaIGI != null)
            {
                foreach (System.Data.DataRow row in tablaIGI.Rows)
                {
                    int anio = ObtenerValorEntero(row, "Año", "A�o");
                    int mes = ObtenerValorEntero(row, "Mes");
                    string formaPagoIGI = row["FormaPago_IGI"]?.ToString()?.Trim() ?? string.Empty;

                    if (anio <= 0 || mes <= 0 || string.IsNullOrWhiteSpace(formaPagoIGI))
                        continue;

                    bool existe = reporteActual.Any(x =>
                        x.FechaPago.HasValue
                        && x.FechaPago.Value.Year == anio
                        && x.FechaPago.Value.Month == mes
                        && string.Equals((x.FormaPago_IGI ?? string.Empty).Trim(), formaPagoIGI, StringComparison.OrdinalIgnoreCase));

                    if (!existe)
                    {
                        reporteActual.Add(new ReporteIGIPagado
                        {
                            BaseDatos = baseDatosPredeterminada,
                            FechaPago = new DateTime(anio, mes, 1),
                            FormaPago_IGI = formaPagoIGI,
                            IGI_Pagado = row["IGI_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Pagado"]),
                            IGI_Calculado = row["IGI_Calculado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Calculado"])
                        });
                    }
                }
            }

            if (tablaIVA != null)
            {
                foreach (System.Data.DataRow row in tablaIVA.Rows)
                {
                    int anio = ObtenerValorEntero(row, "Año", "A�o");
                    int mes = ObtenerValorEntero(row, "Mes");
                    string formaPagoIVA = row["FormaPago_IVA"]?.ToString()?.Trim() ?? string.Empty;

                    if (anio <= 0 || mes <= 0 || string.IsNullOrWhiteSpace(formaPagoIVA))
                        continue;

                    bool existe = reporteActual.Any(x =>
                        x.FechaPago.HasValue
                        && x.FechaPago.Value.Year == anio
                        && x.FechaPago.Value.Month == mes
                        && string.Equals((x.FormaPago_IVA ?? string.Empty).Trim(), formaPagoIVA, StringComparison.OrdinalIgnoreCase));

                    if (!existe)
                    {
                        reporteActual.Add(new ReporteIGIPagado
                        {
                            BaseDatos = baseDatosPredeterminada,
                            FechaPago = new DateTime(anio, mes, 1),
                            FormaPago_IVA = formaPagoIVA,
                            IVA_Pagado = row["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IVA_Pagado"])
                        });
                    }
                }
            }

            reporteActual = reporteActual
                .OrderBy(x => x.FechaPago)
                .ThenBy(x => x.Pedimento)
                .ToList();
        }

        private static int ObtenerValorEntero(System.Data.DataRow row, params string[] nombresColumna)
        {
            foreach (var nombre in nombresColumna)
            {
                if (row.Table.Columns.Contains(nombre) && row[nombre] != DBNull.Value)
                {
                    return Convert.ToInt32(row[nombre]);
                }
            }

            return 0;
        }

        private static string ObtenerNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => $"Mes {mes}"
            };
        }

        private static void PrepararColumnaPeriodo(System.Data.DataTable? tabla)
        {
            if (tabla == null)
                return;

            if (!tabla.Columns.Contains("Periodo"))
                tabla.Columns.Add("Periodo", typeof(string));

            foreach (System.Data.DataRow row in tabla.Rows)
            {
                int anio = ObtenerValorEntero(row, "Año", "A�o");
                int mes = ObtenerValorEntero(row, "Mes");
                row["Periodo"] = anio > 0 && mes > 0
                    ? $"{ObtenerNombreMes(mes)} {anio}"
                    : string.Empty;
            }
        }

        private static string ObtenerTextoMesParaPdf(System.Data.DataRow row)
        {
            int anio = ObtenerValorEntero(row, "Año", "A�o");
            int mes = ObtenerValorEntero(row, "Mes");

            if (anio > 0 && mes > 0)
                return $"{ObtenerNombreMes(mes)} {anio}";

            if (row.Table.Columns.Contains("MES"))
                return row["MES"]?.ToString() ?? string.Empty;

            return string.Empty;
        }

        private System.Data.DataTable CrearTablaPdfIGIDesdeGrid()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("MES", typeof(string));
            dt.Columns.Add("IGI PAGADO", typeof(decimal));
            dt.Columns.Add("IGI CALCULADO", typeof(decimal));
            dt.Columns.Add("DIFERENCIA", typeof(decimal));
            dt.Columns.Add("FORMA DE PAGO IGI", typeof(string));

            if (dgvReporteIGI.DataSource is not System.Data.DataTable origen)
                return dt;

            foreach (System.Data.DataRow row in origen.Rows)
            {
                decimal igiPagado = row.Table.Columns.Contains("IGI_Pagado") && row["IGI_Pagado"] != DBNull.Value
                    ? Convert.ToDecimal(row["IGI_Pagado"])
                    : row.Table.Columns.Contains("IGI PAGADO") && row["IGI PAGADO"] != DBNull.Value
                        ? Convert.ToDecimal(row["IGI PAGADO"])
                        : 0m;

                decimal igiCalculado = row.Table.Columns.Contains("IGI_Calculado") && row["IGI_Calculado"] != DBNull.Value
                    ? Convert.ToDecimal(row["IGI_Calculado"])
                    : row.Table.Columns.Contains("IGI CALCULADO") && row["IGI CALCULADO"] != DBNull.Value
                        ? Convert.ToDecimal(row["IGI CALCULADO"])
                        : 0m;

                decimal diferencia = row.Table.Columns.Contains("Diferencia_IGI") && row["Diferencia_IGI"] != DBNull.Value
                    ? Convert.ToDecimal(row["Diferencia_IGI"])
                    : row.Table.Columns.Contains("DIFERENCIA") && row["DIFERENCIA"] != DBNull.Value
                        ? Convert.ToDecimal(row["DIFERENCIA"])
                        : 0m;

                string formaPago = row.Table.Columns.Contains("FormaPago_IGI")
                    ? row["FormaPago_IGI"]?.ToString() ?? string.Empty
                    : row.Table.Columns.Contains("FORMA DE PAGO IGI")
                        ? row["FORMA DE PAGO IGI"]?.ToString() ?? string.Empty
                        : string.Empty;

                dt.Rows.Add(
                    ObtenerTextoMesParaPdf(row),
                    igiPagado,
                    igiCalculado,
                    diferencia,
                    formaPago);
            }

            return dt;
        }

        private System.Data.DataTable CrearTablaPdfIVADesdeGrid()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("MES", typeof(string));
            dt.Columns.Add("IVA PAGADO", typeof(decimal));
            dt.Columns.Add("FORMA DE PAGO IVA", typeof(string));

            if (dgvReporteIVA.DataSource is not System.Data.DataTable origen)
                return dt;

            foreach (System.Data.DataRow row in origen.Rows)
            {
                decimal ivaPagado = row.Table.Columns.Contains("IVA_Pagado") && row["IVA_Pagado"] != DBNull.Value
                    ? Convert.ToDecimal(row["IVA_Pagado"])
                    : row.Table.Columns.Contains("IVA PAGADO") && row["IVA PAGADO"] != DBNull.Value
                        ? Convert.ToDecimal(row["IVA PAGADO"])
                        : 0m;

                string formaPago = row.Table.Columns.Contains("FormaPago_IVA")
                    ? row["FormaPago_IVA"]?.ToString() ?? string.Empty
                    : row.Table.Columns.Contains("FORMA DE PAGO IVA")
                        ? row["FORMA DE PAGO IVA"]?.ToString() ?? string.Empty
                        : string.Empty;

                dt.Rows.Add(
                    ObtenerTextoMesParaPdf(row),
                    ivaPagado,
                    formaPago);
            }

            return dt;
        }

        private System.Data.DataTable CrearTablaDetalleCompletoParaPdf()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Base Datos", typeof(string));
            dt.Columns.Add("ID Pedimento", typeof(int));
            dt.Columns.Add("Pedimento", typeof(string));
            dt.Columns.Add("Fecha Pago", typeof(DateTime));
            dt.Columns.Add("IGI Pagado", typeof(decimal));
            dt.Columns.Add("IGI Calculado", typeof(decimal));
            dt.Columns.Add("Diferencia IGI", typeof(decimal));
            dt.Columns.Add("IVA Pagado", typeof(decimal));
            dt.Columns.Add("Forma Pago IGI", typeof(string));
            dt.Columns.Add("Forma Pago IVA", typeof(string));
            dt.Columns.Add("Estatus Glosa", typeof(string));
            dt.Columns.Add("Estatus Origen", typeof(string));

            foreach (var r in reporteActual.OrderBy(x => x.FechaPago).ThenBy(x => x.Pedimento))
            {
                dt.Rows.Add(
                    r.BaseDatos ?? string.Empty,
                    r.IdPedimento,
                    r.Pedimento ?? string.Empty,
                    r.FechaPago ?? DateTime.MinValue,
                    r.IGI_Pagado,
                    r.IGI_Calculado,
                    r.DiferenciaIGI,
                    r.IVA_Pagado,
                    r.FormaPago_IGI ?? string.Empty,
                    r.FormaPago_IVA ?? string.Empty,
                    r.EstatusGlosa ?? string.Empty,
                    r.EstatusOrigen ?? string.Empty);
            }

            return dt;
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

        private void chkUsarPerfil_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUsarPerfil.Checked && usuarioActual == null)
            {
                MessageBox.Show("No hay usuario activo para usar el perfil de empresas.",
                    "Perfil no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkUsarPerfil.Checked = false;
                return;
            }

            // Recargar razones sociales según el modo activo
            CargarRazonesSociales();
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
            DataGridViewManualCopyHelper.Configurar(dgvReporteIGI);

            // Configurar grid IVA
            dgvReporteIVA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvReporteIVA.AllowUserToAddRows = false;
            dgvReporteIVA.AllowUserToDeleteRows = false;
            dgvReporteIVA.ReadOnly = true;
            dgvReporteIVA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporteIVA.MultiSelect = false;
            dgvReporteIVA.RowHeadersVisible = false;
            dgvReporteIVA.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);
            DataGridViewManualCopyHelper.Configurar(dgvReporteIVA);

            // Abrir detalle solo con doble clic
            dgvReporteIGI.CellDoubleClick += DgvReporteIGI_CellDoubleClick;
            dgvReporteIVA.CellDoubleClick += DgvReporteIVA_CellDoubleClick;
        }

        private static string ObtenerValorCelda(DataGridViewRow row, params string[] nombresColumna)
        {
            foreach (var nombreColumna in nombresColumna)
            {
                if (row.DataGridView?.Columns.Contains(nombreColumna) == true)
                {
                    return row.Cells[nombreColumna]?.Value?.ToString()?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static DataGridViewColumn? ObtenerColumna(DataGridView grid, params string[] nombresColumna)
        {
            foreach (var nombreColumna in nombresColumna)
            {
                if (grid.Columns.Contains(nombreColumna))
                    return grid.Columns[nombreColumna];
            }

            return null;
        }

        private void FormatearGridIGI()
        {
            if (dgvReporteIGI.Columns.Count == 0)
                return;

            // Renombrar columnas del nuevo formato de conciliación
            if (dgvReporteIGI.Columns["Año"] != null)
                dgvReporteIGI.Columns["Año"].Visible = false;

            if (dgvReporteIGI.Columns["Mes"] != null)
                dgvReporteIGI.Columns["Mes"].Visible = false;

            if (dgvReporteIGI.Columns["Periodo"] != null)
                dgvReporteIGI.Columns["Periodo"].HeaderText = "MES";

            var columnaIGIPagado = ObtenerColumna(dgvReporteIGI, "IGI_Pagado", "IGI PAGADO");
            var columnaIGICalculado = ObtenerColumna(dgvReporteIGI, "IGI_Calculado", "IGI CALCULADO");
            var columnaDiferenciaIGI = ObtenerColumna(dgvReporteIGI, "Diferencia_IGI", "DIFERENCIA");
            var columnaFormaPagoIGI = ObtenerColumna(dgvReporteIGI, "FormaPago_IGI", "FORMA DE PAGO IGI");

            if (columnaIGIPagado != null)
                columnaIGIPagado.HeaderText = "IGI PAGADO";

            if (columnaIGICalculado != null)
                columnaIGICalculado.HeaderText = "IGI CALCULADO";

            if (columnaDiferenciaIGI != null)
                columnaDiferenciaIGI.HeaderText = "DIFERENCIA";

            if (columnaFormaPagoIGI != null)
                columnaFormaPagoIGI.HeaderText = "FORMA DE PAGO IGI";

            // Título del header
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.Font = new Font(dgvReporteIGI.Font.FontFamily, 10, FontStyle.Bold);
            dgvReporteIGI.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvReporteIGI.EnableHeadersVisualStyles = false;

            // Formatear columnas de moneda
            if (columnaIGIPagado != null)
            {
                columnaIGIPagado.DefaultCellStyle.Format = "C2";
                columnaIGIPagado.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (columnaIGICalculado != null)
            {
                columnaIGICalculado.DefaultCellStyle.Format = "C2";
                columnaIGICalculado.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (columnaDiferenciaIGI != null)
            {
                columnaDiferenciaIGI.DefaultCellStyle.Format = "C2";
                columnaDiferenciaIGI.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                columnaDiferenciaIGI.DefaultCellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                columnaDiferenciaIGI.DefaultCellStyle.Font = new Font(dgvReporteIGI.Font.FontFamily, 9, FontStyle.Bold);
            }

            // Configurar ancho de columnas
            if (dgvReporteIGI.Columns["Periodo"] != null)
            {
                dgvReporteIGI.Columns["Periodo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvReporteIGI.Columns["Periodo"].MinimumWidth = 140;
                dgvReporteIGI.Columns["Periodo"].DisplayIndex = 0;
            }

            if (columnaFormaPagoIGI != null)
            {
                columnaFormaPagoIGI.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                columnaFormaPagoIGI.MinimumWidth = 100;
            }
        }

        private void FormatearGridIVA()
        {
            if (dgvReporteIVA.Columns.Count == 0)
                return;

            // Renombrar columnas del nuevo formato de conciliación
            if (dgvReporteIVA.Columns["Año"] != null)
                dgvReporteIVA.Columns["Año"].Visible = false;

            if (dgvReporteIVA.Columns["Mes"] != null)
                dgvReporteIVA.Columns["Mes"].Visible = false;

            if (dgvReporteIVA.Columns["Periodo"] != null)
                dgvReporteIVA.Columns["Periodo"].HeaderText = "MES";

            var columnaIVAPagado = ObtenerColumna(dgvReporteIVA, "IVA_Pagado", "IVA PAGADO");
            var columnaFormaPagoIVA = ObtenerColumna(dgvReporteIVA, "FormaPago_IVA", "FORMA DE PAGO IVA");

            if (columnaIVAPagado != null)
                columnaIVAPagado.HeaderText = "IVA PAGADO";

            if (columnaFormaPagoIVA != null)
                columnaFormaPagoIVA.HeaderText = "FORMA DE PAGO IVA";

            // Título del header
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.Font = new Font(dgvReporteIVA.Font.FontFamily, 10, FontStyle.Bold);
            dgvReporteIVA.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvReporteIVA.EnableHeadersVisualStyles = false;

            // Formatear columnas de moneda
            if (columnaIVAPagado != null)
            {
                columnaIVAPagado.DefaultCellStyle.Format = "C2";
                columnaIVAPagado.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Configurar ancho de columnas
            if (dgvReporteIVA.Columns["Periodo"] != null)
            {
                dgvReporteIVA.Columns["Periodo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvReporteIVA.Columns["Periodo"].MinimumWidth = 140;
                dgvReporteIVA.Columns["Periodo"].DisplayIndex = 0;
            }

            if (columnaFormaPagoIVA != null)
            {
                columnaFormaPagoIVA.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                columnaFormaPagoIVA.MinimumWidth = 100;
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

                string formaPago = ObtenerValorCelda(row, "FormaPago_IGI", "FORMA DE PAGO IGI");
                string periodo = ObtenerValorCelda(row, "Periodo", "MES");
                int anio = 0;
                int mes = 0;

                if (row.DataBoundItem is System.Data.DataRowView rowView)
                {
                    anio = ObtenerValorEntero(rowView.Row, "Año", "A�o");
                    mes = ObtenerValorEntero(rowView.Row, "Mes");
                }

                if (string.IsNullOrEmpty(formaPago))
                {
                    MessageBox.Show("No se pudo obtener la información de forma de pago.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validar que existan datos de detalle
                if (detalleIGIActual == null || detalleIGIActual.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos de detalle disponibles.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Abrir formulario de detalle con la tabla filtrada por forma de pago
                var frmDetalle = new FrmDetalleConciliacion(detalleIGIActual, formaPago, periodo: periodo, anioSeleccionado: anio, mesSeleccionado: mes, tipoReporte: "IGI");
                frmDetalle.ShowDialog(this);
                frmDetalle.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al mostrar el detalle: {ex.Message}",
                    "Error", ex, "Apertura de detalle IGI");
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

                string formaPago = ObtenerValorCelda(row, "FormaPago_IVA", "FORMA DE PAGO IVA");
                string periodo = ObtenerValorCelda(row, "Periodo", "MES");
                int anio = 0;
                int mes = 0;

                if (row.DataBoundItem is System.Data.DataRowView rowView)
                {
                    anio = ObtenerValorEntero(rowView.Row, "Año", "A�o");
                    mes = ObtenerValorEntero(rowView.Row, "Mes");
                }

                if (string.IsNullOrEmpty(formaPago))
                {
                    MessageBox.Show("No se pudo obtener la información de forma de pago.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validar que existan datos de detalle
                if (detalleIVAActual == null || detalleIVAActual.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos de detalle disponibles.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Abrir formulario de detalle con la tabla filtrada por forma de pago
                var frmDetalle = new FrmDetalleConciliacion(detalleIVAActual, formaPago, periodo: periodo, anioSeleccionado: anio, mesSeleccionado: mes, tipoReporte: "IVA");
                frmDetalle.ShowDialog(this);
                frmDetalle.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al mostrar el detalle: {ex.Message}",
                    "Error", ex, "Apertura de detalle IVA");
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
            decimal totalIGI_Pagado5 = 0m;
            decimal totalIGI_Calculado5 = 0m;
            decimal diferenciaIGI_5 = 0m;
            decimal totalIGI_Pagado0 = 0m;
            decimal totalIGI_Calculado0 = 0m;
            decimal diferenciaIGI_0 = 0m;
            decimal totalIVA_Pagado21 = 0m;
            decimal totalIVA_Pagado0 = 0m;

            if (dgvReporteIGI.DataSource is System.Data.DataTable tablaIGI)
            {
                foreach (System.Data.DataRow row in tablaIGI.Rows)
                {
                    string formaPago = row["FormaPago_IGI"]?.ToString()?.Trim() ?? string.Empty;
                    decimal igiPagado = row["IGI_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Pagado"]);
                    decimal igiCalculado = row["IGI_Calculado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IGI_Calculado"]);
                    decimal diferencia = row["Diferencia_IGI"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Diferencia_IGI"]);

                    if (formaPago == "5")
                    {
                        totalIGI_Pagado5 += igiPagado;
                        totalIGI_Calculado5 += igiCalculado;
                        diferenciaIGI_5 += diferencia;
                    }
                    else if (formaPago == "0")
                    {
                        totalIGI_Pagado0 += igiPagado;
                        totalIGI_Calculado0 += igiCalculado;
                        diferenciaIGI_0 += diferencia;
                    }
                }
            }

            if (dgvReporteIVA.DataSource is System.Data.DataTable tablaIVA)
            {
                foreach (System.Data.DataRow row in tablaIVA.Rows)
                {
                    string formaPago = row["FormaPago_IVA"]?.ToString()?.Trim() ?? string.Empty;
                    decimal ivaPagado = row["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(row["IVA_Pagado"]);

                    if (formaPago == "21")
                    {
                        totalIVA_Pagado21 += ivaPagado;
                    }
                    else if (formaPago == "0")
                    {
                        totalIVA_Pagado0 += ivaPagado;
                    }
                }
            }

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

            MostrarGraficaActual();

            if (graficaActual == 0)
                chartIGI?.Update();
            else
                chartIVA?.Update();
        }

        private void InicializarGrafica()
        {
            // Crear control de gráfica
            chartIGI = new CartesianChart
            {
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Visible = true
            };

            // Agregar al panel de gráfica (después del título)
            panelGrafica.Controls.Add(chartIGI);
            // Asegurar que la gráfica quede por delante y ocupe todo el panel
            panelGrafica.Controls.SetChildIndex(chartIGI, 0);
            chartIGI.BringToFront();
            AjustarAreaGrafica(chartIGI, panelGrafica, lblTituloGrafica);

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
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Visible = false
            };

            // Agregar al mismo panel de gráfica principal
            panelGrafica.Controls.Add(chartIVA);
            // Asegurar orden y visibilidad correcta
            panelGrafica.Controls.SetChildIndex(chartIVA, 0);
            chartIVA.BringToFront();
            AjustarAreaGrafica(chartIVA, panelGrafica, lblTituloGrafica);
            chartIVA.Visible = false;

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
                var _dtIGI = dgvReporteIGI.DataSource as System.Data.DataTable;
                System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaPorFormaPago] dgvReporteIGI.DataSource={((dgvReporteIGI.DataSource==null)?"null":"set")} rows={(_dtIGI?.Rows.Count ?? 0)}");
                chartIGI.Series = Array.Empty<ISeries>();

                if (dgvReporteIGI.DataSource is not System.Data.DataTable tablaIGI || tablaIGI.Rows.Count == 0)
                {
                    lblTituloGrafica.Text = "Sin datos para mostrar";
                    return;
                }

                var datosPorMes = tablaIGI.Rows.Cast<System.Data.DataRow>()
                    .Select(r => new
                    {
                        Anio = ObtenerValorEntero(r, "Año", "A�o"),
                        Mes = ObtenerValorEntero(r, "Mes"),
                        FormaPago = r["FormaPago_IGI"]?.ToString()?.Trim() ?? string.Empty,
                        Pagado = r["IGI_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IGI_Pagado"]),
                        Calculado = r["IGI_Calculado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IGI_Calculado"]),
                        Diferencia = r["Diferencia_IGI"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Diferencia_IGI"])
                    })
                    .Where(x => x.Anio > 0 && x.Mes > 0 && (x.FormaPago == "0" || x.FormaPago == "5"))
                    .OrderBy(x => x.Anio)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.FormaPago)
                    .ToList();

                if (datosPorMes.Count == 0)
                {
                    chartIGI.Series = Array.Empty<ISeries>();
                    // Si hay filas en el grid pero no hay datos agrupables por forma de pago,
                    // mostrar mensaje más explícito para facilitar diagnóstico y abrir modal diagnóstico
                    var dtTmp = dgvReporteIGI.DataSource as System.Data.DataTable;
                    if (dtTmp != null && dtTmp.Rows.Count > 0)
                    {
                        lblTituloGrafica.Text = "Sin datos agrupables por forma de pago (revisar columna FormaPago_IGI)";
                        MostrarDiagnosticoTablas("IGI", dtTmp);
                    }
                    else if (graficaActual == 0)
                        lblTituloGrafica.Text = "Sin datos para mostrar";
                    return;
                }

                var labels = new List<string>();
                var pagadoValues = new List<double>();
                var diferenciaValues = new List<double>();
                var pagadoPorcentajes = new List<double>();
                var diferenciaPorcentajes = new List<double>();

                foreach (var dato in datosPorMes)
                {
                    decimal pagado = dato.Pagado;
                    decimal calculado = dato.Calculado;
                    decimal diferencia = dato.Diferencia;

                    pagadoValues.Add((double)pagado);
                    diferenciaValues.Add((double)diferencia);

                    double porcPagado = 0d;
                    double porcDiferencia = 0d;

                    if (calculado > 0)
                    {
                        porcPagado = (double)((pagado / calculado) * 100m);
                        porcDiferencia = (double)((diferencia / calculado) * 100m);
                    }

                    pagadoPorcentajes.Add(porcPagado);
                    diferenciaPorcentajes.Add(porcDiferencia);
                    labels.Add($"{ObtenerNombreMes(dato.Mes)} FP-{dato.FormaPago}");
                }

                System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaPorFormaPago] labels={labels.Count} pagadoValues={pagadoValues.Count} diferenciaValues={diferenciaValues.Count}");

                var seriePagado = new StackedColumnSeries<double>
                {
                    Name = "IGI pagado",
                    Values = pagadoValues.ToArray(),
                    Fill = new SolidColorPaint(new SKColor(79, 129, 189)), // Azul
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    MaxBarWidth = 40,
                    DataLabelsFormatter = point => point.Model != null ? $"{point.Model:N0}" : string.Empty
                };

                var serieDiferencia = new StackedColumnSeries<double>
                {
                    Name = "Diferencia",
                    Values = diferenciaValues.ToArray(),
                    Fill = new SolidColorPaint(new SKColor(155, 194, 230)), // Azul claro
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    MaxBarWidth = 40,
                    DataLabelsFormatter = point => point.Model != null ? $"{point.Model:N0}" : string.Empty
                };

                // Reemplazar el chartIGI existente por uno nuevo para evitar problemas de estado interno del control
                var nuevoChart = new CartesianChart
                {
                    Dock = DockStyle.Fill,
                    ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                    TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top,
                    LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                    BackColor = Color.White,
                    // Mostrar solo si la gráfica IGI es la actual
                    Visible = (graficaActual == 0)
                };

                // Asignar series al nuevo control
                nuevoChart.Series = new ISeries[] { seriePagado, serieDiferencia };

                // Configurar ejes en el nuevo chart
                nuevoChart.XAxes = new[]
                {
                    new Axis
                    {
                        Labels = labels.ToArray(),
                        TextSize = 9,
                        LabelsRotation = 0,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230))
                    }
                };

                nuevoChart.YAxes = new[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        TextSize = 9,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                        Labeler = value => value >= 1000000 ? $"{value/1000000:N1}M"
                                         : value >= 1000 ? $"{value/1000:N1}K"
                                         : value.ToString("N0")
                    }
                };

                // Actualizar título en la UI
                lblTituloGrafica.Text = "IGI pagado + diferencia por mes y forma de pago (1/2)";

                // Reemplazar control existente en el panel (si existe) en el hilo de UI
                try
                {
                    this.Invoke(() =>
                    {
                        try
                        {
                            if (chartIGI != null)
                            {
                                if (panelGrafica.Controls.Contains(chartIGI))
                                {
                                    panelGrafica.Controls.Remove(chartIGI);
                                }
                                try { chartIGI.Dispose(); } catch { }
                            }

                            panelGrafica.Controls.Add(nuevoChart);
                            panelGrafica.Controls.SetChildIndex(nuevoChart, 0);
                            chartIGI = nuevoChart;

                            AjustarAreaGrafica(chartIGI, panelGrafica, lblTituloGrafica);
                            // Asegurar visibilidad correcta según la gráfica actual
                            chartIGI.Visible = (graficaActual == 0);
                            MostrarGraficaActual();
                            try { chartIGI.Update(); chartIGI.Refresh(); panelGrafica.Refresh(); } catch { }
                        }
                        catch { }
                    });
                }
                catch
                {
                    // Si Invoke falla, intentar realizar la operación directamente como fallback
                    try
                    {
                        if (chartIGI != null)
                        {
                            if (panelGrafica.Controls.Contains(chartIGI))
                            {
                                panelGrafica.Controls.Remove(chartIGI);
                            }
                            try { chartIGI.Dispose(); } catch { }
                        }

                            panelGrafica.Controls.Add(nuevoChart);
                            panelGrafica.Controls.SetChildIndex(nuevoChart, 0);
                            chartIGI = nuevoChart;

                            AjustarAreaGrafica(chartIGI, panelGrafica, lblTituloGrafica);
                            // Asegurar visibilidad correcta según la gráfica actual
                            chartIGI.Visible = (graficaActual == 0);
                            MostrarGraficaActual();
                            try { chartIGI.Update(); chartIGI.Refresh(); panelGrafica.Refresh(); } catch { }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar gráfica: {ex.Message}");
                if (graficaActual == 0)
                    lblTituloGrafica.Text = "Error al cargar gráfica (1/2)";
            }
        }

        private void ActualizarGraficaIVAPorFormaPago()
        {
            if (chartIVA == null) return;

            try
            {
                var _dtIVA = dgvReporteIVA.DataSource as System.Data.DataTable;
                System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaIVAPorFormaPago] dgvReporteIVA.DataSource={((dgvReporteIVA.DataSource==null)?"null":"set")} rows={(_dtIVA?.Rows.Count ?? 0)}");
                chartIVA.Series = Array.Empty<ISeries>();

                if (dgvReporteIVA.DataSource is not System.Data.DataTable tablaIVA || tablaIVA.Rows.Count == 0)
                {
                    lblTituloGrafica.Text = "Sin datos de IVA para mostrar";
                    return;
                }

                var datosPorMes = tablaIVA.Rows.Cast<System.Data.DataRow>()
                    .Select(r => new
                    {
                        Anio = ObtenerValorEntero(r, "Año", "A�o"),
                        Mes = ObtenerValorEntero(r, "Mes"),
                        FormaPago = r["FormaPago_IVA"]?.ToString()?.Trim() ?? string.Empty,
                        IVAPagado = r["IVA_Pagado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IVA_Pagado"])
                    })
                    .Where(x => x.Anio > 0 && x.Mes > 0 && (x.FormaPago == "0" || x.FormaPago == "21"))
                    .OrderBy(x => x.Anio)
                    .ThenBy(x => x.Mes)
                    .ThenBy(x => x.FormaPago)
                    .ToList();

                if (datosPorMes.Count == 0)
                {
                    chartIVA.Series = Array.Empty<ISeries>();
                    if (graficaActual == 1)
                        lblTituloGrafica.Text = "Sin datos de IVA para mostrar";
                    return;
                }

                var labels = new List<string>();
                var ivaPagadoValues = new List<double>();
                var ivaPorcentajes = new List<double>();

                foreach (var dato in datosPorMes)
                {
                    decimal ivaPagado = dato.IVAPagado;
                    ivaPagadoValues.Add((double)ivaPagado);
                    ivaPorcentajes.Add(100d);
                    labels.Add($"{ObtenerNombreMes(dato.Mes)} FP-{dato.FormaPago}");
                }
                System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaIVAPorFormaPago] labels={labels.Count} ivaValues={ivaPagadoValues.Count}");

                // Si no hay labels pero hay filas en el grid, mostrar modal diagnóstico para inspección
                if (labels.Count == 0)
                {
                    var dtTmp = dgvReporteIVA.DataSource as System.Data.DataTable;
                    if (dtTmp != null && dtTmp.Rows.Count > 0)
                    {
                        MostrarDiagnosticoTablas("IVA", dtTmp);
                    }
                }

                // DEBUG: volcar valores y labels para diagnóstico
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaIVAPorFormaPago-DBG] labelsList=[{string.Join(", ", labels)}]");
                    System.Diagnostics.Debug.WriteLine($"[ActualizarGraficaIVAPorFormaPago-DBG] ivaValues=[{string.Join(", ", ivaPagadoValues)}]");
                }
                catch { }

                // Reutilizar chartIVA existente cuando sea posible para evitar problemas de estado
                try
                {
                    this.Invoke(() =>
                    {
                        try
                        {
                            if (chartIVA != null)
                            {
                                // Asignar series y ejes al control existente
                                chartIVA.Series = new ISeries[]
                                {
                                    new ColumnSeries<double>
                                    {
                                        Name = "IVA pagado",
                                        Values = ivaPagadoValues.ToArray(),
                                        Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                                        Stroke = null,
                                        DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                                        DataLabelsSize = 11,
                                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                                        MaxBarWidth = 40,
                                        DataLabelsFormatter = point => point.Model != null ? $"{point.Model:N0}" : string.Empty
                                    }
                                };

                                chartIVA.XAxes = new[]
                                {
                                    new Axis
                                    {
                                        Labels = labels.ToArray(),
                                        TextSize = 9,
                                        LabelsRotation = 0,
                                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230))
                                    }
                                };

                                chartIVA.YAxes = new[]
                                {
                                    new Axis
                                    {
                                        MinLimit = 0,
                                        TextSize = 9,
                                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                                        Labeler = value => value >= 1000000 ? $"{value/1000000:N1}M"
                                                         : value >= 1000 ? $"{value/1000:N1}K"
                                                         : value.ToString("N0")
                                    }
                                };

                                chartIVA.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
                                {
                                    Text = "IVA pagado por mes y forma de pago (2/2)",
                                    TextSize = 12,
                                    Paint = new SolidColorPaint(SKColors.Black),
                                    Padding = new LiveChartsCore.Drawing.Padding(8)
                                };

                                AjustarAreaGrafica(chartIVA, panelGrafica, lblTituloGrafica);
                                // Asegurar visibilidad correcta según la gráfica actual
                                chartIVA.Visible = (graficaActual == 1);
                                MostrarGraficaActual();
                                try { chartIVA.Update(); } catch { }
                                try { chartIVA.Refresh(); } catch { }
                                try { panelGrafica.Refresh(); } catch { }
                            }
                            else
                            {
                                // Si no existe chartIVA, crear y asignar como antes
                                var cartesianIVA = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
                                {
                                    Dock = DockStyle.Fill,
                                    ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                                    TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top,
                                    LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                                    BackColor = Color.White
                                };

                                cartesianIVA.Series = new ISeries[]
                                {
                                    new ColumnSeries<double>
                                    {
                                        Name = "IVA pagado",
                                        Values = ivaPagadoValues.ToArray(),
                                        Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                                        Stroke = null,
                                        DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                                        DataLabelsSize = 11,
                                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                                        MaxBarWidth = 40,
                                        DataLabelsFormatter = point => point.Model != null ? $"{point.Model:N0}" : string.Empty
                                    }
                                };

                                cartesianIVA.XAxes = new[]
                                {
                                    new Axis
                                    {
                                        Labels = labels.ToArray(),
                                        TextSize = 9,
                                        LabelsRotation = 0,
                                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230))
                                    }
                                };

                                cartesianIVA.YAxes = new[]
                                {
                                    new Axis
                                    {
                                        MinLimit = 0,
                                        TextSize = 9,
                                        SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)),
                                        Labeler = value => value >= 1000000 ? $"{value/1000000:N1}M"
                                                         : value >= 1000 ? $"{value/1000:N1}K"
                                                         : value.ToString("N0")
                                    }
                                };

                                cartesianIVA.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
                                {
                                    Text = "IVA pagado por mes y forma de pago (2/2)",
                                    TextSize = 12,
                                    Paint = new SolidColorPaint(SKColors.Black),
                                    Padding = new LiveChartsCore.Drawing.Padding(8)
                                };

                                // Mostrar solo si la gráfica IVA es la actual
                                cartesianIVA.Visible = (graficaActual == 1);

                                panelGrafica.Controls.Add(cartesianIVA);
                                panelGrafica.Controls.SetChildIndex(cartesianIVA, 0);
                                chartIVA = cartesianIVA;

                                AjustarAreaGrafica(chartIVA, panelGrafica, lblTituloGrafica);
                                MostrarGraficaActual();
                                try { chartIVA.Update(); } catch { }
                                try { chartIVA.Refresh(); } catch { }
                                try { panelGrafica.Refresh(); } catch { }
                            }
                        }
                        catch { }
                    });
                }
                catch { }

                // Comprobación de valores inválidos y fallback de prueba para IVA
                try
                {
                    bool hasInvalid = false;
                    foreach (var s in chartIVA.Series ?? Enumerable.Empty<ISeries>())
                    {
                        var valuesProp = s?.GetType().GetProperty("Values");
                        if (valuesProp == null) continue;
                        var valsObj = valuesProp.GetValue(s);
                        if (valsObj is System.Collections.IEnumerable vals)
                        {
                            foreach (var v in vals)
                            {
                                if (v == null) { hasInvalid = true; break; }
                                if (double.TryParse(v.ToString(), out double dv))
                                {
                                    if (double.IsNaN(dv) || double.IsInfinity(dv)) { hasInvalid = true; break; }
                                }
                                else { hasInvalid = true; break; }
                            }
                        }
                    }

                    int seriesCountNow = chartIVA.Series != null ? chartIVA.Series.Count() : 0;
                    var panelSizeNow = panelGrafica.ClientSize;
                    if (hasInvalid || seriesCountNow == 0 || panelSizeNow.Width < 100 || panelSizeNow.Height < 100)
                    {
                        var testSeries = new ISeries[]
                        {
                            new ColumnSeries<double>
                            {
                                Name = "Prueba IVA",
                                Values = new double[] { 50, 120, 200 },
                                Fill = new SolidColorPaint(new SKColor(46,204,113))
                            }
                        };

                        chartIVA.Series = testSeries;
                        chartIVA.XAxes = new[] { new Axis { Labels = new[] { "X", "Y", "Z" } } };
                        chartIVA.YAxes = new[] { new Axis { Labeler = v => v.ToString("N0") } };
                        chartIVA.Update(); chartIVA.Refresh(); panelGrafica.Refresh();
                        MessageBox.Show("Se asignó una serie de prueba para verificar renderizado de gráfica IVA.", "Depuración gráfica IVA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar gráfica IVA: {ex.Message}");
                if (graficaActual == 1)
                    lblTituloGrafica.Text = "Error al cargar gráfica IVA (2/2)";
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
                        if (!chkSinGlosa.Checked && cmbCliente.SelectedItem is BaseDatosComboItem item)
                        {
                            baseDatos = item.NombreReal;
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

                        // Obtener las tablas visibles actualmente en pantalla para el PDF
                        var tablaIGI = CrearTablaPdfIGIDesdeGrid();
                        var tablaIVA = CrearTablaPdfIVADesdeGrid();
                        var tablaDetalleCompleto = CrearTablaDetalleCompletoParaPdf();

                        if (tablaIGI.Rows.Count == 0)
                            tablaIGI = reporteService.ConvertirADataTableIGI(reporteActual);

                        if (tablaIVA.Rows.Count == 0)
                            tablaIVA = reporteService.ConvertirADataTableIVA(reporteActual);

                        var pdfService = new PdfGeneradorService();
                        pdfService.GenerarReporteIGIConFormasPagoPDF(
                            reporteActual,
                            tablaIGI,
                            tablaIVA,
                            tablaDetalleCompleto,
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
                ErrorMessageHelper.ShowError(
                    $"Error al generar el PDF:\n{ex.Message}",
                    "Error",
                    ex,
                    "Generación de PDF de reporte IGI/IVA"
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

                            // Hoja 3: Detalle Completo de Pedimentos (todos los registros RAW)
                            var worksheetDetalle = workbook.Worksheets.Add("Detalle Completo");

                            // Encabezados de detalle
                            var detalleHeaders = new[]
                            {
                                "Base Datos", "Clave", "Pedimento", "Fecha Pago", "IGI Pagado", "IGI Calculado", "Diferencia IGI",
                                "IVA Pagado", "Forma Pago IGI", "Forma Pago IVA", "Estatus Glosa", "Estatus Origen"
                            };

                            for (int col = 0; col < detalleHeaders.Length; col++)
                            {
                                worksheetDetalle.Cell(1, col + 1).Value = detalleHeaders[col];
                                worksheetDetalle.Cell(1, col + 1).Style.Font.Bold = true;
                                worksheetDetalle.Cell(1, col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                                worksheetDetalle.Cell(1, col + 1).Style.Font.FontColor = XLColor.White;
                                worksheetDetalle.Cell(1, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            // Rellenar datos desde reporteActual
                            int fila = 2;
                            foreach (var r in reporteActual)
                            {
                                worksheetDetalle.Cell(fila, 1).Value = r.BaseDatos ?? string.Empty;
                                worksheetDetalle.Cell(fila, 2).Value = r.Clave ?? string.Empty;
                                worksheetDetalle.Cell(fila, 3).Value = r.Pedimento ?? string.Empty;
                                worksheetDetalle.Cell(fila, 4).Value = r.FechaPago.HasValue ? r.FechaPago.Value.ToString("dd/MM/yyyy") : string.Empty;

                                worksheetDetalle.Cell(fila, 5).Value = r.IGI_Pagado;
                                worksheetDetalle.Cell(fila, 5).Style.NumberFormat.Format = "$#,##0.00";

                                worksheetDetalle.Cell(fila, 6).Value = r.IGI_Calculado;
                                worksheetDetalle.Cell(fila, 6).Style.NumberFormat.Format = "$#,##0.00";

                                // Usar la propiedad DiferenciaIGI para reflejar regla de forma de pago
                                worksheetDetalle.Cell(fila, 7).Value = r.DiferenciaIGI;
                                worksheetDetalle.Cell(fila, 7).Style.NumberFormat.Format = "$#,##0.00";

                                worksheetDetalle.Cell(fila, 8).Value = r.IVA_Pagado;
                                worksheetDetalle.Cell(fila, 8).Style.NumberFormat.Format = "$#,##0.00";

                                worksheetDetalle.Cell(fila, 9).Value = r.FormaPago_IGI ?? string.Empty;
                                worksheetDetalle.Cell(fila, 10).Value = r.FormaPago_IVA ?? string.Empty;
                                worksheetDetalle.Cell(fila, 11).Value = r.EstatusGlosa ?? string.Empty;
                                worksheetDetalle.Cell(fila, 12).Value = r.EstatusOrigen ?? string.Empty;

                                fila++;
                            }

                            // Ajustar ancho de columnas
                            worksheetDetalle.Columns().AdjustToContents();

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
                ErrorMessageHelper.ShowError(
                    $"Error al generar el archivo Excel:\n{ex.Message}",
                    "Error",
                    ex,
                    "Generación de Excel de reporte IGI/IVA"
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

            MostrarGraficaActual();

            if (graficaActual == 0)
                chartIGI?.Update();
            else
                chartIVA?.Update();
        }


    }
}
