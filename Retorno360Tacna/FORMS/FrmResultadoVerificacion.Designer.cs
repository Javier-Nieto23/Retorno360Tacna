namespace Retorno360Tacna.FORMS
{
    partial class FrmResultadoVerificacion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            panelHeader = new Panel();
            lblMensaje = new Label();
            lblTitulo = new Label();
            panelResumen = new Panel();
            lblFecha = new Label();
            lblTotalNoExistentes = new Label();
            lblCardNoExistentes = new Label();
            lblTotalExistentes = new Label();
            lblCardExistentes = new Label();
            lblTotalConsultados = new Label();
            lblCardConsultados = new Label();
            panelTablas = new Panel();
            panelNoExistentes = new Panel();
            dgvNoExistentes = new DataGridView();
            lblHeaderNoExistentes = new Label();
            panelExistentes = new Panel();
            dgvExistentes = new DataGridView();
            lblHeaderExistentes = new Label();
            btnAgregarGrid = new Button();
            panelBotones = new Panel();
            btnCerrar = new Button();
            btnAgregarFila = new Button();
            btnAgregarValidas = new Button();
            btnAplicarCorrecciones = new Button();
            btnAgregar = new Button();
            panelHeader.SuspendLayout();
            panelResumen.SuspendLayout();
            panelTablas.SuspendLayout();
            panelNoExistentes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvNoExistentes).BeginInit();
            panelExistentes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExistentes).BeginInit();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 80, 50);
            panelHeader.Controls.Add(lblMensaje);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(20, 0, 20, 0);
            panelHeader.Size = new Size(840, 76);
            panelHeader.TabIndex = 0;
            // 
            // lblMensaje
            // 
            lblMensaje.Font = new Font("Segoe UI", 9.5F);
            lblMensaje.ForeColor = Color.FromArgb(200, 240, 210);
            lblMensaje.Location = new Point(20, 44);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(800, 22);
            lblMensaje.TabIndex = 1;
            lblMensaje.Text = "—";
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 8);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(800, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Resultado de Verificación de Partes";
            // 
            // panelResumen
            // 
            panelResumen.BackColor = Color.FromArgb(245, 247, 250);
            panelResumen.Controls.Add(lblFecha);
            panelResumen.Controls.Add(lblTotalNoExistentes);
            panelResumen.Controls.Add(lblCardNoExistentes);
            panelResumen.Controls.Add(lblTotalExistentes);
            panelResumen.Controls.Add(lblCardExistentes);
            panelResumen.Controls.Add(lblTotalConsultados);
            panelResumen.Controls.Add(lblCardConsultados);
            panelResumen.Location = new Point(0, 76);
            panelResumen.Name = "panelResumen";
            panelResumen.Padding = new Padding(20, 14, 20, 14);
            panelResumen.Size = new Size(840, 69);
            panelResumen.TabIndex = 1;
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Font = new Font("Segoe UI", 9.5F);
            lblFecha.Location = new Point(727, 47);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(43, 17);
            lblFecha.TabIndex = 6;
            lblFecha.Text = "label1";
            // 
            // lblTotalNoExistentes
            // 
            lblTotalNoExistentes.AutoSize = true;
            lblTotalNoExistentes.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotalNoExistentes.ForeColor = Color.FromArgb(192, 57, 43);
            lblTotalNoExistentes.Location = new Point(380, 32);
            lblTotalNoExistentes.Name = "lblTotalNoExistentes";
            lblTotalNoExistentes.Size = new Size(28, 32);
            lblTotalNoExistentes.TabIndex = 5;
            lblTotalNoExistentes.Text = "0";
            // 
            // lblCardNoExistentes
            // 
            lblCardNoExistentes.AutoSize = true;
            lblCardNoExistentes.Font = new Font("Segoe UI", 9.5F);
            lblCardNoExistentes.ForeColor = Color.FromArgb(80, 90, 100);
            lblCardNoExistentes.Location = new Point(380, 14);
            lblCardNoExistentes.Name = "lblCardNoExistentes";
            lblCardNoExistentes.Size = new Size(127, 17);
            lblCardNoExistentes.TabIndex = 4;
            lblCardNoExistentes.Text = "NO existentes en BD";
            // 
            // lblTotalExistentes
            // 
            lblTotalExistentes.AutoSize = true;
            lblTotalExistentes.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotalExistentes.ForeColor = Color.FromArgb(30, 150, 70);
            lblTotalExistentes.Location = new Point(200, 32);
            lblTotalExistentes.Name = "lblTotalExistentes";
            lblTotalExistentes.Size = new Size(28, 32);
            lblTotalExistentes.TabIndex = 3;
            lblTotalExistentes.Text = "0";
            // 
            // lblCardExistentes
            // 
            lblCardExistentes.AutoSize = true;
            lblCardExistentes.Font = new Font("Segoe UI", 9.5F);
            lblCardExistentes.ForeColor = Color.FromArgb(80, 90, 100);
            lblCardExistentes.Location = new Point(200, 14);
            lblCardExistentes.Name = "lblCardExistentes";
            lblCardExistentes.Size = new Size(103, 17);
            lblCardExistentes.TabIndex = 2;
            lblCardExistentes.Text = "Existentes en BD";
            // 
            // lblTotalConsultados
            // 
            lblTotalConsultados.AutoSize = true;
            lblTotalConsultados.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotalConsultados.ForeColor = Color.FromArgb(40, 60, 80);
            lblTotalConsultados.Location = new Point(20, 32);
            lblTotalConsultados.Name = "lblTotalConsultados";
            lblTotalConsultados.Size = new Size(28, 32);
            lblTotalConsultados.TabIndex = 1;
            lblTotalConsultados.Text = "0";
            // 
            // lblCardConsultados
            // 
            lblCardConsultados.AutoSize = true;
            lblCardConsultados.Font = new Font("Segoe UI", 9.5F);
            lblCardConsultados.ForeColor = Color.FromArgb(80, 90, 100);
            lblCardConsultados.Location = new Point(20, 14);
            lblCardConsultados.Name = "lblCardConsultados";
            lblCardConsultados.Size = new Size(110, 17);
            lblCardConsultados.TabIndex = 0;
            lblCardConsultados.Text = "Total consultados";
            // 
            // panelTablas
            // 
            panelTablas.Controls.Add(panelNoExistentes);
            panelTablas.Controls.Add(panelExistentes);
            panelTablas.Dock = DockStyle.Fill;
            panelTablas.Location = new Point(0, 76);
            panelTablas.Name = "panelTablas";
            panelTablas.Padding = new Padding(14, 10, 14, 10);
            panelTablas.Size = new Size(840, 512);
            panelTablas.TabIndex = 2;
            // 
            // panelNoExistentes
            // 
            panelNoExistentes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelNoExistentes.BorderStyle = BorderStyle.FixedSingle;
            panelNoExistentes.Controls.Add(dgvNoExistentes);
            panelNoExistentes.Controls.Add(btnAgregarFila);
            panelNoExistentes.Controls.Add(lblHeaderNoExistentes);
            panelNoExistentes.Location = new Point(420, 10);
            panelNoExistentes.Name = "panelNoExistentes";
            panelNoExistentes.Size = new Size(390, 492);
            panelNoExistentes.TabIndex = 1;
            // 
            // dgvNoExistentes
            // 
            dgvNoExistentes.AllowUserToAddRows = false;
            dgvNoExistentes.AllowUserToDeleteRows = false;
            dgvNoExistentes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNoExistentes.BackgroundColor = Color.White;
            dgvNoExistentes.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(250, 230, 225);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(130, 30, 20);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvNoExistentes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvNoExistentes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvNoExistentes.EnableHeadersVisualStyles = false;
            dgvNoExistentes.Location = new Point(0, 56);
            dgvNoExistentes.Name = "dgvNoExistentes";
            dgvNoExistentes.RowHeadersWidth = 30;
            dgvNoExistentes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNoExistentes.Size = new Size(388, 393);
            dgvNoExistentes.TabIndex = 1;
            // 
            // lblHeaderNoExistentes
            // 
            lblHeaderNoExistentes.BackColor = Color.FromArgb(192, 57, 43);
            lblHeaderNoExistentes.Dock = DockStyle.Top;
            lblHeaderNoExistentes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHeaderNoExistentes.ForeColor = Color.White;
            lblHeaderNoExistentes.Location = new Point(0, 0);
            lblHeaderNoExistentes.Name = "lblHeaderNoExistentes";
            lblHeaderNoExistentes.Padding = new Padding(10, 0, 0, 0);
            lblHeaderNoExistentes.Size = new Size(388, 32);
            lblHeaderNoExistentes.TabIndex = 0;
            lblHeaderNoExistentes.Text = "✘  Diferencias detectadas";
            lblHeaderNoExistentes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelExistentes
            // 
            panelExistentes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelExistentes.BorderStyle = BorderStyle.FixedSingle;
            panelExistentes.Controls.Add(dgvExistentes);
            panelExistentes.Controls.Add(lblHeaderExistentes);
            panelExistentes.Controls.Add(btnAgregarGrid);
            panelExistentes.Location = new Point(14, 10);
            panelExistentes.Name = "panelExistentes";
            panelExistentes.Size = new Size(390, 492);
            panelExistentes.TabIndex = 0;
            // 
            // dgvExistentes
            // 
            dgvExistentes.AllowUserToAddRows = false;
            dgvExistentes.AllowUserToDeleteRows = false;
            dgvExistentes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvExistentes.BackgroundColor = Color.White;
            dgvExistentes.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(220, 245, 230);
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(30, 80, 50);
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvExistentes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvExistentes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvExistentes.EnableHeadersVisualStyles = false;
            dgvExistentes.Location = new Point(0, 56);
            dgvExistentes.Name = "dgvExistentes";
            dgvExistentes.ReadOnly = true;
            dgvExistentes.RowHeadersWidth = 30;
            dgvExistentes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvExistentes.Size = new Size(388, 393);
            dgvExistentes.TabIndex = 1;
            // 
            // lblHeaderExistentes
            // 
            lblHeaderExistentes.BackColor = Color.FromArgb(30, 150, 70);
            lblHeaderExistentes.Dock = DockStyle.Top;
            lblHeaderExistentes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHeaderExistentes.ForeColor = Color.White;
            lblHeaderExistentes.Location = new Point(0, 0);
            lblHeaderExistentes.Name = "lblHeaderExistentes";
            lblHeaderExistentes.Padding = new Padding(10, 0, 0, 0);
            lblHeaderExistentes.Size = new Size(388, 32);
            lblHeaderExistentes.TabIndex = 0;
            lblHeaderExistentes.Text = "✔  Campos Correctos";
            lblHeaderExistentes.TextAlign = ContentAlignment.MiddleLeft;
            lblHeaderExistentes.Click += lblHeaderExistentes_Click;
            // 
            // btnAgregarGrid
            // 
            btnAgregarGrid.Location = new Point(0, 0);
            btnAgregarGrid.Name = "btnAgregarGrid";
            btnAgregarGrid.Size = new Size(75, 23);
            btnAgregarGrid.TabIndex = 2;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(245, 247, 250);
            panelBotones.Controls.Add(btnCerrar);
            panelBotones.Controls.Add(btnAgregarValidas);
            panelBotones.Controls.Add(btnAplicarCorrecciones);
            panelBotones.Controls.Add(btnAgregar);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 532);
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(10, 10, 14, 10);
            panelBotones.Size = new Size(840, 56);
            panelBotones.TabIndex = 3;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.BackColor = Color.FromArgb(100, 110, 120);
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Location = new Point(716, 10);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(110, 36);
            btnCerrar.TabIndex = 0;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // btnAgregarFila
            // 
            btnAgregarFila.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAgregarFila.BackColor = Color.FromArgb(52, 152, 219);
            btnAgregarFila.FlatAppearance.BorderSize = 0;
            btnAgregarFila.FlatStyle = FlatStyle.Flat;
            btnAgregarFila.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregarFila.ForeColor = Color.White;
            btnAgregarFila.Location = new Point(87, 386);
            btnAgregarFila.Name = "btnAgregarFila";
            btnAgregarFila.Size = new Size(110, 36);
            btnAgregarFila.TabIndex = 2;
            btnAgregarFila.Text = "Agregar fila";
            btnAgregarFila.UseVisualStyleBackColor = false;
            btnAgregarFila.Click += btnAgregarFila_Click_1;
            // 
            // btnAgregarValidas
            // 
            btnAgregarValidas.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAgregarValidas.BackColor = Color.FromArgb(39, 174, 96);
            btnAgregarValidas.FlatAppearance.BorderSize = 0;
            btnAgregarValidas.FlatStyle = FlatStyle.Flat;
            btnAgregarValidas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregarValidas.ForeColor = Color.White;
            btnAgregarValidas.Location = new Point(484, 10);
            btnAgregarValidas.Name = "btnAgregarValidas";
            btnAgregarValidas.Size = new Size(110, 36);
            btnAgregarValidas.TabIndex = 2;
            btnAgregarValidas.Text = "Agregar Filas";
            btnAgregarValidas.UseVisualStyleBackColor = false;
            btnAgregarValidas.Click += btnAgregarValidas_Click;
            // 
            // btnAplicarCorrecciones
            // 
            btnAplicarCorrecciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAplicarCorrecciones.BackColor = Color.FromArgb(39, 174, 96);
            btnAplicarCorrecciones.FlatAppearance.BorderSize = 0;
            btnAplicarCorrecciones.FlatStyle = FlatStyle.Flat;
            btnAplicarCorrecciones.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAplicarCorrecciones.ForeColor = Color.White;
            btnAplicarCorrecciones.Location = new Point(600, 10);
            btnAplicarCorrecciones.Name = "btnAplicarCorrecciones";
            btnAplicarCorrecciones.Size = new Size(110, 36);
            btnAplicarCorrecciones.TabIndex = 1;
            btnAplicarCorrecciones.Text = "Corregir Campos";
            btnAplicarCorrecciones.UseVisualStyleBackColor = false;
            btnAplicarCorrecciones.Click += btnAplicarCorrecciones_Click;
            // 
            // btnAgregar
            // 
            btnAgregar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAgregar.BackColor = Color.FromArgb(39, 174, 96);
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregar.ForeColor = Color.White;
            btnAgregar.Location = new Point(368, 10);
            btnAgregar.Name = "btnAgregar";
            btnAgregar.Size = new Size(110, 36);
            btnAgregar.TabIndex = 1;
            btnAgregar.Text = "Finalizar";
            btnAgregar.UseVisualStyleBackColor = false;
            btnAgregar.Click += btnAgregar_Click;
            // 
            // FrmResultadoVerificacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(840, 588);
            Controls.Add(panelBotones);
            Controls.Add(panelResumen);
            Controls.Add(panelTablas);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmResultadoVerificacion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resultado de Verificación de Partes";
            panelHeader.ResumeLayout(false);
            panelResumen.ResumeLayout(false);
            panelResumen.PerformLayout();
            panelTablas.ResumeLayout(false);
            panelNoExistentes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvNoExistentes).EndInit();
            panelExistentes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExistentes).EndInit();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        // 
        // btnAplicarCorrecciones (declaración)
        // 

        // btnAgregarExistentes (declaración)
        // 

        // btnAgregarFila (declaración)
        // 
        private System.Windows.Forms.Button btnAplicarCorrecciones;
        private System.Windows.Forms.Button btnAgregarExistentes;
        private System.Windows.Forms.Button btnAgregarFila;
        private System.Windows.Forms.Button btnAgregarValidas;

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.Panel panelResumen;
        private System.Windows.Forms.Label lblCardConsultados;
        private System.Windows.Forms.Label lblTotalConsultados;
        private System.Windows.Forms.Label lblCardExistentes;
        private System.Windows.Forms.Label lblTotalExistentes;
        private System.Windows.Forms.Label lblCardNoExistentes;
        private System.Windows.Forms.Label lblTotalNoExistentes;
        private System.Windows.Forms.Panel panelTablas;
        private System.Windows.Forms.Panel panelExistentes;
        private System.Windows.Forms.Label lblHeaderExistentes;
        private System.Windows.Forms.Button btnAgregarGrid;
        private System.Windows.Forms.DataGridView dgvExistentes;
        private System.Windows.Forms.Panel panelNoExistentes;
        private System.Windows.Forms.Label lblHeaderNoExistentes;
        private System.Windows.Forms.DataGridView dgvNoExistentes;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnAgregar;
        private Label lblFecha;
    }
}
