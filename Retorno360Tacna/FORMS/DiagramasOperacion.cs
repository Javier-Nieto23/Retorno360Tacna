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
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}