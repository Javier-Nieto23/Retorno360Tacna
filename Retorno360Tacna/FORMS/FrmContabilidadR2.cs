using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmContabilidadR2 : Form
    {
        private const string NombreBucketR2 = "retorno360tacnaweb";
        private readonly ConexionInfo conexionActual;
        private readonly Usuario? usuarioActual;
        private readonly ContabilidadR2Service contabilidadService;

        public FrmContabilidadR2(ConexionInfo conexion, Usuario? usuario = null)
        {
            InitializeComponent();
            conexionActual = conexion;
            usuarioActual = usuario;
            contabilidadService = new ContabilidadR2Service(NombreBucketR2);
        }

        private async void FrmContabilidadR2_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            await CargarRazonesSocialesAsync();
        }

        private void ConfigurarGrid()
        {
            dgvResultados.AutoGenerateColumns = true;
            dgvResultados.MultiSelect = false;
            dgvResultados.RowHeadersVisible = false;
        }

        private async Task CargarRazonesSocialesAsync()
        {
            try
            {
                SetLoading(true, "Cargando razones sociales desde R2...");
                List<R2FolderOption> razones = await contabilidadService.ObtenerRazonesSocialesAsync();
                cmbRazonSocial.DataSource = razones;
                cmbRazonSocial.DisplayMember = nameof(R2FolderOption.DisplayName);
                cmbRazonSocial.ValueMember = nameof(R2FolderOption.Prefix);
                lblResumen.Text = razones.Count > 0
                    ? "Seleccione los filtros para generar el consolidado."
                    : "No se encontraron carpetas de razón social en R2.";
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("No fue posible cargar las razones sociales desde R2.",
                    "Contabilidad", ex, "Carga de razones sociales de contabilidad desde R2");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void cmbRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedItem is not R2FolderOption razon)
                return;

            try
            {
                SetLoading(true, "Cargando empresas...");
                cmbEmpresa.DataSource = null;
                cmbAnio.DataSource = null;
                List<R2FolderOption> empresas = await contabilidadService.ObtenerEmpresasAsync(razon.Prefix);
                cmbEmpresa.DataSource = empresas;
                cmbEmpresa.DisplayMember = nameof(R2FolderOption.DisplayName);
                cmbEmpresa.ValueMember = nameof(R2FolderOption.Prefix);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("No fue posible cargar las empresas disponibles.",
                    "Contabilidad", ex, "Carga de empresas de contabilidad desde R2");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void cmbEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbEmpresa.SelectedItem is not R2FolderOption empresa)
                return;

            try
            {
                SetLoading(true, "Cargando años...");
                cmbAnio.DataSource = null;
                List<R2FolderOption> anios = await contabilidadService.ObtenerAniosAsync(empresa.Prefix);
                cmbAnio.DataSource = anios;
                cmbAnio.DisplayMember = nameof(R2FolderOption.DisplayName);
                cmbAnio.ValueMember = nameof(R2FolderOption.Prefix);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("No fue posible cargar los años disponibles.",
                    "Contabilidad", ex, "Carga de años de contabilidad desde R2");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void btnProcesar_Click(object sender, EventArgs e)
        {
            if (!ValidarEntrada())
                return;

            if (cmbAnio.SelectedItem is not R2FolderOption anio)
                return;

            string columna = txtColumna.Text.Trim();

            try
            {
                using SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel (*.xlsx)|*.xlsx",
                    FileName = $"Contabilidad_{anio.DisplayName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Guardar consolidado de contabilidad"
                };

                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                SetLoading(true, "Procesando archivos Excel desde R2...");
                if (cmbEmpresa.SelectedItem is not R2FolderOption empresa)
                    return;

                ContabilidadProcesoResultado resultado = await contabilidadService.ProcesarArchivosAsync(empresa.Prefix, anio.DisplayName, columna);
                dgvResultados.DataSource = resultado.Registros;
                contabilidadService.ExportarResultadosExcel(resultado.Registros, saveDialog.FileName);

                lblResumen.Text = $"Analizados: {resultado.ArchivosAnalizados} | Procesados: {resultado.ArchivosProcesados} | Omitidos: {resultado.ArchivosOmitidos}";

                MessageBox.Show(this,
                    $"Consolidado generado correctamente en:{Environment.NewLine}{saveDialog.FileName}",
                    "Contabilidad",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("Ocurrió un error al generar el consolidado de contabilidad.",
                    "Contabilidad", ex, "Procesamiento de contabilidad desde R2");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private bool ValidarEntrada()
        {
            if (cmbRazonSocial.SelectedItem is not R2FolderOption)
            {
                MessageBox.Show(this, "Seleccione una razón social.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbEmpresa.SelectedItem is not R2FolderOption)
            {
                MessageBox.Show(this, "Seleccione una empresa.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbAnio.SelectedItem is not R2FolderOption)
            {
                MessageBox.Show(this, "Seleccione un año.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtColumna.Text))
            {
                MessageBox.Show(this, "Ingrese el nombre exacto de la columna a analizar.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtColumna.Focus();
                return false;
            }

            return true;
        }

        private void SetLoading(bool loading, string? mensaje = null)
        {
            UseWaitCursor = loading;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
            cmbRazonSocial.Enabled = !loading;
            cmbEmpresa.Enabled = !loading;
            cmbAnio.Enabled = !loading;
            txtColumna.Enabled = !loading;
            btnProcesar.Enabled = !loading;

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                lblResumen.Text = mensaje;
            }
        }
    }
}
