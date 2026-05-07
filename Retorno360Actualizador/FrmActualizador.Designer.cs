namespace Retorno360Actualizador
{
    partial class FrmActualizador
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmActualizador));
            pictureBox1 = new PictureBox();
            lblTitulo = new Label();
            lblEstado = new Label();
            progressBar = new ProgressBar();
            btnFinalizar = new Button();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(175, 30);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitulo.Location = new Point(75, 150);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(300, 30);
            lblTitulo.TabIndex = 1;
            lblTitulo.Text = "Actualizador Retorno 360";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 10F);
            lblEstado.ForeColor = Color.FromArgb(52, 73, 94);
            lblEstado.Location = new Point(30, 220);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(200, 19);
            lblEstado.TabIndex = 2;
            lblEstado.Text = "Iniciando actualización...";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(30, 260);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(390, 30);
            progressBar.TabIndex = 3;
            // 
            // btnFinalizar
            // 
            btnFinalizar.BackColor = Color.FromArgb(193, 39, 45);
            btnFinalizar.Cursor = Cursors.Hand;
            btnFinalizar.FlatAppearance.BorderSize = 0;
            btnFinalizar.FlatStyle = FlatStyle.Flat;
            btnFinalizar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnFinalizar.ForeColor = Color.White;
            btnFinalizar.Location = new Point(155, 320);
            btnFinalizar.Name = "btnFinalizar";
            btnFinalizar.Size = new Size(140, 45);
            btnFinalizar.TabIndex = 4;
            btnFinalizar.Text = "Finalizar";
            btnFinalizar.UseVisualStyleBackColor = false;
            btnFinalizar.Click += btnFinalizar_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(btnFinalizar);
            panel1.Controls.Add(lblTitulo);
            panel1.Controls.Add(progressBar);
            panel1.Controls.Add(lblEstado);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(450, 400);
            panel1.TabIndex = 5;
            // 
            // FrmActualizador
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 400);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmActualizador";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Actualizador Retorno 360";
            Load += FrmActualizador_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Label lblTitulo;
        private Label lblEstado;
        private ProgressBar progressBar;
        private Button btnFinalizar;
        private Panel panel1;
    }
}
