namespace Retorno360Tacna.FORMS
{
    partial class FrmConfiguracion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmConfiguracion));
            groupBoxInterfaz = new GroupBox();
            btnVistaPrevia = new Button();
            lblVistaPrevia = new Label();
            lblEscalaActual = new Label();
            cmbEscalaUI = new ComboBox();
            lblEscalaUI = new Label();
            lblDescripcion = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelBotones = new Panel();
            lblTitulo = new Label();
            groupBoxInterfaz.SuspendLayout();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxInterfaz
            // 
            groupBoxInterfaz.Controls.Add(btnVistaPrevia);
            groupBoxInterfaz.Controls.Add(lblVistaPrevia);
            groupBoxInterfaz.Controls.Add(lblEscalaActual);
            groupBoxInterfaz.Controls.Add(cmbEscalaUI);
            groupBoxInterfaz.Controls.Add(lblEscalaUI);
            groupBoxInterfaz.Controls.Add(lblDescripcion);
            groupBoxInterfaz.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBoxInterfaz.ForeColor = Color.FromArgb(52, 73, 94);
            groupBoxInterfaz.Location = new Point(20, 80);
            groupBoxInterfaz.Name = "groupBoxInterfaz";
            groupBoxInterfaz.Size = new Size(560, 250);
            groupBoxInterfaz.TabIndex = 1;
            groupBoxInterfaz.TabStop = false;
            groupBoxInterfaz.Text = "Configuración de Pantalla";
            // 
            // btnVistaPrevia
            // 
            btnVistaPrevia.BackColor = Color.FromArgb(52, 152, 219);
            btnVistaPrevia.Cursor = Cursors.Hand;
            btnVistaPrevia.FlatAppearance.BorderSize = 0;
            btnVistaPrevia.FlatStyle = FlatStyle.Flat;
            btnVistaPrevia.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnVistaPrevia.ForeColor = Color.White;
            btnVistaPrevia.Location = new Point(400, 156);
            btnVistaPrevia.Name = "btnVistaPrevia";
            btnVistaPrevia.Size = new Size(130, 35);
            btnVistaPrevia.TabIndex = 5;
            btnVistaPrevia.Text = "Vista Previa";
            btnVistaPrevia.UseVisualStyleBackColor = false;
            btnVistaPrevia.Click += btnVistaPrevia_Click;
            // 
            // lblVistaPrevia
            // 
            lblVistaPrevia.AutoSize = true;
            lblVistaPrevia.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblVistaPrevia.ForeColor = Color.FromArgb(127, 140, 141);
            lblVistaPrevia.Location = new Point(30, 205);
            lblVistaPrevia.Name = "lblVistaPrevia";
            lblVistaPrevia.Size = new Size(104, 15);
            lblVistaPrevia.TabIndex = 4;
            lblVistaPrevia.Text = "Vista previa: 100%";
            // 
            // lblEscalaActual
            // 
            lblEscalaActual.AutoSize = true;
            lblEscalaActual.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEscalaActual.ForeColor = Color.FromArgb(39, 174, 96);
            lblEscalaActual.Location = new Point(30, 175);
            lblEscalaActual.Name = "lblEscalaActual";
            lblEscalaActual.Size = new Size(112, 15);
            lblEscalaActual.TabIndex = 3;
            lblEscalaActual.Text = "Escala actual: 100%";
            // 
            // cmbEscalaUI
            // 
            cmbEscalaUI.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEscalaUI.Font = new Font("Segoe UI", 10F);
            cmbEscalaUI.FormattingEnabled = true;
            cmbEscalaUI.Items.AddRange(new object[] { "100% (Predeterminado)", "125% (Recomendado para pantallas Full HD)", "150%", "175%", "200%" });
            cmbEscalaUI.Location = new Point(180, 125);
            cmbEscalaUI.Name = "cmbEscalaUI";
            cmbEscalaUI.Size = new Size(350, 25);
            cmbEscalaUI.TabIndex = 2;
            cmbEscalaUI.SelectedIndexChanged += cmbEscalaUI_SelectedIndexChanged;
            // 
            // lblEscalaUI
            // 
            lblEscalaUI.AutoSize = true;
            lblEscalaUI.Font = new Font("Segoe UI", 10F);
            lblEscalaUI.Location = new Point(30, 128);
            lblEscalaUI.Name = "lblEscalaUI";
            lblEscalaUI.Size = new Size(119, 19);
            lblEscalaUI.TabIndex = 1;
            lblEscalaUI.Text = "Escala de Pantalla:";
            // 
            // lblDescripcion
            // 
            lblDescripcion.Font = new Font("Segoe UI", 9.5F);
            lblDescripcion.ForeColor = Color.FromArgb(127, 140, 141);
            lblDescripcion.Location = new Point(30, 35);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new Size(500, 70);
            lblDescripcion.TabIndex = 0;
            lblDescripcion.Text = resources.GetString("lblDescripcion.Text");
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(39, 174, 96);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(340, 15);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(120, 40);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.FromArgb(231, 76, 60);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Location = new Point(470, 15);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(120, 40);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // panelBotones
            // 
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 350);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(600, 70);
            panelBotones.TabIndex = 2;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(158, 30);
            lblTitulo.TabIndex = 3;
            lblTitulo.Text = "Configuración";
            // 
            // FrmConfiguracion
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(600, 420);
            Controls.Add(lblTitulo);
            Controls.Add(panelBotones);
            Controls.Add(groupBoxInterfaz);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmConfiguracion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configuración - Retorno 360 Tacna";
            Load += FrmConfiguracion_Load;
            groupBoxInterfaz.ResumeLayout(false);
            groupBoxInterfaz.PerformLayout();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxInterfaz;
        private ComboBox cmbEscalaUI;
        private Label lblEscalaUI;
        private Label lblDescripcion;
        private Button btnGuardar;
        private Button btnCancelar;
        private Panel panelBotones;
        private Label lblTitulo;
        private Label lblEscalaActual;
        private Label lblVistaPrevia;
        private Button btnVistaPrevia;
    }
}
