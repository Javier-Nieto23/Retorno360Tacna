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
        private System.Windows.Forms.CheckBox chkUsarPerfil;

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
            panelFiltros = new Panel();
            btnExportarExcel = new Button();
            btnGuardarPortal = new Button();
            btnGenerar = new Button();
            dtpFin = new DateTimePicker();
            dtpInicio = new DateTimePicker();
            cmbBase = new ComboBox();
            cmbRazon = new ComboBox();
            lblFin = new Label();
            lblInicio = new Label();
            lblBase = new Label();
            lblRazon = new Label();
            chkUsarPerfil = new CheckBox();
            panelResultados = new Panel();
            dgvPreview = new DataGridView();
            lblResumen = new Label();
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
            panelFiltros.Controls.Add(chkUsarPerfil);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new Padding(20);
            panelFiltros.Size = new Size(1180, 120);
            panelFiltros.TabIndex = 0;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(0, 186, 65);
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Location = new Point(952, 32);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(160, 50);
            btnExportarExcel.TabIndex = 10;
            btnExportarExcel.Text = "Exportar Excel";
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnGuardarPortal
            // 
            btnGuardarPortal.BackColor = Color.FromArgb(0, 42, 196);
            btnGuardarPortal.FlatAppearance.BorderSize = 0;
            btnGuardarPortal.FlatStyle = FlatStyle.Flat;
            btnGuardarPortal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGuardarPortal.ForeColor = Color.White;
            btnGuardarPortal.Location = new Point(786, 32);
            btnGuardarPortal.Name = "btnGuardarPortal";
            btnGuardarPortal.Size = new Size(160, 50);
            btnGuardarPortal.TabIndex = 9;
            btnGuardarPortal.Text = "Guardar en Portal";
            btnGuardarPortal.UseVisualStyleBackColor = false;
            btnGuardarPortal.Click += btnGuardarPortal_Click;
            // 
            // btnGenerar
            // 
            btnGenerar.BackColor = Color.FromArgb(0, 196, 45);
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.FlatStyle = FlatStyle.Flat;
            btnGenerar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerar.ForeColor = Color.White;
            btnGenerar.Location = new Point(620, 32);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new Size(160, 50);
            btnGenerar.TabIndex = 8;
            btnGenerar.Text = "Generar Preview";
            btnGenerar.UseVisualStyleBackColor = false;
            btnGenerar.Click += btnGenerar_Click;
            // 
            // dtpFin
            // 
            dtpFin.Format = DateTimePickerFormat.Short;
            dtpFin.Location = new Point(408, 65);
            dtpFin.Name = "dtpFin";
            dtpFin.Size = new Size(180, 23);
            dtpFin.TabIndex = 7;
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(408, 28);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new Size(180, 23);
            dtpInicio.TabIndex = 6;
            // 
            // cmbBase
            // 
            cmbBase.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBase.FormattingEnabled = true;
            cmbBase.Location = new Point(110, 65);
            cmbBase.Name = "cmbBase";
            cmbBase.Size = new Size(260, 23);
            cmbBase.TabIndex = 5;
            // 
            // cmbRazon
            // 
            cmbRazon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazon.FormattingEnabled = true;
            cmbRazon.Location = new Point(110, 28);
            cmbRazon.Name = "cmbRazon";
            cmbRazon.Size = new Size(260, 23);
            cmbRazon.TabIndex = 4;
            cmbRazon.SelectedIndexChanged += cmbRazon_SelectedIndexChanged;
            // 
            // lblFin
            // 
            lblFin.AutoSize = true;
            lblFin.Location = new Point(386, 69);
            lblFin.Name = "lblFin";
            lblFin.Size = new Size(23, 15);
            lblFin.TabIndex = 3;
            lblFin.Text = "Fin";
            // 
            // lblInicio
            // 
            lblInicio.AutoSize = true;
            lblInicio.Location = new Point(370, 32);
            lblInicio.Name = "lblInicio";
            lblInicio.Size = new Size(36, 15);
            lblInicio.TabIndex = 2;
            lblInicio.Text = "Inicio";
            // 
            // lblBase
            // 
            lblBase.AutoSize = true;
            lblBase.Location = new Point(23, 69);
            lblBase.Name = "lblBase";
            lblBase.Size = new Size(79, 15);
            lblBase.TabIndex = 1;
            lblBase.Text = "Base de datos";
            // 
            // lblRazon
            // 
            lblRazon.AutoSize = true;
            lblRazon.Location = new Point(23, 32);
            lblRazon.Name = "lblRazon";
            lblRazon.Size = new Size(72, 15);
            lblRazon.TabIndex = 0;
            lblRazon.Text = "Razón social";
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Font = new Font("Segoe UI", 9.5F);
            chkUsarPerfil.Location = new Point(110, 94);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(186, 21);
            chkUsarPerfil.TabIndex = 11;
            chkUsarPerfil.Text = "Usar empresas de mi perfil";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            chkUsarPerfil.CheckedChanged += chkUsarPerfil_CheckedChanged;
            // 
            // panelResultados
            // 
            panelResultados.Controls.Add(dgvPreview);
            panelResultados.Controls.Add(lblResumen);
            panelResultados.Dock = DockStyle.Fill;
            panelResultados.Location = new Point(0, 120);
            panelResultados.Name = "panelResultados";
            panelResultados.Padding = new Padding(20);
            panelResultados.Size = new Size(1180, 600);
            panelResultados.TabIndex = 1;
            // 
            // dgvPreview
            // 
            dgvPreview.AllowUserToAddRows = false;
            dgvPreview.AllowUserToDeleteRows = false;
            dgvPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPreview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPreview.Location = new Point(20, 58);
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.RowHeadersWidth = 51;
            dgvPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPreview.Size = new Size(1140, 522);
            dgvPreview.TabIndex = 1;
            // 
            // lblResumen
            // 
            lblResumen.Dock = DockStyle.Top;
            lblResumen.Location = new Point(20, 20);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(1140, 28);
            lblResumen.TabIndex = 0;
            lblResumen.Text = "Seleccione filtros y genere el preview.";
            lblResumen.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FrmCumplimiento
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1180, 720);
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
