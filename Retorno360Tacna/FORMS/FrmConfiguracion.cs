using System;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.CNX;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmConfiguracion : Form
    {
        private ConfiguracionUsuario configuracion;
        private decimal escalaOriginal;
        private Usuario? usuarioActual;
        private ConexionInfo? conexionActual;
        private int usuarioEditandoId = 0;

        public FrmConfiguracion(ConexionInfo conexion, Usuario? usuario = null)
        {
            InitializeComponent();
            conexionActual = conexion;
            usuarioActual = usuario;
            configuracion = new ConfiguracionUsuario();
            escalaOriginal = configuracion.EscalaUI;
        }

        private void FrmConfiguracion_Load(object sender, EventArgs e)
        {
            CargarConfiguracion();
            OcultarPanelAgregarUsuario();
            ConfigurarAccesoAdmin();
            InicializarCombos();
        }

        private void InicializarCombos()
        {
            cmbActivo.SelectedIndex = 0; // Activo por defecto
        }

        private void OcultarPanelAgregarUsuario()
        {
            panelAgregarUsuario.Visible = false;
            panelListaUsuarios.Visible = false;
            panelEditarUsuario.Visible = false;
        }

        private void ConfigurarAccesoAdmin()
        {
            // Debug: Verificar datos del usuario
            if (usuarioActual == null)
            {
                groupBoxUsuarios.Visible = false;
                return;
            }

            bool esAdmin = usuarioActual.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (esAdmin)
            {
                groupBoxUsuarios.Visible = true;
                panelListaUsuarios.Visible = true;
                CargarListaUsuarios();
            }
            else
            {
                groupBoxUsuarios.Visible = false;
                panelListaUsuarios.Visible = false;
            }
        }

        private void CargarConfiguracion()
        {
            // Cargar escala de UI
            decimal escalaActual = configuracion.EscalaUI;
            if (escalaActual == 1.0m)
                cmbEscalaUI.SelectedIndex = 0; // 100%
            else if (escalaActual == 1.25m)
                cmbEscalaUI.SelectedIndex = 1; // 125%
            else if (escalaActual == 1.5m)
                cmbEscalaUI.SelectedIndex = 2; // 150%
            else if (escalaActual == 1.75m)
                cmbEscalaUI.SelectedIndex = 3; // 175%
            else if (escalaActual == 2.0m)
                cmbEscalaUI.SelectedIndex = 4; // 200%
            else
                cmbEscalaUI.SelectedIndex = 0; // Default 100%

            lblEscalaActual.Text = $"Escala actual: {(configuracion.EscalaUI * 100):0}%";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Guardar escala seleccionada
                decimal nuevaEscala = ObtenerEscalaSeleccionada();
                configuracion.EscalaUI = nuevaEscala;

                // Verificar si cambió la escala
                if (nuevaEscala != escalaOriginal)
                {
                    var result = MessageBox.Show(
                        "Se ha cambiado la escala de la interfaz.\n\n" +
                        "La aplicación debe reiniciarse para aplicar los cambios.\n\n" +
                        "¿Desea reiniciar ahora?",
                        "Reinicio requerido",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Guardar en archivo
                        GuardarConfiguracionEnArchivo();

                        MessageBox.Show(
                            "La configuración se guardó correctamente.\n\n" +
                            "La aplicación se cerrará. Por favor, vuelva a abrirla.",
                            "Configuración guardada",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        Application.Exit();
                        return;
                    }
                }
                else
                {
                    GuardarConfiguracionEnArchivo();
                    MessageBox.Show(
                        "Configuración guardada correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar la configuración: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private decimal ObtenerEscalaSeleccionada()
        {
            return cmbEscalaUI.SelectedIndex switch
            {
                0 => 1.0m,   // 100%
                1 => 1.25m,  // 125%
                2 => 1.5m,   // 150%
                3 => 1.75m,  // 175%
                4 => 2.0m,   // 200%
                _ => 1.0m
            };
        }

        private void GuardarConfiguracionEnArchivo()
        {
            try
            {
                string rutaConfig = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Retorno360Tacna",
                    "config.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(rutaConfig)!);

                string[] lineas = new[]
                {
                    $"EscalaUI={configuracion.EscalaUI}"
                };

                File.WriteAllLines(rutaConfig, lineas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar archivo de configuración: {ex.Message}", ex);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            // En modo embebido, solo limpiamos el formulario
            var result = MessageBox.Show(
                "¿Desea descartar los cambios?",
                "Cancelar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CargarConfiguracion();
            }
        }

        private void cmbEscalaUI_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal escala = ObtenerEscalaSeleccionada();
            lblVistaPrevia.Text = $"Vista previa: {(escala * 100):0}%";
        }

        private void btnVistaPrevia_Click(object sender, EventArgs e)
        {
            decimal escala = ObtenerEscalaSeleccionada();
            MessageBox.Show(
                $"Con la escala al {(escala * 100):0}%, todos los controles, fuentes e imágenes\n" +
                $"se ajustarán proporcionalmente.\n\n" +
                $"• Botones y controles: {(escala * 100):0}% del tamaño original\n" +
                $"• Fuentes: {(escala * 100):0}% del tamaño original\n" +
                $"• Imágenes e iconos: {(escala * 100):0}% del tamaño original\n\n" +
                $"Nota: Para ver el cambio real, debe guardar y reiniciar la aplicación.",
                "Vista previa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnAgregarUsuario_Click(object sender, EventArgs e)
        {
            if (usuarioActual == null || !usuarioActual.NombreRol?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                MessageBox.Show(
                    "No tienes permisos para agregar usuarios.\nSolo los administradores pueden realizar esta acción.",
                    "Acceso Denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Ocultar otros paneles
            panelListaUsuarios.Visible = false;
            panelEditarUsuario.Visible = false;

            // Mostrar panel de agregar usuario
            panelAgregarUsuario.Visible = true;
            CargarRoles();
            LimpiarFormularioUsuario();
        }

        private void CargarRoles()
        {
            try
            {
                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "SELECT IdRol, NombreRol FROM Roles ORDER BY IdRol";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var roles = new List<Rol>();
                            while (reader.Read())
                            {
                                roles.Add(new Rol
                                {
                                    IdRol = reader.GetInt32(0),
                                    NombreRol = reader.GetString(1)
                                });
                            }

                            cmbRol.DataSource = roles;
                            cmbRol.DisplayMember = "NombreRol";
                            cmbRol.ValueMember = "IdRol";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormularioUsuario()
        {
            txtUserAlias.Clear();
            txtPasswordHash.Clear();
            txtNombreUsuario.Clear();
            txtApellidoUsuario.Clear();
            cmbActivo.SelectedIndex = 0; // Activo por defecto
            if (cmbRol.Items.Count > 0)
                cmbRol.SelectedIndex = 0;
        }

        private void btnGuardarUsuario_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(txtUserAlias.Text))
                {
                    MessageBox.Show("El campo UserAlias es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUserAlias.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPasswordHash.Text))
                {
                    MessageBox.Show("El campo Password es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPasswordHash.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
                {
                    MessageBox.Show("El campo Nombre es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombreUsuario.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtApellidoUsuario.Text))
                {
                    MessageBox.Show("El campo Apellido es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtApellidoUsuario.Focus();
                    return;
                }

                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();

                    // Verificar si el UserAlias ya existe
                    string queryCheck = "SELECT COUNT(*) FROM Usuarios WHERE UserAlias = @UserAlias";
                    using (SqlCommand cmdCheck = new SqlCommand(queryCheck, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@UserAlias", txtUserAlias.Text.Trim());
                        int count = (int)cmdCheck.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("El UserAlias ya existe. Por favor, elija otro.", "Usuario Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtUserAlias.Focus();
                            return;
                        }
                    }

                    // Insertar nuevo usuario
                    string queryInsert = @"
                        INSERT INTO Usuarios (UserAlias, PasswordHash, NombreUsuario, ApellidoUsuario, Activo, IdRol, FechaCreacion)
                        VALUES (@UserAlias, @PasswordHash, @NombreUsuario, @ApellidoUsuario, @Activo, @IdRol, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(queryInsert, conn))
                    {
                        // Calcular hash SHA256 de la contraseña
                        string passwordHash = CalcularHashSHA256(txtPasswordHash.Text.Trim());

                        cmd.Parameters.AddWithValue("@UserAlias", txtUserAlias.Text.Trim());
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@NombreUsuario", txtNombreUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@ApellidoUsuario", txtApellidoUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@Activo", cmbActivo.SelectedIndex == 0 ? 1 : 0);
                        cmd.Parameters.AddWithValue("@IdRol", cmbRol.SelectedValue ?? 1);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Usuario agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LimpiarFormularioUsuario();
                            panelAgregarUsuario.Visible = false;
                            panelListaUsuarios.Visible = true;
                            CargarListaUsuarios(); // Recargar lista con el nuevo usuario
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelarUsuario_Click(object sender, EventArgs e)
        {
            panelAgregarUsuario.Visible = false;
            panelListaUsuarios.Visible = true;
            LimpiarFormularioUsuario();
        }

        private string CalcularHashSHA256(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private void CargarListaUsuarios()
        {
            try
            {
                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            u.IdUsuario,
                            u.UserAlias,
                            u.NombreUsuario,
                            u.ApellidoUsuario,
                            CASE WHEN u.Activo = 1 THEN 'Sí' ELSE 'No' END AS Activo,
                            r.NombreRol AS Rol
                        FROM Usuarios u
                        INNER JOIN Roles r ON u.IdRol = r.IdRol
                        ORDER BY u.UserAlias";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            dgvUsuarios.DataSource = dt;

                            // Ocultar columna IdUsuario
                            if (dgvUsuarios.Columns["IdUsuario"] != null)
                                dgvUsuarios.Columns["IdUsuario"].Visible = false;

                            // Configurar encabezados
                            if (dgvUsuarios.Columns["UserAlias"] != null)
                                dgvUsuarios.Columns["UserAlias"].HeaderText = "Usuario";
                            if (dgvUsuarios.Columns["NombreUsuario"] != null)
                                dgvUsuarios.Columns["NombreUsuario"].HeaderText = "Nombre";
                            if (dgvUsuarios.Columns["ApellidoUsuario"] != null)
                                dgvUsuarios.Columns["ApellidoUsuario"].HeaderText = "Apellido";

                            // Eliminar botones existentes si los hay
                            if (dgvUsuarios.Columns.Contains("btnEditar"))
                                dgvUsuarios.Columns.Remove("btnEditar");
                            if (dgvUsuarios.Columns.Contains("btnEliminar"))
                                dgvUsuarios.Columns.Remove("btnEliminar");

                            // Agregar columna de botón Editar
                            DataGridViewButtonColumn btnEditar = new DataGridViewButtonColumn();
                            btnEditar.Name = "btnEditar";
                            btnEditar.HeaderText = "";
                            btnEditar.Text = "✏️ Editar";
                            btnEditar.UseColumnTextForButtonValue = true;
                            btnEditar.Width = 80;
                            dgvUsuarios.Columns.Add(btnEditar);

                            // Agregar columna de botón Eliminar
                            DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn();
                            btnEliminar.Name = "btnEliminar";
                            btnEliminar.HeaderText = "";
                            btnEliminar.Text = "🗑️ Eliminar";
                            btnEliminar.UseColumnTextForButtonValue = true;
                            btnEliminar.Width = 90;
                            dgvUsuarios.Columns.Add(btnEliminar);

                            // Configurar estilo de botones
                            dgvUsuarios.CellFormatting += DgvUsuarios_CellFormatting;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsuarios.Columns[e.ColumnIndex].Name == "btnEditar")
            {
                e.CellStyle.BackColor = Color.FromArgb(230, 126, 34);
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else if (dgvUsuarios.Columns[e.ColumnIndex].Name == "btnEliminar")
            {
                e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private void EliminarUsuario(int idUsuario)
        {
            try
            {
                // Obtener información del usuario para mostrarlo en el mensaje
                string nombreUsuario = "";
                foreach (DataGridViewRow row in dgvUsuarios.Rows)
                {
                    if (Convert.ToInt32(row.Cells["IdUsuario"].Value) == idUsuario)
                    {
                        nombreUsuario = row.Cells["UserAlias"].Value.ToString() ?? "";
                        break;
                    }
                }

                // Confirmar eliminación
                DialogResult resultado = MessageBox.Show(
                    $"¿Está seguro de que desea eliminar el usuario '{nombreUsuario}'?\n\nEsta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (resultado != DialogResult.Yes)
                    return;

                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show(
                                $"Usuario '{nombreUsuario}' eliminado correctamente.",
                                "Éxito",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Recargar lista
                            CargarListaUsuarios();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al eliminar usuario: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Obtener el IdUsuario de la fila seleccionada
                int idUsuario = Convert.ToInt32(dgvUsuarios.Rows[e.RowIndex].Cells["IdUsuario"].Value);

                // Verificar si se hizo clic en el botón Editar
                if (dgvUsuarios.Columns[e.ColumnIndex].Name == "btnEditar")
                {
                    CargarDatosUsuarioParaEditar(idUsuario);
                }
                // Verificar si se hizo clic en el botón Eliminar
                else if (dgvUsuarios.Columns[e.ColumnIndex].Name == "btnEliminar")
                {
                    EliminarUsuario(idUsuario);
                }
            }
        }

        private void CargarDatosUsuarioParaEditar(int idUsuario)
        {
            try
            {
                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            u.IdUsuario,
                            u.UserAlias,
                            u.NombreUsuario,
                            u.ApellidoUsuario,
                            u.Activo,
                            u.IdRol,
                            r.NombreRol
                        FROM Usuarios u
                        INNER JOIN Roles r ON u.IdRol = r.IdRol
                        WHERE u.IdUsuario = @IdUsuario";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuarioEditandoId = reader.GetInt32(0);
                                txtUserAliasEditar.Text = reader.GetString(1);
                                txtNombreEditar.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                txtApellidoEditar.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                cmbActivoEditar.SelectedIndex = reader.GetBoolean(4) ? 0 : 1;

                                // Guardar el IdRol para seleccionarlo después
                                int idRolActual = reader.GetInt32(5);

                                reader.Close();

                                // Cargar roles
                                CargarRolesEditar();

                                // Seleccionar el rol actual
                                for (int i = 0; i < cmbRolEditar.Items.Count; i++)
                                {
                                    Rol rol = (Rol)cmbRolEditar.Items[i];
                                    if (rol.IdRol == idRolActual)
                                    {
                                        cmbRolEditar.SelectedIndex = i;
                                        break;
                                    }
                                }

                                // Resetear contraseña
                                chkCambiarPassword.Checked = false;
                                txtNuevaPassword.Clear();

                                // Ocultar lista y mostrar panel de edición
                                panelListaUsuarios.Visible = false;
                                panelEditarUsuario.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos del usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarRolesEditar()
        {
            try
            {
                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    string query = "SELECT IdRol, NombreRol FROM Roles ORDER BY IdRol";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var roles = new List<Rol>();
                            while (reader.Read())
                            {
                                roles.Add(new Rol
                                {
                                    IdRol = reader.GetInt32(0),
                                    NombreRol = reader.GetString(1)
                                });
                            }

                            cmbRolEditar.DataSource = roles;
                            cmbRolEditar.DisplayMember = "NombreRol";
                            cmbRolEditar.ValueMember = "IdRol";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkCambiarPassword_CheckedChanged(object sender, EventArgs e)
        {
            lblNuevaPassword.Visible = chkCambiarPassword.Checked;
            txtNuevaPassword.Visible = chkCambiarPassword.Checked;
        }

        private void btnGuardarEdicion_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(txtNombreEditar.Text))
                {
                    MessageBox.Show("El campo Nombre es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombreEditar.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtApellidoEditar.Text))
                {
                    MessageBox.Show("El campo Apellido es requerido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtApellidoEditar.Focus();
                    return;
                }

                if (chkCambiarPassword.Checked && string.IsNullOrWhiteSpace(txtNuevaPassword.Text))
                {
                    MessageBox.Show("Si desea cambiar la contraseña, debe ingresar la nueva contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNuevaPassword.Focus();
                    return;
                }

                if (conexionActual == null) return;

                Conexion conexion = new Conexion(
                    conexionActual.Servidor!,
                    conexionActual.UsuarioSQL!,
                    conexionActual.PasswordSQL!,
                    "RetornoMaster"
                );

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();

                    string queryUpdate;
                    if (chkCambiarPassword.Checked)
                    {
                        // Actualizar con nueva contraseña
                        queryUpdate = @"
                            UPDATE Usuarios 
                            SET NombreUsuario = @NombreUsuario,
                                ApellidoUsuario = @ApellidoUsuario,
                                Activo = @Activo,
                                IdRol = @IdRol,
                                PasswordHash = @PasswordHash
                            WHERE IdUsuario = @IdUsuario";
                    }
                    else
                    {
                        // Actualizar sin cambiar contraseña
                        queryUpdate = @"
                            UPDATE Usuarios 
                            SET NombreUsuario = @NombreUsuario,
                                ApellidoUsuario = @ApellidoUsuario,
                                Activo = @Activo,
                                IdRol = @IdRol
                            WHERE IdUsuario = @IdUsuario";
                    }

                    using (SqlCommand cmd = new SqlCommand(queryUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", usuarioEditandoId);
                        cmd.Parameters.AddWithValue("@NombreUsuario", txtNombreEditar.Text.Trim());
                        cmd.Parameters.AddWithValue("@ApellidoUsuario", txtApellidoEditar.Text.Trim());
                        cmd.Parameters.AddWithValue("@Activo", cmbActivoEditar.SelectedIndex == 0 ? 1 : 0);
                        cmd.Parameters.AddWithValue("@IdRol", cmbRolEditar.SelectedValue ?? 1);

                        if (chkCambiarPassword.Checked)
                        {
                            string passwordHash = CalcularHashSHA256(txtNuevaPassword.Text.Trim());
                            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Usuario actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            panelEditarUsuario.Visible = false;
                            panelListaUsuarios.Visible = true;
                            CargarListaUsuarios(); // Recargar lista
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelarEdicion_Click(object sender, EventArgs e)
        {
            panelEditarUsuario.Visible = false;
            panelListaUsuarios.Visible = true;
            usuarioEditandoId = 0;
        }
    }
}
