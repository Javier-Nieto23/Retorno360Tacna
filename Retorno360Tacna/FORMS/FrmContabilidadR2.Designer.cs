namespace Retorno360Tacna.FORMS
{
    partial class FrmContabilidadR2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelFiltros = new Panel();
            btnProcesar = new Button();
            txtColumna = new TextBox();
            lblColumna = new Label();
            cmbAnio = new ComboBox();
            lblAnio = new Label();
            cmbEmpresa = new ComboBox();
            lblEmpresa = new Label();
            cmbRazonSocial = new ComboBox();
            lblRazonSocial = new Label();
            lblDescripcion = new Label();
            lblTitulo = new Label();
            panelResumen = new Panel();
            lblResumen = new Label();
            dgvResultados = new DataGridView();
            panelFiltros.SuspendLayout();
            panelResumen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelFiltros.BackColor = Color.White;
            panelFiltros.BorderStyle = BorderStyle.FixedSingle;
            panelFiltros.Controls.Add(btnProcesar);
            panelFiltros.Controls.Add(txtColumna);
            panelFiltros.Controls.Add(lblColumna);
            panelFiltros.Controls.Add(cmbAnio);
            panelFiltros.Controls.Add(lblAnio);
            panelFiltros.Controls.Add(cmbEmpresa);
            panelFiltros.Controls.Add(lblEmpresa);
            panelFiltros.Controls.Add(cmbRazonSocial);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Controls.Add(lblDescripcion);
            panelFiltros.Controls.Add(lblTitulo);
            panelFiltros.Location = new Point(20, 20);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Size = new Size(1160, 190);
            panelFiltros.TabIndex = 0;
            // 
            // btnProcesar
            // 
            btnProcesar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProcesar.BackColor = Color.FromArgb(39, 174, 96);
            btnProcesar.FlatAppearance.BorderSize = 0;
            btnProcesar.FlatStyle = FlatStyle.Flat;
            btnProcesar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnProcesar.ForeColor = Color.White;
            btnProcesar.Location = new Point(962, 118);
            btnProcesar.Name = "btnProcesar";
            btnProcesar.Size = new Size(170, 40);
            btnProcesar.TabIndex = 10;
            btnProcesar.Text = "Generar Excel";
            btnProcesar.UseVisualStyleBackColor = false;
            btnProcesar.Click += btnProcesar_Click;
            // 
            // txtColumna
            // 
            txtColumna.Location = new Point(602, 126);
            txtColumna.Name = "txtColumna";
            txtColumna.PlaceholderText = "Nombre exacto de la columna";
            txtColumna.Size = new Size(310, 23);
            txtColumna.TabIndex = 9;
            // 
            // lblColumna
            // 
            lblColumna.AutoSize = true;
            lblColumna.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblColumna.Location = new Point(602, 104);
            lblColumna.Name = "lblColumna";
            lblColumna.Size = new Size(123, 17);
            lblColumna.TabIndex = 8;
            lblColumna.Text = "Columna a analizar";
            // 
            // cmbAnio
            // 
            cmbAnio.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAnio.FormattingEnabled = true;
            cmbAnio.Location = new Point(397, 126);
            cmbAnio.Name = "cmbAnio";
            cmbAnio.Size = new Size(170, 23);
            cmbAnio.TabIndex = 7;
            // 
            // lblAnio
            // 
            lblAnio.AutoSize = true;
            lblAnio.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblAnio.Location = new Point(397, 104);
            lblAnio.Name = "lblAnio";
            lblAnio.Size = new Size(32, 17);
            lblAnio.TabIndex = 6;
            lblAnio.Text = "Año";
            // 
            // cmbEmpresa
            // 
            cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmpresa.FormattingEnabled = true;
            cmbEmpresa.Location = new Point(206, 126);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(170, 23);
            cmbEmpresa.TabIndex = 5;
            cmbEmpresa.SelectedIndexChanged += cmbEmpresa_SelectedIndexChanged;
            // 
            // lblEmpresa
            // 
            lblEmpresa.AutoSize = true;
            lblEmpresa.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblEmpresa.Location = new Point(206, 104);
            lblEmpresa.Name = "lblEmpresa";
            lblEmpresa.Size = new Size(59, 17);
            lblEmpresa.TabIndex = 4;
            lblEmpresa.Text = "Empresa";
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(18, 126);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(170, 23);
            cmbRazonSocial.TabIndex = 3;
            cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblRazonSocial.Location = new Point(18, 104);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(84, 17);
            lblRazonSocial.TabIndex = 2;
            lblRazonSocial.Text = "Razón social";
            // 
            // lblDescripcion
            // 
            lblDescripcion.AutoSize = true;
            lblDescripcion.ForeColor = Color.DimGray;
            lblDescripcion.Location = new Point(18, 52);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new Size(727, 15);
            lblDescripcion.TabIndex = 1;
            lblDescripcion.Text = "Selecciona la razón social, empresa y año disponibles en R2. Luego indica la columna exacta del Excel para sumar sus valores y generar un consolidado local.";
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(18, 18);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(251, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Consolidado de Contabilidad";
            // 
            // panelResumen
            // 
            panelResumen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelResumen.BackColor = Color.FromArgb(245, 247, 250);
            panelResumen.BorderStyle = BorderStyle.FixedSingle;
            panelResumen.Controls.Add(lblResumen);
            panelResumen.Location = new Point(20, 225);
            panelResumen.Name = "panelResumen";
            panelResumen.Size = new Size(1160, 55);
            panelResumen.TabIndex = 1;
            // 
            // lblResumen
            // 
            lblResumen.AutoSize = true;
            lblResumen.Font = new Font("Segoe UI", 10F);
            lblResumen.ForeColor = Color.FromArgb(52, 73, 94);
            lblResumen.Location = new Point(18, 17);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(312, 19);
            lblResumen.TabIndex = 0;
            lblResumen.Text = "Seleccione filtros y genere el archivo consolidado.";
            // 
            // dgvResultados
            // 
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.AllowUserToDeleteRows = false;
            dgvResultados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResultados.BackgroundColor = Color.White;
            dgvResultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResultados.Location = new Point(20, 296);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.Size = new Size(1160, 394);
            dgvResultados.TabIndex = 2;
            // 
            // FrmContabilidadR2
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(236, 240, 245);
            ClientSize = new Size(1200, 710);
            Controls.Add(dgvResultados);
            Controls.Add(panelResumen);
            Controls.Add(panelFiltros);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmContabilidadR2";
            Text = "FrmContabilidadR2";
            Load += FrmContabilidadR2_Load;
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelResumen.ResumeLayout(false);
            panelResumen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelFiltros;
        private Button btnProcesar;
        private TextBox txtColumna;
        private Label lblColumna;
        private ComboBox cmbAnio;
        private Label lblAnio;
        private ComboBox cmbEmpresa;
        private Label lblEmpresa;
        private ComboBox cmbRazonSocial;
        private Label lblRazonSocial;
        private Label lblDescripcion;
        private Label lblTitulo;
        private Panel panelResumen;
        private Label lblResumen;
        private DataGridView dgvResultados;
    }
}
