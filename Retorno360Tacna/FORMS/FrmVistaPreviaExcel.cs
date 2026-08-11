using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmVistaPreviaExcel : Form
    {
        private readonly string _rutaArchivo;
        private readonly string _nombreHoja;


        public FrmVistaPreviaExcel(string rutaArchivo, string nombreHoja)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            _rutaArchivo = rutaArchivo;
            _nombreHoja = nombreHoja;

            Text = $"Vista previa: {Path.GetFileName(rutaArchivo)} - [{nombreHoja}]";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(900, 500);

            CargarDatosExcel();
        }

        private void CargarDatosExcel()
        {
            try
            {
                if (!File.Exists(_rutaArchivo))
                {
                    MessageBox.Show("El archivo no existe en la ruta especificada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using XLWorkbook workbook = new XLWorkbook(_rutaArchivo);
                IXLWorksheet hoja = workbook.Worksheets.FirstOrDefault(ws => string.Equals(ws.Name, _nombreHoja, StringComparison.OrdinalIgnoreCase))
                                    ?? workbook.Worksheets.First();

                DataTable dt = new DataTable();

                // Detectar rango usado en la hoja
                var rango = hoja.RangeUsed();
                if (rango == null)
                {
                    MessageBox.Show("La hoja seleccionada está vacía.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int primeraFila = rango.FirstRow().RowNumber();
                int ultimaFila = rango.LastRow().RowNumber();
                int primeraColumna = rango.FirstColumn().ColumnNumber();
                int ultimaColumna = rango.LastColumn().ColumnNumber();

                // Crear columnas del DataTable usando la primera fila como encabezado
                for (int col = primeraColumna; col <= ultimaColumna; col++)
                {
                    string nombreColumna = hoja.Cell(primeraFila, col).GetString().Trim();
                    if (string.IsNullOrEmpty(nombreColumna))
                        nombreColumna = $"Columna {col}";

                    // Evitar nombres de columnas duplicados en DataTable
                    string colFinal = nombreColumna;
                    int contador = 1;
                    while (dt.Columns.Contains(colFinal))
                    {
                        colFinal = $"{nombreColumna}_{contador++}";
                    }

                    dt.Columns.Add(colFinal);
                }

                // Cargar filas de datos
                for (int fila = primeraFila + 1; fila <= ultimaFila; fila++)
                {
                    DataRow dr = dt.NewRow();
                    for (int col = primeraColumna; col <= ultimaColumna; col++)
                    {
                        int indiceColumnaDT = col - primeraColumna;
                        dr[indiceColumnaDT] = hoja.Cell(fila, col).GetString();
                    }
                    dt.Rows.Add(dr);
                }

                dgvPreview.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la vista previa: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            // Lógica para exportar el contenido del DataGridView a Excel
        }

        private void FrmVistaPreviaExcel_Load(object sender, EventArgs e)
        {
            // Lógica al cargar el formulario
        }
    }
}
