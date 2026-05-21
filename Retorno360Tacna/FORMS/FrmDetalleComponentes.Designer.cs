namespace Retorno360Tacna.FORMS
{
    partial class FrmDetalleComponentes
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
            lblTitulo = new Label();
            dgvDetalles = new DataGridView();
            panelHeader = new Panel();
            btnCerrar = new Button();
            lblResumen = new Label();
            txtBuscar = new TextBox();
            lblBuscar = new Label();
            panelBorder = new Panel();
            panelPaginacion = new Panel();
            btnPrimeraPagina = new Button();
            btnPaginaAnterior = new Button();
            lblPaginaInfo = new Label();
            btnPaginaSiguiente = new Button();
            btnUltimaPagina = new Button();
            lblTotalRegistros = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvDetalles).BeginInit();
            panelHeader.SuspendLayout();
            panelBorder.SuspendLayout();
            panelPaginacion.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(15, 13);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(298, 21);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Detalle de Partes MP - BOM";
            // 
            // dgvDetalles
            // 
            dgvDetalles.AllowUserToAddRows = false;
            dgvDetalles.AllowUserToDeleteRows = false;
            dgvDetalles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDetalles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDetalles.BackgroundColor = Color.White;
            dgvDetalles.BorderStyle = BorderStyle.None;
            dgvDetalles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDetalles.Location = new Point(12, 130);
            dgvDetalles.MultiSelect = false;
            dgvDetalles.Name = "dgvDetalles";
            dgvDetalles.ReadOnly = true;
            dgvDetalles.RowHeadersVisible = false;
            dgvDetalles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalles.Size = new Size(1176, 420);
            dgvDetalles.TabIndex = 1;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(46, 204, 113);
            panelHeader.Controls.Add(btnCerrar);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(2, 2);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1196, 50);
            panelHeader.TabIndex = 2;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.BackColor = Color.FromArgb(192, 57, 43);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Location = new Point(1141, 5);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(45, 40);
            btnCerrar.TabIndex = 1;
            btnCerrar.Text = "✖";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // lblResumen
            // 
            lblResumen.AutoSize = true;
            lblResumen.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumen.ForeColor = Color.FromArgb(52, 73, 94);
            lblResumen.Location = new Point(12, 60);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(179, 19);
            lblResumen.TabIndex = 3;
            lblResumen.Text = "Total de Componentes: 0";
            // 
            // txtBuscar
            // 
            txtBuscar.Font = new Font("Segoe UI", 10F);
            txtBuscar.Location = new Point(12, 95);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar por número de parte o descripción...";
            txtBuscar.Size = new Size(500, 25);
            txtBuscar.TabIndex = 4;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Font = new Font("Segoe UI", 9F);
            lblBuscar.ForeColor = Color.FromArgb(52, 73, 94);
            lblBuscar.Location = new Point(12, 75);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(45, 15);
            lblBuscar.TabIndex = 5;
            lblBuscar.Text = "Buscar:";
            // 
            // panelBorder
            // 
            panelBorder.BackColor = Color.FromArgb(41, 128, 185);
            panelBorder.Controls.Add(panelPaginacion);
            panelBorder.Controls.Add(lblResumen);
            panelBorder.Controls.Add(lblBuscar);
            panelBorder.Controls.Add(txtBuscar);
            panelBorder.Controls.Add(dgvDetalles);
            panelBorder.Controls.Add(panelHeader);
            panelBorder.Dock = DockStyle.Fill;
            panelBorder.Location = new Point(0, 0);
            panelBorder.Name = "panelBorder";
            panelBorder.Padding = new Padding(2);
            panelBorder.Size = new Size(1200, 620);
            panelBorder.TabIndex = 6;
            // 
            // panelPaginacion
            // 
            panelPaginacion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelPaginacion.BackColor = Color.FromArgb(236, 240, 241);
            panelPaginacion.Controls.Add(lblTotalRegistros);
            panelPaginacion.Controls.Add(btnPrimeraPagina);
            panelPaginacion.Controls.Add(btnPaginaAnterior);
            panelPaginacion.Controls.Add(lblPaginaInfo);
            panelPaginacion.Controls.Add(btnPaginaSiguiente);
            panelPaginacion.Controls.Add(btnUltimaPagina);
            panelPaginacion.Location = new Point(12, 555);
            panelPaginacion.Name = "panelPaginacion";
            panelPaginacion.Size = new Size(1176, 55);
            panelPaginacion.TabIndex = 7;
            panelPaginacion.Visible = false;
            // 
            // btnPrimeraPagina
            // 
            btnPrimeraPagina.BackColor = Color.FromArgb(52, 152, 219);
            btnPrimeraPagina.Cursor = Cursors.Hand;
            btnPrimeraPagina.FlatAppearance.BorderSize = 0;
            btnPrimeraPagina.FlatStyle = FlatStyle.Flat;
            btnPrimeraPagina.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPrimeraPagina.ForeColor = Color.White;
            btnPrimeraPagina.Location = new Point(250, 10);
            btnPrimeraPagina.Name = "btnPrimeraPagina";
            btnPrimeraPagina.Size = new Size(80, 35);
            btnPrimeraPagina.TabIndex = 0;
            btnPrimeraPagina.Text = "⏮ Primera";
            btnPrimeraPagina.UseVisualStyleBackColor = false;
            btnPrimeraPagina.Click += btnPrimeraPagina_Click;
            // 
            // btnPaginaAnterior
            // 
            btnPaginaAnterior.BackColor = Color.FromArgb(52, 152, 219);
            btnPaginaAnterior.Cursor = Cursors.Hand;
            btnPaginaAnterior.FlatAppearance.BorderSize = 0;
            btnPaginaAnterior.FlatStyle = FlatStyle.Flat;
            btnPaginaAnterior.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPaginaAnterior.ForeColor = Color.White;
            btnPaginaAnterior.Location = new Point(340, 10);
            btnPaginaAnterior.Name = "btnPaginaAnterior";
            btnPaginaAnterior.Size = new Size(100, 35);
            btnPaginaAnterior.TabIndex = 1;
            btnPaginaAnterior.Text = "◀ Anterior";
            btnPaginaAnterior.UseVisualStyleBackColor = false;
            btnPaginaAnterior.Click += btnPaginaAnterior_Click;
            // 
            // lblPaginaInfo
            // 
            lblPaginaInfo.AutoSize = true;
            lblPaginaInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPaginaInfo.ForeColor = Color.FromArgb(52, 73, 94);
            lblPaginaInfo.Location = new Point(450, 17);
            lblPaginaInfo.Name = "lblPaginaInfo";
            lblPaginaInfo.Size = new Size(200, 19);
            lblPaginaInfo.TabIndex = 2;
            lblPaginaInfo.Text = "Página 1 de 1";
            lblPaginaInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnPaginaSiguiente
            // 
            btnPaginaSiguiente.BackColor = Color.FromArgb(52, 152, 219);
            btnPaginaSiguiente.Cursor = Cursors.Hand;
            btnPaginaSiguiente.FlatAppearance.BorderSize = 0;
            btnPaginaSiguiente.FlatStyle = FlatStyle.Flat;
            btnPaginaSiguiente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPaginaSiguiente.ForeColor = Color.White;
            btnPaginaSiguiente.Location = new Point(660, 10);
            btnPaginaSiguiente.Name = "btnPaginaSiguiente";
            btnPaginaSiguiente.Size = new Size(100, 35);
            btnPaginaSiguiente.TabIndex = 3;
            btnPaginaSiguiente.Text = "Siguiente ▶";
            btnPaginaSiguiente.UseVisualStyleBackColor = false;
            btnPaginaSiguiente.Click += btnPaginaSiguiente_Click;
            // 
            // btnUltimaPagina
            // 
            btnUltimaPagina.BackColor = Color.FromArgb(52, 152, 219);
            btnUltimaPagina.Cursor = Cursors.Hand;
            btnUltimaPagina.FlatAppearance.BorderSize = 0;
            btnUltimaPagina.FlatStyle = FlatStyle.Flat;
            btnUltimaPagina.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUltimaPagina.ForeColor = Color.White;
            btnUltimaPagina.Location = new Point(770, 10);
            btnUltimaPagina.Name = "btnUltimaPagina";
            btnUltimaPagina.Size = new Size(80, 35);
            btnUltimaPagina.TabIndex = 4;
            btnUltimaPagina.Text = "Última ⏭";
            btnUltimaPagina.UseVisualStyleBackColor = false;
            btnUltimaPagina.Click += btnUltimaPagina_Click;
            // 
            // lblTotalRegistros
            // 
            lblTotalRegistros.AutoSize = true;
            lblTotalRegistros.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblTotalRegistros.ForeColor = Color.FromArgb(52, 73, 94);
            lblTotalRegistros.Location = new Point(10, 20);
            lblTotalRegistros.Name = "lblTotalRegistros";
            lblTotalRegistros.Size = new Size(150, 15);
            lblTotalRegistros.TabIndex = 5;
            lblTotalRegistros.Text = "Total de registros: 0";
            // 
            // FrmDetalleComponentes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1200, 620);
            Controls.Add(panelBorder);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmDetalleComponentes";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Detalle de Componentes BOM";
            Load += FrmDetalleComponentes_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDetalles).EndInit();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelPaginacion.ResumeLayout(false);
            panelPaginacion.PerformLayout();
            panelBorder.ResumeLayout(false);
            panelBorder.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitulo;
        private DataGridView dgvDetalles;
        private Panel panelHeader;
        private Button btnCerrar;
        private Label lblResumen;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private Panel panelBorder;
        private Panel panelPaginacion;
        private Button btnPrimeraPagina;
        private Button btnPaginaAnterior;
        private Label lblPaginaInfo;
        private Button btnPaginaSiguiente;
        private Button btnUltimaPagina;
        private Label lblTotalRegistros;
    }
}
