using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Text;
using Retorno360Tacna.SERVICES;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Reflection;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmInventarioPreview : Form
    {
        private readonly List<Retorno360Tacna.SERVICES.InventarioPorEmpresaResult> resultados;
        private readonly string? _jobId;
        private int indexActual = 0;
        // Evita dobles ejecuciones concurrentes del mismo handler (por suscripción doble u otro motivo).
        private int _addingAllFlag = 0;
        // Panel de carga local en el preview (se crea bajo demanda)
        private System.Windows.Forms.Panel? _panelCargandoLocal;

        public FrmInventarioPreview(List<Retorno360Tacna.SERVICES.InventarioPorEmpresaResult> resultados, string? jobId = null)
        {
            InitializeComponent();
            this.resultados = resultados ?? new List<Retorno360Tacna.SERVICES.InventarioPorEmpresaResult>();
            this._jobId = jobId;

            btnExportarEmpresa.Click += BtnExportarEmpresa_Click;
            btnExportarTodas.Click += BtnExportarTodas_Click;

            // El manejador BtnAgregarTodasExcel_Click_1 se suscribe en el diseñador; evitar doble suscripción.
            btnAnterior.Click += BtnAnterior_Click;
            btnSiguiente.Click += BtnSiguiente_Click;


            // Por defecto el botón de exportar paginado está deshabilitado; al cargar el formulario
            // se habilitará solo si el formulario originador tiene el checkbox marcado.
            this.Load += FrmInventarioPreview_Load;
            this.FormClosed += FrmInventarioPreview_FormClosed;

            if (this.resultados.Count > 0)
            {
                indexActual = 0;
                MostrarIndice(indexActual);
            }
        }

        private void FrmInventarioPreview_FormClosed(object? sender, FormClosedEventArgs e)
        {
            try { SetOwnerLoading(false); } catch { }
            try { ShowLocalLoading(false); } catch { }
            try { System.Threading.Interlocked.Exchange(ref _addingAllFlag, 0); } catch { }
        }

        private void BtnExportarFooter_Click_1(object? sender, EventArgs e)
        {
            // Reuse export per-company behavior for footer button
            try
            {
                BtnExportarEmpresa_Click(sender, e);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Permite al formulario llamador habilitar o deshabilitar el botón "Agregar todos a Excel" antes de mostrar el preview.
        /// </summary>
        /// <param name="enabled">true para habilitar, false para deshabilitar.</param>
        public void SetEnableAgregarTodas(bool enabled)
        {
            try { btnAgregarTodasExcel.Enabled = enabled; } catch { }
        }

        /// <summary>
        /// Exporta todos los inventarios a un .xlsx usando Open XML en modo streaming para evitar uso excesivo de memoria.
        /// </summary>
        private void ExportAllToXlsxStream(string path, List<Retorno360Tacna.SERVICES.InventarioPorEmpresaResult> resultados, CancellationToken ct)
        {
            // Intentar abrir el archivo de salida; si está en uso, probar con sufijos numerados (hasta 10 intentos)
            string selectedPath = path;
            FileStream? fs = null;
            Exception? lastEx = null;
            for (int attempt = 0; attempt < 11; attempt++)
            {
                try
                {
                    if (attempt == 0)
                        selectedPath = path;
                    else
                    {
                        var dir = Path.GetDirectoryName(path) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var name = Path.GetFileNameWithoutExtension(path);
                        var ext = Path.GetExtension(path);
                        selectedPath = Path.Combine(dir, $"{name}({attempt}){ext}");
                    }

                    fs = new FileStream(selectedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                    lastEx = null;
                    break;
                }
                catch (IOException ioex)
                {
                    lastEx = ioex;
                    // esperar un momento breve antes del siguiente intento
                    try { Thread.Sleep(150); } catch { }
                    continue;
                }
            }

            if (fs == null)
            {
                // No se pudo crear el archivo tras varios intentos
                throw lastEx ?? new IOException("No fue posible crear el archivo de salida.");
            }

            // Es importante disponer del stream y del documento antes de intentar abrir el archivo si el usuario lo solicita.
            using (fs)
            {
                using var doc = SpreadsheetDocument.Create(fs, SpreadsheetDocumentType.Workbook);
                var workbookPart = doc.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            uint sheetId = 1;
            foreach (var r in resultados)
            {
                ct.ThrowIfCancellationRequested();
                if (r.Tabla == null) continue;

                var sheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetName = string.IsNullOrWhiteSpace(r.NombreEmpresa) ? $"Empresa_{r.IdEmpresa}" : r.NombreEmpresa;
                sheetName = SanitizeSheetName(sheetName);

                using (var writer = OpenXmlWriter.Create(sheetPart))
                {
                    writer.WriteStartElement(new Worksheet());
                    writer.WriteStartElement(new SheetData());

                    // escribir encabezados
                    var cols = r.Tabla.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                    WriteRow(writer, cols);

                    // escribir filas
                    foreach (DataRow dr in r.Tabla.Rows)
                    {
                        ct.ThrowIfCancellationRequested();
                        var values = cols.Select(cn => dr[cn] == DBNull.Value ? string.Empty : dr[cn].ToString()).ToArray();
                        WriteRow(writer, values);
                    }

                    writer.WriteEndElement(); // SheetData
                    writer.WriteEndElement(); // Worksheet
                }

                var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(sheetPart), SheetId = sheetId++, Name = sheetName };
                sheets.Append(sheet);
            }
                    workbookPart.Workbook.Save();
            }

            // Si se guardó en un nombre alternativo, intentar notificar al usuario abriendo esa ruta en lugar de la original
            if (!string.Equals(selectedPath, path, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var dr = MessageBox.Show($"Archivo original en uso. Se guardó como: {selectedPath}. ¿Desea abrirlo?", "Archivo guardado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        var psi = new ProcessStartInfo { FileName = selectedPath, UseShellExecute = true };
                        Process.Start(psi);
                    }
                }
                catch { }
            }

        }

        private void ShowLocalLoading(bool mostrar)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => ShowLocalLoading(mostrar)));
                    return;
                }

                if (mostrar)
                {
                    if (_panelCargandoLocal == null)
                    {
                        _panelCargandoLocal = new System.Windows.Forms.Panel();
                        _panelCargandoLocal.Name = "panelCargandoLocal";
                        _panelCargandoLocal.Size = new System.Drawing.Size(300, 80);
                        _panelCargandoLocal.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
                        _panelCargandoLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

                        var lbl = new System.Windows.Forms.Label();
                        lbl.Name = "lblCargandoLocal";
                        lbl.Text = "Procesando archivo Excel...";
                        lbl.AutoSize = true;
                        lbl.Location = new System.Drawing.Point(16, 16);
                        _panelCargandoLocal.Controls.Add(lbl);

                        var pb = new System.Windows.Forms.ProgressBar();
                        pb.Name = "progressBarCargandoLocal";
                        pb.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
                        pb.MarqueeAnimationSpeed = 30;
                        pb.Size = new System.Drawing.Size(260, 20);
                        pb.Location = new System.Drawing.Point(16, 40);
                        _panelCargandoLocal.Controls.Add(pb);
                    }

                    // Centrar en el preview
                    _panelCargandoLocal.Location = new System.Drawing.Point((this.ClientSize.Width - _panelCargandoLocal.Width) / 2, (this.ClientSize.Height - _panelCargandoLocal.Height) / 2);
                    if (!this.Controls.Contains(_panelCargandoLocal)) this.Controls.Add(_panelCargandoLocal);
                    _panelCargandoLocal.BringToFront();
                    _panelCargandoLocal.Visible = true;
                    _panelCargandoLocal.Refresh();
                }
                else
                {
                    if (_panelCargandoLocal != null)
                    {
                        try { _panelCargandoLocal.Visible = false; } catch { }
                        try { this.Controls.Remove(_panelCargandoLocal); } catch { }
                        try { _panelCargandoLocal.Dispose(); } catch { }
                        _panelCargandoLocal = null;
                    }
                }
            }
            catch { }
        }

        private static void WriteRow(OpenXmlWriter writer, string[] values)
        {
            writer.WriteStartElement(new Row());
            foreach (var v in values)
            {
                var safe = SanitizeForXml(v);
                writer.WriteElement(new Cell { DataType = CellValues.String, CellValue = new CellValue(safe) });
            }
            writer.WriteEndElement(); // Row
        }

        private static string SanitizeForXml(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                // XML 1.0 valid chars: #x9 | #xA | #xD | [#x20-#xD7FF] | ...
                if (ch == '\t' || ch == '\n' || ch == '\r' || ch >= 0x20)
                {
                    sb.Append(ch);
                }
                else
                {
                    // skip invalid control characters
                }
            }
            return sb.ToString();
        }

        private static string SanitizeSheetName(string name)
        {
            var invalid = new[] { '\\', '/', '?', '*', '[', ']', ':' };
            foreach (var c in invalid) name = name.Replace(c, '_');
            if (name.Length > 31) name = name.Substring(0, 31);
            return name;
        }

        private void PromptOpenFile(string filePath)
        {
            try
            {
                var dr = MessageBox.Show("Archivo exportado exitosamente. ¿Desea abrir el archivo?", "Éxito", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch
            {
                // ignorar fallos al intentar abrir el archivo
            }
        }





        private void FrmInventarioPreview_Load(object? sender, EventArgs e)
        {
            try
            {
                bool enable = false;

                if (this.Owner != null)
                {
                    var ctrls = this.Owner.Controls.Find("chkCargarTodasRazonesEmpresas", true);
                    if (ctrls != null && ctrls.Length > 0 && ctrls[0] is CheckBox cbOwner)
                        enable = cbOwner.Checked;
                }
                else
                {
                    // Fallback: buscar en formularios abiertos por si no se estableció Owner
                    foreach (Form f in Application.OpenForms)
                    {
                        var ctrls = f.Controls.Find("chkCargarTodasRazonesEmpresas", true);
                        if (ctrls != null && ctrls.Length > 0 && ctrls[0] is CheckBox cb)
                        {
                            enable = cb.Checked;
                            break;
                        }
                    }
                }



                try
                {
                    // Habilitar el botón de agregar todos a Excel cuando se marcó la opción en el formulario llamador
                    btnAgregarTodasExcel.Enabled = enable;
                }
                catch { }


            }
            catch { }
        }



        private void BtnExportarTodasUnificada_Click(object? sender, EventArgs e)
        {
            try
            {
                if (resultados == null || resultados.Count == 0)
                {
                    MessageBox.Show("No hay inventarios para exportar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_TodasEmpresas_Unificado_{DateTime.Now:yyyyMMdd}.xlsx" };
                if (Retorno360Tacna.HELPERS.WinFormsHelper.ShowSaveFileDialog(this, sfd) != DialogResult.OK) return;

                // Construir tabla combinada con columna adicional 'Empresa'
                var combined = new DataTable("TodasEmpresas");
                combined.Columns.Add("Empresa", typeof(string));

                // recopilar columnas únicas de todas las tablas
                var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in resultados)
                {
                    if (r.Tabla == null) continue;
                    foreach (DataColumn c in r.Tabla.Columns)
                        columnas.Add(c.ColumnName);
                }

                foreach (var col in columnas)
                    combined.Columns.Add(col, typeof(object));

                // importar filas
                foreach (var r in resultados)
                {
                    if (r.Tabla == null) continue;
                    foreach (DataRow row in r.Tabla.Rows)
                    {
                        var newRow = combined.NewRow();
                        newRow["Empresa"] = string.IsNullOrWhiteSpace(r.NombreEmpresa) ? $"Empresa_{r.IdEmpresa}" : r.NombreEmpresa;
                        foreach (DataColumn c in r.Tabla.Columns)
                        {
                            try { newRow[c.ColumnName] = row[c.ColumnName]; } catch { }
                        }
                        combined.Rows.Add(newRow);
                    }
                }

                using var wb = new XLWorkbook();
                var sheet = wb.Worksheets.Add("TodasEmpresas");
                sheet.Cell(1, 1).InsertTable(combined);
                sheet.Columns().AdjustToContents();
                wb.SaveAs(sfd.FileName);

                MessageBox.Show("Archivo exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try { PromptOpenFile(sfd.FileName); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar unificado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAgregarTodasExcel_Click_1(object? sender, EventArgs e)
        {
            // Evitar reentrada concurrente o doble ejecución
            if (System.Threading.Interlocked.CompareExchange(ref _addingAllFlag, 1, 0) != 0) return;
            try
            {
                List<InventarioPorEmpresaResult> source = resultados;

                // Si disponemos de JobId, intentar leer directamente desde la tabla staging
                if (!string.IsNullOrWhiteSpace(_jobId))
                {
                    try
                    {
                        var invSvc = new InventarioQueryService();
                        var staged = await invSvc.ObtenerResultadosDesdeCatalogoPorJobIdAsync(_jobId, System.Threading.CancellationToken.None).ConfigureAwait(false);
                        if (staged != null && staged.Count > 0)
                        {
                            source = staged;
                        }
                    }
                    catch
                    {
                        // fallback: usar la lista en memoria
                    }
                }

                if (source == null || source.Count == 0)
                {
                    MessageBox.Show("No hay inventarios para agregar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_TodasEmpresas_{DateTime.Now:yyyyMMdd}.xlsx" };
                if (Retorno360Tacna.HELPERS.WinFormsHelper.ShowSaveFileDialog(this, sfd) != DialogResult.OK)
                {
                    return;
                }
                var filename = sfd.FileName;

                try { btnAgregarTodasExcel.Enabled = false; } catch { }
                var cts = new CancellationTokenSource();
                try
                {
                    // Mostrar panel local en el preview y también informar al formulario padre
                    try { ShowLocalLoading(true); } catch { }
                    try { SetOwnerLoading(true); } catch { }

                    // Yield to the UI thread so the loading panel can render before starting the background work
                    await Task.Yield();

                    await Task.Run(() => ExportAllToXlsxStream(filename, source, cts.Token));

                    MessageBox.Show("Inventarios agregados al archivo Excel correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    try { PromptOpenFile(filename); } catch { }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Operación cancelada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al agregar inventarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    try { btnAgregarTodasExcel.Enabled = true; } catch { }
                    try { SetOwnerLoading(false); } catch { }
                    try { ShowLocalLoading(false); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar inventarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try { btnAgregarTodasExcel.Enabled = true; } catch { }
                try { SetOwnerLoading(false); } catch { }
                try { ShowLocalLoading(false); } catch { }
                System.Threading.Interlocked.Exchange(ref _addingAllFlag, 0);
            }
        }

        private void SetOwnerLoading(bool mostrar)
        {
            try
            {
                if (this.Owner == null) return;
                if (this.Owner.InvokeRequired)
                {
                    this.Owner.Invoke(new Action(() => SetOwnerLoading(mostrar)));
                    return;
                }

                var owner = this.Owner;
                // Intentar invocar método MostrarPanelCargando si existe (puede ser private)
                try
                {
                    var mi = owner.GetType();
                    var method = mi.GetMethod("MostrarPanelCargando", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(owner, new object[] { mostrar });
                        // No return: también intentar restaurar el estado de botones si existe el método correspondiente
                    }

                    // Intentar llamar también a EstablecerEstadoBotonesDuranteCarga(bool) para que el owner restaure los botones
                    try
                    {
                        var method2 = mi.GetMethod("EstablecerEstadoBotonesDuranteCarga", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method2 != null)
                        {
                            method2.Invoke(owner, new object[] { mostrar });
                            return;
                        }
                    }
                    catch { }
                }
                catch { }

                // Si no existe el método, buscar control panelCargando y cambiar Visible
                try
                {
                    var ctrls = owner.Controls.Find("panelCargando", true);
                    if (ctrls != null && ctrls.Length > 0)
                    {
                        var ctrlObj = ctrls[0];
                        var c = ctrlObj as System.Windows.Forms.Control;
                        if (c != null)
                        {
                            c.Visible = mostrar;
                            try { c.BringToFront(); } catch { }
                            try { c.Refresh(); } catch { }

                            // Intentar activar también el progressBar interno si existe
                            try
                            {
                                var pbs = c.Controls.Find("progressBarCargando", true);
                                if (pbs != null && pbs.Length > 0)
                                {
                                    var pb = pbs[0] as System.Windows.Forms.ProgressBar;
                                    if (pb != null)
                                    {
                                        if (mostrar)
                                        {
                                            pb.Style = ProgressBarStyle.Marquee;
                                        }
                                        else
                                        {
                                            pb.Style = ProgressBarStyle.Blocks;
                                        }
                                        try { pb.Refresh(); } catch { }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
            catch { }
        }

        private void MostrarIndice(int idx)
        {
            if (idx < 0 || idx >= resultados.Count) return;

            var r = resultados[idx];
            lblEmpresa.Text = $"Empresa: {r.NombreEmpresa} ";
            dgvResultados.DataSource = null;
            dgvResultados.DataSource = r.Tabla;

            btnAnterior.Enabled = idx > 0;
            btnSiguiente.Enabled = idx < resultados.Count - 1;
        }

        private void BtnAnterior_Click(object? sender, EventArgs e)
        {
            if (indexActual <= 0) return;
            indexActual--;
            MostrarIndice(indexActual);
        }

        private void BtnSiguiente_Click(object? sender, EventArgs e)
        {
            if (indexActual >= resultados.Count - 1) return;
            indexActual++;
            MostrarIndice(indexActual);
        }

        private void BtnExportarTodas_Click(object? sender, EventArgs e)
        {
            try
            {
                using var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_TodasEmpresas_{DateTime.Now:yyyyMMdd}.xlsx" };
                if (Retorno360Tacna.HELPERS.WinFormsHelper.ShowSaveFileDialog(this, sfd) != DialogResult.OK) return;

                using var wb = new XLWorkbook();
                foreach (var r in resultados)
                {
                    var nombre = string.IsNullOrWhiteSpace(r.NombreEmpresa) ? $"Empresa_{r.IdEmpresa}" : r.NombreEmpresa;
                    var hojas = wb.Worksheets.Where(w => w.Name.StartsWith(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
                    var sheet = wb.Worksheets.Add(nombre);
                    if (r.Tabla.Columns.Count == 0) continue;
                    sheet.Cell(1, 1).InsertTable(r.Tabla);
                    sheet.Columns().AdjustToContents();
                }

                wb.SaveAs(sfd.FileName);
                MessageBox.Show("Archivo exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try { PromptOpenFile(sfd.FileName); } catch { }
                try { PromptOpenFile(sfd.FileName); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarEmpresa_Click(object? sender, EventArgs e)
        {
            try
            {
                var r = resultados[indexActual];
                using var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_{r.NombreEmpresa}_{DateTime.Now:yyyyMMdd}.xlsx" };
                if (Retorno360Tacna.HELPERS.WinFormsHelper.ShowSaveFileDialog(this, sfd) != DialogResult.OK) return;

                using var wb = new XLWorkbook();
                var sheet = wb.Worksheets.Add(string.IsNullOrWhiteSpace(r.NombreEmpresa) ? $"Empresa_{r.IdEmpresa}" : r.NombreEmpresa);
                if (r.Tabla.Columns.Count > 0)
                {
                    sheet.Cell(1, 1).InsertTable(r.Tabla);
                    sheet.Columns().AdjustToContents();
                }

                wb.SaveAs(sfd.FileName);
                var result = MessageBox.Show("Desea abrir el archivo ?", "Abrir archivo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true }); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try { SetOwnerLoading(false); } catch { }

            try
            {
                if (this.Owner != null)
                {
                    if (this.Owner.InvokeRequired)
                    {
                        this.Owner.Invoke(new Action(() => RestoreOwnerButtons()));
                    }
                    else
                    {
                        RestoreOwnerButtons();
                    }
                }
            }
            catch { }

            this.Close();
        }

        /// <summary>
        /// Intenta restaurar botones críticos del formulario owner para evitar que queden deshabilitados
        /// cuando se cierra el preview.
        /// </summary>
        private void RestoreOwnerButtons()
        {
            try
            {
                var owner = this.Owner;
                if (owner == null) return;

                // Preferir llamar al método que restaura el estado de botones si existe
                try
                {
                    var mi = owner.GetType();
                    var method = mi.GetMethod("EstablecerEstadoBotonesDuranteCarga", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(owner, new object[] { false });
                    }
                }
                catch { }

                // Asegurar que botones claves estén habilitados por nombre si aún existen
                try
                {
                    var names = new[] { "btnConsultar", "btnExportarPdf", "btnExportarExcel", "btnCargarInventario", "btnGraficaIndividual", "btnGraficaTodos" };
                    foreach (var nm in names)
                    {
                        try
                        {
                            var ctrls = owner.Controls.Find(nm, true);
                            if (ctrls != null && ctrls.Length > 0)
                            {
                                var b = ctrls[0] as Button;
                                if (b != null && !b.IsDisposed) b.Enabled = true;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            catch { }
        }

   
    }
}
