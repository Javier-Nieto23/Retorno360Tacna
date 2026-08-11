using System.Drawing;
using System.Windows.Forms;

#nullable disable

namespace Retorno360Tacna.FORMS
{
    partial class FrmDbConnectionModal
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
            panelContenido = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            btnDelete = new Button();
            btnTest = new Button();
            chkShow = new CheckBox();
            txtConnection = new TextBox();
            lblInfo = new Label();
            lblTitulo = new Label();
            panelContenido.SuspendLayout();
            SuspendLayout();
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.White;
            panelContenido.Controls.Add(btnCancel);
            panelContenido.Controls.Add(btnSave);
            panelContenido.Controls.Add(btnDelete);
            panelContenido.Controls.Add(btnTest);
            panelContenido.Controls.Add(chkShow);
            panelContenido.Controls.Add(txtConnection);
            panelContenido.Controls.Add(lblInfo);
            panelContenido.Controls.Add(lblTitulo);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(32);
            panelContenido.Size = new Size(760, 320);
            panelContenido.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(616, 244);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(108, 36);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Regresar";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.BackColor = Color.FromArgb(31, 78, 121);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(494, 244);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(108, 36);
            btnSave.TabIndex = 6;
            btnSave.Text = "Guardar";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Location = new Point(168, 244);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(140, 36);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Borrar guardado";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTest.BackColor = Color.FromArgb(52, 152, 219);
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.ForeColor = Color.White;
            btnTest.Location = new Point(36, 244);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(120, 36);
            btnTest.TabIndex = 4;
            btnTest.Text = "Probar conexión";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.Click += BtnTest_Click;
            // 
            // chkShow
            // 
            chkShow.AutoSize = true;
            chkShow.Location = new Point(36, 172);
            chkShow.Name = "chkShow";
            chkShow.Size = new Size(105, 19);
            chkShow.TabIndex = 3;
            chkShow.Text = "Mostrar texto";
            chkShow.UseVisualStyleBackColor = true;
            chkShow.CheckedChanged += chkShow_CheckedChanged;
            // 
            // txtConnection
            // 
            txtConnection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConnection.Location = new Point(36, 134);
            txtConnection.Name = "txtConnection";
            txtConnection.Size = new Size(688, 23);
            txtConnection.TabIndex = 2;
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = Color.FromArgb(89, 89, 89);
            lblInfo.Location = new Point(36, 104);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(423, 15);
            lblInfo.TabIndex = 1;
            lblInfo.Text = "Introduzca la URL PostgreSQL (DATABASE_URL) o una cadena de conexión Npgsql.";
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(31, 78, 121);
            lblTitulo.Location = new Point(32, 32);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(288, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Conexión del portal web";
            // 
            // FrmDbConnectionModal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 320);
            Controls.Add(panelContenido);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(600, 280);
            Name = "FrmDbConnectionModal";
            Text = "Conexión del portal web";
            panelContenido.ResumeLayout(false);
            panelContenido.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelContenido;
        private Button btnCancel;
        private Button btnSave;
        private Button btnDelete;
        private Button btnTest;
        private CheckBox chkShow;
        private TextBox txtConnection;
        private Label lblInfo;
        private Label lblTitulo;
    }
}
