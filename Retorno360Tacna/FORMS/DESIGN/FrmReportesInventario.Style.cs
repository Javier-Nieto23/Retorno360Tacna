using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportesInventario
    {
        private void ConfigurarEstiloVisual()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(239, 244, 248);

            panelFiltros.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawLine(pen, 0, panelFiltros.Height - 1, panelFiltros.Width, panelFiltros.Height - 1);
            };

            panelCabeceraContenido.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(pen, 0, 0, panelCabeceraContenido.Width - 1, panelCabeceraContenido.Height - 1);
            };

            panelArchivosObservados.Paint += (_, e) => DibujarTarjeta(e, panelArchivosObservados, Color.FromArgb(219, 234, 254));
            panelMensajes.Paint += (_, e) => DibujarTarjeta(e, panelMensajes, Color.FromArgb(226, 232, 240));
            panelSolicitudesEliminacion.Paint += (_, e) => DibujarTarjeta(e, panelSolicitudesEliminacion, Color.FromArgb(254, 226, 226));
            panelCabeceraContenido.Paint += (_, e) => DibujarTarjeta(e, panelCabeceraContenido, Color.FromArgb(226, 232, 240));

            lvCarpetas.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            lvCarpetas.ForeColor = Color.FromArgb(30, 41, 59);
            lvCarpetas.BackColor = Color.White;
            lvCarpetas.HideSelection = false;
            lvCarpetas.FullRowSelect = true;

            dgvEstadoMensual.BorderStyle = BorderStyle.FixedSingle;
            dgvEstadoMensual.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvEstadoMensual.RowTemplate.Height = 34;
            dgvEstadoMensual.BackgroundColor = Color.White;
            dgvEstadoMensual.EnableHeadersVisualStyles = false;
            dgvEstadoMensual.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(239, 246, 255),
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvEstadoMensual.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(15, 23, 42)
            };

            dgvArchivosObservados.BorderStyle = BorderStyle.FixedSingle;
            dgvArchivosObservados.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvArchivosObservados.EnableHeadersVisualStyles = false;
            dgvArchivosObservados.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(224, 247, 250),
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvArchivosObservados.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(15, 23, 42)
            };
            dgvArchivosObservados.RowTemplate.Height = 30;
            dgvArchivosObservados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvArchivosObservados.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvArchivosObservados.CellClick += dgvArchivosObservados_CellClick;

            dgvSolicitudesEliminacion.BorderStyle = BorderStyle.FixedSingle;
            dgvSolicitudesEliminacion.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvSolicitudesEliminacion.EnableHeadersVisualStyles = false;
            dgvSolicitudesEliminacion.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(254, 242, 242),
                ForeColor = Color.FromArgb(127, 29, 29),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvSolicitudesEliminacion.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(254, 226, 226),
                SelectionForeColor = Color.FromArgb(127, 29, 29)
            };
            dgvSolicitudesEliminacion.RowTemplate.Height = 30;
            dgvSolicitudesEliminacion.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSolicitudesEliminacion.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvSolicitudesEliminacion.CellClick += dgvSolicitudesEliminacion_CellClick;

            txtMensajeObservacion.BackColor = Color.FromArgb(248, 250, 252);
            txtMensajeObservacion.BorderStyle = BorderStyle.FixedSingle;
            btnEnviarMensajeObservacion.Text = "Enviar";
            ActualizarBotonesArchivosObservados();
        }

        private static void DibujarTarjeta(PaintEventArgs e, Control control, Color borde)
        {
            Rectangle rectangulo = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
            using var brocha = new SolidBrush(Color.White);
            using var pluma = new Pen(borde);
            e.Graphics.FillRectangle(brocha, rectangulo);
            e.Graphics.DrawRectangle(pluma, rectangulo);
        }

        private void AjustarLayoutVistaInventario()
        {
            const int margenHorizontal = 18;
            const int margenSuperior = 122;
            const int espacio = 16;
            const int alturaEtiqueta = 24;
            const int alturaMinimaGrafica = 220;
            const int alturaMinimaTabla = 200;
            const int alturaMinimaSolicitudes = 150;

            int anchoDisponible = Math.Max(860, panelContenido.ClientSize.Width - (margenHorizontal * 2));
            int altoDisponible = Math.Max(520, panelContenido.ClientSize.Height - margenSuperior - 15);

            int anchoPanelDerecho = Math.Max(520, (int)(anchoDisponible * 0.46));
            int anchoListado = Math.Max(220, Math.Min(280, (int)(anchoDisponible * 0.18)));
            int anchoCentro = Math.Max(300, anchoDisponible - anchoListado - anchoPanelDerecho - (espacio * 2));

            if (anchoCentro > 360)
            {
                int excedenteCentro = anchoCentro - 360;
                anchoCentro = 360;
                anchoPanelDerecho += excedenteCentro;
            }

            if (anchoCentro < 300)
            {
                int faltanteCentro = 300 - anchoCentro;
                anchoCentro = 300;
                anchoPanelDerecho = Math.Max(420, anchoPanelDerecho - faltanteCentro);
            }

            int alturaListado = Math.Max(200, altoDisponible - 42);
            int alturaPanelObservaciones = Math.Max(180, Math.Min(235, (int)(altoDisponible * 0.27)));
            int alturaPanelSolicitudes = Math.Max(alturaMinimaSolicitudes, Math.Min(175, (int)(altoDisponible * 0.19)));
            int alturaChat = Math.Max(240, altoDisponible - alturaPanelObservaciones - alturaPanelSolicitudes - 24);
            int alturaGrafica = Math.Max(alturaMinimaGrafica, (int)(altoDisponible * 0.48));
            int alturaTabla = Math.Max(alturaMinimaTabla, altoDisponible - alturaGrafica - alturaEtiqueta - espacio - 6);

            lvCarpetas.Location = new Point(margenHorizontal, margenSuperior);
            lvCarpetas.Size = new Size(anchoListado, alturaListado);

            lblTotalCarpetas.Location = new Point(margenHorizontal, lvCarpetas.Bottom + 8);
            lblTotalCarpetas.Size = new Size(anchoListado, lblTotalCarpetas.Height);

            int xPanelCentro = lvCarpetas.Right + espacio;
            panelArchivosObservados.Location = new Point(xPanelCentro, margenSuperior);
            panelArchivosObservados.Size = new Size(anchoCentro, alturaPanelObservaciones);
            dgvArchivosObservados.Location = new Point(14, 46);
            dgvArchivosObservados.Size = new Size(panelArchivosObservados.Width - 28, panelArchivosObservados.Height - 58);
            lblArchivosObservadosEstado.Size = new Size(panelArchivosObservados.Width - 28, lblArchivosObservadosEstado.Height);

            panelSolicitudesEliminacion.Location = new Point(xPanelCentro, panelArchivosObservados.Bottom + 12);
            panelSolicitudesEliminacion.Size = new Size(anchoCentro, alturaPanelSolicitudes);
            dgvSolicitudesEliminacion.Location = new Point(14, 46);
            dgvSolicitudesEliminacion.Size = new Size(panelSolicitudesEliminacion.Width - 28, Math.Max(72, panelSolicitudesEliminacion.Height - 58));
            lblSolicitudesEliminacionEstado.Size = new Size(panelSolicitudesEliminacion.Width - 28, lblSolicitudesEliminacionEstado.Height);

            panelMensajes.Location = new Point(xPanelCentro, panelSolicitudesEliminacion.Bottom + 12);
            panelMensajes.Size = new Size(anchoCentro, alturaChat);
            rtbMensajesObservacion.Location = new Point(14, 44);
            rtbMensajesObservacion.Size = new Size(panelMensajes.Width - 28, Math.Max(140, panelMensajes.Height - 98));
            lblMensajesEstado.Size = new Size(panelMensajes.Width - 28, lblMensajesEstado.Height);
            txtMensajeObservacion.Location = new Point(14, panelMensajes.Height - 38);
            txtMensajeObservacion.Size = new Size(Math.Max(160, panelMensajes.Width - 116), 28);
            btnEnviarMensajeObservacion.Location = new Point(txtMensajeObservacion.Right + 8, panelMensajes.Height - 39);
            btnEnviarMensajeObservacion.Size = new Size(80, 30);

            int xPanelDerecho = panelMensajes.Right + espacio;

            lblExploradorTitulo.Location = new Point(margenHorizontal, 96);
            lblResumenTitulo.Location = new Point(xPanelDerecho, 96);

            chartCargaMensual.Location = new Point(xPanelDerecho, margenSuperior);
            chartCargaMensual.Size = new Size(anchoPanelDerecho, alturaGrafica);
            chartCargaMensual.BringToFront();
            ReposicionarEtiquetasAniosGrafica();

            lblEstadoMensual.Location = new Point(xPanelDerecho, chartCargaMensual.Bottom + espacio);
            lblEstadoMensual.Size = new Size(anchoPanelDerecho, alturaEtiqueta);
            lblEstadoMensual.BringToFront();

            dgvEstadoMensual.Location = new Point(xPanelDerecho, lblEstadoMensual.Bottom + 6);
            dgvEstadoMensual.Size = new Size(anchoPanelDerecho, alturaTabla);
            dgvEstadoMensual.BringToFront();
        }
    }
}
