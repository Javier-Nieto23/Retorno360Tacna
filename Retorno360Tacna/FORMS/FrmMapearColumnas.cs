using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmMapearColumnas : Form
    {
        // Identificadores de razón/empresa que el formulario padre deberá asignar
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int IdRazon { get; set; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int IdEmpresa { get; set; }
        public string SelectedSheet => cmbHojas.SelectedItem?.ToString() ?? string.Empty;
        public string SelectedColParte => cmbParte.SelectedItem?.ToString() ?? string.Empty;
        public string SelectedColCantidad => cmbCant.SelectedItem?.ToString() ?? string.Empty;
        public string SelectedColUM => cmbUm.SelectedItem?.ToString() ?? string.Empty;
        public string SelectedColTotalCosto => cmbTotalCosto != null && chkTotalCosto != null && chkTotalCosto.Checked && cmbTotalCosto.SelectedItem != null
            ? cmbTotalCosto.SelectedItem.ToString() ?? string.Empty
            : string.Empty;
        public string SelectedColCostoUnitario => cmbCostoUnitario != null && chkTotalCosto != null && chkTotalCosto.Checked && cmbCostoUnitario.SelectedItem != null
            ? cmbCostoUnitario.SelectedItem.ToString() ?? string.Empty
            : string.Empty;

        private ExcelLayoutModel? _layout;

        public FrmMapearColumnas()
        {
            InitializeComponent();
            // Wire events
            cmbHojas.SelectedIndexChanged += (s, e) => { TryLoadFieldsForSelectedSheet(); };
            // inicializar combos de mes/año con valores por defecto
            try
            {
                cmbMes.Items.Clear();
                var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
                foreach (var m in meses) cmbMes.Items.Add(m);
                cmbMes.SelectedIndex = DateTime.Now.Month - 1;

                cmbAno.Items.Clear();
                int now = DateTime.Now.Year;
                for (int y = now - 1; y <= now + 1; y++) cmbAno.Items.Add(y.ToString());
                cmbAno.SelectedItem = now.ToString();
            }
            catch { }
            // controlar comportamiento del checkbox TotalCosto
            try
            {
                chkTotalCosto.CheckedChanged += (s, e) => { cmbTotalCosto.Enabled = chkTotalCosto.Checked; cmbCostoUnitario.Enabled = chkTotalCosto.Checked; };
                cmbTotalCosto.Enabled = false;
                cmbCostoUnitario.Enabled = false;
            }
            catch { }
            // Validar en el click OK: comprobar parcialmente que los números de parte y UM coinciden en la BD
            btnOk.Click += async (s, e) =>
            {
                try
                {
                    // Si no hay layout cargado o no se seleccionó columna de parte, permitir cerrar (la validación no aplica)
                    if (_layout == null || string.IsNullOrWhiteSpace(SelectedSheet) || string.IsNullOrWhiteSpace(SelectedColParte))
                    {
                        this.DialogResult = DialogResult.OK; this.Close(); return;
                    }

                    // Si no se proporcionaron credenciales de base (IdRazon/IdEmpresa), no podemos validar contra BD: cerrar
                    if (IdRazon <= 0 || IdEmpresa <= 0)
                    {
                        this.DialogResult = DialogResult.OK; this.Close(); return;
                    }

                    // Abrir archivo y leer muestra de filas de la hoja seleccionada
                    var ruta = _layout.RutaArchivo;
                    if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta)) { MessageBox.Show("Archivo no disponible para validación.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    using var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var wb = new XLWorkbook(stream);
                    var ws = wb.Worksheets.FirstOrDefault(w => string.Equals(w.Name, SelectedSheet, StringComparison.OrdinalIgnoreCase));
                    if (ws == null) { MessageBox.Show("Hoja no encontrada para validación.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    // Detectar fila de encabezado buscando la columna de parte
                    int ultimaFila = ws.LastRowUsed()?.RowNumber() ?? 0;
                    int filaMax = Math.Min(Math.Max(ultimaFila, 1), 20);
                    int filaEnc = 0;
                    for (int r = 1; r <= filaMax; r++)
                    {
                        var row = ws.Row(r);
                        if (row.CellsUsed().Any())
                        {
                            foreach (var c in row.Cells())
                            {
                                var val = c.GetString().Trim();
                                if (string.Equals(val, SelectedColParte, StringComparison.OrdinalIgnoreCase) || val.IndexOf(SelectedColParte, StringComparison.OrdinalIgnoreCase) >= 0)
                                { filaEnc = r; break; }
                            }
                        }
                        if (filaEnc > 0) break;
                    }
                    if (filaEnc == 0) filaEnc = 1;

                    int ultimaCol = ws.Row(filaEnc).LastCellUsed()?.Address.ColumnNumber ?? 0;
                    int idxParte = -1, idxUm = -1;
                    for (int c = 1; c <= ultimaCol; c++)
                    {
                        var txt = ws.Cell(filaEnc, c).GetString().Trim();
                        if (string.Equals(txt, SelectedColParte, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(SelectedColParte, StringComparison.OrdinalIgnoreCase) >= 0) idxParte = c;
                        if (!string.IsNullOrWhiteSpace(SelectedColUM) && (string.Equals(txt, SelectedColUM, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(SelectedColUM, StringComparison.OrdinalIgnoreCase) >= 0)) idxUm = c;
                    }

                    if (idxParte <= 0)
                    {
                        // No se puede validar sin columna de parte
                        this.DialogResult = DialogResult.OK; this.Close(); return;
                    }

                    var entradas = new List<(string Parte, decimal Cantidad, string Unidad)>();
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                    for (int r = filaEnc + 1; r <= lastRow && entradas.Count < 300; r++)
                    {
                        var part = ws.Cell(r, idxParte).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(part)) continue;
                        string unidad = string.Empty;
                        if (idxUm > 0) unidad = ws.Cell(r, idxUm).GetString().Trim();
                        entradas.Add((part, 1m, unidad));
                    }

                    if (entradas.Count == 0)
                    {
                        this.DialogResult = DialogResult.OK; this.Close(); return;
                    }

                    // Llamar al servicio para verificar existencia y UM
                    var svc = new Retorno360Tacna.SERVICES.VerificacionPartesService();
                    var resultado = await svc.VerificarPartesConCantidadAsync(IdRazon, IdEmpresa, entradas);
                    if (resultado != null && resultado.Items != null && resultado.Items.Any(i => !i.Existe || !i.UmCoincide))
                    {
                        MessageBox.Show("Se detectaron discrepancias en número de parte o unidad de medida en la muestra del archivo. Por favor, realiza la comprobación por TXT (por texto) antes de continuar.", "Verificación necesaria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // no cerrar el formulario
                    }

                    this.DialogResult = DialogResult.OK; this.Close();
                }
                catch (Exception ex)
                {
                    try { MessageBox.Show($"Error en validación previa: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
                }
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            // enable dragging by header if needed
            header.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) { DoDragMove(); }
            };
        }

        private void DoDragMove()
        {
            // Allow window to be moved by simulating caption drag when borderless
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HTCAPTION = 0x2;
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        }

        public void LoadLayout(ExcelLayoutModel layout)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            try
            {
                cmbHojas.Items.Clear();
                if (layout.Hojas != null)
                {
                    foreach (var h in layout.Hojas) cmbHojas.Items.Add(h);
                }
                if (cmbHojas.Items.Count > 0) cmbHojas.SelectedIndex = 0;
                TryLoadFieldsForSelectedSheet();
            }
            catch { }
        }

        private void TryLoadFieldsForSelectedSheet()
        {
            try
            {
                if (_layout == null) return;
                if (cmbHojas.SelectedItem == null) return;
                var hoja = cmbHojas.SelectedItem.ToString() ?? string.Empty;
                var campos = _layout.AnalizarHoja(hoja) ?? new List<string>();
                cmbParte.Items.Clear(); cmbCant.Items.Clear(); cmbUm.Items.Clear(); cmbTotalCosto.Items.Clear(); cmbCostoUnitario.Items.Clear();
                foreach (var c in campos) { cmbParte.Items.Add(c); cmbCant.Items.Add(c); cmbUm.Items.Add(c); cmbTotalCosto.Items.Add(c); cmbCostoUnitario.Items.Add(c); }
                if (cmbParte.Items.Count > 0) cmbParte.SelectedIndex = 0;
                if (cmbCant.Items.Count > 1) cmbCant.SelectedIndex = 1; else if (cmbCant.Items.Count > 0) cmbCant.SelectedIndex = 0;
                if (cmbUm.Items.Count > 2) cmbUm.SelectedIndex = 2; else if (cmbUm.Items.Count > 0) cmbUm.SelectedIndex = 0;
                if (cmbTotalCosto.Items.Count > 0) cmbTotalCosto.SelectedIndex = 0;
                if (cmbCostoUnitario.Items.Count > 0) cmbCostoUnitario.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
