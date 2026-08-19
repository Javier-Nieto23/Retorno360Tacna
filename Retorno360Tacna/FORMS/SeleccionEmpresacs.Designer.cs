namespace Retorno360Tacna.FORMS
{
    partial class SeleccionEmpresacs
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
            cmbUsuario = new ComboBox();
            cmbEmpresa = new ComboBox();
            cmbRazon = new ComboBox();
            btnCancelar = new Button();
            btnConfirmar = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            groupBox1 = new GroupBox();
            label5 = new Label();
            label4 = new Label();
            btnFila = new Button();
            dtgPreview = new DataGridView();
            DgvEmpresasConfiguradas = new DataGridView();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dtgPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DgvEmpresasConfiguradas).BeginInit();
            SuspendLayout();
            // 
            // cmbUsuario
            // 
            cmbUsuario.FlatStyle = FlatStyle.Popup;
            cmbUsuario.FormattingEnabled = true;
            cmbUsuario.Location = new Point(6, 83);
            cmbUsuario.Name = "cmbUsuario";
            cmbUsuario.Size = new Size(259, 23);
            cmbUsuario.TabIndex = 1;
            // 
            // cmbEmpresa
            // 
            cmbEmpresa.FlatStyle = FlatStyle.Popup;
            cmbEmpresa.FormattingEnabled = true;
            cmbEmpresa.Location = new Point(290, 151);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(202, 23);
            cmbEmpresa.TabIndex = 2;
            // 
            // cmbRazon
            // 
            cmbRazon.FlatStyle = FlatStyle.Popup;
            cmbRazon.FormattingEnabled = true;
            cmbRazon.Location = new Point(6, 151);
            cmbRazon.Name = "cmbRazon";
            cmbRazon.Size = new Size(259, 23);
            cmbRazon.TabIndex = 3;
            cmbRazon.SelectedIndexChanged += cmbRazon_SelectedIndexChanged;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.FromArgb(231, 76, 60);
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Location = new Point(321, 239);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(130, 48);
            btnCancelar.TabIndex = 4;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // btnConfirmar
            // 
            btnConfirmar.BackColor = Color.FromArgb(39, 174, 96);
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.FlatStyle = FlatStyle.Flat;
            btnConfirmar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.Location = new Point(457, 239);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new Size(130, 48);
            btnConfirmar.TabIndex = 5;
            btnConfirmar.Text = "Guardar";
            btnConfirmar.UseVisualStyleBackColor = false;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 65);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 6;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 133);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 7;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(290, 133);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 8;
            label3.Text = "label3";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(DgvEmpresasConfiguradas);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(btnFila);
            groupBox1.Controls.Add(dtgPreview);
            groupBox1.Controls.Add(cmbRazon);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(cmbUsuario);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(cmbEmpresa);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(btnCancelar);
            groupBox1.Controls.Add(btnConfirmar);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1167, 550);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(650, 28);
            label5.Name = "label5";
            label5.Size = new Size(38, 15);
            label5.TabIndex = 12;
            label5.Text = "label5";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 28);
            label4.Name = "label4";
            label4.Size = new Size(38, 15);
            label4.TabIndex = 11;
            label4.Text = "label4";
            // 
            // btnFila
            // 
            btnFila.FlatAppearance.BorderSize = 0;
            btnFila.FlatStyle = FlatStyle.Flat;
            btnFila.Image = Properties.Resources.plus_40632__1_;
            btnFila.Location = new Point(498, 141);
            btnFila.Name = "btnFila";
            btnFila.Size = new Size(41, 41);
            btnFila.TabIndex = 10;
            btnFila.UseVisualStyleBackColor = true;
            btnFila.Click += btnFila_Click;
            // 
            // dtgPreview
            // 
            dtgPreview.BorderStyle = BorderStyle.Fixed3D;
            dtgPreview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtgPreview.Location = new Point(18, 293);
            dtgPreview.Name = "dtgPreview";
            dtgPreview.Size = new Size(569, 251);
            dtgPreview.TabIndex = 9;
            // 
            // DgvEmpresasConfiguradas
            // 
            DgvEmpresasConfiguradas.BorderStyle = BorderStyle.Fixed3D;
            DgvEmpresasConfiguradas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DgvEmpresasConfiguradas.Location = new Point(650, 65);
            DgvEmpresasConfiguradas.Name = "DgvEmpresasConfiguradas";
            DgvEmpresasConfiguradas.Size = new Size(497, 204);
            DgvEmpresasConfiguradas.TabIndex = 13;
            // 
            // SeleccionEmpresacs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1191, 681);
            Controls.Add(groupBox1);
            Name = "SeleccionEmpresacs";
            Text = "SeleccionEmpresacs";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dtgPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)DgvEmpresasConfiguradas).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cmbUsuario;
        private ComboBox cmbEmpresa;
        private ComboBox cmbRazon;
        private Button btnCancelar;
        private Button btnConfirmar;
        private Label label1;
        private Label label2;
        private Label label3;
        private GroupBox groupBox1;
        private Button btnFila;
        private DataGridView dtgPreview;
        private Label label4;
        private Label label5;
        private DataGridView DgvEmpresasConfiguradas;
    }
}