using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmPlantillaInventario : Form
    {
        private readonly ExcelLayoutModel _layout = new();
        private PlantillaInventarioConfig _config = new();

        public event EventHandler? RegresarSolicitado;

        public FrmPlantillaInventario()
        {
            InitializeComponent();
            Load += FrmPlantillaInventario_Load;
        }

        // â”€â”€ Carga inicial â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void FrmPlantillaInventario_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            cmbRazonSocial.SelectedIndexChanged += CmbRazonSocial_SelectedIndexChanged;

            if (cmbRazonSocial.SelectedValue != null &&
                int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
                CargarEmpresas(idRazon);
        }

        // â”€â”€ Cascading RazÃ³n Social â†’ Empresa â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void CargarRazonesSociales()
        {
            try
            {
                using SqlConnection conn = new SqlConnection(new Conexion().GetConnectionString());
                using SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT IdRazon, Nombre_Razon FROM RAZONXTABLA ORDER BY Nombre_Razon", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbRazonSocial.DataSource    = null;
                cmbRazonSocial.DisplayMember = "Nombre_Razon";
                cmbRazonSocial.ValueMember   = "IdRazon";
                cmbRazonSocial.DataSource    = dt;
                if (cmbRazonSocial.Items.Count > 0) cmbRazonSocial.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarEmpresas(int idRazon)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(new Conexion().GetConnectionString());
                using SqlCommand cmd = new SqlCommand(
                    "SELECT IdTabla, NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA",
                    conn);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                using SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbEmpresa.DataSource    = null;
                cmbEmpresa.DisplayMember = "NOMBRE_TABLA";
                cmbEmpresa.ValueMember   = "IdTabla";
                cmbEmpresa.DataSource    = dt;
                if (cmbEmpresa.Items.Count > 0)
                {
                    cmbEmpresa.SelectedIndex = 0;
                    CargarConfigEmpresaSeleccionada();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar empresas: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbRazonSocial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedValue != null &&
                int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
                CargarEmpresas(idRazon);
        }

        private void CmbEmpresa_SelectedIndexChanged(object? sender, EventArgs e)
        {
            CargarConfigEmpresaSeleccionada();
        }

        private void CargarConfigEmpresaSeleccionada()
        {
            if (cmbEmpresa.SelectedValue == null ||
                !int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa)) return;

            var cfg = PlantillaInventarioServicio.ObtenerParaEmpresa(idEmpresa);
            _config = cfg ?? new PlantillaInventarioConfig { IdEmpresa = idEmpresa };

            // Actualizar UI con config guardada
            if (_config.EstaConfigurada)
            {
                try
                {
                    lblArchivoSeleccionado.Text      = Path.GetFileName(_config.RutaArchivo);
                    lblArchivoSeleccionado.ForeColor  = Color.FromArgb(22, 163, 74);
                    _layout.CargarArchivo(_config.RutaArchivo);

                    cmbHoja.DataSource = null;
                    cmbHoja.DataSource = _layout.Hojas;
                    int idx = _layout.Hojas.IndexOf(_config.Hoja);
                    cmbHoja.SelectedIndex = idx >= 0 ? idx : 0;
                    // CargarCampos se dispara desde SelectedIndexChanged
                    return;
                }
                catch
                {
                    lblArchivoSeleccionado.Text      = "Archivo no encontrado";
                    lblArchivoSeleccionado.ForeColor  = Color.Red;
                }
            }

            // Sin config previa: resetear controles
            lblArchivoSeleccionado.Text      = "NingÃºn archivo seleccionado";
            lblArchivoSeleccionado.ForeColor  = Color.Gray;
            cmbHoja.DataSource     = null;
            cmbCampoTotal.DataSource = null;
            cmbCampoA.DataSource   = null;
            cmbCampoB.DataSource   = null;
        }

        // â”€â”€ Carga de campos del Excel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void CargarCampos()
        {
            if (cmbHoja.SelectedItem is not string hoja || string.IsNullOrWhiteSpace(hoja)) return;

            try
            {
                var campos = _layout.AnalizarHoja(hoja);

                cmbCampoTotal.DataSource = null;
                cmbCampoA.DataSource     = null;
                cmbCampoB.DataSource     = null;

                cmbCampoTotal.DataSource = new System.Collections.Generic.List<string>(campos);
                cmbCampoA.DataSource     = new System.Collections.Generic.List<string>(campos);
                cmbCampoB.DataSource     = new System.Collections.Generic.List<string>(campos);

                SeleccionarSiExiste(cmbCampoTotal, _config.CampoTotal);
                SeleccionarSiExiste(cmbCampoA,     _config.CampoA);
                SeleccionarSiExiste(cmbCampoB,     _config.CampoB);

                cmbOperacion.SelectedIndex = _config.Operacion == "MultiplicarColumnas" ? 0 : 1;
                ActualizarVisibilidadCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer los campos del Excel: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SeleccionarSiExiste(ComboBox cmb, string valor)
        {
            int idx = (cmb.DataSource as System.Collections.Generic.List<string>)?.IndexOf(valor) ?? -1;
            if (idx >= 0) cmb.SelectedIndex = idx;
        }

        // â”€â”€ Eventos de controles â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void BtnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Excel|*.xlsx;*.xls", Title = "Seleccionar plantilla Excel" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                _layout.CargarArchivo(ofd.FileName);
                _config.RutaArchivo              = ofd.FileName;
                lblArchivoSeleccionado.Text       = Path.GetFileName(ofd.FileName);
                lblArchivoSeleccionado.ForeColor   = Color.FromArgb(22, 163, 74);

                cmbHoja.DataSource = null;
                cmbHoja.DataSource = _layout.Hojas;
                if (cmbHoja.Items.Count > 0) cmbHoja.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbHoja_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_config.RutaArchivo))
                CargarCampos();
        }

        private void CmbOperacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarVisibilidadCampos();
        }

        private void ActualizarVisibilidadCampos()
        {
            bool esMultiplicar     = cmbOperacion.SelectedIndex == 0;
            lblCampoTotal.Visible  = !esMultiplicar;
            cmbCampoTotal.Visible  = !esMultiplicar;
            lblCampoA.Visible      = esMultiplicar;
            cmbCampoA.Visible      = esMultiplicar;
            lblCampoB.Visible      = esMultiplicar;
            cmbCampoB.Visible      = esMultiplicar;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool esMultiplicar = cmbOperacion.SelectedIndex == 0;

            // Recuperar empresa/razÃ³n seleccionadas
            if (cmbEmpresa.SelectedItem is not DataRowView rowEmpresa ||
                cmbRazonSocial.SelectedItem is not DataRowView rowRazon)
            {
                MessageBox.Show("Selecciona una razÃ³n social y empresa.", "ValidaciÃ³n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _config.IdEmpresa     = Convert.ToInt32(rowEmpresa["IdTabla"]);
            _config.NombreEmpresa = rowEmpresa["NOMBRE_TABLA"]?.ToString() ?? string.Empty;
            _config.IdRazon       = Convert.ToInt32(rowRazon["IdRazon"]);
            _config.NombreRazon   = rowRazon["Nombre_Razon"]?.ToString() ?? string.Empty;
            _config.Hoja          = cmbHoja.SelectedItem?.ToString() ?? string.Empty;
            _config.Operacion     = esMultiplicar ? "MultiplicarColumnas" : "SumarColumna";
            _config.CampoTotal    = esMultiplicar ? string.Empty : (cmbCampoTotal.SelectedItem?.ToString() ?? string.Empty);
            _config.CampoA        = esMultiplicar ? (cmbCampoA.SelectedItem?.ToString() ?? string.Empty) : string.Empty;
            _config.CampoB        = esMultiplicar ? (cmbCampoB.SelectedItem?.ToString() ?? string.Empty) : string.Empty;

            try
            {
                PlantillaInventarioServicio.Guardar(_config);
                MessageBox.Show(
                    $"Plantilla guardada para {_config.NombreEmpresa}.",
                    "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(_config.RutaArchivo) || !File.Exists(_config.RutaArchivo))
            {
                MessageBox.Show("Selecciona un archivo Excel de plantilla.", "ValidaciÃ³n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbEmpresa.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una empresa.", "ValidaciÃ³n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbHoja.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una hoja del archivo.", "ValidaciÃ³n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnRegresar_Click(object sender, EventArgs e)
        {
            RegresarSolicitado?.Invoke(this, EventArgs.Empty);
        }
    }
}
