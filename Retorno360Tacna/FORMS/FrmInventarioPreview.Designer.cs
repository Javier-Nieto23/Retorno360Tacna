namespace Retorno360Tacna.FORMS
{
    partial class FrmInventarioPreview
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblEmpresa;
        private System.Windows.Forms.Button btnExportarEmpresa;
        private System.Windows.Forms.Button btnExportarTodas;


        private System.Windows.Forms.Button btnAnterior;
        private System.Windows.Forms.Button btnSiguiente;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelStats;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnExportarFooter;
        private System.Windows.Forms.Button btnAgregarTodasExcel;

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
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            lblEmpresa = new Label();
            btnExportarEmpresa = new Button();
            btnExportarTodas = new Button();
            btnAnterior = new Button();
            btnSiguiente = new Button();
            panelTop = new Panel();
            lblTitulo = new Label();
            panelStats = new Panel();
            splitMain = new SplitContainer();
            dgvResultados = new DataGridView();
            panelFooter = new Panel();
            btnExportarFooter = new Button();
            btnAgregarTodasExcel = new Button();
            btnCerrar = new Button();
            panelHeader = new Panel();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // lblEmpresa
            // 
            lblEmpresa.AutoSize = true;
            lblEmpresa.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblEmpresa.ForeColor = Color.White;
            lblEmpresa.Location = new Point(533, 21);
            lblEmpresa.Name = "lblEmpresa";
            lblEmpresa.Size = new Size(113, 20);
            lblEmpresa.TabIndex = 1;
            lblEmpresa.Text = "Empresa: (n/a)";
            // 
            // btnExportarEmpresa
            // 
            btnExportarEmpresa.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportarEmpresa.BackColor = Color.FromArgb(52, 152, 219);
            btnExportarEmpresa.FlatAppearance.BorderSize = 0;
            btnExportarEmpresa.FlatStyle = FlatStyle.Flat;
            btnExportarEmpresa.ForeColor = Color.White;
            btnExportarEmpresa.Location = new Point(1484, 12);
            btnExportarEmpresa.Name = "btnExportarEmpresa";
            btnExportarEmpresa.Size = new Size(90, 30);
            btnExportarEmpresa.TabIndex = 2;
            btnExportarEmpresa.Text = "Exportar";
            btnExportarEmpresa.UseVisualStyleBackColor = false;
            // 
            // btnExportarTodas
            // 
            btnExportarTodas.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportarTodas.BackColor = Color.FromArgb(46, 204, 113);
            btnExportarTodas.FlatAppearance.BorderSize = 0;
            btnExportarTodas.FlatStyle = FlatStyle.Flat;
            btnExportarTodas.ForeColor = Color.White;
            btnExportarTodas.Location = new Point(1584, 12);
            btnExportarTodas.Name = "btnExportarTodas";
            btnExportarTodas.Size = new Size(110, 30);
            btnExportarTodas.TabIndex = 3;
            btnExportarTodas.Text = "Exportar todas";
            btnExportarTodas.UseVisualStyleBackColor = false;
            // 
            // btnAnterior
            // 
            btnAnterior.Image = Properties.Resources.arrow_left_157341;
            btnAnterior.Location = new Point(458, 10);
            btnAnterior.Name = "btnAnterior";
            btnAnterior.Size = new Size(45, 40);
            btnAnterior.TabIndex = 4;
            btnAnterior.UseVisualStyleBackColor = true;
            // 
            // btnSiguiente
            // 
            btnSiguiente.Image = Properties.Resources.arrow_right_15731;
            btnSiguiente.Location = new Point(509, 11);
            btnSiguiente.Name = "btnSiguiente";
            btnSiguiente.Size = new Size(45, 40);
            btnSiguiente.TabIndex = 5;
            btnSiguiente.UseVisualStyleBackColor = true;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(22, 97, 66);
            panelTop.Controls.Add(lblTitulo);
            panelTop.Controls.Add(lblEmpresa);
            panelTop.Controls.Add(btnExportarEmpresa);
            panelTop.Controls.Add(btnExportarTodas);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(984, 56);
            panelTop.TabIndex = 2;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(54, 26);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(139, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Resultado de verificación";
            // 
            // panelStats
            // 
            panelStats.BackColor = Color.WhiteSmoke;
            panelStats.Dock = DockStyle.Top;
            panelStats.Location = new Point(0, 56);
            panelStats.Name = "panelStats";
            panelStats.Size = new Size(984, 84);
            panelStats.TabIndex = 1;
            // 
            // splitMain
            // 
            splitMain.Location = new Point(0, 140);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(dgvResultados);
            splitMain.Size = new Size(1100, 341);
            splitMain.SplitterDistance = 963;
            splitMain.TabIndex = 0;
            // 
            // dgvResultados
            // 
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Control;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvResultados.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvResultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = SystemColors.Window;
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle5.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
            dgvResultados.DefaultCellStyle = dataGridViewCellStyle5;
            dgvResultados.Location = new Point(0, 0);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = SystemColors.Control;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle6.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvResultados.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvResultados.Size = new Size(972, 341);
            dgvResultados.TabIndex = 0;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.WhiteSmoke;
            panelFooter.Controls.Add(btnExportarFooter);
            panelFooter.Controls.Add(btnAgregarTodasExcel);
            panelFooter.Controls.Add(btnAnterior);
            panelFooter.Controls.Add(btnSiguiente);
            panelFooter.Controls.Add(btnCerrar);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 481);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(984, 60);
            panelFooter.TabIndex = 3;
            // 
            // btnExportarFooter
            // 
            btnExportarFooter.Location = new Point(12, 12);
            btnExportarFooter.Name = "btnExportarFooter";
            btnExportarFooter.Size = new Size(110, 36);
            btnExportarFooter.TabIndex = 0;
            btnExportarFooter.Text = "Exportar";
            btnExportarFooter.Click += BtnExportarFooter_Click_1;
            // 
            // btnAgregarTodasExcel
            // 
            btnAgregarTodasExcel.Location = new Point(128, 12);
            btnAgregarTodasExcel.Name = "btnAgregarTodasExcel";
            btnAgregarTodasExcel.Size = new Size(160, 36);
            btnAgregarTodasExcel.TabIndex = 5;
            btnAgregarTodasExcel.Text = "Agregar todos a Excel";
            btnAgregarTodasExcel.Click += BtnAgregarTodasExcel_Click_1;
            // 
            // btnCerrar
            // 
            btnCerrar.Location = new Point(296, 12);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(110, 36);
            btnCerrar.TabIndex = 6;
            btnCerrar.Text = "Cerrar";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // panelHeader
            // 
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(200, 100);
            panelHeader.TabIndex = 0;
            // 
            // FrmInventarioPreview
            // 
            BackColor = Color.White;
            ClientSize = new Size(984, 541);
            ControlBox = false;
            Controls.Add(splitMain);
            Controls.Add(panelStats);
            Controls.Add(panelFooter);
            Controls.Add(panelTop);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmInventarioPreview";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resultado de Verificación de Partes";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            panelFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        private DataGridView dgvResultados;
    }
}