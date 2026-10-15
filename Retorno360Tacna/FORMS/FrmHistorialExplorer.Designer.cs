using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    partial class FrmHistorialExplorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private ComboBox cmbRazon;
        private ComboBox cmbEmpresa;
        private NumericUpDown nudYear;
        private Button btnBuscar;
        private DataGridView dgvSummary;
        private DataGridView dgvDetails;
        private Label lblTotal;
        private Panel header;
        private Panel filterPanel;
        private Button btnClose;

        /// <summary>
        /// Initialize designer controls. This method is safe to be parsed by Visual Studio designer.
        /// </summary>
        private void InitializeComponent()
        {
            filterPanel = new Panel();
            cmbRazon = new ComboBox();
            cmbEmpresa = new ComboBox();
            nudYear = new NumericUpDown();
            lblTotal = new Label();
            btnExportarExcel = new Button();
            btnBuscar = new Button();
            dgvSummary = new DataGridView();
            dgvDetails = new DataGridView();
            BtnCerrar = new Button();
            filterPanel.SuspendLayout();
            ((ISupportInitialize)nudYear).BeginInit();
            ((ISupportInitialize)dgvSummary).BeginInit();
            ((ISupportInitialize)dgvDetails).BeginInit();
            SuspendLayout();
            // 
            // filterPanel
            // 
            filterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterPanel.Controls.Add(cmbRazon);
            filterPanel.Controls.Add(cmbEmpresa);
            filterPanel.Controls.Add(nudYear);
            filterPanel.Controls.Add(lblTotal);
            filterPanel.Location = new Point(12, 75);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(860, 36);
            filterPanel.TabIndex = 1;
            // 
            // cmbRazon
            // 
            cmbRazon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRazon.Location = new Point(4, 6);
            cmbRazon.Name = "cmbRazon";
            cmbRazon.Size = new Size(236, 23);
            cmbRazon.TabIndex = 0;
            // 
            // cmbEmpresa
            // 
            cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmpresa.Location = new Point(252, 6);
            cmbEmpresa.Name = "cmbEmpresa";
            cmbEmpresa.Size = new Size(240, 23);
            cmbEmpresa.TabIndex = 1;
            // 
            // nudYear
            // 
            nudYear.Location = new Point(504, 6);
            nudYear.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            nudYear.Minimum = new decimal(new int[] { 2000, 0, 0, 0 });
            nudYear.Name = "nudYear";
            nudYear.Size = new Size(80, 23);
            nudYear.TabIndex = 2;
            nudYear.Value = new decimal(new int[] { 2026, 0, 0, 0 });
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(613, 8);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(45, 15);
            lblTotal.TabIndex = 4;
            lblTotal.Text = "Total: 0";
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.BackColor = Color.FromArgb(0, 181, 24);
            btnExportarExcel.FlatAppearance.BorderSize = 0;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Location = new Point(675, 21);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(90, 31);
            btnExportarExcel.TabIndex = 5;
            btnExportarExcel.Text = "Exportar";
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnBuscar
            // 
            btnBuscar.BackColor = Color.FromArgb(52, 152, 219);
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.FlatStyle = FlatStyle.Flat;
            btnBuscar.ForeColor = Color.White;
            btnBuscar.Location = new Point(579, 21);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(90, 31);
            btnBuscar.TabIndex = 3;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = false;
            btnBuscar.Click += BtnBuscar_Click;
            // 
            // dgvSummary
            // 
            dgvSummary.AllowUserToAddRows = false;
            dgvSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvSummary.BackgroundColor = Color.WhiteSmoke;
            dgvSummary.Location = new Point(12, 138);
            dgvSummary.Name = "dgvSummary";
            dgvSummary.ReadOnly = true;
            dgvSummary.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSummary.Size = new Size(860, 120);
            dgvSummary.TabIndex = 2;
            dgvSummary.CellDoubleClick += DgvSummary_CellDoubleClick;
            // 
            // dgvDetails
            // 
            dgvDetails.AllowUserToAddRows = false;
            dgvDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetails.BackgroundColor = Color.WhiteSmoke;
            dgvDetails.Location = new Point(12, 275);
            dgvDetails.Name = "dgvDetails";
            dgvDetails.ReadOnly = true;
            dgvDetails.Size = new Size(860, 300);
            dgvDetails.TabIndex = 3;
            // 
            // BtnCerrar
            // 
            BtnCerrar.BackColor = Color.FromArgb(179, 179, 179);
            BtnCerrar.FlatAppearance.BorderSize = 0;
            BtnCerrar.FlatStyle = FlatStyle.Flat;
            BtnCerrar.ForeColor = Color.White;
            BtnCerrar.Location = new Point(771, 21);
            BtnCerrar.Name = "BtnCerrar";
            BtnCerrar.Size = new Size(90, 31);
            BtnCerrar.TabIndex = 6;
            BtnCerrar.Text = "Cerrar";
            BtnCerrar.UseVisualStyleBackColor = false;
            BtnCerrar.Click += BtnCerrar_Click;
            // 
            // FrmHistorialExplorer
            // 
            ClientSize = new Size(900, 600);
            Controls.Add(BtnCerrar);
            Controls.Add(btnExportarExcel);
            Controls.Add(filterPanel);
            Controls.Add(dgvSummary);
            Controls.Add(btnBuscar);
            Controls.Add(dgvDetails);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmHistorialExplorer";
            StartPosition = FormStartPosition.CenterScreen;
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            ((ISupportInitialize)nudYear).EndInit();
            ((ISupportInitialize)dgvSummary).EndInit();
            ((ISupportInitialize)dgvDetails).EndInit();
            ResumeLayout(false);
        }

        /// <summary>
        /// Limpiar recursos
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label title;
        private Button btnExportarExcel;
        private Button BtnCerrar;
    }
}
