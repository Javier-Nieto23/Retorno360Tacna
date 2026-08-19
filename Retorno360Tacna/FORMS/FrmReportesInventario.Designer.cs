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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmReportesInventario));
            panelFiltros = new Panel();
            BtmEliminarArchivo = new Button();
            lblSubtituloFiltros = new Label();
            lblTituloFiltros = new Label();
            btnExportarPdf = new Button();
            btnAgregarObservacion = new Button();
            btnLimpiarFiltro = new Button();
            btnActualizar = new Button();
            cboAnio = new ComboBox();
            lblAnio = new Label();
            cboRazonSocial = new ComboBox();
            lblRazonSocial = new Label();
            lblEstadoConexionPortal = new Label();
            chkUsarPerfil = new CheckBox();
            btnRegresarCarpeta = new Button();
            panelContenido = new Panel();
            panelSolicitudesEliminacion = new Panel();
            dgvSolicitudesEliminacion = new DataGridView();
            lblSolicitudesEliminacionEstado = new Label();
            lblSolicitudesEliminacionTitulo = new Label();
            panelArchivosObservados = new Panel();
            btnEliminarArchivoObservado = new Button();
            btnCerrarObservacion = new Button();
            dgvArchivosObservados = new DataGridView();
            lblArchivosObservadosEstado = new Label();
            lblArchivosObservadosTitulo = new Label();
            panelMensajes = new Panel();
            btnEnviarMensajeObservacion = new Button();
            txtMensajeObservacion = new TextBox();
            rtbMensajesObservacion = new RichTextBox();
            lblMensajesEstado = new Label();
            lblMensajesTitulo = new Label();
            lblResumenTitulo = new Label();
            lblExploradorTitulo = new Label();
            panelCabeceraContenido = new Panel();
            lblRutaActual = new Label();
            lblVistaDescripcion = new Label();
            lblVistaTitulo = new Label();
            lblTotalCarpetas = new Label();
            lvCarpetas = new ListView();
            imageListCarpetas = new ImageList(components);
            panelFiltros.SuspendLayout();
            panelContenido.SuspendLayout();
            panelSolicitudesEliminacion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSolicitudesEliminacion).BeginInit();
            panelArchivosObservados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvArchivosObservados).BeginInit();
            panelMensajes.SuspendLayout();
            panelCabeceraContenido.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltros
            // 
            panelFiltros.BackColor = Color.FromArgb(248, 250, 252);
            panelFiltros.Controls.Add(BtmEliminarArchivo);
            panelFiltros.Controls.Add(lblSubtituloFiltros);
            panelFiltros.Controls.Add(lblTituloFiltros);
            panelFiltros.Controls.Add(btnExportarPdf);
            panelFiltros.Controls.Add(btnAgregarObservacion);
            panelFiltros.Controls.Add(btnLimpiarFiltro);
            panelFiltros.Controls.Add(btnActualizar);
            panelFiltros.Controls.Add(cboAnio);
            panelFiltros.Controls.Add(lblAnio);
            panelFiltros.Controls.Add(cboRazonSocial);
            panelFiltros.Controls.Add(lblRazonSocial);
            panelFiltros.Controls.Add(chkUsarPerfil);
            panelFiltros.Controls.Add(lblEstadoConexionPortal);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Location = new Point(0, 0);
            panelFiltros.Margin = new Padding(3, 2, 3, 2);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new Padding(24, 18, 24, 18);
            panelFiltros.Size = new Size(1469, 132);
            panelFiltros.TabIndex = 1;
            // 
            // BtmEliminarArchivo
            // 
            BtmEliminarArchivo.BackColor = Color.Red;
            BtmEliminarArchivo.FlatAppearance.BorderSize = 0;
            BtmEliminarArchivo.FlatStyle = FlatStyle.Flat;
            BtmEliminarArchivo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            BtmEliminarArchivo.ForeColor = Color.White;
            BtmEliminarArchivo.Image = (Image)resources.GetObject("BtmEliminarArchivo.Image");
            BtmEliminarArchivo.ImageAlign = ContentAlignment.MiddleLeft;
            BtmEliminarArchivo.Location = new Point(738, 53);
            BtmEliminarArchivo.Name = "BtmEliminarArchivo";
            BtmEliminarArchivo.Size = new Size(140, 60);
            BtmEliminarArchivo.TabIndex = 12;
            BtmEliminarArchivo.Text = "Eliminar";
            BtmEliminarArchivo.TextAlign = ContentAlignment.MiddleRight;
            BtmEliminarArchivo.UseVisualStyleBackColor = false;
            BtmEliminarArchivo.Click += BtmEliminarArchivo_Click;
            // 
            // lblSubtituloFiltros
            // 
            lblSubtituloFiltros.AutoSize = true;
            lblSubtituloFiltros.Font = new Font("Segoe UI", 9.5F);
            lblSubtituloFiltros.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtituloFiltros.Location = new Point(24, 39);
            lblSubtituloFiltros.Name = "lblSubtituloFiltros";
            lblSubtituloFiltros.Size = new Size(540, 17);
            lblSubtituloFiltros.TabIndex = 11;
            lblSubtituloFiltros.Text = "Sigue este orden: filtra, abre una carpeta, selecciona un archivo y revisa sus observaciones.";
            // 
            // lblTituloFiltros
            // 
            lblTituloFiltros.AutoSize = true;
            lblTituloFiltros.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTituloFiltros.ForeColor = Color.FromArgb(15, 23, 42);
            lblTituloFiltros.Location = new Point(24, 12);
            lblTituloFiltros.Name = "lblTituloFiltros";
            lblTituloFiltros.Size = new Size(215, 25);
            lblTituloFiltros.TabIndex = 10;
            lblTituloFiltros.Text = "Reportes de inventario";
            // 
            // btnExportarPdf
            // 
            btnExportarPdf.BackColor = Color.FromArgb(192, 57, 43);
            btnExportarPdf.Cursor = Cursors.Hand;
            btnExportarPdf.FlatAppearance.BorderSize = 0;
            btnExportarPdf.FlatStyle = FlatStyle.Flat;
            btnExportarPdf.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportarPdf.ForeColor = Color.White;
            btnExportarPdf.Image = Properties.Resources.applicationpdf_103614;
            btnExportarPdf.ImageAlign = ContentAlignment.MiddleLeft;
            btnExportarPdf.Location = new Point(1176, 53);
            btnExportarPdf.Margin = new Padding(3, 2, 3, 2);
            btnExportarPdf.Name = "btnExportarPdf";
            btnExportarPdf.Size = new Size(140, 59);
            btnExportarPdf.TabIndex = 7;
            btnExportarPdf.Text = "Exportar PDF";
            btnExportarPdf.TextAlign = ContentAlignment.MiddleRight;
            btnExportarPdf.UseVisualStyleBackColor = false;
            btnExportarPdf.Click += btnExportarPdf_Click;
            // 
            // btnAgregarObservacion
            // 
            btnAgregarObservacion.BackColor = Color.FromArgb(142, 68, 173);
            btnAgregarObservacion.Cursor = Cursors.Hand;
            btnAgregarObservacion.Enabled = false;
            btnAgregarObservacion.FlatAppearance.BorderSize = 0;
            btnAgregarObservacion.FlatStyle = FlatStyle.Flat;
            btnAgregarObservacion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregarObservacion.ForeColor = Color.White;
            btnAgregarObservacion.Image = Properties.Resources.search_magnifying_glass_icon_192631;
            btnAgregarObservacion.ImageAlign = ContentAlignment.MiddleLeft;
            btnAgregarObservacion.Location = new Point(592, 53);
            btnAgregarObservacion.Margin = new Padding(3, 2, 3, 2);
            btnAgregarObservacion.Name = "btnAgregarObservacion";
            btnAgregarObservacion.Size = new Size(140, 61);
            btnAgregarObservacion.TabIndex = 9;
            btnAgregarObservacion.Text = "Crear observación";
            btnAgregarObservacion.TextAlign = ContentAlignment.MiddleRight;
            btnAgregarObservacion.UseVisualStyleBackColor = false;
            btnAgregarObservacion.Click += btnAgregarObservacion_Click;
            // 
            // btnLimpiarFiltro
            // 
            btnLimpiarFiltro.BackColor = Color.Teal;
            btnLimpiarFiltro.Cursor = Cursors.Hand;
            btnLimpiarFiltro.FlatAppearance.BorderSize = 0;
            btnLimpiarFiltro.FlatStyle = FlatStyle.Flat;
            btnLimpiarFiltro.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLimpiarFiltro.ForeColor = Color.White;
            btnLimpiarFiltro.Image = Properties.Resources.gui_filter_icon_1571471;
            btnLimpiarFiltro.ImageAlign = ContentAlignment.MiddleLeft;
            btnLimpiarFiltro.Location = new Point(1030, 53);
            btnLimpiarFiltro.Margin = new Padding(3, 2, 3, 2);
            btnLimpiarFiltro.Name = "btnLimpiarFiltro";
            btnLimpiarFiltro.Size = new Size(140, 59);
            btnLimpiarFiltro.TabIndex = 3;
            btnLimpiarFiltro.Text = "Limpiar";
            btnLimpiarFiltro.TextAlign = ContentAlignment.MiddleRight;
            btnLimpiarFiltro.UseVisualStyleBackColor = false;
            btnLimpiarFiltro.Click += btnLimpiarFiltro_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.BackColor = Color.FromArgb(21, 139, 161);
            btnActualizar.Cursor = Cursors.Hand;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.Image = Properties.Resources.reload1_785091;
            btnActualizar.ImageAlign = ContentAlignment.MiddleLeft;
            btnActualizar.Location = new Point(884, 53);
            btnActualizar.Margin = new Padding(3, 2, 3, 2);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(140, 59);
            btnActualizar.TabIndex = 2;
            btnActualizar.Text = "Recargar vista";
            btnActualizar.TextAlign = ContentAlignment.MiddleRight;
            btnActualizar.UseVisualStyleBackColor = false;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // cboAnio
            // 
            cboAnio.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAnio.Font = new Font("Segoe UI", 11F);
            cboAnio.FormattingEnabled = true;
            cboAnio.Location = new Point(82, 74);
            cboAnio.Margin = new Padding(3, 2, 3, 2);
            cboAnio.Name = "cboAnio";
            cboAnio.Size = new Size(120, 28);
            cboAnio.TabIndex = 4;
            cboAnio.SelectedIndexChanged += cboAnio_SelectedIndexChanged;
            // 
            // lblAnio
            // 
            lblAnio.AutoSize = true;
            lblAnio.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblAnio.Location = new Point(25, 78);
            lblAnio.Name = "lblAnio";
            lblAnio.Size = new Size(42, 20);
            lblAnio.TabIndex = 5;
            lblAnio.Text = "Año:";
            // 
            // cboRazonSocial
            // 
            cboRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRazonSocial.Font = new Font("Segoe UI", 11F);
            cboRazonSocial.FormattingEnabled = true;
            cboRazonSocial.Location = new Point(315, 75);
            cboRazonSocial.Margin = new Padding(3, 2, 3, 2);
            cboRazonSocial.Name = "cboRazonSocial";
            cboRazonSocial.Size = new Size(247, 28);
            cboRazonSocial.TabIndex = 1;
            cboRazonSocial.SelectedIndexChanged += cboRazonSocial_SelectedIndexChanged;
            // 
            // chkUsarPerfil
            // 
            chkUsarPerfil.AutoSize = true;
            chkUsarPerfil.Font = new Font("Segoe UI", 9.5F);
            chkUsarPerfil.Location = new Point(570, 79);
            chkUsarPerfil.Margin = new Padding(3, 2, 3, 2);
            chkUsarPerfil.Name = "chkUsarPerfil";
            chkUsarPerfil.Size = new Size(210, 21);
            chkUsarPerfil.TabIndex = 15;
            chkUsarPerfil.Text = "Usar empresas de mi perfil";
            chkUsarPerfil.UseVisualStyleBackColor = true;
            chkUsarPerfil.CheckedChanged += chkUsarPerfil_CheckedChanged;
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblRazonSocial.Location = new Point(218, 78);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(99, 20);
            lblRazonSocial.TabIndex = 0;
            lblRazonSocial.Text = "Razón social ";
            // 
            // lblEstadoConexionPortal
            // 
            lblEstadoConexionPortal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblEstadoConexionPortal.ForeColor = Color.FromArgb(127, 140, 141);
            lblEstadoConexionPortal.Location = new Point(24, 111);
            lblEstadoConexionPortal.Name = "lblEstadoConexionPortal";
            lblEstadoConexionPortal.Size = new Size(880, 17);
            lblEstadoConexionPortal.TabIndex = 8;
            lblEstadoConexionPortal.Text = "Conexión portal web: verificando...";
            lblEstadoConexionPortal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnRegresarCarpeta
            // 
            btnRegresarCarpeta.BackColor = Color.FromArgb(149, 165, 166);
            btnRegresarCarpeta.Cursor = Cursors.Hand;
            btnRegresarCarpeta.Enabled = false;
            btnRegresarCarpeta.FlatAppearance.BorderSize = 0;
            btnRegresarCarpeta.FlatStyle = FlatStyle.Flat;
            btnRegresarCarpeta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRegresarCarpeta.ForeColor = Color.White;
            btnRegresarCarpeta.Location = new Point(232, 87);
            btnRegresarCarpeta.Margin = new Padding(3, 2, 3, 2);
            btnRegresarCarpeta.Name = "btnRegresarCarpeta";
            btnRegresarCarpeta.Size = new Size(94, 31);
            btnRegresarCarpeta.TabIndex = 6;
            btnRegresarCarpeta.Text = "← Volver";
            btnRegresarCarpeta.UseVisualStyleBackColor = false;
            btnRegresarCarpeta.Click += btnRegresarCarpeta_Click;
            // 
            // panelContenido
            // 
            panelContenido.AutoScroll = true;
            panelContenido.BackColor = Color.FromArgb(245, 246, 250);
            panelContenido.Controls.Add(panelSolicitudesEliminacion);
            panelContenido.Controls.Add(panelArchivosObservados);
            panelContenido.Controls.Add(panelMensajes);
            panelContenido.Controls.Add(lblResumenTitulo);
            panelContenido.Controls.Add(lblExploradorTitulo);
            panelContenido.Controls.Add(panelCabeceraContenido);
            panelContenido.Controls.Add(lblTotalCarpetas);
            panelContenido.Controls.Add(btnRegresarCarpeta);
            panelContenido.Controls.Add(lvCarpetas);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 132);
            panelContenido.Margin = new Padding(3, 2, 3, 2);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(20, 18, 20, 18);
            panelContenido.Size = new Size(1469, 845);
            panelContenido.TabIndex = 2;
            // 
            // panelSolicitudesEliminacion
            // 
            panelSolicitudesEliminacion.BackColor = Color.White;
            panelSolicitudesEliminacion.Controls.Add(dgvSolicitudesEliminacion);
            panelSolicitudesEliminacion.Controls.Add(lblSolicitudesEliminacionEstado);
            panelSolicitudesEliminacion.Controls.Add(lblSolicitudesEliminacionTitulo);
            panelSolicitudesEliminacion.Location = new Point(343, 660);
            panelSolicitudesEliminacion.Name = "panelSolicitudesEliminacion";
            panelSolicitudesEliminacion.Size = new Size(380, 170);
            panelSolicitudesEliminacion.TabIndex = 8;
            // 
            // dgvSolicitudesEliminacion
            // 
            dgvSolicitudesEliminacion.AllowUserToAddRows = false;
            dgvSolicitudesEliminacion.AllowUserToDeleteRows = false;
            dgvSolicitudesEliminacion.AllowUserToResizeRows = false;
            dgvSolicitudesEliminacion.BackgroundColor = Color.White;
            dgvSolicitudesEliminacion.BorderStyle = BorderStyle.None;
            dgvSolicitudesEliminacion.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSolicitudesEliminacion.Location = new Point(14, 46);
            dgvSolicitudesEliminacion.MultiSelect = false;
            dgvSolicitudesEliminacion.Name = "dgvSolicitudesEliminacion";
            dgvSolicitudesEliminacion.ReadOnly = true;
            dgvSolicitudesEliminacion.RowHeadersVisible = false;
            dgvSolicitudesEliminacion.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSolicitudesEliminacion.Size = new Size(352, 110);
            dgvSolicitudesEliminacion.TabIndex = 2;
            // 
            // lblSolicitudesEliminacionEstado
            // 
            lblSolicitudesEliminacionEstado.AutoEllipsis = true;
            lblSolicitudesEliminacionEstado.Font = new Font("Segoe UI", 8.75F);
            lblSolicitudesEliminacionEstado.ForeColor = Color.FromArgb(100, 116, 139);
            lblSolicitudesEliminacionEstado.Location = new Point(14, 25);
            lblSolicitudesEliminacionEstado.Name = "lblSolicitudesEliminacionEstado";
            lblSolicitudesEliminacionEstado.Size = new Size(352, 17);
            lblSolicitudesEliminacionEstado.TabIndex = 1;
            lblSolicitudesEliminacionEstado.Text = "Cargando solicitudes de eliminación...";
            // 
            // lblSolicitudesEliminacionTitulo
            // 
            lblSolicitudesEliminacionTitulo.AutoSize = true;
            lblSolicitudesEliminacionTitulo.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblSolicitudesEliminacionTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblSolicitudesEliminacionTitulo.Location = new Point(14, 6);
            lblSolicitudesEliminacionTitulo.Name = "lblSolicitudesEliminacionTitulo";
            lblSolicitudesEliminacionTitulo.Size = new Size(183, 19);
            lblSolicitudesEliminacionTitulo.TabIndex = 0;
            lblSolicitudesEliminacionTitulo.Text = "Solicitudes de eliminación";
            // 
            // panelArchivosObservados
            // 
            panelArchivosObservados.BackColor = Color.White;
            panelArchivosObservados.Controls.Add(btnEliminarArchivoObservado);
            panelArchivosObservados.Controls.Add(btnCerrarObservacion);
            panelArchivosObservados.Controls.Add(dgvArchivosObservados);
            panelArchivosObservados.Controls.Add(lblArchivosObservadosEstado);
            panelArchivosObservados.Controls.Add(lblArchivosObservadosTitulo);
            panelArchivosObservados.Location = new Point(343, 470);
            panelArchivosObservados.Name = "panelArchivosObservados";
            panelArchivosObservados.Size = new Size(920, 178);
            panelArchivosObservados.TabIndex = 7;
            // 
            // btnEliminarArchivoObservado
            // 
            btnEliminarArchivoObservado.BackColor = Color.FromArgb(192, 57, 43);
            btnEliminarArchivoObservado.Cursor = Cursors.Hand;
            btnEliminarArchivoObservado.Enabled = false;
            btnEliminarArchivoObservado.FlatAppearance.BorderSize = 0;
            btnEliminarArchivoObservado.FlatStyle = FlatStyle.Flat;
            btnEliminarArchivoObservado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEliminarArchivoObservado.ForeColor = Color.White;
            btnEliminarArchivoObservado.Location = new Point(742, 10);
            btnEliminarArchivoObservado.Name = "btnEliminarArchivoObservado";
            btnEliminarArchivoObservado.Size = new Size(164, 28);
            btnEliminarArchivoObservado.TabIndex = 4;
            btnEliminarArchivoObservado.Text = "Eliminar archivo y cerrar";
            btnEliminarArchivoObservado.UseVisualStyleBackColor = false;
            btnEliminarArchivoObservado.Click += btnEliminarArchivoObservado_Click;
            // 
            // btnCerrarObservacion
            // 
            btnCerrarObservacion.BackColor = Color.FromArgb(22, 163, 74);
            btnCerrarObservacion.Cursor = Cursors.Hand;
            btnCerrarObservacion.Enabled = false;
            btnCerrarObservacion.FlatAppearance.BorderSize = 0;
            btnCerrarObservacion.FlatStyle = FlatStyle.Flat;
            btnCerrarObservacion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCerrarObservacion.ForeColor = Color.White;
            btnCerrarObservacion.Location = new Point(577, 10);
            btnCerrarObservacion.Name = "btnCerrarObservacion";
            btnCerrarObservacion.Size = new Size(159, 28);
            btnCerrarObservacion.TabIndex = 3;
            btnCerrarObservacion.Text = "Cerrar observación";
            btnCerrarObservacion.UseVisualStyleBackColor = false;
            btnCerrarObservacion.Click += btnCerrarObservacion_Click;
            // 
            // dgvArchivosObservados
            // 
            dgvArchivosObservados.AllowUserToAddRows = false;
            dgvArchivosObservados.AllowUserToDeleteRows = false;
            dgvArchivosObservados.AllowUserToResizeRows = false;
            dgvArchivosObservados.BackgroundColor = Color.White;
            dgvArchivosObservados.BorderStyle = BorderStyle.None;
            dgvArchivosObservados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvArchivosObservados.Location = new Point(14, 46);
            dgvArchivosObservados.MultiSelect = false;
            dgvArchivosObservados.Name = "dgvArchivosObservados";
            dgvArchivosObservados.ReadOnly = true;
            dgvArchivosObservados.RowHeadersVisible = false;
            dgvArchivosObservados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArchivosObservados.Size = new Size(892, 119);
            dgvArchivosObservados.TabIndex = 2;
            // 
            // lblArchivosObservadosEstado
            // 
            lblArchivosObservadosEstado.AutoEllipsis = true;
            lblArchivosObservadosEstado.Font = new Font("Segoe UI", 8.75F);
            lblArchivosObservadosEstado.ForeColor = Color.FromArgb(100, 116, 139);
            lblArchivosObservadosEstado.Location = new Point(14, 25);
            lblArchivosObservadosEstado.Name = "lblArchivosObservadosEstado";
            lblArchivosObservadosEstado.Size = new Size(892, 17);
            lblArchivosObservadosEstado.TabIndex = 1;
            lblArchivosObservadosEstado.Text = "Cargando archivos con observaciones...";
            // 
            // lblArchivosObservadosTitulo
            // 
            lblArchivosObservadosTitulo.AutoSize = true;
            lblArchivosObservadosTitulo.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblArchivosObservadosTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblArchivosObservadosTitulo.Location = new Point(14, 6);
            lblArchivosObservadosTitulo.Name = "lblArchivosObservadosTitulo";
            lblArchivosObservadosTitulo.Size = new Size(182, 19);
            lblArchivosObservadosTitulo.TabIndex = 0;
            lblArchivosObservadosTitulo.Text = "Archivos con observación";
            // 
            // panelMensajes
            // 
            panelMensajes.BackColor = Color.White;
            panelMensajes.Controls.Add(btnEnviarMensajeObservacion);
            panelMensajes.Controls.Add(txtMensajeObservacion);
            panelMensajes.Controls.Add(rtbMensajesObservacion);
            panelMensajes.Controls.Add(lblMensajesEstado);
            panelMensajes.Controls.Add(lblMensajesTitulo);
            panelMensajes.Location = new Point(18, 382);
            panelMensajes.Name = "panelMensajes";
            panelMensajes.Size = new Size(308, 160);
            panelMensajes.TabIndex = 6;
            // 
            // btnEnviarMensajeObservacion
            // 
            btnEnviarMensajeObservacion.BackColor = Color.FromArgb(37, 99, 235);
            btnEnviarMensajeObservacion.Cursor = Cursors.Hand;
            btnEnviarMensajeObservacion.FlatAppearance.BorderSize = 0;
            btnEnviarMensajeObservacion.FlatStyle = FlatStyle.Flat;
            btnEnviarMensajeObservacion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnEnviarMensajeObservacion.ForeColor = Color.White;
            btnEnviarMensajeObservacion.Location = new Point(220, 126);
            btnEnviarMensajeObservacion.Name = "btnEnviarMensajeObservacion";
            btnEnviarMensajeObservacion.Size = new Size(74, 28);
            btnEnviarMensajeObservacion.TabIndex = 4;
            btnEnviarMensajeObservacion.Text = "Enviar";
            btnEnviarMensajeObservacion.UseVisualStyleBackColor = false;
            btnEnviarMensajeObservacion.Click += btnEnviarMensajeObservacion_Click;
            // 
            // txtMensajeObservacion
            // 
            txtMensajeObservacion.BorderStyle = BorderStyle.FixedSingle;
            txtMensajeObservacion.Font = new Font("Segoe UI", 9.5F);
            txtMensajeObservacion.Location = new Point(14, 127);
            txtMensajeObservacion.MaxLength = 1000;
            txtMensajeObservacion.Name = "txtMensajeObservacion";
            txtMensajeObservacion.PlaceholderText = "Escriba una respuesta clara para esta observación...";
            txtMensajeObservacion.Size = new Size(200, 24);
            txtMensajeObservacion.TabIndex = 3;
            // 
            // rtbMensajesObservacion
            // 
            rtbMensajesObservacion.BackColor = Color.White;
            rtbMensajesObservacion.BorderStyle = BorderStyle.None;
            rtbMensajesObservacion.Font = new Font("Segoe UI", 9.5F);
            rtbMensajesObservacion.ForeColor = Color.FromArgb(30, 41, 59);
            rtbMensajesObservacion.Location = new Point(14, 44);
            rtbMensajesObservacion.Name = "rtbMensajesObservacion";
            rtbMensajesObservacion.ReadOnly = true;
            rtbMensajesObservacion.Size = new Size(280, 77);
            rtbMensajesObservacion.TabIndex = 2;
            rtbMensajesObservacion.Text = "";
            // 
            // lblMensajesEstado
            // 
            lblMensajesEstado.AutoEllipsis = true;
            lblMensajesEstado.Font = new Font("Segoe UI", 8.75F);
            lblMensajesEstado.ForeColor = Color.FromArgb(100, 116, 139);
            lblMensajesEstado.Location = new Point(14, 24);
            lblMensajesEstado.Name = "lblMensajesEstado";
            lblMensajesEstado.Size = new Size(280, 17);
            lblMensajesEstado.TabIndex = 1;
            lblMensajesEstado.Text = "Seleccione un archivo o una observación para ver la conversación";
            // 
            // lblMensajesTitulo
            // 
            lblMensajesTitulo.AutoSize = true;
            lblMensajesTitulo.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblMensajesTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblMensajesTitulo.Location = new Point(14, 6);
            lblMensajesTitulo.Name = "lblMensajesTitulo";
            lblMensajesTitulo.Size = new Size(197, 19);
            lblMensajesTitulo.TabIndex = 0;
            lblMensajesTitulo.Text = "Conversación y seguimiento";
            // 
            // lblResumenTitulo
            // 
            lblResumenTitulo.AutoSize = true;
            lblResumenTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblResumenTitulo.ForeColor = Color.FromArgb(30, 41, 59);
            lblResumenTitulo.Location = new Point(357, 96);
            lblResumenTitulo.Name = "lblResumenTitulo";
            lblResumenTitulo.Size = new Size(177, 20);
            lblResumenTitulo.TabIndex = 5;
            lblResumenTitulo.Text = "Resumen y seguimiento";
            // 
            // lblExploradorTitulo
            // 
            lblExploradorTitulo.AutoSize = true;
            lblExploradorTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblExploradorTitulo.ForeColor = Color.FromArgb(30, 41, 59);
            lblExploradorTitulo.Location = new Point(18, 96);
            lblExploradorTitulo.Name = "lblExploradorTitulo";
            lblExploradorTitulo.Size = new Size(144, 20);
            lblExploradorTitulo.TabIndex = 4;
            lblExploradorTitulo.Text = "Carpetas y archivos";
            // 
            // panelCabeceraContenido
            // 
            panelCabeceraContenido.BackColor = Color.White;
            panelCabeceraContenido.Controls.Add(lblRutaActual);
            panelCabeceraContenido.Controls.Add(lblVistaDescripcion);
            panelCabeceraContenido.Controls.Add(lblVistaTitulo);
            panelCabeceraContenido.Dock = DockStyle.Top;
            panelCabeceraContenido.Location = new Point(20, 18);
            panelCabeceraContenido.Name = "panelCabeceraContenido";
            panelCabeceraContenido.Size = new Size(1429, 64);
            panelCabeceraContenido.TabIndex = 3;
            // 
            // lblRutaActual
            // 
            lblRutaActual.AutoEllipsis = true;
            lblRutaActual.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblRutaActual.ForeColor = Color.FromArgb(37, 99, 235);
            lblRutaActual.Location = new Point(18, 39);
            lblRutaActual.Name = "lblRutaActual";
            lblRutaActual.Size = new Size(1195, 17);
            lblRutaActual.TabIndex = 2;
            lblRutaActual.Text = "Ruta actual: Inicio / Todas las carpetas";
            // 
            // lblVistaDescripcion
            // 
            lblVistaDescripcion.AutoSize = true;
            lblVistaDescripcion.Font = new Font("Segoe UI", 9.5F);
            lblVistaDescripcion.ForeColor = Color.FromArgb(100, 116, 139);
            lblVistaDescripcion.Location = new Point(228, 12);
            lblVistaDescripcion.Name = "lblVistaDescripcion";
            lblVistaDescripcion.Size = new Size(609, 17);
            lblVistaDescripcion.TabIndex = 1;
            lblVistaDescripcion.Text = "Empieza con los filtros, navega a la izquierda y usa el centro para atender observaciones paso a paso.";
            // 
            // lblVistaTitulo
            // 
            lblVistaTitulo.AutoSize = true;
            lblVistaTitulo.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            lblVistaTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblVistaTitulo.Location = new Point(18, 10);
            lblVistaTitulo.Name = "lblVistaTitulo";
            lblVistaTitulo.Size = new Size(223, 23);
            lblVistaTitulo.TabIndex = 0;
            lblVistaTitulo.Text = "Guía rápida de navegación";
            // 
            // lblTotalCarpetas
            // 
            lblTotalCarpetas.AutoSize = true;
            lblTotalCarpetas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalCarpetas.ForeColor = Color.FromArgb(71, 85, 105);
            lblTotalCarpetas.Location = new Point(18, 355);
            lblTotalCarpetas.Name = "lblTotalCarpetas";
            lblTotalCarpetas.Size = new Size(226, 19);
            lblTotalCarpetas.TabIndex = 1;
            lblTotalCarpetas.Text = "Total de carpetas encontradas: 0";
            // 
            // lvCarpetas
            // 
            lvCarpetas.BackColor = Color.White;
            lvCarpetas.BorderStyle = BorderStyle.FixedSingle;
            lvCarpetas.LargeImageList = imageListCarpetas;
            lvCarpetas.Location = new Point(18, 122);
            lvCarpetas.Margin = new Padding(3, 2, 3, 2);
            lvCarpetas.MultiSelect = false;
            lvCarpetas.Name = "lvCarpetas";
            lvCarpetas.Size = new Size(308, 227);
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
            ClientSize = new Size(1469, 977);
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
            panelSolicitudesEliminacion.ResumeLayout(false);
            panelSolicitudesEliminacion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSolicitudesEliminacion).EndInit();
            panelArchivosObservados.ResumeLayout(false);
            panelArchivosObservados.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvArchivosObservados).EndInit();
            panelMensajes.ResumeLayout(false);
            panelMensajes.PerformLayout();
            panelCabeceraContenido.ResumeLayout(false);
            panelCabeceraContenido.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelFiltros;
        private ComboBox cboRazonSocial;
        private Label lblRazonSocial;
        private CheckBox chkUsarPerfil;
        private Panel panelContenido;
        private System.Windows.Forms.ListView lvCarpetas;
        private System.Windows.Forms.ImageList imageListCarpetas;
        private Button btnActualizar;
        private Button btnLimpiarFiltro;
        private Label lblTotalCarpetas;
        private ComboBox cboAnio;
        private Label lblAnio;
        private Button btnRegresarCarpeta;
        private Button btnExportarPdf;
        private Label lblEstadoConexionPortal;
        private Button btnAgregarObservacion;
        private Label lblSubtituloFiltros;
        private Label lblTituloFiltros;
        private Panel panelCabeceraContenido;
        private Label lblRutaActual;
        private Label lblVistaDescripcion;
        private Label lblVistaTitulo;
        private Label lblResumenTitulo;
        private Label lblExploradorTitulo;
        private Panel panelArchivosObservados;
        private Button btnEliminarArchivoObservado;
        private Button btnCerrarObservacion;
        private DataGridView dgvArchivosObservados;
        private Label lblArchivosObservadosEstado;
        private Label lblArchivosObservadosTitulo;
        private Panel panelSolicitudesEliminacion;
        private DataGridView dgvSolicitudesEliminacion;
        private Label lblSolicitudesEliminacionEstado;
        private Label lblSolicitudesEliminacionTitulo;
        private Panel panelMensajes;
        private Button btnEnviarMensajeObservacion;
        private TextBox txtMensajeObservacion;
        private RichTextBox rtbMensajesObservacion;
        private Label lblMensajesEstado;
        private Label lblMensajesTitulo;
        private Button BtmEliminarArchivo;
    }
}
