using System.Data;
using System.Globalization;
using Retorno360Tacna.HELPERS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetalleConciliacion : Form
    {
        private readonly DataTable detalleOriginal;
        private readonly string formaPago;
        private readonly string tipoReporte;
        private DataTable detalleFiltrado;

        public FrmDetalleConciliacion(DataTable detalle, string formaPago, string tipoReporte = "IGI")
        {
            InitializeComponent();
            this.detalleOriginal = detalle ?? new DataTable();
            this.formaPago = formaPago;
            this.tipoReporte = tipoReporte;
            this.detalleFiltrado = detalleOriginal.Copy();

            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1200, 700);
        }

        private void FrmDetalleConciliacion_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            FiltrarYCargarDatos();
            ActualizarTitulo();
        }

        private void ConfigurarGrid()
        {
            dgvDetalle.AutoGenerateColumns = true;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.ReadOnly = true;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.MultiSelect = false;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetalle.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDetalle.ColumnHeadersHeight = 40;
            dgvDetalle.EnableHeadersVisualStyles = false;
            dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
            dgvDetalle.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
            dgvDetalle.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDetalle.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvDetalle.RowTemplate.Height = 30;
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.BorderStyle = BorderStyle.None;
            DataGridViewManualCopyHelper.Configurar(dgvDetalle);
        }

        private void FiltrarYCargarDatos()
        {
            try
            {
                // Filtrar por forma de pago
                string columnaFormaPago = tipoReporte == "IGI" ? "FormaPago_IGI" : "FormaPago_IVA";

                var filasFiltradas = detalleOriginal.AsEnumerable()
                    .Where(r => r[columnaFormaPago]?.ToString()?.Trim() == formaPago?.Trim());

                if (filasFiltradas.Any())
                {
                    detalleFiltrado = filasFiltradas.CopyToDataTable();
                }
                else
                {
                    detalleFiltrado = detalleOriginal.Clone(); // Tabla vacía con misma estructura
                }

                dgvDetalle.DataSource = detalleFiltrado;

                // Formatear columnas de moneda
                FormatearColumnasMoneda();

                lblTotalRegistros.Text = $"Total registros: {detalleFiltrado.Rows.Count:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatearColumnasMoneda()
        {
            // Formatear columnas numéricas conocidas
            var columnasMoneda = new[] { "IGI_Pagado", "IGI_Calculado", "Diferencia_IGI", "IVA_Pagado", 
                                         "Gl_ImporteADvalorem", "Gl_ImporteIVA", "Valor_Aduana" };

            foreach (DataGridViewColumn col in dgvDetalle.Columns)
            {
                if (columnasMoneda.Contains(col.Name))
                {
                    col.DefaultCellStyle.Format = "C2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void ActualizarTitulo()
        {
            lblTitulo.Text = $"Detalle de {tipoReporte} - Forma de Pago: {formaPago}";
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
                    sfd.FileName = $"Detalle_{tipoReporte}_{formaPago}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        ExportarAExcel(sfd.FileName);
                        MessageBox.Show("Archivo exportado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarAExcel(string rutaArchivo)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Detalle");
                worksheet.Cell(1, 1).InsertTable(detalleFiltrado);
                workbook.SaveAs(rutaArchivo);
            }
        }
    }
}
