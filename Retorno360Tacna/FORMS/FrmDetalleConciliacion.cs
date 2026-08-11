using System.Data;
using Retorno360Tacna.HELPERS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetalleConciliacion : Form
    {
        private readonly DataTable detalleOriginal;
        private readonly string formaPago;
        private readonly string periodo;
        private readonly int anioSeleccionado;
        private readonly int mesSeleccionado;
        private readonly string tipoReporte;
        private DataTable detalleFiltrado;

        public FrmDetalleConciliacion(DataTable detalle, string formaPago, string tipoReporte)
            : this(detalle, formaPago, string.Empty, 0, 0, tipoReporte)
        {
        }

        public FrmDetalleConciliacion(DataTable detalle, string formaPago, string periodo = "", int anioSeleccionado = 0, int mesSeleccionado = 0, string tipoReporte = "IGI")
        {
            InitializeComponent();
            this.detalleOriginal = detalle ?? new DataTable();
            this.formaPago = formaPago;
            this.periodo = periodo;
            this.anioSeleccionado = anioSeleccionado;
            this.mesSeleccionado = mesSeleccionado;
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
                // Filtrar por forma de pago y período
                string columnaFormaPago = tipoReporte == "IGI" ? "FormaPago_IGI" : "FormaPago_IVA";

                var filasFiltradas = detalleOriginal.AsEnumerable()
                    .Where(r => r[columnaFormaPago]?.ToString()?.Trim() == formaPago?.Trim())
                    .Where(CorrespondeAlPeriodoSeleccionado);

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
                ErrorMessageHelper.ShowError($"Error al cargar datos: {ex.Message}", "Error", ex, "Carga de detalle de conciliación");
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
            lblTitulo.Text = string.IsNullOrWhiteSpace(periodo)
                ? $"Detalle de {tipoReporte} - Forma de Pago: {formaPago}"
                : $"Detalle de {tipoReporte} - Forma de Pago: {formaPago} - {periodo}";
        }

        private bool CorrespondeAlPeriodoSeleccionado(DataRow row)
        {
            if (anioSeleccionado <= 0 || mesSeleccionado <= 0 || !row.Table.Columns.Contains("FechaPago"))
                return true;

            if (row["FechaPago"] == DBNull.Value)
                return false;

            DateTime fechaPago;

            try
            {
                fechaPago = Convert.ToDateTime(row["FechaPago"]);
            }
            catch
            {
                return false;
            }

            return fechaPago.Year == anioSeleccionado && fechaPago.Month == mesSeleccionado;
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
                ErrorMessageHelper.ShowError($"Error al exportar: {ex.Message}", "Error", ex, "Exportación de detalle de conciliación");
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
