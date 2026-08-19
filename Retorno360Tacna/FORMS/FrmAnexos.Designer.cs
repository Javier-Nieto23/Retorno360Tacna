namespace Retorno360Tacna.FORMS
{
    partial class FrmAnexos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controls
        private System.Windows.Forms.Label lblRazon;
        private System.Windows.Forms.Label lblBase;
        private System.Windows.Forms.Label lblInicio;
        private System.Windows.Forms.Label lblFin;
        private System.Windows.Forms.ComboBox cmbRazon;
        private System.Windows.Forms.ComboBox cmbBase;
        private System.Windows.Forms.DateTimePicker dtpInicio;
        private System.Windows.Forms.DateTimePicker dtpFin;
        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.Button btnGuardarPortal;
        private System.Windows.Forms.Button btnExportarExcel;
        private System.Windows.Forms.DataGridView dgvPreview;

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmbRazon = new ComboBox();
            cmbBase = new ComboBox();
            dtpInicio = new DateTimePicker();
            dtpFin = new DateTimePicker();
            btnGenerar = new Button();
            btnGuardarPortal = new Button();
            btnExportarExcel = new Button();
            dgvPreview = new DataGridView();
            lblRazon = new Label();
            lblBase = new Label();
            lblInicio = new Label();
            lblFin = new Label();
            chkUsarPerfil = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
            SuspendLayout();
            // 
            // cmbRazon
            // 
            cmbRazon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazon.FormattingEnabled = true;
            cmbRazon.Location = new Point(110, 6);
            cmbRazon.Name = "cmbRazon";
            cmbRazon.Size = new Size(300, 23);
            cmbRazon.TabIndex = 0;
            cmbRazon.SelectedIndexChanged += CmbRazon_SelectedIndexChanged;
            // 
            // cmbBase
            // 
            cmbBase.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBase.FormattingEnabled = true;
            cmbBase.Location = new Point(530, 6);
            cmbBase.Name = "cmbBase";
            cmbBase.Size = new Size(300, 23);
            cmbBase.TabIndex = 1;
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(110, 36);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new Size(150, 23);
            dtpInicio.TabIndex = 2;
            // 
            // dtpFin
            // 
            dtpFin.Format = DateTimePickerFormat.Short;
            dtpFin.Location = new Point(352, 36);
            dtpFin.Name = "dtpFin";
            dtpFin.Size = new Size(150, 23);
            dtpFin.TabIndex = 3;
            // 
            // btnGenerar
            // 
            btnGenerar.BackColor = Color.FromArgb(152, 79, 224);
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.FlatStyle = FlatStyle.Flat;
            btnGenerar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerar.ForeColor = Color.White;
            btnGenerar.Location = new Point(571, 34);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new Size(140, 40);
            btnGenerar.TabIndex = 4;
            btnGenerar.Text = "Generar Preview";
            btnGenerar.UseVisualStyleBackColor = false;
            btnGenerar.Click += btnGenerar_Click;
            // 
            // btnGuardarPortal
            // 
            btnGuardarPortal.BackColor = Color.FromArgb(0, 10, 196);
            btnGuardarPortal.FlatAppearance.BorderSize = 0;
            btnGuardarPortal.FlatStyle = FlatStyle.Flat;
            btnGuardarPortal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardarPortal.ForeColor = Color.White;
            btnGuardarPortal.Location = new Point(721, 34);
            btnGuardarPortal.Name = "btnGuardarPortal";
            btnGuardarPortal.Size = new Size(140, 40);
            btnGuardarPortal.TabIndex = 5;
            btnGuardarPortal.Text = "Guardar en Portal";
            btnGuardarPortal.UseVisualStyleBackColor = false;
            btnGuardarPortal.Click += btnGuardarPortal_Click;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(0, 196, 20);
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Location = new Point(871, 34);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(120, 40);
            btnExportarExcel.TabIndex = 6;
            btnExportarExcel.Text = "Exportar Excel";
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // dgvPreview
            // 
            dgvPreview.AllowUserToAddRows = false;
            dgvPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPreview.Location = new Point(10, 128);
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.Size = new Size(960, 560);
            dgvPreview.TabIndex = 7;
            // 
            // lblRazon
            // 
            lblRazon.AutoSize = true;
            lblRazon.Location = new Point(10, 9);
            lblRazon.Name = "lblRazon";
            lblRazon.Size = new Size(76, 15);
            lblRazon.TabIndex = 100;
            lblRazon.Text = "Razón Social:";
            // 
            // lblBase
            // 
            lblBase.AutoSize = true;
            lblBase.Location = new Point(430, 9);
            lblBase.Name = "lblBase";
            lblBase.Size = new Size(83, 15);
            lblBase.TabIndex = 101;
            lblBase.Text = "Base de Datos:";
            // 
            // lblInicio
            // 
            lblInicio.AutoSize = true;
            lblInicio.Location = new Point(10, 40);
            lblInicio.Name = "lblInicio";
            lblInicio.Size = new Size(73, 15);
            lblInicio.TabIndex = 102;
            lblInicio.Text = "Fecha Inicio:";
            // 
            // lblFin
            // 
            lblFin.AutoSize = true;
            lblFin.Location = new Point(272, 40);
            lblFin.Name = "lblFin";
            lblFin.Size = new Size(60, 15);
            lblFin.TabIndex = 103;
            lblFin.Text = "Fecha Fin:";
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkUsarPerfil.Location = new Point(12, 76);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(203, 23);
            chkUsarPerfil.TabIndex = 104;
            chkUsarPerfil.Text = "Usar mis empresas (Perfil)";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            // 
            // FrmAnexos
            // 
            ClientSize = new Size(1000, 700);
            Controls.Add(chkUsarPerfil);
            Controls.Add(lblRazon);
            Controls.Add(cmbRazon);
            Controls.Add(lblBase);
            Controls.Add(cmbBase);
            Controls.Add(lblInicio);
            Controls.Add(dtpInicio);
            Controls.Add(lblFin);
            Controls.Add(dtpFin);
            Controls.Add(btnGenerar);
            Controls.Add(btnGuardarPortal);
            Controls.Add(btnExportarExcel);
            Controls.Add(dgvPreview);
            Name = "FrmAnexos";
            Text = "Reporte Anexos y Inventarios";
            ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private CheckBox chkUsarPerfil;
    }
}
