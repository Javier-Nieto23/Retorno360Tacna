namespace Retorno360Tacna.FORMS
{
    partial class DiagramasOperacion
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
            panelPrincipal = new Panel();
            lblVersion = new Label();
            lblSubtitulo = new Label();
            lblBienvenida = new Label();
            pictureBox1 = new PictureBox();
            panelPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panelPrincipal
            // 
            panelPrincipal.BackColor = Color.White;
            panelPrincipal.Controls.Add(pictureBox1);
            panelPrincipal.Controls.Add(lblVersion);
            panelPrincipal.Controls.Add(lblSubtitulo);
            panelPrincipal.Controls.Add(lblBienvenida);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Location = new Point(0, 0);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Size = new Size(1271, 560);
            panelPrincipal.TabIndex = 0;
            // 
            // lblVersion
            // 
            lblVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblVersion.Font = new Font("Segoe UI", 9F);
            lblVersion.ForeColor = Color.FromArgb(189, 195, 199);
            lblVersion.Location = new Point(1071, 520);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(180, 20);
            lblVersion.TabIndex = 3;
            lblVersion.Text = "Versión 2.6 - 2026";
            lblVersion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.Anchor = AnchorStyles.None;
            lblSubtitulo.Font = new Font("Segoe UI", 14F);
            lblSubtitulo.ForeColor = Color.FromArgb(127, 140, 141);
            lblSubtitulo.Location = new Point(12, 70);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(1271, 30);
            lblSubtitulo.TabIndex = 2;
            lblSubtitulo.Text = "Sistema de Gestión de Retorno 360°";
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblBienvenida
            // 
            lblBienvenida.Anchor = AnchorStyles.None;
            lblBienvenida.Font = new Font("Segoe UI", 32F, FontStyle.Bold);
            lblBienvenida.ForeColor = Color.FromArgb(44, 62, 80);
            lblBienvenida.Location = new Point(12, 0);
            lblBienvenida.Name = "lblBienvenida";
            lblBienvenida.Size = new Size(1271, 60);
            lblBienvenida.TabIndex = 1;
            lblBienvenida.Text = "¡Bienvenido al Sistema!";
            lblBienvenida.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.None;
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.Gemini_Generated_Image_o36ilto36ilto36i;
            pictureBox1.Location = new Point(248, 103);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(792, 506);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // DiagramasOperacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1271, 560);
            Controls.Add(panelPrincipal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "DiagramasOperacion";
            Text = "Bienvenida";
            Load += DiagramasOperacion_Load;
            panelPrincipal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private Label lblBienvenida;
        private Label lblSubtitulo;
        private Label lblVersion;
        private PictureBox pictureBox1;
    }
}