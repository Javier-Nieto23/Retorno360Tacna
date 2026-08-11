using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmCumplimiento
    {
        private void ConfigurarEstiloVisual()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(239, 244, 248);

            panelFiltros.BackColor = Color.White;
            panelResultados.BackColor = Color.FromArgb(239, 244, 248);

            dgvPreview.BorderStyle = BorderStyle.FixedSingle;
            dgvPreview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPreview.BackgroundColor = Color.White;
            dgvPreview.EnableHeadersVisualStyles = false;
            dgvPreview.RowTemplate.Height = 32;
            dgvPreview.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(239, 246, 255),
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvPreview.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.FromArgb(15, 23, 42)
            };

            ConfigurarBoton(btnGenerar, Color.FromArgb(52, 152, 219));
            ConfigurarBoton(btnGuardarPortal, Color.FromArgb(46, 125, 50));
            ConfigurarBoton(btnExportarExcel, Color.FromArgb(142, 68, 173));
        }

        private static void ConfigurarBoton(Button boton, Color color)
        {
            boton.BackColor = color;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.ForeColor = Color.White;
            boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
        }

        private void FormatearGrid()
        {
            if (dgvPreview.Columns.Contains("PERIODO"))
            {
                dgvPreview.Columns["PERIODO"].DefaultCellStyle.Format = "yyyy-MM";
            }

            foreach (var nombreColumna in new[] { "IGI_PAGADO", "IGI_CALCULADO", "AHORRO_IGI", "PAGO_IVA", "AHORRO_IVA" })
            {
                if (dgvPreview.Columns.Contains(nombreColumna))
                {
                    dgvPreview.Columns[nombreColumna].DefaultCellStyle.Format = "N2";
                    dgvPreview.Columns[nombreColumna].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            if (dgvPreview.Columns.Contains("OPERACIONES"))
            {
                dgvPreview.Columns["OPERACIONES"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void ActualizarResumen(DataTable tabla)
        {
            int registros = tabla.Rows.Count;
            int operaciones = tabla.Rows.Cast<DataRow>().Sum(r => r["OPERACIONES"] == DBNull.Value ? 0 : System.Convert.ToInt32(r["OPERACIONES"]));
            decimal igiPagado = tabla.Rows.Cast<DataRow>().Sum(r => r["IGI_PAGADO"] == DBNull.Value ? 0m : System.Convert.ToDecimal(r["IGI_PAGADO"]));
            decimal pagoIva = tabla.Rows.Cast<DataRow>().Sum(r => r["PAGO_IVA"] == DBNull.Value ? 0m : System.Convert.ToDecimal(r["PAGO_IVA"]));

            lblResumen.Text = $"Registros: {registros} | Operaciones: {operaciones:N0} | IGI Pagado: {igiPagado:N2} | Pago IVA: {pagoIva:N2}";
        }
    }
}
