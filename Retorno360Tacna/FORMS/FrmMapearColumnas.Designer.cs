namespace Retorno360Tacna.FORMS
{
    partial class FrmMapearColumnas
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

        private void InitializeComponent()
        {
            header = new Panel();
            title = new Label();
            body = new Panel();
            label1 = new Label();
            lblAno = new Label();
            cmbAno = new ComboBox();
            cmbMes = new ComboBox();
            lblHoja = new Label();
            cmbHojas = new ComboBox();
            lblParte = new Label();
            cmbParte = new ComboBox();
            lblCant = new Label();
            cmbCant = new ComboBox();
            lblUm = new Label();
            cmbUm = new ComboBox();
            chkTotalCosto = new CheckBox();
            lblCostoUnitario = new Label();
            cmbCostoUnitario = new ComboBox();
            lblTotalCosto = new Label();
            cmbTotalCosto = new ComboBox();
            btnOk = new Button();
            btnCancel = new Button();
            header.SuspendLayout();
            body.SuspendLayout();
            SuspendLayout();
            // 
            // header
            // 
            header.BackColor = Color.FromArgb(30, 80, 50);
            header.Controls.Add(title);
            header.Dock = DockStyle.Top;
            header.Location = new Point(0, 0);
            header.Name = "header";
            header.Size = new Size(567, 56);
            header.TabIndex = 1;
            // 
            // title
            // 
            title.Dock = DockStyle.Left;
            title.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(0, 0);
            title.Name = "title";
            title.Padding = new Padding(12, 0, 0, 0);
            title.Size = new Size(567, 56);
            title.TabIndex = 1;
            title.Text = "Mapear columnas";
            title.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // body
            // 
            body.BackColor = Color.White;
            body.Controls.Add(label1);
            body.Controls.Add(lblAno);
            body.Controls.Add(cmbAno);
            body.Controls.Add(cmbMes);
            body.Controls.Add(lblHoja);
            body.Controls.Add(cmbHojas);
            body.Controls.Add(lblParte);
            body.Controls.Add(cmbParte);
            body.Controls.Add(lblCant);
            body.Controls.Add(cmbCant);
            body.Controls.Add(lblUm);
            body.Controls.Add(cmbUm);
            body.Controls.Add(chkTotalCosto);
            body.Controls.Add(lblCostoUnitario);
            body.Controls.Add(cmbCostoUnitario);
            body.Controls.Add(lblTotalCosto);
            body.Controls.Add(cmbTotalCosto);
            body.Controls.Add(btnOk);
            body.Controls.Add(btnCancel);
            body.Dock = DockStyle.Fill;
            body.Location = new Point(0, 56);
            body.Name = "body";
            body.Padding = new Padding(14);
            body.Size = new Size(567, 411);
            body.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(50, 60, 70);
            label1.Location = new Point(46, 19);
            label1.Name = "label1";
            label1.Size = new Size(103, 17);
            label1.TabIndex = 13;
            label1.Text = "Mes a verificar:";
            // 
            // lblAno
            // 
            lblAno.AutoSize = true;
            lblAno.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblAno.ForeColor = Color.FromArgb(50, 60, 70);
            lblAno.Location = new Point(282, 18);
            lblAno.Name = "lblAno";
            lblAno.Size = new Size(103, 17);
            lblAno.TabIndex = 12;
            lblAno.Text = "Año a verificar:";
            // 
            // cmbAno
            // 
            cmbAno.FormattingEnabled = true;
            cmbAno.Location = new Point(391, 18);
            cmbAno.Name = "cmbAno";
            cmbAno.Size = new Size(121, 23);
            cmbAno.TabIndex = 11;
            // 
            // cmbMes
            // 
            cmbMes.FormattingEnabled = true;
            cmbMes.Location = new Point(155, 18);
            cmbMes.Name = "cmbMes";
            cmbMes.Size = new Size(121, 23);
            cmbMes.TabIndex = 10;
            // 
            // lblHoja
            // 
            lblHoja.ForeColor = Color.FromArgb(40, 40, 40);
            lblHoja.Location = new Point(4, 103);
            lblHoja.Name = "lblHoja";
            lblHoja.Size = new Size(80, 23);
            lblHoja.TabIndex = 0;
            lblHoja.Text = "Hoja:";
            // 
            // cmbHojas
            // 
            cmbHojas.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHojas.Location = new Point(92, 99);
            cmbHojas.Name = "cmbHojas";
            cmbHojas.Size = new Size(420, 23);
            cmbHojas.TabIndex = 1;
            // 
            // lblParte
            // 
            lblParte.ForeColor = Color.FromArgb(40, 40, 40);
            lblParte.Location = new Point(4, 139);
            lblParte.Name = "lblParte";
            lblParte.Size = new Size(120, 23);
            lblParte.TabIndex = 2;
            lblParte.Text = "Columna Nº Parte:";
            // 
            // cmbParte
            // 
            cmbParte.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParte.Location = new Point(132, 135);
            cmbParte.Name = "cmbParte";
            cmbParte.Size = new Size(380, 23);
            cmbParte.TabIndex = 3;
            // 
            // lblCant
            // 
            lblCant.ForeColor = Color.FromArgb(40, 40, 40);
            lblCant.Location = new Point(4, 177);
            lblCant.Name = "lblCant";
            lblCant.Size = new Size(120, 23);
            lblCant.TabIndex = 4;
            lblCant.Text = "Columna Cantidad:";
            // 
            // cmbCant
            // 
            cmbCant.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCant.Location = new Point(132, 173);
            cmbCant.Name = "cmbCant";
            cmbCant.Size = new Size(380, 23);
            cmbCant.TabIndex = 5;
            // 
            // lblUm
            // 
            lblUm.ForeColor = Color.FromArgb(40, 40, 40);
            lblUm.Location = new Point(4, 215);
            lblUm.Name = "lblUm";
            lblUm.Size = new Size(120, 23);
            lblUm.TabIndex = 6;
            lblUm.Text = "Columna UM:";
            // 
            // cmbUm
            // 
            cmbUm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUm.Location = new Point(132, 211);
            cmbUm.Name = "cmbUm";
            cmbUm.Size = new Size(380, 23);
            cmbUm.TabIndex = 7;
            // 
            // chkTotalCosto
            // 
            chkTotalCosto.AutoSize = true;
            chkTotalCosto.Location = new Point(17, 262);
            chkTotalCosto.Name = "chkTotalCosto";
            chkTotalCosto.Size = new Size(214, 19);
            chkTotalCosto.TabIndex = 14;
            chkTotalCosto.Text = "Usar columna Costo unitario (Excel)";
            chkTotalCosto.UseVisualStyleBackColor = true;
            // 
            // lblCostoUnitario
            // 
            lblCostoUnitario.ForeColor = Color.FromArgb(40, 40, 40);
            lblCostoUnitario.Location = new Point(17, 296);
            lblCostoUnitario.Name = "lblCostoUnitario";
            lblCostoUnitario.Size = new Size(150, 23);
            lblCostoUnitario.TabIndex = 15;
            lblCostoUnitario.Text = "Columna Costo Unitario:";
            // 
            // cmbCostoUnitario
            // 
            cmbCostoUnitario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCostoUnitario.Location = new Point(168, 296);
            cmbCostoUnitario.Name = "cmbCostoUnitario";
            cmbCostoUnitario.Size = new Size(380, 23);
            cmbCostoUnitario.TabIndex = 16;
            // 
            // lblTotalCosto
            // 
            lblTotalCosto.ForeColor = Color.FromArgb(40, 40, 40);
            lblTotalCosto.Location = new Point(17, 328);
            lblTotalCosto.Name = "lblTotalCosto";
            lblTotalCosto.Size = new Size(150, 23);
            lblTotalCosto.TabIndex = 17;
            lblTotalCosto.Text = "Columna Total Costo (opcional):";
            // 
            // cmbTotalCosto
            // 
            cmbTotalCosto.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTotalCosto.Location = new Point(168, 328);
            cmbTotalCosto.Name = "cmbTotalCosto";
            cmbTotalCosto.Size = new Size(380, 23);
            cmbTotalCosto.TabIndex = 16;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.FromArgb(39, 174, 96);
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(362, 371);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(90, 23);
            btnOk.TabIndex = 8;
            btnOk.Text = "Aceptar";
            btnOk.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(200, 200, 200);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.Black;
            btnCancel.Location = new Point(464, 371);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 23);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // FrmMapearColumnas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.White;
            ClientSize = new Size(567, 467);
            Controls.Add(body);
            Controls.Add(header);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmMapearColumnas";
            StartPosition = FormStartPosition.CenterParent;
            header.ResumeLayout(false);
            body.ResumeLayout(false);
            body.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel header;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Panel body;
        private System.Windows.Forms.Label lblHoja;
        private System.Windows.Forms.ComboBox cmbHojas;
        private System.Windows.Forms.Label lblParte;
        private System.Windows.Forms.ComboBox cmbParte;
        private System.Windows.Forms.Label lblCant;
        private System.Windows.Forms.ComboBox cmbCant;
        private System.Windows.Forms.Label lblUm;
        private System.Windows.Forms.ComboBox cmbUm;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private ComboBox cmbAno;
        private ComboBox cmbMes;
        private Label lblAno;
        private Label label1;
        private CheckBox chkTotalCosto;
        private Label lblCostoUnitario;
        private ComboBox cmbCostoUnitario;
        private Label lblTotalCosto;
        private ComboBox cmbTotalCosto;
    }
}
