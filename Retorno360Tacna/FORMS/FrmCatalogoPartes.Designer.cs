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
            btnConsultar = new Button();
            btnExportar = new Button();
            btnVerDetalle = new Button();
            panelResumen = new Panel();
            lblTotalPartes = new Label();
            lblTotalConBOM = new Label();
            lblTotalSinBOM = new Label();
            chartCatalogo = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
            panelFiltros.SuspendLayout();
            panelResumen.SuspendLayout();
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
            panelFiltros.Controls.Add(btnConsultar);
            panelFiltros.Controls.Add(btnExportar);
            panelFiltros.Controls.Add(btnVerDetalle);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Size = new Size(1231, 120);
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
            lblRazonSocial.Location = new Point(15, 50);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(76, 15);
            lblRazonSocial.TabIndex = 1;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // cboRazonSocial
            // 
            cboRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRazonSocial.Font = new Font("Segoe UI", 9F);
            cboRazonSocial.FormattingEnabled = true;
            cboRazonSocial.Location = new Point(15, 70);
            cboRazonSocial.Name = "cboRazonSocial";
            cboRazonSocial.Size = new Size(250, 23);
            cboRazonSocial.TabIndex = 2;
            cboRazonSocial.SelectedIndexChanged += cboRazonSocial_SelectedIndexChanged;
            // 
            // lblBaseDatos
            // 
            lblBaseDatos.AutoSize = true;
            lblBaseDatos.Font = new Font("Segoe UI", 9F);
            lblBaseDatos.Location = new Point(280, 50);
            lblBaseDatos.Name = "lblBaseDatos";
            lblBaseDatos.Size = new Size(83, 15);
            lblBaseDatos.TabIndex = 3;
            lblBaseDatos.Text = "Base de Datos:";
            // 
            // cboBaseDatos
            // 
            cboBaseDatos.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBaseDatos.Enabled = false;
            cboBaseDatos.Font = new Font("Segoe UI", 9F);
            cboBaseDatos.FormattingEnabled = true;
            cboBaseDatos.Location = new Point(280, 70);
            cboBaseDatos.Name = "cboBaseDatos";
            cboBaseDatos.Size = new Size(200, 23);
            cboBaseDatos.TabIndex = 4;
            // 
            // lblFechaInicio
            // 
            lblFechaInicio.AutoSize = true;
            lblFechaInicio.Font = new Font("Segoe UI", 9F);
            lblFechaInicio.Location = new Point(495, 50);
            lblFechaInicio.Name = "lblFechaInicio";
            lblFechaInicio.Size = new Size(73, 15);
            lblFechaInicio.TabIndex = 5;
            lblFechaInicio.Text = "Fecha Inicio:";
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.Font = new Font("Segoe UI", 9F);
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(495, 70);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.Size = new Size(120, 23);
            dtpFechaInicio.TabIndex = 6;
            // 
            // lblFechaFin
            // 
            lblFechaFin.AutoSize = true;
            lblFechaFin.Font = new Font("Segoe UI", 9F);
            lblFechaFin.Location = new Point(630, 50);
            lblFechaFin.Name = "lblFechaFin";
            lblFechaFin.Size = new Size(60, 15);
            lblFechaFin.TabIndex = 7;
            lblFechaFin.Text = "Fecha Fin:";
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.Font = new Font("Segoe UI", 9F);
            dtpFechaFin.Format = DateTimePickerFormat.Short;
            dtpFechaFin.Location = new Point(630, 70);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.Size = new Size(120, 23);
            dtpFechaFin.TabIndex = 8;
            // 
            // btnConsultar
            // 
            btnConsultar.BackColor = Color.FromArgb(79, 129, 189);
            btnConsultar.FlatStyle = FlatStyle.Flat;
            btnConsultar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnConsultar.ForeColor = Color.White;
            btnConsultar.Image = Properties.Resources.search_magnifying_glass_icon_1926311;
            btnConsultar.ImageAlign = ContentAlignment.MiddleRight;
            btnConsultar.Location = new Point(760, 50);
            btnConsultar.Name = "btnConsultar";
            btnConsultar.Size = new Size(136, 51);
            btnConsultar.TabIndex = 9;
            btnConsultar.Text = "Consultar";
            btnConsultar.TextAlign = ContentAlignment.MiddleLeft;
            btnConsultar.UseVisualStyleBackColor = false;
            btnConsultar.Click += btnConsultar_Click;
            // 
            // btnExportar
            // 
            btnExportar.BackColor = Color.FromArgb(46, 204, 113);
            btnExportar.FlatStyle = FlatStyle.Flat;
            btnExportar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportar.ForeColor = Color.White;
            btnExportar.Image = Properties.Resources.gdform_1036941;
            btnExportar.ImageAlign = ContentAlignment.MiddleRight;
            btnExportar.Location = new Point(902, 50);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(136, 51);
            btnExportar.TabIndex = 10;
            btnExportar.Text = "Exportar Excel";
            btnExportar.TextAlign = ContentAlignment.MiddleLeft;
            btnExportar.UseVisualStyleBackColor = false;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnVerDetalle
            // 
            btnVerDetalle.BackColor = Color.FromArgb(155, 89, 182);
            btnVerDetalle.FlatStyle = FlatStyle.Flat;
            btnVerDetalle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnVerDetalle.ForeColor = Color.White;
            btnVerDetalle.Image = Properties.Resources.Surveys_37105;
            btnVerDetalle.ImageAlign = ContentAlignment.MiddleRight;
            btnVerDetalle.Location = new Point(1044, 50);
            btnVerDetalle.Name = "btnVerDetalle";
            btnVerDetalle.Size = new Size(136, 51);
            btnVerDetalle.TabIndex = 11;
            btnVerDetalle.Text = "Ver Detalle";
            btnVerDetalle.TextAlign = ContentAlignment.MiddleLeft;
            btnVerDetalle.UseVisualStyleBackColor = false;
            btnVerDetalle.Click += btnVerDetalle_Click;
            // 
            // panelResumen
            // 
            panelResumen.BackColor = Color.White;
            panelResumen.Controls.Add(lblTotalPartes);
            panelResumen.Controls.Add(lblTotalConBOM);
            panelResumen.Controls.Add(lblTotalSinBOM);
            panelResumen.Dock = DockStyle.Bottom;
            panelResumen.Location = new Point(0, 570);
            panelResumen.Name = "panelResumen";
            panelResumen.Size = new Size(1231, 50);
            panelResumen.TabIndex = 1;
            // 
            // lblTotalPartes
            // 
            lblTotalPartes.AutoSize = true;
            lblTotalPartes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalPartes.Location = new Point(15, 15);
            lblTotalPartes.Name = "lblTotalPartes";
            lblTotalPartes.Size = new Size(125, 19);
            lblTotalPartes.TabIndex = 0;
            lblTotalPartes.Text = "Total de Partes: 0";
            // 
            // lblTotalConBOM
            // 
            lblTotalConBOM.AutoSize = true;
            lblTotalConBOM.Font = new Font("Segoe UI", 10F);
            lblTotalConBOM.ForeColor = Color.Green;
            lblTotalConBOM.Location = new Point(417, 15);
            lblTotalConBOM.Name = "lblTotalConBOM";
            lblTotalConBOM.Size = new Size(85, 19);
            lblTotalConBOM.TabIndex = 1;
            lblTotalConBOM.Text = "Con BOM: 0";
            // 
            // lblTotalSinBOM
            // 
            lblTotalSinBOM.AutoSize = true;
            lblTotalSinBOM.Font = new Font("Segoe UI", 10F);
            lblTotalSinBOM.ForeColor = Color.Red;
            lblTotalSinBOM.Location = new Point(672, 15);
            lblTotalSinBOM.Name = "lblTotalSinBOM";
            lblTotalSinBOM.Size = new Size(78, 19);
            lblTotalSinBOM.TabIndex = 2;
            lblTotalSinBOM.Text = "Sin BOM: 0";
            // 
            // chartCatalogo
            // 
            chartCatalogo.Dock = DockStyle.Fill;
            chartCatalogo.Location = new Point(0, 120);
            chartCatalogo.Name = "chartCatalogo";
            chartCatalogo.Size = new Size(1231, 450);
            chartCatalogo.TabIndex = 2;
            // 
            // FrmCatalogoPartes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1231, 620);
            ControlBox = false;
            Controls.Add(chartCatalogo);
            Controls.Add(panelResumen);
            Controls.Add(panelFiltros);
            Font = new Font("Segoe UI", 9F);
            FormScreenCaptureMode = ScreenCaptureMode.HideWindow;
            Name = "FrmCatalogoPartes";
            Text = "Catálogo de Partes - BOM";
            Load += FrmCatalogoPartes_Load;
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelResumen.ResumeLayout(false);
            panelResumen.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblRazonSocial;
        private System.Windows.Forms.ComboBox cboRazonSocial;
        private System.Windows.Forms.Label lblBaseDatos;
        private System.Windows.Forms.ComboBox cboBaseDatos;
        private System.Windows.Forms.Label lblFechaInicio;
        private System.Windows.Forms.DateTimePicker dtpFechaInicio;
        private System.Windows.Forms.Label lblFechaFin;
        private System.Windows.Forms.DateTimePicker dtpFechaFin;
        private System.Windows.Forms.Button btnConsultar;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnVerDetalle;
        private System.Windows.Forms.Panel panelResumen;
        private System.Windows.Forms.Label lblTotalPartes;
        private System.Windows.Forms.Label lblTotalConBOM;
        private System.Windows.Forms.Label lblTotalSinBOM;
        private LiveChartsCore.SkiaSharpView.WinForms.CartesianChart chartCatalogo;
    }
}
