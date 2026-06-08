namespace Retorno360Tacna.FORMS
{
    partial class FrmDetalleConciliacion
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelHeader;
        private Panel panelAcciones;
        private Label lblTitulo;
        private Button btnCerrar;
        private Button btnExportar;
        private DataGridView dgvDetalle;
        private Panel panelFooter;
        private Label lblTotalRegistros;

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
            panelHeader = new Panel();
            panelAcciones = new Panel();
            lblTitulo = new Label();
            btnExportar = new Button();
            btnCerrar = new Button();
            dgvDetalle = new DataGridView();
            panelFooter = new Panel();
            lblTotalRegistros = new Label();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).BeginInit();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(52, 73, 94);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(panelAcciones);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1200, 106);
            panelHeader.TabIndex = 0;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnExportar);
            panelAcciones.Controls.Add(btnCerrar);
            panelAcciones.Dock = DockStyle.Right;
            panelAcciones.Location = new Point(820, 0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(380, 106);
            panelAcciones.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Padding = new Padding(20, 0, 0, 0);
            lblTitulo.Size = new Size(820, 106);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Detalle de Conciliación";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportar.BackColor = Color.FromArgb(39, 174, 96);
            btnExportar.Cursor = Cursors.Hand;
            btnExportar.FlatAppearance.BorderSize = 0;
            btnExportar.FlatStyle = FlatStyle.Flat;
            btnExportar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportar.ForeColor = Color.White;
            btnExportar.Image = Properties.Resources.gdform_103694;
            btnExportar.ImageAlign = ContentAlignment.MiddleRight;
            btnExportar.Location = new Point(72, 24);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 58);
            btnExportar.TabIndex = 1;
            btnExportar.Text = "Exportar";
            btnExportar.TextAlign = ContentAlignment.MiddleLeft;
            btnExportar.UseVisualStyleBackColor = false;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.BackColor = Color.FromArgb(231, 76, 60);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Location = new Point(224, 24);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(130, 58);
            btnCerrar.TabIndex = 2;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // dgvDetalle
            // 
            dgvDetalle.Dock = DockStyle.Fill;
            dgvDetalle.Location = new Point(0, 106);
            dgvDetalle.Name = "dgvDetalle";
            dgvDetalle.Size = new Size(1200, 554);
            dgvDetalle.TabIndex = 1;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.FromArgb(236, 240, 241);
            panelFooter.Controls.Add(lblTotalRegistros);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 660);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(1200, 40);
            panelFooter.TabIndex = 2;
            // 
            // lblTotalRegistros
            // 
            lblTotalRegistros.AutoSize = true;
            lblTotalRegistros.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalRegistros.ForeColor = Color.FromArgb(52, 73, 94);
            lblTotalRegistros.Location = new Point(20, 10);
            lblTotalRegistros.Name = "lblTotalRegistros";
            lblTotalRegistros.Size = new Size(121, 19);
            lblTotalRegistros.TabIndex = 0;
            lblTotalRegistros.Text = "Total registros: 0";
            // 
            // FrmDetalleConciliacion
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1200, 700);
            Controls.Add(dgvDetalle);
            Controls.Add(panelFooter);
            Controls.Add(panelHeader);
            MinimumSize = new Size(980, 560);
            Name = "FrmDetalleConciliacion";
            Text = "Detalle de Conciliación";
            Load += FrmDetalleConciliacion_Load;
            panelHeader.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).EndInit();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }
    }
}
