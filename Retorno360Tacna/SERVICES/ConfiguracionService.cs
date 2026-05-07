using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Retorno360Tacna.SERVICES
{
    public static class ConfiguracionService
    {
        private static string RutaConfiguracion => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Retorno360Tacna",
            "config.txt");

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
