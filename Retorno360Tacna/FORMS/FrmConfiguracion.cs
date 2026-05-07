using System;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmConfiguracion : Form
    {
        private ConfiguracionUsuario configuracion;
        private decimal escalaOriginal;

        public FrmConfiguracion(ConfiguracionUsuario config)
        {
            InitializeComponent();
            configuracion = config ?? new ConfiguracionUsuario();
            escalaOriginal = configuracion.EscalaUI;
        }

        private void FrmConfiguracion_Load(object sender, EventArgs e)
        {
            CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {
            // Cargar escala de UI
            decimal escalaActual = configuracion.EscalaUI;
            if (escalaActual == 1.0m)
                cmbEscalaUI.SelectedIndex = 0; // 100%
            else if (escalaActual == 1.25m)
                cmbEscalaUI.SelectedIndex = 1; // 125%
            else if (escalaActual == 1.5m)
                cmbEscalaUI.SelectedIndex = 2; // 150%
            else if (escalaActual == 1.75m)
                cmbEscalaUI.SelectedIndex = 3; // 175%
            else if (escalaActual == 2.0m)
                cmbEscalaUI.SelectedIndex = 4; // 200%
            else
                cmbEscalaUI.SelectedIndex = 0; // Default 100%

            lblEscalaActual.Text = $"Escala actual: {(configuracion.EscalaUI * 100):0}%";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Guardar escala seleccionada
                decimal nuevaEscala = ObtenerEscalaSeleccionada();
                configuracion.EscalaUI = nuevaEscala;

                // Verificar si cambió la escala
                if (nuevaEscala != escalaOriginal)
                {
                    var result = MessageBox.Show(
                        "Se ha cambiado la escala de la interfaz.\n\n" +
                        "La aplicación debe reiniciarse para aplicar los cambios.\n\n" +
                        "¿Desea reiniciar ahora?",
                        "Reinicio requerido",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Guardar en archivo
                        GuardarConfiguracionEnArchivo();

                        MessageBox.Show(
                            "La configuración se guardó correctamente.\n\n" +
                            "La aplicación se cerrará. Por favor, vuelva a abrirla.",
                            "Configuración guardada",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        Application.Exit();
                        return;
                    }
                }
                else
                {
                    GuardarConfiguracionEnArchivo();
                    MessageBox.Show(
                        "Configuración guardada correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar la configuración: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private decimal ObtenerEscalaSeleccionada()
        {
            return cmbEscalaUI.SelectedIndex switch
            {
                0 => 1.0m,   // 100%
                1 => 1.25m,  // 125%
                2 => 1.5m,   // 150%
                3 => 1.75m,  // 175%
                4 => 2.0m,   // 200%
                _ => 1.0m
            };
        }

        private void GuardarConfiguracionEnArchivo()
        {
            try
            {
                string rutaConfig = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Retorno360Tacna",
                    "config.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(rutaConfig)!);

                string[] lineas = new[]
                {
                    $"EscalaUI={configuracion.EscalaUI}"
                };

                File.WriteAllLines(rutaConfig, lineas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar archivo de configuración: {ex.Message}", ex);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cmbEscalaUI_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal escala = ObtenerEscalaSeleccionada();
            lblVistaPrevia.Text = $"Vista previa: {(escala * 100):0}%";
        }

        private void btnVistaPrevia_Click(object sender, EventArgs e)
        {
            decimal escala = ObtenerEscalaSeleccionada();
            MessageBox.Show(
                $"Con la escala al {(escala * 100):0}%, todos los controles, fuentes e imágenes\n" +
                $"se ajustarán proporcionalmente.\n\n" +
                $"• Botones y controles: {(escala * 100):0}% del tamaño original\n" +
                $"• Fuentes: {(escala * 100):0}% del tamaño original\n" +
                $"• Imágenes e iconos: {(escala * 100):0}% del tamaño original\n\n" +
                $"Nota: Para ver el cambio real, debe guardar y reiniciar la aplicación.",
                "Vista previa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
