namespace Retorno360Tacna.FORMS
{
    partial class RevisionPL
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
            cmbRazonSocial = new ComboBox();
            cmbCliente = new ComboBox();
            button1 = new Button();
            button2 = new Button();
            panel1 = new DataGridView();
            button3 = new Button();
            label1 = new Label();
            label2 = new Label();
            chkUsarPerfil = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)panel1).BeginInit();
            SuspendLayout();
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(49, 28);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(203, 23);
            cmbRazonSocial.TabIndex = 0;
            // 
            // cmbCliente
            // 
            cmbCliente.FormattingEnabled = true;
            cmbCliente.Location = new Point(270, 28);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(203, 23);
            cmbCliente.TabIndex = 1;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(231, 76, 60);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            button1.ForeColor = Color.White;
            button1.Location = new Point(550, 508);
            button1.Name = "button1";
            button1.Size = new Size(92, 48);
            button1.TabIndex = 4;
            button1.Text = "Cancelar";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(39, 174, 96);
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            button2.ForeColor = Color.White;
            button2.Location = new Point(649, 508);
            button2.Name = "button2";
            button2.Size = new Size(92, 48);
            button2.TabIndex = 5;
            button2.Text = "Confirmar";
            button2.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            panel1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            panel1.Location = new Point(49, 88);
            panel1.Name = "panel1";
            panel1.Size = new Size(692, 404);
            panel1.TabIndex = 6;
            // 
            // button3
            // 
            button3.Location = new Point(516, 24);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 7;
            button3.Text = "SubirPL";
            button3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(49, 10);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 8;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(270, 10);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 9;
            label2.Text = "label2";
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Location = new Point(616, 30);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(82, 19);
            chkUsarPerfil.TabIndex = 12;
            chkUsarPerfil.Text = "checkBox1";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            // 
            // RevisionPL
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(950, 568);
            Controls.Add(chkUsarPerfil);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button3);
            Controls.Add(panel1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(cmbCliente);
            Controls.Add(cmbRazonSocial);
            Name = "RevisionPL";
            Text = "RevisionPL";
            Load += RevisionPL_Load;
            ((System.ComponentModel.ISupportInitialize)panel1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbRazonSocial;
        private ComboBox cmbCliente;
        private Button button1;
        private Button button2;
        private DataGridView panel1;
        private Button button3;
        private Label label1;
        private Label label2;
        private CheckBox chkUsarPerfil;
    }
}