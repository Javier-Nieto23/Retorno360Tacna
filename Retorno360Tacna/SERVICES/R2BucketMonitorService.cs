using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Retorno360Tacna.SERVICES
{
    internal static class R2BucketMonitorService
    {
        private const string BucketName = "retorno360tacnaweb";
        private static readonly object SyncRoot = new();
        private static readonly System.Threading.SemaphoreSlim PollLock = new(1, 1);
        private static readonly Dictionary<string, DateTime> archivosR2Detectados = new(StringComparer.OrdinalIgnoreCase);
        private static readonly CloudflareR2Service cloudflareService = new(BucketName);
        private static readonly NotificacionService notificacionService = new();
        private static global::System.Threading.Timer? timerMonitoreo;
        private static bool inicializado;

        private static string ObtenerDireccionDesdeClaveArchivo(string claveArchivo)
        {
            string claveNormalizada = (claveArchivo ?? string.Empty).Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(claveNormalizada))
                return string.Empty;

            string[] segmentos = claveNormalizada.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segmentos.Length <= 1)
                return string.Empty;

            return string.Join("/", segmentos.Take(segmentos.Length - 1));
        }

        public static event EventHandler? FilesChanged;

        public static void Start()
        {
            lock (SyncRoot)
            {
                if (inicializado)
                    return;

                timerMonitoreo = new global::System.Threading.Timer(async _ => await VerificarNuevasCargasR2Async(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                inicializado = true;
            }
        }

        public static void Stop()
        {
            lock (SyncRoot)
            {
                timerMonitoreo?.Dispose();
                timerMonitoreo = null;
                inicializado = false;
                archivosR2Detectados.Clear();
            }
        }

        private static async Task VerificarNuevasCargasR2Async()
        {
            if (!await PollLock.WaitAsync(0))
                return;

            try
            {
                var archivosActuales = await cloudflareService.ListFileDetailsAsync();
                if (archivosActuales.Count == 0)
                    return;

                bool establecerLineaBase;
                lock (SyncRoot)
                {
                    establecerLineaBase = archivosR2Detectados.Count == 0;
                    if (establecerLineaBase)
                    {
                        foreach (var archivo in archivosActuales)
                        {
                            archivosR2Detectados[archivo.Key] = archivo.LastModifiedUtc;
                        }
                    }
                }

                if (establecerLineaBase)
                    return;

                List<CloudflareR2Service.R2FileInfo> archivosNuevos = new();

                lock (SyncRoot)
                {
                    foreach (var archivo in archivosActuales.OrderBy(x => x.LastModifiedUtc))
                    {
                        bool esNuevo = !archivosR2Detectados.TryGetValue(archivo.Key, out DateTime ultimaFechaRegistrada)
                            || archivo.LastModifiedUtc > ultimaFechaRegistrada;

                        archivosR2Detectados[archivo.Key] = archivo.LastModifiedUtc;

                        if (esNuevo)
                        {
                            archivosNuevos.Add(archivo);
                        }
                    }
                }

                if (archivosNuevos.Count == 0)
                    return;

                foreach (var archivo in archivosNuevos)
                {
                    string nombreArchivo = Path.GetFileName(archivo.Key);
                    string direccion = ObtenerDireccionDesdeClaveArchivo(archivo.Key);
                    string descripcion = $"{nombreArchivo} - {archivo.Key}";
                    int idNotificacion = notificacionService.RegistrarNotificacionYObtenerId("Nueva carga en R2", descripcion, direccion);
                    WindowsNotificationHelper.MostrarNotificacionCargaR2(idNotificacion, nombreArchivo, direccion, archivo.Key);
                    WindowsNotificationHelper.RegistrarNotificacionSistema(idNotificacion, "Nueva carga en R2", nombreArchivo, direccion, archivo.Key, descripcion);
                }

                FilesChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Monitoreo de nuevas cargas en R2");
            }
            finally
            {
                PollLock.Release();
            }
        }
    }
}
