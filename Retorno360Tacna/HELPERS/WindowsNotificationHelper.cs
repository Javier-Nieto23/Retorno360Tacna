using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Retorno360Tacna.HELPERS
{
    internal static class WindowsNotificationHelper
    {
        internal sealed class NotificationClickInfo
        {
            public int IdNotificacion { get; init; }
            public string Direccion { get; init; } = string.Empty;
            public string RutaR2 { get; init; } = string.Empty;
        }

        private static readonly object SyncRoot = new();
        private static NotifyIcon? notifyIcon;
        private static System.Threading.Timer? disposeTimer;
        private static NotificationClickInfo? notificacionPendiente;

        public static event EventHandler<NotificationClickInfo>? NotificationDetailRequested;

        public static void MostrarNotificacionCargaR2(int idNotificacion, string? nombreArchivo, string? direccion, string? rutaR2)
        {
            string archivo = string.IsNullOrWhiteSpace(nombreArchivo) ? "Archivo" : nombreArchivo;
            string carpetaDestino = string.IsNullOrWhiteSpace(direccion) ? string.Empty : direccion;
            string destino = string.IsNullOrWhiteSpace(rutaR2) ? "R2" : rutaR2;

            try
            {
                lock (SyncRoot)
                {
                    disposeTimer?.Dispose();
                    notifyIcon?.Dispose();

                    notifyIcon = new NotifyIcon
                    {
                        Icon = SystemIcons.Information,
                        Visible = true,
                        BalloonTipTitle = "Carga completada",
                        BalloonTipText = $"{archivo} se subió correctamente a R2.\nDestino: {destino}",
                        BalloonTipIcon = ToolTipIcon.Info
                    };

                    notificacionPendiente = new NotificationClickInfo
                    {
                        IdNotificacion = idNotificacion,
                        Direccion = carpetaDestino,
                        RutaR2 = destino
                    };

                    notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
                    notifyIcon.Click += NotifyIcon_BalloonTipClicked;

                    notifyIcon.ShowBalloonTip(4000);
                    disposeTimer = new System.Threading.Timer(_ => OcultarNotificacion(), null, 5000, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Notificación local de carga a R2");
            }
        }

        private static void NotifyIcon_BalloonTipClicked(object? sender, EventArgs e)
        {
            NotificationClickInfo? detalle;

            lock (SyncRoot)
            {
                detalle = notificacionPendiente;
            }

            if (detalle == null)
                return;

            NotificationDetailRequested?.Invoke(null, detalle);
            OcultarNotificacion();
        }

        public static void RegistrarNotificacionSistema(int idNotificacion, string? titulo, string? nombreArchivo, string? direccion, string? rutaR2, string? descripcion)
        {
            try
            {
                R2NotificationCenter.AddNotification(idNotificacion, titulo, nombreArchivo, direccion, rutaR2, descripcion);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Notificación interna de carga detectada en R2");
            }
        }

        private static void OcultarNotificacion()
        {
            lock (SyncRoot)
            {
                disposeTimer?.Dispose();
                disposeTimer = null;

                if (notifyIcon != null)
                {
                    notifyIcon.BalloonTipClicked -= NotifyIcon_BalloonTipClicked;
                    notifyIcon.Click -= NotifyIcon_BalloonTipClicked;
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                    notifyIcon = null;
                }

                notificacionPendiente = null;
            }
        }
    }
}
