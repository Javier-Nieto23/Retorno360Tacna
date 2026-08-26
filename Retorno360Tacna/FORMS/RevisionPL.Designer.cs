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
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            button1 = new Button();
            button2 = new Button();
            dataGridView1 = new DataGridView();
            button3 = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            chkUsarPerfil = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
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
            cmbCliente.Location = new Point(49, 105);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(203, 23);
            cmbCliente.TabIndex = 1;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(326, 28);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(139, 23);
            dateTimePicker1.TabIndex = 2;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new Point(326, 102);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(139, 23);
            dateTimePicker2.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(115, 166);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Cancelar";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(214, 166);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 5;
            button2.Text = "Confirmar";
            button2.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(49, 195);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(240, 150);
            dataGridView1.TabIndex = 6;
            // 
            // button3
            // 
            button3.Location = new Point(304, 322);
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
            label2.Location = new Point(49, 87);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 9;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(326, 10);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 10;
            label3.Text = "label3";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(326, 87);
            label4.Name = "label4";
            label4.Size = new Size(38, 15);
            label4.TabIndex = 11;
            label4.Text = "label4";
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Location = new Point(519, 30);
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
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button3);
            Controls.Add(dataGridView1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(cmbCliente);
            Controls.Add(cmbRazonSocial);
            Name = "RevisionPL";
            Text = "RevisionPL";
            Load += RevisionPL_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbRazonSocial;
        private ComboBox cmbCliente;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private Button button1;
        private Button button2;
        private DataGridView dataGridView1;
        private Button button3;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private CheckBox chkUsarPerfil;
    }
}