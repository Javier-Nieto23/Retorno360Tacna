using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;


namespace Retorno360Tacna.FORMS
{
    public partial class FrmCalculoInventarios : Form
    {
        private MODELS.Usuario? usuarioActual;
        // Servicio de perfil usado para cargar razones/empresas cuando se dispone de usuario
        private SERVICES.PerfilUsuarioService? perfilService;

        // Exponer los ids seleccionados como propiedades públicas para que otros formularios
        // puedan obtener la razón social / empresa activa sin acceder directamente a los
        // controles del diseñador (que son privados).
        public int SelectedIdRazon
        {
            get
            {
                if (cmbRazonSocial != null && cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int id))
                    return id;
                return 0;
            }
        }

        private async void btnCargarInventario_Click(object? sender, EventArgs e)
        {
            string logPath = Path.Combine(Path.GetTempPath(), $"Retorno360_InventarioLog_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            var sbLog = new StringBuilder();
            try
            {
                sbLog.AppendLine($"Inicio exportación inventarios: {DateTime.Now:O}");
                int idRazon = SelectedIdRazon; int idEmpresa = SelectedIdEmpresa;
                // La animación de carga se mostrará después de que el usuario confirme la carpeta (SaveFileDialog)
                sbLog.AppendLine($"SelectedIdRazon={idRazon}, SelectedIdEmpresa={idEmpresa}");
                if (idRazon == 0)
                {
                    sbLog.AppendLine("Abortado: no se seleccionó razón social.");
                    MessageBox.Show("Selecciona razón social antes de cargar inventario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Si el checkbox de cargar todas las razones y empresas está marcado, recorrer todas las razones y empresas desde NOM_TABLARAZON/RAZONXTABLA
                if (chkCargarTodasRazonesEmpresas != null && chkCargarTodasRazonesEmpresas.Checked)
                {
                    DataTable dtRazones = new DataTable();
                    try
                    {
                        Conexion conexion = new Conexion();
                        // Obtener IdRazon y nombre de la razón para crear una hoja por razón
                        string sqlRaz = "SELECT DISTINCT r.IdRazon, r.NOMBRE_RAZON FROM RAZONXTABLA r JOIN NOM_TABLARAZON n ON n.IdRazon = r.IdRazon";
                        using (SqlConnection connection = new SqlConnection(conexion.GetConnectionString()))
                        using (SqlCommand cmdRaz = new SqlCommand(sqlRaz, connection))
                        using (SqlDataAdapter daRaz = new SqlDataAdapter(cmdRaz))
                        {
                            daRaz.Fill(dtRazones);
                        }
                    }
                    catch (Exception ex)
                    {
                        sbLog.AppendLine($"Error al listar razones: {ex}");
                        MessageBox.Show($"Error al listar razones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // escribir log y salir
                        try { File.WriteAllText(logPath, sbLog.ToString()); } catch { }
                        return;
                    }

                    if (dtRazones.Rows.Count == 0)
                    {
                        MessageBox.Show("No se encontraron razones ni empresas.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using var sfdAll = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_TodasRazonesEmpresas_{DateTime.Now:yyyyMMdd}.xlsx" };
                    if (sfdAll.ShowDialog(this) != DialogResult.OK) return;

                    // El usuario seleccionó carpeta/archivo: mostrar panel de carga antes de iniciar consultas largas
                    try { MostrarPanelCargando(true); } catch { }
                    try { EstablecerEstadoBotonesDuranteCarga(true); } catch { }

                    using var wbAll = new XLWorkbook();

                    foreach (DataRow rowR in dtRazones.Rows)
                    {
                        if (rowR == null) continue;
                        int idR = 0; int.TryParse(rowR[0].ToString(), out idR);

                        // Obtener empresas asociadas a la razón (desde NOM_TABLARAZON)
                        DataTable dtEmpresasForRazon = new DataTable();
                        try
                        {
                            Conexion conexion2 = new Conexion();
                            string sqlEmp = "SELECT IdTabla, NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
                            using (SqlConnection connection2 = new SqlConnection(conexion2.GetConnectionString()))
                            using (SqlCommand cmdEmpList = new SqlCommand(sqlEmp, connection2))
                            using (SqlDataAdapter daEmp = new SqlDataAdapter(cmdEmpList))
                            {
                                cmdEmpList.Parameters.AddWithValue("@IdRazon", idR);
                                daEmp.Fill(dtEmpresasForRazon);
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        if (dtEmpresasForRazon.Rows.Count == 0) continue;

                        foreach (DataRow row in dtEmpresasForRazon.Rows)
                        {
                            if (row == null) continue;
                            int idEmp = 0; int.TryParse(row[0].ToString(), out idEmp);
                            string nombreEmpresa = row[1]?.ToString() ?? string.Empty;

                            // Reutilizar lógica de consulta para una empresa (como antes) y agregar hoja por empresa
                            var dtEmpresa = new DataTable();
                            dtEmpresa.Columns.Add("Par_NoParte"); dtEmpresa.Columns.Add("Par_DescripcionIng"); dtEmpresa.Columns.Add("Par_DescripcionEsp"); dtEmpresa.Columns.Add("Fra_FraccionMex"); dtEmpresa.Columns.Add("Med_Clave"); dtEmpresa.Columns.Add("Pac_Costo"); dtEmpresa.Columns.Add("Pac_FechaFin"); dtEmpresa.Columns.Add("Par_PesoUnit"); dtEmpresa.Columns.Add("Par_UMPeso"); dtEmpresa.Columns.Add("Tim_Clave"); dtEmpresa.Columns.Add("Pai_Origen"); dtEmpresa.Columns.Add("Tco_Clave"); dtEmpresa.Columns.Add("Par_Activo");

                            try
                            {
                                var svcEmp = new VerificacionPartesService();
                                string connStrEmp = await svcEmp.ObtenerCadenaConexionPublicAsync(idR, idEmp);

                                using (var connEmp = new SqlConnection(connStrEmp))
                                {
                                    connEmp.Open();
                                    string sql = @"select  cp.Par_NoParte, cp.Par_DescripcionIng, cp.Par_DescripcionEsp, fa.Fra_FraccionMex, cp.Med_Clave, cpc.Pac_Costo, cpc.Pac_FechaFin, cp.Par_PesoUnit, cp.Par_UMPeso, cp.Tim_Clave, cp.Pai_Origen, cpc.Tco_Clave, cp.Par_Activo
                                                        from Ca_Parte cp
                                                        inner join Ca_ParteCosto cpc on cp.Par_Consecutivo = cpc.Par_Consecutivo
                                                        inner join vFracciones fa on fa.Par_Consecutivo = cp.Par_Consecutivo";

                                    using var cmdEmp = new SqlCommand(sql, connEmp);
                                    using var rdrEmp = cmdEmp.ExecuteReader();
                                    while (rdrEmp.Read())
                                    {
                                        var rowEmpresa = dtEmpresa.NewRow();
                                        rowEmpresa[0] = rdrEmp.IsDBNull(0) ? string.Empty : rdrEmp.GetString(0);
                                        rowEmpresa[1] = rdrEmp.IsDBNull(1) ? string.Empty : rdrEmp.GetString(1);
                                        rowEmpresa[2] = rdrEmp.IsDBNull(2) ? string.Empty : rdrEmp.GetString(2);
                                        rowEmpresa[3] = rdrEmp.IsDBNull(3) ? string.Empty : rdrEmp.GetString(3);
                                        rowEmpresa[4] = rdrEmp.IsDBNull(4) ? string.Empty : rdrEmp.GetString(4);
                                        rowEmpresa[5] = rdrEmp.IsDBNull(5) ? (object)0m : rdrEmp.GetDecimal(5);
                                        rowEmpresa[6] = rdrEmp.IsDBNull(6) ? string.Empty : rdrEmp.GetDateTime(6).ToString("yyyy-MM-dd");
                                        rowEmpresa[7] = rdrEmp.IsDBNull(7) ? (object)0m : rdrEmp.GetDecimal(7);
                                        rowEmpresa[8] = rdrEmp.IsDBNull(8) ? string.Empty : rdrEmp.GetString(8);
                                        rowEmpresa[9] = rdrEmp.IsDBNull(9) ? string.Empty : rdrEmp.GetString(9);
                                        rowEmpresa[10] = rdrEmp.IsDBNull(10) ? string.Empty : rdrEmp.GetString(10);
                                        rowEmpresa[11] = rdrEmp.IsDBNull(11) ? string.Empty : rdrEmp.GetString(11);
                                        rowEmpresa[12] = rdrEmp.IsDBNull(12) ? string.Empty : rdrEmp.GetString(12);
                                        dtEmpresa.Rows.Add(rowEmpresa);
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            string sheetName = string.IsNullOrWhiteSpace(nombreEmpresa) ? $"Empresa_{idEmp}" : nombreEmpresa;
                            if (wbAll.Worksheets.Any(w => string.Equals(w.Name, sheetName, StringComparison.OrdinalIgnoreCase)))
                                sheetName = sheetName + "_" + idEmp;

                            var wsEmpresa = wbAll.Worksheets.Add(sheetName);
                            for (int c = 0; c < dtEmpresa.Columns.Count; c++) wsEmpresa.Cell(1, c + 1).Value = dtEmpresa.Columns[c].ColumnName;
                            for (int r = 0; r < dtEmpresa.Rows.Count; r++)
                            {
                                int targetRow = r + 2;
                                if (targetRow < 1)
                                {
                                    sbLog.AppendLine($"SALTANDO escritura: targetRow ({targetRow}) < 1 para IdEmpresa={idEmp}, Hoja={sheetName}");
                                    continue;
                                }
                                if (targetRow > 1048576)
                                {
                                    sbLog.AppendLine($"SALTANDO escritura: targetRow ({targetRow}) > 1048576 para IdEmpresa={idEmp}, Hoja={sheetName}");
                                    break;
                                }
                                for (int c = 0; c < dtEmpresa.Columns.Count; c++)
                                {
                                    var val = dtEmpresa.Rows[r][c];
                                    try { wsEmpresa.Cell(targetRow, c + 1).SetValue(val == null ? string.Empty : val.ToString()); } catch { wsEmpresa.Cell(targetRow, c + 1).Value = val?.ToString() ?? string.Empty; }
                                }
                            }
                            wsEmpresa.Columns().AdjustToContents();
                        }
                    }

                    try
                    {
                        if (wbAll.Worksheets == null || wbAll.Worksheets.Count == 0)
                        {
                            MessageBox.Show("No se encontraron inventarios para exportar. Ninguna hoja fue creada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Intentar guardar con manejo de archivo bloqueado
                            var savedAllPath = SaveWorkbookWithFallback(wbAll, sfdAll.FileName, sbLog);
                            if (savedAllPath != null)
                            {
                                var ask2 = MessageBox.Show("Inventarios exportados a Excel. ¿Deseas abrir el archivo ahora?", "Exportado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (ask2 == DialogResult.Yes)
                                {
                                    try { Process.Start(new ProcessStartInfo(savedAllPath) { UseShellExecute = true }); } catch { }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar inventarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // Ocultar panel de carga y restaurar UI
                        try { MostrarPanelCargando(false); } catch { }
                        try { EstablecerEstadoBotonesDuranteCarga(false); } catch { }
                    }

                    return;
                }

                if (idEmpresa == 0) { MessageBox.Show("Selecciona empresa antes de cargar inventario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // Obtener cadena de conexión de la empresa seleccionada
                var svc = new VerificacionPartesService();
                string connStr = await svc.ObtenerCadenaConexionPublicAsync(idRazon, idEmpresa);

                // Ejecutar consulta y exportar a Excel
                var dt = new DataTable();
                dt.Columns.Add("Par_NoParte"); dt.Columns.Add("Par_DescripcionIng"); dt.Columns.Add("Par_DescripcionEsp"); dt.Columns.Add("Fra_FraccionMex"); dt.Columns.Add("Med_Clave"); dt.Columns.Add("Pac_Costo"); dt.Columns.Add("Pac_FechaFin"); dt.Columns.Add("Par_PesoUnit"); dt.Columns.Add("Par_UMPeso"); dt.Columns.Add("Tim_Clave"); dt.Columns.Add("Pai_Origen"); dt.Columns.Add("Tco_Clave"); dt.Columns.Add("Par_Activo");

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"select  cp.Par_NoParte, cp.Par_DescripcionIng, cp.Par_DescripcionEsp, fa.Fra_FraccionMex, cp.Med_Clave, cpc.Pac_Costo, cpc.Pac_FechaFin, cp.Par_PesoUnit, cp.Par_UMPeso, cp.Tim_Clave, cp.Pai_Origen, cpc.Tco_Clave, cp.Par_Activo
from Ca_Parte cp
inner join Ca_ParteCosto cpc on cp.Par_Consecutivo = cpc.Par_Consecutivo
inner join vFracciones fa on fa.Par_Consecutivo = cp.Par_Consecutivo";

                    using var cmd = new SqlCommand(sql, conn);
                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var row = dt.NewRow();
                        row[0] = rdr.IsDBNull(0) ? string.Empty : rdr.GetString(0);
                        row[1] = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                        row[2] = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2);
                        row[3] = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);
                        row[4] = rdr.IsDBNull(4) ? string.Empty : rdr.GetString(4);
                        row[5] = rdr.IsDBNull(5) ? (object)0m : rdr.GetDecimal(5);
                        row[6] = rdr.IsDBNull(6) ? string.Empty : rdr.GetDateTime(6).ToString("yyyy-MM-dd");
                        row[7] = rdr.IsDBNull(7) ? (object)0m : rdr.GetDecimal(7);
                        row[8] = rdr.IsDBNull(8) ? string.Empty : rdr.GetString(8);
                        row[9] = rdr.IsDBNull(9) ? string.Empty : rdr.GetString(9); // Tim_Clave
                        row[10] = rdr.IsDBNull(10) ? string.Empty : rdr.GetString(10);
                        row[11] = rdr.IsDBNull(11) ? string.Empty : rdr.GetString(11);
                        row[12] = rdr.IsDBNull(12) ? string.Empty : rdr.GetString(12);
                        dt.Rows.Add(row);
                    }
                }

                // Registrar resultado simple al log
                try { sbLog.AppendLine($"Consulta individual finalizada. Filas obtenidas para IdEmpresa={idEmpresa}: {dt.Rows.Count}"); } catch { }

                try { File.WriteAllText(logPath, sbLog.ToString()); } catch { }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron partes para la empresa seleccionada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = $"Inventario_{cmbEmpresa?.Text}_{DateTime.Now:yyyyMMdd}.xlsx" };
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                // Mostrar panel de carga ahora que el usuario eligió destino
                try { MostrarPanelCargando(true); } catch { }
                try { EstablecerEstadoBotonesDuranteCarga(true); } catch { }

                using var wb = new XLWorkbook();
                // Soportar grandes volúmenes: si dt.Rows excede límite de filas, crear múltiples hojas Inventario, Inventario_part2, ...
                const int MAX_ROWS = 1048576;
                string baseSheet = "Inventario";
                int partIndex = 1;
                string MakeUniqueName(string desired)
                {
                    var name = desired;
                    int idx = 1;
                    while (wb.Worksheets.Any(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        idx++;
                        name = desired + "_" + idx;
                    }
                    return name;
                }

                // escribir filas distribuidas en hojas
                var currentSheetName = MakeUniqueName(baseSheet);
                var wsCurrent = wb.Worksheets.Add(currentSheetName);
                // exportar cabeceras
                for (int c = 0; c < dt.Columns.Count; c++) wsCurrent.Cell(1, c + 1).Value = dt.Columns[c].ColumnName;
                int currentRow = 2;
                sbLog.AppendLine($"Escribiendo filas en hojas a partir de '{currentSheetName}'. Total filas: {dt.Rows.Count}");

                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    if (currentRow > MAX_ROWS)
                    {
                        // crear nueva hoja de continuación
                        partIndex++;
                        currentSheetName = MakeUniqueName(baseSheet + "_part" + partIndex);
                        wsCurrent = wb.Worksheets.Add(currentSheetName);
                        for (int c = 0; c < dt.Columns.Count; c++) wsCurrent.Cell(1, c + 1).Value = dt.Columns[c].ColumnName;
                        currentRow = 2;
                        sbLog.AppendLine($"Creada hoja de continuación: {currentSheetName}");
                    }

                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        var val = dt.Rows[r][c];
                        try { wsCurrent.Cell(currentRow, c + 1).SetValue(val == null ? string.Empty : val.ToString()); } catch { wsCurrent.Cell(currentRow, c + 1).Value = val?.ToString() ?? string.Empty; }
                    }
                    currentRow++;
                }

                // Ajustar columnas en todas las hojas inventario creadas
                foreach (var sh in wb.Worksheets.Where(w => w.Name.StartsWith(baseSheet, StringComparison.OrdinalIgnoreCase)))
                {
                    try { sh.Columns().AdjustToContents(); } catch { }
                }
                wb.SaveAs(sfd.FileName);
                // Ocultar panel de carga y restaurar UI después de guardar
                try { MostrarPanelCargando(false); } catch { }
                try { EstablecerEstadoBotonesDuranteCarga(false); } catch { }

                // Guardar individual: manejar archivo bloqueado similar a flujo masivo
                string savedSingle = null;
                try
                {
                    savedSingle = SaveWorkbookWithFallback(wb, sfd.FileName, sbLog);
                }
                catch { }

                try
                {
                    var ask = MessageBox.Show("Inventario exportado a Excel. ¿Deseas abrir el archivo ahora?", "Exportado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (ask == DialogResult.Yes)
                    {
                        try
                        {
                            var psi = new ProcessStartInfo((savedSingle ?? sfd.FileName)) { UseShellExecute = true };
                            Process.Start(psi);
                        }
                        catch { /* no crítico */ }
                    }
                }
                catch
                {
                    try { MessageBox.Show("Inventario exportado a Excel.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar inventario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Generar PDF con los datos del preview (total, razón social, empresa, mes)
        private void btnExportarExcel_Click_1(object? sender, EventArgs e)
        {
            try
            {
                // Determinar ids y año a exportar
                int idRazon = SelectedIdRazon;
                int idEmpresa = SelectedIdEmpresa;

                if (idRazon == 0 || idEmpresa == 0)
                {
                    MessageBox.Show("Selecciona razón social y empresa antes de exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Año a exportar: intentar extraer de lblMesAno (yyyy or yyyy-MM or "Mes: MMMM yyyy")
                int year = DateTime.Now.Year;
                try
                {
                    var txt = lblMesAno?.Text ?? string.Empty;
                    if (txt.Contains(":")) txt = txt.Substring(txt.IndexOf(":") + 1).Trim();
                    if (System.Text.RegularExpressions.Regex.IsMatch(txt, "^\\d{4}-\\d{2}$"))
                    {
                        year = int.Parse(txt.Substring(0, 4));
                    }
                    else
                    {
                        if (DateTime.TryParseExact(txt, "MMMM yyyy", new System.Globalization.CultureInfo("es-ES"), System.Globalization.DateTimeStyles.None, out DateTime dt))
                        {
                            year = dt.Year;
                        }
                        else
                        {
                            var m = System.Text.RegularExpressions.Regex.Match(txt, "\\d{4}");
                            if (m.Success)
                                year = int.Parse(m.Value);
                        }
                    }
                }
                catch { }

                // Consultar la suma por mes desde la tabla historial (join header->detalle)
                var valoresPorMes = new decimal[13]; // 1..12
                for (int i = 1; i <= 12; i++) valoresPorMes[i] = 0m;

                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(new CNX.Conexion().GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"SELECT MONTH(hc.FechaCalculo) AS Mes, ISNULL(SUM(hci.TotalCosto),0) AS TotalMes 
                                   FROM Historial_CalculoCostos hc 
                                   JOIN Historial_CalculoInventario hci ON hc.IdCalculo = hci.IdCalculo
                                   WHERE hc.IdEmpresa = @IdEmpresa AND hc.IdRazonSocial = @IdRazon AND YEAR(hc.FechaCalculo) = @Anio
                                   GROUP BY MONTH(hc.FechaCalculo)";

                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                        cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                        cmd.Parameters.AddWithValue("@Anio", year);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int mes = rdr.GetInt32(0);
                                decimal total = rdr.IsDBNull(1) ? 0m : rdr.GetDecimal(1);
                                if (mes >= 1 && mes <= 12) valoresPorMes[mes] = total;
                            }
                        }
                    }
                }

                // Preparar archivo Excel
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Workbook|*.xlsx";
                    sfd.FileName = $"Historial_{cmbRazonSocial?.Text}_{cmbEmpresa?.Text}_{year}.xlsx";
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("Historial");

                        // Guardar logo temporal desde recursos (intentar varias fuentes)
                        string tmpLogo = null;
                        try
                        {
                            System.Drawing.Image? img = null;
                            try
                            {
                                // Intentar mediante ResourceManager para nombres variados
                                var rm = Retorno360Tacna.Properties.Resources.ResourceManager;
                                object? o = rm.GetObject("logo_tacna") ?? rm.GetObject("Bitmap1") ?? rm.GetObject("bitmap1") ?? rm.GetObject("Logo_tacna");
                                if (o is System.Drawing.Image im) img = im;
                            }
                            catch { }

                            // Si no se obtuvo desde Resources, intentar archivo físico en carpeta Resources
                            if (img == null)
                            {
                                try
                                {
                                    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Bitmap1.bmp");
                                    if (System.IO.File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                                }
                                catch { }
                            }

                            // continuar con la inserción del logo (si se obtuvo)

                            if (img != null)
                            {
                                tmpLogo = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "logo_tacna_export.png");
                                using (var ms = new System.IO.MemoryStream())
                                {
                                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    System.IO.File.WriteAllBytes(tmpLogo, ms.ToArray());
                                }
                                // Añadir imagen si ClosedXML lo permite (no es crítico)
                                try
                                {
                                    var pic = ws.AddPicture(tmpLogo);
                                    // Asegurar espacio en filas/columnas superiores
                                    try { ws.Row(1).Height = 60; } catch { }
                                    try { ws.Column(1).Width = 18; } catch { }
                                    // Posicionar y escalar
                                    try { pic.MoveTo(ws.Cell(1, 1)); pic.Scale(0.6); } catch { try { pic.MoveTo(ws.Cell(1, 1)); } catch { } }
                                }
                                catch { }
                            }
                        }
                        catch { }

                        // Área del logo: combinar celdas A1:C4 y colocar la imagen encima
                        try { ws.Range(ws.Cell(1, 1), ws.Cell(4, 3)).Merge(); } catch { }
                        // Títulos y metadatos (colocar a la derecha del logo)
                        int baseCol = 4; // dejar columnas 1-3 para el logo
                        ws.Range(ws.Cell(1, baseCol), ws.Cell(1, baseCol + 12)).Merge().Value = "INVENTARIO DE MATERIA PRIMA";
                        ws.Range(ws.Cell(1, baseCol), ws.Cell(1, baseCol + 12)).Style.Font.Bold = true;
                        ws.Range(ws.Cell(1, baseCol), ws.Cell(1, baseCol + 12)).Style.Font.FontSize = 14;
                        ws.Range(ws.Cell(2, baseCol), ws.Cell(2, baseCol + 12)).Merge().Value = $"EMPRESA: {cmbEmpresa?.Text ?? string.Empty}";
                        ws.Range(ws.Cell(3, baseCol), ws.Cell(3, baseCol + 12)).Merge().Value = $"RAZÓN SOCIAL: {cmbRazonSocial?.Text ?? string.Empty}";
                        ws.Range(ws.Cell(4, baseCol), ws.Cell(4, baseCol + 12)).Merge().Value = $"PERÍODO: ENERO - DICIEMBRE {year}";
                        ws.Range(ws.Cell(2, baseCol), ws.Cell(4, baseCol + 12)).Style.Font.FontSize = 11;

                        // Encabezados de meses en fila 6, iniciando en baseCol
                        ws.Cell(6, baseCol).Value = "EMPRESA";
                        var mesesNombres = new[] { "", "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE" };
                        for (int m = 1; m <= 12; m++)
                        {
                            ws.Cell(6, baseCol + m).Value = mesesNombres[m];
                            ws.Cell(6, baseCol + m).Style.Font.Bold = true;
                            ws.Cell(6, baseCol + m).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
                            ws.Cell(6, baseCol + m).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        }

                        // Fila de empresa en fila 7
                        ws.Cell(7, baseCol).Value = cmbRazonSocial?.Text ?? string.Empty;
                        for (int m = 1; m <= 12; m++)
                        {
                            var cell = ws.Cell(7, baseCol + m);
                            decimal val = valoresPorMes[m];
                            cell.Value = val;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            if (val == 0m)
                            {
                                // marcar en rojo
                                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FFCDD2");
                            }
                        }

                        // Formato de columnas
                        ws.Column(1).Width = 18;
                        ws.Column(2).Width = 6;
                        ws.Column(3).Width = 6;
                        for (int c = baseCol; c <= baseCol + 12; c++) ws.Column(c).Width = 18;

                        // Crear hoja de detalle con las filas de Historial_CalculoInventario asociadas al año/razon/empresa
                        try
                        {
                            using (var conn2 = new Microsoft.Data.SqlClient.SqlConnection(new CNX.Conexion().GetConnectionString()))
                            {
                                conn2.Open();
                                string sqlDet = @"SELECT hc.IdCalculo, hci.NoParte, hci.Cantidad, hci.UM, hci.CostoUnit, hci.TotalCosto, hc.FechaCalculo
                                                    FROM Historial_CalculoCostos hc
                                                    JOIN Historial_CalculoInventario hci ON hc.IdCalculo = hci.IdCalculo
                                                    WHERE hc.IdEmpresa = @IdEmpresa AND hc.IdRazonSocial = @IdRazon AND YEAR(hc.FechaCalculo) = @Anio
                                                    ORDER BY hc.FechaCalculo, hci.NoParte";

                                using (var cmdDet = new Microsoft.Data.SqlClient.SqlCommand(sqlDet, conn2))
                                {
                                    cmdDet.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                                    cmdDet.Parameters.AddWithValue("@IdRazon", idRazon);
                                    cmdDet.Parameters.AddWithValue("@Anio", year);

                                    using (var rdr = cmdDet.ExecuteReader())
                                    {
                                        var wsDet = wb.Worksheets.Add("Detalle");
                                        // Encabezados
                                        wsDet.Cell(1, 1).Value = "IdCalculo";
                                        wsDet.Cell(1, 2).Value = "FechaCalculo";
                                        wsDet.Cell(1, 3).Value = "NoParte";
                                        wsDet.Cell(1, 4).Value = "Cantidad";
                                        wsDet.Cell(1, 5).Value = "UM";
                                        wsDet.Cell(1, 6).Value = "CostoUnit";
                                        wsDet.Cell(1, 7).Value = "TotalCosto";
                                        for (int i = 1; i <= 7; i++) { wsDet.Cell(1, i).Style.Font.Bold = true; }

                                        int rowDet = 2;
                                        while (rdr.Read())
                                        {
                                            int idc = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                                            string nop = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                                            int cant = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                                            string um = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);
                                            decimal costoUnit = rdr.IsDBNull(4) ? 0m : rdr.GetDecimal(4);
                                            decimal totalC = rdr.IsDBNull(5) ? 0m : rdr.GetDecimal(5);
                                            DateTime fc = rdr.IsDBNull(6) ? DateTime.MinValue : rdr.GetDateTime(6);

                                            wsDet.Cell(rowDet, 1).Value = idc;
                                            wsDet.Cell(rowDet, 2).Value = fc == DateTime.MinValue ? "" : fc.ToString("yyyy-MM-dd");
                                            wsDet.Cell(rowDet, 3).Value = nop;
                                            wsDet.Cell(rowDet, 4).Value = cant;
                                            wsDet.Cell(rowDet, 5).Value = um;
                                            wsDet.Cell(rowDet, 6).Value = costoUnit;
                                            wsDet.Cell(rowDet, 7).Value = totalC;

                                            wsDet.Cell(rowDet, 6).Style.NumberFormat.Format = "#,##0.0000";
                                            wsDet.Cell(rowDet, 7).Style.NumberFormat.Format = "#,##0.00";

                                            rowDet++;
                                        }

                                        // Ajustes de ancho
                                        wsDet.Columns().AdjustToContents();
                                    }
                                }
                            }
                        }
                        catch { }

                        wb.SaveAs(sfd.FileName);

                        // eliminar logo temporal
                        try { if (!string.IsNullOrEmpty(tmpLogo) && System.IO.File.Exists(tmpLogo)) System.IO.File.Delete(tmpLogo); } catch { }
                    }

                    try
                    {
                        var ask = MessageBox.Show("Excel generado. ¿Deseas abrir el archivo ahora?", "Exportado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (ask == DialogResult.Yes)
                        {
                            try
                            {
                                var psi = new ProcessStartInfo(sfd.FileName) { UseShellExecute = true };
                                Process.Start(psi);
                            }
                            catch { /* no crítico */ }
                        }
                    }
                    catch
                    {
                        try { MessageBox.Show("Excel generado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int SelectedIdEmpresa
        {
            get
            {
                if (cmbEmpresa != null && cmbEmpresa.SelectedValue != null && int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int id))
                    return id;
                return 0;
            }
        }

        public FrmCalculoInventarios() : this(null) { }

        public FrmCalculoInventarios(MODELS.Usuario? usuario)
        {
            InitializeComponent();
            usuarioActual = usuario;

            // Si se proporcionó usuario, inicializar el servicio de perfil
            if (usuario != null)
                perfilService = new SERVICES.PerfilUsuarioService();

            this.Load += FrmCalculoInventarios_Load;
            // Registrar sólo una vez el manejador del botón de historial
            try { btnHistorialVerificacion.Click += btnHistorialVerificacion_Click; } catch { }
            // Registrar el manejador del botón de recalcular (limpia el formulario)
            try { btnRecalcular.Click += btnRecalcular_Click; } catch { }
            // Registrar actualizador de gráfico al cambiar razón/empresa
            try { cmbRazonSocial.SelectedIndexChanged += (s, e) => UpdateChartMeses(); } catch { }
            try { cmbEmpresa.SelectedIndexChanged += (s, e) => UpdateChartMeses(); } catch { }
            // Aplicar estilo UI consistente
            try { ApplyUiStyling(); } catch { }

            // Ajustes de ventana: quitar botones de minimizar/maximizar/cerrar, centrar y evitar mover
            try
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                this.ControlBox = false; // elimina los botones de control
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
            }
            catch { }
        }

        // Limpia todos los campos del formulario para reiniciar el proceso
        private void btnRecalcular_Click(object? sender, EventArgs e)
        {
            try
            {
                // Limpiar selecciones y controles visibles
                try { if (cmbRazonSocial != null) cmbRazonSocial.SelectedIndex = -1; } catch { }
                try { if (cmbEmpresa != null) cmbEmpresa.SelectedIndex = -1; } catch { }
                try { if (chkUsarPerfil != null) chkUsarPerfil.Checked = false; } catch { }

                // Limpiar etiquetas de totales / periodo
                try { if (lblTotalGeneral != null) lblTotalGeneral.Text = "Total general: 0"; } catch { }
                try { if (lblMesAno != null) lblMesAno.Text = string.Empty; } catch { }

                // Limpiar grid de resultados
                try
                {
                    if (dgvRelsultados != null)
                    {
                        dgvRelsultados.Rows.Clear();
                        dgvRelsultados.Columns.Clear();
                    }
                }
                catch { }

                // Intentar restablecer otros estados temporales si existen
                try { /* aquí pueden añadirse resets de variables internas si se requieren */ } catch { }

                // Mensaje al usuario
                try { lblTotalGeneral.Text = "Total general: 0"; } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reiniciar el formulario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyUiStyling()
        {
            try
            {
                // Mantener cambios mínimos: fondo y fuente por defecto. El resto del diseño debe estar en el diseñador.
                this.BackColor = Color.White;
                this.Font = new Font("Segoe UI", 9F);

                try { cmbRazonSocial.DropDownStyle = ComboBoxStyle.DropDownList; cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList; } catch { }
                // chkExportarTodasEmpresas fue retirado; no suscribir eventos inexistentes.
                try { chkCargarTodasRazonesEmpresas.CheckedChanged += chkCargarTodasRazonesEmpresas_CheckedChanged; } catch { }
            }
            catch { }
        }

        // Evitar que la ventana sea movida por el usuario (bloquear comando de movimiento y arrastre del caption)
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;

            try
            {
                if (m.Msg == WM_SYSCOMMAND && ((int)m.WParam == SC_MOVE))
                {
                    // Ignorar intento de mover
                    return;
                }
                if (m.Msg == WM_NCLBUTTONDOWN && m.WParam == (IntPtr)HTCAPTION)
                {
                    // Ignorar clics en la barra de título que inician el arrastre
                    return;
                }
            }
            catch { }

            base.WndProc(ref m);
        }

        private void btnHistorialVerificacion_Click(object? sender, EventArgs e)
        {
            try
            {
                var frm = new FrmHistorialExplorer(SelectedIdRazon, SelectedIdEmpresa);
                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir historial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Anexa las filas del DataTable al DataGridView dgvRelsultados y actualiza el label de total
        // Hacer público interno para permitir llamadas desde formularios del mismo ensamblado
        internal void AppendResultadoParaAgregar(System.Data.DataTable dt, decimal totalCosto)
        {
            try
            {
                // LOG: registrar intento de agregar resultados para depuración
                try
                {
                    var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Retorno360Tacna_AddResultados.log");
                    var sbLog = new System.Text.StringBuilder();
                    sbLog.AppendLine($"[{DateTime.Now:O}] AppendResultadoParaAgregar invoked");
                    sbLog.AppendLine($"TotalCosto param: {totalCosto}");
                    if (dt == null)
                    {
                        sbLog.AppendLine("DataTable == null");
                    }
                    else
                    {
                        sbLog.AppendLine($"DataTable Columns: {dt.Columns.Count}, Rows: {dt.Rows.Count}");
                        sbLog.AppendLine("Columns:");
                        foreach (System.Data.DataColumn col in dt.Columns)
                        {
                            sbLog.AppendLine($" - {col.ColumnName} ({col.DataType?.Name})");
                        }
                        int maxRows = Math.Min(10, dt.Rows.Count);
                        sbLog.AppendLine($"First {maxRows} rows (as CSV):");
                        for (int r = 0; r < maxRows; r++)
                        {
                            var row = dt.Rows[r];
                            var parts = new string[dt.Columns.Count];
                            for (int c = 0; c < dt.Columns.Count; c++) parts[c] = row[c]?.ToString() ?? "";
                            sbLog.AppendLine(string.Join(",", parts));
                        }
                    }
                    sbLog.AppendLine("StackTrace:");
                    sbLog.AppendLine(Environment.StackTrace ?? "");
                    sbLog.AppendLine(new string('-', 80));
                    System.IO.File.AppendAllText(logPath, sbLog.ToString());
                }
                catch { }

                if (dt == null || dt.Rows.Count == 0) return;

                // Si dgvRelsultados no tiene columnas, crear columnas desde el DataTable
                if (dgvRelsultados.Columns.Count == 0)
                {
                    foreach (System.Data.DataColumn col in dt.Columns)
                    {
                        dgvRelsultados.Columns.Add(col.ColumnName, col.ColumnName);
                    }
                }

                // Insertar filas
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    var vals = new object[dt.Columns.Count];
                    for (int i = 0; i < dt.Columns.Count; i++) vals[i] = dr[i];
                    dgvRelsultados.Rows.Add(vals);
                }

                // Recalcular total general sumando la columna de "total costo" presente en dgvRelsultados
                try
                {
                    // Determinar índice de la columna que representa el total por fila
                    int idxTotal = -1;
                    // 1) Preferir columna con nombre exacto usado en el diálogo de verificación
                    for (int i = 0; i < dgvRelsultados.Columns.Count; i++)
                    {
                        var nm = (dgvRelsultados.Columns[i].Name ?? string.Empty);
                        if (string.Equals(nm, "colTotalCosto", StringComparison.OrdinalIgnoreCase))
                        {
                            idxTotal = i; break;
                        }
                    }
                    // 2) Preferir encabezado exacto "Total Costo"
                    if (idxTotal == -1)
                    {
                        for (int i = 0; i < dgvRelsultados.Columns.Count; i++)
                        {
                            var hdr = (dgvRelsultados.Columns[i].HeaderText ?? string.Empty);
                            if (string.Equals(hdr.Trim(), "Total Costo", StringComparison.OrdinalIgnoreCase) || string.Equals(hdr.Trim(), "TotalCosto", StringComparison.OrdinalIgnoreCase))
                            {
                                idxTotal = i; break;
                            }
                        }
                    }
                    // 3) Fallback: buscar columna que contenga "total" en el encabezado y no contenga "unit" (evitar Costo Unit.)
                    if (idxTotal == -1)
                    {
                        for (int i = 0; i < dgvRelsultados.Columns.Count; i++)
                        {
                            var hdr = (dgvRelsultados.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                            if (hdr.Contains("total") && !hdr.Contains("unit") && !hdr.Contains("unit.") && !hdr.Contains("unitario"))
                            {
                                idxTotal = i; break;
                            }
                        }
                    }

                    decimal suma = 0m;
                    if (idxTotal >= 0)
                    {
                        foreach (DataGridViewRow r in dgvRelsultados.Rows)
                        {
                            if (r.IsNewRow) continue;
                            var v = r.Cells[idxTotal].Value;
                            if (v == null) continue;
                            if (decimal.TryParse(v.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal parsed))
                            {
                                suma += parsed;
                            }
                            else
                            {
                                // intentar parse con InvariantCulture por si usa punto
                                if (decimal.TryParse(v.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed))
                                    suma += parsed;
                            }
                        }
                    }

                    lblTotalGeneral.Text = $"Total general: {suma:N2}";
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al anexar resultados: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Guardar preview (dgvRelsultados) en tablas de historial

        private void FrmCalculoInventarios_Load(object sender, EventArgs e)
        {
            // Poblamos los combos de razón social y empresas para que la
            // verificación utilice la base de datos correcta.
            try
            {
                CargarRazonesSociales();

                // Asegurar que el evento no se suscriba doblemente
                try { cmbRazonSocial.SelectedIndexChanged -= cmbRazonSocial_SelectedIndexChanged; } catch { }
                cmbRazonSocial.SelectedIndexChanged += cmbRazonSocial_SelectedIndexChanged;

                // Si ya hay una razón seleccionada, cargar empresas
                if (cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
                {
                    CargarEmpresas(idRazon);
                }

                ActualizarEstadoPlantilla();
            }
            catch
            {
                // No bloquear la carga del formulario por errores en carga de combos
            }
        }

        // Implementaciones mínimas para poblar y reaccionar a los combos de
        // razón social y empresa. Estas versiones son livianas y solo cubren
        // el flujo necesario para la verificación de partes.
        private void CargarRazonesSociales()
        {
            try
            {
                // Si el usuario pidió usar su perfil y existe el servicio, preferir la lista del perfil
                if (usuarioActual != null && perfilService != null && chkUsarPerfil.Checked)
                {
                    var razones = perfilService.ObtenerRazonesSocialesDePerfil(usuarioActual.IdUsuario);
                    cmbRazonSocial.DataSource = null;
                    cmbRazonSocial.DisplayMember = "NombreRazon";
                    cmbRazonSocial.ValueMember = "IdRazon";
                    cmbRazonSocial.DataSource = razones;
                    if (razones.Count > 0) cmbRazonSocial.SelectedIndex = 0;
                    return;
                }

                // Consulta general a RAZONXTABLA en RetornoMaster
                Conexion conexion = new Conexion();
                string sql = "SELECT IdRazon, Nombre_Razon FROM RAZONXTABLA ORDER BY Nombre_Razon";
                using SqlConnection connection = new SqlConnection(conexion.GetConnectionString());
                using SqlDataAdapter da = new SqlDataAdapter(sql, connection);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    cmbRazonSocial.DataSource = null;
                    string displayCol = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "Nombre_Razon", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;
                    string valueCol = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "IdRazon", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    cmbRazonSocial.DisplayMember = displayCol;
                    cmbRazonSocial.ValueMember = valueCol;
                    cmbRazonSocial.DataSource = dt;
                    cmbRazonSocial.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las razones sociales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbRazonSocial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRazonSocial == null || cmbEmpresa == null) return;
            if (cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
            {
                CargarEmpresas(idRazon);
            }
            else
            {
                cmbEmpresa.DataSource = null;
            }
            try { UpdateChartMeses(); } catch { }
        }

        private void CargarEmpresas(int idRazon)
        {
            try
            {
                if (usuarioActual != null && perfilService != null && chkUsarPerfil.Checked)
                {
                    var empresas = perfilService.ObtenerEmpresasDePerfilPorRazon(usuarioActual.IdUsuario, idRazon);
                    cmbEmpresa.DataSource = null;
                    if (empresas.Count > 0)
                    {
                        cmbEmpresa.DisplayMember = "NombreTabla";
                        cmbEmpresa.ValueMember = "IdTabla";
                        cmbEmpresa.DataSource = empresas;
                        cmbEmpresa.SelectedIndex = 0;
                    }
                    else
                    {
                        cmbEmpresa.DataSource = null;
                    }

                    ActualizarEstadoPlantilla();
                    return;
                }

                Conexion conexion = new Conexion();
                string sql = "SELECT IdTabla, NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
                using SqlConnection connection = new SqlConnection(conexion.GetConnectionString());
                using SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                using SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    cmbEmpresa.DataSource = null;
                    string displayCol = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "NOMBRE_TABLA", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;
                    string valueCol = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)
                        .FirstOrDefault(n => string.Equals(n, "IdTabla", StringComparison.OrdinalIgnoreCase))
                        ?? dt.Columns[0].ColumnName;

                    cmbEmpresa.DisplayMember = displayCol;
                    cmbEmpresa.ValueMember = valueCol;
                    cmbEmpresa.DataSource = dt;
                    cmbEmpresa.SelectedIndex = 0;
                }
                else
                {
                    cmbEmpresa.DataSource = null;
                }

                ActualizarEstadoPlantilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las empresas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarEstadoPlantilla()
        {
            try
            {
                if (cmbEmpresa != null && cmbEmpresa.SelectedValue != null && int.TryParse(cmbEmpresa.SelectedValue.ToString(), out int idEmpresa))
                {
                    var cfg = PlantillaInventarioServicio.ObtenerParaEmpresa(idEmpresa);
                    if (cfg != null && cfg.EstaConfigurada)
                    {
                        try { lblPlantillaInfo.Text = $"??  Plantilla: {Path.GetFileName(cfg.RutaArchivo)}  |  Hoja: {cfg.Hoja}  |  Operaci�n: {cfg.Operacion}"; } catch { }
                        try { lblPlantillaInfo.ForeColor = Color.FromArgb(22, 90, 50); } catch { }

                        return;
                    }
                }

                try { lblPlantillaInfo.Text = "??  Sin plantilla para esta empresa  (configura una en Configuraci�n)"; } catch { }
                try { lblPlantillaInfo.ForeColor = Color.FromArgb(120, 60, 30); } catch { }

            }
            catch { }
        }

        public async System.Threading.Tasks.Task VerificarPartesAsync(
            int idRazon,
            int idEmpresa,
            List<(string Parte, decimal Cantidad, string Unidad)> entradas)
        {
            var svc = new VerificacionPartesService();
            await svc.VerificarPartesConCantidadAsync(idRazon, idEmpresa, entradas);
        }

        // Actualiza el panel pnlChart mostrando por mes si existe cálculo en Historial_CalculoCostos
        private void UpdateChartMeses()
        {
            try
            {
                if (pnlChart == null) return;

                int idRazon = SelectedIdRazon;
                int idEmpresa = SelectedIdEmpresa;

                // Preparar datos de meses (0 = NO, 1 = SI)
                var valores = Enumerable.Range(1, 12).Select(i => 0).ToArray();

                if (idRazon != 0 && idEmpresa != 0)
                {
                    try
                    {
                        using var conn = new Microsoft.Data.SqlClient.SqlConnection(new CNX.Conexion().GetConnectionString());
                        conn.Open();
                        string sql = @"SELECT MONTH(FechaCalculo) AS Mes, COUNT(1) AS Cant
                                       FROM Historial_CalculoCostos
                                       WHERE IdRazonSocial = @IdRazon AND IdEmpresa = @IdEmpresa
                                       GROUP BY MONTH(FechaCalculo)";
                        using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                        cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                        using var rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            int mes = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                            int cnt = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                            if (mes >= 1 && mes <= 12) valores[mes - 1] = cnt > 0 ? 1 : 0;
                        }
                    }
                    catch { }
                }

                // Render simple chart using labels inside the panel: un control ligero
                try
                {
                    pnlChart.SuspendLayout();
                    pnlChart.Controls.Clear();
                    for (int i = 0; i < 12; i++)
                    {
                        var lbl = new Label();
                        lbl.AutoSize = false;
                        lbl.TextAlign = ContentAlignment.MiddleCenter;
                        lbl.Size = new Size(40, 120);
                        lbl.Location = new Point(10 + i * 45, 10);
                        lbl.Text = new DateTime(DateTime.Now.Year, i + 1, 1).ToString("MMM", new System.Globalization.CultureInfo("es-ES")) + "\n" + (valores[i] == 1 ? "SI" : "NO");
                        lbl.BackColor = valores[i] == 1 ? Color.FromArgb(30, 150, 70) : Color.FromArgb(230, 230, 230);
                        lbl.ForeColor = valores[i] == 1 ? Color.White : Color.FromArgb(80, 80, 80);
                        lbl.BorderStyle = BorderStyle.FixedSingle;
                        pnlChart.Controls.Add(lbl);
                    }
                    pnlChart.ResumeLayout();
                }
                catch { }
            }
            catch { }
        }

        // Intenta mapear columnas del archivo subido usando la plantilla configurada.
        // Devuelve true si se pudo obtener hoja y nombres de columnas (parte, cantidad, um).
        private bool TryAutoMapUsingTemplate(string plantillaPath, string plantillaHoja, XLWorkbook uploadedWorkbook, out string sheetNombre, out string colParte, out string colCant, out string colUm)
        {
            sheetNombre = string.Empty; colParte = string.Empty; colCant = string.Empty; colUm = string.Empty;
            try
            {
                if (!File.Exists(plantillaPath)) return false;
                using var tplWb = new XLWorkbook(plantillaPath);

                var tplWs = tplWb.Worksheets.FirstOrDefault(w => string.Equals(w.Name, plantillaHoja, StringComparison.OrdinalIgnoreCase))
                    ?? tplWb.Worksheets.FirstOrDefault();
                if (tplWs == null) return false;

                // Detectar fila de encabezado en plantilla
                int tplLastRow = tplWs.LastRowUsed()?.RowNumber() ?? 0;
                int tplHeaderRow = 1;
                for (int r = 1; r <= Math.Min(tplLastRow, 20); r++)
                {
                    var row = tplWs.Row(r);
                    if (row.CellsUsed().Any()) { tplHeaderRow = r; break; }
                }

                var headersTpl = new List<string>();
                int tplLastCol = tplWs.Row(tplHeaderRow).LastCellUsed()?.Address.ColumnNumber ?? 0;
                for (int c = 1; c <= tplLastCol; c++) headersTpl.Add(tplWs.Cell(tplHeaderRow, c).GetString().Trim());

                // Revisar cada hoja del archivo subido y buscar coincidencias de nombres de columnas
                foreach (var upWs in uploadedWorkbook.Worksheets)
                {
                    int upLastRow = upWs.LastRowUsed()?.RowNumber() ?? 0;
                    int upHeaderRow = 1;
                    for (int r = 1; r <= Math.Min(upLastRow, 20); r++) { if (upWs.Row(r).CellsUsed().Any()) { upHeaderRow = r; break; } }
                    int upLastCol = upWs.Row(upHeaderRow).LastCellUsed()?.Address.ColumnNumber ?? 0;

                    var headersUp = new List<string>();
                    for (int c = 1; c <= upLastCol; c++) headersUp.Add(upWs.Cell(upHeaderRow, c).GetString().Trim());

                    // Intentar encontrar columna de parte comparando nombres exactos o contención
                    string foundParte = string.Empty, foundCant = string.Empty, foundUm = string.Empty;
                    foreach (var h in headersTpl)
                    {
                        if (string.IsNullOrWhiteSpace(h)) continue;
                        // Heurística: buscar campos conocidos en plantilla como 'Parte','NoParte','Cantidad','UM','Unidad'
                        if (RegexMatchAny(h, new[] { "parte", "no parte", "no_parte", "no_parte", "noParte", "noParte" }))
                        {
                            // buscar coincidencia en headersUp
                            var match = headersUp.FirstOrDefault(x => x.Equals(h, StringComparison.OrdinalIgnoreCase) || x.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0);
                            if (match != null) foundParte = match;
                        }
                        if (string.IsNullOrWhiteSpace(foundCant) && RegexMatchAny(h, new[] { "cantidad", "cant", "qty", "quantity" }))
                        {
                            var match = headersUp.FirstOrDefault(x => x.Equals(h, StringComparison.OrdinalIgnoreCase) || x.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0);
                            if (match != null) foundCant = match;
                        }
                        if (string.IsNullOrWhiteSpace(foundUm) && RegexMatchAny(h, new[] { "um", "unidad", "unidad_medida", "uom" }))
                        {
                            var match = headersUp.FirstOrDefault(x => x.Equals(h, StringComparison.OrdinalIgnoreCase) || x.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0);
                            if (match != null) foundUm = match;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(foundParte))
                    {
                        sheetNombre = upWs.Name;
                        colParte = foundParte;
                        colCant = foundCant;
                        colUm = foundUm;
                        return true;
                    }
                }

                return false;
            }
            catch { return false; }
        }

        private bool RegexMatchAny(string input, string[] terms)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var low = input.ToLowerInvariant();
            foreach (var t in terms) if (low.Contains(t.ToLowerInvariant())) return true;
            return false;
        }

        // Genera una lista de claves yyyy-MM para los n meses anteriores (incluye mes actual)
        private System.Collections.Generic.IEnumerable<string> GenerarMesesRecientes(int meses)
        {
            var lista = new System.Collections.Generic.List<string>();
            var ahora = DateTime.Now;
            for (int i = 0; i < meses; i++)
            {
                var dt = ahora.AddMonths(-i);
                lista.Add(dt.ToString("yyyy-MM"));
            }
            return lista;
        }

        private async void btnVerificarPartes_Click(object sender, EventArgs e)
        {
            try
            {
                using var frm = new FrmIngresarPartes();
                // Poblar combos de año/mes con valores recientes para que el usuario pueda elegir
                try
                {
                    frm.CargarMeses(GenerarMesesRecientes(12), DateTime.Now.ToString("yyyy-MM"));
                }
                catch { }

                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                string? texto = frm.PartesIngresadas;
                if (string.IsNullOrWhiteSpace(texto))
                {
                    MessageBox.Show("No se ingresaron números de parte.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int idRazon = 0, idEmpresa = 0;
                if (cmbRazonSocial != null && cmbRazonSocial.SelectedValue != null)
                    int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out idRazon);
                if (cmbEmpresa != null && cmbEmpresa.SelectedValue != null)
                    int.TryParse(cmbEmpresa.SelectedValue.ToString(), out idEmpresa);

                // Requerir seleccion de razón social y empresa para determinar la BD
                if (idRazon == 0 || idEmpresa == 0)
                {
                    MessageBox.Show("Selecciona una razón social y una empresa antes de verificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var entradas = new List<(string Parte, decimal Cantidad, string Unidad)>();
                var lines = texto.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in lines)
                {
                    string line = raw.Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Contains(',')
                        ? line.Split(',')
                        : line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    string parte = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                    decimal cantidad = 1m;
                    string unidad = string.Empty;

                    if (parts.Length == 2)
                    {
                        if (!decimal.TryParse(parts[1].Trim(), out cantidad))
                            unidad = parts[1].Trim();
                    }
                    else if (parts.Length >= 3)
                    {
                        bool parsed = false;
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var t = parts[i].Trim();
                            if (!parsed && decimal.TryParse(t, out decimal q))
                            {
                                cantidad = q; parsed = true; continue;
                            }
                            if (!string.IsNullOrEmpty(t) && string.IsNullOrEmpty(unidad)) unidad = t;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(parte)) continue;
                    entradas.Add((parte, cantidad, unidad));
                }

                if (entradas.Count == 0)
                {
                    MessageBox.Show("No se encontraron filas válidas para verificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var servicio = new VerificacionPartesService();
                var resultado = await servicio.VerificarPartesConCantidadAsync(idRazon, idEmpresa, entradas);

                using var frmRes = new FrmResultadoVerificacion(resultado);
                // Pasar al formulario de resultados los ids seleccionados para evitar
                // que vuelva a solicitarlos desde el Owner cuando el usuario agregue filas.
                frmRes.SelectedIdRazon = this.SelectedIdRazon;
                frmRes.SelectedIdEmpresa = this.SelectedIdEmpresa;
                // Pasar mes seleccionado al diálogo para que lo muestre en su lblFecha
                try
                {
                    if (!string.IsNullOrWhiteSpace(frm.MesSeleccionado))
                    {
                        var parts = frm.MesSeleccionado.Split('-');
                        string mostrar = frm.MesSeleccionado;
                        if (parts.Length >= 2 && int.TryParse(parts[0], out int y) && int.TryParse(parts[1], out int m))
                        {
                            try { mostrar = new System.DateTime(y, m, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")); } catch { mostrar = frm.MesSeleccionado; }
                        }
                        try { frmRes.FechaTexto = mostrar; frmRes.MesKey = frm.MesSeleccionado; } catch { }
                    }
                }
                catch { }

                frmRes.ShowDialog(this);

                // Si el diálogo devolvió un DataTable para agregar, anexarlo a dgvRelsultados
                try
                {
                    if (frmRes.ResultadoParaAgregar != null && frmRes.ResultadoParaAgregar.Rows.Count > 0)
                    {
                        AppendResultadoParaAgregar(frmRes.ResultadoParaAgregar, frmRes.TotalCostoParaAgregar);
                        // Actualizar etiqueta de mes/año para el preview (usar FechaTexto legible si está disponible)
                        try
                        {
                            string textoMes = !string.IsNullOrWhiteSpace(frmRes.FechaTexto) ? frmRes.FechaTexto : (!string.IsNullOrWhiteSpace(frmRes.MesKey) ? frmRes.MesKey : (frm.MesSeleccionado ?? string.Empty));
                            if (!string.IsNullOrWhiteSpace(textoMes)) lblMesAno.Text = $"Mes: {textoMes}";
                        }
                        catch { }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar partes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ----------------------------------------------------
        // Stubs para eventos que aún declara el diseñador pero cuya lógica
        // original se eliminó. Mantener los stubs evita errores de compilación
        // y permite reimplementar comportamiento si es necesario.
        // ----------------------------------------------------
        private void btnIniciarCalculo_Click(object? sender, EventArgs e)
        {
            // Intencionalmente vacío: la lógica de inicio de cálculo fue removida.
        }

        private async void btnAnalizarExcel_Click(object? sender, EventArgs e)
        {
            try
            {
                using var ofd = new OpenFileDialog() { Filter = "Excel|*.xlsx;*.xls", Multiselect = false };
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                var ruta = ofd.FileName;
                var layout = new MODELS.ExcelLayoutModel();
                try { layout.CargarArchivo(ruta); } catch (Exception ex) { MessageBox.Show($"No se pudo abrir el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                if (layout.Hojas == null || layout.Hojas.Count == 0)
                {
                    MessageBox.Show("El archivo no contiene hojas detectables.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Usar el formulario FrmMapearColumnas para mapear campos (editorable desde el diseñador)
                using var mapForm = new FrmMapearColumnas() { IdRazon = this.SelectedIdRazon, IdEmpresa = this.SelectedIdEmpresa };
                try { mapForm.LoadLayout(layout); } catch { /* seguir */ }
                if (mapForm.ShowDialog(this) != DialogResult.OK) return;

                var hojaSel = mapForm.SelectedSheet;
                var colParte = mapForm.SelectedColParte;
                var colCant = mapForm.SelectedColCantidad;
                var colUm = mapForm.SelectedColUM;

                if (string.IsNullOrWhiteSpace(colParte)) { MessageBox.Show("Selecciona la columna que contiene el número de parte.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // Leer la hoja y obtener filas
                var entradas = new System.Collections.Generic.List<(string Parte, decimal Cantidad, string Unidad)>();
                var totalesPorFila = new System.Collections.Generic.List<decimal?>();
                // lista temporal para costos unitarios por fila (declarada fuera del using para usarla luego)
                var costosUnitariosTemp = new System.Collections.Generic.List<decimal?>();
                using (var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheets.FirstOrDefault(w => string.Equals(w.Name, hojaSel, StringComparison.OrdinalIgnoreCase));
                    if (ws == null) { MessageBox.Show("Hoja no encontrada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                    // Detectar fila de encabezado similar a ExcelLayoutModel
                    int ultimaFila = ws.LastRowUsed()?.RowNumber() ?? 0;
                    int filaMax = Math.Min(ultimaFila, 20);
                    int filaEnc = 0;
                    for (int r = 1; r <= filaMax; r++)
                    {
                        var row = ws.Row(r);
                        if (row.CellsUsed().Any())
                        {
                            // Comprobar que contiene la columna de parte (por nombre)
                            bool tieneParte = false;
                            foreach (var c in row.Cells())
                            {
                                var val = c.GetString().Trim();
                                if (string.Equals(val, colParte, StringComparison.OrdinalIgnoreCase) || val.IndexOf(colParte, StringComparison.OrdinalIgnoreCase) >= 0)
                                { tieneParte = true; break; }
                            }
                            if (tieneParte) { filaEnc = r; break; }
                        }
                    }
                    if (filaEnc == 0) filaEnc = 1;

                    int ultimaCol = ws.Row(filaEnc).LastCellUsed()?.Address.ColumnNumber ?? 0;
                    // localizar indices de las columnas seleccionadas
                    int idxParte = -1, idxCant = -1, idxUm = -1, idxTotal = -1, idxCostoUnit = -1;
                    var colTotalName = mapForm.SelectedColTotalCosto;
                    var colCostoUnitName = mapForm.SelectedColCostoUnitario;
                    for (int c = 1; c <= ultimaCol; c++)
                    {
                        var txt = ws.Cell(filaEnc, c).GetString().Trim();
                        if (string.Equals(txt, colParte, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(colParte, StringComparison.OrdinalIgnoreCase) >= 0) idxParte = c;
                        if (!string.IsNullOrWhiteSpace(colCant) && (string.Equals(txt, colCant, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(colCant, StringComparison.OrdinalIgnoreCase) >= 0)) idxCant = c;
                        if (!string.IsNullOrWhiteSpace(colUm) && (string.Equals(txt, colUm, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(colUm, StringComparison.OrdinalIgnoreCase) >= 0)) idxUm = c;
                        if (!string.IsNullOrWhiteSpace(colTotalName) && (string.Equals(txt, colTotalName, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(colTotalName, StringComparison.OrdinalIgnoreCase) >= 0)) idxTotal = c;
                        if (!string.IsNullOrWhiteSpace(colCostoUnitName) && (string.Equals(txt, colCostoUnitName, StringComparison.OrdinalIgnoreCase) || txt.IndexOf(colCostoUnitName, StringComparison.OrdinalIgnoreCase) >= 0)) idxCostoUnit = c;
                    }

                    if (idxParte == -1) { MessageBox.Show("No se pudo localizar la columna de número de parte en la hoja seleccionada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? filaEnc;

                    for (int r = filaEnc + 1; r <= lastRow; r++)
                    {
                        var cellParte = ws.Cell(r, idxParte).GetString()?.Trim() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(cellParte)) continue;
                        string sCant = idxCant > 0 ? ws.Cell(r, idxCant).GetString()?.Trim() ?? string.Empty : string.Empty;
                        string sUm = idxUm > 0 ? ws.Cell(r, idxUm).GetString()?.Trim() ?? string.Empty : string.Empty;

                        decimal cantidad = 1m;
                        if (!string.IsNullOrWhiteSpace(sCant))
                        {
                            if (!decimal.TryParse(sCant, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out cantidad))
                                decimal.TryParse(sCant, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cantidad);
                        }

                        // leer columna Costo Unitario si fue seleccionada
                        decimal? costoUnit = null;
                        if (idxCostoUnit > 0)
                        {
                            var sCost = ws.Cell(r, idxCostoUnit).GetString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(sCost))
                            {
                                if (!decimal.TryParse(sCost, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal cu))
                                    decimal.TryParse(sCost, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cu);
                                costoUnit = cu;
                            }
                        }

                        // si no hay costo unitario pero hay columna total, intentar derivar unitario = total / cantidad
                        if (costoUnit == null && idxTotal > 0)
                        {
                            var sTot = ws.Cell(r, idxTotal).GetString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(sTot))
                            {
                                if (!decimal.TryParse(sTot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal t))
                                    decimal.TryParse(sTot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out t);
                                if (cantidad != 0) costoUnit = t / cantidad;
                            }
                        }

                        entradas.Add((cellParte, cantidad, sUm));
                        // añadir temporalmente a una lista local; la variable final se construirá fuera
                        costosUnitariosTemp.Add(costoUnit);
                    }
                }

                if (entradas.Count == 0) { MessageBox.Show("No se encontraron filas válidas en el archivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                // trasladar valores temporales de costos unitarios a la lista final que estará en scope
                var costosUnitariosPorFila = costosUnitariosTemp != null ? new System.Collections.Generic.List<decimal?>(costosUnitariosTemp) : new System.Collections.Generic.List<decimal?>();

                int idRazon = SelectedIdRazon; int idEmpresa = SelectedIdEmpresa;
                if (idRazon == 0 || idEmpresa == 0) { MessageBox.Show("Selecciona razón social y empresa antes de analizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                var svc = new VerificacionPartesService();
                // Si se seleccionó columna de costo unitario en el mapeo, pasar la lista de costos unitarios por fila
                var resultado = await svc.VerificarPartesConCantidadAsync(idRazon, idEmpresa, entradas, costosUnitariosPorFila.Count > 0 ? costosUnitariosPorFila : null);

                // Convertir resultado a DataTable y agregar al grid
                var dt = new DataTable();
                dt.Columns.Add("NoParte"); dt.Columns.Add("Cantidad"); dt.Columns.Add("UM"); dt.Columns.Add("CostoUnit"); dt.Columns.Add("TotalCosto");
                decimal totalGeneral = 0m;
                foreach (var it in resultado.Items)
                {
                    var row = dt.NewRow();
                    row[0] = it.Parte;
                    row[1] = it.Cantidad;
                    row[2] = it.UnidadUsuario ?? it.MedComercial;
                    row[3] = it.CostoUnitario;
                    row[4] = it.TotalCosto;
                    dt.Rows.Add(row);
                    totalGeneral += it.TotalCosto;
                }

                // Anexar al grid (usa método existente)
                try { AppendResultadoParaAgregar(dt, totalGeneral); }
                catch
                { /* fallback: intentar añadir manualmente */
                    foreach (DataRow r in dt.Rows)
                    {
                        var vals = new object[r.Table.Columns.Count];
                        for (int i = 0; i < r.Table.Columns.Count; i++) vals[i] = r[i];
                        dgvRelsultados.Rows.Add(vals);
                    }
                    try { lblTotalGeneral.Text = $"Total general: {totalGeneral:N2}"; } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al analizar Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbEmpresa_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try { UpdateChartMeses(); } catch { }
        }



        private void chkCargarTodasRazonesEmpresas_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                // No ejecutar automáticamente aquí.
                // La acción se evalúa y ejecuta en btnCargarInventario_Click según el estado del checkbox.
            }
            catch { }
        }

        private void MostrarPanelCargando(bool mostrar)
        {
            if (panelCargando == null) return;
            if (panelCargando.InvokeRequired)
            {
                panelCargando.Invoke(new Action(() => MostrarPanelCargando(mostrar)));
                return;
            }

            if (mostrar)
            {
                panelCargando.Left = (this.ClientSize.Width - panelCargando.Width) / 2;
                panelCargando.Top = (this.ClientSize.Height - panelCargando.Height) / 2;
                panelCargando.BringToFront();
                panelCargando.Visible = true;
                try { if (progressBarCargando.Style != ProgressBarStyle.Marquee) progressBarCargando.Style = ProgressBarStyle.Marquee; } catch { }
            }
            else
            {
                panelCargando.Visible = false;
            }

            Application.DoEvents();
        }

        private void EstablecerEstadoBotonesDuranteCarga(bool cargando)
        {
            try
            {
                var botones = ObtenerBotonesRecursivamente(this).Where(b => b != null).ToList();
                if (cargando)
                {
                    // almacenar estado actual
                    estadoBotonesAntesCarga = botones.ToDictionary(b => b, b => b.Enabled);
                    foreach (var boton in botones) boton.Enabled = false;
                }
                else if (estadoBotonesAntesCarga != null)
                {
                    foreach (var item in estadoBotonesAntesCarga)
                    {
                        if (!item.Key.IsDisposed)
                            item.Key.Enabled = item.Value;
                    }
                    estadoBotonesAntesCarga = null;
                }
            }
            catch { }
        }

        private static IEnumerable<Button> ObtenerBotonesRecursivamente(Control control)
        {
            foreach (Control hijo in control.Controls)
            {
                if (hijo is Button boton) yield return boton;
                foreach (var b in ObtenerBotonesRecursivamente(hijo)) yield return b;
            }
        }

        private Dictionary<Button, bool>? estadoBotonesAntesCarga;

        /// <summary>
        /// Intenta guardar el workbook en la ruta indicada. Si está bloqueado, intentará guardar con sufijo numerado hasta 10 intentos.
        /// Retorna la ruta final guardada o null si falló.
        /// </summary>
        private string? SaveWorkbookWithFallback(XLWorkbook workbook, string desiredPath, StringBuilder sbLog)
        {
            try
            {
                workbook.SaveAs(desiredPath);
                sbLog.AppendLine($"Archivo guardado: {desiredPath}");
                return desiredPath;
            }
            catch (Exception ex)
            {
                sbLog.AppendLine($"Fallo al guardar en ruta solicitada: {ex.Message}");
                // Intentar con sufijos numerados
                var dir = Path.GetDirectoryName(desiredPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var name = Path.GetFileNameWithoutExtension(desiredPath);
                var ext = Path.GetExtension(desiredPath);
                for (int i = 1; i <= 10; i++)
                {
                    var tryPath = Path.Combine(dir, $"{name}({i}){ext}");
                    try
                    {
                        workbook.SaveAs(tryPath);
                        sbLog.AppendLine($"Archivo guardado en ruta alternativa: {tryPath}");
                        return tryPath;
                    }
                    catch (Exception ex2)
                    {
                        sbLog.AppendLine($"Intento {i} falló guardando en {tryPath}: {ex2.Message}");
                        continue;
                    }
                }

                sbLog.AppendLine("No fue posible guardar el archivo tras varios intentos.");
                MessageBox.Show("No fue posible guardar el archivo de Excel. Verifica si el archivo está abierto por otro programa o prueba otra carpeta.", "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void chkUsarPerfil_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                // Cuando el usuario cambia esta opción, recargar las razones/empresas
                CargarRazonesSociales();
                // Si ya hay una razón seleccionada, forzar recarga de empresas
                if (cmbRazonSocial != null && cmbRazonSocial.SelectedValue != null && int.TryParse(cmbRazonSocial.SelectedValue.ToString(), out int idRazon))
                    CargarEmpresas(idRazon);
            }
            catch { }
        }

        private void btnGuardarCalculos_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dgvRelsultados.Rows.Count == 0) { MessageBox.Show("No hay datos para guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                // Obtener IdRazon / IdEmpresa
                int idRazon = SelectedIdRazon;
                int idEmpresa = SelectedIdEmpresa;
                if (idRazon == 0 || idEmpresa == 0) { MessageBox.Show("Selecciona razón social y empresa antes de guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // Fecha/mes desde lblMesAno (espera formato "Mes: <texto>" o clave yyyy-MM)
                string mesKey = string.Empty;
                try
                {
                    var txt = lblMesAno?.Text ?? string.Empty;
                    if (txt.Contains(":")) txt = txt.Substring(txt.IndexOf(":") + 1).Trim();
                    // intentar convertir texto legible a yyyy-MM
                    if (DateTime.TryParseExact(txt, "MMMM yyyy", new System.Globalization.CultureInfo("es-ES"), System.Globalization.DateTimeStyles.None, out DateTime dt))
                        mesKey = dt.ToString("yyyy-MM");
                    else if (System.Text.RegularExpressions.Regex.IsMatch(txt, "^\\d{4}-\\d{2}$")) mesKey = txt;
                }
                catch { }

                // Total general desde lblTotalGeneral: texto "Total general: 123.45"
                decimal totalGeneral = 0m;
                try
                {
                    var t = lblTotalGeneral?.Text ?? string.Empty;
                    var m = System.Text.RegularExpressions.Regex.Match(t, "([0-9.,]+)$");
                    if (m.Success)
                    {
                        var s = m.Groups[1].Value;
                        if (!decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out totalGeneral))
                            decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out totalGeneral);
                    }
                }
                catch { }

                // Insertar en Historial_CalculoCostos y obtener IdCalculo
                int idCalculo = 0;
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(new CNX.Conexion().GetConnectionString()))
                {
                    conn.Open();
                    using (var tr = conn.BeginTransaction())
                    {
                        try
                        {
                            string sqlIns = "INSERT INTO Historial_CalculoCostos (IdEmpresa, IdRazonSocial, FechaCalculo, TotalCosto) VALUES (@IdEmpresa, @IdRazonSocial, @FechaCalculo, @TotalCosto); SELECT SCOPE_IDENTITY();";
                            // Calcular fechaCalculo fuera del using para reutilizar en detalle
                            DateTime fechaCalculo = DateTime.Now;
                            if (!string.IsNullOrWhiteSpace(mesKey))
                            {
                                var parts = mesKey.Split('-');
                                if (parts.Length == 2 && int.TryParse(parts[0], out int y) && int.TryParse(parts[1], out int mo)) fechaCalculo = new DateTime(y, mo, 1);
                            }
                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlIns, conn, tr))
                            {
                                cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                                cmd.Parameters.AddWithValue("@IdRazonSocial", idRazon);
                                // FechaCalculo: primer día del mes calculado (para agrupaciones mensuales)
                                cmd.Parameters.AddWithValue("@FechaCalculo", fechaCalculo);
                                cmd.Parameters.AddWithValue("@TotalCosto", totalGeneral);
                                var res = cmd.ExecuteScalar();
                                if (res != null && int.TryParse(res.ToString(), out int idc)) idCalculo = idc;
                            }

                            if (idCalculo <= 0) throw new Exception("No se pudo crear registro en Historial_CalculoCostos.");

                            // Preparar inserción masiva para detalle (sin IdConsecutivo)
                            // Insert detalle incluye ahora columna Fecha (DATETIME2 NOT NULL)
                            string sqlInsDet = "INSERT INTO Historial_CalculoInventario (IdCalculo, Fecha, NoParte, Cantidad, UM, CostoUnit, TotalCosto) VALUES (@IdCalculo, @Fecha, @NoParte, @Cantidad, @UM, @CostoUnit, @TotalCosto);";
                            using (var cmdDet = new Microsoft.Data.SqlClient.SqlCommand(sqlInsDet, conn, tr))
                            {
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IdCalculo", System.Data.SqlDbType.Int));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Fecha", System.Data.SqlDbType.DateTime2));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@NoParte", System.Data.SqlDbType.VarChar, 500));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Cantidad", System.Data.SqlDbType.Int));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UM", System.Data.SqlDbType.VarChar, 50));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@CostoUnit", System.Data.SqlDbType.Decimal));
                                cmdDet.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@TotalCosto", System.Data.SqlDbType.Decimal));

                                foreach (DataGridViewRow row in dgvRelsultados.Rows)
                                {
                                    if (row.IsNewRow) continue;
                                    // Mapear columnas por nombre/encabezado
                                    string noParte = row.Cells[0].Value?.ToString() ?? string.Empty;
                                    int cantidad = 0; decimal costoUnit = 0m; decimal totalCosto = 0m; string um = string.Empty;
                                    // intentar buscar columnas por encabezado
                                    for (int i = 0; i < dgvRelsultados.Columns.Count; i++)
                                    {
                                        var hdr = (dgvRelsultados.Columns[i].HeaderText ?? dgvRelsultados.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                                        var val = row.Cells[i].Value?.ToString() ?? string.Empty;
                                        if (hdr.Contains("parte")) noParte = val;
                                        else if (hdr.Contains("cant")) int.TryParse(val.Replace(".", string.Empty), out cantidad);
                                        else if (hdr.Contains("um") && !hdr.Contains("bd")) um = val;
                                        else if (hdr.Contains("costo unit") || hdr.Contains("costounit") || hdr.Contains("unit")) decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out costoUnit);
                                        else if (hdr.Contains("total")) decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out totalCosto);
                                    }

                                    cmdDet.Parameters["@IdCalculo"].Value = idCalculo;
                                    cmdDet.Parameters["@Fecha"].Value = fechaCalculo;
                                    cmdDet.Parameters["@NoParte"].Value = noParte;
                                    cmdDet.Parameters["@Cantidad"].Value = cantidad;
                                    cmdDet.Parameters["@UM"].Value = um;
                                    cmdDet.Parameters["@CostoUnit"].Value = costoUnit;
                                    cmdDet.Parameters["@TotalCosto"].Value = totalCosto;
                                    cmdDet.ExecuteNonQuery();
                                }
                            }

                            tr.Commit();
                        }
                        catch
                        {
                            try { tr.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                MessageBox.Show("Guardado completado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkCargarTodasRazonesEmpresas_CheckedChanged_1(object sender, EventArgs e)
        {
            // Evento reservado por el diseñador.
        }
    }
}
