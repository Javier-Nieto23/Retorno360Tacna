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
            btnConfiguracion = new Button();
            btnCerrarSesion = new Button();
            btnReportes = new Button();
            btnInventarios = new Button();
            panelSubMenuAdmin = new Panel();
            btnSubMenuReporteIGI = new Button();
            btnSubMenuPorcentaje = new Button();
            btnAdministracion = new Button();
            btnDiagramas = new Button();
            pictureBoxLogo = new PictureBox();
            btnToggleSidebar = new Button();
            btnSeleccionRazon = new Button();
            btnRetorno = new Button();
            panelContenido = new Panel();
            panelTop = new Panel();
            lblUsuario = new Label();
            lblTitulo = new Label();
            panelSidebar.SuspendLayout();
            panelSubMenuAdmin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            panelTop.SuspendLayout();
            SuspendLayout();
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.FromArgb(44, 62, 80);
            panelSidebar.Controls.Add(btnConfiguracion);
            panelSidebar.Controls.Add(btnCerrarSesion);
            panelSidebar.Controls.Add(btnReportes);
            panelSidebar.Controls.Add(btnInventarios);
            panelSidebar.Controls.Add(panelSubMenuAdmin);
            panelSidebar.Controls.Add(btnAdministracion);
            panelSidebar.Controls.Add(btnDiagramas);
            panelSidebar.Controls.Add(pictureBoxLogo);
            panelSidebar.Controls.Add(btnToggleSidebar);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(250, 800);
            panelSidebar.TabIndex = 0;
            panelSidebar.Paint += panelSidebar_Paint;
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
            btnReportes.Location = new Point(0, 588);
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
            btnInventarios.Location = new Point(0, 528);
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
            panelSubMenuAdmin.Controls.Add(btnSubMenuReporteIGI);
            panelSubMenuAdmin.Controls.Add(btnSubMenuPorcentaje);
            panelSubMenuAdmin.Dock = DockStyle.Top;
            panelSubMenuAdmin.Location = new Point(0, 408);
            panelSubMenuAdmin.Name = "panelSubMenuAdmin";
            panelSubMenuAdmin.Size = new Size(250, 120);
            panelSubMenuAdmin.TabIndex = 6;
            panelSubMenuAdmin.Visible = false;
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
            btnAdministracion.Location = new Point(0, 348);
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
            btnDiagramas.Location = new Point(0, 278);
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
            // pictureBoxLogo
            // 
            pictureBoxLogo.Dock = DockStyle.Top;
            pictureBoxLogo.Image = Properties.Resources.ChatGPT_Image_Apr_21__2026__12_48_04_PM;
            pictureBoxLogo.Location = new Point(0, 74);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(250, 204);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            // 
            // btnToggleSidebar
            // 
            btnToggleSidebar.Cursor = Cursors.Hand;
            btnToggleSidebar.Dock = DockStyle.Top;
            btnToggleSidebar.FlatAppearance.BorderSize = 0;
            btnToggleSidebar.FlatStyle = FlatStyle.Flat;
            btnToggleSidebar.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnToggleSidebar.ForeColor = Color.White;
            btnToggleSidebar.Image = Properties.Resources.ui_interface_list_navigation_menu_switcher_icon_219789;
            btnToggleSidebar.ImageAlign = ContentAlignment.MiddleRight;
            btnToggleSidebar.Location = new Point(0, 0);
            btnToggleSidebar.Name = "btnToggleSidebar";
            btnToggleSidebar.Size = new Size(250, 74);
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
            panelTop.BackColor = Color.White;
            panelTop.Controls.Add(lblUsuario);
            panelTop.Controls.Add(lblTitulo);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(250, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1207, 80);
            panelTop.TabIndex = 2;
            // 
            // lblUsuario
            // 
            lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUsuario.Font = new Font("Segoe UI", 10F);
            lblUsuario.ForeColor = Color.FromArgb(44, 62, 80);
            lblUsuario.Location = new Point(907, 20);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(280, 40);
            lblUsuario.TabIndex = 1;
            lblUsuario.Text = "Usuario: Admin";
            lblUsuario.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(193, 39, 45);
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(227, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Retorno 360 Tacna";
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1457, 800);
            Controls.Add(panelContenido);
            Controls.Add(panelTop);
            Controls.Add(panelSidebar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Retorno 360 Tacna - Sistema de Gestión - Sistema Desarrollado por Javier Nieto";
            WindowState = FormWindowState.Maximized;
            Load += MainMenu_Load;
            panelSidebar.ResumeLayout(false);
            panelSubMenuAdmin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSidebar;
        private PictureBox pictureBoxLogo;
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
        private Button btnAdministracion;
        private Panel panelSubMenuAdmin;
        private Button btnSubMenuPorcentaje;
        private Button btnSubMenuReporteIGI;
        private Button btnInventarios;
        private Button btnToggleSidebar;
    }
}
