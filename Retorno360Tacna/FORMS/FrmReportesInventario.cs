using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
    using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SkiaSharp;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportesInventario : Form
    {
        // Fuentes reutilizables para el panel de mensajes (evita fugas de handles GDI)

        private bool _inicializandoCombo = false;
        private bool _inicializandoComboAnio = false;
        private bool _filtroAnioManual = false;
        private int _versionCargaCarpetas = 0;
        private readonly Stack<string> _historialCarpetas = new();
        private readonly List<Label> _etiquetasAniosGrafica = new();
        private readonly List<(int Anio, int Inicio, int Fin)> _rangosAniosGrafica = new();
        private readonly List<ArchivoObservadoItem> _archivosObservados = new();
        private const int WM_USER = 0x0400;
        private const int EM_SETPARAFORMAT = WM_USER + 71;
        private const int SCF_SELECTION = 0x0001;
        private const uint PFM_ALIGNMENT = 0x00000008;
        private const short PFA_LEFT = 1;
        private const short PFA_RIGHT = 2;
        private string currentCloudflarePrefix = string.Empty;
        private int? _anioSeleccionado;
        private string _archivoPendienteSeleccion = string.Empty;
        private ArchivoObservadoItem? _archivoObservadoSeleccionado;
        private int? _observacionActivaId;
        private string _rutaArchivoMensajesActiva = string.Empty;
        private string _ultimaFirmaMensajes = string.Empty;
        private bool _actualizandoMensajes;
        private readonly LiveChartsCore.SkiaSharpView.WinForms.CartesianChart chartCargaMensual;
        private readonly DataGridView dgvEstadoMensual;
        private readonly Label lblEstadoMensual;
        private readonly System.Windows.Forms.Timer timerMensajesObservacion;
        private readonly Font _fontNombreUsuario = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font _fontFecha = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        private readonly Font _fontMensaje = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        private static string ConexionPortalWeb => ConfiguracionService.GetRailwayConnectionString();
        private const string BucketStorageBaseUrl = "https://b73e0f75a96164dc3862e335762c3ef6.r2.cloudflarestorage.com/retorno360web";
        private readonly Usuario? usuarioActual;

        private readonly ConexionInfo conexionActual;
        private CatalogoPartesService catalogoService;
        // Eliminada la referencia a la carpeta local
        private CloudflareR2Service cloudflareService = new CloudflareR2Service("retorno360web");




        public FrmReportesInventario(ConexionInfo conexion, Usuario? usuario = null)
        {
            InitializeComponent();
            conexionActual = conexion;
            usuarioActual = usuario;
            catalogoService = new CatalogoPartesService(conexion);
            DataGridViewManualCopyHelper.ConfigurarControles(this);
            chartCargaMensual = CrearGraficaCargaMensual();
            lblEstadoMensual = CrearEtiquetaEstadoMensual();
            dgvEstadoMensual = CrearGridEstadoMensual();
            panelContenido.Controls.Add(chartCargaMensual);
            panelContenido.Controls.Add(lblEstadoMensual);
            panelContenido.Controls.Add(dgvEstadoMensual);
            timerMensajesObservacion = new System.Windows.Forms.Timer { Interval = 5000 };
            timerMensajesObservacion.Tick += async (_, _) => await ActualizarMensajesObservacionAsync();
            panelContenido.Resize += (_, _) => AjustarLayoutVistaInventario();
            lvCarpetas.SelectedIndexChanged += lvCarpetas_SelectedIndexChanged;
            ConfigurarEstiloVisual();
            LimpiarPanelMensajes();
            LimpiarEstadoMensual();
            AjustarLayoutVistaInventario();

            // El botón de configuración de BD se ha movido al formulario de configuración (Conexiones)
        }

        // BtnDbConfig removed: moved to FrmConfiguracion -> 'Conexiones'

        private void lvCarpetas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ActualizarBotonAgregarObservacion();
            _ = ActualizarContextoMensajesDesdeSeleccionAsync();
        }

        private void ActualizarBotonAgregarObservacion()
        {
            bool habilitado = lvCarpetas.SelectedItems.Count > 0
                && lvCarpetas.SelectedItems[0].ImageKey == "file"
                && lvCarpetas.SelectedItems[0].Tag is string;

            btnAgregarObservacion.Enabled = habilitado;
            btnAgregarObservacion.BackColor = habilitado
                ? System.Drawing.Color.FromArgb(142, 68, 173)
                : System.Drawing.Color.FromArgb(189, 195, 199);

            _archivoSeleccionadoValido = habilitado;
            BtmEliminarArchivo.Enabled = habilitado;

            if (habilitado && lvCarpetas.SelectedItems[0].Tag is string rutaArchivo)
            {
                lblRutaActual.Text = $"Ruta actual: {ConstruirRutaVisible(rutaArchivo)}";
            }
        }



        private async void btnAgregarObservacion_Click(object sender, EventArgs e)
        {
            if (lvCarpetas.SelectedItems.Count == 0 || lvCarpetas.SelectedItems[0].ImageKey != "file")
            {
                MessageBox.Show("Seleccione un archivo para agregar una observación.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usuarioActual == null)
            {
                MessageBox.Show("No se pudo identificar el usuario actual.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!usuarioActual.IdWeb.HasValue || usuarioActual.IdWeb.Value <= 0)
            {
                MessageBox.Show("El usuario actual no tiene un usuario web vinculado en configuración.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lvCarpetas.SelectedItems[0].Tag is not string rutaArchivoR2)
                return;

            string nombreArchivo = Path.GetFileName(rutaArchivoR2);
            string descripcion = Microsoft.VisualBasic.Interaction.InputBox(
                $"Escriba la observación para el archivo:\n{nombreArchivo}",
                "Agregar observación",
                string.Empty);

            if (string.IsNullOrWhiteSpace(descripcion))
                return;

            try
            {
                btnAgregarObservacion.Enabled = false;

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                bool usuarioWebExiste = await ValidarUsuarioWebAsync(conexionPortal, usuarioActual.IdWeb.Value);
                if (!usuarioWebExiste)
                {
                    MessageBox.Show("El idWeb vinculado no existe en la tabla usuarios del portal web.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int? idArchivo = await ObtenerIdArchivoHistorialAsync(conexionPortal, rutaArchivoR2);
                if (!idArchivo.HasValue)
                {
                    MessageBox.Show($"No se encontró el archivo '{nombreArchivo}' en la tabla archivos_historial.", "Archivo no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await using var transaccion = await conexionPortal.BeginTransactionAsync();

                int idUsuarioWeb = usuarioActual.IdWeb.Value;
                int idObservacion = await InsertarObservacionAsync(conexionPortal, transaccion, descripcion.Trim(), idUsuarioWeb, idArchivo.Value);
                await InsertarObservacionMensajeAsync(conexionPortal, transaccion, idObservacion, idUsuarioWeb, descripcion.Trim());

                await transaccion.CommitAsync();

                _observacionActivaId = idObservacion;
                _rutaArchivoMensajesActiva = rutaArchivoR2;
                await ActualizarMensajesObservacionAsync();
                timerMensajesObservacion.Start();

                MessageBox.Show("Observación registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al registrar la observación: {ex.Message}", "Error", ex, "Registro de observación desde Reportes de Inventario");
            }
            finally
            {
                ActualizarBotonAgregarObservacion();
            }
        }

        private static async Task<int?> ObtenerIdArchivoHistorialAsync(NpgsqlConnection conexionPortal, string rutaArchivoR2)
        {
            string storageKey = rutaArchivoR2.Replace('\\', '/').Trim('/');
            string nombreAlmacenado = Path.GetFileName(storageKey);
            string storageUrl = $"{BucketStorageBaseUrl}/{storageKey}";

            const string query = @"
                SELECT id
                FROM archivos_historial
                WHERE storage_key = @storageKey
                   OR nombre_almacenado = @nombreAlmacenado
                   OR storage_url = @storageUrl
                ORDER BY uploaded_at DESC NULLS LAST, id DESC
                LIMIT 1;";

            await using var command = new NpgsqlCommand(query, conexionPortal);
            command.Parameters.AddWithValue("@storageKey", storageKey);
            command.Parameters.AddWithValue("@nombreAlmacenado", nombreAlmacenado);
            command.Parameters.AddWithValue("@storageUrl", storageUrl);
            object? resultado = await command.ExecuteScalarAsync();

            if (resultado == null || resultado == DBNull.Value)
                return null;

            return Convert.ToInt32(resultado);
        }

        private static async Task<bool> ValidarUsuarioWebAsync(NpgsqlConnection conexionPortal, int idUsuarioWeb)
        {
            const string query = @"
                SELECT 1
                FROM usuarios
                WHERE id = @idUsuarioWeb
                LIMIT 1;";

            await using var command = new NpgsqlCommand(query, conexionPortal);
            command.Parameters.AddWithValue("@idUsuarioWeb", idUsuarioWeb);
            object? resultado = await command.ExecuteScalarAsync();
            return resultado != null && resultado != DBNull.Value;
        }

        private static async Task<int?> ObtenerObservacionActivaAsync(NpgsqlConnection conexionPortal, int idArchivo, int idUsuarioWeb)
        {
            const string query = @"
                SELECT id
                FROM observaciones
                WHERE idarchivo = @idArchivo
                  AND iduser = @idUsuarioWeb
                ORDER BY created_at DESC, id DESC
                LIMIT 1;";

            await using var command = new NpgsqlCommand(query, conexionPortal);
            command.Parameters.AddWithValue("@idArchivo", idArchivo);
            command.Parameters.AddWithValue("@idUsuarioWeb", idUsuarioWeb);
            object? resultado = await command.ExecuteScalarAsync();
            if (resultado == null || resultado == DBNull.Value)
                return null;

            return Convert.ToInt32(resultado);
        }

        private sealed class MensajeObservacionItem
        {
            public int IdUsuario { get; set; }
            public string NombreUsuario { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PARAFORMAT
        {
            public uint cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref PARAFORMAT lParam);

        private async Task ActualizarContextoMensajesDesdeSeleccionAsync()
        {
            if (usuarioActual?.IdWeb is not int idUsuarioWeb || idUsuarioWeb <= 0)
            {
                LimpiarPanelMensajes("El usuario actual no tiene idWeb vinculado.");
                return;
            }

            if (lvCarpetas.SelectedItems.Count == 0 || lvCarpetas.SelectedItems[0].ImageKey != "file" || lvCarpetas.SelectedItems[0].Tag is not string rutaArchivoR2)
            {
                timerMensajesObservacion.Stop();
                _observacionActivaId = null;
                _rutaArchivoMensajesActiva = string.Empty;
                LimpiarPanelMensajes();
                return;
            }

            if (FormNoDisponible()) return;

            try
            {
                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                int? idArchivo = await ObtenerIdArchivoHistorialAsync(conexionPortal, rutaArchivoR2);
                if (!idArchivo.HasValue)
                {
                    _observacionActivaId = null;
                    _rutaArchivoMensajesActiva = rutaArchivoR2;
                    LimpiarPanelMensajes("No se encontró historial del archivo para consultar mensajes.");
                    return;
                }

                _observacionActivaId = await ObtenerObservacionActivaAsync(conexionPortal, idArchivo.Value, idUsuarioWeb);
                _rutaArchivoMensajesActiva = rutaArchivoR2;

                if (_observacionActivaId.HasValue)
                {
                    await ActualizarMensajesObservacionAsync();
                    timerMensajesObservacion.Start();
                }
                else
                {
                    timerMensajesObservacion.Stop();
                    LimpiarPanelMensajes("Aún no hay conversación para este archivo.");
                }
            }
            catch (Exception ex)
            {
                timerMensajesObservacion.Stop();
                LimpiarPanelMensajes("No fue posible cargar los mensajes.");
                ErrorLogger.LogError(ex, "Carga de mensajes de observación en Reportes de Inventario");
            }

            if (FormNoDisponible()) return;
        }

        private async Task ActualizarMensajesObservacionAsync()
        {
            if (_actualizandoMensajes || !_observacionActivaId.HasValue || usuarioActual?.IdWeb is not int idUsuarioWeb || idUsuarioWeb <= 0)
                return;

            try
            {
                _actualizandoMensajes = true;

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                if (FormNoDisponible()) return;

                const string query = @"
                    SELECT om.iduser,
                           COALESCE(u.alias, u.nombre_usuario, 'Usuario') AS nombre_usuario,
                           om.mensaje,
                           om.created_at
                    FROM observacion_mensajes om
                    INNER JOIN observaciones o ON o.id = om.observacion_id
                    LEFT JOIN usuarios u ON u.id = om.iduser
                    WHERE om.observacion_id = @observacionId
                      AND COALESCE(o.estado, '') = 'en_revision'
                    ORDER BY om.created_at ASC, om.id ASC;";

                await using var command = new NpgsqlCommand(query, conexionPortal);
                command.Parameters.AddWithValue("@observacionId", _observacionActivaId.Value);

                var mensajes = new List<MensajeObservacionItem>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    mensajes.Add(new MensajeObservacionItem
                    {
                        IdUsuario = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        NombreUsuario = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Mensaje = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Fecha = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3)
                    });
                }

                if (FormNoDisponible()) return;

                if (mensajes.Count == 0)
                {
                    _observacionActivaId = null;
                    timerMensajesObservacion.Stop();
                    LimpiarPanelMensajes("La observación ya no está en revisión.");
                    return;
                }

                RenderizarMensajesObservacion(mensajes, idUsuarioWeb);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Actualización en vivo de mensajes de observación");
            }
            finally
            {
                _actualizandoMensajes = false;
            }
        }

        private void RenderizarMensajesObservacion(List<MensajeObservacionItem> mensajes, int idUsuarioWeb)
        {
            string firmaMensajes = string.Join("|", mensajes.Select(m => $"{m.IdUsuario}:{m.NombreUsuario}:{m.Fecha:O}:{m.Mensaje}"));

            if (mensajes.Count == 0)
            {
                lblMensajesEstado.Text = "No hay mensajes disponibles en esta observación.";
                _ultimaFirmaMensajes = string.Empty;
                rtbMensajesObservacion.Clear();
                return;
            }

            lblMensajesEstado.Text = "Conversación activa · actualización automática";

            if (string.Equals(_ultimaFirmaMensajes, firmaMensajes, StringComparison.Ordinal))
                return;

            _ultimaFirmaMensajes = firmaMensajes;

            rtbMensajesObservacion.SuspendLayout();
            rtbMensajesObservacion.Clear();

            foreach (var mensaje in mensajes)
            {
                bool esMio = mensaje.IdUsuario == idUsuarioWeb;
                EstablecerAlineacionMensaje(esMio ? PFA_RIGHT : PFA_LEFT);

                rtbMensajesObservacion.SelectionFont = _fontNombreUsuario;
                rtbMensajesObservacion.SelectionColor = esMio
                    ? Color.FromArgb(37, 99, 235)
                    : Color.FromArgb(22, 163, 74);
                string nombreMostrado = esMio
                    ? (string.IsNullOrWhiteSpace(usuarioActual?.NombreCompleto) ? "Tú" : usuarioActual.NombreCompleto)
                    : (string.IsNullOrWhiteSpace(mensaje.NombreUsuario) ? "Respuesta portal" : mensaje.NombreUsuario);
                rtbMensajesObservacion.AppendText(nombreMostrado);

                rtbMensajesObservacion.SelectionFont = _fontFecha;
                rtbMensajesObservacion.SelectionColor = Color.FromArgb(100, 116, 139);
                rtbMensajesObservacion.AppendText($" · {mensaje.Fecha:dd/MM/yyyy HH:mm}\n");

                rtbMensajesObservacion.SelectionFont = _fontMensaje;
                rtbMensajesObservacion.SelectionColor = Color.FromArgb(30, 41, 59);
                rtbMensajesObservacion.AppendText($"{mensaje.Mensaje}\n\n");
            }

            EstablecerAlineacionMensaje(PFA_LEFT);
            rtbMensajesObservacion.SelectionStart = rtbMensajesObservacion.TextLength;
            rtbMensajesObservacion.ScrollToCaret();
            rtbMensajesObservacion.ResumeLayout();
            btnEnviarMensajeObservacion.Enabled = _observacionActivaId.HasValue;
            txtMensajeObservacion.Enabled = _observacionActivaId.HasValue;
        }

        private void EstablecerAlineacionMensaje(short alineacion)
        {
            PARAFORMAT formato = new PARAFORMAT
            {
                cbSize = (uint)Marshal.SizeOf<PARAFORMAT>(),
                dwMask = PFM_ALIGNMENT,
                wAlignment = alineacion,
                rgxTabs = new int[32]
            };

            SendMessage(rtbMensajesObservacion.Handle, EM_SETPARAFORMAT, (IntPtr)SCF_SELECTION, ref formato);
        }

        private void LimpiarPanelMensajes(string mensajeEstado = "Seleccione un archivo para ver mensajes")
        {
            lblMensajesEstado.Text = mensajeEstado;
            _ultimaFirmaMensajes = string.Empty;
            rtbMensajesObservacion.Clear();
            txtMensajeObservacion.Clear();
            btnEnviarMensajeObservacion.Enabled = _observacionActivaId.HasValue;
            txtMensajeObservacion.Enabled = _observacionActivaId.HasValue;

            if (!_observacionActivaId.HasValue && string.IsNullOrWhiteSpace(mensajeEstado))
            {
                lblMensajesEstado.Text = "Seleccione un archivo desde la izquierda y luego una observación para continuar.";
            }
        }

        private sealed class ArchivoObservadoItem
        {
            public int ObservacionId { get; set; }
            public string Archivo { get; set; } = string.Empty;
            public string RazonSocial { get; set; } = string.Empty;
            public string Empresa { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
            public string StorageKey { get; set; } = string.Empty;
        }

        private sealed class SolicitudEliminacionItem
        {
            public string Archivo { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string Motivo { get; set; } = string.Empty;
            public string Empresa { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
        }

        private async Task CargarArchivosConObservacionesAsync()
        {
            try
            {
                lblArchivosObservadosEstado.Text = "Cargando archivos con observaciones...";

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                const string query = @"
                    SELECT
                        o.id,
                        ah.nombre_almacenado,
                        COALESCE(rs.nombre, 'Sin razón social') AS razon_social,
                        COALESCE(e.nombre, 'Sin empresa') AS empresa,
                        COALESCE(o.estado, o.status, 'sin_estado') AS estado,
                        ah.storage_key,
                        o.created_at
                    FROM observaciones o
                    INNER JOIN archivos_historial ah ON ah.id = o.idarchivo
                    LEFT JOIN razon_social rs ON rs.id = ah.razon_social_id
                    LEFT JOIN empresa e ON e.id = ah.empresa_id
                    WHERE COALESCE(o.estado, o.status, '') = 'en_revision' OR  COALESCE(o.estado, o.status, '') = 'abierto' 
                    ORDER BY o.created_at DESC, o.id DESC;";

                await using var command = new NpgsqlCommand(query, conexionPortal);
                _archivosObservados.Clear();

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    DateTime fecha = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6);
                    _archivosObservados.Add(new ArchivoObservadoItem
                    {
                        ObservacionId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Archivo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        RazonSocial = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Empresa = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Estado = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        StorageKey = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        Fecha = fecha == DateTime.MinValue ? string.Empty : fecha.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                dgvArchivosObservados.Columns.Clear();
                dgvArchivosObservados.DataSource = null;

                if (_archivosObservados.Count == 0)
                {
                    lblArchivosObservadosEstado.Text = "No se encontraron archivos con observaciones.";
                    return;
                }

                dgvArchivosObservados.DataSource = _archivosObservados.ToList();
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.ObservacionId)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.ObservacionId)].Visible = false;
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.StorageKey)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.StorageKey)].Visible = false;

                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.Archivo)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.Archivo)].HeaderText = "Archivo";
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.RazonSocial)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.RazonSocial)].HeaderText = "Razón social";
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.Empresa)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.Empresa)].HeaderText = "Empresa";
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.Estado)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.Estado)].HeaderText = "Estado";
                if (dgvArchivosObservados.Columns.Contains(nameof(ArchivoObservadoItem.Fecha)))
                    dgvArchivosObservados.Columns[nameof(ArchivoObservadoItem.Fecha)].HeaderText = "Fecha";

                lblArchivosObservadosEstado.Text = $"Total de archivos observados: {_archivosObservados.Count}. Haz clic para abrir la conversación.";
            }
            catch (Exception ex)
            {
                lblArchivosObservadosEstado.Text = "No fue posible cargar los archivos observados.";
                ErrorLogger.LogError(ex, "Carga de archivos con observaciones en Reportes de Inventario");
            }
            finally
            {
                _archivoObservadoSeleccionado = null;
                ActualizarBotonesArchivosObservados();
            }
        }

        private async Task CargarSolicitudesEliminacionAsync()
        {
            try
            {
                lblSolicitudesEliminacionEstado.Text = "Cargando solicitudes de eliminación...";

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                const string query = @"
                   SELECT
                        COALESCE(ah.nombre_archivo, ah.storage_key, 'Archivo') AS archivo,
                        COALESCE(adr.estado, 'sin_estado') AS estado,
                        COALESCE(adr.motivo, 'Sin motivo') AS motivo,
                        COALESCE(e.nombre, 'Sin empresa') AS empresa,
                        COALESCE(u.alias, u.nombre_usuario, 'Usuario no identificado') AS usuario,
                        adr.solicitado_at
                    FROM archivo_delete_requests as adr
                    LEFT JOIN archivos_historial ah ON ah.id = adr.archivo_id
                    LEFT JOIN empresa e ON e.id = ah.empresa_id
                    LEFT JOIN usuarios u ON u.id = adr.solicitado_por
                    WHERE COALESCE(adr.estado, '') = 'en_proceso'
                    ORDER BY adr.solicitado_at DESC, adr.id DESC;
";

                await using var command = new NpgsqlCommand(query, conexionPortal);
                var solicitudes = new List<SolicitudEliminacionItem>();

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    DateTime fecha = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5);
                    solicitudes.Add(new SolicitudEliminacionItem
                    {
                        Archivo = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Estado = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Motivo = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Empresa = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Usuario = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        Fecha = fecha == DateTime.MinValue ? string.Empty : fecha.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                dgvSolicitudesEliminacion.Columns.Clear();
                dgvSolicitudesEliminacion.DataSource = null;

                if (solicitudes.Count == 0)
                {
                    lblSolicitudesEliminacionEstado.Text = "No hay solicitudes de eliminación en proceso.";
                    return;
                }

                dgvSolicitudesEliminacion.DataSource = solicitudes;
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Archivo)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Archivo)].HeaderText = "Archivo";
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Estado)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Estado)].HeaderText = "Estado";
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Motivo)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Motivo)].HeaderText = "Motivo";
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Fecha)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Fecha)].HeaderText = "Fecha";
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Empresa)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Empresa)].Visible = false;
                if (dgvSolicitudesEliminacion.Columns.Contains(nameof(SolicitudEliminacionItem.Usuario)))
                    dgvSolicitudesEliminacion.Columns[nameof(SolicitudEliminacionItem.Usuario)].Visible = false;

                lblSolicitudesEliminacionEstado.Text = $"Total de solicitudes: {solicitudes.Count}";
            }
            catch (Exception ex)
            {
                lblSolicitudesEliminacionEstado.Text = "No fue posible cargar las solicitudes de eliminación.";
                ErrorLogger.LogError(ex, "Carga de solicitudes de eliminación en Reportes de Inventario");
            }
        }

        private void dgvArchivosObservados_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvArchivosObservados.Rows.Count)
                return;

            if (dgvArchivosObservados.Rows[e.RowIndex].DataBoundItem is not ArchivoObservadoItem archivoObservado || string.IsNullOrWhiteSpace(archivoObservado.StorageKey))
                return;

            _archivoObservadoSeleccionado = archivoObservado;
            ActualizarBotonesArchivosObservados();
            string direccion = ObtenerDirectorioDesdeClaveArchivo(archivoObservado.StorageKey);
            NavegarADetalleNotificacion(direccion, archivoObservado.StorageKey);
            _observacionActivaId = archivoObservado.ObservacionId;
            lblMensajesEstado.Text = $"Abriendo conversación para {archivoObservado.Archivo}...";
        }


        //esta seccion se encarga de abrir la ventana de solicitud de eliminacion de archivos
        private void dgvSolicitudesEliminacion_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvSolicitudesEliminacion.Rows.Count)
                return;

            if (dgvSolicitudesEliminacion.Rows[e.RowIndex].DataBoundItem is not SolicitudEliminacionItem solicitud)
                return;

            using var frmSolicitud = new FrmSolicitud(
                solicitud.Archivo,
                solicitud.Estado,
                solicitud.Motivo,
                solicitud.Empresa,
                solicitud.Usuario,
                solicitud.Fecha);

            frmSolicitud.StartPosition = FormStartPosition.CenterParent;
            frmSolicitud.ShowDialog(this);
        }

        private void ActualizarBotonesArchivosObservados()
        {
            bool habilitado = _archivoObservadoSeleccionado != null && _archivoObservadoSeleccionado.ObservacionId > 0;
            btnCerrarObservacion.Enabled = habilitado;
            btnEliminarArchivoObservado.Enabled = habilitado;
        }

        private async void btnCerrarObservacion_Click(object sender, EventArgs e)
        {
            if (_archivoObservadoSeleccionado == null)
                return;

            DialogResult confirmacion = MessageBox.Show(
                $"¿Desea cerrar la observación del archivo '{_archivoObservadoSeleccionado.Archivo}'?",
                "Cerrar observación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();
                await CerrarObservacionAsync(conexionPortal, _archivoObservadoSeleccionado.ObservacionId);

                if (_observacionActivaId == _archivoObservadoSeleccionado.ObservacionId)
                {
                    _observacionActivaId = null;
                    timerMensajesObservacion.Stop();
                    LimpiarPanelMensajes("Observación cerrada correctamente.");
                }

                await CargarArchivosConObservacionesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cerrar la observación: {ex.Message}", "Error", ex, "Cierre de observación desde Reportes de Inventario");
            }
        }

        private async void btnEliminarArchivoObservado_Click(object sender, EventArgs e)
        {
            if (_archivoObservadoSeleccionado == null || string.IsNullOrWhiteSpace(_archivoObservadoSeleccionado.StorageKey))
                return;

            DialogResult confirmacion = MessageBox.Show(
                $"¿Desea eliminar el archivo '{_archivoObservadoSeleccionado.Archivo}' del bucket y cerrar la observación?",
                "Eliminar archivo en revisión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                await cloudflareService.DeleteFileAsync(_archivoObservadoSeleccionado.StorageKey);

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();
                await CerrarObservacionAsync(conexionPortal, _archivoObservadoSeleccionado.ObservacionId);

                if (string.Equals(_rutaArchivoMensajesActiva, _archivoObservadoSeleccionado.StorageKey, StringComparison.OrdinalIgnoreCase))
                {
                    _observacionActivaId = null;
                    _rutaArchivoMensajesActiva = string.Empty;
                    timerMensajesObservacion.Stop();
                    LimpiarPanelMensajes("Archivo eliminado y observación cerrada.");
                }

                await CargarArchivosConObservacionesAsync();
                btnActualizar_Click(btnActualizar, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al eliminar el archivo y cerrar la observación: {ex.Message}", "Error", ex, "Eliminación de archivo en revisión desde Reportes de Inventario");
            }
        }

        private static string ObtenerDirectorioDesdeClaveArchivo(string storageKey)
        {
            var segmentos = ObtenerSegmentosRutaArchivo(storageKey);
            if (segmentos.Length <= 1)
                return string.Empty;

            return string.Join("/", segmentos.Take(segmentos.Length - 1));
        }

        private async void btnEnviarMensajeObservacion_Click(object sender, EventArgs e)
        {
            if (!_observacionActivaId.HasValue || usuarioActual?.IdWeb is not int idUsuarioWeb || idUsuarioWeb <= 0)
            {
                MessageBox.Show("No hay una observación activa para responder.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mensaje = txtMensajeObservacion.Text.Trim();
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                MessageBox.Show("Escriba un mensaje antes de enviarlo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMensajeObservacion.Focus();
                return;
            }

            try
            {
                btnEnviarMensajeObservacion.Enabled = false;

                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();
                await InsertarMensajeObservacionAsync(conexionPortal, _observacionActivaId.Value, idUsuarioWeb, mensaje);

                txtMensajeObservacion.Clear();
                await ActualizarMensajesObservacionAsync();
                await CargarArchivosConObservacionesAsync();
                timerMensajesObservacion.Start();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al enviar el mensaje: {ex.Message}", "Error", ex, "Envío de mensaje de observación desde Reportes de Inventario");
            }
            finally
            {
                btnEnviarMensajeObservacion.Enabled = _observacionActivaId.HasValue;
            }
        }

        private static async Task<int> InsertarObservacionAsync(NpgsqlConnection conexionPortal, NpgsqlTransaction transaccion, string descripcion, int idUsuario, int idArchivo)
        {
            const string query = @"
                INSERT INTO observaciones (descripcion, iduser, idarchivo, created_at, status, estado, cliente_participante_id)
                VALUES (@descripcion, @iduser, @idarchivo, NOW(), @status, @estado, @clienteParticipanteId)
                RETURNING id;";

            await using var command = new NpgsqlCommand(query, conexionPortal, transaccion);
            command.Parameters.AddWithValue("@descripcion", descripcion);
            command.Parameters.AddWithValue("@iduser", idUsuario);
            command.Parameters.AddWithValue("@idarchivo", idArchivo);
            command.Parameters.AddWithValue("@status", DBNull.Value);// este campo siempre esta en nulo, debido a que ni el portal web lo toma en cuenta
            command.Parameters.AddWithValue("@estado", "en_revision");
            command.Parameters.AddWithValue("@clienteParticipanteId", DBNull.Value);

            object? resultado = await command.ExecuteScalarAsync();
            return Convert.ToInt32(resultado);
        }

        private static async Task InsertarObservacionMensajeAsync(NpgsqlConnection conexionPortal, NpgsqlTransaction transaccion, int idObservacion, int idUsuario, string mensaje)
        {
            const string query = @"
                INSERT INTO observacion_mensajes (observacion_id, iduser, mensaje, created_at)
                VALUES (@observacionId, @iduser, @mensaje, NOW());";

            await using var command = new NpgsqlCommand(query, conexionPortal, transaccion);
            command.Parameters.AddWithValue("@observacionId", idObservacion);
            command.Parameters.AddWithValue("@iduser", idUsuario);
            command.Parameters.AddWithValue("@mensaje", mensaje);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CerrarObservacionAsync(NpgsqlConnection conexionPortal, int idObservacion)
        {
            const string query = @"
                UPDATE observaciones
                SET estado = 'cerrado'
                WHERE id = @idObservacion;";

            await using var command = new NpgsqlCommand(query, conexionPortal);
            command.Parameters.AddWithValue("@idObservacion", idObservacion);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertarMensajeObservacionAsync(NpgsqlConnection conexionPortal, int idObservacion, int idUsuario, string mensaje)
        {
            const string query = @"
                INSERT INTO observacion_mensajes (observacion_id, iduser, mensaje, created_at)
                VALUES (@observacionId, @iduser, @mensaje, NOW());";

            await using var command = new NpgsqlCommand(query, conexionPortal);
            command.Parameters.AddWithValue("@observacionId", idObservacion);
            command.Parameters.AddWithValue("@iduser", idUsuario);
            command.Parameters.AddWithValue("@mensaje", mensaje);
            await command.ExecuteNonQueryAsync();
        }

        private LiveChartsCore.SkiaSharpView.WinForms.CartesianChart CrearGraficaCargaMensual()
        {
            return new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart
            {
                Name = "chartCargaMensual",
                BackColor = System.Drawing.Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Visible = true
            };
        }

        private void LimpiarGraficaCargaMensual(string titulo = "Ingrese a una carpeta de empresa con archivos")
        {
            LimpiarEtiquetasAniosGrafica();
            chartCargaMensual.Series = Array.Empty<ISeries>();
            chartCargaMensual.XAxes = new[] { new Axis { Labels = Array.Empty<string>() } };
            chartCargaMensual.YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 1 } };
            chartCargaMensual.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = titulo,
                TextSize = 14,
                Padding = new LiveChartsCore.Drawing.Padding(10),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };
            chartCargaMensual.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        }

        private void LimpiarEtiquetasAniosGrafica()
        {
            foreach (var etiqueta in _etiquetasAniosGrafica)
            {
                panelContenido.Controls.Remove(etiqueta);
                etiqueta.Dispose();
            }

            _etiquetasAniosGrafica.Clear();
            _rangosAniosGrafica.Clear();
        }

        private void ActualizarEtiquetasAniosGrafica(List<(int Anio, int Inicio, int Fin)> rangos)
        {
            LimpiarEtiquetasAniosGrafica();

            if (_anioSeleccionado.HasValue || rangos.Count <= 1)
                return;

            _rangosAniosGrafica.AddRange(rangos);

            foreach (var rango in rangos)
            {
                var etiqueta = new Label
                {
                    AutoSize = false,
                    Height = 26,
                    Width = 110,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = System.Drawing.Color.FromArgb(255, 255, 255),
                    ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Text = rango.Anio.ToString()
                };

                _etiquetasAniosGrafica.Add(etiqueta);
                panelContenido.Controls.Add(etiqueta);
                etiqueta.BringToFront();
            }

            ReposicionarEtiquetasAniosGrafica();
        }

        private void ReposicionarEtiquetasAniosGrafica()
        {
            if (_etiquetasAniosGrafica.Count == 0 || _rangosAniosGrafica.Count == 0)
                return;

            int totalSlots = _rangosAniosGrafica.Max(x => x.Fin) + 1;
            if (totalSlots <= 0 || chartCargaMensual.Width <= 0)
                return;

            int totalBarras = _rangosAniosGrafica.Sum(x => x.Fin - x.Inicio + 1);
            if (totalBarras <= 0)
                return;

            double anchoSlot = (double)chartCargaMensual.Width / totalSlots;
            double factorBarras = (double)totalSlots / totalBarras;

            for (int i = 0; i < _etiquetasAniosGrafica.Count; i++)
            {
                var etiqueta = _etiquetasAniosGrafica[i];
                var rango = _rangosAniosGrafica[i];
                double centro = ((rango.Inicio + rango.Fin) / 2d) + 0.5d;
                int x = chartCargaMensual.Left + (int)Math.Round((centro * anchoSlot * factorBarras)) - (etiqueta.Width / 2);
                int y = chartCargaMensual.Top + 34;

                etiqueta.Location = new Point(
                    Math.Min(Math.Max(chartCargaMensual.Left, x), chartCargaMensual.Right - etiqueta.Width),
                    y);
            }
        }

        private void ActualizarVistaResumenMensual(IEnumerable<string> archivos)
        {
            try
            {
                var archivosLista = archivos
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                var aniosDisponibles = archivosLista
                    .Select(Path.GetFileName)
                    .Select(IntentarObtenerPeriodoArchivo)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value.Anio)
                    .ToList();

                ActualizarComboAnios(aniosDisponibles);

                var periodosDetectados = archivosLista
                    .Select(Path.GetFileName)
                    .Select(IntentarObtenerPeriodoArchivo)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .GroupBy(x => new { x.Anio, x.Mes })
                    .Select(g => (g.Key.Anio, g.Key.Mes, TotalArchivos: g.Count()))
                    .OrderBy(x => x.Anio)
                    .ThenBy(x => x.Mes)
                    .ToList();

                ConfigurarGraficaCargaMensual(periodosDetectados);
                ConfigurarEstadoMensual(archivosLista, ObtenerContextoEstadoMensual());
            }
            catch
            {
                ActualizarComboAnios(Array.Empty<int>());
                LimpiarGraficaCargaMensual("Control de carga mensual (sin archivos con fecha válida)");
                LimpiarEstadoMensual("Semáforo mensual de cargas (sin archivos con fecha válida)");
            }
        }

        private async Task<List<string>> ObtenerArchivosParaResumenAsync(string prefix)
        {
            var archivos = new List<string>();
            string prefijoNormalizado = (prefix ?? string.Empty).Replace('\\', '/').Trim('/');
            string prefijoConsulta = string.IsNullOrWhiteSpace(prefijoNormalizado) ? string.Empty : prefijoNormalizado + "/";

            archivos.AddRange(await cloudflareService.ListFilesAsync(prefijoConsulta));

            var carpetas = await cloudflareService.ListFoldersAsync(prefijoNormalizado);

            foreach (var carpeta in carpetas)
            {
                string carpetaNormalizada = carpeta.Replace('\\', '/').Trim('/');
                archivos.AddRange(await ObtenerArchivosParaResumenAsync(carpetaNormalizada));
            }

            return archivos
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task CargarCarpetasCloudflareAsync(string prefix = "", bool registrarHistorial = true)
        {
            int versionCarga = ++_versionCargaCarpetas;
            string prefijoNormalizado = (prefix ?? string.Empty).Replace('\\', '/').Trim('/');

            if (registrarHistorial)
            {
                string prefijoActual = (currentCloudflarePrefix ?? string.Empty).Replace('\\', '/').Trim('/');
                if (!string.Equals(prefijoActual, prefijoNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    _historialCarpetas.Push(prefijoActual);
                }
            }

            currentCloudflarePrefix = prefijoNormalizado;
            ActualizarBotonRegresar();
            ActualizarRutaActual();

            var carpetas = await cloudflareService.ListFoldersAsync(prefijoNormalizado);

            if (versionCarga != _versionCargaCarpetas)
                return;

            lvCarpetas.Items.Clear();
            bool mostrarArchivos = false;

            if (string.IsNullOrEmpty(prefijoNormalizado))
            {
                var nombresUnicos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var carpeta in carpetas)
                {
                    var rutaNormalizada = carpeta.Replace('\\', '/').Trim('/');
                    var segmentos = rutaNormalizada.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var nombre = segmentos.Length > 0 ? segmentos[0] : string.Empty;

                    if (!string.IsNullOrWhiteSpace(nombre) && !nombresUnicos.Contains(nombre))
                    {
                        nombresUnicos.Add(nombre);
                        var item = new ListViewItem(nombre, "folder")
                        {
                            Tag = nombre,
                            ToolTipText = nombre
                        };
                        lvCarpetas.Items.Add(item);
                    }
                }

                lblTotalCarpetas.Text = $"Total de carpetas encontradas: {nombresUnicos.Count}";
                _ = CargarResumenMensualAsync(prefijoNormalizado, versionCarga);
            }
            else
            {
                var subcarpetas = await cloudflareService.ListFoldersAsync(prefijoNormalizado + "/");

                if (versionCarga != _versionCargaCarpetas)
                    return;

                var nombresUnicos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var subcarpeta in subcarpetas)
                {
                    var nombre = Path.GetFileName(subcarpeta.TrimEnd('/', '\\'));
                    if (string.IsNullOrWhiteSpace(nombre) || !nombresUnicos.Add(nombre))
                        continue;

                    var item = new ListViewItem(nombre, "folder")
                    {
                        Tag = subcarpeta,
                        ToolTipText = subcarpeta
                    };
                    lvCarpetas.Items.Add(item);
                }

                if (lvCarpetas.Items.Count == 0)
                {
                    mostrarArchivos = true;
                }
                else
                {
                    lblTotalCarpetas.Text = $"Total de carpetas encontradas: {lvCarpetas.Items.Count}";
                    _ = CargarResumenMensualAsync(prefijoNormalizado, versionCarga);
                }
            }

            if (mostrarArchivos)
            {
                var files = await cloudflareService.ListFilesAsync(prefijoNormalizado + "/");

                if (versionCarga != _versionCargaCarpetas)
                    return;

                ActualizarVistaResumenMensual(files);
                var filesFiltrados = FiltrarArchivosPorAnioSeleccionado(files);
                var periodosDuplicados = ObtenerClavesPeriodosDuplicados(filesFiltrados);

                foreach (var file in filesFiltrados)
                {
                    string nombreArchivo = Path.GetFileName(file);
                    string textoMostrado = nombreArchivo;
                    var periodoArchivo = IntentarObtenerPeriodoArchivo(nombreArchivo);

                    if (periodoArchivo.HasValue)
                    {
                        textoMostrado = $"{ObtenerNombreMes(periodoArchivo.Value.Mes)} {periodoArchivo.Value.Anio}";
                    }

                    var item = new ListViewItem(textoMostrado, "file")
                    {
                        Tag = file,
                        ToolTipText = nombreArchivo
                    };

                    if (periodoArchivo.HasValue && periodosDuplicados.Contains(ObtenerClavePeriodo(periodoArchivo.Value)))
                    {
                        item.BackColor = System.Drawing.Color.FromArgb(255, 230, 230);
                        item.ForeColor = System.Drawing.Color.DarkRed;
                        item.ToolTipText = $"Duplicado detectado para {ObtenerNombreMes(periodoArchivo.Value.Mes)} {periodoArchivo.Value.Anio}: {nombreArchivo}";
                    }

                    lvCarpetas.Items.Add(item);
                }

                SeleccionarArchivoPendienteEnLista();
                lblTotalCarpetas.Text = $"Total de archivos encontrados: {filesFiltrados.Count}";
            }
            else
            {
                lblTotalCarpetas.Text = $"Total de carpetas encontradas: {lvCarpetas.Items.Count}";
            }
        }

        private async Task CargarResumenMensualAsync(string prefix, int versionCarga)
        {
            try
            {
                var filesResumen = await ObtenerArchivosParaResumenAsync(prefix);

                if (versionCarga != _versionCargaCarpetas)
                    return;

                ActualizarVistaResumenMensual(filesResumen);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CargarResumenMensualAsync] Error al cargar resumen: {ex.Message}");

                if (versionCarga != _versionCargaCarpetas)
                    return;

                ActualizarComboAnios(Array.Empty<int>());
                LimpiarGraficaCargaMensual();
                LimpiarEstadoMensual();
            }
        }

        private async Task DescargarArchivoSeleccionadoAsync()
        {
            if (lvCarpetas.SelectedItems.Count == 0)
                return;

            var item = lvCarpetas.SelectedItems[0];
            if (item.ImageKey == "file" && item.Tag is string fileKey)
            {
                using SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Path.GetFileName(fileKey);
                sfd.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    await cloudflareService.DownloadFileAsync(fileKey, sfd.FileName);
                    MessageBox.Show("Archivo descargado correctamente.", "Descarga completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ActualizarRutaActual()
        {
            lblRutaActual.Text = string.IsNullOrWhiteSpace(currentCloudflarePrefix)
                ? "Ruta actual: Inicio / Todas las carpetas"
                : $"Ruta actual: {ConstruirRutaVisible(currentCloudflarePrefix)}";
        }

        private static string ConstruirRutaVisible(string ruta)
        {
            string normalizada = (ruta ?? string.Empty).Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(normalizada))
                return "Inicio / Todas las carpetas";

            return "Inicio / " + normalizada.Replace("/", " / ");
        }

        private Label CrearEtiquetaEstadoMensual()
        {
            return new Label
            {
                Name = "lblEstadoMensual",
                AutoSize = false,
                BackColor = System.Drawing.Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private DataGridView CrearGridEstadoMensual()
        {
            var grid = new DataGridView
            {
                Name = "dgvEstadoMensual",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                BackgroundColor = System.Drawing.Color.White,
                GridColor = System.Drawing.Color.FromArgb(220, 226, 231),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(224, 247, 250),
                ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = System.Drawing.Color.FromArgb(30, 41, 59),
                SelectionForeColor = System.Drawing.Color.White
            };

            return grid;
        }

        private (int Anio, int Mes)? IntentarObtenerPeriodoArchivo(string? nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return null;

            string nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo).Trim();
            var match = Regex.Match(nombreBase, @"^(?<anio>\d{4})[^\d]?(?<mes>\d{2})");

            if (!match.Success)
                return null;

            if (!int.TryParse(match.Groups["anio"].Value, out int anio) ||
                !int.TryParse(match.Groups["mes"].Value, out int mes) ||
                mes < 1 || mes > 12)
            {
                return null;
            }

            return (anio, mes);
        }

        private static string ObtenerClavePeriodo((int Anio, int Mes) periodo)
        {
            return $"{periodo.Anio:D4}-{periodo.Mes:D2}";
        }

        private HashSet<string> ObtenerClavesPeriodosDuplicados(IEnumerable<string> archivos)
        {
            return archivos
                .Select(Path.GetFileName)
                .Select(IntentarObtenerPeriodoArchivo)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .GroupBy(ObtenerClavePeriodo)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private void ConfigurarGraficaCargaMensual(List<(int Anio, int Mes, int TotalArchivos)> periodosDetectados)
        {
            if (periodosDetectados.Count == 0)
            {
                LimpiarGraficaCargaMensual("Control de carga mensual (sin archivos con fecha válida)");
                return;
            }

            var datosPorPeriodo = periodosDetectados
                .GroupBy(x => new { x.Anio, x.Mes })
                .ToDictionary(
                    g => (g.Key.Anio, g.Key.Mes),
                    g => g.Sum(x => x.TotalArchivos));

            int anioMinimo = periodosDetectados.Min(x => x.Anio);
            int anioMaximo = periodosDetectados.Max(x => x.Anio);
            int mesMinimo = periodosDetectados
                .Where(x => x.Anio == anioMinimo)
                .Min(x => x.Mes);
            int mesMaximo = periodosDetectados
                .Where(x => x.Anio == anioMaximo)
                .Max(x => x.Mes);

            var periodosLineaTiempo = new List<(int? Anio, int? Mes, bool EsSeparador)>();
            var rangosAnios = new List<(int Anio, int Inicio, int Fin)>();
            for (int anio = anioMinimo; anio <= anioMaximo; anio++)
            {
                int mesInicio = anio == anioMinimo ? mesMinimo : 1;
                int mesFin = anio == anioMaximo ? mesMaximo : 12;
                int indiceInicioAnio = periodosLineaTiempo.Count;

                for (int mes = mesInicio; mes <= mesFin; mes++)
                {
                    periodosLineaTiempo.Add((anio, mes, false));
                }

                rangosAnios.Add((anio, indiceInicioAnio, periodosLineaTiempo.Count - 1));

                if (!_anioSeleccionado.HasValue && anio < anioMaximo)
                {
                    periodosLineaTiempo.Add((null, null, true));
                }
            }

            var labels = periodosLineaTiempo
                .Select(x =>
                {
                    if (x.EsSeparador || !x.Anio.HasValue || !x.Mes.HasValue)
                        return string.Empty;

                    return _anioSeleccionado.HasValue
                        ? ObtenerNombreMesCorto(x.Mes.Value)
                        : ObtenerNombreMesCorto(x.Mes.Value);
                })
                .ToArray();

            var totalesArchivos = periodosLineaTiempo
                .Select(periodo =>
                {
                    if (periodo.EsSeparador || !periodo.Anio.HasValue || !periodo.Mes.HasValue)
                        return 0;

                    return datosPorPeriodo.TryGetValue((periodo.Anio.Value, periodo.Mes.Value), out int total) ? total : 0;
                })
                .ToArray();

            var mesesDuplicados = periodosLineaTiempo
                .Select(periodo =>
                {
                    if (periodo.EsSeparador || !periodo.Anio.HasValue || !periodo.Mes.HasValue)
                        return 0;

                    return datosPorPeriodo.TryGetValue((periodo.Anio.Value, periodo.Mes.Value), out int total) && total > 1 ? total : 0;
                })
                .ToArray();

            var mesesSinCarga = totalesArchivos
                .Select((total, indice) => periodosLineaTiempo[indice].EsSeparador ? 0 : total == 0 ? 1 : 0)
                .ToArray();

            int totalMesesSinCarga = mesesSinCarga.Sum();
            int totalMesesDuplicados = mesesDuplicados.Count(total => total > 0);
            int maximoArchivos = Math.Max(2, Math.Max(totalesArchivos.Max(), mesesDuplicados.Max()) + 1);

            chartCargaMensual.Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Meses cargados",
                    Values = totalesArchivos,
                    Fill = new SolidColorPaint(SKColor.Parse("#1d4ed8")),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#0f172a")),
                    DataLabelsSize = 14,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue > 0 ? $"{point.Coordinate.PrimaryValue:0}" : string.Empty,
                    MaxBarWidth = 38,
                    IgnoresBarPosition = false
                },
                new ColumnSeries<int>
                {
                    Name = "Mes duplicado",
                    Values = mesesDuplicados,
                    Fill = new SolidColorPaint(SKColor.Parse("#dc2626")),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#7f1d1d")),
                    DataLabelsSize = 14,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue > 1 ? $"{point.Coordinate.PrimaryValue:0}" : string.Empty,
                    MaxBarWidth = 38,
                    IgnoresBarPosition = false
                },
                new ColumnSeries<int>
                {
                    Name = "Sin carga",
                    Values = mesesSinCarga,
                    Fill = new SolidColorPaint(SKColor.Parse("#ef4444")),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#b91c1c")),
                    DataLabelsSize = 14,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue > 0 ? "0" : string.Empty,
                    MaxBarWidth = 38,
                    IgnoresBarPosition = false
                }
            };

            chartCargaMensual.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    TextSize = 11,
                    SeparatorsPaint = null,
                    TicksPaint = null,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#475569"))
                }
            };

            chartCargaMensual.VisualElements = Array.Empty<LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual>();
            ActualizarEtiquetasAniosGrafica(rangosAnios);

            chartCargaMensual.YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MinStep = 1,                    MaxLimit = maximoArchivos,
                    IsVisible = false,
                    SeparatorsPaint = null,
                    TicksPaint = null
                }
            };

            chartCargaMensual.Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
            {
                Text = anioMinimo == anioMaximo
                    ? $"Control de carga mensual - {anioMinimo} ({totalMesesSinCarga} sin carga, {totalMesesDuplicados} con duplicados)"
                    : $"Control de carga mensual - {anioMinimo} a {anioMaximo} ({totalMesesSinCarga} sin carga, {totalMesesDuplicados} con duplicados)",
                TextSize = 18,
                Padding = new LiveChartsCore.Drawing.Padding(10),
                Paint = new SolidColorPaint(SKColor.Parse("#2c3e50"))
            };
            chartCargaMensual.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        }

        private static string ObtenerNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => $"Mes {mes}"
            };
        }

        private static string ObtenerNombreMesCorto(int mes)
        {
            return mes switch
            {
                1 => "ENE",
                2 => "FEB",
                3 => "MAR",
                4 => "ABR",
                5 => "MAY",
                6 => "JUN",
                7 => "JUL",
                8 => "AGO",
                9 => "SEP",
                10 => "OCT",
                11 => "NOV",
                12 => "DIC",
                _ => $"M{mes}"
            };
        }

        private static string[] ObtenerSegmentosRutaArchivo(string claveArchivo)
        {
            string claveNormalizada = claveArchivo.Replace('\\', '/').Trim('/');
            return claveNormalizada.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ObtenerRazonSocialDesdeClaveArchivo(string claveArchivo)
        {
            var segmentos = ObtenerSegmentosRutaArchivo(claveArchivo);
            return segmentos.Length > 0 ? segmentos[0] : string.Empty;
        }



        private string ObtenerContextoEstadoMensual()
        {
            string prefijoNormalizado = (currentCloudflarePrefix ?? string.Empty).Replace('\\', '/').Trim('/');

            if (string.IsNullOrWhiteSpace(prefijoNormalizado))
                return string.Empty;

            return prefijoNormalizado;
        }

        private static string ObtenerEtiquetaContextoEstadoMensual(string contexto)
        {
            var segmentos = ObtenerSegmentosRutaArchivo(contexto);
            return segmentos.Length == 0 ? string.Empty : segmentos[^1];
        }

        private static string ObtenerClaveAgrupacionEstadoMensual(string claveArchivo, string contexto)
        {
            string contextoNormalizado = (contexto ?? string.Empty).Replace('\\', '/').Trim('/');

            if (!string.IsNullOrWhiteSpace(contextoNormalizado))
            {
                var segmentosContexto = ObtenerSegmentosRutaArchivo(contextoNormalizado);
                if (segmentosContexto.Length > 0)
                    return segmentosContexto[^1];
            }

            return ObtenerRazonSocialDesdeClaveArchivo(claveArchivo);
        }

        private List<string> ObtenerFilasEstadoMensual(string contexto, IEnumerable<string> filasDetectadas)
        {
            if (!string.IsNullOrWhiteSpace(contexto))
                return new List<string> { ObtenerEtiquetaContextoEstadoMensual(contexto) };

            try
            {
                var empresasCatalogo = catalogoService.ObtenerRazonesSociales()
                    .Select(x => x.NombreRazon?.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();

                if (empresasCatalogo.Count > 0)
                    return empresasCatalogo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EstadoMensual] No fue posible cargar razones sociales: {ex.Message}");
            }

            return filasDetectadas
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void ActualizarBotonRegresar()
        {
            btnRegresarCarpeta.Enabled = _historialCarpetas.Count > 0;
            btnRegresarCarpeta.BackColor = btnRegresarCarpeta.Enabled
                ? System.Drawing.Color.FromArgb(52, 73, 94)
                : System.Drawing.Color.FromArgb(149, 165, 166);
        }



        private async void btnRegresarCarpeta_Click(object sender, EventArgs e)
        {
            if (_historialCarpetas.Count == 0)
                return;

            string prefijoAnterior = _historialCarpetas.Pop();
            ActualizarBotonRegresar();
            await CargarCarpetasCloudflareAsync(prefijoAnterior, false);
        }

        private int ObtenerAnioAnalisis(IEnumerable<int> aniosDisponibles)
        {
            var anios = aniosDisponibles
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (anios.Count == 0)
                return DateTime.Now.Year;

            if (_anioSeleccionado.HasValue && anios.Contains(_anioSeleccionado.Value))
                return _anioSeleccionado.Value;

            return anios.Max();
        }

        private void ActualizarComboAnios(IEnumerable<int> aniosDisponibles)
        {
            var anios = aniosDisponibles
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            _inicializandoComboAnio = true;
            cboAnio.BeginUpdate();
            cboAnio.Items.Clear();

            if (anios.Count == 0)
            {
                _anioSeleccionado = null;
                cboAnio.SelectedIndex = -1;
                cboAnio.Enabled = false;
            }
            else
            {
                cboAnio.Items.Add("Todos");

                foreach (int anio in anios)
                {
                    cboAnio.Items.Add(anio);
                }

                cboAnio.Enabled = true;

                if (_filtroAnioManual && _anioSeleccionado.HasValue && anios.Contains(_anioSeleccionado.Value))
                {
                    cboAnio.SelectedItem = _anioSeleccionado.Value;
                }
                else
                {
                    _filtroAnioManual = false;
                    _anioSeleccionado = null;
                    cboAnio.SelectedItem = "Todos";
                }
            }

            cboAnio.EndUpdate();
            _inicializandoComboAnio = false;
        }

        private void RestablecerFiltroAnioATodos()
        {
            _filtroAnioManual = false;
            _anioSeleccionado = null;

            if (cboAnio.Items.Count == 0)
                return;

            _inicializandoComboAnio = true;
            cboAnio.SelectedItem = "Todos";
            _inicializandoComboAnio = false;
        }

        private void LimpiarEstadoMensual(string titulo = "Semáforo mensual de cargas")
        {
            lblEstadoMensual.Text = titulo;
            dgvEstadoMensual.Columns.Clear();
            dgvEstadoMensual.Rows.Clear();
        }

        private void ConfigurarEstadoMensual(IEnumerable<string> archivos, string empresaContexto)
        {
            var archivosNormalizados = archivos
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Replace('\\', '/').Trim('/'))
                .ToList();

            var registros = archivosNormalizados
                .Select(archivo => new
                {
                    Fila = ObtenerClaveAgrupacionEstadoMensual(archivo, empresaContexto),
                    Periodo = IntentarObtenerPeriodoArchivo(Path.GetFileName(archivo))
                })
                .Where(x => x.Periodo.HasValue)
                .ToList();

            if (registros.Count == 0)
            {
                LimpiarEstadoMensual("Semáforo mensual de cargas (sin archivos con fecha válida)");
                return;
            }

            var empresas = ObtenerFilasEstadoMensual(empresaContexto, registros.Select(x => x.Fila));
            var periodosDisponibles = registros
                .Select(x => x.Periodo!.Value)
                .Distinct()
                .OrderBy(x => x.Anio)
                .ThenBy(x => x.Mes)
                .ToList();

            List<(int Anio, int Mes)> periodosVisibles;
            string tituloPeriodo;
            var aniosVisibles = periodosDisponibles
                .Select(x => x.Anio)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (_anioSeleccionado.HasValue)
            {
                int anioSeleccionado = _anioSeleccionado.Value;
                periodosVisibles = Enumerable.Range(1, 12)
                    .Select(mes => (anioSeleccionado, mes))
                    .ToList();
                tituloPeriodo = anioSeleccionado.ToString();
            }
            else
            {
                int anioMinimo = periodosDisponibles.Min(x => x.Anio);
                int anioMaximo = periodosDisponibles.Max(x => x.Anio);
                periodosVisibles = Enumerable.Range(anioMinimo, anioMaximo - anioMinimo + 1)
                    .SelectMany(anio => Enumerable.Range(1, 12).Select(mes => (anio, mes)))
                    .ToList();
                tituloPeriodo = anioMinimo == anioMaximo
                    ? anioMinimo.ToString()
                    : $"{anioMinimo} a {anioMaximo}";
            }

            var mesesPorEmpresa = registros
                .GroupBy(x => x.Fila, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Periodo!.Value).ToHashSet(),
                    StringComparer.OrdinalIgnoreCase);

            dgvEstadoMensual.SuspendLayout();
            dgvEstadoMensual.Columns.Clear();
            dgvEstadoMensual.Rows.Clear();

            var columnaEmpresa = new DataGridViewTextBoxColumn
            {
                Name = "Empresa",
                HeaderText = "EMPRESA",
                Width = 170,
                Frozen = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvEstadoMensual.Columns.Add(columnaEmpresa);

            if (!_anioSeleccionado.HasValue)
            {
                dgvEstadoMensual.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Anio",
                    HeaderText = "AÑO",
                    Width = 70,
                    Frozen = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            dgvEstadoMensual.ColumnHeadersHeight = 38;

            for (int mes = 1; mes <= 12; mes++)
            {
                dgvEstadoMensual.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = $"Mes{mes}",
                    HeaderText = ObtenerNombreMesCorto(mes),
                    Width = 58,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            foreach (string empresa in empresas)
            {
                var mesesConArchivo = mesesPorEmpresa.TryGetValue(empresa, out var mesesEmpresa)
                    ? mesesEmpresa
                    : new HashSet<(int Anio, int Mes)>();

                var aniosFila = _anioSeleccionado.HasValue
                    ? new List<int> { _anioSeleccionado.Value }
                    : aniosVisibles;

                for (int indiceAnio = 0; indiceAnio < aniosFila.Count; indiceAnio++)
                {
                    int anio = aniosFila[indiceAnio];
                    int indiceFila = dgvEstadoMensual.Rows.Add();
                    var fila = dgvEstadoMensual.Rows[indiceFila];
                    fila.Height = 32;
                    fila.Cells[0].Value = empresa;
                    fila.Cells[0].Style.BackColor = System.Drawing.Color.White;
                    fila.Cells[0].Style.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
                    fila.Cells[0].Style.SelectionBackColor = System.Drawing.Color.FromArgb(226, 232, 240);
                    fila.Cells[0].Style.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);

                    int indiceInicioMeses = 1;
                    if (!_anioSeleccionado.HasValue)
                    {
                        fila.Cells[1].Value = anio;
                        fila.Cells[1].Style.BackColor = System.Drawing.Color.White;
                        fila.Cells[1].Style.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
                        fila.Cells[1].Style.SelectionBackColor = System.Drawing.Color.FromArgb(226, 232, 240);
                        fila.Cells[1].Style.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
                        indiceInicioMeses = 2;
                    }

                    for (int mes = 1; mes <= 12; mes++)
                    {
                        var periodo = (anio, mes);
                        bool tieneArchivo = mesesConArchivo.Contains(periodo);
                        var celda = fila.Cells[indiceInicioMeses + mes - 1];
                        celda.Value = tieneArchivo ? "OK" : "NO";
                        celda.Style.BackColor = tieneArchivo ? System.Drawing.Color.FromArgb(146, 208, 80) : System.Drawing.Color.FromArgb(239, 68, 68);
                        celda.Style.ForeColor = System.Drawing.Color.White;
                        celda.Style.SelectionBackColor = tieneArchivo ? System.Drawing.Color.FromArgb(95, 158, 38) : System.Drawing.Color.FromArgb(185, 28, 28);
                        celda.Style.SelectionForeColor = System.Drawing.Color.White;
                    }
                }

            }

            dgvEstadoMensual.ClearSelection();
            dgvEstadoMensual.ResumeLayout();

            int totalMesesOk = mesesPorEmpresa.Values.Sum(x => x.Count);
            int totalMesesNo = (empresas.Count * periodosVisibles.Count) - totalMesesOk;
            lblEstadoMensual.Text = $"Semáforo mensual de cargas - {tituloPeriodo} ({totalMesesOk} OK / {totalMesesNo} sin archivo)";
        }

        private void InicializarImageList()
        {
            imageListCarpetas.Images.Clear();
            imageListCarpetas.ImageSize = new System.Drawing.Size(32, 32); // Forzar tamaño adecuado
            try
            {
                string folderIconPath = ObtenerRutaIconoCarpeta();
                if (File.Exists(folderIconPath))
                {
                    using (var bmp = new System.Drawing.Bitmap(folderIconPath))
                    {
                        var resized = new System.Drawing.Bitmap(bmp, imageListCarpetas.ImageSize);
                        imageListCarpetas.Images.Add("folder", resized);
                    }
                }
                else
                {
                    MessageBox.Show($"No se encontró el icono personalizado en: {folderIconPath}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    imageListCarpetas.Images.Add("folder", SystemIcons.WinLogo);
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar el icono personalizado: {ex.Message}", "Error", ex, "Carga de icono de carpetas en inventario");
                imageListCarpetas.Images.Add("folder", SystemIcons.WinLogo);
            }

            try
            {
                string excelIconPath = ObtenerRutaIconoExcel();
                if (File.Exists(excelIconPath))
                {
                    using (var bmp = new System.Drawing.Bitmap(excelIconPath))
                    {
                        var resized = new System.Drawing.Bitmap(bmp, imageListCarpetas.ImageSize);
                        imageListCarpetas.Images.Add("file", resized);
                    }
                }
                else
                {
                    using var bmp = new System.Drawing.Bitmap(Retorno360Tacna.Properties.Resources.ext_xlsx_icon_176245, imageListCarpetas.ImageSize);
                    imageListCarpetas.Images.Add("file", bmp);
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar el icono de Excel: {ex.Message}", "Error", ex, "Carga de icono de archivos Excel en inventario");
                imageListCarpetas.Images.Add("file", SystemIcons.Application);
            }
        }

        private byte[] CapturarControlComoPng(Control control)
        {
            using Bitmap bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        private byte[] CapturarGraficaInventarioComoPng()
        {
            int margenSuperior = _etiquetasAniosGrafica.Count > 0 ? 44 : 0;
            using Bitmap bitmap = new Bitmap(chartCargaMensual.Width, chartCargaMensual.Height + margenSuperior);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.White);

            using Bitmap grafica = new Bitmap(chartCargaMensual.Width, chartCargaMensual.Height);
            chartCargaMensual.DrawToBitmap(grafica, new Rectangle(0, 0, grafica.Width, grafica.Height));
            graphics.DrawImage(grafica, 0, margenSuperior);

            foreach (var etiqueta in _etiquetasAniosGrafica)
            {
                Rectangle rectanguloEtiqueta = new Rectangle(
                    etiqueta.Left - chartCargaMensual.Left,
                    8,
                    etiqueta.Width,
                    etiqueta.Height);

                using SolidBrush fondo = new SolidBrush(etiqueta.BackColor);
                using SolidBrush texto = new SolidBrush(etiqueta.ForeColor);
                using StringFormat formato = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.FillRectangle(fondo, rectanguloEtiqueta);
                graphics.DrawString(etiqueta.Text, etiqueta.Font, texto, rectanguloEtiqueta, formato);
            }

            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        private void btnExportarPdf_Click(object sender, EventArgs e)
        {
            try
            {
                if (chartCargaMensual.Width <= 0 || chartCargaMensual.Height <= 0 || dgvEstadoMensual.Rows.Count == 0)
                {
                    MessageBox.Show("No hay información gráfica disponible para exportar.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                using SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Archivo PDF (*.pdf)|*.pdf";
                saveDialog.Title = "Guardar reporte de inventario";
                saveDialog.FileName = $"ReporteInventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                byte[] graficaImagen = CapturarGraficaInventarioComoPng();
                byte[] tablaImagen = CapturarControlComoPng(dgvEstadoMensual);
                string tituloGrafica = chartCargaMensual.Title is LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual title
                    ? title.Text
                    : "Control de carga mensual";
                string tituloTabla = lblEstadoMensual.Text;
                string contexto = string.IsNullOrWhiteSpace(currentCloudflarePrefix) ? "Todas las carpetas" : currentCloudflarePrefix;
                string anio = _anioSeleccionado?.ToString() ?? "Todos";

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                        page.Header().Column(column =>
                        {
                            column.Item().Text("Reporte de Inventario")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(9);
                            column.Item().Text($"Filtro Año: {anio}").FontSize(9);
                            column.Item().Text($"Ruta analizada: {contexto}").FontSize(9);
                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingVertical(10).Column(column =>
                        {
                            column.Item().Text(tituloGrafica).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().PaddingTop(8).Image(graficaImagen, QuestPDF.Infrastructure.ImageScaling.FitWidth);
                            column.Item().PaddingTop(14).Text(tituloTabla).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().PaddingTop(8).Image(tablaImagen, QuestPDF.Infrastructure.ImageScaling.FitWidth);
                        });
                    });
                }).GeneratePdf(saveDialog.FileName);

                DialogResult resultado = MessageBox.Show(
            "PDF generado exitosamente.\n\n¿Desea abrir el archivo?",
            "Éxito",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

                if (resultado == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al exportar el PDF del inventario: {ex.Message}",
                    "Error", ex, "Exportación de gráficos de inventario a PDF");
            }
        }

        private string ObtenerRutaIconoCarpeta()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] rutasCandidatas =
            {
                Path.Combine(baseDirectory, "Resources", "folder_icon-icons.com_55318.png"),
                Path.Combine(baseDirectory, @"..\..\..\Resources\folder_icon-icons.com_55318.png"),
                Path.Combine(baseDirectory, @"..\..\..\..\Resources\folder_icon-icons.com_55318.png")
            };

            foreach (var ruta in rutasCandidatas)
            {
                string rutaCompleta = Path.GetFullPath(ruta);
                if (File.Exists(rutaCompleta))
                    return rutaCompleta;
            }

            return Path.GetFullPath(rutasCandidatas[0]);
        }

        private string ObtenerRutaIconoExcel()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] rutasCandidatas =
            {
                Path.Combine(baseDirectory, "Resources", "xls_filetype_spreadsheet_microsoft_excel_icon_151825.png"),
                Path.Combine(baseDirectory, @"..\..\..\Resources\xls_filetype_spreadsheet_microsoft_excel_icon_151825.png"),
                Path.Combine(baseDirectory, @"..\..\..\..\Resources\xls_filetype_spreadsheet_microsoft_excel_icon_151825.png")
            };

            foreach (var ruta in rutasCandidatas)
            {
                string rutaCompleta = Path.GetFullPath(ruta);
                if (File.Exists(rutaCompleta))
                    return rutaCompleta;
            }

            return Path.GetFullPath(rutasCandidatas[0]);
        }

        private async void FrmReportesInventario_Load(object sender, EventArgs e)
        {
            await VerificarConexionPortalWebAsync();
            await CargarArchivosConObservacionesAsync();
            await CargarSolicitudesEliminacionAsync();
            CargarRazonesSociales();
            InicializarImageList();
            cboAnio.Enabled = false;
            // Usar el mismo método del botón Actualizar para mostrar las carpetas correctamente
            btnActualizar_Click(btnActualizar, EventArgs.Empty);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerMensajesObservacion.Stop();
            timerMensajesObservacion.Dispose();
            _fontNombreUsuario.Dispose();
            _fontFecha.Dispose();
            _fontMensaje.Dispose();
            base.OnFormClosed(e);
        }
        private async Task VerificarConexionPortalWebAsync()
        {
            lblEstadoConexionPortal.Text = "Conexión portal web: verificando...";
            lblEstadoConexionPortal.ForeColor = System.Drawing.Color.FromArgb(127, 140, 141);

            try
            {
                await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                await conexionPortal.OpenAsync();

                string dbName = conexionPortal.Database ?? string.Empty;
                lblEstadoConexionPortal.Text = $"Conexión portal web: correcta ({dbName})";
                lblEstadoConexionPortal.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                lblEstadoConexionPortal.Text = $"Conexión portal web: sin conexión ({ex.Message})";
                lblEstadoConexionPortal.ForeColor = System.Drawing.Color.FromArgb(192, 57, 43);
                ErrorLogger.LogError(ex, "Verificación de conexión PostgreSQL del portal web en Reportes de Inventario");
            }
        }

        private bool FormNoDisponible() => IsDisposed || !IsHandleCreated;


        // El método de verificación de carpeta local ya no es necesario

        private void CargarRazonesSociales()
        {
            try
            {
                _inicializandoCombo = true;
                var razones = catalogoService.ObtenerRazonesSociales();

                cboRazonSocial.DataSource = razones;
                cboRazonSocial.DisplayMember = "NombreRazon";
                cboRazonSocial.ValueMember = "IdRazon";

                if (razones.Any())
                {
                    cboRazonSocial.SelectedIndex = -1;
                }
                _inicializandoCombo = false;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al cargar razones sociales: {ex.Message}",
                    "Error", ex, "Carga de razones sociales en inventario");
            }
        }

        private void cboRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inicializandoCombo)
                return;

            if (cboRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                CargarCarpetasPorRazonSocial(razonSeleccionada.NombreRazon);
            }
            else if (cboRazonSocial.SelectedIndex == -1)
            {
                CargarTodasLasCarpetas();
            }
        }

        private string carpetaActual = null;

        private async void CargarTodasLasCarpetas()
        {
            carpetaActual = null;
            RestablecerFiltroAnioATodos();
            _historialCarpetas.Clear();
            ActualizarBotonRegresar();
            await CargarCarpetasCloudflareAsync(string.Empty, false);
        }



        private async void CargarCarpetasPorRazonSocial(string nombreRazon)
        {
            carpetaActual = nombreRazon;
            RestablecerFiltroAnioATodos();
            _historialCarpetas.Clear();
            ActualizarBotonRegresar();
            await CargarCarpetasCloudflareAsync(nombreRazon, false);
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                CargarCarpetasPorRazonSocial(razonSeleccionada.NombreRazon);
            }
            else
            {
                CargarTodasLasCarpetas();
            }
        }

        private void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            cboRazonSocial.SelectedIndex = -1;
            _anioSeleccionado = null;
            CargarTodasLasCarpetas();
        }

        public void NavegarADetalleNotificacion(string? direccion, string? rutaR2)
        {
            string prefijoDestino = (direccion ?? string.Empty).Replace('\\', '/').Trim('/');
            _archivoPendienteSeleccion = Path.GetFileName((rutaR2 ?? string.Empty).Replace('\\', '/').Trim('/'));

            if (string.IsNullOrWhiteSpace(prefijoDestino))
            {
                _archivoPendienteSeleccion = string.Empty;
                CargarTodasLasCarpetas();
                return;
            }

            var segmentos = ObtenerSegmentosRutaArchivo(prefijoDestino);
            if (segmentos.Length > 0)
            {
                string razonSocial = segmentos[0];
                RazonSocial? razon = null;
                if (cboRazonSocial.DataSource is IEnumerable<RazonSocial> razonesSociales)
                {
                    foreach (var item in razonesSociales)
                    {
                        if (string.Equals(item.NombreRazon, razonSocial, StringComparison.OrdinalIgnoreCase))
                        {
                            razon = item;
                            break;
                        }
                    }
                }

                if (razon != null)
                {
                    cboRazonSocial.SelectedItem = razon;
                }
            }

            RestablecerFiltroAnioATodos();
            _historialCarpetas.Clear();
            ActualizarBotonRegresar();
            _ = CargarCarpetasCloudflareAsync(prefijoDestino, false);
        }

        private void SeleccionarArchivoPendienteEnLista()
        {
            if (string.IsNullOrWhiteSpace(_archivoPendienteSeleccion))
                return;

            foreach (ListViewItem item in lvCarpetas.Items)
            {
                if (item.ImageKey != "file" || item.Tag is not string rutaArchivo)
                    continue;

                string nombreArchivo = Path.GetFileName(rutaArchivo);
                if (!string.Equals(nombreArchivo, _archivoPendienteSeleccion, StringComparison.OrdinalIgnoreCase))
                    continue;

                lvCarpetas.SelectedItems.Clear();
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                break;
            }

            _archivoPendienteSeleccion = string.Empty;
        }

        private List<string> FiltrarArchivosPorAnioSeleccionado(IEnumerable<string> archivos)
        {
            var archivosNormalizados = archivos
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (!_anioSeleccionado.HasValue)
                return archivosNormalizados;

            return archivosNormalizados
                .Where(archivo =>
                {
                    var periodo = IntentarObtenerPeriodoArchivo(Path.GetFileName(archivo));
                    return periodo.HasValue && periodo.Value.Anio == _anioSeleccionado.Value;
                })
                .ToList();
        }

        private void cboAnio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inicializandoComboAnio)
                return;

            if (cboAnio.SelectedItem is int anio)
            {
                _anioSeleccionado = anio;
                _filtroAnioManual = true;
            }
            else
            {
                _anioSeleccionado = null;
                _filtroAnioManual = false;
            }

            if (!string.IsNullOrWhiteSpace(currentCloudflarePrefix) && lvCarpetas.Items.Count > 0)
            {
                _ = CargarCarpetasCloudflareAsync(currentCloudflarePrefix);
            }
        }



        private async void lvCarpetas_DoubleClick(object sender, EventArgs e)
        {
            if (lvCarpetas.SelectedItems.Count == 0)
                return;
            var item = lvCarpetas.SelectedItems[0];
            if (item.ImageKey == "folder" && item.Tag is string folderKey)
            {
                // Al hacer doble clic en una carpeta, mostrar sus subcarpetas o archivos
                _ = CargarCarpetasCloudflareAsync(folderKey);
            }
            else if (item.ImageKey == "file")
            {
                // Al hacer doble clic en un archivo: primero mostrar/abrir el archivo y después abrir (si existe)
                // la ventana de observación para no perder la funcionalidad previa.
                try
                {
                    // Iniciar la descarga/visualización del archivo (comportamiento existente)
                    await DescargarArchivoSeleccionadoAsync();

                    // Después intentar abrir el modal de observación si existe una observación activa
                    if (item.Tag is string rutaArchivoR2)
                    {
                        await using var conexionPortal = new NpgsqlConnection(ConexionPortalWeb);
                        await conexionPortal.OpenAsync();

                        int? idArchivo = await ObtenerIdArchivoHistorialAsync(conexionPortal, rutaArchivoR2);
                        if (idArchivo.HasValue && usuarioActual?.IdWeb is int idUsuarioWeb && idUsuarioWeb > 0)
                        {
                            int? idObs = await ObtenerObservacionActivaAsync(conexionPortal, idArchivo.Value, idUsuarioWeb);
                            if (idObs.HasValue)
                            {
                                using var frm = new FrmObservacionDetalle(idObs.Value, idUsuarioWeb);
                                // Mostrar modal encima de la ventana principal
                                frm.ShowDialog(this);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Registrar el error y no bloquear la experiencia del usuario
                    ErrorLogger.LogError(ex, "Acción doble clic archivo: abrir/mostrar observación y archivo");
                }
            }
        }



        private void lblRazonSocial_Click(object sender, EventArgs e)
        {

        }

        public async Task EliminarArchivoDbAsync(string rutaArchivoR2)
        {
            await using var conn = new NpgsqlConnection(ConexionPortalWeb);
            await conn.OpenAsync();

            int? idArchivo = await ObtenerIdArchivoHistorialAsync(conn, rutaArchivoR2);
            if (!idArchivo.HasValue)
                return;

            const string queryMensajes = @"
                DELETE FROM observacion_mensajes
                WHERE observacion_id IN (
                    SELECT id
                    FROM observaciones
                    WHERE idarchivo = @idArchivo
                );";

            const string queryObservaciones = @"
                DELETE FROM observaciones
                WHERE idarchivo = @idArchivo;";

            const string querySolicitudes = @"
                DELETE FROM archivo_delete_requests
                WHERE archivo_id = @idArchivo;";

            const string queryArchivo = @"
                DELETE FROM archivos_historial
                WHERE id = @idArchivo;";

            await using var transaccion = await conn.BeginTransactionAsync();

            await using (var cmdMensajes = new NpgsqlCommand(queryMensajes, conn, transaccion))
            {
                cmdMensajes.Parameters.AddWithValue("@idArchivo", idArchivo.Value);
                await cmdMensajes.ExecuteNonQueryAsync();
            }

            await using (var cmdObservaciones = new NpgsqlCommand(queryObservaciones, conn, transaccion))
            {
                cmdObservaciones.Parameters.AddWithValue("@idArchivo", idArchivo.Value);
                await cmdObservaciones.ExecuteNonQueryAsync();
            }

            await using (var cmdSolicitudes = new NpgsqlCommand(querySolicitudes, conn, transaccion))
            {
                cmdSolicitudes.Parameters.AddWithValue("@idArchivo", idArchivo.Value);
                await cmdSolicitudes.ExecuteNonQueryAsync();
            }

            await using (var cmdArchivo = new NpgsqlCommand(queryArchivo, conn, transaccion))
            {
                cmdArchivo.Parameters.AddWithValue("@idArchivo", idArchivo.Value);
                await cmdArchivo.ExecuteNonQueryAsync();
            }

            await transaccion.CommitAsync();
        }

        private bool _archivoSeleccionadoValido = false;

        private async void BtmEliminarArchivo_Click(object sender, EventArgs e)
        {

            if (!_archivoSeleccionadoValido
                || lvCarpetas.SelectedItems.Count == 0
                || lvCarpetas.SelectedItems[0].ImageKey != "file"
                || lvCarpetas.SelectedItems[0].Tag is not string rutaArchivoR2)
            {
                MessageBox.Show("Seleccione un archivo para eliminar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombreArchivo = Path.GetFileName(rutaArchivoR2);

            DialogResult confirmacion = MessageBox.Show(
                $"¿Desea eliminar el archivo '{nombreArchivo}' del bucket? Esta acción no se puede deshacer.",
                "Eliminar archivo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {

                BtmEliminarArchivo.Enabled = false;

                await EliminarArchivoDbAsync(rutaArchivoR2);

                await cloudflareService.DeleteFileAsync(rutaArchivoR2);

                if (FormNoDisponible()) return;

                if (string.Equals(_rutaArchivoMensajesActiva, rutaArchivoR2, StringComparison.OrdinalIgnoreCase))
                {
                    _observacionActivaId = null;
                    _rutaArchivoMensajesActiva = string.Empty;
                    timerMensajesObservacion.Stop();
                    LimpiarPanelMensajes("Archivo eliminado.");
                }

                MessageBox.Show("Archivo eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnActualizar_Click(btnActualizar, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError($"Error al eliminar el archivo: {ex.Message}", "Error", ex, "Eliminación de archivo desde el panel de carpetas");
            }
            finally
            {
                BtmEliminarArchivo.Enabled = true;
                ActualizarBotonAgregarObservacion(); // re-sincroniza el ícono según la selección actual
            }

        }
    }
}
