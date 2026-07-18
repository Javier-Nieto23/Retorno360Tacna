namespace Retorno360Tacna.FORMS
{
    partial class MainMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenu));
            panelSidebar = new Panel();
            pictureBox1 = new PictureBox();
            btnConfiguracion = new Button();
            btnCerrarSesion = new Button();
            btnReportes = new Button();
            panelSubMenuInventarios = new Panel();
            btnReportesInventario = new Button();
            btnCatalogoPartes = new Button();
            btnInventarios = new Button();
            panelSubMenuAdmin = new Panel();
            btnSubMenuContabilidad = new Button();
            btnSubMenuReporteIGI = new Button();
            btnSubMenuPorcentaje = new Button();
            btnAdministracion = new Button();
            btnDiagramas = new Button();
            btnToggleSidebar = new Button();
            btnSeleccionRazon = new Button();
            btnRetorno = new Button();
            panelContenido = new Panel();
            panelTop = new Panel();
            lblContadorNotificaciones = new Label();
            btnNotificaciones = new Button();
            lblUsuario = new Label();
            lblTitulo = new Label();
            panelNotificacionesPico = new Panel();
            panelNotificaciones = new Panel();
            flpNotificaciones = new FlowLayoutPanel();
            panelNotificacionesHeader = new Panel();
            btnEliminarNotificaciones = new LinkLabel();
            lblTabNoLeidas = new Label();
            lblTabTodas = new Label();
            lblTituloNotificaciones = new Label();
            lblNotificacionesVacias = new Label();
            panelSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panelSubMenuInventarios.SuspendLayout();
            panelSubMenuAdmin.SuspendLayout();
            panelTop.SuspendLayout();
            panelNotificaciones.SuspendLayout();
            panelNotificacionesHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.FromArgb(44, 62, 80);
            panelSidebar.Controls.Add(pictureBox1);
            panelSidebar.Controls.Add(btnConfiguracion);
            panelSidebar.Controls.Add(btnCerrarSesion);
            panelSidebar.Controls.Add(btnReportes);
            panelSidebar.Controls.Add(panelSubMenuInventarios);
            panelSidebar.Controls.Add(btnInventarios);
            panelSidebar.Controls.Add(panelSubMenuAdmin);
            panelSidebar.Controls.Add(btnAdministracion);
            panelSidebar.Controls.Add(btnDiagramas);
            panelSidebar.Controls.Add(btnToggleSidebar);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(250, 800);
            panelSidebar.TabIndex = 0;
            panelSidebar.Paint += panelSidebar_Paint;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(23, 29);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(163, 31);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 10;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnConfiguracion
            // 
            btnConfiguracion.Cursor = Cursors.Hand;
            btnConfiguracion.Dock = DockStyle.Bottom;
            btnConfiguracion.FlatAppearance.BorderSize = 0;
            btnConfiguracion.FlatStyle = FlatStyle.Flat;
            btnConfiguracion.Font = new Font("Segoe UI", 11F);
            btnConfiguracion.ForeColor = Color.White;
            btnConfiguracion.Image = Properties.Resources.configure_icon_icons_com_52404;
            btnConfiguracion.ImageAlign = ContentAlignment.MiddleRight;
            btnConfiguracion.Location = new Point(0, 680);
            btnConfiguracion.Name = "btnConfiguracion";
            btnConfiguracion.Padding = new Padding(20, 0, 0, 0);
            btnConfiguracion.Size = new Size(250, 60);
            btnConfiguracion.TabIndex = 8;
            btnConfiguracion.Text = "Configuración";
            btnConfiguracion.TextAlign = ContentAlignment.MiddleLeft;
            btnConfiguracion.UseVisualStyleBackColor = true;
            btnConfiguracion.Click += btnConfiguracion_Click;
            btnConfiguracion.MouseEnter += MenuButton_MouseEnter;
            btnConfiguracion.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.Cursor = Cursors.Hand;
            btnCerrarSesion.Dock = DockStyle.Bottom;
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            btnCerrarSesion.Font = new Font("Segoe UI", 11F);
            btnCerrarSesion.ForeColor = Color.White;
            btnCerrarSesion.Image = Properties.Resources.Logout_371271;
            btnCerrarSesion.ImageAlign = ContentAlignment.MiddleRight;
            btnCerrarSesion.Location = new Point(0, 740);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Padding = new Padding(20, 0, 0, 0);
            btnCerrarSesion.Size = new Size(250, 60);
            btnCerrarSesion.TabIndex = 4;
            btnCerrarSesion.Text = "Cerrar Sesión";
            btnCerrarSesion.TextAlign = ContentAlignment.MiddleLeft;
            btnCerrarSesion.UseVisualStyleBackColor = true;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            btnCerrarSesion.MouseEnter += MenuButton_MouseEnter;
            btnCerrarSesion.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnReportes
            // 
            btnReportes.Cursor = Cursors.Hand;
            btnReportes.Dock = DockStyle.Top;
            btnReportes.FlatAppearance.BorderSize = 0;
            btnReportes.FlatStyle = FlatStyle.Flat;
            btnReportes.Font = new Font("Segoe UI", 11F);
            btnReportes.ForeColor = Color.White;
            btnReportes.Image = Properties.Resources.Sales_report_25411;
            btnReportes.ImageAlign = ContentAlignment.MiddleRight;
            btnReportes.Location = new Point(0, 510);
            btnReportes.Name = "btnReportes";
            btnReportes.Padding = new Padding(20, 0, 0, 0);
            btnReportes.Size = new Size(250, 60);
            btnReportes.TabIndex = 3;
            btnReportes.Text = "Otros Reportes";
            btnReportes.TextAlign = ContentAlignment.MiddleLeft;
            btnReportes.UseVisualStyleBackColor = true;
            btnReportes.Click += btnReportes_Click;
            btnReportes.MouseEnter += MenuButton_MouseEnter;
            btnReportes.MouseLeave += MenuButton_MouseLeave;
            // 
            // panelSubMenuInventarios
            // 
            panelSubMenuInventarios.BackColor = Color.FromArgb(35, 42, 50);
            panelSubMenuInventarios.Controls.Add(btnReportesInventario);
            panelSubMenuInventarios.Controls.Add(btnCatalogoPartes);
            panelSubMenuInventarios.Dock = DockStyle.Top;
            panelSubMenuInventarios.Location = new Point(0, 390);
            panelSubMenuInventarios.Name = "panelSubMenuInventarios";
            panelSubMenuInventarios.Size = new Size(250, 120);
            panelSubMenuInventarios.TabIndex = 9;
            panelSubMenuInventarios.Visible = false;
            // 
            // btnReportesInventario
            // 
            btnReportesInventario.Cursor = Cursors.Hand;
            btnReportesInventario.Dock = DockStyle.Top;
            btnReportesInventario.FlatAppearance.BorderSize = 0;
            btnReportesInventario.FlatStyle = FlatStyle.Flat;
            btnReportesInventario.Font = new Font("Segoe UI", 10F);
            btnReportesInventario.ForeColor = Color.LightGray;
            btnReportesInventario.Image = Properties.Resources.truck_icon_icons_com_52347__1_;
            btnReportesInventario.ImageAlign = ContentAlignment.MiddleRight;
            btnReportesInventario.Location = new Point(0, 60);
            btnReportesInventario.Name = "btnReportesInventario";
            btnReportesInventario.Padding = new Padding(35, 0, 0, 0);
            btnReportesInventario.Size = new Size(250, 60);
            btnReportesInventario.TabIndex = 1;
            btnReportesInventario.Text = "Reportes de Inventario";
            btnReportesInventario.TextAlign = ContentAlignment.MiddleLeft;
            btnReportesInventario.UseVisualStyleBackColor = true;
            btnReportesInventario.Click += btnReportesInventario_Click;
            btnReportesInventario.MouseEnter += MenuButton_MouseEnter;
            btnReportesInventario.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnCatalogoPartes
            // 
            btnCatalogoPartes.Cursor = Cursors.Hand;
            btnCatalogoPartes.Dock = DockStyle.Top;
            btnCatalogoPartes.FlatAppearance.BorderSize = 0;
            btnCatalogoPartes.FlatStyle = FlatStyle.Flat;
            btnCatalogoPartes.Font = new Font("Segoe UI", 10F);
            btnCatalogoPartes.ForeColor = Color.LightGray;
            btnCatalogoPartes.Image = Properties.Resources.Packing1_25393;
            btnCatalogoPartes.ImageAlign = ContentAlignment.MiddleRight;
            btnCatalogoPartes.Location = new Point(0, 0);
            btnCatalogoPartes.Name = "btnCatalogoPartes";
            btnCatalogoPartes.Padding = new Padding(35, 0, 0, 0);
            btnCatalogoPartes.Size = new Size(250, 60);
            btnCatalogoPartes.TabIndex = 0;
            btnCatalogoPartes.Text = "Catálogo de Partes";
            btnCatalogoPartes.TextAlign = ContentAlignment.MiddleLeft;
            btnCatalogoPartes.UseVisualStyleBackColor = true;
            btnCatalogoPartes.Click += btnCatalogoPartes_Click;
            btnCatalogoPartes.MouseEnter += MenuButton_MouseEnter;
            btnCatalogoPartes.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnInventarios
            // 
            btnInventarios.Cursor = Cursors.Hand;
            btnInventarios.Dock = DockStyle.Top;
            btnInventarios.FlatAppearance.BorderSize = 0;
            btnInventarios.FlatStyle = FlatStyle.Flat;
            btnInventarios.Font = new Font("Segoe UI", 11F);
            btnInventarios.ForeColor = Color.White;
            btnInventarios.Image = Properties.Resources.business_inventory_maintenance_product_box_boxes_2326;
            btnInventarios.ImageAlign = ContentAlignment.MiddleRight;
            btnInventarios.Location = new Point(0, 330);
            btnInventarios.Name = "btnInventarios";
            btnInventarios.Padding = new Padding(20, 0, 0, 0);
            btnInventarios.Size = new Size(250, 60);
            btnInventarios.TabIndex = 7;
            btnInventarios.Text = "Inventarios";
            btnInventarios.TextAlign = ContentAlignment.MiddleLeft;
            btnInventarios.UseVisualStyleBackColor = true;
            btnInventarios.Click += btnInventarios_Click;
            btnInventarios.MouseEnter += MenuButton_MouseEnter;
            btnInventarios.MouseLeave += MenuButton_MouseLeave;
            // 
            // panelSubMenuAdmin
            // 
            panelSubMenuAdmin.BackColor = Color.FromArgb(35, 42, 50);
            panelSubMenuAdmin.Controls.Add(btnSubMenuContabilidad);
            panelSubMenuAdmin.Controls.Add(btnSubMenuReporteIGI);
            panelSubMenuAdmin.Controls.Add(btnSubMenuPorcentaje);
            panelSubMenuAdmin.Dock = DockStyle.Top;
            panelSubMenuAdmin.Location = new Point(0, 210);
            panelSubMenuAdmin.Name = "panelSubMenuAdmin";
            panelSubMenuAdmin.Size = new Size(250, 180);
            panelSubMenuAdmin.TabIndex = 6;
            panelSubMenuAdmin.Visible = false;
            // 
            // btnSubMenuContabilidad
            // 
            btnSubMenuContabilidad.Cursor = Cursors.Hand;
            btnSubMenuContabilidad.Dock = DockStyle.Top;
            btnSubMenuContabilidad.FlatAppearance.BorderSize = 0;
            btnSubMenuContabilidad.FlatStyle = FlatStyle.Flat;
            btnSubMenuContabilidad.Font = new Font("Segoe UI", 10F);
            btnSubMenuContabilidad.ForeColor = Color.LightGray;
            btnSubMenuContabilidad.Image = Properties.Resources.Earning_statement_253912;
            btnSubMenuContabilidad.ImageAlign = ContentAlignment.MiddleRight;
            btnSubMenuContabilidad.Location = new Point(0, 120);
            btnSubMenuContabilidad.Name = "btnSubMenuContabilidad";
            btnSubMenuContabilidad.Padding = new Padding(35, 0, 0, 0);
            btnSubMenuContabilidad.Size = new Size(250, 60);
            btnSubMenuContabilidad.TabIndex = 2;
            btnSubMenuContabilidad.Text = "Contabilidad";
            btnSubMenuContabilidad.TextAlign = ContentAlignment.MiddleLeft;
            btnSubMenuContabilidad.UseVisualStyleBackColor = true;
            btnSubMenuContabilidad.Click += btnSubMenuContabilidad_Click;
            btnSubMenuContabilidad.MouseEnter += MenuButton_MouseEnter;
            btnSubMenuContabilidad.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnSubMenuReporteIGI
            // 
            btnSubMenuReporteIGI.Cursor = Cursors.Hand;
            btnSubMenuReporteIGI.Dock = DockStyle.Top;
            btnSubMenuReporteIGI.FlatAppearance.BorderSize = 0;
            btnSubMenuReporteIGI.FlatStyle = FlatStyle.Flat;
            btnSubMenuReporteIGI.Font = new Font("Segoe UI", 10F);
            btnSubMenuReporteIGI.ForeColor = Color.LightGray;
            btnSubMenuReporteIGI.Image = Properties.Resources.Earning_statement_253912;
            btnSubMenuReporteIGI.ImageAlign = ContentAlignment.MiddleRight;
            btnSubMenuReporteIGI.Location = new Point(0, 60);
            btnSubMenuReporteIGI.Name = "btnSubMenuReporteIGI";
            btnSubMenuReporteIGI.Padding = new Padding(35, 0, 0, 0);
            btnSubMenuReporteIGI.Size = new Size(250, 60);
            btnSubMenuReporteIGI.TabIndex = 1;
            btnSubMenuReporteIGI.Text = "Reporte IGI";
            btnSubMenuReporteIGI.TextAlign = ContentAlignment.MiddleLeft;
            btnSubMenuReporteIGI.UseVisualStyleBackColor = true;
            btnSubMenuReporteIGI.Click += btnSeleccionRazon_Click;
            btnSubMenuReporteIGI.MouseEnter += MenuButton_MouseEnter;
            btnSubMenuReporteIGI.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnSubMenuPorcentaje
            // 
            btnSubMenuPorcentaje.Cursor = Cursors.Hand;
            btnSubMenuPorcentaje.Dock = DockStyle.Top;
            btnSubMenuPorcentaje.FlatAppearance.BorderSize = 0;
            btnSubMenuPorcentaje.FlatStyle = FlatStyle.Flat;
            btnSubMenuPorcentaje.Font = new Font("Segoe UI", 10F);
            btnSubMenuPorcentaje.ForeColor = Color.LightGray;
            btnSubMenuPorcentaje.Image = Properties.Resources.increase_25373;
            btnSubMenuPorcentaje.ImageAlign = ContentAlignment.MiddleRight;
            btnSubMenuPorcentaje.Location = new Point(0, 0);
            btnSubMenuPorcentaje.Name = "btnSubMenuPorcentaje";
            btnSubMenuPorcentaje.Padding = new Padding(35, 0, 0, 0);
            btnSubMenuPorcentaje.Size = new Size(250, 60);
            btnSubMenuPorcentaje.TabIndex = 0;
            btnSubMenuPorcentaje.Text = "% Retorno";
            btnSubMenuPorcentaje.TextAlign = ContentAlignment.MiddleLeft;
            btnSubMenuPorcentaje.UseVisualStyleBackColor = true;
            btnSubMenuPorcentaje.Click += btnRetorno_Click;
            btnSubMenuPorcentaje.MouseEnter += MenuButton_MouseEnter;
            btnSubMenuPorcentaje.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnAdministracion
            // 
            btnAdministracion.Cursor = Cursors.Hand;
            btnAdministracion.Dock = DockStyle.Top;
            btnAdministracion.FlatAppearance.BorderSize = 0;
            btnAdministracion.FlatStyle = FlatStyle.Flat;
            btnAdministracion.Font = new Font("Segoe UI", 11F);
            btnAdministracion.ForeColor = Color.White;
            btnAdministracion.Image = Properties.Resources.Teachers_35749;
            btnAdministracion.ImageAlign = ContentAlignment.MiddleRight;
            btnAdministracion.Location = new Point(0, 150);
            btnAdministracion.Name = "btnAdministracion";
            btnAdministracion.Padding = new Padding(20, 0, 0, 0);
            btnAdministracion.Size = new Size(250, 60);
            btnAdministracion.TabIndex = 5;
            btnAdministracion.Text = "Administración ";
            btnAdministracion.TextAlign = ContentAlignment.MiddleLeft;
            btnAdministracion.UseVisualStyleBackColor = true;
            btnAdministracion.Click += btnAdministracion_Click;
            btnAdministracion.MouseEnter += MenuButton_MouseEnter;
            btnAdministracion.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnDiagramas
            // 
            btnDiagramas.Cursor = Cursors.Hand;
            btnDiagramas.Dock = DockStyle.Top;
            btnDiagramas.FlatAppearance.BorderSize = 0;
            btnDiagramas.FlatStyle = FlatStyle.Flat;
            btnDiagramas.Font = new Font("Segoe UI", 11F);
            btnDiagramas.ForeColor = Color.White;
            btnDiagramas.Image = Properties.Resources.home256_24783;
            btnDiagramas.ImageAlign = ContentAlignment.MiddleRight;
            btnDiagramas.Location = new Point(0, 80);
            btnDiagramas.Name = "btnDiagramas";
            btnDiagramas.Padding = new Padding(20, 0, 0, 0);
            btnDiagramas.Size = new Size(250, 70);
            btnDiagramas.TabIndex = 5;
            btnDiagramas.Text = "Inicio";
            btnDiagramas.TextAlign = ContentAlignment.MiddleLeft;
            btnDiagramas.UseVisualStyleBackColor = true;
            btnDiagramas.Click += btnDiagramas_Click;
            btnDiagramas.MouseEnter += MenuButton_MouseEnter;
            btnDiagramas.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnToggleSidebar
            // 
            btnToggleSidebar.Cursor = Cursors.Hand;
            btnToggleSidebar.Dock = DockStyle.Top;
            btnToggleSidebar.FlatAppearance.BorderSize = 0;
            btnToggleSidebar.FlatStyle = FlatStyle.Flat;
            btnToggleSidebar.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnToggleSidebar.ForeColor = Color.White;
            btnToggleSidebar.Image = Properties.Resources._1491313929_menu_82986;
            btnToggleSidebar.ImageAlign = ContentAlignment.MiddleRight;
            btnToggleSidebar.Location = new Point(0, 0);
            btnToggleSidebar.Name = "btnToggleSidebar";
            btnToggleSidebar.Size = new Size(250, 80);
            btnToggleSidebar.TabIndex = 9;
            btnToggleSidebar.TextAlign = ContentAlignment.MiddleRight;
            btnToggleSidebar.UseVisualStyleBackColor = true;
            btnToggleSidebar.Click += btnToggleSidebar_Click;
            btnToggleSidebar.MouseEnter += MenuButton_MouseEnter;
            btnToggleSidebar.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnSeleccionRazon
            // 
            btnSeleccionRazon.Cursor = Cursors.Hand;
            btnSeleccionRazon.Dock = DockStyle.Top;
            btnSeleccionRazon.FlatAppearance.BorderSize = 0;
            btnSeleccionRazon.FlatStyle = FlatStyle.Flat;
            btnSeleccionRazon.Font = new Font("Segoe UI", 11F);
            btnSeleccionRazon.ForeColor = Color.White;
            btnSeleccionRazon.Image = Properties.Resources.US_dollar_25324;
            btnSeleccionRazon.ImageAlign = ContentAlignment.MiddleRight;
            btnSeleccionRazon.Location = new Point(0, 334);
            btnSeleccionRazon.Name = "btnSeleccionRazon";
            btnSeleccionRazon.Padding = new Padding(20, 0, 0, 0);
            btnSeleccionRazon.Size = new Size(250, 60);
            btnSeleccionRazon.TabIndex = 1;
            btnSeleccionRazon.Text = "Reporte de IGI";
            btnSeleccionRazon.TextAlign = ContentAlignment.MiddleLeft;
            btnSeleccionRazon.UseVisualStyleBackColor = true;
            btnSeleccionRazon.Click += btnSeleccionRazon_Click;
            btnSeleccionRazon.MouseEnter += MenuButton_MouseEnter;
            btnSeleccionRazon.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnRetorno
            // 
            btnRetorno.Cursor = Cursors.Hand;
            btnRetorno.Dock = DockStyle.Top;
            btnRetorno.FlatAppearance.BorderSize = 0;
            btnRetorno.FlatStyle = FlatStyle.Flat;
            btnRetorno.Font = new Font("Segoe UI", 11F);
            btnRetorno.ForeColor = Color.White;
            btnRetorno.Image = Properties.Resources.increase_25373;
            btnRetorno.ImageAlign = ContentAlignment.MiddleRight;
            btnRetorno.Location = new Point(0, 274);
            btnRetorno.Name = "btnRetorno";
            btnRetorno.Padding = new Padding(20, 0, 0, 0);
            btnRetorno.Size = new Size(250, 60);
            btnRetorno.TabIndex = 2;
            btnRetorno.Text = "Porcentaje de Retorno";
            btnRetorno.TextAlign = ContentAlignment.MiddleLeft;
            btnRetorno.UseVisualStyleBackColor = true;
            btnRetorno.Click += btnRetorno_Click;
            btnRetorno.MouseEnter += MenuButton_MouseEnter;
            btnRetorno.MouseLeave += MenuButton_MouseLeave;
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.FromArgb(245, 246, 250);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(250, 80);
            panelContenido.Name = "panelContenido";
            panelContenido.Size = new Size(1207, 720);
            panelContenido.TabIndex = 1;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(44, 62, 80);
            panelTop.Controls.Add(lblContadorNotificaciones);
            panelTop.Controls.Add(btnNotificaciones);
            panelTop.Controls.Add(lblUsuario);
            panelTop.Controls.Add(lblTitulo);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(250, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1207, 80);
            panelTop.TabIndex = 2;
            // 
            // lblContadorNotificaciones
            // 
            lblContadorNotificaciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblContadorNotificaciones.BackColor = Color.FromArgb(231, 76, 60);
            lblContadorNotificaciones.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblContadorNotificaciones.ForeColor = Color.White;
            lblContadorNotificaciones.Location = new Point(1022, 10);
            lblContadorNotificaciones.Name = "lblContadorNotificaciones";
            lblContadorNotificaciones.Size = new Size(22, 22);
            lblContadorNotificaciones.TabIndex = 3;
            lblContadorNotificaciones.Text = "0";
            lblContadorNotificaciones.TextAlign = ContentAlignment.MiddleCenter;
            lblContadorNotificaciones.Visible = false;
            // 
            // btnNotificaciones
            // 
            btnNotificaciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNotificaciones.Cursor = Cursors.Hand;
            btnNotificaciones.FlatAppearance.BorderSize = 0;
            btnNotificaciones.FlatStyle = FlatStyle.Flat;
            btnNotificaciones.Font = new Font("Segoe UI Emoji", 18F);
            btnNotificaciones.ForeColor = Color.White;
            btnNotificaciones.Image = Properties.Resources._1490886276_19_school_bell_82497;
            btnNotificaciones.Location = new Point(954, 12);
            btnNotificaciones.Name = "btnNotificaciones";
            btnNotificaciones.Size = new Size(68, 52);
            btnNotificaciones.TabIndex = 2;
            btnNotificaciones.UseVisualStyleBackColor = true;
            btnNotificaciones.Click += btnNotificaciones_Click;
            // 
            // lblUsuario
            // 
            lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUsuario.Font = new Font("Segoe UI", 10F);
            lblUsuario.ForeColor = Color.White;
            lblUsuario.Location = new Point(1045, 20);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(150, 40);
            lblUsuario.TabIndex = 1;
            lblUsuario.Text = "Usuario: Admin";
            lblUsuario.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(227, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Retorno 360 Tacna";
            // 
            // panelNotificacionesPico
            // 
            panelNotificacionesPico.BackColor = Color.Transparent;
            panelNotificacionesPico.Location = new Point(1100, 72);
            panelNotificacionesPico.Name = "panelNotificacionesPico";
            panelNotificacionesPico.Size = new Size(28, 16);
            panelNotificacionesPico.TabIndex = 5;
            panelNotificacionesPico.Visible = false;
            panelNotificacionesPico.Paint += panelNotificacionesPico_Paint;
            // 
            // panelNotificaciones
            // 
            panelNotificaciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelNotificaciones.BackColor = Color.White;
            panelNotificaciones.BorderStyle = BorderStyle.FixedSingle;
            panelNotificaciones.Controls.Add(flpNotificaciones);
            panelNotificaciones.Controls.Add(panelNotificacionesHeader);
            panelNotificaciones.Controls.Add(lblNotificacionesVacias);
            panelNotificaciones.Location = new Point(718, 56);
            panelNotificaciones.Name = "panelNotificaciones";
            panelNotificaciones.Size = new Size(360, 360);
            panelNotificaciones.TabIndex = 4;
            panelNotificaciones.Visible = false;
            // 
            // flpNotificaciones
            // 
            flpNotificaciones.AutoScroll = true;
            flpNotificaciones.BackColor = Color.FromArgb(249, 250, 251);
            flpNotificaciones.Dock = DockStyle.Fill;
            flpNotificaciones.FlowDirection = FlowDirection.TopDown;
            flpNotificaciones.Location = new Point(0, 88);
            flpNotificaciones.Name = "flpNotificaciones";
            flpNotificaciones.Padding = new Padding(12, 0, 12, 12);
            flpNotificaciones.Size = new Size(358, 270);
            flpNotificaciones.TabIndex = 2;
            flpNotificaciones.WrapContents = false;
            // 
            // panelNotificacionesHeader
            // 
            panelNotificacionesHeader.Controls.Add(btnEliminarNotificaciones);
            panelNotificacionesHeader.Controls.Add(lblTabNoLeidas);
            panelNotificacionesHeader.Controls.Add(lblTabTodas);
            panelNotificacionesHeader.Controls.Add(lblTituloNotificaciones);
            panelNotificacionesHeader.Dock = DockStyle.Top;
            panelNotificacionesHeader.Location = new Point(0, 0);
            panelNotificacionesHeader.Name = "panelNotificacionesHeader";
            panelNotificacionesHeader.Size = new Size(358, 88);
            panelNotificacionesHeader.TabIndex = 1;
            // 
            // btnEliminarNotificaciones
            // 
            btnEliminarNotificaciones.ActiveLinkColor = Color.FromArgb(37, 99, 235);
            btnEliminarNotificaciones.AutoSize = true;
            btnEliminarNotificaciones.LinkColor = Color.FromArgb(37, 99, 235);
            btnEliminarNotificaciones.Location = new Point(206, 50);
            btnEliminarNotificaciones.Name = "btnEliminarNotificaciones";
            btnEliminarNotificaciones.Size = new Size(101, 15);
            btnEliminarNotificaciones.TabIndex = 3;
            btnEliminarNotificaciones.TabStop = true;
            btnEliminarNotificaciones.Text = "Marcar todo leído";
            btnEliminarNotificaciones.LinkClicked += btnEliminarNotificaciones_LinkClicked;
            // 
            // lblTabNoLeidas
            // 
            lblTabNoLeidas.AutoSize = true;
            lblTabNoLeidas.Cursor = Cursors.Hand;
            lblTabNoLeidas.Font = new Font("Segoe UI", 9F);
            lblTabNoLeidas.ForeColor = Color.Gray;
            lblTabNoLeidas.Location = new Point(58, 50);
            lblTabNoLeidas.Name = "lblTabNoLeidas";
            lblTabNoLeidas.Size = new Size(56, 15);
            lblTabNoLeidas.TabIndex = 2;
            lblTabNoLeidas.Text = "No leídas";
            lblTabNoLeidas.Click += lblTabNoLeidas_Click;
            // 
            // lblTabTodas
            // 
            lblTabTodas.AutoSize = true;
            lblTabTodas.Cursor = Cursors.Hand;
            lblTabTodas.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            lblTabTodas.ForeColor = Color.FromArgb(14, 116, 144);
            lblTabTodas.Location = new Point(12, 50);
            lblTabTodas.Name = "lblTabTodas";
            lblTabTodas.Size = new Size(38, 15);
            lblTabTodas.TabIndex = 1;
            lblTabTodas.Text = "Todas";
            lblTabTodas.Click += lblTabTodas_Click;
            // 
            // lblTituloNotificaciones
            // 
            lblTituloNotificaciones.AutoSize = true;
            lblTituloNotificaciones.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloNotificaciones.ForeColor = Color.FromArgb(55, 65, 81);
            lblTituloNotificaciones.Location = new Point(12, 12);
            lblTituloNotificaciones.Name = "lblTituloNotificaciones";
            lblTituloNotificaciones.Size = new Size(121, 21);
            lblTituloNotificaciones.TabIndex = 0;
            lblTituloNotificaciones.Text = "Notificaciones";
            // 
            // lblNotificacionesVacias
            // 
            lblNotificacionesVacias.Dock = DockStyle.Fill;
            lblNotificacionesVacias.Font = new Font("Segoe UI", 10F);
            lblNotificacionesVacias.ForeColor = Color.Gray;
            lblNotificacionesVacias.Location = new Point(0, 0);
            lblNotificacionesVacias.Name = "lblNotificacionesVacias";
            lblNotificacionesVacias.Size = new Size(358, 358);
            lblNotificacionesVacias.TabIndex = 1;
            lblNotificacionesVacias.Text = "Sin notificaciones";
            lblNotificacionesVacias.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1457, 800);
            Controls.Add(panelNotificacionesPico);
            Controls.Add(panelNotificaciones);
            Controls.Add(panelContenido);
            Controls.Add(panelTop);
            Controls.Add(panelSidebar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Retorno 360 Tacna - Sistema de Gestión - Sistema Desarrollado por Javier Nieto    |  Version: 3.0.0";
            WindowState = FormWindowState.Maximized;
            Load += MainMenu_Load;
            panelSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panelSubMenuInventarios.ResumeLayout(false);
            panelSubMenuAdmin.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelNotificaciones.ResumeLayout(false);
            panelNotificacionesHeader.ResumeLayout(false);
            panelNotificacionesHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSidebar;
        private Button btnSeleccionRazon;
        private Button btnRetorno;
        private Button btnDiagramas;
        private Button btnReportes;
        private Button btnCerrarSesion;
        private Button btnConfiguracion;
        private Panel panelContenido;
        private Panel panelTop;
        private Label lblTitulo;
        private Label lblUsuario;
        private Button btnNotificaciones;
        private Label lblContadorNotificaciones;
        private Panel panelNotificacionesPico;
        private Panel panelNotificaciones;
        private FlowLayoutPanel flpNotificaciones;
        private Panel panelNotificacionesHeader;
        private LinkLabel btnEliminarNotificaciones;
        private Label lblTabNoLeidas;
        private Label lblTabTodas;
        private Label lblTituloNotificaciones;
        private Label lblNotificacionesVacias;
        private Button btnAdministracion;
        private Panel panelSubMenuAdmin;
        private Button btnSubMenuPorcentaje;
        private Button btnSubMenuReporteIGI;
        private Button btnSubMenuContabilidad;
        private Button btnInventarios;
        private Button btnToggleSidebar;
        private Panel panelSubMenuInventarios;
        private Button btnCatalogoPartes;
        private Button btnReportesInventario;
        private PictureBox pictureBox1;
    }
}
