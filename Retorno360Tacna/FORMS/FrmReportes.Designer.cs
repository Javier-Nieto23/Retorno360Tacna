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
            dgvReporte = new DataGridView();
            panelGrafica = new Panel();
            lblTituloGrafica = new Label();
            panelResumen = new Panel();
            lblResumenInfo = new Label();
            lblProgreso = new Label();
            panelCargando = new Panel();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            panelFiltros.SuspendLayout();
            panelResultados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReporte).BeginInit();
            panelGrafica.SuspendLayout();
            panelResumen.SuspendLayout();
            panelCargando.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.BackColor = Color.White;
            panelFiltros.Controls.Add(chkSinGlosa);
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
            chkSinGlosa.Location = new Point(390, 95);
            chkSinGlosa.Name = "chkSinGlosa";
            chkSinGlosa.Size = new Size(425, 21);
            chkSinGlosa.TabIndex = 9;
            chkSinGlosa.Text = "Consultar por Razon Social(solo mostrara los datos de TR_Glosa)";
            chkSinGlosa.UseVisualStyleBackColor = true;
            chkSinGlosa.CheckedChanged += chkSinGlosa_CheckedChanged;
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
            btnGenerarPDF.Image = Properties.Resources.PDF_icon_icons_com_52413;
            btnGenerarPDF.ImageAlign = ContentAlignment.MiddleRight;
            btnGenerarPDF.Location = new Point(1140, 2);
            btnGenerarPDF.Name = "btnGenerarPDF";
            btnGenerarPDF.Size = new Size(177, 56);
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
            btnConsultar.Image = Properties.Resources.Analysis_36777;
            btnConsultar.ImageAlign = ContentAlignment.MiddleRight;
            btnConsultar.Location = new Point(957, 2);
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Size = new Size(177, 56);
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
            dtpFechaFin.Location = new Point(756, 60);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new Size(180, 25);
            dtpFechaFin.TabIndex = 7;
            // 
            // lblFechaFin
            // 
            lblFechaFin.AutoSize = true;
            lblFechaFin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaFin.ForeColor = Color.FromArgb(52, 73, 94);
            lblFechaFin.Location = new Point(756, 35);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new Size(74, 19);
            lblFechaFin.TabIndex = 6;
            lblFechaFin.Text = "Fecha Fin:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Font = new Font("Segoe UI", 10F);
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(560, 60);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new Size(180, 25);
            dtpFechaInicio.TabIndex = 5;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.AutoSize = true;
            lblFechaInicio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaInicio.ForeColor = Color.FromArgb(52, 73, 94);
            lblFechaInicio.Location = new Point(560, 35);
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
            cmbCliente.Location = new Point(290, 60);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(230, 25);
            cmbCliente.TabIndex = 3;
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.ForeColor = Color.FromArgb(52, 73, 94);
            lblCliente.Location = new Point(290, 35);
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
            cmbRazonSocial.Location = new Point(20, 60);
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
            lblRazonSocial.Location = new Point(20, 35);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(98, 19);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // panelResultados
            // 
            panelResultados.Controls.Add(dgvReporte);
            panelResultados.Controls.Add(panelGrafica);
            panelResultados.Controls.Add(panelResumen);
            panelResultados.Dock = DockStyle.Fill;
            panelResultados.Location = new Point(0, 131);
            panelResultados.Name = "panelResultados";
            panelResultados.Padding = new Padding(20);
            panelResultados.Size = new Size(1200, 519);
            panelResultados.TabIndex = 2;
            // 
            // dgvReporte
            // 
            dgvReporte.AllowUserToAddRows = false;
            dgvReporte.AllowUserToDeleteRows = false;
            dgvReporte.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporte.BackgroundColor = Color.White;
            dgvReporte.BorderStyle = BorderStyle.None;
            dgvReporte.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReporte.Location = new Point(20, 20);
            dgvReporte.Name = "dgvReporte";
            dgvReporte.ReadOnly = true;
            dgvReporte.RowHeadersWidth = 51;
            dgvReporte.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporte.Size = new Size(750, 339);
            dgvReporte.TabIndex = 0;
            // 
            // panelGrafica
            // 
            panelGrafica.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelGrafica.BackColor = Color.White;
            panelGrafica.Controls.Add(lblTituloGrafica);
            panelGrafica.Location = new Point(780, 20);
            panelGrafica.Name = "panelGrafica";
            panelGrafica.Padding = new Padding(10);
            panelGrafica.Size = new Size(400, 339);
            panelGrafica.TabIndex = 2;
            // 
            // lblTituloGrafica
            // 
            lblTituloGrafica.Dock = DockStyle.Top;
            lblTituloGrafica.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloGrafica.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloGrafica.Location = new Point(10, 10);
            lblTituloGrafica.Name = "lblTituloGrafica";
            lblTituloGrafica.Size = new Size(380, 30);
            lblTituloGrafica.TabIndex = 0;
            lblTituloGrafica.Text = "IGI Pagado vs Calculado";
            lblTituloGrafica.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelResumen
            // 
            panelResumen.BackColor = Color.FromArgb(236, 240, 241);
            panelResumen.Controls.Add(lblResumenInfo);
            panelResumen.Controls.Add(lblProgreso);
            panelResumen.Dock = DockStyle.Bottom;
            panelResumen.Location = new Point(0, 439);
            panelResumen.Name = "panelResumen";
            panelResumen.Padding = new Padding(10);
            panelResumen.Size = new Size(1200, 80);
            panelResumen.TabIndex = 1;
            // 
            // lblResumenInfo
            // 
            lblResumenInfo.Dock = DockStyle.Fill;
            lblResumenInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumenInfo.ForeColor = Color.FromArgb(52, 73, 94);
            lblResumenInfo.Location = new Point(10, 10);
            lblResumenInfo.Name = "lblResumenInfo";
            lblResumenInfo.Size = new Size(1140, 40);
            lblResumenInfo.TabIndex = 0;
            lblResumenInfo.Text = "Seleccione los filtros y presione Consultar";
            lblResumenInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProgreso
            // 
            lblProgreso.Dock = DockStyle.Bottom;
            lblProgreso.Font = new Font("Segoe UI", 9F);
            lblProgreso.ForeColor = Color.FromArgb(127, 140, 141);
            lblProgreso.Location = new Point(10, 50);
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
            ((System.ComponentModel.ISupportInitialize)dgvReporte).EndInit();
            panelGrafica.ResumeLayout(false);
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
        private CheckBox chkSinGlosa;
        private Panel panelResultados;
        private DataGridView dgvReporte;
        private Panel panelResumen;
        private Label lblResumenInfo;
        private Label lblProgreso;
        private Panel panelGrafica;
        private Label lblTituloGrafica;
        private Panel panelCargando;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
    }
}
