namespace Retorno360Tacna.FORMS
{
    partial class FrmRetorno
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
            lblBaseDatos = new Label();
            cmbBaseDatos = new ComboBox();
            lblRazonSocial = new Label();
            cmbRazonSocial = new ComboBox();
            lblFechaInicio = new Label();
            dtpFechaInicio = new DateTimePicker();
            lblFechaFin = new Label();
            dtpFechaFin = new DateTimePicker();
            chkMateriaPrima = new CheckBox();
            btnCalcular = new Button();
            groupBoxResultados = new GroupBox();
            lblPorcentajeValor = new Label();
            lblPorcentajeTitulo = new Label();
            lblExportadoValor = new Label();
            lblExportadoTitulo = new Label();
            lblImportadoValor = new Label();
            lblImportadoTitulo = new Label();
            lblTotalPedimentos = new Label();
            lblCantPedimentosImp = new Label();
            lblCantPedimentosExp = new Label();
            pieChartView = new LiveChartsCore.SkiaSharpView.WinForms.PieChart();
            cartesianChartView = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
            chkCalRazon = new CheckBox();
            chkForzarCalculo = new CheckBox();
            panelCargando = new Panel();
            lblCargando = new Label();
            progressBarCargando = new ProgressBar();
            btnPDF = new Button();
            panelGraficaColumnas = new Panel();
            lblTituloColumnas = new Label();
            btnAnteriorColumnas = new Button();
            btnSiguienteColumnas = new Button();
            panelGraficaPie = new Panel();
            lblTituloPie = new Label();
            btnAnteriorPie = new Button();
            btnSiguientePie = new Button();
            groupBoxResultados.SuspendLayout();
            panelCargando.SuspendLayout();
            SuspendLayout();
            // 
            // lblBaseDatos
            // 
            lblBaseDatos.AutoSize = true;
            lblBaseDatos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBaseDatos.Location = new Point(26, 73);
            lblBaseDatos.Name = "lblBaseDatos";
            lblBaseDatos.Size = new Size(107, 19);
            lblBaseDatos.TabIndex = 2;
            lblBaseDatos.Text = "Base de Datos:";
            // 
            // cmbBaseDatos
            // 
            cmbBaseDatos.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBaseDatos.Enabled = false;
            cmbBaseDatos.Font = new Font("Segoe UI", 10F);
            cmbBaseDatos.FormattingEnabled = true;
            cmbBaseDatos.Location = new Point(26, 92);
            cmbBaseDatos.Margin = new Padding(3, 2, 3, 2);
            cmbBaseDatos.Name = "cmbBaseDatos";
            cmbBaseDatos.Size = new Size(263, 25);
            cmbBaseDatos.TabIndex = 3;
            cmbBaseDatos.SelectedIndexChanged += cmbBaseDatos_SelectedIndexChanged;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRazonSocial.Location = new Point(26, 15);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(98, 19);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazonSocial.Font = new Font("Segoe UI", 10F);
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(26, 34);
            cmbRazonSocial.Margin = new Padding(3, 2, 3, 2);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(350, 25);
            cmbRazonSocial.TabIndex = 1;
            cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.AutoSize = true;
            lblFechaInicio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaInicio.Location = new Point(400, 18);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new Size(91, 19);
            lblFechaInicio.TabIndex = 4;
            lblFechaInicio.Text = "Fecha Inicio:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Font = new Font("Segoe UI", 10F);
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(400, 37);
            dtpFechaInicio.Margin = new Padding(3, 2, 3, 2);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new Size(176, 25);
            dtpFechaInicio.TabIndex = 5;
            // 
            // lblFechaFin
            // 
            lblFechaFin.AutoSize = true;
            lblFechaFin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaFin.Location = new Point(602, 18);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new Size(74, 19);
            lblFechaFin.TabIndex = 6;
            lblFechaFin.Text = "Fecha Fin:";
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Font = new Font("Segoe UI", 10F);
            dtpFechaFin.Format = DateTimePickerFormat.Short;
            dtpFechaFin.Location = new Point(602, 37);
            dtpFechaFin.Margin = new Padding(3, 2, 3, 2);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new Size(176, 25);
            dtpFechaFin.TabIndex = 7;
            // 
            // chkMateriaPrima
            // 
            chkMateriaPrima.AutoSize = true;
            chkMateriaPrima.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkMateriaPrima.Location = new Point(400, 76);
            chkMateriaPrima.Margin = new Padding(3, 2, 3, 2);
            chkMateriaPrima.Name = "chkMateriaPrima";
            chkMateriaPrima.Size = new Size(201, 23);
            chkMateriaPrima.TabIndex = 8;
            chkMateriaPrima.Text = "Considerar Materia Prima";
            chkMateriaPrima.UseVisualStyleBackColor = true;
            // 
            // btnCalcular
            // 
            btnCalcular.BackColor = Color.FromArgb(39, 174, 96);
            btnCalcular.Cursor = Cursors.Hand;
            btnCalcular.FlatAppearance.BorderSize = 0;
            btnCalcular.FlatStyle = FlatStyle.Flat;
            btnCalcular.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCalcular.ForeColor = Color.White;
            btnCalcular.Image = Properties.Resources.search_magnifying_glass_icon_192631;
            btnCalcular.ImageAlign = ContentAlignment.MiddleRight;
            btnCalcular.Location = new Point(811, 23);
            btnCalcular.Margin = new Padding(3, 2, 3, 2);
            btnCalcular.Name = "btnCalcular";
            btnCalcular.Size = new Size(175, 56);
            btnCalcular.TabIndex = 9;
            btnCalcular.Text = "Calcular Retorno";
            btnCalcular.TextAlign = ContentAlignment.MiddleLeft;
            btnCalcular.UseVisualStyleBackColor = false;
            btnCalcular.Click += btnCalcular_Click;
            // 
            // groupBoxResultados
            // 
            groupBoxResultados.Controls.Add(lblPorcentajeValor);
            groupBoxResultados.Controls.Add(lblPorcentajeTitulo);
            groupBoxResultados.Controls.Add(lblExportadoValor);
            groupBoxResultados.Controls.Add(lblExportadoTitulo);
            groupBoxResultados.Controls.Add(lblImportadoValor);
            groupBoxResultados.Controls.Add(lblImportadoTitulo);
            groupBoxResultados.Controls.Add(lblTotalPedimentos);
            groupBoxResultados.Controls.Add(lblCantPedimentosImp);
            groupBoxResultados.Controls.Add(lblCantPedimentosExp);
            groupBoxResultados.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBoxResultados.Location = new Point(12, 172);
            groupBoxResultados.Margin = new Padding(3, 2, 3, 2);
            groupBoxResultados.Name = "groupBoxResultados";
            groupBoxResultados.Padding = new Padding(3, 2, 3, 2);
            groupBoxResultados.Size = new Size(310, 298);
            groupBoxResultados.TabIndex = 10;
            groupBoxResultados.TabStop = false;
            groupBoxResultados.Text = "Resultados";
            // 
            // lblPorcentajeValor
            // 
            lblPorcentajeValor.AutoSize = true;
            lblPorcentajeValor.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblPorcentajeValor.ForeColor = Color.FromArgb(39, 174, 96);
            lblPorcentajeValor.Location = new Point(15, 160);
            lblPorcentajeValor.Name = "lblPorcentajeValor";
            lblPorcentajeValor.Size = new Size(58, 30);
            lblPorcentajeValor.TabIndex = 5;
            lblPorcentajeValor.Text = "0.00";
            // 
            // lblPorcentajeTitulo
            // 
            lblPorcentajeTitulo.AutoSize = true;
            lblPorcentajeTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPorcentajeTitulo.Location = new Point(13, 143);
            lblPorcentajeTitulo.Name = "lblPorcentajeTitulo";
            lblPorcentajeTitulo.Size = new Size(164, 19);
            lblPorcentajeTitulo.TabIndex = 4;
            lblPorcentajeTitulo.Text = "Porcentaje de Retorno:";
            // 
            // lblExportadoValor
            // 
            lblExportadoValor.AutoSize = true;
            lblExportadoValor.Font = new Font("Segoe UI", 12F);
            lblExportadoValor.ForeColor = Color.FromArgb(41, 128, 185);
            lblExportadoValor.Location = new Point(15, 111);
            lblExportadoValor.Name = "lblExportadoValor";
            lblExportadoValor.Size = new Size(40, 21);
            lblExportadoValor.TabIndex = 3;
            lblExportadoValor.Text = "0.00";
            // 
            // lblExportadoTitulo
            // 
            lblExportadoTitulo.AutoSize = true;
            lblExportadoTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblExportadoTitulo.Location = new Point(15, 92);
            lblExportadoTitulo.Name = "lblExportadoTitulo";
            lblExportadoTitulo.Size = new Size(161, 19);
            lblExportadoTitulo.TabIndex = 2;
            lblExportadoTitulo.Text = "Valor Com. Exportado:";
            // 
            // lblImportadoValor
            // 
            lblImportadoValor.AutoSize = true;
            lblImportadoValor.Font = new Font("Segoe UI", 12F);
            lblImportadoValor.ForeColor = Color.FromArgb(192, 57, 43);
            lblImportadoValor.Location = new Point(15, 53);
            lblImportadoValor.Name = "lblImportadoValor";
            lblImportadoValor.Size = new Size(40, 21);
            lblImportadoValor.TabIndex = 1;
            lblImportadoValor.Text = "0.00";
            // 
            // lblImportadoTitulo
            // 
            lblImportadoTitulo.AutoSize = true;
            lblImportadoTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblImportadoTitulo.Location = new Point(13, 34);
            lblImportadoTitulo.Name = "lblImportadoTitulo";
            lblImportadoTitulo.Size = new Size(163, 19);
            lblImportadoTitulo.TabIndex = 0;
            lblImportadoTitulo.Text = "Valor Com. Importado:";
            // 
            // lblTotalPedimentos
            // 
            lblTotalPedimentos.AutoSize = true;
            lblTotalPedimentos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalPedimentos.ForeColor = Color.FromArgb(41, 128, 185);
            lblTotalPedimentos.Location = new Point(14, 240);
            lblTotalPedimentos.Name = "lblTotalPedimentos";
            lblTotalPedimentos.Size = new Size(116, 15);
            lblTotalPedimentos.TabIndex = 18;
            lblTotalPedimentos.Text = "Total Pedimentos: 0";
            // 
            // lblCantPedimentosImp
            // 
            lblCantPedimentosImp.AutoSize = true;
            lblCantPedimentosImp.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCantPedimentosImp.Location = new Point(14, 196);
            lblCantPedimentosImp.Name = "lblCantPedimentosImp";
            lblCantPedimentosImp.Size = new Size(157, 15);
            lblCantPedimentosImp.TabIndex = 16;
            lblCantPedimentosImp.Text = "Pedimentos Importación: 0";
            // 
            // lblCantPedimentosExp
            // 
            lblCantPedimentosExp.AutoSize = true;
            lblCantPedimentosExp.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCantPedimentosExp.Location = new Point(14, 218);
            lblCantPedimentosExp.Name = "lblCantPedimentosExp";
            lblCantPedimentosExp.Size = new Size(155, 15);
            lblCantPedimentosExp.TabIndex = 17;
            lblCantPedimentosExp.Text = "Pedimentos Exportación: 0";
            // 
            // pieChartView
            // 
            pieChartView.InitialRotation = 0D;
            pieChartView.IsClockwise = true;
            pieChartView.Location = new Point(799, 188);
            pieChartView.Margin = new Padding(3, 2, 3, 2);
            pieChartView.MaxAngle = 360D;
            pieChartView.MaxValue = null;
            pieChartView.MinValue = 0D;
            pieChartView.Name = "pieChartView";
            pieChartView.Size = new Size(400, 396);
            pieChartView.TabIndex = 11;
            // 
            // cartesianChartView
            // 
            cartesianChartView.Location = new Point(350, 188);
            cartesianChartView.Name = "cartesianChartView";
            cartesianChartView.Size = new Size(428, 396);
            cartesianChartView.TabIndex = 15;
            // 
            // chkCalRazon
            // 
            chkCalRazon.AutoSize = true;
            chkCalRazon.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkCalRazon.Location = new Point(400, 103);
            chkCalRazon.Margin = new Padding(3, 2, 3, 2);
            chkCalRazon.Name = "chkCalRazon";
            chkCalRazon.Size = new Size(199, 23);
            chkCalRazon.TabIndex = 19;
            chkCalRazon.Text = "Calcular por Razón Social";
            chkCalRazon.UseVisualStyleBackColor = true;
            chkCalRazon.CheckedChanged += chkCalRazon_CheckedChanged;
            // 
            // chkForzarCalculo
            // 
            chkForzarCalculo.AutoSize = true;
            chkForzarCalculo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkForzarCalculo.Location = new Point(400, 128);
            chkForzarCalculo.Margin = new Padding(3, 2, 3, 2);
            chkForzarCalculo.Name = "chkForzarCalculo";
            chkForzarCalculo.Size = new Size(264, 23);
            chkForzarCalculo.TabIndex = 21;
            chkForzarCalculo.Text = "Forzar cálculo (omitir validaciones)";
            chkForzarCalculo.UseVisualStyleBackColor = true;
            // 
            // panelCargando
            // 
            panelCargando.BackColor = Color.White;
            panelCargando.BorderStyle = BorderStyle.FixedSingle;
            panelCargando.Controls.Add(lblCargando);
            panelCargando.Controls.Add(progressBarCargando);
            panelCargando.Location = new Point(415, 234);
            panelCargando.Name = "panelCargando";
            panelCargando.Size = new Size(400, 150);
            panelCargando.TabIndex = 20;
            panelCargando.Visible = false;
            // 
            // lblCargando
            // 
            lblCargando.Dock = DockStyle.Bottom;
            lblCargando.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCargando.ForeColor = Color.FromArgb(52, 73, 94);
            lblCargando.Location = new Point(0, 80);
            lblCargando.Name = "lblCargando";
            lblCargando.Size = new Size(398, 68);
            lblCargando.TabIndex = 1;
            lblCargando.Text = "Calculando retorno...\r\nPor favor espere";
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
            // btnPDF
            // 
            btnPDF.BackColor = Color.FromArgb(231, 76, 60);
            btnPDF.Cursor = Cursors.Hand;
            btnPDF.Enabled = false;
            btnPDF.FlatAppearance.BorderSize = 0;
            btnPDF.FlatStyle = FlatStyle.Flat;
            btnPDF.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnPDF.ForeColor = Color.White;
            btnPDF.Image = Properties.Resources.applicationpdf_103614;
            btnPDF.ImageAlign = ContentAlignment.MiddleRight;
            btnPDF.Location = new Point(811, 92);
            btnPDF.Name = "btnPDF";
            btnPDF.Size = new Size(177, 56);
            btnPDF.TabIndex = 22;
            btnPDF.Text = "Generar PDF";
            btnPDF.TextAlign = ContentAlignment.MiddleLeft;
            btnPDF.UseVisualStyleBackColor = false;
            btnPDF.Click += btnGenerarPDF_Click;
            // 
            // panelGraficaColumnas
            // 
            panelGraficaColumnas.BackColor = Color.White;
            panelGraficaColumnas.BorderStyle = BorderStyle.FixedSingle;
            panelGraficaColumnas.Location = new Point(685, 170);
            panelGraficaColumnas.Name = "panelGraficaColumnas";
            panelGraficaColumnas.Size = new Size(428, 396);
            panelGraficaColumnas.TabIndex = 23;
            // 
            // lblTituloColumnas
            // 
            lblTituloColumnas.AutoSize = true;
            lblTituloColumnas.BackColor = Color.White;
            lblTituloColumnas.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTituloColumnas.Location = new Point(60, 10);
            lblTituloColumnas.Name = "lblTituloColumnas";
            lblTituloColumnas.Size = new Size(213, 20);
            lblTituloColumnas.TabIndex = 2;
            lblTituloColumnas.Text = "Importaciones vs Exportaciones (1/2)";
            // 
            // btnAnteriorColumnas
            // 
            btnAnteriorColumnas.BackColor = Color.FromArgb(52, 152, 219);
            btnAnteriorColumnas.FlatAppearance.BorderSize = 0;
            btnAnteriorColumnas.FlatStyle = FlatStyle.Flat;
            btnAnteriorColumnas.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnAnteriorColumnas.ForeColor = Color.White;
            btnAnteriorColumnas.Location = new Point(5, 5);
            btnAnteriorColumnas.Name = "btnAnteriorColumnas";
            btnAnteriorColumnas.Size = new Size(40, 30);
            btnAnteriorColumnas.TabIndex = 0;
            btnAnteriorColumnas.Text = "◀";
            btnAnteriorColumnas.UseVisualStyleBackColor = false;
            btnAnteriorColumnas.Click += btnCambiarGrafica_Click;
            // 
            // btnSiguienteColumnas
            // 
            btnSiguienteColumnas.BackColor = Color.FromArgb(52, 152, 219);
            btnSiguienteColumnas.FlatAppearance.BorderSize = 0;
            btnSiguienteColumnas.FlatStyle = FlatStyle.Flat;
            btnSiguienteColumnas.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnSiguienteColumnas.ForeColor = Color.White;
            btnSiguienteColumnas.Location = new Point(380, 5);
            btnSiguienteColumnas.Name = "btnSiguienteColumnas";
            btnSiguienteColumnas.Size = new Size(40, 30);
            btnSiguienteColumnas.TabIndex = 1;
            btnSiguienteColumnas.Text = "▶";
            btnSiguienteColumnas.UseVisualStyleBackColor = false;
            btnSiguienteColumnas.Click += btnCambiarGrafica_Click;
            // 
            // panelGraficaPie
            // 
            panelGraficaPie.BackColor = Color.White;
            panelGraficaPie.BorderStyle = BorderStyle.FixedSingle;
            panelGraficaPie.Location = new Point(685, 170);
            panelGraficaPie.Name = "panelGraficaPie";
            panelGraficaPie.Size = new Size(428, 396);
            panelGraficaPie.TabIndex = 24;
            panelGraficaPie.Visible = false;
            // 
            // lblTituloPie
            // 
            lblTituloPie.AutoSize = true;
            lblTituloPie.BackColor = Color.White;
            lblTituloPie.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTituloPie.Location = new Point(60, 10);
            lblTituloPie.Name = "lblTituloPie";
            lblTituloPie.Size = new Size(179, 20);
            lblTituloPie.TabIndex = 2;
            lblTituloPie.Text = "Distribución Porcentual (2/2)";
            // 
            // btnAnteriorPie
            // 
            btnAnteriorPie.BackColor = Color.FromArgb(52, 152, 219);
            btnAnteriorPie.FlatAppearance.BorderSize = 0;
            btnAnteriorPie.FlatStyle = FlatStyle.Flat;
            btnAnteriorPie.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnAnteriorPie.ForeColor = Color.White;
            btnAnteriorPie.Location = new Point(5, 5);
            btnAnteriorPie.Name = "btnAnteriorPie";
            btnAnteriorPie.Size = new Size(40, 30);
            btnAnteriorPie.TabIndex = 0;
            btnAnteriorPie.Text = "◀";
            btnAnteriorPie.UseVisualStyleBackColor = false;
            btnAnteriorPie.Click += btnCambiarGrafica_Click;
            // 
            // btnSiguientePie
            // 
            btnSiguientePie.BackColor = Color.FromArgb(52, 152, 219);
            btnSiguientePie.FlatAppearance.BorderSize = 0;
            btnSiguientePie.FlatStyle = FlatStyle.Flat;
            btnSiguientePie.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnSiguientePie.ForeColor = Color.White;
            btnSiguientePie.Location = new Point(380, 5);
            btnSiguientePie.Name = "btnSiguientePie";
            btnSiguientePie.Size = new Size(40, 30);
            btnSiguientePie.TabIndex = 1;
            btnSiguientePie.Text = "▶";
            btnSiguientePie.UseVisualStyleBackColor = false;
            btnSiguientePie.Click += btnCambiarGrafica_Click;
            // 
            // FrmRetorno
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1230, 618);
            Controls.Add(groupBoxResultados);
            Controls.Add(panelGraficaPie);
            Controls.Add(panelGraficaColumnas);
            Controls.Add(btnPDF);
            Controls.Add(chkForzarCalculo);
            Controls.Add(panelCargando);
            Controls.Add(chkCalRazon);
            Controls.Add(btnCalcular);
            Controls.Add(chkMateriaPrima);
            Controls.Add(dtpFechaFin);
            Controls.Add(lblFechaFin);
            Controls.Add(dtpFechaInicio);
            Controls.Add(lblFechaInicio);
            Controls.Add(cmbRazonSocial);
            Controls.Add(lblRazonSocial);
            Controls.Add(cmbBaseDatos);
            Controls.Add(lblBaseDatos);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "FrmRetorno";
            Text = "Retorno";
            Load += FrmRetorno_Load;
            groupBoxResultados.ResumeLayout(false);
            groupBoxResultados.PerformLayout();
            panelCargando.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBaseDatos;
        private ComboBox cmbBaseDatos;
        private Label lblRazonSocial;
        private ComboBox cmbRazonSocial;
        private Label lblFechaInicio;
        private DateTimePicker dtpFechaInicio;
        private Label lblFechaFin;
        private DateTimePicker dtpFechaFin;
        private CheckBox chkMateriaPrima;
        private Button btnCalcular;
        private GroupBox groupBoxResultados;
        private Label lblPorcentajeValor;
        private Label lblPorcentajeTitulo;
        private Label lblExportadoValor;
        private Label lblExportadoTitulo;
        private Label lblImportadoValor;
        private Label lblImportadoTitulo;
        private LiveChartsCore.SkiaSharpView.WinForms.PieChart pieChartView;
        private LiveChartsCore.SkiaSharpView.WinForms.CartesianChart cartesianChartView;
        private Label lblCantPedimentosImp;
        private Label lblCantPedimentosExp;
        private Label lblTotalPedimentos;
        private Button btnPDF;
        private CheckBox chkCalRazon;
        private CheckBox chkForzarCalculo;
        private Panel panelCargando;
        private Label lblCargando;
        private ProgressBar progressBarCargando;
        private Panel panelGraficaColumnas;
        private Panel panelGraficaPie;
        private Button btnAnteriorColumnas;
        private Button btnSiguienteColumnas;
        private Label lblTituloColumnas;
        private Button btnAnteriorPie;
        private Button btnSiguientePie;
        private Label lblTituloPie;
    }
}
