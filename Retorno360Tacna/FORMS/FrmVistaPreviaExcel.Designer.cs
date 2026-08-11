namespace Retorno360Tacna.FORMS
{
    partial class FrmVistaPreviaExcel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        /// 
        private System.Windows.Forms.DataGridView dgvPreview;
        private System.ComponentModel.IContainer components = null;
        private Panel panelHeader;
        private Panel panelAcciones;
        private Label lblTitulo;
        private Button btnCerrar;
        private Panel panelFooter;
        private Label lblTotalRegistros;

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
            panelHeader = new Panel();
            lblTitulo = new Label();
            panelAcciones = new Panel();
            btnCerrar = new Button();
            dgvPreview = new DataGridView();
            panelFooter = new Panel();
            lblTotalRegistros = new Label();
            panelHeader.SuspendLayout();
            panelAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
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
            lblTitulo.Text = "Vista Previa de Reporte Excel";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnCerrar);
            panelAcciones.Dock = DockStyle.Right;
            panelAcciones.Location = new Point(820, 0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(380, 106);
            panelAcciones.TabIndex = 1;
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
            // dgvPreview
            // 
            dgvPreview.AllowUserToAddRows = false;
            dgvPreview.AllowUserToDeleteRows = false;
            dgvPreview.Dock = DockStyle.Fill;
            dgvPreview.Location = new Point(0, 106);
            dgvPreview.Name = "dgvPreview";
            dgvPreview.ReadOnly = true;
            dgvPreview.Size = new Size(1200, 554);
            dgvPreview.TabIndex = 1;
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
            // FrmVistaPreviaExcel
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1200, 700);
            Controls.Add(dgvPreview);
            Controls.Add(panelFooter);
            Controls.Add(panelHeader);
            MinimumSize = new Size(980, 560);
            Name = "FrmVistaPreviaExcel";
            Text = "Vista Previa de Reporte Excel";
            Load += FrmVistaPreviaExcel_Load;
            panelHeader.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}