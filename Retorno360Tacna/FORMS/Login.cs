using Retorno360Tacna.CNX;
using Retorno360Tacna.SERVICES;
using Retorno360Tacna.MODELS;
using System.Runtime.InteropServices;

namespace Retorno360Tacna.FORMS
{
    public partial class Login : Form
    {
        private List<ConexionInfo> conexiones;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public Login()
        {
            InitializeComponent();
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            CargarConexiones();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HTCAPTION = 0x2;

            if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
            {
                return;
            }

            base.WndProc(ref m);
        }

        private void CargarConexiones()
        {
            try
            {
                LoginService loginService = new LoginService();
                conexiones = loginService.ObtenerConexiones();

                comboBox1.DataSource = conexiones;
                comboBox1.DisplayMember = "NombreConexion";
                comboBox1.ValueMember = "IdConexion";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar conexiones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un servidor.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Por favor, ingrese usuario y contraseña.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ConexionInfo conexionSeleccionada = (ConexionInfo)comboBox1.SelectedItem;

                // Validar usuario SIEMPRE en el servidor base (172.20.20.26 -> RetornoMaster)
                LoginService loginService = new LoginService();
                Usuario? usuario = loginService.ValidarUsuario(textBox1.Text, textBox2.Text);

                if (usuario != null)
                {
                    // Probar conexión al servidor seleccionado para trabajar con datos
                    Conexion conexionTrabajo = new Conexion(
                        conexionSeleccionada.Servidor!,
                        conexionSeleccionada.UsuarioSQL!,
                        conexionSeleccionada.PasswordSQL!
                    );

                    if (!conexionTrabajo.ProbarConexion())
                    {
                        MessageBox.Show("Usuario válido, pero no se pudo conectar al servidor seleccionado para trabajar.", "Advertencia de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Mostrar pantalla de carga antes de abrir MainMenu
                    this.Hide();
                    CargaDePantalla cargaDePantalla = new CargaDePantalla(usuario, conexionSeleccionada);

                    if (cargaDePantalla.ShowDialog() == DialogResult.OK)
                    {
                        // Obtener los datos del formulario de carga
                        Usuario? usuarioRecuperado = cargaDePantalla.ObtenerUsuario();
                        ConexionInfo? conexionRecuperada = cargaDePantalla.ObtenerConexion();

                        if (usuarioRecuperado != null && conexionRecuperada != null)
                        {
                            // Abrir MainMenu
                            Retorno360Tacna.FORMS.MainMenu mainMenu = new Retorno360Tacna.FORMS.MainMenu(usuarioRecuperado, conexionRecuperada);
                            mainMenu.FormClosed += (s, args) => this.Close();
                            mainMenu.Show();
                        }
                    }
                    else
                    {
                        this.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
