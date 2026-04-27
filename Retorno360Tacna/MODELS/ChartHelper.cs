using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace Retorno360Tacna.MODELS
{
    public static class ChartHelper
    {
        public static SKBitmap? CapturarGraficaLineal(CartesianChart chart)
        {
            if (chart == null || chart.Width <= 0 || chart.Height <= 0)
                return null;

            return CapturarControl(chart);
        }

        public static SKBitmap? CapturarGraficaCircular(PieChart chart)
        {
            if (chart == null || chart.Width <= 0 || chart.Height <= 0)
                return null;

            return CapturarControl(chart);
        }

        public static SKBitmap? CapturarControl(Control control)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0)
                return null;

            try
            {
                // Crear un bitmap de System.Drawing
                using (var bmp = new Bitmap(control.Width, control.Height))
                {
                    // Dibujar el control en el bitmap
                    control.DrawToBitmap(bmp, new Rectangle(0, 0, control.Width, control.Height));

                    // Convertir a SKBitmap
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        ms.Position = 0;

                        var skBitmap = SKBitmap.Decode(ms);
                        return skBitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al capturar control: {ex.Message}");
                return null;
            }
        }
    }
}
