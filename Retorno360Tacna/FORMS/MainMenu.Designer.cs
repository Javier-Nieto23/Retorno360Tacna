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
            btnCerrarSesion = new Button();
            btnReportes = new Button();
            btnRetorno = new Button();
            btnSeleccionRazon = new Button();
            pictureBoxLogo = new PictureBox();
            panelContenido = new Panel();
            panelTop = new Panel();
            lblUsuario = new Label();
            lblTitulo = new Label();
            panelSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            panelTop.SuspendLayout();
            SuspendLayout();
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.FromArgb(44, 62, 80);
            panelSidebar.Controls.Add(btnCerrarSesion);
            panelSidebar.Controls.Add(btnReportes);
            panelSidebar.Controls.Add(btnRetorno);
            panelSidebar.Controls.Add(btnSeleccionRazon);
            panelSidebar.Controls.Add(pictureBoxLogo);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(250, 800);
            panelSidebar.TabIndex = 0;
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
            btnCerrarSesion.Text = "  Cerrar Sesión";
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
            btnReportes.ImageAlign = ContentAlignment.MiddleLeft;
            btnReportes.Location = new Point(0, 330);
            btnReportes.Name = "btnReportes";
            btnReportes.Padding = new Padding(20, 0, 0, 0);
            btnReportes.Size = new Size(250, 60);
            btnReportes.TabIndex = 3;
            btnReportes.Text = "Buttom3";
            btnReportes.TextAlign = ContentAlignment.MiddleLeft;
            btnReportes.UseVisualStyleBackColor = true;
            btnReportes.Click += btnReportes_Click;
            btnReportes.MouseEnter += MenuButton_MouseEnter;
            btnReportes.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnRetorno
            // 
            btnRetorno.Cursor = Cursors.Hand;
            btnRetorno.Dock = DockStyle.Top;
            btnRetorno.FlatAppearance.BorderSize = 0;
            btnRetorno.FlatStyle = FlatStyle.Flat;
            btnRetorno.Font = new Font("Segoe UI", 11F);
            btnRetorno.ForeColor = Color.White;
            btnRetorno.Image = Properties.Resources.Order_history_25404;
            btnRetorno.ImageAlign = ContentAlignment.MiddleRight;
            btnRetorno.Location = new Point(0, 270);
            btnRetorno.Name = "btnRetorno";
            btnRetorno.Padding = new Padding(20, 0, 0, 0);
            btnRetorno.Size = new Size(250, 60);
            btnRetorno.TabIndex = 2;
            btnRetorno.Text = "Reportes de Retorno";
            btnRetorno.TextAlign = ContentAlignment.MiddleLeft;
            btnRetorno.UseVisualStyleBackColor = true;
            btnRetorno.Click += btnRetorno_Click;
            btnRetorno.MouseEnter += MenuButton_MouseEnter;
            btnRetorno.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnSeleccionRazon
            // 
            btnSeleccionRazon.Cursor = Cursors.Hand;
            btnSeleccionRazon.Dock = DockStyle.Top;
            btnSeleccionRazon.FlatAppearance.BorderSize = 0;
            btnSeleccionRazon.FlatStyle = FlatStyle.Flat;
            btnSeleccionRazon.Font = new Font("Segoe UI", 11F);
            btnSeleccionRazon.ForeColor = Color.White;
            btnSeleccionRazon.ImageAlign = ContentAlignment.MiddleLeft;
            btnSeleccionRazon.Location = new Point(0, 210);
            btnSeleccionRazon.Name = "btnSeleccionRazon";
            btnSeleccionRazon.Padding = new Padding(20, 0, 0, 0);
            btnSeleccionRazon.Size = new Size(250, 60);
            btnSeleccionRazon.TabIndex = 1;
            btnSeleccionRazon.Text = "  Selección Razón";
            btnSeleccionRazon.TextAlign = ContentAlignment.MiddleLeft;
            btnSeleccionRazon.UseVisualStyleBackColor = true;
            btnSeleccionRazon.Click += btnSeleccionRazon_Click;
            btnSeleccionRazon.MouseEnter += MenuButton_MouseEnter;
            btnSeleccionRazon.MouseLeave += MenuButton_MouseLeave;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.Dock = DockStyle.Top;
            pictureBoxLogo.Image = Properties.Resources.ChatGPT_Image_Apr_21__2026__12_48_04_PM;
            pictureBoxLogo.Location = new Point(0, 0);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(250, 210);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
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
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1457, 800);
            Controls.Add(panelContenido);
            Controls.Add(panelTop);
            Controls.Add(panelSidebar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Retorno 360 Tacna - Sistema de Gestión";
            WindowState = FormWindowState.Maximized;
            Load += MainMenu_Load;
            panelSidebar.ResumeLayout(false);
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
        private Button btnReportes;
        private Button btnCerrarSesion;
        private Panel panelContenido;
        private Panel panelTop;
        private Label lblTitulo;
        private Label lblUsuario;
    }
}
