using Retorno360Tacna.SERVICES;
using System;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDbConnectionModal : Form
    {
        public event EventHandler? SolicitarRegreso;

        public FrmDbConnectionModal()
        {
            InitializeComponent();
            CargarConexionGuardada();
        }

        private void CargarConexionGuardada()
        {
            if (SecretStoreService.TryGetSecret("DATABASE_URL", out string existing))
            {
                txtConnection.Text = existing;
            }

            txtConnection.UseSystemPasswordChar = true;
            chkShow.Checked = false;
        }

        private void chkShow_CheckedChanged(object? sender, EventArgs e)
        {
            txtConnection.UseSystemPasswordChar = !chkShow.Checked;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string value = txtConnection.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show(this, "La cadena de conexión no puede estar vacía.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) && value.IndexOf("Host=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                var res = MessageBox.Show(this, "La cadena no parece ser una URL PostgreSQL ni una cadena Npgsql. ¿Desea guardarla de todos modos?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes)
                    return;
            }

            try
            {
                SecretStoreService.SaveSecret("DATABASE_URL", value);
                MessageBox.Show(this, "Cadena guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SolicitarRegreso?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No fue posible guardar la clave: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnTest_Click(object? sender, EventArgs e)
        {
            await BtnTest_ClickAsync();
        }

        private async System.Threading.Tasks.Task BtnTest_ClickAsync()
        {
            string value = txtConnection.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show(this, "La cadena de conexión está vacía.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (value.Contains(".railway.internal", StringComparison.OrdinalIgnoreCase))
            {
                var r = MessageBox.Show(this, "La URL contiene un host '.railway.internal' que puede no ser accesible desde tu máquina. ¿Deseas intentar la conexión de todos modos?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    return;
            }

            try
            {
                string connString;
                if (value.IndexOf("postgresql://", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var uri = new Uri(value);
                    var userInfo = uri.UserInfo.Split(':');
                    var builder = new Npgsql.NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port > 0 ? uri.Port : 5432,
                        Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
                        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                        Database = uri.AbsolutePath.TrimStart('/'),
                        SslMode = Npgsql.SslMode.Require,
                        TrustServerCertificate = true,
                        Timeout = 5,
                        CommandTimeout = 5
                    };
                    connString = builder.ToString();
                }
                else if (value.IndexOf("Host=", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    connString = value;
                }
                else
                {
                    MessageBox.Show(this, "Formato no reconocido. Introduzca una URL postgresql://... o una cadena Npgsql.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var conn = new Npgsql.NpgsqlConnection(connString);
                await conn.OpenAsync();
                MessageBox.Show(this, $"Conexión correcta. Base de datos: {conn.Database ?? "(desconocida)"}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error al probar la conexión:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            var r = MessageBox.Show(this, "¿Desea eliminar la cadena guardada del almacén seguro?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes)
                return;

            try
            {
                SecretStoreService.DeleteSecret("DATABASE_URL");
                txtConnection.Text = string.Empty;
                MessageBox.Show(this, "Se eliminó la cadena guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No fue posible eliminar la cadena: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            SolicitarRegreso?.Invoke(this, EventArgs.Empty);
        }
    }
}
