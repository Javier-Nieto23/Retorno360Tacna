using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmSolicitud
    {
        private void ConfigurarVista()
        {
            BackColor = Color.FromArgb(245, 246, 250);
            lblTitulo.Text = "Detalle de solicitud de eliminación";
            btnExportar.Visible = false;
            btnCerrar.Text = "Cerrar";
            lblTotalRegistros.Text = "Información general de la solicitud seleccionada";

            panelHeader.BackColor = Color.FromArgb(52, 73, 94);
            panelFooter.BackColor = Color.FromArgb(236, 240, 241);
            lblTotalRegistros.ForeColor = Color.FromArgb(52, 73, 94);

            btnCerrar.BackColor = Color.FromArgb(231, 76, 60);
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;

            dgvDetalle.ReadOnly = true;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.AllowUserToResizeRows = false;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.ColumnHeadersHeight = 38;
            dgvDetalle.RowTemplate.Height = 32;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.MultiSelect = false;
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDetalle.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.BorderStyle = BorderStyle.None;
            dgvDetalle.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDetalle.GridColor = Color.FromArgb(226, 232, 240);
            dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvDetalle.EnableHeadersVisualStyles = false;
            dgvDetalle.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(52, 73, 94),
                SelectionForeColor = Color.White
            };
            dgvDetalle.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 9.5F),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(15, 23, 42),
                WrapMode = DataGridViewTriState.True
            };

            Paint += FrmSolicitud_Paint;
        }

        private void FrmSolicitud_Paint(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
