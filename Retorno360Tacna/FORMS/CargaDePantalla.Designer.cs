namespace Retorno360Tacna
{
    partial class CargaDePantalla
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
            pictureBox1 = new PictureBox();
            lblMensajeCarga = new Label();
            progressBar1 = new ProgressBar();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.ChatGPT_Image_Apr_21__2026__12_48_04_PM;
            pictureBox1.Location = new Point(-186, -30);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1169, 468);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // lblMensajeCarga
            // 
            lblMensajeCarga.Dock = DockStyle.Bottom;
            lblMensajeCarga.Font = new Font("Segoe UI", 10F);
            lblMensajeCarga.ForeColor = Color.FromArgb(64, 64, 64);
            lblMensajeCarga.Location = new Point(0, 420);
            lblMensajeCarga.Name = "lblMensajeCarga";
            lblMensajeCarga.Size = new Size(800, 30);
            lblMensajeCarga.TabIndex = 1;
            lblMensajeCarga.Text = "Iniciando sistema...";
            lblMensajeCarga.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(200, 370);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(400, 10);
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(702, 429);
            label1.Name = "label1";
            label1.Size = new Size(86, 15);
            label1.TabIndex = 3;
            label1.Text = "Version 0.0.01A";
            // 
            // CargaDePantalla
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Controls.Add(lblMensajeCarga);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "CargaDePantalla";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cargando";
            Load += CargaDePantalla_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label lblMensajeCarga;
        private ProgressBar progressBar1;
        private Label label1;
    }
}