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
            this.components = new System.ComponentModel.Container();
            this.cmbRazon = new System.Windows.Forms.ComboBox();
            this.cmbBase = new System.Windows.Forms.ComboBox();
            this.dtpInicio = new System.Windows.Forms.DateTimePicker();
            this.dtpFin = new System.Windows.Forms.DateTimePicker();
            this.btnGenerar = new System.Windows.Forms.Button();
            this.btnGuardarPortal = new System.Windows.Forms.Button();
            this.btnExportarExcel = new System.Windows.Forms.Button();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRazon
            // 
            this.lblRazon = new System.Windows.Forms.Label();
            this.lblRazon.AutoSize = true;
            this.lblRazon.Location = new System.Drawing.Point(10, 9);
            this.lblRazon.Name = "lblRazon";
            this.lblRazon.Size = new System.Drawing.Size(90, 15);
            this.lblRazon.TabIndex = 100;
            this.lblRazon.Text = "Razón Social:";
            // 
            // cmbRazon
            // 
            this.cmbRazon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRazon.FormattingEnabled = true;
            this.cmbRazon.Location = new System.Drawing.Point(110, 6);
            this.cmbRazon.Name = "cmbRazon";
            this.cmbRazon.Size = new System.Drawing.Size(300, 23);
            this.cmbRazon.TabIndex = 0;
            this.cmbRazon.SelectedIndexChanged += new System.EventHandler(this.CmbRazon_SelectedIndexChanged);
            // 
            // lblBase
            // 
            this.lblBase = new System.Windows.Forms.Label();
            this.lblBase.AutoSize = true;
            this.lblBase.Location = new System.Drawing.Point(430, 9);
            this.lblBase.Name = "lblBase";
            this.lblBase.Size = new System.Drawing.Size(90, 15);
            this.lblBase.TabIndex = 101;
            this.lblBase.Text = "Base de Datos:";
            // 
            // cmbBase
            // 
            this.cmbBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBase.FormattingEnabled = true;
            this.cmbBase.Location = new System.Drawing.Point(530, 6);
            this.cmbBase.Name = "cmbBase";
            this.cmbBase.Size = new System.Drawing.Size(300, 23);
            this.cmbBase.TabIndex = 1;
            // 
            // dtpInicio
            // 
            this.dtpInicio.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpInicio.Location = new System.Drawing.Point(110, 36);
            this.dtpInicio.Name = "dtpInicio";
            this.dtpInicio.Size = new System.Drawing.Size(150, 23);
            this.dtpInicio.TabIndex = 2;
            // 
            // lblInicio
            // 
            this.lblInicio = new System.Windows.Forms.Label();
            this.lblInicio.AutoSize = true;
            this.lblInicio.Location = new System.Drawing.Point(10, 40);
            this.lblInicio.Name = "lblInicio";
            this.lblInicio.Size = new System.Drawing.Size(70, 15);
            this.lblInicio.TabIndex = 102;
            this.lblInicio.Text = "Fecha Inicio:";
            // 
            // dtpFin
            // 
            this.dtpFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFin.Location = new System.Drawing.Point(360, 36);
            this.dtpFin.Name = "dtpFin";
            this.dtpFin.Size = new System.Drawing.Size(150, 23);
            this.dtpFin.TabIndex = 3;
            // 
            // lblFin
            // 
            this.lblFin = new System.Windows.Forms.Label();
            this.lblFin.AutoSize = true;
            this.lblFin.Location = new System.Drawing.Point(280, 40);
            this.lblFin.Name = "lblFin";
            this.lblFin.Size = new System.Drawing.Size(60, 15);
            this.lblFin.TabIndex = 103;
            this.lblFin.Text = "Fecha Fin:";
            // 
            // btnGenerar
            // 
            this.btnGenerar.Location = new System.Drawing.Point(530, 34);
            this.btnGenerar.Name = "btnGenerar";
            this.btnGenerar.Size = new System.Drawing.Size(140, 25);
            this.btnGenerar.TabIndex = 4;
            this.btnGenerar.Text = "Generar Preview";
            this.btnGenerar.UseVisualStyleBackColor = true;
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);
            // 
            // btnGuardarPortal
            // 
            this.btnGuardarPortal.Location = new System.Drawing.Point(680, 34);
            this.btnGuardarPortal.Name = "btnGuardarPortal";
            this.btnGuardarPortal.Size = new System.Drawing.Size(140, 25);
            this.btnGuardarPortal.TabIndex = 5;
            this.btnGuardarPortal.Text = "Guardar en Portal";
            this.btnGuardarPortal.UseVisualStyleBackColor = true;
            this.btnGuardarPortal.Click += new System.EventHandler(this.btnGuardarPortal_Click);
            // 
            // btnExportarExcel
            // 
            this.btnExportarExcel.Location = new System.Drawing.Point(830, 34);
            this.btnExportarExcel.Name = "btnExportarExcel";
            this.btnExportarExcel.Size = new System.Drawing.Size(120, 25);
            this.btnExportarExcel.TabIndex = 6;
            this.btnExportarExcel.Text = "Exportar Excel";
            this.btnExportarExcel.UseVisualStyleBackColor = true;
            this.btnExportarExcel.Click += new System.EventHandler(this.btnExportarExcel_Click);
            // 
            // dgvPreview
            // 
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPreview.Location = new System.Drawing.Point(10, 80);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.Size = new System.Drawing.Size(960, 560);
            this.dgvPreview.TabIndex = 7;
            // 
            // FrmAnexos
            // 
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.lblRazon);
            this.Controls.Add(this.cmbRazon);
            this.Controls.Add(this.lblBase);
            this.Controls.Add(this.cmbBase);
            this.Controls.Add(this.lblInicio);
            this.Controls.Add(this.dtpInicio);
            this.Controls.Add(this.lblFin);
            this.Controls.Add(this.dtpFin);
            this.Controls.Add(this.btnGenerar);
            this.Controls.Add(this.btnGuardarPortal);
            this.Controls.Add(this.btnExportarExcel);
            this.Controls.Add(this.dgvPreview);
            this.Name = "FrmAnexos";
            this.Text = "Reporte Anexos y Inventarios";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
