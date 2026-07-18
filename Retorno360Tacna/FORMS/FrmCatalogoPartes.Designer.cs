namespace Retorno360Tacna.FORMS
{
    partial class FrmCatalogoPartes
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
            lblTitulo = new Label();
            lblRazonSocial = new Label();
            cboRazonSocial = new ComboBox();
            lblBaseDatos = new Label();
            cboBaseDatos = new ComboBox();
            lblFechaInicio = new Label();
            dtpFechaInicio = new DateTimePicker();
            lblFechaFin = new Label();
            dtpFechaFin = new DateTimePicker();
            lblTipoParte = new Label();
            cboTipoParte = new ComboBox();
            btnConsultar = new Button();
            btnExportarExcel = new Button();
            btnExportarPdf = new Button();
            chkPdfTodasEmpresas = new CheckBox();
            lblTotalPartes = new Label();
            panelContenido = new Panel();
            dgvMateriaPrima = new DataGridView();
            panelGrafico = new Panel();
            chartEstatus = new LiveChartsCore.SkiaSharpView.WinForms.PieChart();
            panelBotonesGrafica = new Panel();
            btnGraficaTodos = new Button();
            btnGraficaIndividual = new Button();
            panelCargando = new Panel();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            panelFiltros.SuspendLayout();
            panelContenido.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMateriaPrima).BeginInit();
            panelGrafico.SuspendLayout();
            panelBotonesGrafica.SuspendLayout();
            panelCargando.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.BackColor = Color.FromArgb(240, 240, 240);
            panelFiltros.Controls.Add(lblTitulo);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Controls.Add(cboRazonSocial);
            panelFiltros.Controls.Add(lblBaseDatos);
            panelFiltros.Controls.Add(cboBaseDatos);
            panelFiltros.Controls.Add(lblFechaInicio);
            panelFiltros.Controls.Add(dtpFechaInicio);
            panelFiltros.Controls.Add(lblFechaFin);
            panelFiltros.Controls.Add(dtpFechaFin);
            panelFiltros.Controls.Add(lblTipoParte);
            panelFiltros.Controls.Add(cboTipoParte);
            panelFiltros.Controls.Add(btnConsultar);
            panelFiltros.Controls.Add(btnExportarExcel);
            panelFiltros.Controls.Add(btnExportarPdf);
            panelFiltros.Controls.Add(chkPdfTodasEmpresas);
            panelFiltros.Controls.Add(lblTotalPartes);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Size = new Size(1495, 147);
            panelFiltros.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(64, 64, 64);
            lblTitulo.Location = new Point(15, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(210, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Catálogo de Partes";
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 9F);
            lblRazonSocial.Location = new Point(15, 43);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(76, 15);
            lblRazonSocial.TabIndex = 1;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // cboRazonSocial
            // 
            cboRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRazonSocial.FormattingEnabled = true;
            cboRazonSocial.Location = new Point(15, 63);
            cboRazonSocial.Name = "cboRazonSocial";
            cboRazonSocial.Size = new Size(250, 23);
            cboRazonSocial.TabIndex = 2;
            cboRazonSocial.SelectedIndexChanged += cboRazonSocial_SelectedIndexChanged;
            // 
            // lblBaseDatos
            // 
            lblBaseDatos.AutoSize = true;
            lblBaseDatos.Font = new Font("Segoe UI", 9F);
            lblBaseDatos.Location = new Point(280, 43);
            lblBaseDatos.Name = "lblBaseDatos";
            lblBaseDatos.Size = new Size(83, 15);
            lblBaseDatos.TabIndex = 3;
            lblBaseDatos.Text = "Base de Datos:";
            // 
            // cboBaseDatos
            // 
            cboBaseDatos.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBaseDatos.Enabled = false;
            cboBaseDatos.FormattingEnabled = true;
            cboBaseDatos.Location = new Point(280, 63);
            cboBaseDatos.Name = "cboBaseDatos";
            cboBaseDatos.Size = new Size(200, 23);
            cboBaseDatos.TabIndex = 4;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.AutoSize = true;
            lblFechaInicio.Font = new Font("Segoe UI", 9F);
            lblFechaInicio.Location = new Point(504, 9);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new Size(73, 15);
            lblFechaInicio.TabIndex = 5;
            lblFechaInicio.Text = "Fecha Inicio:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(504, 29);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new Size(120, 23);
            dtpFechaInicio.TabIndex = 6;
            // 
            // lblFechaFin
            // 
            lblFechaFin.AutoSize = true;
            lblFechaFin.Font = new Font("Segoe UI", 9F);
            lblFechaFin.Location = new Point(639, 9);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new Size(60, 15);
            lblFechaFin.TabIndex = 7;
            lblFechaFin.Text = "Fecha Fin:";
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Format = DateTimePickerFormat.Short;
            dtpFechaFin.Location = new Point(639, 29);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new Size(120, 23);
            dtpFechaFin.TabIndex = 8;
            // 
            // lblTipoParte
            // 
            lblTipoParte.AutoSize = true;
            lblTipoParte.Font = new Font("Segoe UI", 9F);
            lblTipoParte.Location = new Point(504, 60);
            lblTipoParte.Name = "lblTipoParte";
            lblTipoParte.Size = new Size(120, 15);
            lblTipoParte.TabIndex = 12;
            lblTipoParte.Text = "Tipo N° de Parte (all):";
            // 
            // cboTipoParte
            // 
            cboTipoParte.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoParte.Enabled = false;
            cboTipoParte.FormattingEnabled = true;
            cboTipoParte.Location = new Point(504, 78);
            cboTipoParte.Name = "cboTipoParte";
            cboTipoParte.Size = new Size(255, 23);
            cboTipoParte.TabIndex = 13;
            cboTipoParte.SelectedIndexChanged += cboTipoParte_SelectedIndexChanged;
            // 
            // btnConsultar
            // 
            btnConsultar.BackColor = Color.FromArgb(41, 128, 185);
            btnConsultar.Cursor = Cursors.Hand;
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.FlatStyle = FlatStyle.Flat;
            btnConsultar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConsultar.ForeColor = Color.White;
            btnConsultar.Image = Properties.Resources.search_magnifying_glass_icon_1926311;
            btnConsultar.ImageAlign = ContentAlignment.MiddleRight;
            btnConsultar.Location = new Point(851, 16);
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Size = new Size(151, 50);
            btnConsultar.TabIndex = 9;
            btnConsultar.Text = "Consultar MP";
            btnConsultar.TextAlign = ContentAlignment.MiddleLeft;
            btnConsultar.UseVisualStyleBackColor = false;
            btnConsultar.Click += btnConsultar_Click;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(39, 174, 96);
            btnExportarExcel.Cursor = Cursors.Hand;
            btnExportarExcel.Enabled = false;
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Image = Properties.Resources.ext_xlsx_icon_176245;
            btnExportarExcel.ImageAlign = ContentAlignment.MiddleRight;
            btnExportarExcel.Location = new Point(1187, 16);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(151, 50);
            btnExportarExcel.TabIndex = 14;
            btnExportarExcel.Text = "Exportar Excel";
            btnExportarExcel.TextAlign = ContentAlignment.MiddleLeft;
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnExportarPdf
            // 
            btnExportarPdf.BackColor = Color.FromArgb(231, 76, 60);
            btnExportarPdf.Cursor = Cursors.Hand;
            btnExportarPdf.Enabled = false;
            btnExportarPdf.FlatAppearance.BorderSize = 0;
            btnExportarPdf.FlatStyle = FlatStyle.Flat;
            btnExportarPdf.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportarPdf.ForeColor = Color.White;
            btnExportarPdf.Image = Properties.Resources.applicationpdf_1036141;
            btnExportarPdf.ImageAlign = ContentAlignment.MiddleRight;
            btnExportarPdf.Location = new Point(1019, 16);
            btnExportarPdf.Name = "btnExportarPdf";
            btnExportarPdf.Size = new Size(151, 50);
            btnExportarPdf.TabIndex = 11;
            btnExportarPdf.Text = "Exportar PDF";
            btnExportarPdf.TextAlign = ContentAlignment.MiddleLeft;
            btnExportarPdf.UseVisualStyleBackColor = false;
            btnExportarPdf.Click += btnExportarPdf_Click;
            // 
            // chkPdfTodasEmpresas
            // 
            chkPdfTodasEmpresas.AutoSize = true;
            chkPdfTodasEmpresas.Location = new Point(1019, 82);
            chkPdfTodasEmpresas.Name = "chkPdfTodasEmpresas";
            chkPdfTodasEmpresas.Size = new Size(231, 19);
            chkPdfTodasEmpresas.TabIndex = 15;
            chkPdfTodasEmpresas.Text = "Consultar todas las empresas de la razón";
            chkPdfTodasEmpresas.UseVisualStyleBackColor = true;
            chkPdfTodasEmpresas.CheckedChanged += chkPdfTodasEmpresas_CheckedChanged;
            // 
            // lblTotalPartes
            // 
            lblTotalPartes.AutoSize = true;
            lblTotalPartes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalPartes.ForeColor = Color.FromArgb(41, 128, 185);
            lblTotalPartes.Location = new Point(851, 112);
            lblTotalPartes.Name = "lblTotalPartes";
            lblTotalPartes.Size = new Size(125, 19);
            lblTotalPartes.TabIndex = 10;
            lblTotalPartes.Text = "Total de partes: 0";
            // 
            // panelContenido
            // 
            panelContenido.Controls.Add(dgvMateriaPrima);
            panelContenido.Controls.Add(panelGrafico);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 147);
            panelContenido.Name = "panelContenido";
            panelContenido.Size = new Size(1495, 514);
            panelContenido.TabIndex = 2;
            // 
            // dgvMateriaPrima
            // 
            dgvMateriaPrima.AllowUserToAddRows = false;
            dgvMateriaPrima.AllowUserToDeleteRows = false;
            dgvMateriaPrima.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMateriaPrima.BackgroundColor = Color.White;
            dgvMateriaPrima.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMateriaPrima.Dock = DockStyle.Fill;
            dgvMateriaPrima.Location = new Point(400, 0);
            dgvMateriaPrima.Name = "dgvMateriaPrima";
            dgvMateriaPrima.ReadOnly = true;
            dgvMateriaPrima.RowHeadersWidth = 51;
            dgvMateriaPrima.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMateriaPrima.Size = new Size(1095, 514);
            dgvMateriaPrima.TabIndex = 1;
            // 
            // panelGrafico
            // 
            panelGrafico.BackColor = Color.White;
            panelGrafico.BorderStyle = BorderStyle.FixedSingle;
            panelGrafico.Controls.Add(chartEstatus);
            panelGrafico.Controls.Add(panelBotonesGrafica);
            panelGrafico.Dock = DockStyle.Left;
            panelGrafico.Location = new Point(0, 0);
            panelGrafico.Name = "panelGrafico";
            panelGrafico.Padding = new Padding(10);
            panelGrafico.Size = new Size(400, 514);
            panelGrafico.TabIndex = 0;
            // 
            // chartEstatus
            // 
            chartEstatus.Dock = DockStyle.Fill;
            chartEstatus.InitialRotation = 0D;
            chartEstatus.IsClockwise = true;
            chartEstatus.Location = new Point(10, 59);
            chartEstatus.MaxAngle = 360D;
            chartEstatus.MaxValue = null;
            chartEstatus.MinValue = 0D;
            chartEstatus.Name = "chartEstatus";
            chartEstatus.Size = new Size(378, 443);
            chartEstatus.TabIndex = 0;
            // 
            // panelBotonesGrafica
            // 
            panelBotonesGrafica.Controls.Add(btnGraficaTodos);
            panelBotonesGrafica.Controls.Add(btnGraficaIndividual);
            panelBotonesGrafica.Dock = DockStyle.Top;
            panelBotonesGrafica.Location = new Point(10, 10);
            panelBotonesGrafica.Name = "panelBotonesGrafica";
            panelBotonesGrafica.Size = new Size(378, 49);
            panelBotonesGrafica.TabIndex = 1;
            // 
            // btnGraficaTodos
            // 
            btnGraficaTodos.BackColor = Color.FromArgb(41, 128, 185);
            btnGraficaTodos.Cursor = Cursors.Hand;
            btnGraficaTodos.Enabled = false;
            btnGraficaTodos.FlatAppearance.BorderSize = 0;
            btnGraficaTodos.FlatStyle = FlatStyle.Flat;
            btnGraficaTodos.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnGraficaTodos.ForeColor = Color.White;
            btnGraficaTodos.Location = new Point(326, 5);
            btnGraficaTodos.Name = "btnGraficaTodos";
            btnGraficaTodos.Size = new Size(40, 38);
            btnGraficaTodos.TabIndex = 1;
            btnGraficaTodos.Text = "▶";
            btnGraficaTodos.UseVisualStyleBackColor = false;
            btnGraficaTodos.Click += btnGraficaTodos_Click;
            // 
            // btnGraficaIndividual
            // 
            btnGraficaIndividual.BackColor = Color.FromArgb(52, 152, 219);
            btnGraficaIndividual.Cursor = Cursors.Hand;
            btnGraficaIndividual.Enabled = false;
            btnGraficaIndividual.FlatAppearance.BorderSize = 0;
            btnGraficaIndividual.FlatStyle = FlatStyle.Flat;
            btnGraficaIndividual.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnGraficaIndividual.ForeColor = Color.White;
            btnGraficaIndividual.Location = new Point(22, 5);
            btnGraficaIndividual.Name = "btnGraficaIndividual";
            btnGraficaIndividual.Size = new Size(40, 38);
            btnGraficaIndividual.TabIndex = 0;
            btnGraficaIndividual.Text = "◀";
            btnGraficaIndividual.UseVisualStyleBackColor = false;
            btnGraficaIndividual.Click += btnGraficaIndividual_Click;
            // 
            // panelCargando
            // 
            panelCargando.BackColor = Color.FromArgb(250, 250, 250);
            panelCargando.BorderStyle = BorderStyle.FixedSingle;
            panelCargando.Controls.Add(lblCargando);
            panelCargando.Controls.Add(progressBarCargando);
            panelCargando.Location = new Point(450, 300);
            panelCargando.Name = "panelCargando";
            panelCargando.Size = new Size(350, 120);
            panelCargando.TabIndex = 1;
            panelCargando.Visible = false;
            // 
            // lblCargando
            // 
            lblCargando.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCargando.ForeColor = Color.FromArgb(41, 128, 185);
            lblCargando.Location = new Point(20, 20);
            lblCargando.Name = "lblCargando";
            lblCargando.Size = new Size(310, 40);
            lblCargando.TabIndex = 0;
            lblCargando.Text = "Cargando...\r\nPor favor espere";
            lblCargando.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBarCargando
            // 
            progressBarCargando.Location = new Point(20, 70);
            progressBarCargando.MarqueeAnimationSpeed = 30;
            progressBarCargando.Name = "progressBarCargando";
            progressBarCargando.Size = new Size(310, 23);
            progressBarCargando.Style = ProgressBarStyle.Marquee;
            progressBarCargando.TabIndex = 1;
            // 
            // FrmCatalogoPartes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1495, 661);
            Controls.Add(panelContenido);
            Controls.Add(panelCargando);
            Controls.Add(panelFiltros);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmCatalogoPartes";
            Text = "Catálogo de Partes";
            Load += FrmCatalogoPartes_Load;
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelContenido.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMateriaPrima).EndInit();
            panelGrafico.ResumeLayout(false);
            panelBotonesGrafica.ResumeLayout(false);
            panelCargando.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelFiltros;
        private Label lblTitulo;
        private Label lblRazonSocial;
        private ComboBox cboRazonSocial;
        private Label lblBaseDatos;
        private ComboBox cboBaseDatos;
        private Label lblFechaInicio;
        private DateTimePicker dtpFechaInicio;
        private Label lblFechaFin;
        private DateTimePicker dtpFechaFin;
        private Label lblTipoParte;
        private ComboBox cboTipoParte;
        private Button btnConsultar;
        private Button btnExportarExcel;
        private Button btnExportarPdf;
        private CheckBox chkPdfTodasEmpresas;
        private Label lblTotalPartes;
        private Panel panelContenido;
        private DataGridView dgvMateriaPrima;
        private Panel panelGrafico;
        private Panel panelBotonesGrafica;
        private Button btnGraficaTodos;
        private Button btnGraficaIndividual;
        private LiveChartsCore.SkiaSharpView.WinForms.PieChart chartEstatus;
        private Panel panelCargando;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
    }
}
