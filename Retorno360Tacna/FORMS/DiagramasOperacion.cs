using System;
using System.Drawing;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Pantalla de bienvenida del sistema
    /// Se muestra al iniciar la aplicación
    /// </summary>
    public partial class DiagramasOperacion : Form
    {
        private Usuario? usuarioActual;

        public DiagramasOperacion()
        {
            InitializeComponent();
        }

        public DiagramasOperacion(ConexionInfo conexion, Usuario? usuario = null)
        {
            InitializeComponent();
            usuarioActual = usuario;
        }

        private void DiagramasOperacion_Load(object sender, EventArgs e)
        {
            // Personalizar mensaje de bienvenida si hay usuario
            if (usuarioActual != null)
            {
                lblBienvenida.Text = $"¡Bienvenido, {usuarioActual.NombreCompleto}!";
            }
            else
            {
                lblBienvenida.Text = "¡Bienvenido al Sistema!";
            }

            // Intentar cargar logo si existe
            try
            {
                if (Properties.Resources.ChatGPT_Image_Apr_21__2026__12_48_04_PM != null)
                {
                    pictureBoxLogo.Image = Properties.Resources.ChatGPT_Image_Apr_21__2026__12_48_04_PM;
                }
            }
            catch
            {
                // Si no hay logo, ocultar el PictureBox
                pictureBoxLogo.Visible = false;
                lblBienvenida.Location = new Point(0, 200);
            }
        }
    }
}
