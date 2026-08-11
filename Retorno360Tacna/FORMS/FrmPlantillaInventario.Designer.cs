using System.Drawing;
using System.Windows.Forms;

#nullable disable

namespace Retorno360Tacna.FORMS
{
    partial class FrmPlantillaInventario
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
            panelPrincipal = new Panel();
            panelContenido = new Panel();
            lblRazonSocial = new Label();
            cmbRazonSocial = new ComboBox();
            lblEmpresa = new Label();
            cmbEmpresa = new ComboBox();
            lblSep0 = new Label();
            lblArchivo = new Label();
            lblArchivoSeleccionado = new Label();
            btnSeleccionarArchivo = new Button();
            lblSeparador1 = new Label();
            lblHoja = new Label();
            cmbHoja = new ComboBox();
            lblOperacion = new Label();
            cmbOperacion = new ComboBox();
            lblSeparador2 = new Label();
            lblCampoTotal = new Label();
            cmbCampoTotal = new ComboBox();
            lblCampoA = new Label();
            cmbCampoA = new ComboBox();
            lblCampoB = new Label();
            cmbCampoB = new ComboBox();
            panelBotones = new Panel();
            btnRegresar = new Button();
            btnGuardar = new Button();
            panelPrincipal.SuspendLayout();
            panelContenido.SuspendLayout();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelPrincipal
            // 
            panelPrincipal.BackColor = Color.FromArgb(245, 247, 250);
            panelPrincipal.Controls.Add(panelContenido);
            panelPrincipal.Controls.Add(panelBotones);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Location = new Point(0, 0);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Size = new Size(640, 660);
            panelPrincipal.TabIndex = 0;
            // 
            // panelContenido
            // 
            panelContenido.AutoScroll = true;
            panelContenido.Controls.Add(lblRazonSocial);
            panelContenido.Controls.Add(cmbRazonSocial);
            panelContenido.Controls.Add(lblEmpresa);
            panelContenido.Controls.Add(cmbEmpresa);
            panelContenido.Controls.Add(lblSep0);
            panelContenido.Controls.Add(lblArchivo);
            panelContenido.Controls.Add(lblArchivoSeleccionado);
            panelContenido.Controls.Add(btnSeleccionarArchivo);
            panelContenido.Controls.Add(lblSeparador1);
            panelContenido.Controls.Add(lblHoja);
            panelContenido.Controls.Add(cmbHoja);
            panelContenido.Controls.Add(lblOperacion);
            panelContenido.Controls.Add(cmbOperacion);
            panelContenido.Controls.Add(lblSeparador2);
            panelContenido.Controls.Add(lblCampoTotal);
            panelContenido.Controls.Add(cmbCampoTotal);
            panelContenido.Controls.Add(lblCampoA);
            panelContenido.Controls.Add(cmbCampoA);
            panelContenido.Controls.Add(lblCampoB);
            panelContenido.Controls.Add(cmbCampoB);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(32, 20, 32, 10);
            panelContenido.Size = new Size(640, 600);
            panelContenido.TabIndex = 0;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRazonSocial.ForeColor = Color.FromArgb(50, 60, 70);
            lblRazonSocial.Location = new Point(32, 20);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(94, 19);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Razón Social";
            // 
            // cmbRazonSocial
            // 
            cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazonSocial.Font = new Font("Segoe UI", 10F);
            cmbRazonSocial.FormattingEnabled = true;
            cmbRazonSocial.Location = new Point(32, 44);
            cmbRazonSocial.Name = "cmbRazonSocial";
            cmbRazonSocial.Size = new Size(380, 25);
            cmbRazonSocial.TabIndex = 1;
            // 
            // lblEmpresa
            // 
            lblEmpresa.AutoSize = true;
            lblEmpresa.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEmpresa.ForeColor = Color.FromArgb(50, 60, 70);
            lblEmpresa.Location = new Point(32, 82);
            lblEmpresa.Name = "lblEmpresa";
            lblEmpresa.Size = new Size(66, 19);
            lblEmpresa.TabIndex = 2;
            lblEmpresa.Text = "Empresa";
            // 
            // cmbEmpresa
            // 
            cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmpresa.Font = new Font("Segoe UI", 10F);
            cmbEmpresa.FormattingEnabled = true;
            cmbEmpresa.Location = new Point(32, 106);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(380, 25);
            cmbEmpresa.TabIndex = 3;
            cmbEmpresa.SelectedIndexChanged += CmbEmpresa_SelectedIndexChanged;
            // 
            // lblSep0
            // 
            lblSep0.BackColor = Color.FromArgb(218, 226, 236);
            lblSep0.Location = new Point(32, 148);
            lblSep0.Name = "lblSep0";
            lblSep0.Size = new Size(576, 1);
            lblSep0.TabIndex = 4;
            // 
            // lblArchivo
            // 
            lblArchivo.AutoSize = true;
            lblArchivo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblArchivo.ForeColor = Color.FromArgb(50, 60, 70);
            lblArchivo.Location = new Point(32, 162);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new Size(178, 19);
            lblArchivo.TabIndex = 5;
            lblArchivo.Text = "Archivo de plantilla Excel";
            // 
            // lblArchivoSeleccionado
            // 
            lblArchivoSeleccionado.AutoSize = true;
            lblArchivoSeleccionado.Font = new Font("Segoe UI", 9F);
            lblArchivoSeleccionado.ForeColor = Color.Gray;
            lblArchivoSeleccionado.Location = new Point(32, 199);
            lblArchivoSeleccionado.Name = "lblArchivoSeleccionado";
            lblArchivoSeleccionado.Size = new Size(161, 15);
            lblArchivoSeleccionado.TabIndex = 6;
            lblArchivoSeleccionado.Text = "Ningún archivo seleccionado";
            // 
            // btnSeleccionarArchivo
            // 
            btnSeleccionarArchivo.BackColor = Color.FromArgb(41, 128, 185);
            btnSeleccionarArchivo.Cursor = Cursors.Hand;
            btnSeleccionarArchivo.FlatAppearance.BorderSize = 0;
            btnSeleccionarArchivo.FlatStyle = FlatStyle.Flat;
            btnSeleccionarArchivo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSeleccionarArchivo.ForeColor = Color.White;
            btnSeleccionarArchivo.Location = new Point(32, 254);
            btnSeleccionarArchivo.Name = "btnSeleccionarArchivo";
            btnSeleccionarArchivo.Size = new Size(200, 36);
            btnSeleccionarArchivo.TabIndex = 7;
            btnSeleccionarArchivo.Text = "Seleccionar archivo…";
            btnSeleccionarArchivo.UseVisualStyleBackColor = false;
            btnSeleccionarArchivo.Click += BtnSeleccionarArchivo_Click;
            // 
            // lblSeparador1
            // 
            lblSeparador1.BackColor = Color.FromArgb(218, 226, 236);
            lblSeparador1.Location = new Point(32, 317);
            lblSeparador1.Name = "lblSeparador1";
            lblSeparador1.Size = new Size(576, 1);
            lblSeparador1.TabIndex = 8;
            // 
            // lblHoja
            // 
            lblHoja.AutoSize = true;
            lblHoja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHoja.ForeColor = Color.FromArgb(50, 60, 70);
            lblHoja.Location = new Point(32, 328);
            lblHoja.Name = "lblHoja";
            lblHoja.Size = new Size(104, 19);
            lblHoja.TabIndex = 9;
            lblHoja.Text = "Hoja del Excel";
            // 
            // cmbHoja
            // 
            cmbHoja.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHoja.Font = new Font("Segoe UI", 10F);
            cmbHoja.FormattingEnabled = true;
            cmbHoja.Location = new Point(32, 352);
            cmbHoja.Name = "cmbHoja";
            cmbHoja.Size = new Size(300, 25);
            cmbHoja.TabIndex = 10;
            cmbHoja.SelectedIndexChanged += CmbHoja_SelectedIndexChanged;
            // 
            // lblOperacion
            // 
            lblOperacion.AutoSize = true;
            lblOperacion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOperacion.ForeColor = Color.FromArgb(50, 60, 70);
            lblOperacion.Location = new Point(32, 394);
            lblOperacion.Name = "lblOperacion";
            lblOperacion.Size = new Size(132, 19);
            lblOperacion.TabIndex = 11;
            lblOperacion.Text = "Tipo de operación";
            // 
            // cmbOperacion
            // 
            cmbOperacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperacion.Font = new Font("Segoe UI", 10F);
            cmbOperacion.FormattingEnabled = true;
            cmbOperacion.Items.AddRange(new object[] { "Multiplicar Campo A × Campo B y sumar", "Suma simple de un campo" });
            cmbOperacion.Location = new Point(32, 418);
            cmbOperacion.Name = "cmbOperacion";
            cmbOperacion.Size = new Size(340, 25);
            cmbOperacion.TabIndex = 12;
            cmbOperacion.SelectedIndexChanged += CmbOperacion_SelectedIndexChanged;
            // 
            // lblSeparador2
            // 
            lblSeparador2.BackColor = Color.FromArgb(218, 226, 236);
            lblSeparador2.Location = new Point(32, 462);
            lblSeparador2.Name = "lblSeparador2";
            lblSeparador2.Size = new Size(576, 1);
            lblSeparador2.TabIndex = 13;
            // 
            // lblCampoTotal
            // 
            lblCampoTotal.AutoSize = true;
            lblCampoTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCampoTotal.ForeColor = Color.FromArgb(50, 60, 70);
            lblCampoTotal.Location = new Point(32, 476);
            lblCampoTotal.Name = "lblCampoTotal";
            lblCampoTotal.Size = new Size(220, 19);
            lblCampoTotal.TabIndex = 14;
            lblCampoTotal.Text = "Campo total (columna a sumar)";
            // 
            // cmbCampoTotal
            // 
            cmbCampoTotal.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoTotal.Font = new Font("Segoe UI", 10F);
            cmbCampoTotal.FormattingEnabled = true;
            cmbCampoTotal.Location = new Point(32, 500);
            cmbCampoTotal.Name = "cmbCampoTotal";
            cmbCampoTotal.Size = new Size(300, 25);
            cmbCampoTotal.TabIndex = 15;
            // 
            // lblCampoA
            // 
            lblCampoA.AutoSize = true;
            lblCampoA.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCampoA.ForeColor = Color.FromArgb(50, 60, 70);
            lblCampoA.Location = new Point(32, 476);
            lblCampoA.Name = "lblCampoA";
            lblCampoA.Size = new Size(71, 19);
            lblCampoA.TabIndex = 16;
            lblCampoA.Text = "Campo A";
            lblCampoA.Visible = false;
            // 
            // cmbCampoA
            // 
            cmbCampoA.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoA.Font = new Font("Segoe UI", 10F);
            cmbCampoA.FormattingEnabled = true;
            cmbCampoA.Location = new Point(32, 500);
            cmbCampoA.Name = "cmbCampoA";
            cmbCampoA.Size = new Size(260, 25);
            cmbCampoA.TabIndex = 17;
            cmbCampoA.Visible = false;
            // 
            // lblCampoB
            // 
            lblCampoB.AutoSize = true;
            lblCampoB.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCampoB.ForeColor = Color.FromArgb(50, 60, 70);
            lblCampoB.Location = new Point(310, 476);
            lblCampoB.Name = "lblCampoB";
            lblCampoB.Size = new Size(70, 19);
            lblCampoB.TabIndex = 18;
            lblCampoB.Text = "Campo B";
            lblCampoB.Visible = false;
            // 
            // cmbCampoB
            // 
            cmbCampoB.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoB.Font = new Font("Segoe UI", 10F);
            cmbCampoB.FormattingEnabled = true;
            cmbCampoB.Location = new Point(310, 500);
            cmbCampoB.Name = "cmbCampoB";
            cmbCampoB.Size = new Size(260, 25);
            cmbCampoB.TabIndex = 19;
            cmbCampoB.Visible = false;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(237, 242, 247);
            panelBotones.Controls.Add(btnRegresar);
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 600);
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(24, 10, 24, 10);
            panelBotones.Size = new Size(640, 60);
            panelBotones.TabIndex = 1;
            // 
            // btnRegresar
            // 
            btnRegresar.BackColor = Color.FromArgb(100, 116, 139);
            btnRegresar.Cursor = Cursors.Hand;
            btnRegresar.FlatAppearance.BorderSize = 0;
            btnRegresar.FlatStyle = FlatStyle.Flat;
            btnRegresar.Font = new Font("Segoe UI", 10F);
            btnRegresar.ForeColor = Color.White;
            btnRegresar.Location = new Point(24, 10);
            btnRegresar.Name = "btnRegresar";
            btnRegresar.Size = new Size(120, 38);
            btnRegresar.TabIndex = 0;
            btnRegresar.Text = "← Regresar";
            btnRegresar.UseVisualStyleBackColor = false;
            btnRegresar.Click += BtnRegresar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuardar.BackColor = Color.FromArgb(22, 163, 74);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(460, 10);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(156, 38);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = "Guardar plantilla";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // FrmPlantillaInventario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 660);
            Controls.Add(panelPrincipal);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmPlantillaInventario";
            Text = "Configurar Plantilla de Inventario";
            panelPrincipal.ResumeLayout(false);
            panelContenido.ResumeLayout(false);
            panelContenido.PerformLayout();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel     panelPrincipal;
        private Panel     panelContenido;
        private Label     lblRazonSocial;
        private ComboBox  cmbRazonSocial;
        private Label     lblEmpresa;
        private ComboBox  cmbEmpresa;
        private Label     lblSep0;
        private Label     lblArchivo;
        private Label lblArchivoSeleccionado;
        private Button btnSeleccionarArchivo;
        private Label lblSeparador1;
        private Label lblHoja;
        private ComboBox cmbHoja;
        private Label lblOperacion;
        private ComboBox cmbOperacion;
        private Label lblSeparador2;
        private Label lblCampoTotal;
        private ComboBox cmbCampoTotal;
        private Label lblCampoA;
        private ComboBox cmbCampoA;
        private Label lblCampoB;
        private ComboBox cmbCampoB;
        private Panel panelBotones;
        private Button btnGuardar;
        private Button btnRegresar;
    }
}
