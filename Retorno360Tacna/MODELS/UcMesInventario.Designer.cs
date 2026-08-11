namespace Retorno360Tacna.MODELS
{
    partial class UcMesInventario
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>

        private void InitializeComponent()
        {
            lblNumero = new Label();
            cmbMes = new ComboBox();
            btnSeleccionarExcel = new Button();
            lblArchivo = new Label();
            cmbHoja = new ComboBox();
            cmbTipoOperacion = new ComboBox();
            lblCampoA = new Label();
            lblCampoB = new Label();
            cmbCampoA = new ComboBox();
            cmbCampoB = new ComboBox();
            lblCampoTotal = new Label();
            cmbCampoTotal = new ComboBox();
            lblTotal = new Label();
            btnVerExcel = new Button();
            SuspendLayout();
            // 
            // lblNumero
            // 
            lblNumero.AutoSize = true;
            lblNumero.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNumero.Location = new Point(10, 10);
            lblNumero.Name = "lblNumero";
            lblNumero.Size = new Size(21, 15);
            lblNumero.TabIndex = 12;
            lblNumero.Text = "#1";
            // 
            // cmbMes
            // 
            cmbMes.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMes.FormattingEnabled = true;
            cmbMes.Location = new Point(60, 8);
            cmbMes.Name = "cmbMes";
            cmbMes.Size = new Size(110, 23);
            cmbMes.TabIndex = 11;
            // 
            // btnSeleccionarExcel
            // 
            btnSeleccionarExcel.Location = new Point(180, 7);
            btnSeleccionarExcel.Name = "btnSeleccionarExcel";
            btnSeleccionarExcel.Size = new Size(120, 25);
            btnSeleccionarExcel.TabIndex = 10;
            btnSeleccionarExcel.Text = "Seleccionar Excel...";
            btnSeleccionarExcel.UseVisualStyleBackColor = true;
            btnSeleccionarExcel.Click += btnSeleccionarExcel_Click;
            // 
            // lblArchivo
            // 
            lblArchivo.AutoSize = true;
            lblArchivo.ForeColor = Color.Gray;
            lblArchivo.Location = new Point(310, 12);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new Size(137, 15);
            lblArchivo.TabIndex = 9;
            lblArchivo.Text = "Sin archivo seleccionado";
            // 
            // cmbHoja
            // 
            cmbHoja.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHoja.FormattingEnabled = true;
            cmbHoja.Location = new Point(486, 5);
            cmbHoja.Name = "cmbHoja";
            cmbHoja.Size = new Size(140, 23);
            cmbHoja.TabIndex = 8;
            cmbHoja.SelectedIndexChanged += cmbHoja_SelectedIndexChanged;
            // 
            // cmbTipoOperacion
            // 
            cmbTipoOperacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoOperacion.FormattingEnabled = true;
            cmbTipoOperacion.Location = new Point(10, 45);
            cmbTipoOperacion.Name = "cmbTipoOperacion";
            cmbTipoOperacion.Size = new Size(230, 23);
            cmbTipoOperacion.TabIndex = 6;
            cmbTipoOperacion.SelectedIndexChanged += cmbTipoOperacion_SelectedIndexChanged;
            // 
            // lblCampoA
            // 
            lblCampoA.AutoSize = true;
            lblCampoA.Location = new Point(250, 49);
            lblCampoA.Name = "lblCampoA";
            lblCampoA.Size = new Size(60, 15);
            lblCampoA.TabIndex = 5;
            lblCampoA.Text = "Campo A:";
            // 
            // lblCampoB
            // 
            lblCampoB.AutoSize = true;
            lblCampoB.Location = new Point(465, 49);
            lblCampoB.Name = "lblCampoB";
            lblCampoB.Size = new Size(59, 15);
            lblCampoB.TabIndex = 3;
            lblCampoB.Text = "Campo B:";
            // 
            // cmbCampoA
            // 
            cmbCampoA.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoA.FormattingEnabled = true;
            cmbCampoA.Location = new Point(315, 45);
            cmbCampoA.Name = "cmbCampoA";
            cmbCampoA.Size = new Size(140, 23);
            cmbCampoA.TabIndex = 4;
            cmbCampoA.SelectedIndexChanged += cmbCampoA_SelectedIndexChanged;
            // 
            // cmbCampoB
            // 
            cmbCampoB.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoB.FormattingEnabled = true;
            cmbCampoB.Location = new Point(530, 45);
            cmbCampoB.Name = "cmbCampoB";
            cmbCampoB.Size = new Size(140, 23);
            cmbCampoB.TabIndex = 2;
            cmbCampoB.SelectedIndexChanged += cmbCampoB_SelectedIndexChanged;
            // 
            // lblCampoTotal
            // 
            lblCampoTotal.AutoSize = true;
            lblCampoTotal.Location = new Point(250, 49);
            lblCampoTotal.Name = "lblCampoTotal";
            lblCampoTotal.Size = new Size(94, 15);
            lblCampoTotal.TabIndex = 1;
            lblCampoTotal.Text = "Campo a sumar:";
            // 
            // cmbCampoTotal
            // 
            cmbCampoTotal.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoTotal.FormattingEnabled = true;
            cmbCampoTotal.Location = new Point(360, 45);
            cmbCampoTotal.Name = "cmbCampoTotal";
            cmbCampoTotal.Size = new Size(180, 23);
            cmbCampoTotal.TabIndex = 0;
            cmbCampoTotal.SelectedIndexChanged += cmbCampoTotal_SelectedIndexChanged;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotal.Location = new Point(637, 11);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(50, 15);
            lblTotal.TabIndex = 7;
            lblTotal.Text = "Total: --";
            // 
            // btnVerExcel
            // 
            btnVerExcel.BackColor = Color.FromArgb(156, 213, 255);
            btnVerExcel.FlatAppearance.BorderSize = 0;
            btnVerExcel.FlatStyle = FlatStyle.Flat;
            btnVerExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnVerExcel.Image = Properties.Resources.eye_icon_172481;
            btnVerExcel.ImageAlign = ContentAlignment.MiddleRight;
            btnVerExcel.Location = new Point(737, 36);
            btnVerExcel.Name = "btnVerExcel";
            btnVerExcel.Size = new Size(83, 39);
            btnVerExcel.TabIndex = 13;
            btnVerExcel.Text = "Ver";
            btnVerExcel.TextAlign = ContentAlignment.MiddleLeft;
            btnVerExcel.UseVisualStyleBackColor = false;
            btnVerExcel.Click += btnVerExcel_Click;
            // 
            // UcMesInventario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(cmbCampoTotal);
            Controls.Add(lblCampoTotal);
            Controls.Add(cmbCampoB);
            Controls.Add(lblCampoB);
            Controls.Add(cmbCampoA);
            Controls.Add(lblCampoA);
            Controls.Add(cmbTipoOperacion);
            Controls.Add(lblTotal);
            Controls.Add(cmbHoja);
            Controls.Add(lblArchivo);
            Controls.Add(btnSeleccionarExcel);
            Controls.Add(cmbMes);
            Controls.Add(lblNumero);
            Controls.Add(btnVerExcel);
            Margin = new Padding(0, 0, 0, 8);
            Name = "UcMesInventario";
            Size = new Size(900, 85);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblNumero;
        private ComboBox cmbMes;
        private Button btnSeleccionarExcel;
        private Label lblArchivo;
        private ComboBox cmbHoja;
        private ComboBox cmbTipoOperacion;
        private Label lblCampoA;
        private Label lblCampoB;
        private ComboBox cmbCampoA;
        private ComboBox cmbCampoB;
        private Label lblCampoTotal;
        private ComboBox cmbCampoTotal;
        private Label lblTotal;
        private Button btnVerExcel;
    }
}
