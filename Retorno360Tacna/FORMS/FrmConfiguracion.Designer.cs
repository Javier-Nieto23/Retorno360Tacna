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
            chkAjustarPantallaLogica = new CheckBox();
            lblPantallaLogica = new Label();
            lblDescripcion = new Label();
            btnSalir = new Button();
            panelBotones = new Panel();
            lblTitulo = new Label();
            groupBoxUsuarios = new GroupBox();
            btnAgregarUsuario = new Button();
            panelAgregarUsuario = new Panel();
            btnCancelarUsuario = new Button();
            btnGuardarUsuario = new Button();
            cmbRol = new ComboBox();
            lblRol = new Label();
            cmbActivo = new ComboBox();
            lblActivo = new Label();
            txtApellidoUsuario = new TextBox();
            lblApellidoUsuario = new Label();
            txtNombreUsuario = new TextBox();
            lblNombreUsuario = new Label();
            txtPasswordHash = new TextBox();
            lblPasswordHash = new Label();
            txtConfirmarPassword = new TextBox();
            lblConfirmarPassword = new Label();
            txtUserAlias = new TextBox();
            lblUserAlias = new Label();
            lblTituloPanel = new Label();
            panelListaUsuarios = new Panel();
            dgvUsuarios = new DataGridView();
            panelEditarUsuario = new Panel();
            chkCambiarPassword = new CheckBox();
            txtNuevaPassword = new TextBox();
            lblNuevaPassword = new Label();
            btnCancelarEdicion = new Button();
            btnGuardarEdicion = new Button();
            cmbRolEditar = new ComboBox();
            lblRolEditar = new Label();
            cmbActivoEditar = new ComboBox();
            lblActivoEditar = new Label();
            txtApellidoEditar = new TextBox();
            lblApellidoEditar = new Label();
            txtNombreEditar = new TextBox();
            lblNombreEditar = new Label();
            btnVincularUsuarioWeb = new Button();
            cmbIdWebEditar = new ComboBox();
            lblIdWebEditar = new Label();
            txtUserAliasEditar = new TextBox();
            lblUserAliasEditar = new Label();
            lblTituloEditar = new Label();
            groupBoxInterfaz.SuspendLayout();
            groupBoxUsuarios.SuspendLayout();
            panelAgregarUsuario.SuspendLayout();
            panelListaUsuarios.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            panelEditarUsuario.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxInterfaz
            // 
            groupBoxInterfaz.Controls.Add(btnVistaPrevia);
            groupBoxInterfaz.Controls.Add(lblVistaPrevia);
            groupBoxInterfaz.Controls.Add(lblEscalaActual);
            groupBoxInterfaz.Controls.Add(cmbEscalaUI);
            groupBoxInterfaz.Controls.Add(lblEscalaUI);
            groupBoxInterfaz.Controls.Add(chkAjustarPantallaLogica);
            groupBoxInterfaz.Controls.Add(lblPantallaLogica);
            groupBoxInterfaz.Controls.Add(lblDescripcion);
            groupBoxInterfaz.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBoxInterfaz.ForeColor = Color.FromArgb(52, 73, 94);
            groupBoxInterfaz.Location = new Point(20, 80);
            groupBoxInterfaz.Name = "groupBoxInterfaz";
            groupBoxInterfaz.Size = new Size(560, 290);
            groupBoxInterfaz.TabIndex = 1;
            groupBoxInterfaz.TabStop = false;
            groupBoxInterfaz.Text = "Configuración de Pantalla";
            groupBoxInterfaz.Visible = false;
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
            // chkAjustarPantallaLogica
            // 
            chkAjustarPantallaLogica.AutoSize = true;
            chkAjustarPantallaLogica.Font = new Font("Segoe UI", 9.5F);
            chkAjustarPantallaLogica.Location = new Point(30, 165);
            chkAjustarPantallaLogica.Name = "chkAjustarPantallaLogica";
            chkAjustarPantallaLogica.Size = new Size(306, 21);
            chkAjustarPantallaLogica.TabIndex = 6;
            chkAjustarPantallaLogica.Text = "Ajustar ventana para pantalla lógica 1536 × 864";
            chkAjustarPantallaLogica.UseVisualStyleBackColor = true;
            // 
            // lblPantallaLogica
            // 
            lblPantallaLogica.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblPantallaLogica.ForeColor = Color.FromArgb(127, 140, 141);
            lblPantallaLogica.Location = new Point(30, 190);
            lblPantallaLogica.Name = "lblPantallaLogica";
            lblPantallaLogica.Size = new Size(500, 30);
            lblPantallaLogica.TabIndex = 7;
            lblPantallaLogica.Text = "Activa esta opción solo si deseas que la ventana del software se adapte a una pantalla lógica de 1536 × 864.";
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
            // btnSalir
            // 
            btnSalir.BackColor = Color.FromArgb(231, 76, 60);
            btnSalir.Cursor = Cursors.Hand;
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.FlatStyle = FlatStyle.Flat;
            btnSalir.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSalir.ForeColor = Color.White;
            btnSalir.Location = new Point(121, 20);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(100, 30);
            btnSalir.TabIndex = 1;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = false;
            btnSalir.Click += btnSalir_Click;
            // 
            // panelBotones
            // 
            panelBotones.Location = new Point(331, 670);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(609, 107);
            panelBotones.TabIndex = 2;
            panelBotones.Visible = false;
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
            lblTitulo.Visible = false;
            // 
            // groupBoxUsuarios
            // 
            groupBoxUsuarios.Controls.Add(btnSalir);
            groupBoxUsuarios.Controls.Add(btnAgregarUsuario);
            groupBoxUsuarios.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBoxUsuarios.ForeColor = Color.FromArgb(52, 73, 94);
            groupBoxUsuarios.Location = new Point(20, 80);
            groupBoxUsuarios.Name = "groupBoxUsuarios";
            groupBoxUsuarios.Size = new Size(1160, 60);
            groupBoxUsuarios.TabIndex = 4;
            groupBoxUsuarios.TabStop = false;
            groupBoxUsuarios.Text = "Usuarios";
            groupBoxUsuarios.Visible = false;
            // 
            // btnAgregarUsuario
            // 
            btnAgregarUsuario.BackColor = Color.FromArgb(41, 128, 185);
            btnAgregarUsuario.Cursor = Cursors.Hand;
            btnAgregarUsuario.FlatAppearance.BorderSize = 0;
            btnAgregarUsuario.FlatStyle = FlatStyle.Flat;
            btnAgregarUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAgregarUsuario.ForeColor = Color.White;
            btnAgregarUsuario.Location = new Point(15, 20);
            btnAgregarUsuario.Name = "btnAgregarUsuario";
            btnAgregarUsuario.Size = new Size(100, 30);
            btnAgregarUsuario.TabIndex = 1;
            btnAgregarUsuario.Text = "➕ Nuevo";
            btnAgregarUsuario.UseVisualStyleBackColor = false;
            btnAgregarUsuario.Click += btnAgregarUsuario_Click;
            // 
            // panelAgregarUsuario
            // 
            panelAgregarUsuario.BackColor = Color.FromArgb(236, 240, 241);
            panelAgregarUsuario.BorderStyle = BorderStyle.FixedSingle;
            panelAgregarUsuario.Controls.Add(btnCancelarUsuario);
            panelAgregarUsuario.Controls.Add(btnGuardarUsuario);
            panelAgregarUsuario.Controls.Add(cmbRol);
            panelAgregarUsuario.Controls.Add(lblRol);
            panelAgregarUsuario.Controls.Add(cmbActivo);
            panelAgregarUsuario.Controls.Add(lblActivo);
            panelAgregarUsuario.Controls.Add(txtApellidoUsuario);
            panelAgregarUsuario.Controls.Add(lblApellidoUsuario);
            panelAgregarUsuario.Controls.Add(txtNombreUsuario);
            panelAgregarUsuario.Controls.Add(lblNombreUsuario);
            panelAgregarUsuario.Controls.Add(txtPasswordHash);
            panelAgregarUsuario.Controls.Add(lblPasswordHash);
            panelAgregarUsuario.Controls.Add(txtConfirmarPassword);
            panelAgregarUsuario.Controls.Add(lblConfirmarPassword);
            panelAgregarUsuario.Controls.Add(txtUserAlias);
            panelAgregarUsuario.Controls.Add(lblUserAlias);
            panelAgregarUsuario.Controls.Add(lblTituloPanel);
            panelAgregarUsuario.Location = new Point(20, 150);
            panelAgregarUsuario.Name = "panelAgregarUsuario";
            panelAgregarUsuario.Size = new Size(1160, 500);
            panelAgregarUsuario.TabIndex = 5;
            panelAgregarUsuario.Visible = false;
            // 
            // btnCancelarUsuario
            // 
            btnCancelarUsuario.BackColor = Color.FromArgb(231, 76, 60);
            btnCancelarUsuario.Cursor = Cursors.Hand;
            btnCancelarUsuario.FlatAppearance.BorderSize = 0;
            btnCancelarUsuario.FlatStyle = FlatStyle.Flat;
            btnCancelarUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelarUsuario.ForeColor = Color.White;
            btnCancelarUsuario.Location = new Point(430, 320);
            btnCancelarUsuario.Name = "btnCancelarUsuario";
            btnCancelarUsuario.Size = new Size(110, 40);
            btnCancelarUsuario.TabIndex = 14;
            btnCancelarUsuario.Text = "Cancelar";
            btnCancelarUsuario.UseVisualStyleBackColor = false;
            btnCancelarUsuario.Click += btnCancelarUsuario_Click;
            // 
            // btnGuardarUsuario
            // 
            btnGuardarUsuario.BackColor = Color.FromArgb(39, 174, 96);
            btnGuardarUsuario.Cursor = Cursors.Hand;
            btnGuardarUsuario.FlatAppearance.BorderSize = 0;
            btnGuardarUsuario.FlatStyle = FlatStyle.Flat;
            btnGuardarUsuario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardarUsuario.ForeColor = Color.White;
            btnGuardarUsuario.Location = new Point(310, 320);
            btnGuardarUsuario.Name = "btnGuardarUsuario";
            btnGuardarUsuario.Size = new Size(110, 40);
            btnGuardarUsuario.TabIndex = 13;
            btnGuardarUsuario.Text = "Guardar";
            btnGuardarUsuario.UseVisualStyleBackColor = false;
            btnGuardarUsuario.Click += btnGuardarUsuario_Click;
            // 
            // cmbRol
            // 
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.Font = new Font("Segoe UI", 10F);
            cmbRol.FormattingEnabled = true;
            cmbRol.Location = new Point(180, 297);
            cmbRol.Name = "cmbRol";
            cmbRol.Size = new Size(350, 25);
            cmbRol.TabIndex = 12;
            // 
            // lblRol
            // 
            lblRol.AutoSize = true;
            lblRol.Font = new Font("Segoe UI", 10F);
            lblRol.ForeColor = Color.FromArgb(52, 73, 94);
            lblRol.Location = new Point(30, 300);
            lblRol.Name = "lblRol";
            lblRol.Size = new Size(31, 19);
            lblRol.TabIndex = 11;
            lblRol.Text = "Rol:";
            // 
            // cmbActivo
            // 
            cmbActivo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActivo.Font = new Font("Segoe UI", 10F);
            cmbActivo.FormattingEnabled = true;
            cmbActivo.Items.AddRange(new object[] { "Sí", "No" });
            cmbActivo.Location = new Point(180, 257);
            cmbActivo.Name = "cmbActivo";
            cmbActivo.Size = new Size(150, 25);
            cmbActivo.TabIndex = 10;
            // 
            // lblActivo
            // 
            lblActivo.AutoSize = true;
            lblActivo.Font = new Font("Segoe UI", 10F);
            lblActivo.ForeColor = Color.FromArgb(52, 73, 94);
            lblActivo.Location = new Point(30, 260);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(50, 19);
            lblActivo.TabIndex = 9;
            lblActivo.Text = "Activo:";
            // 
            // txtApellidoUsuario
            // 
            txtApellidoUsuario.Font = new Font("Segoe UI", 10F);
            txtApellidoUsuario.Location = new Point(180, 217);
            txtApellidoUsuario.Name = "txtApellidoUsuario";
            txtApellidoUsuario.Size = new Size(350, 25);
            txtApellidoUsuario.TabIndex = 8;
            // 
            // lblApellidoUsuario
            // 
            lblApellidoUsuario.AutoSize = true;
            lblApellidoUsuario.Font = new Font("Segoe UI", 10F);
            lblApellidoUsuario.ForeColor = Color.FromArgb(52, 73, 94);
            lblApellidoUsuario.Location = new Point(30, 220);
            lblApellidoUsuario.Name = "lblApellidoUsuario";
            lblApellidoUsuario.Size = new Size(61, 19);
            lblApellidoUsuario.TabIndex = 7;
            lblApellidoUsuario.Text = "Apellido:";
            // 
            // txtNombreUsuario
            // 
            txtNombreUsuario.Font = new Font("Segoe UI", 10F);
            txtNombreUsuario.Location = new Point(180, 177);
            txtNombreUsuario.Name = "txtNombreUsuario";
            txtNombreUsuario.Size = new Size(350, 25);
            txtNombreUsuario.TabIndex = 6;
            // 
            // lblNombreUsuario
            // 
            lblNombreUsuario.AutoSize = true;
            lblNombreUsuario.Font = new Font("Segoe UI", 10F);
            lblNombreUsuario.ForeColor = Color.FromArgb(52, 73, 94);
            lblNombreUsuario.Location = new Point(30, 180);
            lblNombreUsuario.Name = "lblNombreUsuario";
            lblNombreUsuario.Size = new Size(62, 19);
            lblNombreUsuario.TabIndex = 5;
            lblNombreUsuario.Text = "Nombre:";
            // 
            // txtPasswordHash
            // 
            txtPasswordHash.Font = new Font("Segoe UI", 10F);
            txtPasswordHash.Location = new Point(180, 97);
            txtPasswordHash.Name = "txtPasswordHash";
            txtPasswordHash.PasswordChar = '●';
            txtPasswordHash.Size = new Size(350, 25);
            txtPasswordHash.TabIndex = 4;
            // 
            // lblPasswordHash
            // 
            lblPasswordHash.AutoSize = true;
            lblPasswordHash.Font = new Font("Segoe UI", 10F);
            lblPasswordHash.ForeColor = Color.FromArgb(52, 73, 94);
            lblPasswordHash.Location = new Point(30, 100);
            lblPasswordHash.Name = "lblPasswordHash";
            lblPasswordHash.Size = new Size(82, 19);
            lblPasswordHash.TabIndex = 3;
            lblPasswordHash.Text = "Contraseña:";
            // 
            // txtConfirmarPassword
            // 
            txtConfirmarPassword.Font = new Font("Segoe UI", 10F);
            txtConfirmarPassword.Location = new Point(180, 137);
            txtConfirmarPassword.Name = "txtConfirmarPassword";
            txtConfirmarPassword.PasswordChar = '●';
            txtConfirmarPassword.Size = new Size(350, 25);
            txtConfirmarPassword.TabIndex = 16;
            // 
            // lblConfirmarPassword
            // 
            lblConfirmarPassword.AutoSize = true;
            lblConfirmarPassword.Font = new Font("Segoe UI", 10F);
            lblConfirmarPassword.ForeColor = Color.FromArgb(52, 73, 94);
            lblConfirmarPassword.Location = new Point(30, 140);
            lblConfirmarPassword.Name = "lblConfirmarPassword";
            lblConfirmarPassword.Size = new Size(144, 19);
            lblConfirmarPassword.TabIndex = 15;
            lblConfirmarPassword.Text = "Confirmar contraseña:";
            // 
            // txtUserAlias
            // 
            txtUserAlias.Font = new Font("Segoe UI", 10F);
            txtUserAlias.Location = new Point(180, 57);
            txtUserAlias.Name = "txtUserAlias";
            txtUserAlias.Size = new Size(350, 25);
            txtUserAlias.TabIndex = 2;
            // 
            // lblUserAlias
            // 
            lblUserAlias.AutoSize = true;
            lblUserAlias.Font = new Font("Segoe UI", 10F);
            lblUserAlias.ForeColor = Color.FromArgb(52, 73, 94);
            lblUserAlias.Location = new Point(30, 60);
            lblUserAlias.Name = "lblUserAlias";
            lblUserAlias.Size = new Size(68, 19);
            lblUserAlias.TabIndex = 1;
            lblUserAlias.Text = "UserAlias:";
            // 
            // lblTituloPanel
            // 
            lblTituloPanel.AutoSize = true;
            lblTituloPanel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloPanel.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloPanel.Location = new Point(15, 15);
            lblTituloPanel.Name = "lblTituloPanel";
            lblTituloPanel.Size = new Size(189, 21);
            lblTituloPanel.TabIndex = 0;
            lblTituloPanel.Text = "Agregar Nuevo Usuario";
            // 
            // panelListaUsuarios
            // 
            panelListaUsuarios.BackColor = Color.White;
            panelListaUsuarios.BorderStyle = BorderStyle.FixedSingle;
            panelListaUsuarios.Controls.Add(dgvUsuarios);
            panelListaUsuarios.Location = new Point(20, 150);
            panelListaUsuarios.Name = "panelListaUsuarios";
            panelListaUsuarios.Size = new Size(1160, 500);
            panelListaUsuarios.TabIndex = 6;
            panelListaUsuarios.Visible = false;
            // 
            // dgvUsuarios
            // 
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsuarios.BackgroundColor = Color.White;
            dgvUsuarios.BorderStyle = BorderStyle.None;
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsuarios.Location = new Point(10, 10);
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.Size = new Size(1135, 475);
            dgvUsuarios.TabIndex = 1;
            dgvUsuarios.CellClick += dgvUsuarios_CellClick;
            // 
            // panelEditarUsuario
            // 
            panelEditarUsuario.BackColor = Color.FromArgb(236, 240, 241);
            panelEditarUsuario.BorderStyle = BorderStyle.FixedSingle;
            panelEditarUsuario.Controls.Add(chkCambiarPassword);
            panelEditarUsuario.Controls.Add(txtNuevaPassword);
            panelEditarUsuario.Controls.Add(lblNuevaPassword);
            panelEditarUsuario.Controls.Add(btnCancelarEdicion);
            panelEditarUsuario.Controls.Add(btnGuardarEdicion);
            panelEditarUsuario.Controls.Add(cmbRolEditar);
            panelEditarUsuario.Controls.Add(lblRolEditar);
            panelEditarUsuario.Controls.Add(cmbActivoEditar);
            panelEditarUsuario.Controls.Add(lblActivoEditar);
            panelEditarUsuario.Controls.Add(txtApellidoEditar);
            panelEditarUsuario.Controls.Add(lblApellidoEditar);
            panelEditarUsuario.Controls.Add(txtNombreEditar);
            panelEditarUsuario.Controls.Add(lblNombreEditar);
            panelEditarUsuario.Controls.Add(btnVincularUsuarioWeb);
            panelEditarUsuario.Controls.Add(cmbIdWebEditar);
            panelEditarUsuario.Controls.Add(lblIdWebEditar);
            panelEditarUsuario.Controls.Add(txtUserAliasEditar);
            panelEditarUsuario.Controls.Add(lblUserAliasEditar);
            panelEditarUsuario.Controls.Add(lblTituloEditar);
            panelEditarUsuario.Location = new Point(20, 150);
            panelEditarUsuario.Name = "panelEditarUsuario";
            panelEditarUsuario.Size = new Size(1160, 500);
            panelEditarUsuario.TabIndex = 7;
            panelEditarUsuario.Visible = false;
            // 
            // chkCambiarPassword
            // 
            chkCambiarPassword.AutoSize = true;
            chkCambiarPassword.Font = new Font("Segoe UI", 10F);
            chkCambiarPassword.ForeColor = Color.FromArgb(52, 73, 94);
            chkCambiarPassword.Location = new Point(30, 270);
            chkCambiarPassword.Name = "chkCambiarPassword";
            chkCambiarPassword.Size = new Size(150, 23);
            chkCambiarPassword.TabIndex = 11;
            chkCambiarPassword.Text = "Cambiar contraseña";
            chkCambiarPassword.UseVisualStyleBackColor = true;
            chkCambiarPassword.CheckedChanged += chkCambiarPassword_CheckedChanged;
            // 
            // txtNuevaPassword
            // 
            txtNuevaPassword.Font = new Font("Segoe UI", 10F);
            txtNuevaPassword.Location = new Point(180, 307);
            txtNuevaPassword.Name = "txtNuevaPassword";
            txtNuevaPassword.PasswordChar = '●';
            txtNuevaPassword.Size = new Size(350, 25);
            txtNuevaPassword.TabIndex = 13;
            txtNuevaPassword.Visible = false;
            // 
            // lblNuevaPassword
            // 
            lblNuevaPassword.AutoSize = true;
            lblNuevaPassword.Font = new Font("Segoe UI", 10F);
            lblNuevaPassword.ForeColor = Color.FromArgb(52, 73, 94);
            lblNuevaPassword.Location = new Point(30, 310);
            lblNuevaPassword.Name = "lblNuevaPassword";
            lblNuevaPassword.Size = new Size(125, 19);
            lblNuevaPassword.TabIndex = 12;
            lblNuevaPassword.Text = "Nueva Contraseña:";
            lblNuevaPassword.Visible = false;
            // 
            // btnCancelarEdicion
            // 
            btnCancelarEdicion.BackColor = Color.FromArgb(231, 76, 60);
            btnCancelarEdicion.Cursor = Cursors.Hand;
            btnCancelarEdicion.FlatAppearance.BorderSize = 0;
            btnCancelarEdicion.FlatStyle = FlatStyle.Flat;
            btnCancelarEdicion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelarEdicion.ForeColor = Color.White;
            btnCancelarEdicion.Location = new Point(430, 445);
            btnCancelarEdicion.Name = "btnCancelarEdicion";
            btnCancelarEdicion.Size = new Size(110, 40);
            btnCancelarEdicion.TabIndex = 15;
            btnCancelarEdicion.Text = "Cancelar";
            btnCancelarEdicion.UseVisualStyleBackColor = false;
            btnCancelarEdicion.Click += btnCancelarEdicion_Click;
            // 
            // btnGuardarEdicion
            // 
            btnGuardarEdicion.BackColor = Color.FromArgb(39, 174, 96);
            btnGuardarEdicion.Cursor = Cursors.Hand;
            btnGuardarEdicion.FlatAppearance.BorderSize = 0;
            btnGuardarEdicion.FlatStyle = FlatStyle.Flat;
            btnGuardarEdicion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardarEdicion.ForeColor = Color.White;
            btnGuardarEdicion.Location = new Point(310, 445);
            btnGuardarEdicion.Name = "btnGuardarEdicion";
            btnGuardarEdicion.Size = new Size(110, 40);
            btnGuardarEdicion.TabIndex = 14;
            btnGuardarEdicion.Text = "Guardar";
            btnGuardarEdicion.UseVisualStyleBackColor = false;
            btnGuardarEdicion.Click += btnGuardarEdicion_Click;
            // 
            // cmbRolEditar
            // 
            cmbRolEditar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRolEditar.Font = new Font("Segoe UI", 10F);
            cmbRolEditar.FormattingEnabled = true;
            cmbRolEditar.Location = new Point(180, 217);
            cmbRolEditar.Name = "cmbRolEditar";
            cmbRolEditar.Size = new Size(350, 25);
            cmbRolEditar.TabIndex = 10;
            // 
            // lblRolEditar
            // 
            lblRolEditar.AutoSize = true;
            lblRolEditar.Font = new Font("Segoe UI", 10F);
            lblRolEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblRolEditar.Location = new Point(30, 220);
            lblRolEditar.Name = "lblRolEditar";
            lblRolEditar.Size = new Size(31, 19);
            lblRolEditar.TabIndex = 9;
            lblRolEditar.Text = "Rol:";
            // 
            // cmbActivoEditar
            // 
            cmbActivoEditar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActivoEditar.Font = new Font("Segoe UI", 10F);
            cmbActivoEditar.FormattingEnabled = true;
            cmbActivoEditar.Items.AddRange(new object[] { "Sí", "No" });
            cmbActivoEditar.Location = new Point(180, 177);
            cmbActivoEditar.Name = "cmbActivoEditar";
            cmbActivoEditar.Size = new Size(150, 25);
            cmbActivoEditar.TabIndex = 8;
            // 
            // lblActivoEditar
            // 
            lblActivoEditar.AutoSize = true;
            lblActivoEditar.Font = new Font("Segoe UI", 10F);
            lblActivoEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblActivoEditar.Location = new Point(30, 180);
            lblActivoEditar.Name = "lblActivoEditar";
            lblActivoEditar.Size = new Size(50, 19);
            lblActivoEditar.TabIndex = 7;
            lblActivoEditar.Text = "Activo:";
            // 
            // txtApellidoEditar
            // 
            txtApellidoEditar.Font = new Font("Segoe UI", 10F);
            txtApellidoEditar.Location = new Point(180, 137);
            txtApellidoEditar.Name = "txtApellidoEditar";
            txtApellidoEditar.Size = new Size(350, 25);
            txtApellidoEditar.TabIndex = 6;
            // 
            // lblApellidoEditar
            // 
            lblApellidoEditar.AutoSize = true;
            lblApellidoEditar.Font = new Font("Segoe UI", 10F);
            lblApellidoEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblApellidoEditar.Location = new Point(30, 140);
            lblApellidoEditar.Name = "lblApellidoEditar";
            lblApellidoEditar.Size = new Size(61, 19);
            lblApellidoEditar.TabIndex = 5;
            lblApellidoEditar.Text = "Apellido:";
            // 
            // txtNombreEditar
            // 
            txtNombreEditar.Font = new Font("Segoe UI", 10F);
            txtNombreEditar.Location = new Point(180, 97);
            txtNombreEditar.Name = "txtNombreEditar";
            txtNombreEditar.Size = new Size(350, 25);
            txtNombreEditar.TabIndex = 4;
            // 
            // lblNombreEditar
            // 
            lblNombreEditar.AutoSize = true;
            lblNombreEditar.Font = new Font("Segoe UI", 10F);
            lblNombreEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblNombreEditar.Location = new Point(30, 100);
            lblNombreEditar.Name = "lblNombreEditar";
            lblNombreEditar.Size = new Size(62, 19);
            lblNombreEditar.TabIndex = 3;
            lblNombreEditar.Text = "Nombre:";
            // 
            // btnVincularUsuarioWeb
            // 
            btnVincularUsuarioWeb.BackColor = Color.FromArgb(142, 68, 173);
            btnVincularUsuarioWeb.Cursor = Cursors.Hand;
            btnVincularUsuarioWeb.FlatAppearance.BorderSize = 0;
            btnVincularUsuarioWeb.FlatStyle = FlatStyle.Flat;
            btnVincularUsuarioWeb.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnVincularUsuarioWeb.ForeColor = Color.White;
            btnVincularUsuarioWeb.Location = new Point(560, 355);
            btnVincularUsuarioWeb.Name = "btnVincularUsuarioWeb";
            btnVincularUsuarioWeb.Size = new Size(180, 35);
            btnVincularUsuarioWeb.TabIndex = 14;
            btnVincularUsuarioWeb.Text = "Vincular usuario web";
            btnVincularUsuarioWeb.UseVisualStyleBackColor = false;
            btnVincularUsuarioWeb.Click += btnVincularUsuarioWeb_Click;
            // 
            // cmbIdWebEditar
            // 
            cmbIdWebEditar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIdWebEditar.Font = new Font("Segoe UI", 10F);
            cmbIdWebEditar.FormattingEnabled = true;
            cmbIdWebEditar.Location = new Point(180, 355);
            cmbIdWebEditar.Name = "cmbIdWebEditar";
            cmbIdWebEditar.Size = new Size(350, 25);
            cmbIdWebEditar.TabIndex = 14;
            // 
            // lblIdWebEditar
            // 
            lblIdWebEditar.AutoSize = true;
            lblIdWebEditar.Font = new Font("Segoe UI", 10F);
            lblIdWebEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblIdWebEditar.Location = new Point(30, 358);
            lblIdWebEditar.Name = "lblIdWebEditar";
            lblIdWebEditar.Size = new Size(57, 19);
            lblIdWebEditar.TabIndex = 13;
            lblIdWebEditar.Text = "ID Web:";
            // 
            // txtUserAliasEditar
            // 
            txtUserAliasEditar.BackColor = Color.FromArgb(220, 220, 220);
            txtUserAliasEditar.Font = new Font("Segoe UI", 10F);
            txtUserAliasEditar.Location = new Point(180, 57);
            txtUserAliasEditar.Name = "txtUserAliasEditar";
            txtUserAliasEditar.ReadOnly = true;
            txtUserAliasEditar.Size = new Size(350, 25);
            txtUserAliasEditar.TabIndex = 2;
            // 
            // lblUserAliasEditar
            // 
            lblUserAliasEditar.AutoSize = true;
            lblUserAliasEditar.Font = new Font("Segoe UI", 10F);
            lblUserAliasEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblUserAliasEditar.Location = new Point(30, 60);
            lblUserAliasEditar.Name = "lblUserAliasEditar";
            lblUserAliasEditar.Size = new Size(68, 19);
            lblUserAliasEditar.TabIndex = 1;
            lblUserAliasEditar.Text = "UserAlias:";
            // 
            // lblTituloEditar
            // 
            lblTituloEditar.AutoSize = true;
            lblTituloEditar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTituloEditar.ForeColor = Color.FromArgb(52, 73, 94);
            lblTituloEditar.Location = new Point(15, 15);
            lblTituloEditar.Name = "lblTituloEditar";
            lblTituloEditar.Size = new Size(118, 21);
            lblTituloEditar.TabIndex = 0;
            lblTituloEditar.Text = "Editar Usuario";
            // 
            // FrmConfiguracion
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = Color.White;
            ClientSize = new Size(1207, 720);
            Controls.Add(panelEditarUsuario);
            Controls.Add(panelBotones);
            Controls.Add(panelListaUsuarios);
            Controls.Add(panelAgregarUsuario);
            Controls.Add(groupBoxUsuarios);
            Controls.Add(lblTitulo);
            Controls.Add(groupBoxInterfaz);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmConfiguracion";
            Text = "Configuración - Retorno 360 Tacna";
            Load += FrmConfiguracion_Load;
            groupBoxInterfaz.ResumeLayout(false);
            groupBoxInterfaz.PerformLayout();
            groupBoxUsuarios.ResumeLayout(false);
            panelAgregarUsuario.ResumeLayout(false);
            panelAgregarUsuario.PerformLayout();
            panelListaUsuarios.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            panelEditarUsuario.ResumeLayout(false);
            panelEditarUsuario.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxInterfaz;
        private ComboBox cmbEscalaUI;
        private Label lblEscalaUI;
        private Label lblDescripcion;
        private Button btnSalir;
        private Panel panelBotones;
        private Label lblTitulo;
        private Label lblEscalaActual;
        private Label lblVistaPrevia;
        private Button btnVistaPrevia;
        private CheckBox chkAjustarPantallaLogica;
        private Label lblPantallaLogica;
        private GroupBox groupBoxUsuarios;
        private Button btnAgregarUsuario;
        private Panel panelAgregarUsuario;
        private Label lblTituloPanel;
        private Label lblUserAlias;
        private TextBox txtUserAlias;
        private Label lblPasswordHash;
        private TextBox txtPasswordHash;
        private Label lblConfirmarPassword;
        private TextBox txtConfirmarPassword;
        private Label lblNombreUsuario;
        private TextBox txtNombreUsuario;
        private Label lblApellidoUsuario;
        private TextBox txtApellidoUsuario;
        private Label lblActivo;
        private ComboBox cmbActivo;
        private Label lblRol;
        private ComboBox cmbRol;
        private Button btnGuardarUsuario;
        private Button btnCancelarUsuario;
        private Panel panelListaUsuarios;
        private DataGridView dgvUsuarios;
        private Panel panelEditarUsuario;
        private CheckBox chkCambiarPassword;
        private TextBox txtNuevaPassword;
        private Label lblNuevaPassword;
        private Button btnCancelarEdicion;
        private Button btnGuardarEdicion;
        private ComboBox cmbRolEditar;
        private Label lblRolEditar;
        private ComboBox cmbActivoEditar;
        private Label lblActivoEditar;
        private TextBox txtApellidoEditar;
        private Label lblApellidoEditar;
        private TextBox txtNombreEditar;
        private Label lblNombreEditar;
        private TextBox txtUserAliasEditar;
        private Label lblUserAliasEditar;
        private Label lblTituloEditar;
        private Button btnVincularUsuarioWeb;
        private ComboBox cmbIdWebEditar;
        private Label lblIdWebEditar;
    }
}
