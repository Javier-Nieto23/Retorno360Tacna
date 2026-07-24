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
            cboColumnas = new ComboBox();
            CboHojas = new ComboBox();
            BtnAnalizarExcel = new Button();
            btnProcesar = new Button();
            cmbAnio = new ComboBox();
            lblAnio = new Label();
            cmbEmpresa = new ComboBox();
            lblEmpresa = new Label();
            cmbRazonSocial = new ComboBox();
            lblRazonSocial = new Label();
            lblColumnasDetectadas = new Label();
            lblHojasExcel = new Label();
            lblDescripcion = new Label();
            lblTitulo = new Label();
            lblEstadoExcel = new Label();
            panelResumen = new Panel();
            lblResumen = new Label();
            dgvResultados = new DataGridView();
            panelCargando = new Panel();
            lblTituloCargaExcel = new Label();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            panelFiltros.SuspendLayout();
            panelResumen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            panelCargando.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelFiltros.BackColor = Color.White;
            panelFiltros.BorderStyle = BorderStyle.FixedSingle;
            panelFiltros.Controls.Add(cboColumnas);
            panelFiltros.Controls.Add(CboHojas);
            panelFiltros.Controls.Add(BtnAnalizarExcel);
            panelFiltros.Controls.Add(btnProcesar);
            panelFiltros.Controls.Add(cmbAnio);
            panelFiltros.Controls.Add(lblAnio);
            panelFiltros.Controls.Add(cmbEmpresa);
            panelFiltros.Controls.Add(lblEmpresa);
            panelFiltros.Controls.Add(cmbRazonSocial);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Controls.Add(lblColumnasDetectadas);
            panelFiltros.Controls.Add(lblHojasExcel);
            panelFiltros.Controls.Add(lblDescripcion);
            panelFiltros.Controls.Add(lblTitulo);
            panelFiltros.Controls.Add(lblEstadoExcel);
            panelFiltros.Location = new Point(20, 20);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Size = new Size(1160, 225);
            panelFiltros.TabIndex = 0;
            // 
            // cboColumnas
            // 
            cboColumnas.DropDownStyle = ComboBoxStyle.DropDownList;
            cboColumnas.FormattingEnabled = true;
            cboColumnas.Location = new Point(596, 157);
            cboColumnas.Name = "cboColumnas";
            cboColumnas.Size = new Size(305, 23);
            cboColumnas.TabIndex = 14;
            // 
            // CboHojas
            // 
            CboHojas.DropDownStyle = ComboBoxStyle.DropDownList;
            CboHojas.FormattingEnabled = true;
            CboHojas.Location = new Point(596, 103);
            CboHojas.Name = "CboHojas";
            CboHojas.Size = new Size(305, 23);
            CboHojas.TabIndex = 13;
            CboHojas.SelectedIndexChanged += CboHojas_SelectedIndexChanged;
            // 
            // BtnAnalizarExcel
            // 
            BtnAnalizarExcel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnAnalizarExcel.BackColor = Color.FromArgb(52, 152, 219);
            BtnAnalizarExcel.FlatAppearance.BorderSize = 0;
            BtnAnalizarExcel.FlatStyle = FlatStyle.Flat;
            BtnAnalizarExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            BtnAnalizarExcel.ForeColor = Color.White;
            BtnAnalizarExcel.Location = new Point(962, 72);
            BtnAnalizarExcel.Name = "BtnAnalizarExcel";
            BtnAnalizarExcel.Size = new Size(170, 40);
            BtnAnalizarExcel.TabIndex = 11;
            BtnAnalizarExcel.Text = "Analizar Excel";
            BtnAnalizarExcel.UseVisualStyleBackColor = false;
            BtnAnalizarExcel.Click += BtnAnalizarExcel_Click;
            // 
            // btnProcesar
            // 
            btnProcesar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProcesar.BackColor = Color.FromArgb(39, 174, 96);
            btnProcesar.FlatAppearance.BorderSize = 0;
            btnProcesar.FlatStyle = FlatStyle.Flat;
            btnProcesar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnProcesar.ForeColor = Color.White;
            btnProcesar.Location = new Point(962, 140);
            btnProcesar.Name = "btnProcesar";
            btnProcesar.Size = new Size(170, 40);
            btnProcesar.TabIndex = 10;
            btnProcesar.Text = "Generar Excel";
            btnProcesar.UseVisualStyleBackColor = false;
            btnProcesar.Click += btnProcesar_Click;
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
            lblAnio.Size = new Size(33, 17);
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
            lblEmpresa.Size = new Size(60, 17);
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
            // lblColumnasDetectadas
            // 
            lblColumnasDetectadas.AutoSize = true;
            lblColumnasDetectadas.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblColumnasDetectadas.Location = new Point(596, 135);
            lblColumnasDetectadas.Name = "lblColumnasDetectadas";
            lblColumnasDetectadas.Size = new Size(139, 17);
            lblColumnasDetectadas.TabIndex = 16;
            lblColumnasDetectadas.Text = "Columnas detectadas";
            // 
            // lblHojasExcel
            // 
            lblHojasExcel.AutoSize = true;
            lblHojasExcel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblHojasExcel.Location = new Point(596, 81);
            lblHojasExcel.Name = "lblHojasExcel";
            lblHojasExcel.Size = new Size(78, 17);
            lblHojasExcel.TabIndex = 15;
            lblHojasExcel.Text = "Hojas Excel";
            // 
            // lblDescripcion
            // 
            lblDescripcion.AutoSize = true;
            lblDescripcion.ForeColor = Color.DimGray;
            lblDescripcion.Location = new Point(18, 52);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new Size(911, 15);
            lblDescripcion.TabIndex = 1;
            lblDescripcion.Text = "Selecciona la razón social, empresa y año disponibles en R2. Después analiza un layout Excel para cargar hojas y columnas, o captura manualmente la columna a consolidar.";
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(18, 18);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(270, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Consolidado de Contabilidad";
            // 
            // lblEstadoExcel
            // 
            lblEstadoExcel.AutoEllipsis = true;
            lblEstadoExcel.ForeColor = Color.FromArgb(52, 73, 94);
            lblEstadoExcel.Location = new Point(18, 81);
            lblEstadoExcel.Name = "lblEstadoExcel";
            lblEstadoExcel.Size = new Size(549, 17);
            lblEstadoExcel.TabIndex = 17;
            lblEstadoExcel.Text = "Archivo Excel: no seleccionado.";
            // 
            // panelResumen
            // 
            panelResumen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelResumen.BackColor = Color.FromArgb(245, 247, 250);
            panelResumen.BorderStyle = BorderStyle.FixedSingle;
            panelResumen.Controls.Add(lblResumen);
            panelResumen.Location = new Point(20, 260);
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
            lblResumen.Size = new Size(308, 19);
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
            dgvResultados.Location = new Point(20, 331);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.Size = new Size(1160, 359);
            dgvResultados.TabIndex = 2;
            // 
            // panelCargando
            // 
            panelCargando.BackColor = Color.White;
            panelCargando.BorderStyle = BorderStyle.FixedSingle;
            panelCargando.Controls.Add(lblCargando);
            panelCargando.Controls.Add(lblTituloCargaExcel);
            panelCargando.Controls.Add(progressBarCargando);
            panelCargando.Location = new Point(391, 357);
            panelCargando.Name = "panelCargando";
            panelCargando.Size = new Size(420, 170);
            panelCargando.TabIndex = 21;
            panelCargando.Visible = false;
            // 
            // lblTituloCargaExcel
            // 
            lblTituloCargaExcel.Dock = DockStyle.Top;
            lblTituloCargaExcel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTituloCargaExcel.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloCargaExcel.Location = new Point(0, 0);
            lblTituloCargaExcel.Name = "lblTituloCargaExcel";
            lblTituloCargaExcel.Size = new Size(418, 44);
            lblTituloCargaExcel.TabIndex = 2;
            lblTituloCargaExcel.Text = "Analizando archivo Excel";
            lblTituloCargaExcel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCargando
            // 
            lblCargando.Dock = DockStyle.Bottom;
            lblCargando.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCargando.ForeColor = Color.FromArgb(52, 73, 94);
            lblCargando.Location = new Point(0, 102);
            lblCargando.Name = "lblCargando";
            lblCargando.Size = new Size(418, 66);
            lblCargando.TabIndex = 1;
            lblCargando.Text = "Preparando la lectura del archivo...\r\nPor favor espere";
            lblCargando.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBarCargando
            // 
            progressBarCargando.Location = new Point(50, 56);
            progressBarCargando.MarqueeAnimationSpeed = 30;
            progressBarCargando.Name = "progressBarCargando";
            progressBarCargando.Size = new Size(320, 30);
            progressBarCargando.Style = ProgressBarStyle.Marquee;
            progressBarCargando.TabIndex = 0;
            // 
            // FrmContabilidadR2
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(236, 240, 245);
            ClientSize = new Size(1200, 710);
            Controls.Add(panelCargando);
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
            panelCargando.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelFiltros;
        private Button btnProcesar;
        private ComboBox cmbAnio;
        private Label lblAnio;
        private ComboBox cmbEmpresa;
        private Label lblEmpresa;
        private ComboBox cmbRazonSocial;
        private Label lblRazonSocial;
        private Label lblDescripcion;
        private Label lblTitulo;
        private Label lblEstadoExcel;
        private Label lblColumnasDetectadas;
        private Label lblHojasExcel;
        private Panel panelResumen;
        private Label lblResumen;
        private DataGridView dgvResultados;
        private Button BtnAnalizarExcel;
        private ComboBox CboHojas;
        private ComboBox cboColumnas;
        private Panel panelCargando;
        private Label lblTituloCargaExcel;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
    }
}
