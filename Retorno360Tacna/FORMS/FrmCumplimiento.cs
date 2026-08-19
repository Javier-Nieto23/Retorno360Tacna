using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmCumplimiento : Form
    {
        private readonly CumplimientoAnexosService cumplimientoService;
        private readonly ConexionInfo conexion;
        private MODELS.Usuario? usuarioActual;
        private SERVICES.PerfilUsuarioService? perfilService;

        public FrmCumplimiento()
        {
            InitializeComponent();
            cumplimientoService = null!;
            conexion = null!;
        }

        public FrmCumplimiento(ConexionInfo conexionInfo) : this(conexionInfo, null) { }

        public FrmCumplimiento(ConexionInfo conexionInfo, MODELS.Usuario? usuario) : this()
        {
            conexion = conexionInfo;
            cumplimientoService = new CumplimientoAnexosService(conexionInfo);
            usuarioActual = usuario;

            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            DataGridViewManualCopyHelper.ConfigurarControles(this);
            ConfigurarEstiloVisual();

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            { 
                if (usuarioActual == null || perfilService == null)
                {
                    chkUsarPerfil.Checked = false; 
                    chkUsarPerfil.Enabled = false;

                }


                CargarRazonesSociales();
                dtpInicio.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtpFin.Value = DateTime.Today;
            }
        }

        private void CargarRazonesSociales()
        {
            try
            {
                List<RazonSocial> razones;
                if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                    razones = perfilService.ObtenerRazonesSocialesDePerfil(usuarioActual.IdUsuario);
                else
                    razones = cumplimientoService.ObtenerRazonesSociales();

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

        private void cmbRazon_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRazon.SelectedItem is RazonSocial razon)
            {
                CargarBasesDatosRazon(razon.IdRazon);
            }
            else
            {
                cmbBase.DataSource = null;
            }
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                List<string> bases;
                if (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                    bases = perfilService.ObtenerBasesDatosDePerfilPorRazon(usuarioActual.IdUsuario, idRazon);
                else
                    bases = cumplimientoService.ObtenerBasesDatosRazon(idRazon);

                cmbBase.DataSource = bases
                    .Select(b => new { NombreReal = b, NombreVisible = b.Replace("SEERT_", string.Empty, StringComparison.OrdinalIgnoreCase).Trim('_', '-', ' ') })
                    .Cast<object>()
                    .ToList();
                cmbBase.DisplayMember = "NombreVisible";
                cmbBase.ValueMember = "NombreReal";
                cmbBase.SelectedIndex = -1;
            }
            catch
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
            await GuardarEnPortalAsync();
        }

        private async void btnExportarExcel_Click(object? sender, EventArgs e)
        {
            await ExportarExcelAsync();
        }

        private async Task GenerarPreviewAsync()
        {
            if (cumplimientoService == null)
                return;

            if (cmbRazon.SelectedItem is not RazonSocial razon)
            {
                MessageBox.Show("Seleccione una razón social.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGenerar.Enabled = false;
                dgvPreview.DataSource = null;
                lblResumen.Text = "Generando preview de cumplimiento...";

                string? baseSeleccionada = null;
                if (cmbBase.SelectedItem != null)
                {
                    dynamic item = cmbBase.SelectedItem;
                    baseSeleccionada = item.NombreReal;
                }

                var tabla = await Task.Run(() => cumplimientoService.GenerarPreview(
                    razon.IdRazon,
                    razon.NombreRazon,
                    baseSeleccionada,
                    dtpInicio.Value.Date,
                    dtpFin.Value.Date));

                dgvPreview.DataSource = tabla;
                FormatearGrid();
                ActualizarResumen(tabla);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando cumplimiento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblResumen.Text = "No se pudo generar el preview.";
            }
            finally
            {
                btnGenerar.Enabled = true;
            }
        }

        private async Task GuardarEnPortalAsync()
        {
            if (dgvPreview.DataSource is not DataTable tabla || tabla.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para guardar. Genere el preview primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                btnGuardarPortal.Enabled = false;
                bool guardado = await PortalWebService.GuardarCumplimientoAnexosAsync(tabla);

                MessageBox.Show(
                    guardado ? "Cumplimiento guardado correctamente en portal." : "No se pudo guardar el cumplimiento en portal.",
                    guardado ? "Éxito" : "Error",
                    MessageBoxButtons.OK,
                    guardado ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cumplimiento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardarPortal.Enabled = true;
            }
        }

        private async Task ExportarExcelAsync()
        {
            if (dgvPreview.DataSource is not DataTable tabla || tabla.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var save = new SaveFileDialog();
            save.Filter = "Archivos Excel|*.xlsx";
            save.FileName = $"Cumplimiento_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            if (save.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Cumplimiento");

                    for (int c = 0; c < tabla.Columns.Count; c++)
                    {
                        worksheet.Cell(1, c + 1).Value = tabla.Columns[c].ColumnName;
                    }

                    for (int r = 0; r < tabla.Rows.Count; r++)
                    {
                        for (int c = 0; c < tabla.Columns.Count; c++)
                        {
                            var valor = tabla.Rows[r][c];
                            worksheet.Cell(r + 2, c + 1).Value = valor == DBNull.Value ? string.Empty : valor.ToString();
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(save.FileName);
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
