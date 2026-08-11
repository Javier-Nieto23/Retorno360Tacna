using System;
using System.Drawing;
using Npgsql;
using System.IO;
using System.Windows.Forms;

namespace Retorno360Tacna.SERVICES
{
    public static class ConfiguracionService
    {
        private static readonly Size ResolucionLogicaObjetivo = new(1536, 864);
        private static readonly Size ResolucionMinimaTrabajo = new(1280, 720);

        private static string RutaConfiguracion => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Retorno360Tacna",
            "config.txt");

        public static void AplicarPerfilPantallaLogica(Form formulario, bool ocuparAreaTrabajo = false)
        {
            try
            {
                formulario.AutoScaleMode = AutoScaleMode.Dpi;

                if (!formulario.TopLevel)
                    return;

                var areaTrabajo = Screen.FromPoint(Cursor.Position).WorkingArea;
                if (areaTrabajo.Width <= 0 || areaTrabajo.Height <= 0)
                    areaTrabajo = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, ResolucionLogicaObjetivo.Width, ResolucionLogicaObjetivo.Height);

                formulario.MinimumSize = new Size(
                    Math.Min(areaTrabajo.Width, ResolucionMinimaTrabajo.Width),
                    Math.Min(areaTrabajo.Height, ResolucionMinimaTrabajo.Height));

                if (ocuparAreaTrabajo)
                {
                    formulario.WindowState = FormWindowState.Normal;
                    formulario.StartPosition = FormStartPosition.Manual;
                    formulario.Bounds = areaTrabajo;
                    return;
                }





                float factorDpi = 1f;
                using (var graphics = formulario.CreateGraphics())
                {
                    factorDpi = Math.Clamp(graphics.DpiX / 96f, 1f, 1.25f);
                }

                int anchoObjetivo = Math.Min(areaTrabajo.Width, (int)Math.Round(formulario.ClientSize.Width * factorDpi));
                int altoObjetivo = Math.Min(areaTrabajo.Height, (int)Math.Round(formulario.ClientSize.Height * factorDpi));

                anchoObjetivo = Math.Min(anchoObjetivo, ResolucionLogicaObjetivo.Width);
                altoObjetivo = Math.Min(altoObjetivo, ResolucionLogicaObjetivo.Height);

                formulario.WindowState = FormWindowState.Normal;
                formulario.StartPosition = FormStartPosition.Manual;
                formulario.ClientSize = new Size(anchoObjetivo, altoObjetivo);
                formulario.Location = new Point(
                    areaTrabajo.Left + Math.Max(0, (areaTrabajo.Width - anchoObjetivo) / 2),
                    areaTrabajo.Top + Math.Max(0, (areaTrabajo.Height - altoObjetivo) / 2));
            }
            catch
            {
                // Ignorar errores de adaptación visual
            }
        }

        public static bool ObtenerAjusteVentanaPantallaLogica()
        {
            try
            {
                if (File.Exists(RutaConfiguracion))
                {
                    var lineas = File.ReadAllLines(RutaConfiguracion);
                    foreach (var linea in lineas)
                    {
                        if (linea.StartsWith("AjustarPantallaLogica="))
                        {
                            string valor = linea.Replace("AjustarPantallaLogica=", "").Trim();
                            if (bool.TryParse(valor, out bool ajustar))
                                return ajustar;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Construye una cadena de conexión compatible con Npgsql a partir de la variable
        /// de entorno DATABASE_URL o RAILWAY_DATABASE_URL (formato de Railway).
        /// Si no existe la variable, devuelve la conexión local por defecto.
        /// </summary>
        public static string GetRailwayConnectionString()
        {
            string databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                                 ?? Environment.GetEnvironmentVariable("RAILWAY_DATABASE_URL");

            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                // Intentar leer del almacén seguro local
                if (SecretStoreService.TryGetSecret("DATABASE_URL", out string stored))
                {
                    databaseUrl = stored;
                }
                else
                {
                    throw new InvalidOperationException("La variable de entorno DATABASE_URL o RAILWAY_DATABASE_URL no está definida y no existe un secreto guardado. Configure la conexión a Railway antes de iniciar la aplicación.");
                }
            }

            try
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');

                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
                    Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                    Database = uri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true,
                    Timeout = 5,
                    CommandTimeout = 5
                };

                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("La variable DATABASE_URL tiene un formato inválido.", ex);
            }
        }

        public static void AplicarEscalaUI()
        {
            try
            {
                decimal escala = ObtenerEscalaUI();

                if (escala != 1.0m)
                {
                    // La escala se aplica individualmente a cada formulario cuando se abre
                }
            }
            catch
            {
                // Si hay error, usar escala predeterminada
            }
        }

        public static decimal ObtenerEscalaUI()
        {
            try
            {
                if (File.Exists(RutaConfiguracion))
                {
                    var lineas = File.ReadAllLines(RutaConfiguracion);
                    foreach (var linea in lineas)
                    {
                        if (linea.StartsWith("EscalaUI="))
                        {
                            string valor = linea.Replace("EscalaUI=", "").Trim();
                            if (decimal.TryParse(valor, out decimal escala))
                            {
                                return escala;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores y usar escala predeterminada
            }

            return 1.0m; // Escala predeterminada 100%
        }

        public static void AplicarEscalaFormulario(Form formulario, decimal escala)
        {
            if (escala == 1.0m) return;

            try
            {
                formulario.SuspendLayout();

                float factor = (float)escala;

                // Escalar el formulario base
                formulario.AutoScaleMode = AutoScaleMode.None;

                // Escalar recursivamente todos los controles
                EscalarControl(formulario, factor);

                formulario.ResumeLayout(true);
            }
            catch
            {
                // Ignorar errores de escalado
            }
        }

        private static void EscalarControl(Control control, float factor)
        {
            try
            {
                // Escalar tamaño del control
                control.Size = new Size(
                    (int)(control.Size.Width * factor),
                    (int)(control.Size.Height * factor)
                );

                // Escalar ubicación del control
                control.Location = new Point(
                    (int)(control.Location.X * factor),
                    (int)(control.Location.Y * factor)
                );

                // Escalar fuente
                if (control.Font != null)
                {
                    control.Font = new Font(
                        control.Font.FontFamily,
                        control.Font.Size * factor,
                        control.Font.Style
                    );
                }

                // Escalar padding
                control.Padding = new Padding(
                    (int)(control.Padding.Left * factor),
                    (int)(control.Padding.Top * factor),
                    (int)(control.Padding.Right * factor),
                    (int)(control.Padding.Bottom * factor)
                );

                // Escalar margin
                control.Margin = new Padding(
                    (int)(control.Margin.Left * factor),
                    (int)(control.Margin.Top * factor),
                    (int)(control.Margin.Right * factor),
                    (int)(control.Margin.Bottom * factor)
                );

                // Escalar imágenes en botones
                if (control is Button btn && btn.Image != null)
                {
                    int nuevoAncho = (int)(btn.Image.Width * factor);
                    int nuevoAlto = (int)(btn.Image.Height * factor);

                    var imagenOriginal = btn.Image;
                    btn.Image = new Bitmap(imagenOriginal, nuevoAncho, nuevoAlto);
                }

                // Escalar imágenes en PictureBox
                if (control is PictureBox pb && pb.Image != null && pb.SizeMode == PictureBoxSizeMode.Normal)
                {
                    int nuevoAncho = (int)(pb.Image.Width * factor);
                    int nuevoAlto = (int)(pb.Image.Height * factor);

                    var imagenOriginal = pb.Image;
                    pb.Image = new Bitmap(imagenOriginal, nuevoAncho, nuevoAlto);
                }

                // Escalar DataGridView
                if (control is DataGridView dgv)
                {
                    dgv.RowTemplate.Height = (int)(dgv.RowTemplate.Height * factor);
                    dgv.ColumnHeadersHeight = (int)(dgv.ColumnHeadersHeight * factor);

                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        col.Width = (int)(col.Width * factor);
                    }
                }

                // Recursivamente escalar controles hijos
                foreach (Control hijo in control.Controls)
                {
                    EscalarControl(hijo, factor);
                }
            }
            catch
            {
                // Ignorar errores en controles individuales
            }
        }
    }
}
