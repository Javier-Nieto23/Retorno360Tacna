using Retorno360Tacna.MODELS;
using Retorno360Tacna.CNX;

namespace Retorno360Tacna.FORMS
{
    public partial class MainMenu : Form
    {
        private Usuario? usuarioActual;
        private ConexionInfo? conexionActual;
        private Button? botonActivo;
        private bool sidebarColapsado = false;
        private bool menuAdminExpandido = false;
        private const int ANCHO_SIDEBAR_EXPANDIDO = 250;
        private const int ANCHO_SIDEBAR_COLAPSADO = 60;

        public MainMenu()
        {
            InitializeComponent();
        }

        public MainMenu(Usuario usuario, ConexionInfo conexion)
        {
            InitializeComponent();
            usuarioActual = usuario;
            conexionActual = conexion;
            InicializarMenuDesplegable();
        }

        private void InicializarMenuDesplegable()
        {
            // Ocultar el panel de sub-menú al inicio
            panelSubMenuAdmin.Visible = false;
            panelSubMenuAdmin.Height = 0;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            // Aplicar escalado de UI
            decimal escala = SERVICES.ConfiguracionService.ObtenerEscalaUI();
            if (escala != 1.0m)
            {
                SERVICES.ConfiguracionService.AplicarEscalaFormulario(this, escala);
            }

            if (usuarioActual != null)
            {
                lblUsuario.Text = $"Usuario: {usuarioActual.NombreCompleto}";
            }

            // Cargar automáticamente pantalla de bienvenida al iniciar
            if (conexionActual != null)
            {
                ActivarBoton(btnDiagramas);
                lblTitulo.Text = "Bienvenida";
                LimpiarPanel();

                DiagramasOperacion frmBienvenida = new DiagramasOperacion(conexionActual, usuarioActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmBienvenida);
                frmBienvenida.Show();
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

        private void btnAdministracion_Click(object sender, EventArgs e)
        {
            if (sidebarColapsado)
            {
                // Si el sidebar está colapsado, expandirlo primero
                btnToggleSidebar_Click(sender, e);
            }

            menuAdminExpandido = !menuAdminExpandido;

            if (menuAdminExpandido)
            {
                // Expandir sub-menú
                panelSubMenuAdmin.Visible = true;
                panelSubMenuAdmin.Height = 120; // 2 botones x 60px
                btnAdministracion.Text = "Administración";
            }
            else
            {
                // Colapsar sub-menú
                panelSubMenuAdmin.Visible = false;
                panelSubMenuAdmin.Height = 0;
                btnAdministracion.Text = "Administración";
            }
        }

        private void btnToggleSidebar_Click(object sender, EventArgs e)
        {
            sidebarColapsado = !sidebarColapsado;

            if (sidebarColapsado)
            {
                // Colapsar sidebar
                panelSidebar.Width = ANCHO_SIDEBAR_COLAPSADO;

                // Ocultar sub-menú si está expandido
                if (menuAdminExpandido)
                {
                    panelSubMenuAdmin.Visible = false;
                    menuAdminExpandido = false;
                }

                // Ocultar textos de botones principales
                btnDiagramas.Text = "";
                btnAdministracion.Text = "";
                btnInventarios.Text = "";
                btnConfiguracion.Text = "";
                btnCerrarSesion.Text = "";
                btnToggleSidebar.Text = "";

                // Ocultar logo
                pictureBoxLogo.Visible = false;
            }
            else
            {
                // Expandir sidebar
                panelSidebar.Width = ANCHO_SIDEBAR_EXPANDIDO;

                // Mostrar textos de botones
                btnDiagramas.Text = "Inicio";
                btnAdministracion.Text = "Administración";
                btnInventarios.Text = "Inventarios";
                btnConfiguracion.Text = "Configuración";
                btnCerrarSesion.Text = "Cerrar Sesión";
                btnToggleSidebar.Text = "";

                // Mostrar logo
                pictureBoxLogo.Visible = true;
            }
        }

        private void btnInventarios_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnInventarios);
            lblTitulo.Text = "Inventarios";
            LimpiarPanel();

            MessageBox.Show("Módulo de Inventarios en desarrollo",
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // TODO: Implementar formulario de Inventarios cuando esté disponible
            // if (conexionActual != null)
            // {
            //     FrmInventarios frmInventarios = new FrmInventarios(conexionActual)
            //     {
            //         TopLevel = false,
            //         FormBorderStyle = FormBorderStyle.None,
            //         Dock = DockStyle.Fill
            //     };
            //     panelContenido.Controls.Add(frmInventarios);
            //     frmInventarios.Show();
            // }
        }

        private void btnSeleccionRazon_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnSubMenuReporteIGI);
            lblTitulo.Text = "Cálculo de IGI Pagado";
            LimpiarPanel();

            if (conexionActual != null)
            {
                FrmReportes frmReportes = new FrmReportes(conexionActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmReportes);
                frmReportes.Show();
            }
            else
            {
                MessageBox.Show("No hay información de conexión disponible.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRetorno_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnSubMenuPorcentaje);
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
            lblTitulo.Text = "Reporte IGI Pagado";
            LimpiarPanel();


        }

        private void btnDiagramas_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnDiagramas);
            lblTitulo.Text = "Inicio";
            LimpiarPanel();

            if (conexionActual != null)
            {
                DiagramasOperacion frmDiagramas = new DiagramasOperacion(conexionActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmDiagramas);
                frmDiagramas.Show();
            }
            else
            {
                MessageBox.Show("No hay información de conexión disponible.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnConfiguracion);
            lblTitulo.Text = "Configuración";
            LimpiarPanel();

            if (conexionActual != null)
            {
                FrmConfiguracion frmConfig = new FrmConfiguracion(conexionActual, usuarioActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmConfig);
                frmConfig.Show();
            }
            else
            {
                MessageBox.Show("No hay información de conexión disponible.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private MODELS.ConfiguracionUsuario CargarConfiguracion()
        {
            try
            {
                string rutaConfig = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Retorno360Tacna",
                    "config.txt");

                if (File.Exists(rutaConfig))
                {
                    var config = new MODELS.ConfiguracionUsuario();
                    var lineas = File.ReadAllLines(rutaConfig);

                    foreach (var linea in lineas)
                    {
                        var partes = linea.Split('=');
                        if (partes.Length == 2)
                        {
                            string clave = partes[0].Trim();
                            string valor = partes[1].Trim();

                            if (clave == "EscalaUI" && decimal.TryParse(valor, out decimal escala))
                            {
                                config.EscalaUI = escala;
                            }
                        }
                    }

                    return config;
                }
                else
                {
                    // Configuración por defecto
                    return new MODELS.ConfiguracionUsuario
                    {
                        EscalaUI = 1.0m
                    };
                }
            }
            catch
            {
                return new MODELS.ConfiguracionUsuario
                {
                    EscalaUI = 1.0m
                };
            }
        }

        private void panelSidebar_Paint(object sender, PaintEventArgs e)
        {

        }

 
    }
}
