using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace Retorno360Tacna.FORMS
{
    public partial class FrmCalculoInventarios : Form
    {
        private readonly SesionCalculoInventario _sesion = new();

        // ----------------------------------------------------
        // VARIABLES PARA ALMACENAR RAZÓN SOCIAL Y EMPRESA
        // ----------------------------------------------------
        private string _razonSocial = string.Empty;
        private string _nombreEmpresa = string.Empty;
        private MODELS.Usuario? usuarioActual;
        private SERVICES.PerfilUsuarioService? perfilService;

        public FrmCalculoInventarios() : this(null) { }

        public FrmCalculoInventarios(MODELS.Usuario? usuario)
        {
            InitializeComponent();
            usuarioActual = usuario;

            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            this.Load += FrmCalculoInventarios_Load;

            pnlCantidadMeses.Visible = true;
            pnlCaptura.Visible = false;
        }

        // ----------------------------------------------------
        // EVENTO LOAD DEL FORMULARIO
        // ----------------------------------------------------
        private void FrmCalculoInventarios_Load(object sender, EventArgs e)
        {
            // 1. Cargar el primer ComboBox
            CargarRazonesSociales();

            // 2. Suscribir el evento para cambios posteriores del usuario
            cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;

            // 3. Forzar manualmente la primera carga del segundo ComboBox (Empresas)
            if (cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
            {
                CargarEmpresas(idRazon);
            }

            // 4. Mostrar estado de la plantilla guardada
            ActualizarEstadoPlantilla();

            //5. Cargar histórico de inventarios
            CargarHistorico();
        }

        // ----------------------------------------------------
        // ESTADO DE LA PLANTILLA CONFIGURADA
        // ----------------------------------------------------
        private void cmbEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarEstadoPlantilla();
            CargarHistorico();
        }

        private void ActualizarEstadoPlantilla()
        {
            if (cmbEmpresa.SelectedValue != null &&
                int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa))
            {
                var cfg = PlantillaInventarioServicio.ObtenerParaEmpresa(idEmpresa);
                if (cfg != null && cfg.EstaConfigurada)
                {
                    lblPlantillaInfo.Text = $"??  Plantilla: {Path.GetFileName(cfg.RutaArchivo)}  |  Hoja: {cfg.Hoja}  |  Operación: {cfg.Operacion}";
                    lblPlantillaInfo.ForeColor = Color.FromArgb(22, 90, 50);
                    btnCargarPlantilla.Text    = "? Plantilla configurada";
                    btnCargarPlantilla.Enabled = true;
                    return;
                }
            }

            lblPlantillaInfo.Text      = "??  Sin plantilla para esta empresa  (configura una en Configuración)";
            lblPlantillaInfo.ForeColor = Color.FromArgb(120, 60, 30);
            btnCargarPlantilla.Enabled = false;
            btnCargarPlantilla.Text    = "Sin plantilla";
        }



        public class InventarioHistorialItem
        {
            public int NumeroMes { get; set; }
            public string Mes { get; set; } = string.Empty;
            public string TipoInventario { get; set; } = string.Empty;
            public string Operacion { get; set; } = string.Empty;
            public string CampoTotal { get; set; } = string.Empty;
            public string? CampoA { get; set; }
            public string? CampoB { get; set; }
            public decimal Total { get; set; }
            public int IdEmpresa { get; set; }
            public int IdRazonSocial { get; set; }
        }


        private void CargarHistorico()
        {
            if (cmbEmpresa.SelectedValue == null || cmbRazonSocial.SelectedValue == null)
            {
                dgvHistorico.DataSource = null;
                return;
            }

            if (!int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa) ||
                !int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
            {
                dgvHistorico.DataSource = null;
                return;
            }

            try
            {
                var historialService = new InventarioHistorialService();
                var historico = historialService.ObtenerHistorico(idEmpresa, idRazon);

                dgvHistorico.DataSource = null;
                dgvHistorico.DataSource = historico;
                ConfigurarColumnasHistorico();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el histérico: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasHistorico()
        {
            if (dgvHistorico.Columns.Count == 0) return;

            // Ocultar columnas internas
            foreach (DataGridViewColumn col in dgvHistorico.Columns)
                col.Visible = false;

            string[] visibles = { "Mes", "TipoInventario", "Operacion", "CampoTotal", "CampoA", "CampoB", "Total" };
            string[] encabezados = { "Mes", "Tipo Inventario", "Operación", "Campo Total", "Campo A", "Campo B", "Total" };

            for (int i = 0; i < visibles.Length; i++)
            {
                if (dgvHistorico.Columns.Contains(visibles[i]))
                {
                    dgvHistorico.Columns[visibles[i]].Visible = true;
                    dgvHistorico.Columns[visibles[i]].HeaderText = encabezados[i];
                }
            }

            if (dgvHistorico.Columns.Contains("Total"))
            {
                dgvHistorico.Columns["Total"].DefaultCellStyle.Format = "N2";
                dgvHistorico.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Estilo de cabecera
            dgvHistorico.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 80, 50);
            dgvHistorico.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistorico.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvHistorico.EnableHeadersVisualStyles = false;

            // Filas alternas
            dgvHistorico.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 243);
        }


        private void btnCargarPlantilla_Click(object sender, EventArgs e)
        {
            if (cmbEmpresa.SelectedValue == null ||
                !int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa))
            {
                MessageBox.Show("Selecciona una empresa primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var cfg = PlantillaInventarioServicio.ObtenerParaEmpresa(idEmpresa);
            if (cfg == null || !cfg.EstaConfigurada)
            {
                MessageBox.Show("No hay plantilla configurada para esta empresa.\nVe a Configuración ? Plantilla.",
                    "Sin plantilla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                $"Plantilla cargada:\n• Archivo: {Path.GetFileName(cfg.RutaArchivo)}\n• Hoja: {cfg.Hoja}\n• Operación: {cfg.Operacion}\n\nAl cargar el Excel mensual se te pedirá relacionar sus columnas con los campos de la plantilla.",
                "Plantilla lista", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ----------------------------------------------------
        // CARGAR DATOS DE LA RAZÓN SOCIAL
        // ----------------------------------------------------
        private void CargarRazonesSociales()
        {
            try
            {
                if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                {
                    var razones = perfilService.ObtenerRazonesSocialesDePerfil(usuarioActual.IdUsuario);
                    cmbRazonSocial.DataSource = null;
                    cmbRazonSocial.DisplayMember = "NombreRazon";
                    cmbRazonSocial.ValueMember = "IdRazon";
                    cmbRazonSocial.DataSource = razones;
                    if (razones.Count > 0) cmbRazonSocial.SelectedIndex = 0;
                    return;
                }

                Conexion conexion = new Conexion();
                string cnx = @"SELECT IdRazon, Nombre_Razon FROM RAZONXTABLA ORDER BY Nombre_Razon";

                using SqlConnection connection = new SqlConnection(conexion.GetConnectionString());
                using SqlDataAdapter da = new SqlDataAdapter(cnx, connection);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Si la consulta devuelve filas
                if (dt.Rows.Count > 0)
                {
                    // 1. Limpiar asignaciones previas
                    cmbRazonSocial.DataSource = null;

                    // 2. Determinar columnas existentes (robusto a mayúsculas/minúsculas)
                    string displayCol = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "Nombre_Razon", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    string valueCol = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "IdRazon", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    // 3. Definir miembros ANTES del DataSource
                    cmbRazonSocial.DisplayMember = displayCol;
                    cmbRazonSocial.ValueMember = valueCol;

                    // 4. Asignar origen de datos
                    cmbRazonSocial.DataSource = dt;

                    // 5. Seleccionar el primer elemento por defecto para forzar la carga
                    cmbRazonSocial.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No se encontraron razones sociales en la base de datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las razones sociales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------- 
        // EVENTO AL CAMBIAR DE RAZÓN SOCIAL
        // ----------------------------------------------------
        private void cmbRazonSocial_SelectedIndexChanged(object? sender, EventArgs e) // Modificado object? para corregir CS8622
        {
            // Guard de seguridad integral
            if (cmbEmpresa == null || cmbRazonSocial == null) return;

            // Conversión segura de SelectedValue a int
            if (cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
            {
                CargarEmpresas(idRazon);
            }
            else
            {
                cmbEmpresa.DataSource = null;
            }
        }

        private void GuardarCalculoEnHistorial(SesionCalculoInventario sesion)
        {
            try
            {
                var historialService = new InventarioHistorialService();
                historialService.GuardarResultados(sesion.Resultados);

                MessageBox.Show("El historial de inventarios se ha guardado correctamente. ",
                        "Exito", MessageBoxButtons.OK,MessageBoxIcon.Information);
             
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el historial: {ex.Message}",
                    "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }





        /// <summary>
        /// Carga las empresas asociadas a una razón social específica en el ComboBox de empresas.
        /// </summary>
        private void CargarEmpresas(int idRazon)
        {
            if (cmbEmpresa == null) return;

            try
            {
                if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                {
                    var empresas = perfilService.ObtenerEmpresasDePerfilPorRazon(usuarioActual.IdUsuario, idRazon);
                    cmbEmpresa.DataSource = null;
                    if (empresas.Count > 0)
                    {
                        cmbEmpresa.DisplayMember = "NombreTabla";
                        cmbEmpresa.ValueMember = "IdTabla";
                        cmbEmpresa.DataSource = empresas;
                        cmbEmpresa.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron empresas guardadas en su perfil para la razón social seleccionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    ActualizarEstadoPlantilla();
                    return;
                }

                Conexion conexion = new Conexion();
                // Consulta con parámetro para prevenir inyección SQL
                string cnx = "SELECT n.IdTabla, n.NOMBRE_TABLA FROM NOM_TABLARAZON n WHERE n.IdRazon = @IdRazon ORDER BY n.NOMBRE_TABLA";

                using SqlConnection connection = new SqlConnection(conexion.GetConnectionString());
                using SqlCommand cmd = new SqlCommand(cnx, connection);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);

                using SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);


                cmbEmpresa.DataSource = null; // Limpiar asignaciones previas
                if (dt.Rows.Count > 0)
                {
                    // Determinar columnas existentes (robusto a mayúsculas/minúsculas)
                    string displayCol = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "NOMBRE_TABLA", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(n, "Nombre_Tabla", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(n, "NOMBRE_TABLA", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    string valueCol = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "IdTabla", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(n, "Idtabla", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(n, "IdTabla", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    cmbEmpresa.DisplayMember = displayCol;
                    cmbEmpresa.ValueMember = valueCol;
                    cmbEmpresa.DataSource = dt;
                    cmbEmpresa.SelectedIndex = 0;
                }
                else
                {
                    // Mostrar aviso si no hay empresas asociadas
                    MessageBox.Show("No se encontraron empresas para la razón social seleccionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ActualizarEstadoPlantilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las empresas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private readonly List<UcMesInventario> _paneles = new();

        private void chkUsarPerfil_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkUsarPerfil.Checked && (usuarioActual == null || perfilService == null))
            {
                MessageBox.Show("No se ha cargado el perfil de usuario. Cierre y vuelva a abrir el formulario.",
                    "Perfil no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkUsarPerfil.Checked = false;
                return;
            }
            CargarRazonesSociales();
        }

        private void btnIniciarCalculo_Click(object sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedIndex == -1 || cmbRazonSocial.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una razón social.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbRazonSocial.Focus();
                return;
            }

            if (cmbEmpresa.SelectedIndex == -1 || cmbEmpresa.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una empresa.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbEmpresa.Focus();
                return;
            }

            _razonSocial = cmbRazonSocial.Text.Trim();
            _nombreEmpresa = cmbEmpresa.Text.Trim();

            // ----------------------------------------------------
            // NUEVO: capturar los IDs seleccionados de forma segura
            // ----------------------------------------------------
            int idEmpresaSeleccionada = cmbEmpresa.SelectedValue != null &&
                int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpParsed)
                ? idEmpParsed : 0;

            int idRazonSeleccionada = cmbRazonSocial.SelectedValue != null &&
                int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazonParsed)
                ? idRazonParsed : 0;

            int cantidadMeses = (int)nudCantidadMeses.Value;

            if (cantidadMeses < 1)
            {
                MessageBox.Show("Debe indicar al menos 1 mes a calcular.",
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlCantidadMeses.Visible = false;
            pnlCaptura.Visible = true;

            _sesion.Iniciar(cantidadMeses);

            flpPaneles.SuspendLayout();
            flpPaneles.Controls.Clear();
            _paneles.Clear();

            for (int i = 1; i <= cantidadMeses; i++)
            {
                var panel = new UcMesInventario(i)
                {
                    Width = flpPaneles.ClientSize.Width - 30,
                    IdEmpresaActiva = idEmpresaSeleccionada,
                    IdRazonSocialActiva = idRazonSeleccionada   // NUEVO
                };

                panel.ResultadoActualizado += Panel_ResultadoActualizado;
                _paneles.Add(panel);
                flpPaneles.Controls.Add(panel);
            }

            flpPaneles.ResumeLayout();
        }
        private void Panel_ResultadoActualizado(object? sender, EventArgs e)
        {
            ActualizarGridYTotalGeneral();
        }

        private void ActualizarGridYTotalGeneral()
        {
            var resultados = _paneles
                .Where(p => p.Resultado != null && !p.Resultado.TieneError)
                .Select(p => p.Resultado!)
                .ToList();

            dgvResultados.DataSource = null;
            dgvResultados.DataSource = resultados;

            decimal totalGeneral = resultados.Sum(r => r.Total);
            lblTotalGeneral.Text = $"Total general: {totalGeneral:N2}";

            btnExportarExcel.Enabled = resultados.Count == _paneles.Count;
        }

        private void flpPaneles_Resize(object sender, EventArgs e)
        {
            foreach (Control ctrl in flpPaneles.Controls)
            {
                if (ctrl is UcMesInventario uc)
                {
                    uc.Width = flpPaneles.ClientSize.Width - 30;
                }
            }
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            if (_paneles.Count == 0)
            {
                MessageBox.Show("No hay paneles de meses para conciliar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int calculados = 0;
            foreach (var panel in _paneles)
            {
                panel.IntentarCalcular();
                if (panel.Resultado != null && !panel.Resultado.TieneError)
                {
                    calculados++;
                }
            }

            ActualizarGridYTotalGeneral();

            // Obtener los resultados válidos para guardarlos en el historial
            var resultadosValidos = _paneles
                .Where(p => p.Resultado != null && !p.Resultado.TieneError)
                .Select(p => p.Resultado!)
                .ToList();

            if (resultadosValidos.Count > 0)
            {
                try
                {
                    var historialService = new InventarioHistorialService();
                    historialService.GuardarResultados(resultadosValidos);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"La conciliación finalizó, pero ocurrió un error al guardar el historial en la base de datos: {ex.Message}",
                                    "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            MessageBox.Show($"Conciliación completada y guardada en el historial. Se actualizaron {calculados} de {_paneles.Count} mes(es).",
                            "Proceso Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            if (_paneles.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var resultadosValidos = _paneles
                .Where(p => p.Resultado != null && !p.Resultado.TieneError)
                .Select(p => p.Resultado!)
                .ToList();

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"Reporte_Inventarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            string rutaGuardado = sfd.FileName;

            try
            {
                using XLWorkbook workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("MATERIA PRIMA");

                ws.Style.Font.FontName = "Century Gothic";
                ws.Style.Font.FontSize = 10;

                // ----------------------------------------------------
                // 1. ENCABEZADO Y TÍTULOS
                // ----------------------------------------------------
                ws.Cell("D2").Value = "INVENTARIO DE MATERIA PRIMA";
                ws.Cell("D2").Style.Font.Bold = true;
                ws.Cell("D2").Style.Font.FontSize = 12;

                ws.Cell("D3").Value = $"{_razonSocial.ToUpper()} - {_nombreEmpresa.ToUpper()}";
                ws.Cell("D3").Style.Font.Bold = true;

                ws.Cell("D4").Value = $"ENERO-DICIEMBRE {DateTime.Now.Year}";
                ws.Cell("D4").Style.Font.Bold = true;

                // ----------------------------------------------------
                // 2. LEYENDA
                // ----------------------------------------------------
                var azulOscuro = XLColor.FromHtml("#0D233A");
                var rojoTacna = XLColor.FromHtml("#BA0000");

                ws.Cell("I1").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("J1").Value = "CALCULO A TRAVES DEL INVENTARIO ENVIADO";
                ws.Cell("J1").Style.Font.FontSize = 8;

                ws.Cell("I2").Style.Fill.BackgroundColor = rojoTacna;
                ws.Cell("J2").Value = "SIN INVENTARIO ENTREGADO, CALCULO A TRAVES DEL SISTEMA";
                ws.Cell("J2").Style.Font.FontSize = 8;

                // ----------------------------------------------------
                // 3. TABLA HORIZONTAL
                // ----------------------------------------------------
                string[] mesesNombres = {
                    "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO",
                    "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE"
                };

                ws.Cell("A6").Value = "EMPRESA";
                ws.Cell("A6").Style.Font.Bold = true;
                ws.Cell("A6").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("A6").Style.Font.FontColor = XLColor.White;

                ws.Cell("B6").Value = $"DICIEMBRE {DateTime.Now.Year - 1}";
                ws.Cell("B6").Style.Font.Bold = true;
                ws.Cell("B6").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("B6").Style.Font.FontColor = XLColor.White;

                var resultadosMap = _paneles
                    .Where(p => p.Resultado != null && !p.Resultado.TieneError)
                    .GroupBy(p => p.Resultado!.NumeroMes)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Resultado!.Total));

                int colInicio = 3;

                for (int i = 1; i <= 12; i++)
                {
                    IXLCell celdaHeader = ws.Cell(6, colInicio + (i - 1));
                    IXLCell celdaValor = ws.Cell(7, colInicio + (i - 1));

                    celdaHeader.Value = mesesNombres[i - 1];
                    celdaHeader.Style.Font.Bold = true;
                    celdaHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    bool estaCalculado = resultadosMap.TryGetValue(i, out decimal valorTotal);

                    if (estaCalculado)
                    {
                        celdaHeader.Style.Fill.BackgroundColor = azulOscuro;
                        celdaHeader.Style.Font.FontColor = XLColor.White;
                        celdaValor.Value = valorTotal;
                    }
                    else
                    {
                        celdaHeader.Style.Fill.BackgroundColor = rojoTacna;
                        celdaHeader.Style.Font.FontColor = XLColor.White;
                        celdaValor.Value = 0m;
                    }

                    celdaValor.Style.NumberFormat.Format = "$ #,##0.00";
                    celdaValor.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                // ----------------------------------------------------
                // 4. DATOS DE FILA (Dinamizados con la variable de empresa)
                // ----------------------------------------------------
                ws.Cell("A7").Value = string.IsNullOrEmpty(_nombreEmpresa) ? "EMPRESA" : _nombreEmpresa.ToUpper();
                ws.Cell("A7").Style.Font.Bold = true;
                ws.Cell("B7").Value = 0m;
                ws.Cell("B7").Style.NumberFormat.Format = "$ #,##0.00";

                var rangoTabla = ws.Range(6, 1, 7, colInicio + 11);
                rangoTabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rangoTabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ----------------------------------------------------
                // 5. INSERTAR LOGO TACNA (Si existe)
                // ----------------------------------------------------
                string rutaLogo = Path.Combine(Application.StartupPath, "logo_tacna.png");
                if (File.Exists(rutaLogo))
                {
                    ws.AddPicture(rutaLogo)
                      .MoveTo(ws.Cell("A1"))
                      .WithSize(180, 50);
                }

                ws.Columns().AdjustToContents();
                ws.Column("A").Width = 25;

                workbook.SaveAs(rutaGuardado);

                var respuesta = MessageBox.Show(
                    "¡Reporte generado exitosamente!\n\n¿Desea abrir el archivo Excel en este momento?",
                    "Exportación Completada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rutaGuardado,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

   
            private void btnExportarHistoricoExcel_Click(object sender, EventArgs e)
        {
            if (dgvHistorico.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos en el historial para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var historico = dgvHistorico.DataSource as List<InventarioHistorialItem>;
            if (historico == null || historico.Count == 0)
            {
                MessageBox.Show("No hay datos en el historial para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"Historico_Inventarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            string rutaGuardado = sfd.FileName;

            try
            {
                using XLWorkbook workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("MATERIA PRIMA");

                ws.Style.Font.FontName = "Century Gothic";
                ws.Style.Font.FontSize = 10;

                // ----------------------------------------------------
                // 1. ENCABEZADO Y TÍTULOS
                // ----------------------------------------------------
                ws.Cell("D2").Value = "HISTORIAL DE INVENTARIOS";
                ws.Cell("D2").Style.Font.Bold = true;
                ws.Cell("D2").Style.Font.FontSize = 12;

                ws.Cell("D3").Value = $"{_razonSocial.ToUpper()} - {_nombreEmpresa.ToUpper()}";
                ws.Cell("D3").Style.Font.Bold = true;

                ws.Cell("D4").Value = $"ENERO-DICIEMBRE {DateTime.Now.Year}";
                ws.Cell("D4").Style.Font.Bold = true;

                // ----------------------------------------------------
                // 2. LEYENDA
                // ----------------------------------------------------
                var azulOscuro = XLColor.FromHtml("#0D233A");
                var rojoTacna = XLColor.FromHtml("#BA0000");

                ws.Cell("I1").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("J1").Value = "CALCULO A TRAVES DEL INVENTARIO ENVIADO";
                ws.Cell("J1").Style.Font.FontSize = 8;

                ws.Cell("I2").Style.Fill.BackgroundColor = rojoTacna;
                ws.Cell("J2").Value = "SIN INVENTARIO ENTREGADO, CALCULO A TRAVES DEL SISTEMA";
                ws.Cell("J2").Style.Font.FontSize = 8;

                // ----------------------------------------------------
                // 3. TABLA HORIZONTAL
                // ----------------------------------------------------
                string[] mesesNombres = {
            "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO",
            "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE"
        };

                ws.Cell("A6").Value = "EMPRESA";
                ws.Cell("A6").Style.Font.Bold = true;
                ws.Cell("A6").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("A6").Style.Font.FontColor = XLColor.White;

                ws.Cell("B6").Value = $"DICIEMBRE {DateTime.Now.Year - 1}";
                ws.Cell("B6").Style.Font.Bold = true;
                ws.Cell("B6").Style.Fill.BackgroundColor = azulOscuro;
                ws.Cell("B6").Style.Font.FontColor = XLColor.White;

                // ----------------------------------------------------
                // Filtrar histórico al año actual y agrupar por mes
                // (Mes viene como texto "MMMM yyyy", igual que en btnEditarHistorico/btnEliminarHistorico)
                // ----------------------------------------------------
                var itemsDelAnio = historico
                    .Where(h => DateTime.TryParseExact(h.Mes, "MMMM yyyy",
                                    new CultureInfo("es-ES"), DateTimeStyles.None, out DateTime fecha)
                                && fecha.Year == DateTime.Now.Year)
                    .ToList();

                var resultadosMap = itemsDelAnio
                    .GroupBy(h => h.NumeroMes)
                    .ToDictionary(g => g.Key, g => g.Sum(h => h.Total));

                // Determina si el mes fue calculado con inventario ENVIADO (azul) o por SISTEMA (rojo).
                // Ajusta esta condición si "TipoInventario" usa otros valores/textos.
                var tipoPorMes = itemsDelAnio
                    .GroupBy(h => h.NumeroMes)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Any(h => (h.TipoInventario ?? "").Contains("Sistema", StringComparison.OrdinalIgnoreCase))
                    );

                int colInicio = 3;

                for (int i = 1; i <= 12; i++)
                {
                    IXLCell celdaHeader = ws.Cell(6, colInicio + (i - 1));
                    IXLCell celdaValor = ws.Cell(7, colInicio + (i - 1));

                    celdaHeader.Value = mesesNombres[i - 1];
                    celdaHeader.Style.Font.Bold = true;
                    celdaHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    bool estaCalculado = resultadosMap.TryGetValue(i, out decimal valorTotal);
                    bool esSistema = tipoPorMes.TryGetValue(i, out bool esSist) && esSist;

                    if (estaCalculado && !esSistema)
                    {
                        celdaHeader.Style.Fill.BackgroundColor = azulOscuro;
                        celdaHeader.Style.Font.FontColor = XLColor.White;
                        celdaValor.Value = valorTotal;
                    }
                    else
                    {
                        celdaHeader.Style.Fill.BackgroundColor = rojoTacna;
                        celdaHeader.Style.Font.FontColor = XLColor.White;
                        celdaValor.Value = estaCalculado ? valorTotal : 0m;
                    }

                    celdaValor.Style.NumberFormat.Format = "$ #,##0.00";
                    celdaValor.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                // ----------------------------------------------------
                // 4. DATOS DE FILA (Dinamizados con la variable de empresa)
                // ----------------------------------------------------
                ws.Cell("A7").Value = string.IsNullOrEmpty(_nombreEmpresa) ? "EMPRESA" : _nombreEmpresa.ToUpper();
                ws.Cell("A7").Style.Font.Bold = true;
                ws.Cell("B7").Value = 0m;
                ws.Cell("B7").Style.NumberFormat.Format = "$ #,##0.00";

                var rangoTabla = ws.Range(6, 1, 7, colInicio + 11);
                rangoTabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rangoTabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ----------------------------------------------------
                // 5. INSERTAR LOGO TACNA (Si existe)
                // ----------------------------------------------------
                string rutaLogo = Path.Combine(Application.StartupPath, "logo_tacna.png");
                if (File.Exists(rutaLogo))
                {
                    ws.AddPicture(rutaLogo)
                      .MoveTo(ws.Cell("A1"))
                      .WithSize(180, 50);
                }

                ws.Columns().AdjustToContents();
                ws.Column("A").Width = 25;

                workbook.SaveAs(rutaGuardado);

                var respuesta = MessageBox.Show(
                    "¡Reporte generado exitosamente!\n\n¿Desea abrir el archivo Excel en este momento?",
                    "Exportación Completada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rutaGuardado,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void btnEditarHistorico_Click(object sender, EventArgs e)
        {
            if (dgvHistorico.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un registro del historial para editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = dgvHistorico.CurrentRow.DataBoundItem as InventarioHistorialItem;
            if (item == null) return;

            string? input = MostrarInputDialog(
                $"Editar Total para {item.Mes}:\n(Valor actual: {item.Total:N2})",
                "Editar Total",
                item.Total.ToString("F2"));

            if (string.IsNullOrWhiteSpace(input)) return;

            if (!decimal.TryParse(input.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal nuevoTotal))
            {
                MessageBox.Show("El valor ingresado no es un n\u00famero v\u00e1lido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int anio = DateTime.Now.Year;
                if (DateTime.TryParseExact(item.Mes, "MMMM yyyy",
                    new System.Globalization.CultureInfo("es-ES"),
                    System.Globalization.DateTimeStyles.None, out DateTime fechaMes))
                    anio = fechaMes.Year;

                new InventarioHistorialService()
                    .ActualizarTotal(item.IdEmpresa, item.IdRazonSocial, item.NumeroMes, anio, nuevoTotal);

                CargarHistorico();
                MessageBox.Show("Total actualizado correctamente.", "\u00c9xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminarHistorico_Click(object sender, EventArgs e)
        {
            if (dgvHistorico.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un registro del historial para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = dgvHistorico.CurrentRow.DataBoundItem as InventarioHistorialItem;
            if (item == null) return;

            if (MessageBox.Show(
                $"\u00bfEst\u00e1 seguro de eliminar el registro de {item.Mes} con Total {item.Total:N2}?",
                "Confirmar eliminaci\u00f3n", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                int anio = DateTime.Now.Year;
                if (DateTime.TryParseExact(item.Mes, "MMMM yyyy",
                    new System.Globalization.CultureInfo("es-ES"),
                    System.Globalization.DateTimeStyles.None, out DateTime fechaMes))
                    anio = fechaMes.Year;

                new InventarioHistorialService()
                    .EliminarRegistro(item.IdEmpresa, item.IdRazonSocial, item.NumeroMes, anio);

                CargarHistorico();
                MessageBox.Show("Registro eliminado correctamente.", "\u00c9xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string? MostrarInputDialog(string mensaje, string titulo, string valorInicial)
        {
            using var form = new Form
            {
                Text = titulo,
                Size = new Size(380, 160),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label { Text = mensaje, AutoSize = true, Location = new Point(16, 14) };
            var txt = new TextBox { Text = valorInicial, Location = new Point(16, 50), Width = 330 };
            var btnOk = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(200, 85), Width = 80 };
            var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(290, 85), Width = 80 };
            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;
            return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
        }
    }
}
