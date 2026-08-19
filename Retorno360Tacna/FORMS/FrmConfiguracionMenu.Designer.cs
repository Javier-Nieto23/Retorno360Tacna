using System.Drawing;
using System.Windows.Forms;

#nullable disable

namespace Retorno360Tacna.FORMS
{
    partial class FrmConfiguracionMenu
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
            panelContenido = new Panel();
            panelOpciones = new Panel();
            btnUsuarioEmpresa = new Button();
            btnConexiones = new Button();
            btnUsuarios = new Button();
            btnPlantilla = new Button();
            panelContenido.SuspendLayout();
            panelOpciones.SuspendLayout();
            SuspendLayout();
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.FromArgb(245, 247, 250);
            panelContenido.Controls.Add(panelOpciones);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Size = new Size(600, 480);
            panelContenido.TabIndex = 0;
            // 
            // panelOpciones
            // 
            panelOpciones.Controls.Add(btnUsuarioEmpresa);
            panelOpciones.Controls.Add(btnConexiones);
            panelOpciones.Controls.Add(btnUsuarios);
            panelOpciones.Controls.Add(btnPlantilla);
            panelOpciones.Dock = DockStyle.Fill;
            panelOpciones.Location = new Point(0, 0);
            panelOpciones.Name = "panelOpciones";
            panelOpciones.Padding = new Padding(32, 28, 32, 28);
            panelOpciones.Size = new Size(600, 480);
            panelOpciones.TabIndex = 0;
            // 
            // btnUsuarioEmpresa
            // 
            btnUsuarioEmpresa.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnUsuarioEmpresa.BackColor = Color.FromArgb(196, 140, 255);
            btnUsuarioEmpresa.Cursor = Cursors.Hand;
            btnUsuarioEmpresa.FlatAppearance.BorderSize = 0;
            btnUsuarioEmpresa.FlatAppearance.MouseDownBackColor = Color.FromArgb(22, 100, 75);
            btnUsuarioEmpresa.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 168, 130);
            btnUsuarioEmpresa.FlatStyle = FlatStyle.Flat;
            btnUsuarioEmpresa.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnUsuarioEmpresa.ForeColor = Color.White;
            btnUsuarioEmpresa.Image = Properties.Resources.company_22169;
            btnUsuarioEmpresa.ImageAlign = ContentAlignment.MiddleLeft;
            btnUsuarioEmpresa.Location = new Point(32, 245);
            btnUsuarioEmpresa.Name = "btnUsuarioEmpresa";
            btnUsuarioEmpresa.Padding = new Padding(12, 0, 0, 0);
            btnUsuarioEmpresa.Size = new Size(536, 54);
            btnUsuarioEmpresa.TabIndex = 3;
            btnUsuarioEmpresa.Text = "Configurar empresas para usuario";
            btnUsuarioEmpresa.UseVisualStyleBackColor = false;
            btnUsuarioEmpresa.Click += btnUsuarioEmpresa_Click;
            // 
            // btnConexiones
            // 
            btnConexiones.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnConexiones.BackColor = Color.FromArgb(31, 78, 121);
            btnConexiones.Cursor = Cursors.Hand;
            btnConexiones.FlatAppearance.BorderSize = 0;
            btnConexiones.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 58, 91);
            btnConexiones.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 104, 154);
            btnConexiones.FlatStyle = FlatStyle.Flat;
            btnConexiones.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnConexiones.ForeColor = Color.White;
            btnConexiones.Image = Properties.Resources.Network_Connection_Control_Panel_22605;
            btnConexiones.ImageAlign = ContentAlignment.MiddleLeft;
            btnConexiones.Location = new Point(32, 28);
            btnConexiones.Name = "btnConexiones";
            btnConexiones.Padding = new Padding(12, 0, 0, 0);
            btnConexiones.Size = new Size(536, 54);
            btnConexiones.TabIndex = 0;
            btnConexiones.Text = "Configurar conexión del portal web";
            btnConexiones.UseVisualStyleBackColor = false;
            btnConexiones.Click += BtnConexiones_Click;
            // 
            // btnUsuarios
            // 
            btnUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnUsuarios.BackColor = Color.FromArgb(41, 128, 185);
            btnUsuarios.Cursor = Cursors.Hand;
            btnUsuarios.FlatAppearance.BorderSize = 0;
            btnUsuarios.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 100, 145);
            btnUsuarios.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 152, 219);
            btnUsuarios.FlatStyle = FlatStyle.Flat;
            btnUsuarios.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnUsuarios.ForeColor = Color.White;
            btnUsuarios.Image = Properties.Resources.configure_user_16726;
            btnUsuarios.ImageAlign = ContentAlignment.MiddleLeft;
            btnUsuarios.Location = new Point(32, 100);
            btnUsuarios.Name = "btnUsuarios";
            btnUsuarios.Padding = new Padding(12, 0, 0, 0);
            btnUsuarios.Size = new Size(536, 54);
            btnUsuarios.TabIndex = 1;
            btnUsuarios.Text = "Administrar usuarios";
            btnUsuarios.UseVisualStyleBackColor = false;
            btnUsuarios.Click += BtnUsuarios_Click;
            // 
            // btnPlantilla
            // 
            btnPlantilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnPlantilla.BackColor = Color.FromArgb(39, 130, 100);
            btnPlantilla.Cursor = Cursors.Hand;
            btnPlantilla.FlatAppearance.BorderSize = 0;
            btnPlantilla.FlatAppearance.MouseDownBackColor = Color.FromArgb(22, 100, 75);
            btnPlantilla.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 168, 130);
            btnPlantilla.FlatStyle = FlatStyle.Flat;
            btnPlantilla.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnPlantilla.ForeColor = Color.White;
            btnPlantilla.Image = Properties.Resources.templates_rule_page_document_5850;
            btnPlantilla.ImageAlign = ContentAlignment.MiddleLeft;
            btnPlantilla.Location = new Point(32, 172);
            btnPlantilla.Name = "btnPlantilla";
            btnPlantilla.Padding = new Padding(12, 0, 0, 0);
            btnPlantilla.Size = new Size(536, 54);
            btnPlantilla.TabIndex = 2;
            btnPlantilla.Text = "Configurar plantilla de cálculo de inventarios";
            btnPlantilla.UseVisualStyleBackColor = false;
            btnPlantilla.Click += BtnPlantilla_Click;
            // 
            // FrmConfiguracionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 480);
            Controls.Add(panelContenido);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(500, 360);
            Name = "FrmConfiguracionMenu";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configuración";
            panelContenido.ResumeLayout(false);
            panelOpciones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelContenido;
        private Panel panelOpciones;
        private Button btnUsuarios;
        private Button btnConexiones;
        private Button btnPlantilla;
        private Button btnUsuarioEmpresa;
    }
}
