namespace Retorno360Tacna.FORMS
{
    partial class FrmDetallePedimentos
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
            lblTitulo = new Label();
            dgvDetalle = new DataGridView();
            panelHeader = new Panel();
            btnCalculadora = new Button();
            btnCerrar = new Button();
            lblResumen = new Label();
            panelCalculadora = new Panel();
            btnCalcCerrar = new Button();
            lblCalcTitulo = new Label();
            txtDisplay = new TextBox();
            btnCalcClear = new Button();
            btnCalcDivide = new Button();
            btnCalcMultiply = new Button();
            btnCalcSubtract = new Button();
            btnCalcAdd = new Button();
            btnCalcEquals = new Button();
            btnCalcDecimal = new Button();
            btnCalc0 = new Button();
            btnCalc9 = new Button();
            btnCalc8 = new Button();
            btnCalc7 = new Button();
            btnCalc6 = new Button();
            btnCalc5 = new Button();
            btnCalc4 = new Button();
            btnCalc3 = new Button();
            btnCalc2 = new Button();
            btnCalc1 = new Button();
            panelBorder = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).BeginInit();
            panelHeader.SuspendLayout();
            panelCalculadora.SuspendLayout();
            panelBorder.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(15, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(183, 21);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Detalle de Pedimentos";
            // 
            // dgvDetalle
            // 
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.BorderStyle = BorderStyle.None;
            dgvDetalle.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDetalle.Location = new Point(12, 80);
            dgvDetalle.MultiSelect = false;
            dgvDetalle.Name = "dgvDetalle";
            dgvDetalle.ReadOnly = true;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.Size = new Size(776, 420);
            dgvDetalle.TabIndex = 1;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            panelHeader.Controls.Add(btnCalculadora);
            panelHeader.Controls.Add(btnCerrar);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(2, 2);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(800, 47);
            panelHeader.TabIndex = 2;
            // 
            // btnCalculadora
            // 
            btnCalculadora.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCalculadora.BackColor = Color.FromArgb(39, 174, 96);
            btnCalculadora.Cursor = Cursors.Hand;
            btnCalculadora.FlatAppearance.BorderSize = 0;
            btnCalculadora.FlatStyle = FlatStyle.Flat;
            btnCalculadora.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCalculadora.ForeColor = Color.White;
            btnCalculadora.Image = Properties.Resources.caculator_icon_icons_com_52399__1_;
            btnCalculadora.Location = new Point(694, 4);
            btnCalculadora.Name = "btnCalculadora";
            btnCalculadora.Size = new Size(45, 40);
            btnCalculadora.TabIndex = 2;
            btnCalculadora.UseVisualStyleBackColor = false;
            btnCalculadora.Click += btnCalculadora_Click;
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
            btnCerrar.Location = new Point(745, 4);
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
            lblResumen.Location = new Point(12, 52);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(141, 19);
            lblResumen.TabIndex = 3;
            lblResumen.Text = "Total Pedimentos: 0";
            // 
            // panelCalculadora
            // 
            panelCalculadora.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelCalculadora.BackColor = Color.White;
            panelCalculadora.BorderStyle = BorderStyle.FixedSingle;
            panelCalculadora.Controls.Add(btnCalcCerrar);
            panelCalculadora.Controls.Add(lblCalcTitulo);
            panelCalculadora.Controls.Add(txtDisplay);
            panelCalculadora.Controls.Add(btnCalcClear);
            panelCalculadora.Controls.Add(btnCalcDivide);
            panelCalculadora.Controls.Add(btnCalcMultiply);
            panelCalculadora.Controls.Add(btnCalcSubtract);
            panelCalculadora.Controls.Add(btnCalcAdd);
            panelCalculadora.Controls.Add(btnCalcEquals);
            panelCalculadora.Controls.Add(btnCalcDecimal);
            panelCalculadora.Controls.Add(btnCalc0);
            panelCalculadora.Controls.Add(btnCalc9);
            panelCalculadora.Controls.Add(btnCalc8);
            panelCalculadora.Controls.Add(btnCalc7);
            panelCalculadora.Controls.Add(btnCalc6);
            panelCalculadora.Controls.Add(btnCalc5);
            panelCalculadora.Controls.Add(btnCalc4);
            panelCalculadora.Controls.Add(btnCalc3);
            panelCalculadora.Controls.Add(btnCalc2);
            panelCalculadora.Controls.Add(btnCalc1);
            panelCalculadora.Location = new Point(550, 80);
            panelCalculadora.Name = "panelCalculadora";
            panelCalculadora.Size = new Size(240, 350);
            panelCalculadora.TabIndex = 4;
            panelCalculadora.Visible = false;
            // 
            // btnCalcCerrar
            // 
            btnCalcCerrar.BackColor = Color.FromArgb(192, 57, 43);
            btnCalcCerrar.Cursor = Cursors.Hand;
            btnCalcCerrar.FlatAppearance.BorderSize = 0;
            btnCalcCerrar.FlatStyle = FlatStyle.Flat;
            btnCalcCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCalcCerrar.ForeColor = Color.White;
            btnCalcCerrar.Location = new Point(205, 5);
            btnCalcCerrar.Name = "btnCalcCerrar";
            btnCalcCerrar.Size = new Size(30, 25);
            btnCalcCerrar.TabIndex = 20;
            btnCalcCerrar.Text = "✖";
            btnCalcCerrar.UseVisualStyleBackColor = false;
            btnCalcCerrar.Click += btnCalcCerrar_Click;
            // 
            // lblCalcTitulo
            // 
            lblCalcTitulo.AutoSize = true;
            lblCalcTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCalcTitulo.ForeColor = Color.FromArgb(41, 128, 185);
            lblCalcTitulo.Location = new Point(10, 8);
            lblCalcTitulo.Name = "lblCalcTitulo";
            lblCalcTitulo.Size = new Size(89, 19);
            lblCalcTitulo.TabIndex = 19;
            lblCalcTitulo.Text = "Calculadora";
            // 
            // txtDisplay
            // 
            txtDisplay.BackColor = Color.FromArgb(236, 240, 241);
            txtDisplay.BorderStyle = BorderStyle.FixedSingle;
            txtDisplay.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            txtDisplay.Location = new Point(10, 35);
            txtDisplay.Name = "txtDisplay";
            txtDisplay.ReadOnly = true;
            txtDisplay.Size = new Size(220, 39);
            txtDisplay.TabIndex = 0;
            txtDisplay.Text = "0";
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.KeyDown += txtDisplay_KeyDown;
            // 
            // btnCalcClear
            // 
            btnCalcClear.BackColor = Color.FromArgb(231, 76, 60);
            btnCalcClear.Cursor = Cursors.Hand;
            btnCalcClear.FlatStyle = FlatStyle.Flat;
            btnCalcClear.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcClear.ForeColor = Color.White;
            btnCalcClear.Location = new Point(120, 80);
            btnCalcClear.Name = "btnCalcClear";
            btnCalcClear.Size = new Size(110, 50);
            btnCalcClear.TabIndex = 18;
            btnCalcClear.Text = "C";
            btnCalcClear.UseVisualStyleBackColor = false;
            btnCalcClear.Click += btnCalcClear_Click;
            // 
            // btnCalcDivide
            // 
            btnCalcDivide.BackColor = Color.FromArgb(52, 152, 219);
            btnCalcDivide.Cursor = Cursors.Hand;
            btnCalcDivide.FlatStyle = FlatStyle.Flat;
            btnCalcDivide.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcDivide.ForeColor = Color.White;
            btnCalcDivide.Location = new Point(10, 80);
            btnCalcDivide.Name = "btnCalcDivide";
            btnCalcDivide.Size = new Size(50, 50);
            btnCalcDivide.TabIndex = 17;
            btnCalcDivide.Text = "/";
            btnCalcDivide.UseVisualStyleBackColor = false;
            btnCalcDivide.Click += btnOperator_Click;
            // 
            // btnCalcMultiply
            // 
            btnCalcMultiply.BackColor = Color.FromArgb(52, 152, 219);
            btnCalcMultiply.Cursor = Cursors.Hand;
            btnCalcMultiply.FlatStyle = FlatStyle.Flat;
            btnCalcMultiply.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcMultiply.ForeColor = Color.White;
            btnCalcMultiply.Location = new Point(65, 80);
            btnCalcMultiply.Name = "btnCalcMultiply";
            btnCalcMultiply.Size = new Size(50, 50);
            btnCalcMultiply.TabIndex = 16;
            btnCalcMultiply.Text = "*";
            btnCalcMultiply.UseVisualStyleBackColor = false;
            btnCalcMultiply.Click += btnOperator_Click;
            // 
            // btnCalcSubtract
            // 
            btnCalcSubtract.BackColor = Color.FromArgb(52, 152, 219);
            btnCalcSubtract.Cursor = Cursors.Hand;
            btnCalcSubtract.FlatStyle = FlatStyle.Flat;
            btnCalcSubtract.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcSubtract.ForeColor = Color.White;
            btnCalcSubtract.Location = new Point(180, 135);
            btnCalcSubtract.Name = "btnCalcSubtract";
            btnCalcSubtract.Size = new Size(50, 105);
            btnCalcSubtract.TabIndex = 15;
            btnCalcSubtract.Text = "-";
            btnCalcSubtract.UseVisualStyleBackColor = false;
            btnCalcSubtract.Click += btnOperator_Click;
            // 
            // btnCalcAdd
            // 
            btnCalcAdd.BackColor = Color.FromArgb(52, 152, 219);
            btnCalcAdd.Cursor = Cursors.Hand;
            btnCalcAdd.FlatStyle = FlatStyle.Flat;
            btnCalcAdd.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcAdd.ForeColor = Color.White;
            btnCalcAdd.Location = new Point(180, 245);
            btnCalcAdd.Name = "btnCalcAdd";
            btnCalcAdd.Size = new Size(50, 95);
            btnCalcAdd.TabIndex = 14;
            btnCalcAdd.Text = "+";
            btnCalcAdd.UseVisualStyleBackColor = false;
            btnCalcAdd.Click += btnOperator_Click;
            // 
            // btnCalcEquals
            // 
            btnCalcEquals.BackColor = Color.FromArgb(39, 174, 96);
            btnCalcEquals.Cursor = Cursors.Hand;
            btnCalcEquals.FlatStyle = FlatStyle.Flat;
            btnCalcEquals.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcEquals.ForeColor = Color.White;
            btnCalcEquals.Location = new Point(120, 290);
            btnCalcEquals.Name = "btnCalcEquals";
            btnCalcEquals.Size = new Size(55, 50);
            btnCalcEquals.TabIndex = 13;
            btnCalcEquals.Text = "=";
            btnCalcEquals.UseVisualStyleBackColor = false;
            btnCalcEquals.Click += btnEquals_Click;
            // 
            // btnCalcDecimal
            // 
            btnCalcDecimal.BackColor = Color.FromArgb(189, 195, 199);
            btnCalcDecimal.Cursor = Cursors.Hand;
            btnCalcDecimal.FlatStyle = FlatStyle.Flat;
            btnCalcDecimal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalcDecimal.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalcDecimal.Location = new Point(65, 290);
            btnCalcDecimal.Name = "btnCalcDecimal";
            btnCalcDecimal.Size = new Size(50, 50);
            btnCalcDecimal.TabIndex = 12;
            btnCalcDecimal.Text = ".";
            btnCalcDecimal.UseVisualStyleBackColor = false;
            btnCalcDecimal.Click += btnNumber_Click;
            // 
            // btnCalc0
            // 
            btnCalc0.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc0.Cursor = Cursors.Hand;
            btnCalc0.FlatStyle = FlatStyle.Flat;
            btnCalc0.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc0.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc0.Location = new Point(10, 290);
            btnCalc0.Name = "btnCalc0";
            btnCalc0.Size = new Size(50, 50);
            btnCalc0.TabIndex = 11;
            btnCalc0.Text = "0";
            btnCalc0.UseVisualStyleBackColor = false;
            btnCalc0.Click += btnNumber_Click;
            // 
            // btnCalc9
            // 
            btnCalc9.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc9.Cursor = Cursors.Hand;
            btnCalc9.FlatStyle = FlatStyle.Flat;
            btnCalc9.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc9.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc9.Location = new Point(120, 135);
            btnCalc9.Name = "btnCalc9";
            btnCalc9.Size = new Size(55, 50);
            btnCalc9.TabIndex = 10;
            btnCalc9.Text = "9";
            btnCalc9.UseVisualStyleBackColor = false;
            btnCalc9.Click += btnNumber_Click;
            // 
            // btnCalc8
            // 
            btnCalc8.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc8.Cursor = Cursors.Hand;
            btnCalc8.FlatStyle = FlatStyle.Flat;
            btnCalc8.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc8.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc8.Location = new Point(65, 135);
            btnCalc8.Name = "btnCalc8";
            btnCalc8.Size = new Size(50, 50);
            btnCalc8.TabIndex = 9;
            btnCalc8.Text = "8";
            btnCalc8.UseVisualStyleBackColor = false;
            btnCalc8.Click += btnNumber_Click;
            // 
            // btnCalc7
            // 
            btnCalc7.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc7.Cursor = Cursors.Hand;
            btnCalc7.FlatStyle = FlatStyle.Flat;
            btnCalc7.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc7.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc7.Location = new Point(10, 135);
            btnCalc7.Name = "btnCalc7";
            btnCalc7.Size = new Size(50, 50);
            btnCalc7.TabIndex = 8;
            btnCalc7.Text = "7";
            btnCalc7.UseVisualStyleBackColor = false;
            btnCalc7.Click += btnNumber_Click;
            // 
            // btnCalc6
            // 
            btnCalc6.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc6.Cursor = Cursors.Hand;
            btnCalc6.FlatStyle = FlatStyle.Flat;
            btnCalc6.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc6.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc6.Location = new Point(120, 190);
            btnCalc6.Name = "btnCalc6";
            btnCalc6.Size = new Size(55, 50);
            btnCalc6.TabIndex = 7;
            btnCalc6.Text = "6";
            btnCalc6.UseVisualStyleBackColor = false;
            btnCalc6.Click += btnNumber_Click;
            // 
            // btnCalc5
            // 
            btnCalc5.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc5.Cursor = Cursors.Hand;
            btnCalc5.FlatStyle = FlatStyle.Flat;
            btnCalc5.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc5.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc5.Location = new Point(65, 190);
            btnCalc5.Name = "btnCalc5";
            btnCalc5.Size = new Size(50, 50);
            btnCalc5.TabIndex = 6;
            btnCalc5.Text = "5";
            btnCalc5.UseVisualStyleBackColor = false;
            btnCalc5.Click += btnNumber_Click;
            // 
            // btnCalc4
            // 
            btnCalc4.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc4.Cursor = Cursors.Hand;
            btnCalc4.FlatStyle = FlatStyle.Flat;
            btnCalc4.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc4.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc4.Location = new Point(10, 190);
            btnCalc4.Name = "btnCalc4";
            btnCalc4.Size = new Size(50, 50);
            btnCalc4.TabIndex = 5;
            btnCalc4.Text = "4";
            btnCalc4.UseVisualStyleBackColor = false;
            btnCalc4.Click += btnNumber_Click;
            // 
            // btnCalc3
            // 
            btnCalc3.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc3.Cursor = Cursors.Hand;
            btnCalc3.FlatStyle = FlatStyle.Flat;
            btnCalc3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc3.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc3.Location = new Point(120, 245);
            btnCalc3.Name = "btnCalc3";
            btnCalc3.Size = new Size(55, 40);
            btnCalc3.TabIndex = 4;
            btnCalc3.Text = "3";
            btnCalc3.UseVisualStyleBackColor = false;
            btnCalc3.Click += btnNumber_Click;
            // 
            // btnCalc2
            // 
            btnCalc2.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc2.Cursor = Cursors.Hand;
            btnCalc2.FlatStyle = FlatStyle.Flat;
            btnCalc2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc2.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc2.Location = new Point(65, 245);
            btnCalc2.Name = "btnCalc2";
            btnCalc2.Size = new Size(50, 40);
            btnCalc2.TabIndex = 3;
            btnCalc2.Text = "2";
            btnCalc2.UseVisualStyleBackColor = false;
            btnCalc2.Click += btnNumber_Click;
            // 
            // btnCalc1
            // 
            btnCalc1.BackColor = Color.FromArgb(236, 240, 241);
            btnCalc1.Cursor = Cursors.Hand;
            btnCalc1.FlatStyle = FlatStyle.Flat;
            btnCalc1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCalc1.ForeColor = Color.FromArgb(44, 62, 80);
            btnCalc1.Location = new Point(10, 245);
            btnCalc1.Name = "btnCalc1";
            btnCalc1.Size = new Size(50, 40);
            btnCalc1.TabIndex = 2;
            btnCalc1.Text = "1";
            btnCalc1.UseVisualStyleBackColor = false;
            btnCalc1.Click += btnNumber_Click;
            // 
            // panelBorder
            // 
            panelBorder.BackColor = Color.FromArgb(41, 128, 185);
            panelBorder.Controls.Add(panelHeader);
            panelBorder.Controls.Add(dgvDetalle);
            panelBorder.Controls.Add(lblResumen);
            panelBorder.Controls.Add(panelCalculadora);
            panelBorder.Dock = DockStyle.Fill;
            panelBorder.Location = new Point(0, 0);
            panelBorder.Name = "panelBorder";
            panelBorder.Padding = new Padding(2);
            panelBorder.Size = new Size(804, 516);
            panelBorder.TabIndex = 5;
            // 
            // FrmDetallePedimentos
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(804, 516);
            Controls.Add(panelBorder);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmDetallePedimentos";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Detalle de Pedimentos";
            Load += FrmDetallePedimentos_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).EndInit();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelCalculadora.ResumeLayout(false);
            panelCalculadora.PerformLayout();
            panelBorder.ResumeLayout(false);
            panelBorder.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitulo;
        private DataGridView dgvDetalle;
        private Panel panelHeader;
        private Button btnCerrar;
        private Label lblResumen;
        private Button btnCalculadora;
        private Panel panelCalculadora;
        private TextBox txtDisplay;
        private Button btnCalc1;
        private Button btnCalc2;
        private Button btnCalc3;
        private Button btnCalc4;
        private Button btnCalc5;
        private Button btnCalc6;
        private Button btnCalc7;
        private Button btnCalc8;
        private Button btnCalc9;
        private Button btnCalc0;
        private Button btnCalcDecimal;
        private Button btnCalcEquals;
        private Button btnCalcAdd;
        private Button btnCalcSubtract;
        private Button btnCalcMultiply;
        private Button btnCalcDivide;
        private Button btnCalcClear;
        private Label lblCalcTitulo;
        private Button btnCalcCerrar;
        private Panel panelBorder;
    }
}
