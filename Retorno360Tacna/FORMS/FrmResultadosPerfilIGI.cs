using ClosedXML.Excel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DrawingColor = System.Drawing.Color;
using DrawingFont = System.Drawing.Font;
using DrawingSize = System.Drawing.Size;
using DrawingPoint = System.Drawing.Point;
using DrawingPen = System.Drawing.Pen;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Muestra los resultados IGI/IVA de todas las empresas del perfil del usuario
    /// para una razón social dada, cada una en su propio panel con grid, gráfica y exportación.
    /// </summary>
    public class FrmResultadosPerfilIGI : Form
    {
        // ── Modelo de datos por empresa ──────────────────────────────────────
        private sealed class ResultadoEmpresa
        {
            public string NombreEmpresa { get; set; } = string.Empty;
            public string BaseDatos { get; set; } = string.Empty;
            public DataTable? TablaIGI { get; set; }
            public DataTable? TablaIVA { get; set; }
        }

        // ── Campos ────────────────────────────────────────────────────────────
        private readonly ReporteIGIService _reporteService;
        private readonly RazonSocial _razon;
        private readonly List<string> _empresas;
        private readonly DateTime _fechaInicio;
        private readonly DateTime _fechaFin;

        // ── Controles del header ──────────────────────────────────────────────
        private Panel panelHeader = null!;
        private Label lblTitulo = null!;
        private Label lblSubtitulo = null!;
        private ProgressBar progressBar = null!;
        private Label lblProgreso = null!;

        // ── Panel de contenido scrollable ─────────────────────────────────────
        private Panel panelContenido = null!;

        public FrmResultadosPerfilIGI(
            ReporteIGIService reporteService,
            RazonSocial razon,
            List<string> empresas,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            _reporteService = reporteService;
            _razon = razon;
            _empresas = empresas;
            _fechaInicio = fechaInicio;
            _fechaFin = fechaFin;

            InicializarComponentes();
            this.Load += async (s, e) => await CargarResultadosAsync();
        }

        // ── Inicialización de controles ───────────────────────────────────────
        private void InicializarComponentes()
        {
            // Header verde
            panelHeader = new Panel
            {
                BackColor = DrawingColor.FromArgb(39, 174, 96),
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 0, 20, 0)
            };

            lblTitulo = new Label
            {
                Text = $"Reporte IGI — {_razon.NombreRazon}",
                Font = new DrawingFont("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                AutoSize = false,
                Location = new DrawingPoint(20, 10),
                Size = new DrawingSize(900, 30)
            };

            lblSubtitulo = new Label
            {
                Text = $"Período: {_fechaInicio:dd/MM/yyyy}  →  {_fechaFin:dd/MM/yyyy}  |  {_empresas.Count} empresa(s)",
                Font = new DrawingFont("Segoe UI", 9.5F),
                ForeColor = DrawingColor.FromArgb(220, 255, 220),
                AutoSize = false,
                Location = new DrawingPoint(20, 44),
                Size = new DrawingSize(900, 20)
            };

            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Location = new DrawingPoint(20, 65),
                Size = new DrawingSize(500, 8),
                Visible = false
            };

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Controls.Add(progressBar);

            // Barra de progreso inferior
            lblProgreso = new Label
            {
                Text = "Cargando datos...",
                Font = new DrawingFont("Segoe UI", 9F),
                ForeColor = DrawingColor.FromArgb(127, 140, 141),
                BackColor = DrawingColor.FromArgb(236, 240, 241),
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Panel de contenido scrollable
            panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = DrawingColor.FromArgb(245, 247, 250),
                Padding = new Padding(16, 16, 16, 16)
            };

            // Form
            FormBorderStyle = FormBorderStyle.None;
            BackColor = DrawingColor.FromArgb(245, 247, 250);
            WindowState = FormWindowState.Maximized;

            Controls.Add(panelContenido);
            Controls.Add(lblProgreso);
            Controls.Add(panelHeader);
        }

        // ── Carga asíncrona ───────────────────────────────────────────────────
        private async Task CargarResultadosAsync()
        {
            progressBar.Visible = true;
            lblProgreso.Text = "Consultando empresas del perfil...";

            var resultados = new List<ResultadoEmpresa>();

            await Task.Run(() =>
            {
                foreach (var empresa in _empresas)
                {
                    try
                    {
                        var conciliacion = _reporteService.ObtenerConciliacionIGI(empresa, _fechaInicio, _fechaFin);

                        var tablaIGI = conciliacion.ResumenIGI;
                        var tablaIVA = conciliacion.ResumenIVA;

                        PrepararColumnaPeriodo(tablaIGI);
                        PrepararColumnaPeriodo(tablaIVA);

                        resultados.Add(new ResultadoEmpresa
                        {
                            NombreEmpresa = LimpiarNombreEmpresa(empresa),
                            BaseDatos = empresa,
                            TablaIGI = tablaIGI,
                            TablaIVA = tablaIVA
                        });
                    }
                    catch
                    {
                        // Empresa con error: agregar con tablas vacías
                        resultados.Add(new ResultadoEmpresa
                        {
                            NombreEmpresa = LimpiarNombreEmpresa(empresa),
                            BaseDatos = empresa,
                            TablaIGI = null,
                            TablaIVA = null
                        });
                    }
                }
            });

            progressBar.Visible = false;
            lblProgreso.Text = $"{resultados.Count(r => r.TablaIGI != null)} empresa(s) con datos cargadas.";

            RenderizarResultados(resultados);
        }

        // ── Renderizar un panel por empresa ───────────────────────────────────
        private void RenderizarResultados(List<ResultadoEmpresa> resultados)
        {
            panelContenido.SuspendLayout();
            panelContenido.Controls.Clear();

            int yOffset = 16;
            int ancho = panelContenido.ClientSize.Width - 32;

            // Cuando el form se redimensione, recalculamos anchos
            panelContenido.Resize += (s, e) => RecalcularAnchos();

            foreach (var resultado in resultados)
            {
                var tarjeta = CrearTarjetaEmpresa(resultado, ancho);
                tarjeta.Location = new DrawingPoint(16, yOffset);
                panelContenido.Controls.Add(tarjeta);
                yOffset += tarjeta.Height + 20;
            }

            panelContenido.ResumeLayout();
        }

        private void RecalcularAnchos()
        {
            int ancho = panelContenido.ClientSize.Width - 32;
            foreach (Control ctrl in panelContenido.Controls)
            {
                ctrl.Width = ancho;
            }
        }

        // ── Tarjeta por empresa ───────────────────────────────────────────────
        private Panel CrearTarjetaEmpresa(ResultadoEmpresa resultado, int ancho)
        {
            bool sinDatos = resultado.TablaIGI == null || resultado.TablaIGI.Rows.Count == 0;
            int alturaTotal = sinDatos ? 90 : 520;

            var tarjeta = new Panel
            {
                Width = ancho,
                Height = alturaTotal,
                BackColor = DrawingColor.White,
                Padding = new Padding(0)
            };
            tarjeta.Paint += (s, e) =>
            {
                using var pen = new DrawingPen(DrawingColor.FromArgb(220, 220, 225), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, tarjeta.Width - 1, tarjeta.Height - 1);
            };

            // ── Header de tarjeta ──────────────────────────────────────────
            var headerTarjeta = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = DrawingColor.FromArgb(41, 128, 185),
                Padding = new Padding(14, 0, 14, 0)
            };

            var lblEmpresa = new Label
            {
                Text = $"🏢  {resultado.NombreEmpresa}",
                Font = new DrawingFont("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = ancho - 320,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Botones de exportar en el header
            var btnExcelTarjeta = CrearBotonTarjeta("Excel", DrawingColor.FromArgb(46, 125, 50));
            var btnPdfTarjeta = CrearBotonTarjeta("PDF", DrawingColor.FromArgb(192, 57, 43));
            btnPdfTarjeta.Location = new DrawingPoint(ancho - 155, 7);
            btnExcelTarjeta.Location = new DrawingPoint(ancho - 300, 7);

            if (!sinDatos)
            {
                var tablaIgi = resultado.TablaIGI!;
                var tablaIva = resultado.TablaIVA;
                var nombreEmpresa = resultado.NombreEmpresa;
                var baseDatos = resultado.BaseDatos;

                btnExcelTarjeta.Click += (s, e) => ExportarExcel(tablaIgi, tablaIva, nombreEmpresa);
                btnPdfTarjeta.Click += (s, e) => ExportarPdf(tablaIgi, tablaIva, nombreEmpresa, baseDatos);
            }
            else
            {
                btnExcelTarjeta.Enabled = false;
                btnPdfTarjeta.Enabled = false;
            }

            headerTarjeta.Controls.Add(lblEmpresa);
            headerTarjeta.Controls.Add(btnExcelTarjeta);
            headerTarjeta.Controls.Add(btnPdfTarjeta);
            tarjeta.Controls.Add(headerTarjeta);

            if (sinDatos)
            {
                var lblSinDatos = new Label
                {
                    Text = "⚠  Sin datos para el período seleccionado",
                    Font = new DrawingFont("Segoe UI", 10F),
                    ForeColor = DrawingColor.FromArgb(149, 165, 166),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                tarjeta.Controls.Add(lblSinDatos);
                return tarjeta;
            }

            // ── Contenedor principal (grid izq + gráfica der) ─────────────
            var panelCuerpoTarjeta = new Panel
            {
                Location = new DrawingPoint(0, 44),
                Width = ancho,
                Height = alturaTotal - 44,
                BackColor = DrawingColor.White
            };

            int anchoGrid = (int)(ancho * 0.55);
            int anchoGrafica = ancho - anchoGrid - 2;

            // Grids IGI / IVA
            var panelGrids = new Panel
            {
                Location = new DrawingPoint(0, 0),
                Width = anchoGrid,
                Height = panelCuerpoTarjeta.Height,
                BackColor = DrawingColor.White
            };

            int mitadGrid = panelCuerpoTarjeta.Height / 2;

            var lblTituloIGI = new Label
            {
                Text = "IGI Pagado vs Calculado",
                Font = new DrawingFont("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                BackColor = DrawingColor.FromArgb(41, 128, 185),
                Location = new DrawingPoint(0, 0),
                Width = anchoGrid,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            var gridIGI = CrearGrid(resultado.TablaIGI!);
            gridIGI.Location = new DrawingPoint(0, 26);
            gridIGI.Width = anchoGrid;
            gridIGI.Height = mitadGrid - 26;
            EstilarGridIGI(gridIGI);

            var lblTituloIVA = new Label
            {
                Text = "IVA Pagado",
                Font = new DrawingFont("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                BackColor = DrawingColor.FromArgb(142, 68, 173),
                Location = new DrawingPoint(0, mitadGrid),
                Width = anchoGrid,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            DataGridView? gridIVA = null;
            if (resultado.TablaIVA != null && resultado.TablaIVA.Rows.Count > 0)
            {
                gridIVA = CrearGrid(resultado.TablaIVA);
                gridIVA.Location = new DrawingPoint(0, mitadGrid + 26);
                gridIVA.Width = anchoGrid;
                gridIVA.Height = panelCuerpoTarjeta.Height - mitadGrid - 26;
                EstilarGridIVA(gridIVA);
            }

            panelGrids.Controls.Add(lblTituloIGI);
            panelGrids.Controls.Add(gridIGI);
            panelGrids.Controls.Add(lblTituloIVA);
            if (gridIVA != null)
                panelGrids.Controls.Add(gridIVA);

            // Gráfica
            var panelGrafica = new Panel
            {
                Location = new DrawingPoint(anchoGrid + 2, 0),
                Width = anchoGrafica,
                Height = panelCuerpoTarjeta.Height,
                BackColor = DrawingColor.White
            };

            var lblTituloGrafica = new Label
            {
                Text = "IGI Pagado  ┤  Diferencia  (por mes y forma de pago)",
                Font = new DrawingFont("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                BackColor = DrawingColor.FromArgb(52, 73, 94),
                Location = new DrawingPoint(0, 0),
                Width = anchoGrafica,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            var chart = CrearGraficaIGI(resultado.TablaIGI!);
            chart.Location = new DrawingPoint(0, 26);
            chart.Width = anchoGrafica;
            chart.Height = panelCuerpoTarjeta.Height - 26;

            panelGrafica.Controls.Add(lblTituloGrafica);
            panelGrafica.Controls.Add(chart);

            // Separador vertical
            var separador = new Panel
            {
                Location = new DrawingPoint(anchoGrid, 0),
                Width = 2,
                Height = panelCuerpoTarjeta.Height,
                BackColor = DrawingColor.FromArgb(220, 220, 225)
            };

            panelCuerpoTarjeta.Controls.Add(panelGrids);
            panelCuerpoTarjeta.Controls.Add(separador);
            panelCuerpoTarjeta.Controls.Add(panelGrafica);

            tarjeta.Controls.Add(panelCuerpoTarjeta);
            return tarjeta;
        }

        // ── Helpers de UI ─────────────────────────────────────────────────────
        private static Button CrearBotonTarjeta(string texto, System.Drawing.Color fondo)
        {
            var btn = new Button
            {
                Text = texto,
                Font = new DrawingFont("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DrawingColor.White,
                BackColor = fondo,
                FlatStyle = FlatStyle.Flat,
                Size = new DrawingSize(130, 30),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static DataGridView CrearGrid(DataTable tabla)
        {
            var dgv = new DataGridView
            {
                DataSource = tabla,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false,
                BackgroundColor = DrawingColor.White,
                BorderStyle = BorderStyle.None
            };
            dgv.AlternatingRowsDefaultCellStyle.BackColor = DrawingColor.FromArgb(245, 246, 250);
            return dgv;
        }

        private static void EstilarGridIGI(DataGridView dgv)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = DrawingColor.FromArgb(41, 128, 185);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = DrawingColor.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new DrawingFont("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.EnableHeadersVisualStyles = false;

            dgv.DataBindingComplete += (s, e) =>
            {
                if (dgv.Columns.Contains("Año")) dgv.Columns["Año"]!.Visible = false;
                if (dgv.Columns.Contains("Mes")) dgv.Columns["Mes"]!.Visible = false;
                if (dgv.Columns.Contains("Periodo")) dgv.Columns["Periodo"]!.HeaderText = "MES";
                if (dgv.Columns.Contains("IGI_Pagado")) dgv.Columns["IGI_Pagado"]!.HeaderText = "IGI PAGADO";
                if (dgv.Columns.Contains("IGI_Calculado")) dgv.Columns["IGI_Calculado"]!.HeaderText = "IGI CALCULADO";
                if (dgv.Columns.Contains("Diferencia_IGI")) dgv.Columns["Diferencia_IGI"]!.HeaderText = "DIFERENCIA";
                if (dgv.Columns.Contains("FormaPago_IGI")) dgv.Columns["FormaPago_IGI"]!.HeaderText = "F. PAGO";
            };
        }

        private static void EstilarGridIVA(DataGridView dgv)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = DrawingColor.FromArgb(142, 68, 173);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = DrawingColor.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new DrawingFont("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.EnableHeadersVisualStyles = false;

            dgv.DataBindingComplete += (s, e) =>
            {
                if (dgv.Columns.Contains("Año")) dgv.Columns["Año"]!.Visible = false;
                if (dgv.Columns.Contains("Mes")) dgv.Columns["Mes"]!.Visible = false;
                if (dgv.Columns.Contains("Periodo")) dgv.Columns["Periodo"]!.HeaderText = "MES";
                if (dgv.Columns.Contains("IVA_Pagado")) dgv.Columns["IVA_Pagado"]!.HeaderText = "IVA PAGADO";
                if (dgv.Columns.Contains("FormaPago_IVA")) dgv.Columns["FormaPago_IVA"]!.HeaderText = "F. PAGO";
            };
        }

        private static CartesianChart CrearGraficaIGI(DataTable tablaIGI)
        {
            var chart = new CartesianChart
            {
                BackColor = DrawingColor.White,
                ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
                TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom
            };

            // Agrupar por período para armar etiquetas y series
            var filas = tablaIGI.Rows.Cast<DataRow>()
                .Where(r => r["FormaPago_IGI"]?.ToString() == "5" || r["FormaPago_IGI"]?.ToString() == "0")
                .OrderBy(r => ObtenerInt(r, "Año")).ThenBy(r => ObtenerInt(r, "Mes"))
                .ToList();

            if (filas.Count == 0)
            {
                chart.Series = Array.Empty<ISeries>();
                return chart;
            }

            var labels = filas.Select(r =>
            {
                int mes = ObtenerInt(r, "Mes");
                int anio = ObtenerInt(r, "Año");
                string fp = r["FormaPago_IGI"]?.ToString() ?? "";
                return $"{NombreMesCorto(mes)}/{anio}\n({fp})";
            }).ToArray();

            var pagados = filas.Select(r => ObtenerDecimal(r, "IGI_Pagado")).ToArray();
            var diferencias = filas.Select(r => ObtenerDecimal(r, "Diferencia_IGI")).ToArray();

            chart.Series = new ISeries[]
            {
                new StackedColumnSeries<decimal>
                {
                    Name = "IGI Pagado",
                    Values = pagados,
                    Fill = new SolidColorPaint(new SKColor(41, 128, 185)),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 9,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                },
                new StackedColumnSeries<decimal>
                {
                    Name = "Diferencia",
                    Values = diferencias,
                    Fill = new SolidColorPaint(new SKColor(155, 194, 230)),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(64, 64, 64)),
                    DataLabelsSize = 9,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle
                }
            };

            chart.XAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = -30,
                    TextSize = 9,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200))
                }
            };

            chart.YAxes = new[]
            {
                new Axis
                {
                    TextSize = 9,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    Labeler = v => v.ToString("C0")
                }
            };

            return chart;
        }

        // ── Exportar Excel ────────────────────────────────────────────────────
        private void ExportarExcel(DataTable tablaIGI, DataTable? tablaIVA, string nombreEmpresa)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Guardar Excel",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"IGI_{LimpiarParaArchivo(nombreEmpresa)}_{_fechaInicio:yyyyMM}-{_fechaFin:yyyyMM}.xlsx"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var wb = new XLWorkbook();

                AgregarHojaExcel(wb, "IGI", tablaIGI);
                if (tablaIVA != null && tablaIVA.Rows.Count > 0)
                    AgregarHojaExcel(wb, "IVA", tablaIVA);

                wb.SaveAs(dlg.FileName);

                if (MessageBox.Show("Excel generado. ¿Deseas abrirlo?",
                    "Listo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar Excel:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void AgregarHojaExcel(XLWorkbook wb, string nombre, DataTable tabla)
        {
            var ws = wb.Worksheets.Add(nombre);
            ws.Cell(1, 1).InsertTable(tabla);
            ws.Columns().AdjustToContents();

            // Estilo de encabezado
            var headerRange = ws.Range(1, 1, 1, tabla.Columns.Count);
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Font.Bold = true;
        }

        // ── Exportar PDF ──────────────────────────────────────────────────────
        private void ExportarPdf(DataTable tablaIGI, DataTable? tablaIVA, string nombreEmpresa, string baseDatos)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Guardar PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"IGI_{LimpiarParaArchivo(nombreEmpresa)}_{_fechaInicio:yyyyMM}-{_fechaFin:yyyyMM}.pdf"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.DefaultTextStyle(ts => ts.FontSize(8).FontFamily("Segoe UI"));

                        page.Header().Element(c => ConstruirHeaderPdf(c, nombreEmpresa, baseDatos));
                        page.Content().Element(c => ConstruirContenidoPdf(c, tablaIGI, tablaIVA));
                        page.Footer().AlignCenter().Text(t =>
                        {
                            t.Span("Retorno360 Tacna  |  Generado el ").FontColor(QuestPDF.Infrastructure.Color.FromHex("#95a5a6"));
                            t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontColor(QuestPDF.Infrastructure.Color.FromHex("#95a5a6"));
                        });
                    });
                }).GeneratePdf(dlg.FileName);

                if (MessageBox.Show("PDF generado. ¿Deseas abrirlo?",
                    "Listo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConstruirHeaderPdf(IContainer c, string nombreEmpresa, string baseDatos)
        {
            c.Padding(5).Column(col =>
            {
                col.Item().Text($"Reporte IGI — {_razon.NombreRazon}")
                    .FontSize(16).Bold().FontColor(QuestPDF.Infrastructure.Color.FromHex("#2980b9"));

                col.Item().Text($"Empresa: {nombreEmpresa}  |  Base: {baseDatos}")
                    .FontSize(9).FontColor(QuestPDF.Infrastructure.Color.FromHex("#7f8c8d"));

                col.Item().Text($"Período: {_fechaInicio:dd/MM/yyyy}  →  {_fechaFin:dd/MM/yyyy}")
                    .FontSize(9).FontColor(QuestPDF.Infrastructure.Color.FromHex("#7f8c8d"));
            });
        }

        private static void ConstruirContenidoPdf(IContainer c, DataTable tablaIGI, DataTable? tablaIVA)
        {
            c.Column(col =>
            {
                col.Item().Text("IGI Pagado vs Calculado").Bold().FontSize(10)
                    .FontColor(QuestPDF.Infrastructure.Color.FromHex("#2980b9"));
                col.Item().PaddingBottom(4).Table(t => LlenarTablaQuestPdf(t, tablaIGI,
                    new[] { "Periodo", "FormaPago_IGI", "IGI_Pagado", "IGI_Calculado", "Diferencia_IGI" },
                    new[] { "MES", "F. PAGO", "IGI PAGADO", "IGI CALCULADO", "DIFERENCIA" }));

                if (tablaIVA != null && tablaIVA.Rows.Count > 0)
                {
                    col.Item().PaddingTop(8).Text("IVA Pagado").Bold().FontSize(10)
                        .FontColor(QuestPDF.Infrastructure.Color.FromHex("#8e44ad"));
                    col.Item().Table(t => LlenarTablaQuestPdf(t, tablaIVA,
                        new[] { "Periodo", "FormaPago_IVA", "IVA_Pagado" },
                        new[] { "MES", "F. PAGO", "IVA PAGADO" }));
                }
            });
        }

        private static void LlenarTablaQuestPdf(TableDescriptor t, DataTable tabla,
            string[] columnas, string[] encabezados)
        {
            // Columnas con tamaño relativo
            t.ColumnsDefinition(c =>
            {
                for (int i = 0; i < columnas.Length; i++)
                    c.RelativeColumn();
            });

            // Encabezados
            foreach (var enc in encabezados)
            {
                t.Header(h =>
                    h.Cell().Background(QuestPDF.Infrastructure.Color.FromHex("#2980b9"))
                     .Padding(4).Text(enc).Bold().FontColor(Colors.White).FontSize(8));
            }

            // Filas
            bool alt = false;
            foreach (DataRow row in tabla.Rows)
            {
                var bgColor = alt
                    ? QuestPDF.Infrastructure.Color.FromHex("#f5f6fa")
                    : Colors.White;
                alt = !alt;

                foreach (var col in columnas)
                {
                    string val = tabla.Columns.Contains(col) && row[col] != DBNull.Value
                        ? FormatearValorPdf(row[col])
                        : "";

                    t.Cell().Background(bgColor).Padding(3).Text(val).FontSize(8);
                }
            }
        }

        private static string FormatearValorPdf(object val)
        {
            if (val is decimal d) return d.ToString("N2");
            return val?.ToString() ?? "";
        }

        // ── Utilidades estáticas ──────────────────────────────────────────────
        private static void PrepararColumnaPeriodo(DataTable tabla)
        {
            if (tabla == null) return;
            if (!tabla.Columns.Contains("Periodo"))
                tabla.Columns.Add("Periodo", typeof(string));

            foreach (DataRow row in tabla.Rows)
            {
                int anio = ObtenerInt(row, "Año");
                int mes = ObtenerInt(row, "Mes");
                row["Periodo"] = anio > 0 && mes > 0 ? $"{NombreMes(mes)} {anio}" : string.Empty;
            }
        }

        private static int ObtenerInt(DataRow row, string col)
        {
            if (row.Table.Columns.Contains(col) && row[col] != DBNull.Value)
                return Convert.ToInt32(row[col]);
            return 0;
        }

        private static decimal ObtenerDecimal(DataRow row, string col)
        {
            if (row.Table.Columns.Contains(col) && row[col] != DBNull.Value)
                return Convert.ToDecimal(row[col]);
            return 0m;
        }

        private static string NombreMes(int mes) => mes switch
        {
            1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
            5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
            9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
            _ => $"Mes {mes}"
        };

        private static string NombreMesCorto(int mes) => mes switch
        {
            1 => "Ene", 2 => "Feb", 3 => "Mar", 4 => "Abr",
            5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Ago",
            9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Dic",
            _ => $"M{mes}"
        };

        private static string LimpiarNombreEmpresa(string nombre) =>
            nombre.Replace("SEERT_", "", StringComparison.OrdinalIgnoreCase).Trim(' ', '_', '-');

        private static string LimpiarParaArchivo(string nombre) =>
            string.Concat(nombre.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
    }
}
