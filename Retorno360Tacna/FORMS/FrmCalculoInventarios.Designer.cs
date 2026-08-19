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
            chkUsarPerfil = new CheckBox();
            pnlCantidadMeses = new Panel();
            panelConfigCuerpo = new Panel();
            lblLblRazon = new Label();
            cmbRazonSocial = new ComboBox();
            lblLblEmpresa = new Label();
            cmbEmpresa = new ComboBox();
            lblLblMeses = new Label();
            nudCantidadMeses = new NumericUpDown();
            blCantidadMeses = new Label();
            panelPlantilla = new Panel();
            lblPlantillaInfo = new Label();
            btnCargarPlantilla = new Button();
            btnIniciarCalculo = new Button();
            panelConfigHeader = new Panel();
            lblConfigSubtitulo = new Label();
            pnlCaptura = new Panel();
            dgvResultados = new DataGridView();
            pnlBotonesInferiores = new Panel();
            btnRecalcular = new Button();
            btnExportarExcel = new Button();
            lblTotalGeneral = new Label();
            flpPaneles = new FlowLayoutPanel();
            pnlCantidadMeses.SuspendLayout();
            panelConfigCuerpo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudCantidadMeses).BeginInit();
            panelPlantilla.SuspendLayout();
            panelConfigHeader.SuspendLayout();
            pnlCaptura.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            pnlBotonesInferiores.SuspendLayout();
            SuspendLayout();
            // 
            // pnlCantidadMeses
            // 
            pnlCantidadMeses.BackColor = Color.FromArgb(245, 247, 250);
            pnlCantidadMeses.Controls.Add(panelConfigCuerpo);
            pnlCantidadMeses.Controls.Add(panelConfigHeader);
            pnlCantidadMeses.Dock = DockStyle.Fill;
            pnlCantidadMeses.Location = new Point(0, 0);
            pnlCantidadMeses.Name = "pnlCantidadMeses";
            pnlCantidadMeses.Size = new Size(1001, 734);
            pnlCantidadMeses.TabIndex = 0;
            // 
            // panelConfigCuerpo
            // 
            panelConfigCuerpo.BackColor = Color.Transparent;
            panelConfigCuerpo.Controls.Add(chkUsarPerfil);
            panelConfigCuerpo.Controls.Add(lblLblRazon);
            panelConfigCuerpo.Controls.Add(cmbRazonSocial);
            panelConfigCuerpo.Controls.Add(lblLblEmpresa);
            panelConfigCuerpo.Controls.Add(cmbEmpresa);
            panelConfigCuerpo.Controls.Add(lblLblMeses);
            panelConfigCuerpo.Controls.Add(nudCantidadMeses);
            panelConfigCuerpo.Controls.Add(blCantidadMeses);
            panelConfigCuerpo.Controls.Add(panelPlantilla);
            panelConfigCuerpo.Controls.Add(btnIniciarCalculo);
            panelConfigCuerpo.Dock = DockStyle.Fill;
            panelConfigCuerpo.Location = new Point(0, 75);
            panelConfigCuerpo.Name = "panelConfigCuerpo";
            panelConfigCuerpo.Padding = new Padding(40, 30, 40, 30);
            panelConfigCuerpo.Size = new Size(1001, 659);
            panelConfigCuerpo.TabIndex = 0;
            // 
            // lblLblRazon
            // 
            lblLblRazon.AutoSize = true;
            lblLblRazon.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLblRazon.ForeColor = Color.FromArgb(50, 60, 70);
            lblLblRazon.Location = new Point(40, 30);
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
            cmbRazonSocial.Location = new Point(40, 54);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(420, 25);
            cmbRazonSocial.TabIndex = 0;
            // 
            // lblLblEmpresa
            // 
            lblLblEmpresa.AutoSize = true;
            lblLblEmpresa.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLblEmpresa.ForeColor = Color.FromArgb(50, 60, 70);
            lblLblEmpresa.Location = new Point(40, 98);
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
            cmbEmpresa.Location = new Point(40, 122);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(420, 25);
            cmbEmpresa.TabIndex = 1;
            cmbEmpresa.SelectedIndexChanged += cmbEmpresa_SelectedIndexChanged;
            // 
            // lblLblMeses
            // 
            lblLblMeses.AutoSize = true;
            lblLblMeses.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLblMeses.ForeColor = Color.FromArgb(50, 60, 70);
            lblLblMeses.Location = new Point(40, 166);
            lblLblMeses.Name = "lblLblMeses";
            lblLblMeses.Size = new Size(207, 19);
            lblLblMeses.TabIndex = 2;
            lblLblMeses.Text = "¿Cuántos meses va a calcular?";
            // 
            // nudCantidadMeses
            // 
            nudCantidadMeses.Font = new Font("Segoe UI", 10F);
            nudCantidadMeses.Location = new Point(40, 190);
            nudCantidadMeses.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            nudCantidadMeses.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudCantidadMeses.Name = "nudCantidadMeses";
            nudCantidadMeses.Size = new Size(80, 25);
            nudCantidadMeses.TabIndex = 2;
            nudCantidadMeses.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // blCantidadMeses
            // 
            blCantidadMeses.AutoSize = true;
            blCantidadMeses.Font = new Font("Segoe UI", 8.5F);
            blCantidadMeses.ForeColor = Color.Gray;
            blCantidadMeses.Location = new Point(108, 170);
            blCantidadMeses.Name = "blCantidadMeses";
            blCantidadMeses.Size = new Size(45, 15);
            blCantidadMeses.TabIndex = 3;
            blCantidadMeses.Text = "(1 – 12)";
            // 
            // panelPlantilla
            // 
            panelPlantilla.BackColor = Color.FromArgb(236, 245, 255);
            panelPlantilla.BorderStyle = BorderStyle.FixedSingle;
            panelPlantilla.Controls.Add(lblPlantillaInfo);
            panelPlantilla.Controls.Add(btnCargarPlantilla);
            panelPlantilla.Location = new Point(40, 242);
            panelPlantilla.Name = "panelPlantilla";
            panelPlantilla.Padding = new Padding(14, 12, 14, 12);
            panelPlantilla.Size = new Size(500, 70);
            panelPlantilla.TabIndex = 4;
            // 
            // lblPlantillaInfo
            // 
            lblPlantillaInfo.AutoSize = true;
            lblPlantillaInfo.Font = new Font("Segoe UI", 9F);
            lblPlantillaInfo.ForeColor = Color.FromArgb(50, 80, 130);
            lblPlantillaInfo.Location = new Point(14, 14);
            lblPlantillaInfo.Name = "lblPlantillaInfo";
            lblPlantillaInfo.Size = new Size(180, 30);
            lblPlantillaInfo.TabIndex = 0;
            lblPlantillaInfo.Text = "📊  Sin plantilla configurada";
            // 
            // btnCargarPlantilla
            // 
            btnCargarPlantilla.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCargarPlantilla.BackColor = Color.FromArgb(41, 128, 185);
            btnCargarPlantilla.Cursor = Cursors.Hand;
            btnCargarPlantilla.FlatAppearance.BorderSize = 0;
            btnCargarPlantilla.FlatStyle = FlatStyle.Flat;
            btnCargarPlantilla.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCargarPlantilla.ForeColor = Color.White;
            btnCargarPlantilla.Location = new Point(360, 18);
            btnCargarPlantilla.Name = "btnCargarPlantilla";
            btnCargarPlantilla.Size = new Size(124, 32);
            btnCargarPlantilla.TabIndex = 1;
            btnCargarPlantilla.Text = "Usar plantilla";
            btnCargarPlantilla.UseVisualStyleBackColor = false;
            btnCargarPlantilla.Click += btnCargarPlantilla_Click;
            // 
            // btnIniciarCalculo
            // 
            btnIniciarCalculo.BackColor = Color.FromArgb(22, 163, 74);
            btnIniciarCalculo.FlatAppearance.BorderSize = 0;
            btnIniciarCalculo.FlatStyle = FlatStyle.Flat;
            btnIniciarCalculo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnIniciarCalculo.ForeColor = Color.White;
            btnIniciarCalculo.Location = new Point(40, 336);
            btnIniciarCalculo.Name = "btnIniciarCalculo";
            btnIniciarCalculo.Size = new Size(160, 46);
            btnIniciarCalculo.TabIndex = 3;
            btnIniciarCalculo.Text = "Continuar →";
            btnIniciarCalculo.UseVisualStyleBackColor = false;
            btnIniciarCalculo.Click += btnIniciarCalculo_Click;
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Font = new Font("Segoe UI", 9.5F);
            chkUsarPerfil.Location = new Point(40, 395);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(210, 21);
            chkUsarPerfil.TabIndex = 10;
            chkUsarPerfil.Text = "Usar empresas de mi perfil";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            chkUsarPerfil.CheckedChanged += chkUsarPerfil_CheckedChanged;

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
            // pnlCaptura
            // 
            pnlCaptura.Controls.Add(dgvResultados);
            pnlCaptura.Controls.Add(pnlBotonesInferiores);
            pnlCaptura.Controls.Add(flpPaneles);
            pnlCaptura.Dock = DockStyle.Fill;
            pnlCaptura.Location = new Point(0, 0);
            pnlCaptura.Name = "pnlCaptura";
            pnlCaptura.Size = new Size(1001, 734);
            pnlCaptura.TabIndex = 1;
            pnlCaptura.Visible = false;
            // 
            // dgvResultados
            // 
            dgvResultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResultados.Dock = DockStyle.Fill;
            dgvResultados.Location = new Point(0, 440);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.Size = new Size(1001, 294);
            dgvResultados.TabIndex = 2;
            // 
            // pnlBotonesInferiores
            // 
            pnlBotonesInferiores.Controls.Add(btnRecalcular);
            pnlBotonesInferiores.Controls.Add(btnExportarExcel);
            pnlBotonesInferiores.Controls.Add(lblTotalGeneral);
            pnlBotonesInferiores.Dock = DockStyle.Top;
            pnlBotonesInferiores.Location = new Point(0, 380);
            pnlBotonesInferiores.Name = "pnlBotonesInferiores";
            pnlBotonesInferiores.Size = new Size(1001, 60);
            pnlBotonesInferiores.TabIndex = 1;
            // 
            // btnRecalcular
            // 
            btnRecalcular.BackColor = Color.FromArgb(81, 162, 255);
            btnRecalcular.FlatAppearance.BorderSize = 0;
            btnRecalcular.FlatStyle = FlatStyle.Flat;
            btnRecalcular.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRecalcular.ForeColor = Color.White;
            btnRecalcular.Location = new Point(172, 7);
            btnRecalcular.Name = "btnRecalcular";
            btnRecalcular.Size = new Size(154, 50);
            btnRecalcular.TabIndex = 2;
            btnRecalcular.Text = "Conciliar";
            btnRecalcular.UseVisualStyleBackColor = false;
            btnRecalcular.Click += btnRecalcular_Click;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(39, 174, 96);
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Image = Properties.Resources.ext_xlsx_icon_176245;
            btnExportarExcel.ImageAlign = ContentAlignment.MiddleLeft;
            btnExportarExcel.Location = new Point(12, 7);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(154, 50);
            btnExportarExcel.TabIndex = 0;
            btnExportarExcel.Text = "Exportar Excel";
            btnExportarExcel.TextAlign = ContentAlignment.MiddleRight;
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // lblTotalGeneral
            // 
            lblTotalGeneral.AutoSize = true;
            lblTotalGeneral.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalGeneral.Location = new Point(369, 24);
            lblTotalGeneral.Name = "lblTotalGeneral";
            lblTotalGeneral.Size = new Size(117, 19);
            lblTotalGeneral.TabIndex = 1;
            lblTotalGeneral.Text = "Total general: --";
            // 
            // flpPaneles
            // 
            flpPaneles.AutoScroll = true;
            flpPaneles.Dock = DockStyle.Top;
            flpPaneles.Location = new Point(0, 0);
            flpPaneles.Name = "flpPaneles";
            flpPaneles.Padding = new Padding(10);
            flpPaneles.Size = new Size(1001, 380);
            flpPaneles.TabIndex = 0;
            // 
            // FrmCalculoInventarios
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1001, 734);
            Controls.Add(pnlCaptura);
            Controls.Add(pnlCantidadMeses);
            Name = "FrmCalculoInventarios";
            Text = "Cálculo de Inventarios";
            pnlCantidadMeses.ResumeLayout(false);
            panelConfigCuerpo.ResumeLayout(false);
            panelConfigCuerpo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudCantidadMeses).EndInit();
            panelPlantilla.ResumeLayout(false);
            panelPlantilla.PerformLayout();
            panelConfigHeader.ResumeLayout(false);
            panelConfigHeader.PerformLayout();
            pnlCaptura.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            pnlBotonesInferiores.ResumeLayout(false);
            pnlBotonesInferiores.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlCantidadMeses;
        private Panel panelConfigHeader;
        private Label lblConfigSubtitulo;
        private Panel panelConfigCuerpo;
        private Label lblLblRazon;
        private Label lblLblEmpresa;
        private Label lblLblMeses;
        private Label blCantidadMeses;
        private Button btnIniciarCalculo;
        private NumericUpDown nudCantidadMeses;
        private Panel panelPlantilla;
        private Label lblPlantillaInfo;
        private Button btnCargarPlantilla;
        private Panel pnlCaptura;
        private FlowLayoutPanel flpPaneles;
        private Panel pnlBotonesInferiores;
        private DataGridView dgvResultados;
        private Button btnExportarExcel;
        private Label lblTotalGeneral;
        private Button btnRecalcular;
        private ComboBox cmbEmpresa;
        private ComboBox cmbRazonSocial;
        private CheckBox chkUsarPerfil;
    }
}