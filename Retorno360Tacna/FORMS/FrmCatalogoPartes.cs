using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ClosedXML.Excel;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmCatalogoPartes : Form
    {
        private readonly ConexionInfo conexionActual;
        private CatalogoPartesService catalogoService;
        private List<ParteBOM> catalogoActual = new List<ParteBOM>();

        public FrmCatalogoPartes(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
        }

        private void FrmCatalogoPartes_Load(object sender, EventArgs e)
        {
            ConfigurarGraficoBarras();
            CargarRazonesSociales();

            // Configurar fechas por defecto (último mes)
            dtpFechaFin.Value = DateTime.Now;
            dtpFechaInicio.Value = DateTime.Now.AddMonths(-1);
        }

        private void ConfigurarGraficoBarras()
        {
            chartCatalogo.LegendPosition = LiveChartsCore.Measure.LegendPosition.Right;
            chartCatalogo.LegendTextSize = 14;
            chartCatalogo.LegendTextPaint = new SolidColorPaint(new SKColor(50, 50, 50));
        }

        private void CargarRazonesSociales()
        {
            try
            {
                var razones = catalogoService.ObtenerRazonesSociales();

                cboRazonSocial.DataSource = razones;
                cboRazonSocial.DisplayMember = "NombreRazon";
                cboRazonSocial.ValueMember = "IdRazon";

                if (razones.Any())
                {
                    cboRazonSocial.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedValue != null && cboRazonSocial.SelectedValue is int idRazon)
            {
                CargarBasesDatos(idRazon);
            }
        }

        private void CargarBasesDatos(int idRazon)
        {
            try
            {
                List<string> basesDatos = catalogoService.ObtenerBasesDatosRazon(idRazon);

                if (basesDatos.Count > 0)
                {
                    cboBaseDatos.DataSource = basesDatos;
                    cboBaseDatos.Enabled = true;
                    cboBaseDatos.SelectedIndex = -1;
                }
                else
                {
                    cboBaseDatos.DataSource = null;
                    cboBaseDatos.Enabled = false;
                    MessageBox.Show("No se encontraron bases de datos asociadas a esta razón social.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar bases de datos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboBaseDatos.DataSource = null;
                cboBaseDatos.Enabled = false;
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedValue == null)
            {
                MessageBox.Show("Por favor seleccione una razón social",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboBaseDatos.SelectedValue == null)
            {
                MessageBox.Show("Por favor seleccione una base de datos",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpFechaInicio.Value > dtpFechaFin.Value)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            GenerarCatalogo();
        }

        private void GenerarCatalogo()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnConsultar.Enabled = false;

                string baseDatos = cboBaseDatos.SelectedValue.ToString();
                DateTime fechaInicio = dtpFechaInicio.Value.Date;
                DateTime fechaFin = dtpFechaFin.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                catalogoActual = catalogoService.ObtenerCatalogoPartes(baseDatos, fechaInicio, fechaFin);

                if (catalogoActual.Any())
                {
                    MostrarResultados();
                    MostrarResumen();
                }
                else
                {
                    chartCatalogo.Series = Array.Empty<ISeries>();
                    lblTotalPartes.Text = "Total de Partes: 0";
                    lblTotalConBOM.Text = "Con BOM: 0";
                    lblTotalSinBOM.Text = "Sin BOM: 0";

                    MessageBox.Show("No se encontraron partes en el rango de fechas seleccionado",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar catálogo: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnConsultar.Enabled = true;
            }
        }

        private void MostrarResultados()
        {
            // Calcular totales por tipo de componente
            int totalSUB = catalogoActual.Sum(p => p.TotalSUB);
            int totalMP = catalogoActual.Sum(p => p.TotalMP);
            int totalEQ = catalogoActual.Sum(p => p.TotalEQ);
            int totalRT = catalogoActual.Sum(p => p.TotalRT);
            int totalEMP = catalogoActual.Sum(p => p.TotalEMP);
            int totalMAQ = catalogoActual.Sum(p => p.TotalMAQ);
            int totalOtros = catalogoActual.Sum(p => p.TotalOtros);
            int totalGeneral = totalSUB + totalMP + totalEQ + totalRT + totalEMP + totalMAQ + totalOtros;

            if (totalGeneral == 0)
            {
                chartCatalogo.Series = Array.Empty<ISeries>();
                return;
            }

            // Crear lista de categorías y valores
            var categorias = new List<string>();
            var valores = new List<int>();
            var colores = new List<SKColor>();

            if (totalSUB > 0)
            {
                categorias.Add("SUB");
                valores.Add(totalSUB);
                colores.Add(new SKColor(41, 128, 185)); // Azul
            }

            if (totalMP > 0)
            {
                categorias.Add("MP");
                valores.Add(totalMP);
                colores.Add(new SKColor(46, 204, 113)); // Verde
            }

            if (totalEQ > 0)
            {
                categorias.Add("EQ");
                valores.Add(totalEQ);
                colores.Add(new SKColor(241, 196, 15)); // Amarillo
            }

            if (totalRT > 0)
            {
                categorias.Add("RT");
                valores.Add(totalRT);
                colores.Add(new SKColor(231, 76, 60)); // Rojo
            }

            if (totalEMP > 0)
            {
                categorias.Add("EMP");
                valores.Add(totalEMP);
                colores.Add(new SKColor(155, 89, 182)); // Púrpura
            }

            if (totalMAQ > 0)
            {
                categorias.Add("MAQ");
                valores.Add(totalMAQ);
                colores.Add(new SKColor(230, 126, 34)); // Naranja
            }

            if (totalOtros > 0)
            {
                categorias.Add("Otros");
                valores.Add(totalOtros);
                colores.Add(new SKColor(149, 165, 166)); // Gris
            }

            // Crear serie de barras con colores individuales
            var series = new ISeries[]
            {
                new LiveChartsCore.SkiaSharpView.ColumnSeries<int>
                {
                    Name = "Componentes",
                    Values = valores,
                    Fill = new SolidColorPaint(new SKColor(41, 128, 185)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    DataLabelsFormatter = point => $"{point.Model:N0}",
                    MaxBarWidth = 80
                }
            };

            chartCatalogo.Series = series;

            // Configurar ejes
            chartCatalogo.XAxes = new[]
            {
                new Axis
                {
                    Labels = categorias,
                    LabelsRotation = 0,
                    TextSize = 14,
                    LabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1 }
                }
            };

            chartCatalogo.YAxes = new[]
            {
                new Axis
                {
                    Name = "Cantidad de Componentes",
                    NameTextSize = 14,
                    NamePaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1 },
                    MinLimit = 0
                }
            };
        }

        private void MostrarResumen()
        {
            int totalPartes = catalogoActual.Count;
            int conBOM = catalogoActual.Count(p => p.EstatusBOM == "SI TIENE COMPONENTES");
            int sinBOM = catalogoActual.Count(p => p.EstatusBOM == "NO TIENE COMPONENTES");

            int totalSUB = catalogoActual.Sum(p => p.TotalSUB);
            int totalMP = catalogoActual.Sum(p => p.TotalMP);
            int totalEQ = catalogoActual.Sum(p => p.TotalEQ);
            int totalRT = catalogoActual.Sum(p => p.TotalRT);
            int totalEMP = catalogoActual.Sum(p => p.TotalEMP);
            int totalMAQ = catalogoActual.Sum(p => p.TotalMAQ);
            int totalOtros = catalogoActual.Sum(p => p.TotalOtros);
            int totalComponentes = totalSUB + totalMP + totalEQ + totalRT + totalEMP + totalMAQ + totalOtros;

            lblTotalPartes.Text = $"Total de Partes: {totalPartes:N0} | Total Componentes: {totalComponentes:N0}";
            lblTotalConBOM.Text = $"Con BOM: {conBOM:N0} | Sin BOM: {sinBOM:N0}";
            lblTotalSinBOM.Text = $"SUB: {totalSUB:N0} | MP: {totalMP:N0} | EQ: {totalEQ:N0} | RT: {totalRT:N0} | EMP: {totalEMP:N0} | MAQ: {totalMAQ:N0} | Otros: {totalOtros:N0}";
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (!catalogoActual.Any())
            {
                MessageBox.Show("No hay datos para exportar",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel|*.xlsx",
                    Title = "Exportar Catálogo de Partes",
                    FileName = $"CatalogoPartes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportarAExcel(saveDialog.FileName);
                    MessageBox.Show("Catálogo exportado exitosamente",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarAExcel(string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Catálogo de Partes");

                // Encabezados
                worksheet.Cell(1, 1).Value = "NO PARTE";
                worksheet.Cell(1, 2).Value = "DESCRIPCIÓN";
                worksheet.Cell(1, 3).Value = "FECHA INSERCIÓN";
                worksheet.Cell(1, 4).Value = "BOM INICIO";
                worksheet.Cell(1, 5).Value = "BOM FIN";
                worksheet.Cell(1, 6).Value = "TOTAL COMPONENTES";
                worksheet.Cell(1, 7).Value = "SUB";
                worksheet.Cell(1, 8).Value = "MP";
                worksheet.Cell(1, 9).Value = "EQ";
                worksheet.Cell(1, 10).Value = "RT";
                worksheet.Cell(1, 11).Value = "EMP";
                worksheet.Cell(1, 12).Value = "MAQ";
                worksheet.Cell(1, 13).Value = "OTROS";
                worksheet.Cell(1, 14).Value = "ESTATUS BOM";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 14);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Datos
                int fila = 2;
                foreach (var parte in catalogoActual)
                {
                    worksheet.Cell(fila, 1).Value = parte.NoPartePadre;
                    worksheet.Cell(fila, 2).Value = parte.Par_DescripcionEsp;

                    if (parte.Par_InsercionFecha.HasValue)
                        worksheet.Cell(fila, 3).Value = parte.Par_InsercionFecha.Value;

                    if (parte.Bom_FechaInicio.HasValue)
                        worksheet.Cell(fila, 4).Value = parte.Bom_FechaInicio.Value;

                    if (parte.Bom_FechaFin.HasValue)
                        worksheet.Cell(fila, 5).Value = parte.Bom_FechaFin.Value;

                    worksheet.Cell(fila, 6).Value = parte.TotalComponentes;
                    worksheet.Cell(fila, 7).Value = parte.TotalSUB;
                    worksheet.Cell(fila, 8).Value = parte.TotalMP;
                    worksheet.Cell(fila, 9).Value = parte.TotalEQ;
                    worksheet.Cell(fila, 10).Value = parte.TotalRT;
                    worksheet.Cell(fila, 11).Value = parte.TotalEMP;
                    worksheet.Cell(fila, 12).Value = parte.TotalMAQ;
                    worksheet.Cell(fila, 13).Value = parte.TotalOtros;
                    worksheet.Cell(fila, 14).Value = parte.EstatusBOM;

                    // Estilo de filas alternas
                    if (fila % 2 == 0)
                    {
                        worksheet.Range(fila, 1, fila, 14).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 246, 250);
                    }

                    fila++;
                }

                // Formato de fechas
                worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";
                worksheet.Column(4).Style.DateFormat.Format = "dd/MM/yyyy";
                worksheet.Column(5).Style.DateFormat.Format = "dd/MM/yyyy";

                // Centrar columnas numéricas
                for (int col = 6; col <= 13; col++)
                {
                    worksheet.Column(col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Centrar columna de estatus
                worksheet.Column(14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordes para todos los datos
                var dataRange = worksheet.Range(1, 1, fila - 1, 14);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Ajustar ancho de columnas
                worksheet.Column(1).Width = 18;
                worksheet.Column(2).Width = 40;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 12;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 10;
                worksheet.Column(9).Width = 10;
                worksheet.Column(10).Width = 10;
                worksheet.Column(11).Width = 10;
                worksheet.Column(12).Width = 10;
                worksheet.Column(13).Width = 10;
                worksheet.Column(14).Width = 20;

                // Congelar panel de encabezados
                worksheet.SheetView.FreezeRows(1);

                workbook.SaveAs(rutaArchivo);
            }
        }

        private void btnVerDetalle_Click(object sender, EventArgs e)
        {
            if (cboBaseDatos.SelectedItem == null)
            {
                MessageBox.Show("Por favor seleccione una base de datos",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string baseDatos = cboBaseDatos.SelectedItem.ToString() ?? string.Empty;
                DateTime fechaInicio = dtpFechaInicio.Value;
                DateTime fechaFin = dtpFechaFin.Value;

                FrmDetalleComponentes frmDetalle = new FrmDetalleComponentes(
                    catalogoService,
                    baseDatos,
                    fechaInicio,
                    fechaFin
                );

                frmDetalle.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir detalle: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
