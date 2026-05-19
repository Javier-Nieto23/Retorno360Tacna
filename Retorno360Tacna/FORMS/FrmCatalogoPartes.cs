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
        private List<ParteBOMCompleto> catalogoCompleto = new List<ParteBOMCompleto>();
        private int graficoActual = 0; // 0 = MP, 1 = BOM Completo

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

            ActualizarIndicadorGrafico();
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

                // Cargar ambos catálogos
                catalogoActual = catalogoService.ObtenerCatalogoPartes(baseDatos, fechaInicio, fechaFin);
                catalogoCompleto = catalogoService.ObtenerCatalogoBOMCompleto(baseDatos, fechaInicio, fechaFin);

                if (catalogoActual.Any() || catalogoCompleto.Any())
                {
                    MostrarGraficoActual();
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

        private void MostrarGraficoActual()
        {
            if (graficoActual == 0)
            {
                MostrarGraficoMP();
            }
            else
            {
                MostrarGraficoBOMCompleto();
            }
            ActualizarIndicadorGrafico();
        }

        private void MostrarGraficoMP()
        {
            if (!catalogoActual.Any())
            {
                chartCatalogo.Series = Array.Empty<ISeries>();
                lblTotalPartes.Text = "Total de Partes MP: 0";
                lblTotalConBOM.Text = "En BOM: 0";
                lblTotalSinBOM.Text = "No en BOM: 0";
                return;
            }

            // Contar partes que están en BOM y las que no
            int enBOM = catalogoActual.Count(p => p.ExisteEnBOM == "SI");
            int noEnBOM = catalogoActual.Count(p => p.ExisteEnBOM == "NO");

            // Crear serie de barras
            var series = new ISeries[]
            {
                new LiveChartsCore.SkiaSharpView.ColumnSeries<int>
                {
                    Name = "Partes MP",
                    Values = new int[] { enBOM, noEnBOM },
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    DataLabelsFormatter = point => $"{point.Model:N0}",
                    MaxBarWidth = 100
                }
            };

            chartCatalogo.Series = series;

            // Configurar ejes
            chartCatalogo.XAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { "En BOM", "No en BOM" },
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
                    Name = "Cantidad de Partes",
                    NameTextSize = 14,
                    NamePaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1 },
                    MinLimit = 0
                }
            };

            // Actualizar resumen
            int totalPartes = catalogoActual.Count;
            lblTotalPartes.Text = $"Total de Partes MP: {totalPartes:N0}";
            lblTotalConBOM.Text = $"En BOM: {enBOM:N0}";
            lblTotalSinBOM.Text = $"No en BOM: {noEnBOM:N0}";
        }

        private void MostrarGraficoBOMCompleto()
        {
            if (!catalogoCompleto.Any())
            {
                chartCatalogo.Series = Array.Empty<ISeries>();
                lblTotalPartes.Text = "Total de Partes: 0";
                lblTotalConBOM.Text = "Total Componentes: 0";
                lblTotalSinBOM.Text = "";
                return;
            }

            // Calcular totales por tipo
            int totalSUB = catalogoCompleto.Sum(p => p.TotalSUB);
            int totalEQ = catalogoCompleto.Sum(p => p.TotalEQ);
            int totalRT = catalogoCompleto.Sum(p => p.TotalRT);
            int totalOtros = catalogoCompleto.Sum(p => p.TotalOtros);

            // Crear listas para categorías con datos
            var categorias = new List<string>();
            var valores = new List<int>();

            if (totalSUB > 0)
            {
                categorias.Add("SUB");
                valores.Add(totalSUB);
            }

            if (totalEQ > 0)
            {
                categorias.Add("EQ");
                valores.Add(totalEQ);
            }

            if (totalRT > 0)
            {
                categorias.Add("RT");
                valores.Add(totalRT);
            }

            if (totalOtros > 0)
            {
                categorias.Add("Otros");
                valores.Add(totalOtros);
            }

            if (valores.Count == 0)
            {
                chartCatalogo.Series = Array.Empty<ISeries>();
                return;
            }

            // Crear serie de barras
            var series = new ISeries[]
            {
                new LiveChartsCore.SkiaSharpView.ColumnSeries<int>
                {
                    Name = "Componentes BOM",
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

            // Actualizar resumen
            int totalPartes = catalogoCompleto.Count;
            int conBOM = catalogoCompleto.Count(p => p.EstatusBOM == "SI TIENE COMPONENTES");
            int sinBOM = catalogoCompleto.Count(p => p.EstatusBOM == "NO TIENE COMPONENTES");
            int totalComponentes = totalSUB + totalEQ + totalRT + totalOtros;

            lblTotalPartes.Text = $"Total de Partes: {totalPartes:N0} | Total Componentes: {totalComponentes:N0}";
            lblTotalConBOM.Text = $"Con BOM: {conBOM:N0} | Sin BOM: {sinBOM:N0}";
            lblTotalSinBOM.Text = $"SUB: {totalSUB:N0} | EQ: {totalEQ:N0} | RT: {totalRT:N0} | Otros: {totalOtros:N0}";
        }

        private void MostrarResultados()
        {
            // Contar partes que están en BOM y las que no
            int enBOM = catalogoActual.Count(p => p.ExisteEnBOM == "SI");
            int noEnBOM = catalogoActual.Count(p => p.ExisteEnBOM == "NO");

            if (enBOM == 0 && noEnBOM == 0)
            {
                chartCatalogo.Series = Array.Empty<ISeries>();
                return;
            }

            // Crear serie de barras
            var series = new ISeries[]
            {
                new LiveChartsCore.SkiaSharpView.ColumnSeries<int>
                {
                    Name = "Partes MP",
                    Values = new int[] { enBOM, noEnBOM },
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    DataLabelsFormatter = point => $"{point.Model:N0}",
                    MaxBarWidth = 100
                }
            };

            chartCatalogo.Series = series;

            // Configurar ejes
            chartCatalogo.XAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { "En BOM", "No en BOM" },
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
                    Name = "Cantidad de Partes",
                    NameTextSize = 14,
                    NamePaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    TextSize = 12,
                    LabelsPaint = new SolidColorPaint(new SKColor(50, 50, 50)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1 },
                    MinLimit = 0
                }
            };
        }

        private void ActualizarIndicadorGrafico()
        {
            if (graficoActual == 0)
            {
                lblIndicadorGrafico.Text = "Gráfico: Partes MP (1 de 2)";
            }
            else
            {
                lblIndicadorGrafico.Text = "Gráfico: BOM Completo (2 de 2)";
            }
        }

        private void btnGraficoAnterior_Click(object sender, EventArgs e)
        {
            if (graficoActual > 0)
            {
                graficoActual--;
                MostrarGraficoActual();
            }
        }

        private void btnGraficoSiguiente_Click(object sender, EventArgs e)
        {
            if (graficoActual < 1)
            {
                graficoActual++;
                MostrarGraficoActual();
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            bool tieneDatos = (graficoActual == 0 && catalogoActual.Any()) || (graficoActual == 1 && catalogoCompleto.Any());

            if (!tieneDatos)
            {
                MessageBox.Show("No hay datos para exportar",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string nombreArchivo = graficoActual == 0 
                    ? $"CatalogoPartesMP_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    : $"CatalogoBOMCompleto_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel|*.xlsx",
                    Title = "Exportar Catálogo de Partes",
                    FileName = nombreArchivo
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (graficoActual == 0)
                    {
                        ExportarMPAExcel(saveDialog.FileName);
                    }
                    else
                    {
                        ExportarBOMCompletoAExcel(saveDialog.FileName);
                    }

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

        private void ExportarMPAExcel(string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Catálogo de Partes MP");

                // Encabezados
                worksheet.Cell(1, 1).Value = "NO PARTE";
                worksheet.Cell(1, 2).Value = "DESCRIPCIÓN";
                worksheet.Cell(1, 3).Value = "FECHA INSERCIÓN";
                worksheet.Cell(1, 4).Value = "EXISTE EN BOM";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(46, 204, 113);
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Datos
                int fila = 2;
                foreach (var parte in catalogoActual)
                {
                    worksheet.Cell(fila, 1).Value = parte.Par_NoParte;
                    worksheet.Cell(fila, 2).Value = parte.Par_DescripcionEsp;

                    if (parte.Par_InsercionFecha.HasValue)
                        worksheet.Cell(fila, 3).Value = parte.Par_InsercionFecha.Value;

                    worksheet.Cell(fila, 4).Value = parte.ExisteEnBOM;

                    // Estilo de filas alternas
                    if (fila % 2 == 0)
                    {
                        worksheet.Range(fila, 1, fila, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 246, 250);
                    }

                    // Colorear según estatus BOM
                    if (parte.ExisteEnBOM == "SI")
                    {
                        worksheet.Cell(fila, 4).Style.Font.FontColor = XLColor.FromArgb(39, 174, 96);
                        worksheet.Cell(fila, 4).Style.Font.Bold = true;
                    }
                    else
                    {
                        worksheet.Cell(fila, 4).Style.Font.FontColor = XLColor.FromArgb(231, 76, 60);
                        worksheet.Cell(fila, 4).Style.Font.Bold = true;
                    }

                    fila++;
                }

                // Formato de fechas
                worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";

                // Centrar columna de estatus
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordes para todos los datos
                var dataRange = worksheet.Range(1, 1, fila - 1, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Ajustar ancho de columnas
                worksheet.Column(1).Width = 18;
                worksheet.Column(2).Width = 50;
                worksheet.Column(3).Width = 18;
                worksheet.Column(4).Width = 15;

                // Congelar panel de encabezados
                worksheet.SheetView.FreezeRows(1);

                workbook.SaveAs(rutaArchivo);
            }
        }

        private void ExportarBOMCompletoAExcel(string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Catálogo BOM Completo");

                // Encabezados
                worksheet.Cell(1, 1).Value = "NO PARTE";
                worksheet.Cell(1, 2).Value = "DESCRIPCIÓN";
                worksheet.Cell(1, 3).Value = "FECHA INSERCIÓN";
                worksheet.Cell(1, 4).Value = "BOM INICIO";
                worksheet.Cell(1, 5).Value = "BOM FIN";
                worksheet.Cell(1, 6).Value = "TOTAL COMP.";
                worksheet.Cell(1, 7).Value = "SUB";
                worksheet.Cell(1, 8).Value = "EQ";
                worksheet.Cell(1, 9).Value = "RT";
                worksheet.Cell(1, 10).Value = "OTROS";
                worksheet.Cell(1, 11).Value = "ESTATUS BOM";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 11);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Datos
                int fila = 2;
                foreach (var parte in catalogoCompleto)
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
                    worksheet.Cell(fila, 8).Value = parte.TotalEQ;
                    worksheet.Cell(fila, 9).Value = parte.TotalRT;
                    worksheet.Cell(fila, 10).Value = parte.TotalOtros;
                    worksheet.Cell(fila, 11).Value = parte.EstatusBOM;

                    // Estilo de filas alternas
                    if (fila % 2 == 0)
                    {
                        worksheet.Range(fila, 1, fila, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 246, 250);
                    }

                    fila++;
                }

                // Formato de fechas
                worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";
                worksheet.Column(4).Style.DateFormat.Format = "dd/MM/yyyy";
                worksheet.Column(5).Style.DateFormat.Format = "dd/MM/yyyy";

                // Centrar columnas numéricas
                for (int col = 6; col <= 10; col++)
                {
                    worksheet.Column(col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Centrar columna de estatus
                worksheet.Column(11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordes para todos los datos
                var dataRange = worksheet.Range(1, 1, fila - 1, 11);
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
                worksheet.Column(11).Width = 20;

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
