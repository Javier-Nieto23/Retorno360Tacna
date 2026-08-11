namespace Retorno360Tacna.FORMS
{
    partial class FrmCumplimiento
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Panel panelResultados;
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
        private System.Windows.Forms.Label lblResumen;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelFiltros = new System.Windows.Forms.Panel();
            btnExportarExcel = new System.Windows.Forms.Button();
            btnGuardarPortal = new System.Windows.Forms.Button();
            btnGenerar = new System.Windows.Forms.Button();
            dtpFin = new System.Windows.Forms.DateTimePicker();
            dtpInicio = new System.Windows.Forms.DateTimePicker();
            cmbBase = new System.Windows.Forms.ComboBox();
            cmbRazon = new System.Windows.Forms.ComboBox();
            lblFin = new System.Windows.Forms.Label();
            lblInicio = new System.Windows.Forms.Label();
            lblBase = new System.Windows.Forms.Label();
            lblRazon = new System.Windows.Forms.Label();
            panelResultados = new System.Windows.Forms.Panel();
            dgvPreview = new System.Windows.Forms.DataGridView();
            lblResumen = new System.Windows.Forms.Label();
            panelFiltros.SuspendLayout();
            panelResultados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.Controls.Add(btnExportarExcel);
            panelFiltros.Controls.Add(btnGuardarPortal);
            panelFiltros.Controls.Add(btnGenerar);
            panelFiltros.Controls.Add(dtpFin);
            panelFiltros.Controls.Add(dtpInicio);
            panelFiltros.Controls.Add(cmbBase);
            panelFiltros.Controls.Add(cmbRazon);
            panelFiltros.Controls.Add(lblFin);
            panelFiltros.Controls.Add(lblInicio);
            panelFiltros.Controls.Add(lblBase);
            panelFiltros.Controls.Add(lblRazon);
            panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            panelFiltros.Location = new System.Drawing.Point(0, 0);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new System.Windows.Forms.Padding(20);
            panelFiltros.Size = new System.Drawing.Size(1180, 120);
            panelFiltros.TabIndex = 0;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.Location = new System.Drawing.Point(952, 32);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new System.Drawing.Size(160, 50);
            btnExportarExcel.TabIndex = 10;
            btnExportarExcel.Text = "Exportar Excel";
            btnExportarExcel.UseVisualStyleBackColor = true;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnGuardarPortal
            // 
            btnGuardarPortal.Location = new System.Drawing.Point(786, 32);
            btnGuardarPortal.Name = "btnGuardarPortal";
            btnGuardarPortal.Size = new System.Drawing.Size(160, 50);
            btnGuardarPortal.TabIndex = 9;
            btnGuardarPortal.Text = "Guardar en Portal";
            btnGuardarPortal.UseVisualStyleBackColor = true;
            btnGuardarPortal.Click += btnGuardarPortal_Click;
            // 
            // btnGenerar
            // 
            btnGenerar.Location = new System.Drawing.Point(620, 32);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new System.Drawing.Size(160, 50);
            btnGenerar.TabIndex = 8;
            btnGenerar.Text = "Generar Preview";
            btnGenerar.UseVisualStyleBackColor = true;
            btnGenerar.Click += btnGenerar_Click;
            // 
            // dtpFin
            // 
            dtpFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpFin.Location = new System.Drawing.Point(408, 65);
            dtpFin.Name = "dtpFin";
            dtpFin.Size = new System.Drawing.Size(180, 23);
            dtpFin.TabIndex = 7;
            // 
            // dtpInicio
            // 
            dtpInicio.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpInicio.Location = new System.Drawing.Point(408, 28);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new System.Drawing.Size(180, 23);
            dtpInicio.TabIndex = 6;
            // 
            // cmbBase
            // 
            cmbBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbBase.FormattingEnabled = true;
            cmbBase.Location = new System.Drawing.Point(110, 65);
            cmbBase.Name = "cmbBase";
            cmbBase.Size = new System.Drawing.Size(260, 23);
            cmbBase.TabIndex = 5;
            // 
            // cmbRazon
            // 
            cmbRazon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbRazon.FormattingEnabled = true;
            cmbRazon.Location = new System.Drawing.Point(110, 28);
            cmbRazon.Name = "cmbRazon";
            cmbRazon.Size = new System.Drawing.Size(260, 23);
            cmbRazon.TabIndex = 4;
            cmbRazon.SelectedIndexChanged += cmbRazon_SelectedIndexChanged;
            // 
            // lblFin
            // 
            lblFin.AutoSize = true;
            lblFin.Location = new System.Drawing.Point(386, 69);
            lblFin.Name = "lblFin";
            lblFin.Size = new System.Drawing.Size(22, 15);
            lblFin.TabIndex = 3;
            lblFin.Text = "Fin";
            // 
            // lblInicio
            // 
            lblInicio.AutoSize = true;
            lblInicio.Location = new System.Drawing.Point(370, 32);
            lblInicio.Name = "lblInicio";
            lblInicio.Size = new System.Drawing.Size(38, 15);
            lblInicio.TabIndex = 2;
            lblInicio.Text = "Inicio";
            // 
            // lblBase
            // 
            lblBase.AutoSize = true;
            lblBase.Location = new System.Drawing.Point(23, 69);
            lblBase.Name = "lblBase";
            lblBase.Size = new System.Drawing.Size(81, 15);
            lblBase.TabIndex = 1;
            lblBase.Text = "Base de datos";
            // 
            // lblRazon
            // 
            lblRazon.AutoSize = true;
            lblRazon.Location = new System.Drawing.Point(23, 32);
            lblRazon.Name = "lblRazon";
            lblRazon.Size = new System.Drawing.Size(78, 15);
            lblRazon.TabIndex = 0;
            lblRazon.Text = "Razón social";
            // 
            // panelResultados
            // 
            panelResultados.Controls.Add(dgvPreview);
            panelResultados.Controls.Add(lblResumen);
            panelResultados.Dock = System.Windows.Forms.DockStyle.Fill;
            panelResultados.Location = new System.Drawing.Point(0, 120);
            panelResultados.Name = "panelResultados";
            panelResultados.Padding = new System.Windows.Forms.Padding(20);
            panelResultados.Size = new System.Drawing.Size(1180, 600);
            panelResultados.TabIndex = 1;
            // 
            // dgvPreview
            // 
            dgvPreview.AllowUserToAddRows = false;
            dgvPreview.AllowUserToDeleteRows = false;
            dgvPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvPreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPreview.Location = new System.Drawing.Point(20, 58);
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.RowHeadersWidth = 51;
            dgvPreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvPreview.Size = new System.Drawing.Size(1140, 522);
            dgvPreview.TabIndex = 1;
            // 
            // lblResumen
            // 
            lblResumen.Dock = System.Windows.Forms.DockStyle.Top;
            lblResumen.Location = new System.Drawing.Point(20, 20);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new System.Drawing.Size(1140, 28);
            lblResumen.TabIndex = 0;
            lblResumen.Text = "Seleccione filtros y genere el preview.";
            lblResumen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FrmCumplimiento
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1180, 720);
            Controls.Add(panelResultados);
            Controls.Add(panelFiltros);
            Name = "FrmCumplimiento";
            Text = "Cumplimiento";
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelResultados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
            ResumeLayout(false);
        }
    }
}
