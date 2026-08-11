using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmAnexos : Form
    {

        private CatalogoPartesService catalogoService;
        private RetornoService retornoService;
        private ConexionInfo conexion;

        // Parameterless ctor for designer support
        public FrmAnexos()
        {
            // Only initialize UI for designer; do not access runtime services
            InitializeComponent();
        }

        public FrmAnexos(ConexionInfo conexionInfo) : this()
        {
            // runtime initialization
            conexion = conexionInfo;
            catalogoService = new CatalogoPartesService(conexionInfo);
            retornoService = new RetornoService(conexionInfo);

            // Load data only at runtime (not at design-time)
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                CargarRazonesSociales();
            }
        }

        // InitializeComponent is provided by the designer partial class (FrmAnexos.Designer.cs)

        private void CargarRazonesSociales()
        {
            try
            {
                var razones = catalogoService.ObtenerRazonesSociales();
                cmbRazon.DataSource = razones;
                cmbRazon.DisplayMember = "NombreRazon";
                cmbRazon.ValueMember = "IdRazon";
                cmbRazon.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando razones sociales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbRazon_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRazon.SelectedItem is RazonSocial rz)
            {
                CargarBasesDatosRazon(rz.IdRazon);
            }
            else
            {
                cmbBase.DataSource = null;
            }
        }

        private async void btnGenerar_Click(object? sender, EventArgs e)
        {
            await GenerarPreviewAsync();
        }

        private async void btnGuardarPortal_Click(object? sender, EventArgs e)
        {
            await GuardarPreviewEnPortalAsync();
        }

        private async void btnExportarExcel_Click(object? sender, EventArgs e)
        {
            await ExportarExcelAsync();
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                var bases = catalogoService.ObtenerBasesDatosRazon(idRazon);
                cmbBase.DataSource = CrearItemsBaseDatos(bases);
                cmbBase.DisplayMember = "NombreVisible";
                cmbBase.ValueMember = "NombreReal";
                cmbBase.SelectedIndex = -1;
            }
            catch { cmbBase.DataSource = null; }
        }

        private static List<object> CrearItemsBaseDatos(IEnumerable<string> bases)
        {
            return bases.Select(b => new { NombreReal = b, NombreVisible = b.Replace("SEERT_", "", StringComparison.OrdinalIgnoreCase).Trim('_','-',' ') }).Cast<object>().ToList();
        }

        private List<DateTime> ObtenerMesesEnRango(DateTime inicio, DateTime fin)
        {
            var meses = new List<DateTime>();
            var actual = new DateTime(inicio.Year, inicio.Month, 1);
            var finMes = new DateTime(fin.Year, fin.Month, 1);
            while (actual <= finMes)
            {
                meses.Add(actual);
                actual = actual.AddMonths(1);
            }
            return meses;
        }

        private async Task GenerarPreviewAsync()
        {
            try
            {
                dgvPreview.DataSource = null;
                btnGenerar.Enabled = false;

                var razon = cmbRazon.SelectedItem as RazonSocial;
                if (razon == null)
                {
                    MessageBox.Show("Seleccione una razón social.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var bases = new List<string>();
                if (cmbBase.SelectedItem != null)
                {
                    dynamic it = cmbBase.SelectedItem;
                    bases.Add((string)it.NombreReal);
                }
                else
                {
                    bases = catalogoService.ObtenerBasesDatosRazon(razon.IdRazon);
                }

                var meses = ObtenerMesesEnRango(dtpInicio.Value, dtpFin.Value);

                var dt = new DataTable();
                dt.Columns.Add("MES", typeof(string));
                dt.Columns.Add("AÑO", typeof(int));
                dt.Columns.Add("PLANTA", typeof(string));
                dt.Columns.Add("TOTAL_NP", typeof(decimal));
                dt.Columns.Add("ALTAS_NP", typeof(decimal));
                dt.Columns.Add("VIGENTE_BOM", typeof(decimal));
                dt.Columns.Add("PCT_BASE_LIMPIA", typeof(decimal));
                dt.Columns.Add("PCT_RETORNOS_CUBIERTOS", typeof(decimal));

                foreach (var mes in meses)
                {
                    var inicioMes = mes;
                    var finMes = mes.AddMonths(1).AddDays(-1);

                    foreach (var baseDb in bases)
                    {
                        // Obtener partes (materia prima y otros) en el mes
                        var partes = await Task.Run(() => catalogoService.ObtenerMateriaPrimaBOMMultiple(baseDb, inicioMes, finMes));
                        var totalNp = partes.Count; // total NP (puede representar el parcial o total según definición)
                        var altasNp = partes.Count; // Altas NP = partes con inserción en el periodo
                        var vigenteBom = partes.Count(p => (p.EstatusComponente ?? string.Empty).Equals("VIGENTE EN BOM", StringComparison.OrdinalIgnoreCase));

                        // % Base de Datos Limpia: =SI([Altas NP]=0, 1, [Vigente BOM]/[Altas NP])
                        decimal pctBaseLimpia = (altasNp == 0) ? 1m : ((decimal)vigenteBom / (decimal)altasNp);

                        // Obtener porcentaje retorno para ese mes+base (RetornoService devuelve porcentaje en unidades, p.ej. 116.25)
                        ResultadoRetorno res = await Task.Run(() => retornoService.CalcularRetorno(razon.IdRazon, baseDb, inicioMes, finMes, false, true));
                        decimal pctRetornos = res?.PorcentajeRetorno ?? 0m;
                        // Convertir a fracción para mostrar/formatear como porcentaje (P2)
                        decimal pctRetornosFraction = pctRetornos / 100m;

                        dt.Rows.Add(mes.ToString("MMMM").ToUpper(), mes.Year, baseDb, totalNp, altasNp, vigenteBom, pctBaseLimpia, pctRetornosFraction);
                    }
                }

                dgvPreview.DataSource = dt;

                // Formatear columnas de porcentaje para mostrar como %
                try
                {
                    if (dgvPreview.Columns.Contains("PCT_BASE_LIMPIA"))
                        dgvPreview.Columns["PCT_BASE_LIMPIA"].DefaultCellStyle.Format = "P2";
                    if (dgvPreview.Columns.Contains("PCT_RETORNOS_CUBIERTOS"))
                        dgvPreview.Columns["PCT_RETORNOS_CUBIERTOS"].DefaultCellStyle.Format = "P2";
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerar.Enabled = true;
            }
        }

        private async Task GuardarPreviewEnPortalAsync()
        {
            if (dgvPreview.DataSource is not DataTable dt || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para guardar. Genere el preview primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                btnGuardarPortal.Enabled = false;
                var razon = cmbRazon.SelectedItem as RazonSocial;
                bool ok = await PortalWebService.GuardarAnexosAsync(dt, razon?.NombreRazon ?? string.Empty);

                if (ok) MessageBox.Show("Datos guardados en portal correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else MessageBox.Show("Ocurrió un error al guardar en portal. Revise logs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en portal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardarPortal.Enabled = true;
            }
        }

        private async Task ExportarExcelAsync()
        {
            if (dgvPreview.DataSource is not DataTable dt || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var save = new SaveFileDialog();
            save.Filter = "Archivos Excel|*.xlsx";
            save.FileName = $"Anexos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            if (save.ShowDialog() != DialogResult.OK) return;

            // Simple export using ClosedXML
            try
            {
                await Task.Run(() =>
                {
                    using var wb = new ClosedXML.Excel.XLWorkbook();
                    var ws = wb.Worksheets.Add("Anexos");
                    for (int c = 0; c < dt.Columns.Count; c++) ws.Cell(1, c + 1).Value = dt.Columns[c].ColumnName;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var val = dt.Rows[r][c];
                            ws.Cell(r + 2, c + 1).Value = val == null || val == DBNull.Value ? string.Empty : val.ToString();
                        }
                    }
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(save.FileName);
                });
                MessageBox.Show("Excel generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
