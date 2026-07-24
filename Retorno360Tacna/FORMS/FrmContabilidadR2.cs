using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System.Drawing.Text;
using System.IO;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmContabilidadR2 : Form
    {


        private const string NombreBucketR2 = "retorno360web";
        private readonly ConexionInfo conexionActual;
        private readonly Usuario? usuarioActual;
        private readonly ContabilidadR2Service contabilidadService;
        private readonly ExcelLayoutModel _excelLayoutModel;
        private bool _actualizandoAnalisisExcel;
        private string _rutaArchivoExcelSeleccionado = string.Empty;



        public FrmContabilidadR2(ConexionInfo conexion, Usuario? usuario = null)
        {
            InitializeComponent();
            conexionActual = conexion;
            usuarioActual = usuario;
            contabilidadService = new ContabilidadR2Service(NombreBucketR2);
            _excelLayoutModel = new ExcelLayoutModel();
            Resize += FrmContabilidadR2_Resize;
        }

        private async void FrmContabilidadR2_Load(object sender, EventArgs e)
        {
            ConfigurarLayout();
            ConfigurarGrid();
            await CargarRazonesSocialesAsync();
            RestablecerAnalisisExcel();
        }

        private void FrmContabilidadR2_Resize(object? sender, EventArgs e)
        {
            ConfigurarLayout();
        }


        private void MostrarPanelCargando(bool mostrar, string? titulo = null, string? detalle = null)
        {
            if (panelCargando.InvokeRequired)
            {
                panelCargando.Invoke(new Action(() => MostrarPanelCargando(mostrar, titulo, detalle)));
                return;
            }

            if (!string.IsNullOrWhiteSpace(titulo))
            {
                lblTituloCargaExcel.Text = titulo;
            }

            if (!string.IsNullOrWhiteSpace(detalle))
            {
                lblCargando.Text = detalle;
            }

            panelCargando.Visible = mostrar;
            if (mostrar)
            {
                panelCargando.Left = (this.ClientSize.Width - panelCargando.Width) / 2;
                panelCargando.Top = (this.ClientSize.Height - panelCargando.Height) / 2;

                if (progressBarCargando.Style != ProgressBarStyle.Marquee)
                {
                    progressBarCargando.Style = ProgressBarStyle.Marquee;
                }

                panelCargando.BringToFront();
            }

            Application.DoEvents();
        }

        private void ConfigurarGrid()
        {
            dgvResultados.AutoGenerateColumns = true;
            dgvResultados.MultiSelect = false;
            dgvResultados.RowHeadersVisible = false;
        }

        private void ConfigurarLayout()
        {
            if (!IsHandleCreated)
                return;

            SuspendLayout();
            panelFiltros.SuspendLayout();

            try
            {
                panelFiltros.Location = new Point(20, 20);
                panelFiltros.Size = new Size(Math.Max(980, ClientSize.Width - 40), 225);

                int xBotones = panelFiltros.ClientSize.Width - 198;
                BtnAnalizarExcel.Location = new Point(xBotones, 72);
                btnProcesar.Location = new Point(xBotones, 140);

                lblDescripcion.Text = "Selecciona la razón social, empresa y año disponibles en R2. Después analiza un layout Excel para cargar hojas y columnas, o captura manualmente la columna a consolidar.";

                BtnAnalizarExcel.Visible = true;
                CboHojas.Visible = true;
                cboColumnas.Visible = true;
                lblHojasExcel.Visible = true;
                lblColumnasDetectadas.Visible = true;
                lblEstadoExcel.Visible = true;
                lblEstadoExcel.Size = new Size(Math.Max(340, xBotones - lblEstadoExcel.Left - 24), 17);

                BtnAnalizarExcel.BringToFront();
                CboHojas.BringToFront();
                cboColumnas.BringToFront();
                lblHojasExcel.BringToFront();
                lblColumnasDetectadas.BringToFront();
                lblEstadoExcel.BringToFront();

                panelResumen.Location = new Point(20, panelFiltros.Bottom + 15);
                panelResumen.Size = new Size(panelFiltros.Width, 55);

                dgvResultados.Location = new Point(20, panelResumen.Bottom + 16);
                dgvResultados.Size = new Size(panelFiltros.Width, Math.Max(220, ClientSize.Height - dgvResultados.Top - 20));
            }
            finally
            {
                panelFiltros.ResumeLayout();
                ResumeLayout();
            }
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

            if (cmbEmpresa.SelectedItem is not R2FolderOption empresa)
                return;

            string columnaSeleccionada = cboColumnas.SelectedItem?.ToString()?.Trim() ?? string.Empty;


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

                ContabilidadProcesoResultado resultado = await contabilidadService.ProcesarArchivosAsync(empresa.Prefix, anio.DisplayName, columnaSeleccionada);
                dgvResultados.DataSource = resultado.Registros;
                contabilidadService.ExportarResultadosExcel(resultado.Registros, saveDialog.FileName);

                lblResumen.Text = $"Analizados: {resultado.ArchivosAnalizados} | Procesados: {resultado.ArchivosProcesados} | Omitidos: {resultado.ArchivosOmitidos} | Meses faltantes: {resultado.MesesFaltantes}";

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
            if (CboHojas.SelectedItem is not string)
            {
                MessageBox.Show(this, "Seleccione una hoja.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cboColumnas.SelectedItem is not string columnaSeleccionada || string.IsNullOrWhiteSpace(columnaSeleccionada))
            {
                MessageBox.Show(this, "Seleccione una columna a analizar.", "Contabilidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            cboColumnas.Enabled = !loading;
            CboHojas.Enabled = !loading;
            BtnAnalizarExcel.Enabled = !loading;
            btnProcesar.Enabled = !loading;

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                lblResumen.Text = mensaje;
            }
        }

        private void RestablecerAnalisisExcel()
        {
            _actualizandoAnalisisExcel = true;

            try
            {
                _rutaArchivoExcelSeleccionado = string.Empty;
                _excelLayoutModel.RutaArchivo = string.Empty;
                _excelLayoutModel.Hojas.Clear();
                _excelLayoutModel.Campos.Clear();

                CboHojas.Items.Clear();
                cboColumnas.Items.Clear();
                CboHojas.SelectedIndex = -1;
                cboColumnas.SelectedIndex = -1;
            }
            finally
            {
                _actualizandoAnalisisExcel = false;
            }

            lblHojasExcel.Text = "Hojas Excel";
            lblColumnasDetectadas.Text = "Columnas detectadas";
            lblEstadoExcel.Text = "Archivo Excel: no seleccionado.";
        }

        private void PrepararNuevoAnalisisExcel(string rutaArchivo)
        {
            _actualizandoAnalisisExcel = true;

            try
            {
                _rutaArchivoExcelSeleccionado = rutaArchivo;
                _excelLayoutModel.Hojas.Clear();
                _excelLayoutModel.Campos.Clear();

                CboHojas.Items.Clear();
                cboColumnas.Items.Clear();
                CboHojas.SelectedIndex = -1;
                cboColumnas.SelectedIndex = -1;
            }
            finally
            {
                _actualizandoAnalisisExcel = false;
            }

            ActualizarResumenAnalisisExcel();
        }

        private void ActualizarResumenAnalisisExcel(string? hojaSeleccionada = null)
        {
            lblHojasExcel.Text = _excelLayoutModel.Hojas.Count > 0
                ? $"Hojas Excel ({_excelLayoutModel.Hojas.Count})"
                : "Hojas Excel";

            lblColumnasDetectadas.Text = _excelLayoutModel.Campos.Count > 0
                ? $"Columnas detectadas ({_excelLayoutModel.Campos.Count})"
                : "Columnas detectadas";

            if (string.IsNullOrWhiteSpace(_rutaArchivoExcelSeleccionado))
            {
                lblEstadoExcel.Text = "Archivo Excel: no seleccionado.";
                return;
            }

            string nombreArchivo = Path.GetFileName(_rutaArchivoExcelSeleccionado);
            lblEstadoExcel.Text = $"Archivo Excel: {nombreArchivo}";

            if (_excelLayoutModel.Hojas.Count == 0)
            {
                lblResumen.Text = $"Archivo seleccionado: {nombreArchivo}. No se detectaron hojas válidas.";
                return;
            }

            if (string.IsNullOrWhiteSpace(hojaSeleccionada))
            {
                lblResumen.Text = $"Archivo: {nombreArchivo} | Hojas detectadas: {_excelLayoutModel.Hojas.Count}. Seleccione una hoja para revisar sus columnas.";
                return;
            }

            lblResumen.Text = $"Archivo: {nombreArchivo} | Hoja: {hojaSeleccionada} | Hojas detectadas: {_excelLayoutModel.Hojas.Count} | Columnas detectadas: {_excelLayoutModel.Campos.Count}";
        }

        private async Task AnalizarHojaAsync(string nombreHoja)
        {
            MostrarPanelCargando(true, "Analizando archivo Excel", $"Leyendo columnas y campos de la hoja \"{nombreHoja}\"...");
            await Task.Delay(50);

            List<string> campos = await Task.Run(() => _excelLayoutModel.AnalizarHoja(nombreHoja));

            _actualizandoAnalisisExcel = true;
            try
            {
                cboColumnas.Items.Clear();

                if (campos.Count > 0)
                {
                    cboColumnas.Items.AddRange(campos.ToArray());
                    cboColumnas.SelectedIndex = 0;
                }
                else
                {
                    cboColumnas.SelectedIndex = -1;
                }
            }
            finally
            {
                _actualizandoAnalisisExcel = false;
            }

            ActualizarResumenAnalisisExcel(nombreHoja);
        }

        private async void BtnAnalizarExcel_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialogo = new OpenFileDialog
            {
                Filter = "Archivos de Excel (*.xlsx)|*.xlsx",
                Title = "Seleccione un archivo de Excel para analizar"
            };

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                SetLoading(true, "Analizando layout Excel...");
                PrepararNuevoAnalisisExcel(dialogo.FileName);
                MostrarPanelCargando(true, "Analizando archivo Excel", "Cargando el archivo seleccionado...");
                await Task.Delay(50);
                await Task.Run(() => _excelLayoutModel.CargarArchivo(dialogo.FileName));

                if (_excelLayoutModel.Hojas.Count == 0)
                {
                    MessageBox.Show("El archivo no contiene hojas válidas.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ActualizarResumenAnalisisExcel();
                    return;
                }

                _actualizandoAnalisisExcel = true;
                try
                {
                    CboHojas.Items.AddRange(_excelLayoutModel.Hojas.ToArray());
                    CboHojas.SelectedIndex = 0;
                }
                finally
                {
                    _actualizandoAnalisisExcel = false;
                }

                await AnalizarHojaAsync(_excelLayoutModel.Hojas[0]);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("Ocurrió un error al analizar el archivo de Excel.", "Análisis de Excel", ex, "Análisis de archivo Excel");
            }
            finally
            {
                MostrarPanelCargando(false);
                SetLoading(false);
            }

        }

        private async void CboHojas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_actualizandoAnalisisExcel)
                return;

            if (CboHojas.SelectedItem is not string nombreHoja)
                return;

            try
            {
                SetLoading(true, $"Analizando la hoja {nombreHoja}...");
                await AnalizarHojaAsync(nombreHoja);

                if (cboColumnas.Items.Count == 0)
                {
                    MessageBox.Show("La hoja seleccionada no contiene datos válidos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("Ocurrió un error al analizar la hoja de Excel.", "Análisis de Excel", ex, "Análisis de hoja Excel");
            }
            finally
            {
                MostrarPanelCargando(false);
                SetLoading(false);
            }
        }

    }
}
