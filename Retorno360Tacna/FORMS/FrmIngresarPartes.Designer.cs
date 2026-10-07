namespace Retorno360Tacna.FORMS
{
    partial class FrmIngresarPartes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblSubtitulo = new Label();
            lblTitulo = new Label();
            panelCuerpo = new Panel();
            lblAyuda = new Label();
            txtPartes = new TextBox();
            lblInstruccion = new Label();
            lblAno = new Label();
            cmbAno = new ComboBox();
            lblMes = new Label();
            cmbMes = new ComboBox();
            panelBotones = new Panel();
            btnCancelar = new Button();
            btnVerificar = new Button();
            label1 = new Label();
            panelHeader.SuspendLayout();
            panelCuerpo.SuspendLayout();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 80, 50);
            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(20, 0, 20, 0);
            panelHeader.Size = new Size(560, 72);
            panelHeader.TabIndex = 0;
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.Font = new Font("Segoe UI", 9.5F);
            lblSubtitulo.ForeColor = Color.FromArgb(200, 240, 210);
            lblSubtitulo.Location = new Point(20, 42);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(440, 20);
            lblSubtitulo.TabIndex = 1;
            lblSubtitulo.Text = "Ingrese número de parte, cantidad y unidad por línea (ej: PARTE 2 PCS o PARTE,2,PCS)";
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 8);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(440, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Verificación de Números de Parte";
            // 
            // panelCuerpo
            // 
            panelCuerpo.BackColor = Color.White;
            panelCuerpo.Controls.Add(label1);
            panelCuerpo.Controls.Add(lblAyuda);
            panelCuerpo.Controls.Add(txtPartes);
            panelCuerpo.Controls.Add(lblInstruccion);
            panelCuerpo.Controls.Add(lblAno);
            panelCuerpo.Controls.Add(cmbAno);
            panelCuerpo.Controls.Add(lblMes);
            panelCuerpo.Controls.Add(cmbMes);
            panelCuerpo.Dock = DockStyle.Fill;
            panelCuerpo.Location = new Point(0, 72);
            panelCuerpo.Name = "panelCuerpo";
            panelCuerpo.Padding = new Padding(24, 18, 24, 10);
            panelCuerpo.Size = new Size(560, 300);
            panelCuerpo.TabIndex = 1;
            // 
            // lblAyuda
            // 
            lblAyuda.AutoSize = true;
            lblAyuda.Font = new Font("Segoe UI", 8.5F);
            lblAyuda.ForeColor = Color.FromArgb(120, 130, 140);
            lblAyuda.Location = new Point(0, 215);
            lblAyuda.Name = "lblAyuda";
            lblAyuda.Size = new Size(566, 15);
            lblAyuda.TabIndex = 2;
            lblAyuda.Text = "Ejemplo por línea: VAP-3000002-S 164 PCS  —  Separe por comas o espacios; si omite cantidad se asume 1.";
            // 
            // txtPartes
            // 
            txtPartes.Font = new Font("Segoe UI", 10F);
            txtPartes.Location = new Point(24, 42);
            txtPartes.Multiline = true;
            txtPartes.Name = "txtPartes";
            txtPartes.ScrollBars = ScrollBars.Vertical;
            txtPartes.Size = new Size(452, 160);
            txtPartes.TabIndex = 1;
            // 
            // lblInstruccion
            // 
            lblInstruccion.AutoSize = true;
            lblInstruccion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblInstruccion.ForeColor = Color.FromArgb(50, 60, 70);
            lblInstruccion.Location = new Point(24, 18);
            lblInstruccion.Name = "lblInstruccion";
            lblInstruccion.Size = new Size(311, 17);
            lblInstruccion.TabIndex = 0;
            lblInstruccion.Text = "Formato: PARTE, CANTIDAD, UM (una por línea):";
            // 
            // lblAno
            // 
            lblAno.AutoSize = true;
            lblAno.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblAno.ForeColor = Color.FromArgb(50, 60, 70);
            lblAno.Location = new Point(8, 253);
            lblAno.Name = "lblAno";
            lblAno.Size = new Size(103, 17);
            lblAno.TabIndex = 3;
            lblAno.Text = "Año a verificar:";
            // 
            // cmbAno
            // 
            cmbAno.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAno.Font = new Font("Segoe UI", 10F);
            cmbAno.FormattingEnabled = true;
            cmbAno.Location = new Point(117, 247);
            cmbAno.Name = "cmbAno";
            cmbAno.Size = new Size(120, 25);
            cmbAno.TabIndex = 4;
            // 
            // lblMes
            // 
            lblMes.AutoSize = true;
            lblMes.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMes.ForeColor = Color.FromArgb(50, 60, 70);
            lblMes.Location = new Point(200, 250);
            lblMes.Name = "lblMes";
            lblMes.Size = new Size(37, 17);
            lblMes.TabIndex = 5;
            lblMes.Text = "Mes:";
            // 
            // cmbMes
            // 
            cmbMes.BackColor = Color.White;
            cmbMes.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMes.Font = new Font("Segoe UI", 10F);
            cmbMes.ForeColor = Color.FromArgb(40, 40, 40);
            cmbMes.FormattingEnabled = true;
            cmbMes.Location = new Point(352, 247);
            cmbMes.Name = "cmbMes";
            cmbMes.Size = new Size(160, 25);
            cmbMes.TabIndex = 6;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(245, 247, 250);
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnVerificar);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 372);
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(10, 10, 14, 10);
            panelBotones.Size = new Size(560, 56);
            panelBotones.TabIndex = 2;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.BackColor = Color.FromArgb(149, 165, 166);
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Location = new Point(456, 10);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(90, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // btnVerificar
            // 
            btnVerificar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVerificar.BackColor = Color.FromArgb(30, 150, 70);
            btnVerificar.FlatAppearance.BorderSize = 0;
            btnVerificar.FlatStyle = FlatStyle.Flat;
            btnVerificar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnVerificar.ForeColor = Color.White;
            btnVerificar.Location = new Point(350, 10);
            btnVerificar.Name = "btnVerificar";
            btnVerificar.Size = new Size(100, 36);
            btnVerificar.TabIndex = 0;
            btnVerificar.Text = "Verificar";
            btnVerificar.UseVisualStyleBackColor = false;
            btnVerificar.Click += btnVerificar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(50, 60, 70);
            label1.Location = new Point(243, 253);
            label1.Name = "label1";
            label1.Size = new Size(103, 17);
            label1.TabIndex = 7;
            label1.Text = "Mes a verificar:";
            // 
            // FrmIngresarPartes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(560, 428);
            Controls.Add(panelCuerpo);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmIngresarPartes";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Verificación de Números de Parte";
            panelHeader.ResumeLayout(false);
            panelCuerpo.ResumeLayout(false);
            panelCuerpo.PerformLayout();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel panelCuerpo;
        private System.Windows.Forms.Label lblInstruccion;
        private System.Windows.Forms.TextBox txtPartes;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnVerificar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblMes;
        private System.Windows.Forms.ComboBox cmbMes;
        private System.Windows.Forms.Label lblAno;
        private System.Windows.Forms.ComboBox cmbAno;
        private Label label1;
    }
}
