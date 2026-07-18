using System;
using System.Collections.Generic;
using System.Linq;

namespace Retorno360Tacna.HELPERS
{
    internal static class R2NotificationCenter
    {
        internal sealed class NotificationItem
        {
            public int Id { get; init; }
            public DateTime Fecha { get; init; }
            public string Titulo { get; init; } = string.Empty;
            public string Archivo { get; init; } = string.Empty;
            public string Direccion { get; init; } = string.Empty;
            public string RutaR2 { get; init; } = string.Empty;
            public string Descripcion { get; init; } = string.Empty;
            public bool Leida { get; set; }

            public string Mensaje => $"[{Fecha:dd/MM HH:mm}] {Titulo} - {Descripcion}";
        }

        private static readonly object SyncRoot = new();
        private static readonly List<NotificationItem> notificaciones = new();

        public static event EventHandler? NotificationsChanged;

        public static void AddNotification(int id, string? titulo, string? archivo, string? direccion, string? rutaR2, string? descripcion, bool leida = false)
        {
            string tituloNormalizado = string.IsNullOrWhiteSpace(titulo) ? "Nueva carga en R2" : titulo;
            string archivoNormalizado = string.IsNullOrWhiteSpace(archivo) ? "Archivo" : archivo;
            string direccionNormalizada = string.IsNullOrWhiteSpace(direccion) ? string.Empty : direccion;
            string rutaNormalizada = string.IsNullOrWhiteSpace(rutaR2) ? "R2" : rutaR2;
            string descripcionNormalizada = string.IsNullOrWhiteSpace(descripcion)
                ? $"{archivoNormalizado} - {rutaNormalizada}"
                : descripcion;

            lock (SyncRoot)
            {
                notificaciones.Insert(0, new NotificationItem
                {
                    Id = id,
                    Fecha = DateTime.Now,
                    Titulo = tituloNormalizado,
                    Archivo = archivoNormalizado,
                    Direccion = direccionNormalizada,
                    RutaR2 = rutaNormalizada,
                    Descripcion = descripcionNormalizada,
                    Leida = leida
                });

                if (notificaciones.Count > 200)
                {
                    notificaciones.RemoveRange(200, notificaciones.Count - 200);
                }
            }

            NotificationsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static IReadOnlyList<NotificationItem> GetNotifications()
        {
            lock (SyncRoot)
            {
                return notificaciones
                    .Select(n => new NotificationItem
                    {
                        Id = n.Id,
                        Fecha = n.Fecha,
                        Titulo = n.Titulo,
                        Archivo = n.Archivo,
                        Direccion = n.Direccion,
                        RutaR2 = n.RutaR2,
                        Descripcion = n.Descripcion,
                        Leida = n.Leida
                    })
                    .ToList();
            }
        }

        public static void ReplaceNotifications(IEnumerable<NotificationItem> items)
        {
            lock (SyncRoot)
            {
                notificaciones.Clear();
                notificaciones.AddRange(items.Select(n => new NotificationItem
                {
                    Id = n.Id,
                    Fecha = n.Fecha,
                    Titulo = n.Titulo,
                    Archivo = n.Archivo,
                    Direccion = n.Direccion,
                    RutaR2 = n.RutaR2,
                    Descripcion = n.Descripcion,
                    Leida = n.Leida
                }));
            }

            NotificationsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static int GetUnreadCount()
        {
            lock (SyncRoot)
            {
                return notificaciones.Count(n => !n.Leida);
            }
        }

        public static void MarkAllAsRead()
        {
            lock (SyncRoot)
            {
                foreach (var notificacion in notificaciones)
                {
                    notificacion.Leida = true;
                }
            }

            NotificationsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void MarkAsRead(int id)
        {
            lock (SyncRoot)
            {
                var notificacion = notificaciones.FirstOrDefault(n => n.Id == id);
                if (notificacion == null || notificacion.Leida)
                    return;

                notificacion.Leida = true;
            }

            NotificationsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ClearAll()
        {
            lock (SyncRoot)
            {
                notificaciones.Clear();
            }

            NotificationsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
