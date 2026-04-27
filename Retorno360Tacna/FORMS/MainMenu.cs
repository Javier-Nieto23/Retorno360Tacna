using Retorno360Tacna.MODELS;
using Retorno360Tacna.CNX;

namespace Retorno360Tacna.FORMS
{
    public partial class MainMenu : Form
    {
        private Usuario? usuarioActual;
        private ConexionInfo? conexionActual;
        private Button? botonActivo;

        public MainMenu()
        {
            InitializeComponent();
        }

        public MainMenu(Usuario usuario, ConexionInfo conexion)
        {
            InitializeComponent();
            usuarioActual = usuario;
            conexionActual = conexion;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            if (usuarioActual != null)
            {
                lblUsuario.Text = $"Usuario: {usuarioActual.NombreCompleto}";
            }
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn != botonActivo)
            {
                btn.BackColor = Color.FromArgb(52, 73, 94);
            }
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn != botonActivo)
            {
                btn.BackColor = Color.FromArgb(44, 62, 80);
            }
        }

        private void ActivarBoton(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = Color.FromArgb(44, 62, 80);
            }

            botonActivo = boton;
            boton.BackColor = Color.FromArgb(193, 39, 45);
        }

        private void LimpiarPanel()
        {
            panelContenido.Controls.Clear();
        }

        private void btnSeleccionRazon_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnSeleccionRazon);
            lblTitulo.Text = "Selección de Razón Social";
            LimpiarPanel();

            // Aquí cargarás el control FrmSeleccionRazon
            // UserControl o Panel personalizado
        }

        private void btnRetorno_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnRetorno);
            lblTitulo.Text = "Gestión de Retorno";
            LimpiarPanel();

            if (conexionActual != null)
            {
                FrmRetorno frmRetorno = new FrmRetorno(conexionActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmRetorno);
                frmRetorno.Show();
            }
            else
            {
                MessageBox.Show("No hay información de conexión disponible.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnReportes);
            lblTitulo.Text = "Reportes";
            LimpiarPanel();

            // Aquí cargarás el control FrmReportes
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
