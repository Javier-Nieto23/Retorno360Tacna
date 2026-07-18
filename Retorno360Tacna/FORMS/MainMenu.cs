using Retorno360Tacna.MODELS;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.CNX;
using Retorno360Tacna.SERVICES;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

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
        private bool navegacionLateralBloqueada = false;
        private const int ANCHO_SIDEBAR_EXPANDIDO = 250;
        private const int ANCHO_SIDEBAR_COLAPSADO = 60;
        private int anchoSidebarExpandidoActual = ANCHO_SIDEBAR_EXPANDIDO;
        private int anchoSidebarColapsadoActual = ANCHO_SIDEBAR_COLAPSADO;
        private decimal escalaUiActual = 1.0m;
        private bool panelNotificacionesVisible = false;
        private bool mostrarSoloNoLeidas = false;
        private readonly NotificacionService notificacionService = new();
        private FrmReportesInventario? frmReportesInventarioActivo;
        private readonly Dictionary<Button, Image?> imagenesOriginalesMenu = new();
        private readonly Dictionary<Button, Image?> imagenesHoverMenu = new();

        // Importar funciones de Windows para personalizar la barra de título
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;

        public MainMenu()
        {
            InitializeComponent();
            ConfigurarCierrePanelNotificaciones();
            if (SERVICES.ConfiguracionService.ObtenerAjusteVentanaPantallaLogica())
            {
                SERVICES.ConfiguracionService.AplicarPerfilPantallaLogica(this, true);
            }
            PersonalizarBarraTitulo();
            R2NotificationCenter.NotificationsChanged += R2NotificationCenter_NotificationsChanged;
            WindowsNotificationHelper.NotificationDetailRequested += WindowsNotificationHelper_NotificationDetailRequested;
        }

        public MainMenu(Usuario usuario, ConexionInfo conexion)
        {
            InitializeComponent();
            ConfigurarCierrePanelNotificaciones();
            if (SERVICES.ConfiguracionService.ObtenerAjusteVentanaPantallaLogica())
            {
                SERVICES.ConfiguracionService.AplicarPerfilPantallaLogica(this, true);
            }
            usuarioActual = usuario;
            conexionActual = conexion;
            InicializarMenuDesplegable();
            PersonalizarBarraTitulo();
            R2NotificationCenter.NotificationsChanged += R2NotificationCenter_NotificationsChanged;
            WindowsNotificationHelper.NotificationDetailRequested += WindowsNotificationHelper_NotificationDetailRequested;
        }

        private void WindowsNotificationHelper_NotificationDetailRequested(object? sender, WindowsNotificationHelper.NotificationClickInfo e)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => WindowsNotificationHelper_NotificationDetailRequested(sender, e)));
                return;
            }

            R2NotificationCenter.MarkAsRead(e.IdNotificacion);
            AbrirReportesInventarioDesdeNotificacion(e.Direccion, e.RutaR2);
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
            R2BucketMonitorService.Start();

            // Aplicar escalado de UI
            escalaUiActual = SERVICES.ConfiguracionService.ObtenerEscalaUI();
            if (escalaUiActual != 1.0m)
            {
                SERVICES.ConfiguracionService.AplicarEscalaFormulario(this, escalaUiActual);
            }

            AplicarEscalaMenuLateral();

            if (usuarioActual != null)
            {
                lblUsuario.Text = $"Usuario: {usuarioActual.NombreCompleto}";
            }

            pictureBox1.Visible = !sidebarColapsado;
            InicializarImagenesHoverMenu();

            CargarHistorialNotificaciones();
            RefrescarNotificacionesR2();

            ConfigurarAccesosPorRol();

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

        private void LimpiarPanel()
        {
            panelContenido.Controls.Clear();
            OcultarPanelNotificaciones();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            R2NotificationCenter.NotificationsChanged -= R2NotificationCenter_NotificationsChanged;
            R2BucketMonitorService.Stop();
            base.OnFormClosed(e);
        }

        private void R2NotificationCenter_NotificationsChanged(object? sender, EventArgs e)
        {
            if (IsDisposed)
                return;

            if (!IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefrescarNotificacionesR2));
                return;
            }

            RefrescarNotificacionesR2();
        }

        private void RefrescarNotificacionesR2()
        {
            var notificaciones = R2NotificationCenter.GetNotifications();
            int noLeidas = R2NotificationCenter.GetUnreadCount();

            if (mostrarSoloNoLeidas)
            {
                notificaciones = notificaciones.Where(n => !n.Leida).ToList();
            }

            lblContadorNotificaciones.Text = noLeidas > 99 ? "99+" : noLeidas.ToString();
            lblContadorNotificaciones.Visible = noLeidas > 0;
            ActualizarEstiloTabsNotificaciones();

            flpNotificaciones.SuspendLayout();
            flpNotificaciones.Controls.Clear();

            foreach (var notificacion in notificaciones)
            {
                flpNotificaciones.Controls.Add(CrearTarjetaNotificacion(notificacion));
            }

            flpNotificaciones.ResumeLayout();
            bool hayNotificaciones = flpNotificaciones.Controls.Count > 0;
            flpNotificaciones.Visible = hayNotificaciones;
            lblNotificacionesVacias.Visible = !hayNotificaciones;
        }

        private void CargarHistorialNotificaciones()
        {
            try
            {
                var historial = notificacionService.ObtenerHistorial();
                R2NotificationCenter.ReplaceNotifications(historial);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Carga de historial de notificaciones desde base de datos");
            }
        }

        private void lblTabTodas_Click(object sender, EventArgs e)
        {
            mostrarSoloNoLeidas = false;
            RefrescarNotificacionesR2();
        }

        private void lblTabNoLeidas_Click(object sender, EventArgs e)
        {
            mostrarSoloNoLeidas = true;
            RefrescarNotificacionesR2();
        }

        private void btnEliminarNotificaciones_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                R2NotificationCenter.MarkAllAsRead();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Marcado de notificaciones como leídas");
            }
        }

        private Control CrearIndicadorNoLeida(bool visible, int anchoTarjeta)
        {
            Panel indicador = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(Math.Max(0, anchoTarjeta - 18), 10),
                BackColor = Color.Transparent,
                Visible = visible
            };

            indicador.Paint += (_, e) =>
            {
                using SolidBrush brush = new SolidBrush(Color.FromArgb(239, 68, 68));
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, 0, 0, indicador.Width - 1, indicador.Height - 1);
            };

            return indicador;
        }

        private void ActualizarEstiloTabsNotificaciones()
        {
            lblTabTodas.Font = new Font("Segoe UI", 9F, mostrarSoloNoLeidas ? FontStyle.Regular : FontStyle.Bold | FontStyle.Underline);
            lblTabTodas.ForeColor = mostrarSoloNoLeidas ? Color.Gray : Color.FromArgb(14, 116, 144);

            lblTabNoLeidas.Font = new Font("Segoe UI", 9F, mostrarSoloNoLeidas ? FontStyle.Bold | FontStyle.Underline : FontStyle.Regular);
            lblTabNoLeidas.ForeColor = mostrarSoloNoLeidas ? Color.FromArgb(14, 116, 144) : Color.Gray;
        }

        private Control CrearTarjetaNotificacion(R2NotificationCenter.NotificationItem notificacion)
        {
            Panel tarjeta = new Panel
            {
                Width = flpNotificaciones.ClientSize.Width - 28,
                Height = 94,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = notificacion
            };

            Panel barra = new Panel
            {
                BackColor = notificacion.Leida ? Color.FromArgb(203, 213, 225) : Color.FromArgb(239, 68, 68),
                Dock = DockStyle.Left,
                Width = 4
            };

            Label lblFecha = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 8F),
                Location = new Point(14, 8),
                Text = notificacion.Fecha.ToString("d. M. yyyy")
            };

            Label lblTituloTarjeta = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(14, 28),
                Size = new Size(tarjeta.Width - 80, 20),
                Text = notificacion.Titulo
            };

            Label lblDescripcionTarjeta = new Label
            {
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(14, 48),
                Size = new Size(tarjeta.Width - 28, 20),
                Text = notificacion.Descripcion
            };

            LinkLabel lnkDetalle = new LinkLabel
            {
                AutoSize = true,
                LinkColor = Color.FromArgb(37, 99, 235),
                Location = new Point(14, 70),
                Text = "Ver detalle"
            };
            lnkDetalle.Click += (_, _) => MostrarDetalleNotificacion(notificacion);

            Control indicadorNoLeida = CrearIndicadorNoLeida(!notificacion.Leida, tarjeta.Width);

            Button btnCerrarTarjeta = new Button
            {
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9F),
                Text = "×",
                Size = new Size(24, 24),
                Location = new Point(tarjeta.Width - 34, 10),
                BackColor = Color.White,
                TabStop = false
            };
            btnCerrarTarjeta.FlatAppearance.BorderSize = 0;
            btnCerrarTarjeta.Click += (_, _) => CerrarTarjeta(notificacion.Id);

            tarjeta.Controls.Add(btnCerrarTarjeta);
            tarjeta.Controls.Add(lnkDetalle);
            tarjeta.Controls.Add(lblDescripcionTarjeta);
            tarjeta.Controls.Add(lblTituloTarjeta);
            tarjeta.Controls.Add(lblFecha);
            tarjeta.Controls.Add(barra);
            tarjeta.Controls.Add(indicadorNoLeida);
            indicadorNoLeida.BringToFront();

            return tarjeta;
        }

        private void MostrarDetalleNotificacion(R2NotificationCenter.NotificationItem notificacion)
        {
            R2NotificationCenter.MarkAsRead(notificacion.Id);
            panelNotificacionesVisible = false;
            panelNotificaciones.Visible = false;
            panelNotificacionesPico.Visible = false;

            AbrirReportesInventarioDesdeNotificacion(notificacion.Direccion, notificacion.RutaR2);
        }

        private void AbrirReportesInventarioDesdeNotificacion(string? direccion, string? rutaR2)
        {
            if (usuarioActual?.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) != true)
            {
                MessageBox.Show("Esta opción solo está disponible para usuarios administradores.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (conexionActual == null)
            {
                ErrorMessageHelper.ShowError("No hay conexión activa. Por favor, inicie sesión nuevamente.",
                    "Error", contexto: "Apertura de Reportes de Inventario desde notificación sin conexión activa");
                return;
            }

            ActivarBoton(btnReportesInventario);
            lblTitulo.Text = "Reportes de Inventario";
            LimpiarPanel();

            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(this, EventArgs.Empty);
            }

                frmReportesInventarioActivo = new FrmReportesInventario(conexionActual, usuarioActual)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            panelContenido.Controls.Add(frmReportesInventarioActivo);
            frmReportesInventarioActivo.Show();
            frmReportesInventarioActivo.NavegarADetalleNotificacion(direccion, rutaR2);
        }

        private void CerrarTarjeta(int idNotificacion)
        {
            R2NotificationCenter.MarkAsRead(idNotificacion);
            RefrescarNotificacionesR2();
        }

        private void btnNotificaciones_Click(object sender, EventArgs e)
        {
            panelNotificacionesVisible = !panelNotificacionesVisible;
            ReposicionarPanelNotificaciones();
            panelNotificaciones.Visible = panelNotificacionesVisible;
            panelNotificacionesPico.Visible = panelNotificacionesVisible;

            if (panelNotificacionesVisible)
            {
                R2NotificationCenter.MarkAllAsRead();
                RefrescarNotificacionesR2();
                panelNotificaciones.BringToFront();
                panelNotificacionesPico.BringToFront();
            }
        }

        private void ReposicionarPanelNotificaciones()
        {
            Point centroCampana = PointToClient(
                panelTop.PointToScreen(new Point(
                    btnNotificaciones.Left + (btnNotificaciones.Width / 2),
                    btnNotificaciones.Bottom)));

            int x = centroCampana.X - panelNotificaciones.Width + 120;
            int y = panelTop.Bottom + 8;

            if (x < panelSidebar.Width + 10)
            {
                x = panelSidebar.Width + 10;
            }

            int maxX = ClientSize.Width - panelNotificaciones.Width - 10;
            if (x > maxX)
            {
                x = maxX;
            }

            panelNotificaciones.Location = new Point(x, y);

            int picoX = centroCampana.X - (panelNotificacionesPico.Width / 2);
            int picoY = panelNotificaciones.Top - panelNotificacionesPico.Height + 2;
            panelNotificacionesPico.Location = new Point(picoX, picoY);
            AplicarFormaPicoNotificaciones();
            panelNotificaciones.BringToFront();
            panelNotificacionesPico.BringToFront();
        }

        private void OcultarPanelNotificaciones()
        {
            panelNotificacionesVisible = false;
            panelNotificaciones.Visible = false;
            panelNotificacionesPico.Visible = false;
        }

        private void ConfigurarAccesosPorRol()
        {
            bool esAdmin = usuarioActual?.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
            btnReportesInventario.Visible = esAdmin;
            btnSubMenuContabilidad.Visible = esAdmin;

            if (!esAdmin)
            {
                panelSubMenuAdmin.Height = ObtenerAlturaSubmenu(panelSubMenuAdmin);
                panelSubMenuInventarios.Height = ObtenerAlturaSubmenu(panelSubMenuInventarios);
            }
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
                panelSubMenuAdmin.Height = ObtenerAlturaSubmenu(panelSubMenuAdmin);
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

        private void btnSubMenuContabilidad_Click(object sender, EventArgs e)
        {
            if (usuarioActual?.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) != true)
            {
                MessageBox.Show("Esta opción solo está disponible para usuarios administradores.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivarBoton(btnSubMenuContabilidad);
            lblTitulo.Text = "Contabilidad";
            LimpiarPanel();

            if (!sidebarColapsado)
            {
                btnToggleSidebar_Click(sender, e);
            }

            if (conexionActual != null)
            {
                FrmContabilidadR2 frmContabilidad = new FrmContabilidadR2(conexionActual, usuarioActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                panelContenido.Controls.Add(frmContabilidad);
                frmContabilidad.Show();
            }
            else
            {
                ErrorMessageHelper.ShowError("No hay información de conexión disponible.",
                    "Error", contexto: "Apertura de Contabilidad sin conexión disponible");
            }
        }

        private void btnToggleSidebar_Click(object sender, EventArgs e)
        {
            sidebarColapsado = !sidebarColapsado;

            if (sidebarColapsado)
            {
                // Colapsar sidebar
                panelSidebar.Width = anchoSidebarColapsadoActual;

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

                pictureBox1.Visible = false;
            }
            else
            {
                // Expandir sidebar
                panelSidebar.Width = anchoSidebarExpandidoActual;

                // Mostrar textos de botones
                btnDiagramas.Text = "Inicio";
                btnAdministracion.Text = "Administración";
                btnInventarios.Text = "Inventarios";
                btnConfiguracion.Text = "Configuración";
                btnCerrarSesion.Text = "Cerrar Sesión";
                btnToggleSidebar.Text = "";

                pictureBox1.Visible = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (navegacionLateralBloqueada && keyData == Keys.Escape)
            {
                return true;
            }

            if (keyData == Keys.Escape && !sidebarColapsado)
            {
                btnToggleSidebar_Click(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
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
                panelSubMenuInventarios.Height = ObtenerAlturaSubmenu(panelSubMenuInventarios);
                btnInventarios.Text = "Inventarios";
            }
            else
            {
                panelSubMenuInventarios.Visible = false;
                panelSubMenuInventarios.Height = 0;
                btnInventarios.Text = "Inventarios";
            }
        }

        public void EstablecerNavegacionLateralHabilitada(bool habilitada)
        {
            navegacionLateralBloqueada = !habilitada;
            panelSidebar.Enabled = habilitada;
            panelSidebar.Cursor = habilitada ? Cursors.Default : Cursors.No;
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
                ErrorMessageHelper.ShowError("No hay conexión activa. Por favor, inicie sesión nuevamente.",
                    "Error", contexto: "Apertura de Catálogo de Partes sin conexión activa");
            }
        }

        private void btnReportesInventario_Click(object sender, EventArgs e)
        {
            if (usuarioActual?.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) != true)
            {
                MessageBox.Show("Esta opción solo está disponible para usuarios administradores.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                frmReportesInventarioActivo = new FrmReportesInventario(conexionActual, usuarioActual)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                panelContenido.Controls.Add(frmReportesInventarioActivo);
                frmReportesInventarioActivo.Show();
            }
            else
            {
                ErrorMessageHelper.ShowError("No hay conexión activa. Por favor, inicie sesión nuevamente.",
                    "Error", contexto: "Apertura de Reportes de Inventario sin conexión activa");
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
                ErrorMessageHelper.ShowError("No hay información de conexión disponible.",
                    "Error", contexto: "Apertura de Cálculo de IGI sin conexión disponible");
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
                ErrorMessageHelper.ShowError("No hay información de conexión disponible.",
                    "Error", contexto: "Apertura de Gestión de Retorno sin conexión disponible");
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
                ErrorMessageHelper.ShowError("No hay información de conexión disponible.",
                    "Error", contexto: "Apertura de Inicio sin conexión disponible");
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
                ErrorMessageHelper.ShowError("No hay información de conexión disponible.",
                    "Error", contexto: "Apertura de Configuración sin conexión disponible");
            }
        }

        private void panelSidebar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
