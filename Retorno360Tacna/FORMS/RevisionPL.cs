using System.Data;
using ClosedXML.Excel;
using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.FORMS
{
    public partial class RevisionPL : Form
    {
        #region Conexion limpia

        // ---------------------------------------------------------------
        // Tipo auxiliar para mostrar nombre "limpio" de la base de datos
        // ---------------------------------------------------------------
        private sealed class BaseDatosComboItem
        {
            public string NombreReal { get; set; } = string.Empty;
            public string NombreVisible { get; set; } = string.Empty;
        }

        #endregion  

        #region Atributos 
        // ---------------------------------------------------------------
        // Estado / servicios
        // ---------------------------------------------------------------
        private ConexionInfo? conexionInfo;
        private RetornoService? retornoService;   // reutilizado solo para cargar combos
        private PartesService? partesService;     // query de CA_PARTE para el Excel

        private MODELS.Usuario? usuarioActual;
        private SERVICES.PerfilUsuarioService? perfilService;

        // Vista previa del PL (solo en memoria, nunca se persiste)
        private Image? imagenPLSeleccionada;
        private string? nombreArchivoPLSeleccionado;
        private bool esArchivoPdf;

        #endregion

        #region Constructores

        public RevisionPL() : this(new ConexionInfo(), null) { }

        public RevisionPL(ConexionInfo conexion) : this(conexion, null) { }




        public RevisionPL(ConexionInfo conexion, MODELS.Usuario? usuario)
        {
            InitializeComponent();

            conexionInfo = conexion;
            retornoService = new RetornoService(conexion);
            partesService = new PartesService(conexion);
            usuarioActual = usuario;

            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            // Enlazar eventos sobre los controles ya existentes del diseñador
            this.Load += RevisionPL_Load;
            cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;
            chkUsarPerfil.CheckedChanged += checkBox1_CheckedChanged;
            button3.Click += button3_SubirPL_Click;
            button2.Click += button2_Confirmar_Click;
            button1.Click += button1_Cancelar_Click;
            panel1.Paint += panel1_Paint;
        }

        #endregion

        #region Carga inicial
        // =================================================================
        // Carga inicial
        // =================================================================
        private void RevisionPL_Load(object? sender, EventArgs e)
        {
            CargarRazonesSociales();
        }

        #endregion

        #region Selección de razón social y empresa
        // =================================================================
        // Selección de razón social (cmbRazonSocial) / empresa (cmbCliente)
        // =================================================================
        private void CargarRazonesSociales()
        {
            try
            {
                if (retornoService == null)
                {
                    ErrorMessageHelper.ShowError("El servicio no está disponible.",
                        "Error", contexto: "Carga de razones sociales en revisión de PL");
                    return;
                }

                List<RazonSocial> razones = (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                    ? perfilService.ObtenerRazonesSocialesDePerfil(usuarioActual.IdUsuario)
                    : retornoService.ObtenerRazonesSociales();

                cmbRazonSocial.DataSource = razones;
                cmbRazonSocial.DisplayMember = "NombreRazon";
                cmbRazonSocial.ValueMember = "IdRazon";
                cmbRazonSocial.SelectedIndex = -1;

                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar razones sociales: {ex.Message}",
                    "Error", ex, "Carga de razones sociales en revisión de PL");
            }
        }

        private void cmbRazonSocial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            button2.Enabled = false;

            if (cmbRazonSocial.SelectedIndex == -1)
            {
                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
                return;
            }

            if (cmbRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                CargarBasesDatosRazon(razonSeleccionada.IdRazon);
            }
        }

        private void CargarBasesDatosRazon(int idRazon)
        {
            try
            {
                if (retornoService == null)
                    return;

                List<string> basesDatos = (chkUsarPerfil.Checked && usuarioActual != null && perfilService != null)
                    ? perfilService.ObtenerBasesDatosDePerfilPorRazon(usuarioActual.IdUsuario, idRazon)
                    : retornoService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cmbCliente.DataSource = basesDatos
                        .Select(bd => new BaseDatosComboItem
                        {
                            NombreReal = bd,
                            NombreVisible = bd.Replace("SEERT_", string.Empty, StringComparison.OrdinalIgnoreCase)
                                              .Trim(' ', '_', '-')
                        })
                        .ToList();
                    cmbCliente.DisplayMember = nameof(BaseDatosComboItem.NombreVisible);
                    cmbCliente.ValueMember = nameof(BaseDatosComboItem.NombreReal);
                    cmbCliente.Enabled = true;
                    cmbCliente.SelectedIndex = -1;
                }
                else
                {
                    cmbCliente.DataSource = null;
                    cmbCliente.Enabled = false;
                    MessageBox.Show("No se encontraron bases de datos asociadas a esta razón social.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                cmbCliente.SelectedIndexChanged -= cmbCliente_SelectedIndexChanged;
                cmbCliente.SelectedIndexChanged += cmbCliente_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar bases de datos: {ex.Message}",
                    "Error", ex, "Carga de bases de datos en revisión de PL");
                cmbCliente.DataSource = null;
                cmbCliente.Enabled = false;
            }
        }

        private void cmbCliente_SelectedIndexChanged(object? sender, EventArgs e)
        {
            button2.Enabled = cmbCliente.SelectedIndex != -1;
        }

        private void checkBox1_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkUsarPerfil.Checked && usuarioActual == null)
            {
                MessageBox.Show("No hay usuario activo para usar el perfil de empresas.",
                    "Perfil no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkUsarPerfil.Checked = false;
                return;
            }

            button2.Enabled = false;
            CargarRazonesSociales();
        }

        #endregion

        #region SubirPL: selección y vista previa de imagen/PDF
        // =================================================================
        // SubirPL: selecciona imagen/PDF y lo muestra en panel1 (solo vista previa)
        // =================================================================
        private void button3_SubirPL_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialogo = new()
            {
                Title = "Seleccionar Packing List",
                Filter = "Imágenes y PDF (*.jpg;*.jpeg;*.png;*.bmp;*.pdf)|*.jpg;*.jpeg;*.png;*.bmp;*.pdf"
            };

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                imagenPLSeleccionada?.Dispose();
                imagenPLSeleccionada = null;
                nombreArchivoPLSeleccionado = Path.GetFileName(dialogo.FileName);
                esArchivoPdf = Path.GetExtension(dialogo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

                if (!esArchivoPdf)
                {
                    // Se carga en memoria y se libera el handle del archivo original
                    using var flujo = new MemoryStream(File.ReadAllBytes(dialogo.FileName));
                    imagenPLSeleccionada = Image.FromStream(flujo);
                }

                panel1.Invalidate();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar el archivo: {ex.Message}",
                    "Error", ex, "Carga de vista previa de PL");
            }
        }

        private void panel1_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(panel1.BackColor);

            if (imagenPLSeleccionada != null)
            {
                Size tamanoDestino = CalcularTamanoEscalado(imagenPLSeleccionada.Size, panel1.ClientSize);
                Point origen = new(
                    (panel1.ClientSize.Width - tamanoDestino.Width) / 2,
                    (panel1.ClientSize.Height - tamanoDestino.Height) / 2);

                e.Graphics.DrawImage(imagenPLSeleccionada, new Rectangle(origen, tamanoDestino));
            }
            else if (esArchivoPdf && !string.IsNullOrEmpty(nombreArchivoPLSeleccionado))
            {
                // Vista previa de PDF pendiente: se requiere una librería de
                // renderizado (p. ej. PdfiumViewer) para mostrar la primera
                // página como imagen. Por ahora solo se indica el nombre.
                TextRenderer.DrawText(
                    e.Graphics,
                    $"Archivo PDF seleccionado:\n{nombreArchivoPLSeleccionado}\n(vista previa no disponible)",
                    panel1.Font,
                    panel1.ClientRectangle,
                    Color.DimGray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }

        private static Size CalcularTamanoEscalado(Size original, Size contenedor)
        {
            if (original.Width == 0 || original.Height == 0)
                return contenedor;

            float escala = Math.Min(
                (float)contenedor.Width / original.Width,
                (float)contenedor.Height / original.Height);
            escala = Math.Min(escala, 1f); // no ampliar más allá del tamaño original

            return new Size((int)(original.Width * escala), (int)(original.Height * escala));
        }

        #endregion

        #region Confirmar: generación del Excel
        // =================================================================
        // Confirmar: ejecuta el query de CA_PARTE y genera el Excel
        // =================================================================
        private async void button2_Confirmar_Click(object? sender, EventArgs e)
        {
            if (cmbRazonSocial.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor seleccione una razón social.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCliente.SelectedValue is not string baseDatosSeleccionada || string.IsNullOrWhiteSpace(baseDatosSeleccionada))
            {
                MessageBox.Show("Por favor seleccione una empresa.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await GenerarExcelPartesAsync(baseDatosSeleccionada);
        }

        private async Task GenerarExcelPartesAsync(string baseDatos)
        {
            try
            {
                if (partesService == null)
                {
                    ErrorMessageHelper.ShowError("El servicio de consulta no está disponible.",
                        "Error", contexto: "Generación de Excel en revisión de PL");
                    return;
                }

                using SaveFileDialog saveDialog = new()
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar Reporte de Partes",
                    FileName = $"RevisionPL_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                button2.Enabled = false;
                button1.Enabled = false;
                Cursor = Cursors.WaitCursor;

                DataTable tablaPartes = await Task.Run(() => partesService.ObtenerPartes(baseDatos));

                GenerarArchivoExcel(tablaPartes, saveDialog.FileName);

                Cursor = Cursors.Default;

                DialogResult respuesta = MessageBox.Show(
                    "Excel generado exitosamente.\n\n¿Desea abrir el archivo?",
                    "Éxito", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (respuesta == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }

                // Se asume que, tras generar el reporte, el formulario se cierra.
                // Quita estas dos líneas si prefieres dejarlo abierto.
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al generar el Excel: {ex.Message}",
                    "Error", ex, "Generación de Excel en revisión de PL");
            }
            finally
            {
                Cursor = Cursors.Default;
                if (!this.IsDisposed)
                {
                    button2.Enabled = true;
                    button1.Enabled = true;
                }
            }
        }

        private static void GenerarArchivoExcel(DataTable tabla, string rutaDestino)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add(tabla, "Partes");
            hoja.Columns().AdjustToContents();
            libro.SaveAs(rutaDestino);
        }

        #endregion

        #region Cancelar
        // =================================================================
        // Cancelar
        // =================================================================
        private void button1_Cancelar_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Ciclo de vida del formulario

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            imagenPLSeleccionada?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion
    }
}