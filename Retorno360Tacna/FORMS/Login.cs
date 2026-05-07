using Retorno360Tacna.CNX;
using Retorno360Tacna.SERVICES;
using Retorno360Tacna.MODELS;
using System.Runtime.InteropServices;

namespace Retorno360Tacna.FORMS
{
    public partial class Login : Form
    {
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
                // SIMPLIFICADO: Solo cargar la conexión principal
                // Los servidores secundarios se configuran automáticamente en RetornoService
                LoginService loginService = new LoginService();
                List<ConexionInfo> conexiones = loginService.ObtenerConexiones();

                // Filtrar solo la conexión principal (172.20.20.26)
                var conexionPrincipal = conexiones.FirstOrDefault(c => 
                    c.Servidor != null && c.Servidor.Equals("172.20.20.26", StringComparison.OrdinalIgnoreCase));

                if (conexionPrincipal == null && conexiones.Count > 0)
                {
                    // Si no encuentra 172.20.20.26, usar la primera
                    conexionPrincipal = conexiones[0];
                }

                if (conexionPrincipal != null)
                {
                    comboBox1.DataSource = new List<ConexionInfo> { conexionPrincipal };
                    comboBox1.DisplayMember = "NombreConexion";
                    comboBox1.ValueMember = "IdConexion";
                    comboBox1.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show(
                        "No se encontró la conexión al servidor principal.\n" +
                        "Por favor, verifica la tabla Conexiones en RetornoMaster.",
                        "Error de Configuración",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
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
                    MessageBox.Show("Error: No hay conexión disponible.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Por favor, ingrese usuario y contraseña.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ConexionInfo conexionPrincipal = (ConexionInfo)comboBox1.SelectedItem;

                // ✅ Validar usuario contra RetornoMaster (servidor principal)
                LoginService loginService = new LoginService();
                Usuario? usuario = loginService.ValidarUsuario(textBox1.Text, textBox2.Text);

                if (usuario != null)
                {
                    // ✅ Probar conexión al servidor principal
                    Conexion conexionPrueba = new Conexion(
                        conexionPrincipal.Servidor!,
                        conexionPrincipal.UsuarioSQL!,
                        conexionPrincipal.PasswordSQL!,
                        "RetornoMaster"
                    );

                    if (!conexionPrueba.ProbarConexion())
                    {
                        MessageBox.Show(
                            "Usuario válido, pero no se pudo conectar al servidor principal.\n\n" +
                            $"Servidor: {conexionPrincipal.Servidor}\n" +
                            "Por favor, verifica la conexión de red.",
                            "Error de Conexión",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    // ℹ️ NOTA: Los servidores externos se configuran automáticamente
                    //    en RetornoService leyendo RAZONXTABLA.ConnExterna

                    // Mostrar pantalla de carga antes de abrir MainMenu
                    this.Hide();
                    CargaDePantalla cargaDePantalla = new CargaDePantalla(usuario, conexionPrincipal);

                    if (cargaDePantalla.ShowDialog() == DialogResult.OK)
                    {
                        // Obtener los datos del formulario de carga
                        Usuario? usuarioRecuperado = cargaDePantalla.ObtenerUsuario();
                        ConexionInfo? conexionRecuperada = cargaDePantalla.ObtenerConexion();
                        MainMenu? mainMenuPrecargado = cargaDePantalla.ObtenerMainMenuPrecargado();

                        if (usuarioRecuperado != null && conexionRecuperada != null)
                        {
                            // Si el MainMenu fue pre-cargado, usarlo; si no, crear uno nuevo
                            Retorno360Tacna.FORMS.MainMenu mainMenu = mainMenuPrecargado 
                                ?? new Retorno360Tacna.FORMS.MainMenu(usuarioRecuperado, conexionRecuperada);

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

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                button1_Click(sender, e);
            }
        }
    }
}
