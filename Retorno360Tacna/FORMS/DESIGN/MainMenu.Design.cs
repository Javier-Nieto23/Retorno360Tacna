using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class MainMenu
    {
        private void PersonalizarBarraTitulo()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            {
                int colorBGR = 0x00503E2C;
                DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, ref colorBGR, sizeof(int));
            }
        }

        private void ConfigurarCierrePanelNotificaciones()
        {
            RegistrarEventosClickRecursivos(this);
        }

        private void RegistrarEventosClickRecursivos(Control control)
        {
            control.MouseDown -= CerrarPanelNotificacionesPorClickExterno;
            control.MouseDown += CerrarPanelNotificacionesPorClickExterno;

            control.ControlAdded -= Control_AddedRegistrarClickNotificaciones;
            control.ControlAdded += Control_AddedRegistrarClickNotificaciones;

            foreach (Control hijo in control.Controls)
            {
                RegistrarEventosClickRecursivos(hijo);
            }
        }

        private void Control_AddedRegistrarClickNotificaciones(object? sender, ControlEventArgs e)
        {
            RegistrarEventosClickRecursivos(e.Control);
        }

        private void CerrarPanelNotificacionesPorClickExterno(object? sender, MouseEventArgs e)
        {
            if (!panelNotificacionesVisible)
                return;

            if (sender is not Control controlOrigen)
            {
                OcultarPanelNotificaciones();
                return;
            }

            if (EsControlDentroDe(controlOrigen, panelNotificaciones) ||
                EsControlDentroDe(controlOrigen, panelNotificacionesPico) ||
                EsControlDentroDe(controlOrigen, btnNotificaciones))
            {
                return;
            }

            Point puntoPantalla = controlOrigen.PointToScreen(e.Location);

            if (ControlContienePuntoPantalla(panelNotificaciones, puntoPantalla) ||
                ControlContienePuntoPantalla(panelNotificacionesPico, puntoPantalla) ||
                ControlContienePuntoPantalla(btnNotificaciones, puntoPantalla))
            {
                return;
            }

            OcultarPanelNotificaciones();
        }

        private static bool EsControlDentroDe(Control? control, Control contenedor)
        {
            while (control != null)
            {
                if (control == contenedor)
                    return true;

                control = control.Parent;
            }

            return false;
        }

        private static bool ControlContienePuntoPantalla(Control control, Point puntoPantalla)
        {
            return control.Visible && control.RectangleToScreen(control.ClientRectangle).Contains(puntoPantalla);
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn != botonActivo)
            {
                btn.BackColor = Color.FromArgb(52, 73, 94);
            }

            AplicarImagenHover(btn, true);
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn != botonActivo)
            {
                btn.BackColor = Color.FromArgb(44, 62, 80);
            }

            AplicarImagenHover(btn, false);
        }

        private void ActivarBoton(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = Color.FromArgb(44, 62, 80);
                AplicarImagenHover(botonActivo, false);
            }

            botonActivo = boton;
            boton.BackColor = Color.FromArgb(193, 39, 45);
            AplicarImagenHover(boton, true);
        }

        private void InicializarImagenesHoverMenu()
        {
            foreach (Button boton in ObtenerBotonesMenuConIcono())
            {
                if (!imagenesOriginalesMenu.ContainsKey(boton))
                {
                    imagenesOriginalesMenu[boton] = boton.Image;
                }

                if (!imagenesHoverMenu.ContainsKey(boton))
                {
                    imagenesHoverMenu[boton] = CrearImagenAclarada(boton.Image);
                }
            }
        }

        private IEnumerable<Button> ObtenerBotonesMenuConIcono()
        {
            yield return btnDiagramas;
            yield return btnAdministracion;
            yield return btnInventarios;
            yield return btnReportes;
            yield return btnConfiguracion;
            yield return btnCerrarSesion;
            yield return btnSubMenuPorcentaje;
            yield return btnSubMenuReporteIGI;
            yield return btnSubMenuContabilidad;
            yield return btnCatalogoPartes;
            yield return btnReportesInventario;
            yield return btnToggleSidebar;
        }

        private void AplicarImagenHover(Button boton, bool hover)
        {
            if (!imagenesOriginalesMenu.TryGetValue(boton, out Image? imagenOriginal))
                return;

            if (hover)
            {
                if (imagenesHoverMenu.TryGetValue(boton, out Image? imagenHover) && imagenHover != null)
                {
                    boton.Image = imagenHover;
                    return;
                }
            }

            boton.Image = imagenOriginal;
        }

        private static Image? CrearImagenAclarada(Image? imagenOriginal)
        {
            if (imagenOriginal == null)
                return null;

            Bitmap bitmap = new Bitmap(imagenOriginal.Width, imagenOriginal.Height);

            using Graphics graphics = Graphics.FromImage(bitmap);
            using ImageAttributes atributos = new ImageAttributes();

            ColorMatrix matriz = new ColorMatrix(new float[][]
            {
                new float[] { 1.25f, 0, 0, 0, 0 },
                new float[] { 0, 1.25f, 0, 0, 0 },
                new float[] { 0, 0, 1.25f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0.08f, 0.08f, 0.08f, 0, 1 }
            });

            atributos.SetColorMatrix(matriz);
            graphics.DrawImage(
                imagenOriginal,
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                imagenOriginal.Width,
                imagenOriginal.Height,
                GraphicsUnit.Pixel,
                atributos);

            return bitmap;
        }

        private void AplicarFormaPicoNotificaciones()
        {
            using System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            Point[] triangle =
            {
                new Point(panelNotificacionesPico.Width / 2, 0),
                new Point(6, panelNotificacionesPico.Height - 1),
                new Point(panelNotificacionesPico.Width - 6, panelNotificacionesPico.Height - 1)
            };

            path.AddPolygon(triangle);
            panelNotificacionesPico.Region = new Region(path);
        }

        private void panelNotificacionesPico_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using SolidBrush brush = new(Color.White);
            using Pen pen = new(Color.FromArgb(209, 213, 219));

            Point[] triangle =
            {
                new Point(panelNotificacionesPico.Width / 2, 0),
                new Point(6, panelNotificacionesPico.Height - 1),
                new Point(panelNotificacionesPico.Width - 6, panelNotificacionesPico.Height - 1)
            };

            e.Graphics.FillPolygon(brush, triangle);
            e.Graphics.DrawPolygon(pen, triangle);
        }

        private void AplicarEscalaMenuLateral()
        {
            float factor = Math.Max(1f, (float)escalaUiActual);

            anchoSidebarExpandidoActual = (int)Math.Round(ANCHO_SIDEBAR_EXPANDIDO * factor);
            anchoSidebarColapsadoActual = Math.Max(60, (int)Math.Round(ANCHO_SIDEBAR_COLAPSADO * factor));

            panelSidebar.Width = sidebarColapsado ? anchoSidebarColapsadoActual : anchoSidebarExpandidoActual;
            pictureBox1.Visible = !sidebarColapsado;
            panelSubMenuAdmin.Height = menuAdminExpandido ? ObtenerAlturaSubmenu(panelSubMenuAdmin) : 0;
            panelSubMenuInventarios.Height = menuInventariosExpandido ? ObtenerAlturaSubmenu(panelSubMenuInventarios) : 0;
            ReposicionarPanelNotificaciones();
        }

        private static int ObtenerAlturaSubmenu(Panel panelSubMenu)
        {
            return panelSubMenu.Controls
                .OfType<Control>()
                .Where(control => control.Visible)
                .Sum(control => control.Height);
        }
    }
}
