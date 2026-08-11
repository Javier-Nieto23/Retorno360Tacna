using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
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

        public FrmCalculoInventarios()
        {
            InitializeComponent();

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
        }

        // ----------------------------------------------------
        // ESTADO DE LA PLANTILLA CONFIGURADA
        // ----------------------------------------------------
        private void cmbEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarEstadoPlantilla();
        }

        private void ActualizarEstadoPlantilla()
        {
            if (cmbEmpresa.SelectedValue != null &&
                int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa))
            {
                var cfg = PlantillaInventarioServicio.ObtenerParaEmpresa(idEmpresa);
                if (cfg != null && cfg.EstaConfigurada)
                {
                    lblPlantillaInfo.Text = $"📊  Plantilla: {Path.GetFileName(cfg.RutaArchivo)}  |  Hoja: {cfg.Hoja}  |  Operación: {cfg.Operacion}";
                    lblPlantillaInfo.ForeColor = Color.FromArgb(22, 90, 50);
                    btnCargarPlantilla.Text    = "✔ Plantilla configurada";
                    btnCargarPlantilla.Enabled = true;
                    return;
                }
            }

            lblPlantillaInfo.Text      = "📊  Sin plantilla para esta empresa  (configura una en Configuración)";
            lblPlantillaInfo.ForeColor = Color.FromArgb(120, 60, 30);
            btnCargarPlantilla.Enabled = false;
            btnCargarPlantilla.Text    = "Sin plantilla";
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
                MessageBox.Show("No hay plantilla configurada para esta empresa.\nVe a Configuración → Plantilla.",
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

        /// <summary>
        /// Carga las empresas asociadas a una razón social específica en el ComboBox de empresas.
        /// </summary>
        private void CargarEmpresas(int idRazon)
        {
            if (cmbEmpresa == null) return;

            try
            {
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
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las empresas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private readonly List<UcMesInventario> _paneles = new();

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
                    Width             = flpPaneles.ClientSize.Width - 30,
                    IdEmpresaActiva   = cmbEmpresa.SelectedValue != null &&
                                        int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmp)
                                        ? idEmp : 0
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

            MessageBox.Show($"Conciliación completada. Se actualizaron {calculados} de {_paneles.Count} mes(es).",
                            "Proceso Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            if (_paneles.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
    }
}