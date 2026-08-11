using Npgsql;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public class FrmObservacionDetalle : Form
    {
        private readonly int _observacionId;
        private readonly int _idUsuarioWeb;
        private Label lblTitulo;
        private Label lblEstado;
        private Label lblFecha;
        private RichTextBox rtbMensajes;
        private Button btnCerrar;

        public FrmObservacionDetalle(int observacionId, int idUsuarioWeb)
        {
            _observacionId = observacionId;
            _idUsuarioWeb = idUsuarioWeb;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 560);

            lblTitulo = new Label { Text = "Observación", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(52,73,94), Location = new Point(16, 12), AutoSize = true };
            lblEstado = new Label { Text = "Estado:", Location = new Point(16, 46), AutoSize = true, ForeColor = Color.FromArgb(71,85,105) };
            lblFecha = new Label { Text = string.Empty, Location = new Point(200, 46), AutoSize = true, ForeColor = Color.FromArgb(127,140,141) };

            rtbMensajes = new RichTextBox
            {
                Location = new Point(16, 80),
                Size = new Size(ClientSize.Width - 32, ClientSize.Height - 140),
                ReadOnly = true,
                BackColor = Color.White
            };

            btnCerrar = new Button { Text = "Cerrar", Size = new Size(100, 30), Location = new Point(ClientSize.Width - 116, ClientSize.Height - 44), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCerrar.Click += (s, e) => Close();

            Controls.Add(lblTitulo);
            Controls.Add(lblEstado);
            Controls.Add(lblFecha);
            Controls.Add(rtbMensajes);
            Controls.Add(btnCerrar);

            Load += async (_, __) => await CargarObservacionAsync();
        }

        private async Task CargarObservacionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConfiguracionService.GetRailwayConnectionString());
                await conn.OpenAsync();

                const string queryObs = @"
                    SELECT o.descripcion, COALESCE(o.estado, o.status, '') AS estado, o.created_at
                    FROM observaciones o
                    WHERE o.id = @id;";

                await using (var cmd = new NpgsqlCommand(queryObs, conn))
                {
                    cmd.Parameters.AddWithValue("@id", _observacionId);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        string desc = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        string estado = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        DateTime fecha = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);

                        lblTitulo.Text = string.IsNullOrWhiteSpace(desc) ? "Observación" : desc;
                        lblEstado.Text = $"Estado: {estado}";
                        lblFecha.Text = fecha == DateTime.MinValue ? string.Empty : fecha.ToString("dd/MM/yyyy HH:mm");
                    }
                }

                // Cargar mensajes
                const string queryMsgs = @"
                    SELECT om.iduser,
                           COALESCE(u.alias, u.nombre_usuario, 'Usuario') AS nombre_usuario,
                           om.mensaje,
                           om.created_at
                    FROM observacion_mensajes om
                    LEFT JOIN usuarios u ON u.id = om.iduser
                    WHERE om.observacion_id = @id
                    ORDER BY om.created_at ASC, om.id ASC;";

                var mensajes = new List<(int idUser, string nombre, string texto, DateTime fecha)>();
                await using (var cmd = new NpgsqlCommand(queryMsgs, conn))
                {
                    cmd.Parameters.AddWithValue("@id", _observacionId);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int idu = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        string nomb = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        string msg = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        DateTime f = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                        mensajes.Add((idu, nomb, msg, f));
                    }
                }

                RenderizarMensajes(mensajes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error al cargar la observación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderizarMensajes(List<(int idUser, string nombre, string texto, DateTime fecha)> mensajes)
        {
            rtbMensajes.Clear();
            var fontUser = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            var fontMsg = new Font("Segoe UI", 9F, FontStyle.Regular);

            foreach (var m in mensajes)
            {
                bool esMio = m.idUser == _idUsuarioWeb;
                rtbMensajes.SelectionFont = fontUser;
                rtbMensajes.SelectionColor = esMio ? Color.FromArgb(37, 99, 235) : Color.FromArgb(22, 163, 74);
                string nombre = esMio ? ("Tú") : (string.IsNullOrWhiteSpace(m.nombre) ? "Usuario" : m.nombre);
                rtbMensajes.AppendText($"{nombre} · {m.fecha:dd/MM/yyyy HH:mm}\r\n");
                rtbMensajes.SelectionFont = fontMsg;
                rtbMensajes.SelectionColor = Color.FromArgb(51, 65, 85);
                rtbMensajes.AppendText(m.texto + "\r\n\r\n");
            }
            if (mensajes.Count == 0)
            {
                rtbMensajes.AppendText("No hay mensajes en esta observación.");
            }
        }
    }
}
