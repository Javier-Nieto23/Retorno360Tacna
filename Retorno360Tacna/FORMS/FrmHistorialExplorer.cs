using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmHistorialExplorer : Form
    {
        private int initialRazonId;
        private int initialEmpresaId;

        public FrmHistorialExplorer(int selectedRazonId = 0, int selectedEmpresaId = 0)
        {
            initialRazonId = selectedRazonId;
            initialEmpresaId = selectedEmpresaId;
            InitializeComponent();
            Load += FrmHistorialExplorer_Load;
        }



        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmHistorialExplorer_Load(object? sender, EventArgs e)
        {
            cmbRazon.Items.Clear();
            cmbEmpresa.Items.Clear();

            // Cargar razones sociales con nombres legibles
            cmbRazon.Items.Add(new ComboBoxItem("Todas", 0));
            try
            {
                using (var conn = new SqlConnection(new Conexion().GetConnectionString()))
                {
                    conn.Open();
                    string sqlRazones = "SELECT DISTINCT hc.IdRazonSocial, rs.NOMBRE_RAZON FROM Historial_CalculoCostos hc LEFT JOIN RAZONXTABLA rs ON hc.IdRazonSocial = rs.IdRazon ORDER BY rs.NOMBRE_RAZON";
                    using (var cmd = new SqlCommand(sqlRazones, conn))
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            int id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                            string nombre = rdr.IsDBNull(1) ? $"Id {id}" : rdr.GetString(1);
                            cmbRazon.Items.Add(new ComboBoxItem(nombre, id));
                        }
                    }
                }
            }
            catch { }

            // Seleccionar el valor inicial
            SelectComboBoxValue(cmbRazon, initialRazonId);

            // Cuando se cambie la razón social, cargar empresas asociadas
            cmbRazon.SelectedIndexChanged += (s, ea) =>
            {
                int selRazon = GetSelectedValue(cmbRazon);
                LoadEmpresasForRazon(selRazon);
            };

            // Si se tenía una razón seleccionada, disparar la carga de empresas
            LoadEmpresasForRazon(initialRazonId);
        }

        private void LoadEmpresasForRazon(int idRazon)
        {
            cmbEmpresa.Items.Clear();
            cmbEmpresa.Items.Add(new ComboBoxItem("Todas", 0));
            try
            {
                using (var conn = new SqlConnection(new Conexion().GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT hc.IdEmpresa, ISNULL(n.NOMBRE_TABLA, 'Id ' + CONVERT(varchar(50), hc.IdEmpresa)) AS NombreEmpresa FROM Historial_CalculoCostos hc LEFT JOIN NOM_TABLARAZON n ON n.IdTabla = hc.IdEmpresa WHERE (@IdRazon=0 OR hc.IdRazonSocial=@IdRazon) ORDER BY NombreEmpresa";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                                string nombre = rdr.IsDBNull(1) ? $"Id {id}" : rdr.GetString(1);
                                cmbEmpresa.Items.Add(new ComboBoxItem(nombre, id));
                            }
                        }
                    }
                }
            }
            catch { }

            SelectComboBoxValue(cmbEmpresa, initialEmpresaId);
        }

        private void SelectComboBoxValue(ComboBox cb, int val)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (cb.Items[i] is ComboBoxItem it && it.Value == val) { cb.SelectedIndex = i; return; }
            }
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
        }

        private void BtnBuscar_Click(object? sender, EventArgs e)
        {
            int idRazon = GetSelectedValue(cmbRazon);
            int idEmpresa = GetSelectedValue(cmbEmpresa);
            if (idRazon == 0 && idEmpresa == 0)
            {
                MessageBox.Show("Elija una razon social y/o un cliente", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {


                int year = (int)nudYear.Value;

                LoadSummary(idRazon, idEmpresa, year);
                LoadDetails(idRazon, idEmpresa, year, month: 0);
            }
        }

        private int GetSelectedValue(ComboBox cb)
        {
            if (cb.SelectedItem is ComboBoxItem it) return it.Value;
            return 0;
        }

        private void LoadSummary(int idRazon, int idEmpresa, int year)
        {
            dgvSummary.Columns.Clear();
            dgvSummary.Rows.Clear();
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mes", Name = "colMes" });
            dgvSummary.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", Name = "colTotal" });

            decimal grandTotal = 0m;

            try
            {
                using (var conn = new SqlConnection(new Conexion().GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"SELECT MONTH(hc.FechaCalculo) AS Mes, ISNULL(SUM(hci.TotalCosto),0) AS TotalMes
                                   FROM Historial_CalculoCostos hc
                                   JOIN Historial_CalculoInventario hci ON hc.IdCalculo = hci.IdCalculo
                                   WHERE YEAR(hc.FechaCalculo)=@Anio";
                    if (idEmpresa > 0) sql += " AND hc.IdEmpresa = @IdEmpresa";
                    if (idRazon > 0) sql += " AND hc.IdRazonSocial = @IdRazon";
                    sql += " GROUP BY MONTH(hc.FechaCalculo)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Anio", year);
                        if (idEmpresa > 0) cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                        if (idRazon > 0) cmd.Parameters.AddWithValue("@IdRazon", idRazon);

                        var totals = new decimal[13];
                        for (int i = 1; i <= 12; i++) totals[i] = 0m;

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int m = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                                decimal t = rdr.IsDBNull(1) ? 0m : rdr.GetDecimal(1);
                                if (m >= 1 && m <= 12) totals[m] = t;
                            }
                        }

                        for (int m = 1; m <= 12; m++)
                        {
                            dgvSummary.Rows.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m), totals[m].ToString("N2"));
                            grandTotal += totals[m];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar resumen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lblTotal.Text = $"Total: {grandTotal:N2}";
        }

        private void LoadDetails(int idRazon, int idEmpresa, int year, int month)
        {
            dgvDetails.Columns.Clear();
            dgvDetails.Rows.Clear();

            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IdCalculo", Name = "colIdCalculo" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", Name = "colFecha" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "NoParte", Name = "colNoParte" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", Name = "colCantidad" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UM", Name = "colUM" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CostoUnit", Name = "colCostoUnit" });
            dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TotalCosto", Name = "colTotalCosto" });

            try
            {
                using (var conn = new SqlConnection(new Conexion().GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"SELECT hc.IdCalculo, hci.NoParte, hci.Cantidad, hci.UM, hci.CostoUnit, hci.TotalCosto, hc.FechaCalculo
                                   FROM Historial_CalculoCostos hc
                                   JOIN Historial_CalculoInventario hci ON hc.IdCalculo = hci.IdCalculo
                                   WHERE YEAR(hc.FechaCalculo)=@Anio";
                    if (month >= 1 && month <= 12) sql += " AND MONTH(hc.FechaCalculo)=@Mes";
                    if (idEmpresa > 0) sql += " AND hc.IdEmpresa = @IdEmpresa";
                    if (idRazon > 0) sql += " AND hc.IdRazonSocial = @IdRazon";
                    sql += " ORDER BY hc.FechaCalculo, hc.IdCalculo";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Anio", year);
                        if (month >= 1 && month <= 12) cmd.Parameters.AddWithValue("@Mes", month);
                        if (idEmpresa > 0) cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                        if (idRazon > 0) cmd.Parameters.AddWithValue("@IdRazon", idRazon);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int idc = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                                string noParte = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                                // Usar Convert.ToDecimal sobre GetValue para evitar excepciones cuando el tipo en BD sea int
                                decimal cantidad = 0m;
                                try { cantidad = rdr.IsDBNull(2) ? 0m : Convert.ToDecimal(rdr.GetValue(2)); } catch { cantidad = 0m; }
                                string um = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);
                                decimal cUnit = 0m;
                                try { cUnit = rdr.IsDBNull(4) ? 0m : Convert.ToDecimal(rdr.GetValue(4)); } catch { cUnit = 0m; }
                                decimal tot = 0m;
                                try { tot = rdr.IsDBNull(5) ? 0m : Convert.ToDecimal(rdr.GetValue(5)); } catch { tot = 0m; }
                                DateTime fecha = rdr.IsDBNull(6) ? DateTime.MinValue : rdr.GetDateTime(6);

                                dgvDetails.Rows.Add(idc, fecha.ToString("yyyy-MM-dd"), noParte, cantidad.ToString("N3"), um, cUnit.ToString("N3"), tot.ToString("N2"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSummary_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // Mes seleccionado -> cargar detalles para ese mes
            int month = e.RowIndex + 1; // summary rows are 1..12
            int idRazon = GetSelectedValue(cmbRazon);
            int idEmpresa = GetSelectedValue(cmbEmpresa);
            int year = (int)nudYear.Value;
            LoadDetails(idRazon, idEmpresa, year, month);
        }

        private class ComboBoxItem


        {
            public string Text { get; }
            public int Value { get; }
            public ComboBoxItem(string text, int value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }

        private void btnExportarExcel_Click(object? sender, EventArgs e)
        {
            try
            {
                int idRazon = GetSelectedValue(cmbRazon);
                int idEmpresa = GetSelectedValue(cmbEmpresa);

                if (idEmpresa == 0 && idRazon == 0) 
                {
                    MessageBox.Show("No se puede generar un excel sin razon social y/o cliente", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else 
                {

                    using var sfd = new SaveFileDialog
                    {
                        Filter = "Excel Workbook|*.xlsx",
                        FileName = $"Historial_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    };


                    if (Retorno360Tacna.HELPERS.WinFormsHelper.ShowSaveFileDialog(this, sfd) != DialogResult.OK)
                        return;

                    using var wb = new XLWorkbook();

                    // Resumen (dgvSummary)
                    var dtResumen = new DataTable("Resumen");
                    foreach (DataGridViewColumn col in dgvSummary.Columns)
                    {
                        var colName = string.IsNullOrWhiteSpace(col.HeaderText) ? col.Name : col.HeaderText;
                        dtResumen.Columns.Add(colName);
                    }
                    foreach (DataGridViewRow row in dgvSummary.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var dr = dtResumen.NewRow();
                        for (int i = 0; i < dgvSummary.Columns.Count; i++) dr[i] = row.Cells[i].Value ?? string.Empty;
                        dtResumen.Rows.Add(dr);
                    }
                    wb.Worksheets.Add(dtResumen, "Resumen");

                    // Detalles (dgvDetails)
                    var dtDetalles = new DataTable("Detalles");
                    foreach (DataGridViewColumn col in dgvDetails.Columns)
                    {
                        var colName = string.IsNullOrWhiteSpace(col.HeaderText) ? col.Name : col.HeaderText;
                        dtDetalles.Columns.Add(colName);
                    }
                    foreach (DataGridViewRow row in dgvDetails.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var dr = dtDetalles.NewRow();
                        for (int i = 0; i < dgvDetails.Columns.Count; i++) dr[i] = row.Cells[i].Value ?? string.Empty;
                        dtDetalles.Rows.Add(dr);
                    }
                    wb.Worksheets.Add(dtDetalles, "Detalles");

                    wb.SaveAs(sfd.FileName);
                    //Abrir archivo
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
                MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
