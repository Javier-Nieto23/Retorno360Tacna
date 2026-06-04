using Retorno360Tacna.MODELS;
using Retorno360Tacna.CNX;
using System.Runtime.InteropServices;

namespace Retorno360Tacna.FORMS
{
    public partial class MainMenu : Form
    {
        private Usuario? usuarioActual;
        private ConexionInfo? conexionActual;
        private Button? botonActivo;
        private bool sidebarColapsado = false;
        private bool menuAdminExpandido = false;
        private bool menuInventariosExpandido = false;
        private const int ANCHO_SIDEBAR_EXPANDIDO = 250;
        private const int ANCHO_SIDEBAR_COLAPSADO = 60;

        // Importar funciones de Windows para personalizar la barra de título
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;

        public MainMenu()
        {
            InitializeComponent();
            PersonalizarBarraTitulo();
        }

        public MainMenu(Usuario usuario, ConexionInfo conexion)
        {
            InitializeComponent();
            usuarioActual = usuario;
            conexionActual = conexion;
            InicializarMenuDesplegable();
            PersonalizarBarraTitulo();
        }

        private void PersonalizarBarraTitulo()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            {
                // Color azul oscuro RGB(44, 62, 80) convertido a formato BGR para Windows API
                int colorBGR = 0x00503E2C; // 80, 62, 44 en hexadecimal BGR
                DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, ref colorBGR, sizeof(int));
            }
        }

        private void InicializarMenuDesplegable()
        {
            // Ocultar el panel de sub-menú al inicio
            panelSubMenuAdmin.Visible = false;
            panelSubMenuAdmin.Height = 0;

            panelSubMenuInventarios.Visible = false;
            panelSubMenuInventarios.Height = 0;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            // Aplicar color personalizado a la barra de título
            PersonalizarBarraTitulo();

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

                // Ocultar sub-menú de Administración si está expandido
                if (menuAdminExpandido)
                {
                    panelSubMenuAdmin.Visible = false;
                    menuAdminExpandido = false;
                }

                // Ocultar sub-menú de Inventarios si está expandido
                if (menuInventariosExpandido)
                {
                    panelSubMenuInventarios.Visible = false;
                    menuInventariosExpandido = false;
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
            if (sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

            menuInventariosExpandido = !menuInventariosExpandido;

            if (menuInventariosExpandido)
            {
                panelSubMenuInventarios.Visible = true;
                panelSubMenuInventarios.Height = 120;
                btnInventarios.Text = "Inventarios";
            }
            else
            {
                panelSubMenuInventarios.Visible = false;
                panelSubMenuInventarios.Height = 0;
                btnInventarios.Text = "Inventarios";
            }
        }

        private void btnCatalogoPartes_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnCatalogoPartes);
            lblTitulo.Text = "Catálogo de Partes";
            LimpiarPanel();

            // Colapsar sidebar automáticamente al seleccionar una opción del submenú
            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

            if (conexionActual != null)
            {
                FrmCatalogoPartes frmCatalogo = new FrmCatalogoPartes(conexionActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmCatalogo);
                frmCatalogo.Show();
            }
            else
            {
                MessageBox.Show("No hay conexión activa. Por favor, inicie sesión nuevamente.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReportesInventario_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnReportesInventario);
            lblTitulo.Text = "Reportes de Inventario";
            LimpiarPanel();

            // Colapsar sidebar automáticamente al seleccionar una opción del submenú
            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

            if (conexionActual != null)
            {
                FrmReportesInventario frmReportesInventario = new FrmReportesInventario(conexionActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmReportesInventario);
                frmReportesInventario.Show();
            }
            else
            {
                MessageBox.Show("No hay conexión activa. Por favor, inicie sesión nuevamente.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSeleccionRazon_Click(object sender, EventArgs e)
        {
            ActivarBoton(btnSubMenuReporteIGI);
            lblTitulo.Text = "Cálculo de IGI Pagado";
            LimpiarPanel();

            // Colapsar sidebar automáticamente al seleccionar una opción del submenú
            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

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

            // Colapsar sidebar automáticamente al seleccionar una opción del submenú
            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

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

        private void panelSidebar_Paint(object sender, PaintEventArgs e)
        {

        }

 
    }
}
