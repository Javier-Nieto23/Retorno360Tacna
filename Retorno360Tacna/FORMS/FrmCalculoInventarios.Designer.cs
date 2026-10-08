namespace Retorno360Tacna.FORMS
{
    partial class FrmCalculoInventarios
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
            lblConfigSubtitulo = new Label();
            panelConfigHeader = new Panel();
            splitCentral = new SplitContainer();
            pnlLeft = new Panel();
            pnlRight = new Panel();
            btnProcesarPreview = new Button();
            btnGuardarCalculos = new Button();
            panelConfigCuerpo = new Panel();
            label1 = new Label();
            lblMesAno = new Label();
            lblTotalGeneral = new Label();
            btnHistorialVerificacion = new Button();
            btnExportarExcel = new Button();
            btnRecalcular = new Button();
            btnVerificarPartes = new Button();
            chkUsarPerfil = new CheckBox();
            lblLblRazon = new Label();
            cmbRazonSocial = new ComboBox();
            lblLblEmpresa = new Label();
            cmbEmpresa = new ComboBox();
            chkCargarTodasRazonesEmpresas = new CheckBox();        
            lblPlantillaInfo = new Label();
            dgvRelsultados = new DataGridView();
            btnCargarInventario = new Button();
            btnAnalizarExcel = new Button();
            pnlChart = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitCentral).BeginInit();
            splitCentral.SuspendLayout();
            panelConfigCuerpo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRelsultados).BeginInit();
            SuspendLayout();
            // 
            // lblConfigSubtitulo
            // 
            lblConfigSubtitulo.AutoSize = true;
            lblConfigSubtitulo.Font = new Font("Segoe UI", 9F);
            lblConfigSubtitulo.ForeColor = Color.FromArgb(180, 210, 240);
            lblConfigSubtitulo.Location = new Point(34, 44);
            lblConfigSubtitulo.Name = "lblConfigSubtitulo";
            lblConfigSubtitulo.Size = new Size(357, 15);
            lblConfigSubtitulo.TabIndex = 0;
            lblConfigSubtitulo.Text = "Selecciona la razón social, empresa y cantidad de meses a calcular.";
            lblConfigSubtitulo.Visible = false;
            // 
            // panelConfigHeader
            // 
            panelConfigHeader.Location = new Point(0, 0);
            panelConfigHeader.Name = "panelConfigHeader";
            panelConfigHeader.Size = new Size(200, 100);
            panelConfigHeader.TabIndex = 1;
            // 
            // splitCentral
            // 
            splitCentral.Location = new Point(0, 0);
            splitCentral.Name = "splitCentral";
            splitCentral.Size = new Size(150, 100);
            splitCentral.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(200, 100);
            pnlLeft.TabIndex = 0;
            // 
            // pnlRight
            // 
            pnlRight.Location = new Point(0, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(200, 100);
            pnlRight.TabIndex = 0;
            // 
            // btnProcesarPreview
            // 
            btnProcesarPreview.Location = new Point(0, 0);
            btnProcesarPreview.Name = "btnProcesarPreview";
            btnProcesarPreview.Size = new Size(130, 30);
            btnProcesarPreview.TabIndex = 0;
            btnProcesarPreview.Text = "Procesar preview";
            // 
            // btnGuardarCalculos
            // 
            btnGuardarCalculos.Location = new Point(642, 21);
            btnGuardarCalculos.Name = "btnGuardarCalculos";
            btnGuardarCalculos.Size = new Size(120, 30);
            btnGuardarCalculos.TabIndex = 0;
            btnGuardarCalculos.Text = "Guardar";
            btnGuardarCalculos.Click += btnGuardarCalculos_Click_1;
            // 
            // panelConfigCuerpo
            // 
            panelConfigCuerpo.BackColor = Color.Transparent;
            panelConfigCuerpo.Controls.Add(label1);
            panelConfigCuerpo.Controls.Add(lblMesAno);
            panelConfigCuerpo.Controls.Add(lblTotalGeneral);
            panelConfigCuerpo.Controls.Add(btnHistorialVerificacion);
            panelConfigCuerpo.Controls.Add(btnGuardarCalculos);
            panelConfigCuerpo.Controls.Add(btnExportarExcel);
            panelConfigCuerpo.Controls.Add(btnRecalcular);
            panelConfigCuerpo.Controls.Add(btnVerificarPartes);
            panelConfigCuerpo.Controls.Add(chkUsarPerfil);
            panelConfigCuerpo.Controls.Add(lblLblRazon);
            panelConfigCuerpo.Controls.Add(cmbRazonSocial);
            panelConfigCuerpo.Controls.Add(lblLblEmpresa);
            panelConfigCuerpo.Controls.Add(cmbEmpresa);
            panelConfigCuerpo.Controls.Add(chkCargarTodasRazonesEmpresas);
            panelConfigCuerpo.Controls.Add(lblPlantillaInfo);
            panelConfigCuerpo.Controls.Add(dgvRelsultados);
            panelConfigCuerpo.Controls.Add(btnCargarInventario);
            panelConfigCuerpo.Controls.Add(btnAnalizarExcel);
            panelConfigCuerpo.Controls.Add(pnlChart);
            panelConfigCuerpo.Dock = DockStyle.Fill;
            panelConfigCuerpo.Location = new Point(0, 0);
            panelConfigCuerpo.Name = "panelConfigCuerpo";
            panelConfigCuerpo.Padding = new Padding(40, 30, 40, 30);
            panelConfigCuerpo.Size = new Size(1457, 800);
            panelConfigCuerpo.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(775, 222);
            label1.Name = "label1";
            label1.Size = new Size(118, 19);
            label1.TabIndex = 25;
            label1.Text = "Meses Cargados";
            // 
            // lblMesAno
            // 
            lblMesAno.AutoSize = true;
            lblMesAno.Location = new Point(430, 222);
            lblMesAno.Name = "lblMesAno";
            lblMesAno.Size = new Size(35, 15);
            lblMesAno.TabIndex = 23;
            lblMesAno.Text = "Mes: ";
            // 
            // lblTotalGeneral
            // 
            lblTotalGeneral.AutoSize = true;
            lblTotalGeneral.Location = new Point(63, 222);
            lblTotalGeneral.Name = "lblTotalGeneral";
            lblTotalGeneral.Size = new Size(87, 15);
            lblTotalGeneral.TabIndex = 4;
            lblTotalGeneral.Text = "Total general: 0";
            // 
            // btnHistorialVerificacion
            // 
            btnHistorialVerificacion.Location = new Point(390, 21);
            btnHistorialVerificacion.Name = "btnHistorialVerificacion";
            btnHistorialVerificacion.Size = new Size(120, 30);
            btnHistorialVerificacion.TabIndex = 3;
            btnHistorialVerificacion.Text = "Historial";
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.Location = new Point(516, 21);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(120, 30);
            btnExportarExcel.TabIndex = 2;
            btnExportarExcel.Text = "Exportar";
            btnExportarExcel.Click += btnExportarExcel_Click_1;
            // 
            // btnRecalcular
            // 
            btnRecalcular.Location = new Point(264, 21);
            btnRecalcular.Name = "btnRecalcular";
            btnRecalcular.Size = new Size(120, 30);
            btnRecalcular.TabIndex = 1;
            btnRecalcular.Text = "Recalcular";
            // 
            // btnVerificarPartes
            // 
            btnVerificarPartes.Location = new Point(12, 21);
            btnVerificarPartes.Name = "btnVerificarPartes";
            btnVerificarPartes.Size = new Size(120, 30);
            btnVerificarPartes.TabIndex = 0;
            btnVerificarPartes.Text = "Verificar partes";
            btnVerificarPartes.Click += btnVerificarPartes_Click;
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Font = new Font("Segoe UI", 9.5F);
            chkUsarPerfil.Location = new Point(31, 149);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(186, 21);
            chkUsarPerfil.TabIndex = 10;
            chkUsarPerfil.Text = "Usar empresas de mi perfil";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            chkUsarPerfil.CheckedChanged += chkUsarPerfil_CheckedChanged;
            // 
            // lblLblRazon
            // 
            lblLblRazon.AutoSize = true;
            lblLblRazon.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLblRazon.ForeColor = Color.FromArgb(50, 60, 70);
            lblLblRazon.Location = new Point(31, 83);
            lblLblRazon.Name = "lblLblRazon";
            lblLblRazon.Size = new Size(94, 19);
            lblLblRazon.TabIndex = 0;
            lblLblRazon.Text = "Razón Social";
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazonSocial.Font = new Font("Segoe UI", 10F);
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(31, 107);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(237, 25);
            cmbRazonSocial.TabIndex = 0;
            // 
            // lblLblEmpresa
            // 
            lblLblEmpresa.AutoSize = true;
            lblLblEmpresa.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLblEmpresa.ForeColor = Color.FromArgb(50, 60, 70);
            lblLblEmpresa.Location = new Point(290, 82);
            lblLblEmpresa.Name = "lblLblEmpresa";
            lblLblEmpresa.Size = new Size(66, 19);
            lblLblEmpresa.TabIndex = 1;
            lblLblEmpresa.Text = "Empresa";
            // 
            // cmbEmpresa
            // 
            cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmpresa.Font = new Font("Segoe UI", 10F);
            cmbEmpresa.FormattingEnabled = true;
            cmbEmpresa.Location = new Point(290, 107);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(217, 25);
            cmbEmpresa.TabIndex = 1;
            cmbEmpresa.SelectedIndexChanged += cmbEmpresa_SelectedIndexChanged;
            // 
            // chkCargarTodasRazonesEmpresas
            // 
            chkCargarTodasRazonesEmpresas.AutoSize = true;
            chkCargarTodasRazonesEmpresas.Font = new Font("Segoe UI", 9F);
            chkCargarTodasRazonesEmpresas.Location = new Point(775, 69);
            chkCargarTodasRazonesEmpresas.Name = "chkCargarTodasRazonesEmpresas";
            chkCargarTodasRazonesEmpresas.Size = new Size(274, 19);
            chkCargarTodasRazonesEmpresas.TabIndex = 3;
            chkCargarTodasRazonesEmpresas.Text = "Cargar inventario: todas las razones y empresas";
            chkCargarTodasRazonesEmpresas.UseVisualStyleBackColor = true;
            chkCargarTodasRazonesEmpresas.CheckedChanged += chkCargarTodasRazonesEmpresas_CheckedChanged_1;
            // 
            // lblPlantillaInfo
            // 
            lblPlantillaInfo.AutoSize = true;
            lblPlantillaInfo.Font = new Font("Segoe UI", 9F);
            lblPlantillaInfo.ForeColor = Color.FromArgb(50, 80, 130);
            lblPlantillaInfo.Location = new Point(43, 185);
            lblPlantillaInfo.Name = "lblPlantillaInfo";
            lblPlantillaInfo.Size = new Size(153, 15);
            lblPlantillaInfo.TabIndex = 20;
            lblPlantillaInfo.Text = "📊  Sin plantilla configurada";
            // 
            // dgvRelsultados
            // 
            dgvRelsultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRelsultados.Location = new Point(12, 256);
            dgvRelsultados.Name = "dgvRelsultados";
            dgvRelsultados.Size = new Size(731, 413);
            dgvRelsultados.TabIndex = 22;
            // 
            // btnCargarInventario
            // 
            btnCargarInventario.Location = new Point(768, 21);
            btnCargarInventario.Name = "btnCargarInventario";
            btnCargarInventario.Size = new Size(120, 30);
            btnCargarInventario.TabIndex = 5;
            btnCargarInventario.Text = "Cargar inventario";
            btnCargarInventario.Click += btnCargarInventario_Click;
            // 
            // btnAnalizarExcel
            // 
            btnAnalizarExcel.Location = new Point(138, 21);
            btnAnalizarExcel.Name = "btnAnalizarExcel";
            btnAnalizarExcel.Size = new Size(120, 30);
            btnAnalizarExcel.TabIndex = 4;
            btnAnalizarExcel.Text = "Analizar Excel";
            btnAnalizarExcel.Click += btnAnalizarExcel_Click;
            // 
            // pnlChart
            // 
            pnlChart.Location = new Point(775, 256);
            pnlChart.Name = "pnlChart";
            pnlChart.Size = new Size(670, 413);
            pnlChart.TabIndex = 24;

            // panelCargando
            panelCargando = new Panel();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            panelCargando.SuspendLayout();
            // 
            // panelCargando
            // 
            panelCargando.BackColor = Color.FromArgb(236, 240, 241);
            panelCargando.BorderStyle = BorderStyle.FixedSingle;
            panelCargando.Controls.Add(lblCargando);
            panelCargando.Controls.Add(progressBarCargando);
            panelCargando.Location = new Point(400, 250);
            panelCargando.Name = "panelCargando";
            panelCargando.Size = new Size(400, 120);
            panelCargando.TabIndex = 30;
            panelCargando.Visible = false;
            // 
            // lblCargando
            // 
            lblCargando.AutoSize = true;
            lblCargando.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCargando.Location = new Point(20, 18);
            lblCargando.Name = "lblCargando";
            lblCargando.Size = new Size(200, 23);
            lblCargando.Text = "Cargando, por favor espera...";
            // 
            // progressBarCargando
            // 
            progressBarCargando.Location = new Point(20, 52);
            progressBarCargando.Name = "progressBarCargando";
            progressBarCargando.Size = new Size(360, 20);
            progressBarCargando.Style = ProgressBarStyle.Marquee;
            progressBarCargando.MarqueeAnimationSpeed = 30;
            panelCargando.ResumeLayout(false);
            panelCargando.PerformLayout();
            // 
            // FrmCalculoInventarios
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1457, 800);
            Controls.Add(panelConfigCuerpo);
            Controls.Add(panelCargando);
            Name = "FrmCalculoInventarios";
            Text = "Cálculo de Inventarios";
            ((System.ComponentModel.ISupportInitialize)splitCentral).EndInit();
            splitCentral.ResumeLayout(false);
            panelConfigCuerpo.ResumeLayout(false);
            panelConfigCuerpo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRelsultados).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Label lblConfigSubtitulo;
        private Panel panelConfigHeader;
        private Button btnPreviewMes;
        private Button btnAgregarMes;
        private Button btnQuitarMes;
        private Button btnAnalizarMeses;
        private Button btnProcesarPreview;
        private Button btnGuardarCalculos;
        private SplitContainer splitCentral;
        private Panel pnlLeft;
        private Panel pnlRight;
        private Panel panelConfigCuerpo;
        private Panel panelCargando;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
        private Label lblTotalGeneral;
        private Button btnHistorialVerificacion;
        private Button btnExportarExcel;
        private Button btnRecalcular;
        private Button btnVerificarPartes;
        private CheckBox chkUsarPerfil;
        private Label lblLblRazon;
        private ComboBox cmbRazonSocial;
        private Label lblLblEmpresa;
        private ComboBox cmbEmpresa;
        private CheckBox chkCargarTodasRazonesEmpresas;
        private Label lblPlantillaInfo;
        private DataGridView dgvRelsultados;
        private Panel pnlChart;
        private Button btnCargarInventario;
        private Button btnAnalizarExcel;
        private Label lblMesAno;
        private Label label1;
    }
}