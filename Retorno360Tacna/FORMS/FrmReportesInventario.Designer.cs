namespace Retorno360Tacna.FORMS
{
    partial class FrmReportesInventario
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
            components = new System.ComponentModel.Container();
            panelFiltros = new Panel();
            btnLimpiarFiltro = new Button();
            btnActualizar = new Button();
            cboRazonSocial = new ComboBox();
            lblRazonSocial = new Label();
            panelContenido = new Panel();
            lblTotalCarpetas = new Label();
            lvCarpetas = new ListView();
            imageListCarpetas = new ImageList(components);
            panelFiltros.SuspendLayout();
            panelContenido.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.BackColor = Color.White;
            panelFiltros.Controls.Add(btnLimpiarFiltro);
            panelFiltros.Controls.Add(btnActualizar);
            panelFiltros.Controls.Add(cboRazonSocial);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Margin = new Padding(3, 2, 3, 2);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new Padding(18, 15, 18, 15);
            panelFiltros.Size = new Size(1050, 75);
            panelFiltros.TabIndex = 1;
            // 
            // btnLimpiarFiltro
            // 
            btnLimpiarFiltro.BackColor = Color.FromArgb(230, 126, 34);
            btnLimpiarFiltro.Cursor = Cursors.Hand;
            btnLimpiarFiltro.FlatAppearance.BorderSize = 0;
            btnLimpiarFiltro.FlatStyle = FlatStyle.Flat;
            btnLimpiarFiltro.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLimpiarFiltro.ForeColor = Color.White;
            btnLimpiarFiltro.Location = new Point(839, 24);
            btnLimpiarFiltro.Margin = new Padding(3, 2, 3, 2);
            btnLimpiarFiltro.Name = "btnLimpiarFiltro";
            btnLimpiarFiltro.Size = new Size(131, 30);
            btnLimpiarFiltro.TabIndex = 3;
            btnLimpiarFiltro.Text = "🔄 Limpiar Filtro";
            btnLimpiarFiltro.UseVisualStyleBackColor = false;
            btnLimpiarFiltro.Click += btnLimpiarFiltro_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.BackColor = Color.FromArgb(52, 152, 219);
            btnActualizar.Cursor = Cursors.Hand;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.Location = new Point(690, 24);
            btnActualizar.Margin = new Padding(3, 2, 3, 2);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(131, 30);
            btnActualizar.TabIndex = 2;
            btnActualizar.Text = "🔄 Actualizar";
            btnActualizar.UseVisualStyleBackColor = false;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // cboRazonSocial
            // 
            cboRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRazonSocial.Font = new Font("Segoe UI", 11F);
            cboRazonSocial.FormattingEnabled = true;
            cboRazonSocial.Location = new Point(275, 25);
            cboRazonSocial.Margin = new Padding(3, 2, 3, 2);
            cboRazonSocial.Name = "cboRazonSocial";
            cboRazonSocial.Size = new Size(342, 28);
            cboRazonSocial.TabIndex = 1;
            cboRazonSocial.SelectedIndexChanged += cboRazonSocial_SelectedIndexChanged;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblRazonSocial.Location = new Point(18, 28);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(251, 20);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Filtrar por Razón Social (Opcional):";
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.FromArgb(245, 246, 250);
            panelContenido.Controls.Add(lblTotalCarpetas);
            panelContenido.Controls.Add(lvCarpetas);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 75);
            panelContenido.Margin = new Padding(3, 2, 3, 2);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(18, 15, 18, 15);
            panelContenido.Size = new Size(1050, 450);
            panelContenido.TabIndex = 2;
            // 
            // lblTotalCarpetas
            // 
            lblTotalCarpetas.AutoSize = true;
            lblTotalCarpetas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalCarpetas.Location = new Point(18, 368);
            lblTotalCarpetas.Name = "lblTotalCarpetas";
            lblTotalCarpetas.Size = new Size(226, 19);
            lblTotalCarpetas.TabIndex = 1;
            lblTotalCarpetas.Text = "Total de carpetas encontradas: 0";
            // 
            // lvCarpetas
            // 
            lvCarpetas.LargeImageList = imageListCarpetas;
            lvCarpetas.Location = new Point(18, 15);
            lvCarpetas.Margin = new Padding(3, 2, 3, 2);
            lvCarpetas.MultiSelect = false;
            lvCarpetas.Name = "lvCarpetas";
            lvCarpetas.Size = new Size(1016, 334);
            lvCarpetas.SmallImageList = imageListCarpetas;
            lvCarpetas.TabIndex = 0;
            lvCarpetas.UseCompatibleStateImageBehavior = false;
            lvCarpetas.DoubleClick += lvCarpetas_DoubleClick;
            // 
            // imageListCarpetas
            // 
            imageListCarpetas.ColorDepth = ColorDepth.Depth32Bit;
            imageListCarpetas.ImageSize = new Size(16, 16);
            imageListCarpetas.TransparentColor = Color.Transparent;
            // 
            // FrmReportesInventario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1050, 525);
            Controls.Add(panelContenido);
            Controls.Add(panelFiltros);
            Margin = new Padding(3, 2, 3, 2);
            Name = "FrmReportesInventario";
            Text = "Reportes de Inventario";
            Load += FrmReportesInventario_Load;
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            panelContenido.ResumeLayout(false);
            panelContenido.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelFiltros;
        private ComboBox cboRazonSocial;
        private Label lblRazonSocial;
        private Panel panelContenido;
        private System.Windows.Forms.ListView lvCarpetas;
        private System.Windows.Forms.ImageList imageListCarpetas;
        private Button btnActualizar;
        private Button btnLimpiarFiltro;
        private Label lblTotalCarpetas;
    }
}
