using System;
using System.IO;
using System.Text;

namespace Retorno360Tacna.HELPERS
{
    internal static class ErrorLogger
    {
        private const string NombreSoftware = "Retorno 360 Tacna";

        public static string LogError(Exception? exception, string contexto = "Error no controlado")
        {
            try
            {
                DateTime fechaError = DateTime.Now;
                string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Retorno360TacnaError.log");

                var contenido = new StringBuilder();
                contenido.AppendLine(new string('=', 80));
                contenido.AppendLine($"Software     : {NombreSoftware}");
                contenido.AppendLine($"Fecha y hora : {fechaError:dd/MM/yyyy HH:mm:ss}");
                contenido.AppendLine($"Contexto     : {contexto}");
                contenido.AppendLine($"Equipo       : {Environment.MachineName}");
                contenido.AppendLine($"Usuario      : {Environment.UserName}");
                contenido.AppendLine(new string('=', 80));
                contenido.AppendLine();

                if (exception != null)
                {
                    contenido.AppendLine("TIPO DE ERROR");
                    contenido.AppendLine(exception.GetType().FullName ?? "No disponible");
                    contenido.AppendLine();

                    contenido.AppendLine("MENSAJE");
                    contenido.AppendLine(exception.Message);
                    contenido.AppendLine();

                    if (exception.InnerException != null)
                    {
                        contenido.AppendLine("ERROR INTERNO");
                        contenido.AppendLine(exception.InnerException.Message);
                        contenido.AppendLine();
                    }

                    contenido.AppendLine("STACK TRACE");
                    contenido.AppendLine(exception.StackTrace ?? "No disponible");
                    contenido.AppendLine();
                }
                else
                {
                    contenido.AppendLine("No se recibió una excepción para registrar.");
                    contenido.AppendLine();
                }

                contenido.AppendLine(new string('=', 80));

                File.AppendAllText(rutaArchivo, contenido.ToString() + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
                return rutaArchivo;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
