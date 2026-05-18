namespace Retorno360Tacna.FORMS
{
    partial class FrmReportes
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
            panelFiltros = new Panel();
            chkSinGlosa = new CheckBox();
            btnExportarExcel = new Button();
            btnGenerarPDF = new Button();
            btnConsultar = new Button();
            dtpFechaFin = new DateTimePicker();
            lblFechaFin = new Label();
            dtpFechaInicio = new DateTimePicker();
            lblFechaInicio = new Label();
            cmbCliente = new ComboBox();
            lblCliente = new Label();
            cmbRazonSocial = new ComboBox();
            lblRazonSocial = new Label();
            panelResultados = new Panel();
            dgvReporteIVA = new DataGridView();
            dgvReporteIGI = new DataGridView();
            panelGrafica = new Panel();
            btnSiguienteGrafica = new Button();
            btnAnteriorGrafica = new Button();
            lblTituloGrafica = new Label();
            panelGraficaIVA = new Panel();
            btnSiguienteGraficaIVA = new Button();
            btnAnteriorGraficaIVA = new Button();
            lblTituloGraficaIVA = new Label();
            panelResumen = new Panel();
            lblResumenInfo = new Label();
            lblProgreso = new Label();
            panelCargando = new Panel();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            panelFiltros.SuspendLayout();
            panelResultados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReporteIVA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvReporteIGI).BeginInit();
            panelGrafica.SuspendLayout();
            panelGraficaIVA.SuspendLayout();
            panelResumen.SuspendLayout();
            panelCargando.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.BackColor = Color.White;
            panelFiltros.Controls.Add(chkSinGlosa);
            panelFiltros.Controls.Add(btnExportarExcel);
            panelFiltros.Controls.Add(btnGenerarPDF);
            panelFiltros.Controls.Add(btnConsultar);
            panelFiltros.Controls.Add(dtpFechaFin);
            panelFiltros.Controls.Add(lblFechaFin);
            panelFiltros.Controls.Add(dtpFechaInicio);
            panelFiltros.Controls.Add(lblFechaInicio);
            panelFiltros.Controls.Add(cmbCliente);
            panelFiltros.Controls.Add(lblCliente);
            panelFiltros.Controls.Add(cmbRazonSocial);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new Padding(20);
            panelFiltros.Size = new Size(1200, 131);
            panelFiltros.TabIndex = 1;
            // 
            // chkSinGlosa
            // 
            chkSinGlosa.AutoSize = true;
            chkSinGlosa.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            chkSinGlosa.ForeColor = Color.FromArgb(52, 73, 94);
            chkSinGlosa.Location = new Point(267, 104);
            chkSinGlosa.Name = "chkSinGlosa";
            chkSinGlosa.Size = new Size(425, 21);
            chkSinGlosa.TabIndex = 9;
            chkSinGlosa.Text = "Consultar por Razon Social(solo mostrara los datos de TR_Glosa)";
            chkSinGlosa.UseVisualStyleBackColor = true;
            chkSinGlosa.CheckedChanged += chkSinGlosa_CheckedChanged;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(46, 125, 50);
            btnExportarExcel.Cursor = Cursors.Hand;
            btnExportarExcel.Enabled = false;
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Image = Properties.Resources.gdform_103694;
            btnExportarExcel.ImageAlign = ContentAlignment.MiddleRight;
            btnExportarExcel.Location = new Point(969, 42);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(154, 56);
            btnExportarExcel.TabIndex = 11;
            btnExportarExcel.Text = "Excel";
            btnExportarExcel.TextAlign = ContentAlignment.MiddleLeft;
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnGenerarPDF
            // 
            btnGenerarPDF.BackColor = Color.FromArgb(231, 76, 60);
            btnGenerarPDF.Cursor = Cursors.Hand;
            btnGenerarPDF.Enabled = false;
            btnGenerarPDF.FlatAppearance.BorderSize = 0;
            btnGenerarPDF.FlatStyle = FlatStyle.Flat;
            btnGenerarPDF.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerarPDF.ForeColor = Color.White;
            btnGenerarPDF.Image = Properties.Resources.applicationpdf_103614;
            btnGenerarPDF.ImageAlign = ContentAlignment.MiddleRight;
            btnGenerarPDF.Location = new Point(809, 42);
            btnGenerarPDF.Name = "btnGenerarPDF";
            btnGenerarPDF.Size = new Size(154, 56);
            btnGenerarPDF.TabIndex = 10;
            btnGenerarPDF.Text = "Generar PDF";
            btnGenerarPDF.TextAlign = ContentAlignment.MiddleLeft;
            btnGenerarPDF.UseVisualStyleBackColor = false;
            btnGenerarPDF.Click += btnGenerarPDF_Click;
            // 
            // btnConsultar
            // 
            btnConsultar.BackColor = Color.FromArgb(39, 174, 96);
            btnConsultar.Cursor = Cursors.Hand;
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.FlatStyle = FlatStyle.Flat;
            btnConsultar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnConsultar.ForeColor = Color.White;
            btnConsultar.Image = Properties.Resources.search_magnifying_glass_icon_192631;
            btnConsultar.ImageAlign = ContentAlignment.MiddleRight;
            btnConsultar.Location = new Point(649, 42);
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Size = new Size(154, 56);
            btnConsultar.TabIndex = 8;
            btnConsultar.Text = "Consultar";
            btnConsultar.TextAlign = ContentAlignment.MiddleLeft;
            btnConsultar.UseVisualStyleBackColor = false;
            btnConsultar.Click += btnConsultar_Click;
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Font = new Font("Segoe UI", 10F);
            dtpFechaFin.Format = DateTimePickerFormat.Short;
            dtpFechaFin.Location = new Point(463, 60);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new Size(180, 25);
            dtpFechaFin.TabIndex = 7;
            // 
            // lblFechaFin
            // 
            lblFechaFin.AutoSize = true;
            lblFechaFin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaFin.ForeColor = Color.FromArgb(52, 73, 94);
            lblFechaFin.Location = new Point(463, 35);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new Size(74, 19);
            lblFechaFin.TabIndex = 6;
            lblFechaFin.Text = "Fecha Fin:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Font = new Font("Segoe UI", 10F);
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(267, 60);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new Size(180, 25);
            dtpFechaInicio.TabIndex = 5;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.AutoSize = true;
            lblFechaInicio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaInicio.ForeColor = Color.FromArgb(52, 73, 94);
            lblFechaInicio.Location = new Point(267, 35);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new Size(91, 19);
            lblFechaInicio.TabIndex = 4;
            lblFechaInicio.Text = "Fecha Inicio:";
            // 
            // cmbCliente
            // 
            cmbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCliente.Enabled = false;
            cmbCliente.Font = new Font("Segoe UI", 10F);
            cmbCliente.FormattingEnabled = true;
            cmbCliente.Location = new Point(20, 91);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(230, 25);
            cmbCliente.TabIndex = 3;
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.ForeColor = Color.FromArgb(52, 73, 94);
            lblCliente.Location = new Point(20, 66);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(140, 19);
            lblCliente.TabIndex = 2;
            lblCliente.Text = "Cliente (Base Dato):";
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazonSocial.Font = new Font("Segoe UI", 10F);
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(20, 33);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(230, 25);
            cmbRazonSocial.TabIndex = 1;
            cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRazonSocial.ForeColor = Color.FromArgb(52, 73, 94);
            lblRazonSocial.Location = new Point(20, 8);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(98, 19);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // panelResultados
            // 
            panelResultados.Controls.Add(dgvReporteIVA);
            panelResultados.Controls.Add(dgvReporteIGI);
            panelResultados.Controls.Add(panelGrafica);
            panelResultados.Controls.Add(panelGraficaIVA);
            panelResultados.Controls.Add(panelResumen);
            panelResultados.Dock = DockStyle.Fill;
            panelResultados.Location = new Point(0, 131);
            panelResultados.Name = "panelResultados";
            panelResultados.Padding = new Padding(20);
            panelResultados.Size = new Size(1200, 519);
            panelResultados.TabIndex = 2;
            // 
            // dgvReporteIVA
            // 
            dgvReporteIVA.AllowUserToAddRows = false;
            dgvReporteIVA.AllowUserToDeleteRows = false;
            dgvReporteIVA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReporteIVA.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporteIVA.BackgroundColor = Color.White;
            dgvReporteIVA.BorderStyle = BorderStyle.None;
            dgvReporteIVA.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReporteIVA.Location = new Point(20, 165);
            dgvReporteIVA.Name = "dgvReporteIVA";
            dgvReporteIVA.ReadOnly = true;
            dgvReporteIVA.RowHeadersWidth = 51;
            dgvReporteIVA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporteIVA.Size = new Size(517, 177);
            dgvReporteIVA.TabIndex = 1;
            // 
            // dgvReporteIGI
            // 
            dgvReporteIGI.AllowUserToAddRows = false;
            dgvReporteIGI.AllowUserToDeleteRows = false;
            dgvReporteIGI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvReporteIGI.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporteIGI.BackgroundColor = Color.White;
            dgvReporteIGI.BorderStyle = BorderStyle.None;
            dgvReporteIGI.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReporteIGI.Location = new Point(20, 6);
            dgvReporteIGI.Name = "dgvReporteIGI";
            dgvReporteIGI.ReadOnly = true;
            dgvReporteIGI.RowHeadersWidth = 51;
            dgvReporteIGI.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporteIGI.Size = new Size(517, 153);
            dgvReporteIGI.TabIndex = 0;
            // 
            // panelGrafica
            // 
            panelGrafica.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelGrafica.BackColor = Color.White;
            panelGrafica.BorderStyle = BorderStyle.FixedSingle;
            panelGrafica.Controls.Add(btnSiguienteGrafica);
            panelGrafica.Controls.Add(btnAnteriorGrafica);
            panelGrafica.Controls.Add(lblTituloGrafica);
            panelGrafica.Location = new Point(543, 6);
            panelGrafica.Name = "panelGrafica";
            panelGrafica.Padding = new Padding(10);
            panelGrafica.Size = new Size(637, 336);
            panelGrafica.TabIndex = 2;
            // 
            // btnSiguienteGrafica
            // 
            btnSiguienteGrafica.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSiguienteGrafica.BackColor = Color.FromArgb(52, 152, 219);
            btnSiguienteGrafica.Cursor = Cursors.Hand;
            btnSiguienteGrafica.FlatAppearance.BorderSize = 0;
            btnSiguienteGrafica.FlatStyle = FlatStyle.Flat;
            btnSiguienteGrafica.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnSiguienteGrafica.ForeColor = Color.White;
            btnSiguienteGrafica.Location = new Point(595, 10);
            btnSiguienteGrafica.Name = "btnSiguienteGrafica";
            btnSiguienteGrafica.Size = new Size(30, 30);
            btnSiguienteGrafica.TabIndex = 2;
            btnSiguienteGrafica.Text = "›";
            btnSiguienteGrafica.UseVisualStyleBackColor = false;
            btnSiguienteGrafica.Click += btnSiguienteGrafica_Click;
            // 
            // btnAnteriorGrafica
            // 
            btnAnteriorGrafica.BackColor = Color.FromArgb(52, 152, 219);
            btnAnteriorGrafica.Cursor = Cursors.Hand;
            btnAnteriorGrafica.FlatAppearance.BorderSize = 0;
            btnAnteriorGrafica.FlatStyle = FlatStyle.Flat;
            btnAnteriorGrafica.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnAnteriorGrafica.ForeColor = Color.White;
            btnAnteriorGrafica.Location = new Point(10, 10);
            btnAnteriorGrafica.Name = "btnAnteriorGrafica";
            btnAnteriorGrafica.Size = new Size(30, 30);
            btnAnteriorGrafica.TabIndex = 1;
            btnAnteriorGrafica.Text = "‹";
            btnAnteriorGrafica.UseVisualStyleBackColor = false;
            btnAnteriorGrafica.Click += btnAnteriorGrafica_Click;
            // 
            // lblTituloGrafica
            // 
            lblTituloGrafica.Dock = DockStyle.Top;
            lblTituloGrafica.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloGrafica.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloGrafica.Location = new Point(10, 10);
            lblTituloGrafica.Name = "lblTituloGrafica";
            lblTituloGrafica.Padding = new Padding(40, 0, 40, 0);
            lblTituloGrafica.Size = new Size(615, 30);
            lblTituloGrafica.TabIndex = 0;
            lblTituloGrafica.Text = "IGI por Mes y Forma de Pago (1/2)";
            lblTituloGrafica.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelGraficaIVA
            // 
            panelGraficaIVA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelGraficaIVA.BackColor = Color.White;
            panelGraficaIVA.Controls.Add(btnSiguienteGraficaIVA);
            panelGraficaIVA.Controls.Add(btnAnteriorGraficaIVA);
            panelGraficaIVA.Controls.Add(lblTituloGraficaIVA);
            panelGraficaIVA.Location = new Point(543, 6);
            panelGraficaIVA.Name = "panelGraficaIVA";
            panelGraficaIVA.Padding = new Padding(10);
            panelGraficaIVA.Size = new Size(637, 336);
            panelGraficaIVA.TabIndex = 3;
            panelGraficaIVA.Visible = false;
            // 
            // btnSiguienteGraficaIVA
            // 
            btnSiguienteGraficaIVA.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSiguienteGraficaIVA.BackColor = Color.FromArgb(52, 152, 219);
            btnSiguienteGraficaIVA.Cursor = Cursors.Hand;
            btnSiguienteGraficaIVA.FlatAppearance.BorderSize = 0;
            btnSiguienteGraficaIVA.FlatStyle = FlatStyle.Flat;
            btnSiguienteGraficaIVA.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnSiguienteGraficaIVA.ForeColor = Color.White;
            btnSiguienteGraficaIVA.Location = new Point(597, 10);
            btnSiguienteGraficaIVA.Name = "btnSiguienteGraficaIVA";
            btnSiguienteGraficaIVA.Size = new Size(30, 30);
            btnSiguienteGraficaIVA.TabIndex = 2;
            btnSiguienteGraficaIVA.Text = "›";
            btnSiguienteGraficaIVA.UseVisualStyleBackColor = false;
            btnSiguienteGraficaIVA.Click += btnSiguienteGrafica_Click;
            // 
            // btnAnteriorGraficaIVA
            // 
            btnAnteriorGraficaIVA.BackColor = Color.FromArgb(52, 152, 219);
            btnAnteriorGraficaIVA.Cursor = Cursors.Hand;
            btnAnteriorGraficaIVA.FlatAppearance.BorderSize = 0;
            btnAnteriorGraficaIVA.FlatStyle = FlatStyle.Flat;
            btnAnteriorGraficaIVA.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnAnteriorGraficaIVA.ForeColor = Color.White;
            btnAnteriorGraficaIVA.Location = new Point(10, 10);
            btnAnteriorGraficaIVA.Name = "btnAnteriorGraficaIVA";
            btnAnteriorGraficaIVA.Size = new Size(30, 30);
            btnAnteriorGraficaIVA.TabIndex = 1;
            btnAnteriorGraficaIVA.Text = "‹";
            btnAnteriorGraficaIVA.UseVisualStyleBackColor = false;
            btnAnteriorGraficaIVA.Click += btnAnteriorGrafica_Click;
            // 
            // lblTituloGraficaIVA
            // 
            lblTituloGraficaIVA.Dock = DockStyle.Top;
            lblTituloGraficaIVA.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloGraficaIVA.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloGraficaIVA.Location = new Point(10, 10);
            lblTituloGraficaIVA.Name = "lblTituloGraficaIVA";
            lblTituloGraficaIVA.Padding = new Padding(40, 0, 40, 0);
            lblTituloGraficaIVA.Size = new Size(617, 30);
            lblTituloGraficaIVA.TabIndex = 0;
            lblTituloGraficaIVA.Text = "IVA por Mes y Forma de Pago (2/2)";
            lblTituloGraficaIVA.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelResumen
            // 
            panelResumen.BackColor = Color.FromArgb(236, 240, 241);
            panelResumen.Controls.Add(lblResumenInfo);
            panelResumen.Controls.Add(lblProgreso);
            panelResumen.Dock = DockStyle.Bottom;
            panelResumen.Location = new Point(20, 348);
            panelResumen.Name = "panelResumen";
            panelResumen.Padding = new Padding(10);
            panelResumen.Size = new Size(1160, 151);
            panelResumen.TabIndex = 1;
            // 
            // lblResumenInfo
            // 
            lblResumenInfo.Dock = DockStyle.Fill;
            lblResumenInfo.Font = new Font("Consolas", 9F, FontStyle.Bold);
            lblResumenInfo.ForeColor = Color.FromArgb(52, 73, 94);
            lblResumenInfo.Location = new Point(10, 10);
            lblResumenInfo.Name = "lblResumenInfo";
            lblResumenInfo.Size = new Size(1140, 111);
            lblResumenInfo.TabIndex = 0;
            lblResumenInfo.Text = "Seleccione los filtros y presione Consultar";
            // 
            // lblProgreso
            // 
            lblProgreso.Dock = DockStyle.Bottom;
            lblProgreso.Font = new Font("Segoe UI", 9F);
            lblProgreso.ForeColor = Color.FromArgb(127, 140, 141);
            lblProgreso.Location = new Point(10, 121);
            lblProgreso.Name = "lblProgreso";
            lblProgreso.Size = new Size(1140, 20);
            lblProgreso.TabIndex = 1;
            lblProgreso.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelCargando
            // 
            panelCargando.BackColor = Color.FromArgb(236, 240, 241);
            panelCargando.Controls.Add(lblCargando);
            panelCargando.Controls.Add(progressBarCargando);
            panelCargando.Location = new Point(400, 250);
            panelCargando.Name = "panelCargando";
            panelCargando.Size = new Size(400, 150);
            panelCargando.TabIndex = 10;
            panelCargando.Visible = false;
            // 
            // lblCargando
            // 
            lblCargando.Dock = DockStyle.Bottom;
            lblCargando.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCargando.ForeColor = Color.FromArgb(52, 73, 94);
            lblCargando.Location = new Point(0, 80);
            lblCargando.Name = "lblCargando";
            lblCargando.Size = new Size(400, 70);
            lblCargando.TabIndex = 1;
            lblCargando.Text = "Cargando datos...\r\nPor favor espere";
            lblCargando.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBarCargando
            // 
            progressBarCargando.Location = new Point(50, 30);
            progressBarCargando.MarqueeAnimationSpeed = 30;
            progressBarCargando.Name = "progressBarCargando";
            progressBarCargando.Size = new Size(300, 30);
            progressBarCargando.Style = ProgressBarStyle.Marquee;
            progressBarCargando.TabIndex = 0;
            // 
            // FrmReportes
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(236, 240, 241);
            ClientSize = new Size(1200, 650);
            Controls.Add(panelCargando);
            Controls.Add(panelResultados);
            Controls.Add(panelFiltros);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmReportes";
            Text = "Reporte IGI Pagado";
            Load += FrmReportes_Load;
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelResultados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvReporteIVA).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvReporteIGI).EndInit();
            panelGrafica.ResumeLayout(false);
            panelGraficaIVA.ResumeLayout(false);
            panelResumen.ResumeLayout(false);
            panelCargando.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panelFiltros;
        private ComboBox cmbRazonSocial;
        private Label lblRazonSocial;
        private ComboBox cmbCliente;
        private Label lblCliente;
        private DateTimePicker dtpFechaInicio;
        private Label lblFechaInicio;
        private DateTimePicker dtpFechaFin;
        private Label lblFechaFin;
        private Button btnConsultar;
        private Button btnGenerarPDF;
        private Button btnExportarExcel;
        private CheckBox chkSinGlosa;
        private Panel panelResultados;
        private DataGridView dgvReporteIGI;
        private DataGridView dgvReporteIVA;
        private Panel panelResumen;
        private Label lblResumenInfo;
        private Label lblProgreso;
        private Panel panelGrafica;
        private Button btnSiguienteGrafica;
        private Button btnAnteriorGrafica;
        private Label lblTituloGrafica;
        private Panel panelGraficaIVA;
        private Button btnSiguienteGraficaIVA;
        private Button btnAnteriorGraficaIVA;
        private Label lblTituloGraficaIVA;
        private Panel panelCargando;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
    }
}
