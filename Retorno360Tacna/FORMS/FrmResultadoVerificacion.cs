using Retorno360Tacna.SERVICES;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Ventana que muestra el resultado detallado de la verificación de números de parte.
    /// </summary>
    public partial class FrmResultadoVerificacion : Form
    {
        // Valores seleccionados por el formulario padre (si se asignan, se usan para las re-verificaciones)
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal int SelectedIdRazon { get; set; } = 0;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal int SelectedIdEmpresa { get; set; } = 0;

        // Tabla que será poblada con las columnas/filas visibles del grid y expuesta al caller
        public DataTable? ResultadoParaAgregar { get; private set; }
        // Total de costos calculado para las filas que se agregarán
        public decimal TotalCostoParaAgregar { get; private set; }

        // Texto legible del mes/año a mostrar en el diálogo (ej. "enero 2026")
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal string FechaTexto
        {
            get => lblFecha?.Text ?? string.Empty;
            set { try { if (lblFecha != null) lblFecha.Text = value; } catch { } }
        }

        // Permite aplicar un valor a todas las celdas de una columna seleccionada en dgvExistentes
        private void btnAplicarCorrecciones_Click(object? sender, System.EventArgs e)
        {
            try
            {
                if (dgvNoExistentes.Columns.Count == 0) return;

                // Preguntar al usuario qué columna desea modificar mediante un diálogo simple
                using (var dlg = new Form())
                {
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.ClientSize = new Size(420, 120);
                    dlg.Text = "Aplicar valor a columna";

                    var lbl = new Label() { Text = "Selecciona la columna (Diferencias):", Location = new Point(12, 12), AutoSize = true };
                    var cmb = new ComboBox() { Location = new Point(12, 36), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
                    // Incluir sólo columnas del grid de diferencias (dgvNoExistentes)
                    foreach (DataGridViewColumn c in dgvNoExistentes.Columns)
                        cmb.Items.Add(new { Text = (c.HeaderText ?? c.Name), Index = c.Index });
                    cmb.DisplayMember = "Text"; cmb.ValueMember = "Index";
                    if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;

                    var lblVal = new Label() { Text = "Valor a aplicar:", Location = new Point(12, 68), AutoSize = true };
                    var txtVal = new TextBox() { Location = new Point(120, 64), Width = 272 };

                    var btnOk = new Button() { Text = "Aplicar", DialogResult = DialogResult.OK, Location = new Point(232, 88), Size = new Size(75, 24) };
                    var btnCancel = new Button() { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(313, 88), Size = new Size(79, 24) };

                    dlg.Controls.AddRange(new Control[] { lbl, cmb, lblVal, txtVal, btnOk, btnCancel });
                    dlg.AcceptButton = btnOk;
                    dlg.CancelButton = btnCancel;

                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    // Recuperar índice de columna seleccionado (solo dgvNoExistentes)
                    var sel = cmb.SelectedItem;
                    if (sel == null) return;
                    var selType = sel.GetType();
                    var idxProp = selType.GetProperty("Index");
                    if (idxProp == null) return;
                    int colIndex = (int)idxProp.GetValue(sel, null);
                    DataGridView targetGrid = dgvNoExistentes;

                    // Aplicar valor a todas las filas editables del grid objetivo
                    string valor = txtVal.Text ?? string.Empty;
                    try { if (targetGrid.IsCurrentCellInEditMode) targetGrid.EndEdit(); } catch { }
                    targetGrid.BeginEdit(true);
                    foreach (DataGridViewRow row in targetGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (colIndex < 0 || colIndex >= row.Cells.Count) continue;
                        var cell = row.Cells[colIndex];
                        if (cell.ReadOnly) continue;
                        try
                        {
                            if (cell.ValueType == typeof(int))
                            {
                                if (int.TryParse(valor, out int vi)) cell.Value = vi; else cell.Value = 0;
                            }
                            else if (cell.ValueType == typeof(decimal) || cell.ValueType == typeof(double) || cell.ValueType == typeof(float))
                            {
                                if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal vd)) cell.Value = vd; else cell.Value = 0m;
                            }
                            else
                            {
                                cell.Value = valor;
                            }
                        }
                        catch { cell.Value = valor; }
                    }
                    targetGrid.EndEdit();
                    targetGrid.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar correcciones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Key del mes en formato yyyy-MM para almacenar en base de datos
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal string MesKey { get; set; } = string.Empty;

        public FrmResultadoVerificacion(
            int totalConsultados,
            List<string> partesExistentes,
            List<string> partesNoExistentes)
        {
            InitializeComponent();
            // Mantener compatibilidad: aliasar btnAgregar al control actual
            btnAgregar = btnAgregarGrid;
            CargarResultados(totalConsultados, partesExistentes, partesNoExistentes);
        }

        private void DgvNoExistentes_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var grid = dgvNoExistentes;
                if (grid.ReadOnly) return;
                // Iniciar edición en la celda doble clickeada
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                grid.BeginEdit(true);
            }
            catch { }
        }

        // Al finalizar la edición en dgvNoExistentes, validar que el número de parte no exista
        // ya en dgvExistentes; si existe, revertir la edición y avisar al usuario.
        private void DgvNoExistentes_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var cell = dgvNoExistentes.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var newVal = cell.Value?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(newVal)) return;

                // Buscar en dgvExistentes columna de 'Parte'
                int idxParteExist = -1;
                for (int i = 0; i < dgvExistentes.Columns.Count; i++)
                {
                    var nm = (dgvExistentes.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                    var hdr = (dgvExistentes.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                    if (nm.Contains("parte") || hdr.Contains("parte")) { idxParteExist = i; break; }
                }

                if (idxParteExist == -1) return; // no se puede validar sin columna 'Parte'

                // Comparar contra cada fila en dgvExistentes
                for (int r = 0; r < dgvExistentes.Rows.Count; r++)
                {
                    var existing = dgvExistentes.Rows[r].Cells[idxParteExist].Value?.ToString() ?? string.Empty;
                    if (string.Equals(existing.Trim(), newVal.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("El número de parte ya existe en el listado de existentes. No puede duplicarse entre grids.", "Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // revertir edición: limpiar la celda editada
                        cell.Value = string.Empty;
                        return;
                    }
                }
            }
            catch { }
        }

        // Nuevo constructor para visualizar detalle con cantidad, unidad y costos
        public FrmResultadoVerificacion(VerificacionPartesService.ResultadoVerificacionDetalle detalle)
        {
            InitializeComponent();
            // Mantener compatibilidad: aliasar btnAgregar al control actual
            btnAgregar = btnAgregarGrid;
            CargarResultadosDetalle(detalle?.Items ?? new List<VerificacionPartesService.DetalleParte>());
        }

        private void CargarResultados(
            int totalConsultados,
            List<string> partesExistentes,
            List<string> partesNoExistentes)
        {
            lblTotalConsultados.Text = totalConsultados.ToString();
            lblTotalExistentes.Text = partesExistentes.Count.ToString();
            lblTotalNoExistentes.Text = partesNoExistentes.Count.ToString();

            // Color del contador de no existentes: rojo si hay alguna, verde si todas existen
            lblTotalNoExistentes.ForeColor = partesNoExistentes.Count > 0
                ? Color.FromArgb(192, 57, 43)
                : Color.FromArgb(30, 150, 70);

            // Cargar grids
            CargarGrid(dgvExistentes, partesExistentes, "Número de Parte", Color.FromArgb(235, 248, 240));
            CargarGrid(dgvNoExistentes, partesNoExistentes, "Número de Parte", Color.FromArgb(253, 237, 236));

            // Mensaje resumen en header
            if (partesNoExistentes.Count == 0)
            {
                lblMensaje.Text = "✔  Todos los números de parte existen en la base de datos.";
                lblMensaje.ForeColor = Color.FromArgb(200, 240, 210);
            }
            else
            {
                lblMensaje.Text = $"⚠  Se encontraron {partesNoExistentes.Count} número(s) de parte NO registrado(s).";
                lblMensaje.ForeColor = Color.FromArgb(255, 220, 180);
            }
        }

        private void CargarResultadosDetalle(List<VerificacionPartesService.DetalleParte> items)
        {
            // Totales
            lblTotalConsultados.Text = items.Count.ToString();
            lblTotalExistentes.Text = items.Count(i => i.Existe).ToString();
            lblTotalNoExistentes.Text = items.Count(i => !i.Existe).ToString();

            lblTotalNoExistentes.ForeColor = items.Any(i => !i.Existe)
                ? Color.FromArgb(192, 57, 43)
                : Color.FromArgb(30, 150, 70);

            // Preparar grid de existentes para mostrar columnas detalladas
            dgvExistentes.Rows.Clear();
            dgvExistentes.Columns.Clear();

            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Parte", Name = "colParte", ReadOnly = true });
            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", Name = "colCantidad", ReadOnly = true });
            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UM Usuario", Name = "colUMUsuario", ReadOnly = true });
            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UM BD", Name = "colUMBD", ReadOnly = true });
            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Costo Unit.", Name = "colCostoUnit", ReadOnly = true });
            dgvExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Costo", Name = "colTotalCosto", ReadOnly = true });
            // Columnas internas para marcar existencia y coincidencia de UM (ocultas)
            dgvExistentes.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Existe", Name = "colExiste", ReadOnly = true, Visible = false });
            dgvExistentes.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "UmCoincide", Name = "colUmCoincide", ReadOnly = true, Visible = false });

            // Añadir SOLO las piezas que están correctas (existen, UM coincide y costo válido)
            foreach (var item in items)
            {
                if (!(item.Existe && item.UmCoincide && item.CostoValido))
                    continue; // saltar las filas con diferencias

                int idx = dgvExistentes.Rows.Add(
                    item.Parte,
                    item.Cantidad.ToString("N3"),
                    item.UnidadUsuario,
                    item.MedComercial,
                    item.CostoUnitario.ToString("N3"),
                    item.TotalCosto.ToString("N3")
                );

                var row = dgvExistentes.Rows[idx];

                // Marcar columnas internas
                try { row.Cells["colExiste"].Value = item.Existe; } catch { }
                try { row.Cells["colUmCoincide"].Value = item.UmCoincide; } catch { }
            }

            // Preparar tabla de no existentes para edición: permitimos que el usuario
            // corrija o modifique las filas con diferencias y luego las agregue.
            dgvNoExistentes.Rows.Clear();
            dgvNoExistentes.Columns.Clear();
            dgvNoExistentes.Enabled = true;
            dgvNoExistentes.ReadOnly = false;
            dgvNoExistentes.AllowUserToAddRows = false;
            dgvNoExistentes.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

            // Columnas idénticas a las de existentes, pero editables por el usuario
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Parte", Name = "colParte", ReadOnly = false });
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", Name = "colCantidad", ReadOnly = false });
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UM Usuario", Name = "colUMUsuario", ReadOnly = false });
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UM BD", Name = "colUMBD", ReadOnly = false });
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Costo Unit.", Name = "colCostoUnit", ReadOnly = false });
            dgvNoExistentes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Costo", Name = "colTotalCosto", ReadOnly = false });

            // Llenar dgvNoExistentes con las piezas que tienen alguna diferencia
            foreach (var item in items)
            {
                if (item.Existe && item.UmCoincide && item.CostoValido)
                    continue; // esta fila no tiene diferencia, la dejaremos solo en dgvExistentes

                int idxNo = dgvNoExistentes.Rows.Add(
                    item.Parte,
                    item.Cantidad.ToString("N3"),
                    item.UnidadUsuario,
                    item.MedComercial,
                    item.CostoUnitario.ToString("N3"),
                    item.TotalCosto.ToString("N3")
                );

                var rowNo = dgvNoExistentes.Rows[idxNo];
                // Marcar visualmente las filas con diferencia (color de fondo suave)
                rowNo.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);

                // Si la unidad de medida no coincide, resaltar específicamente las celdas de UM
                // con un color más fuerte (rojo) para que el usuario lo identifique rápidamente.
                try
                {
                    if (!item.UmCoincide)
                    {
                        if (rowNo.Cells.IndexOf(rowNo.Cells["colUMUsuario"]) >= 0)
                            rowNo.Cells["colUMUsuario"].Style.BackColor = Color.FromArgb(255, 102, 102);
                        if (rowNo.Cells.IndexOf(rowNo.Cells["colUMBD"]) >= 0)
                            rowNo.Cells["colUMBD"].Style.BackColor = Color.FromArgb(255, 102, 102);
                    }
                }
                catch { }
            }

            // Permitir edición directa y validar cambios para evitar duplicados entre grids.
            try
            {
                dgvNoExistentes.CellEndEdit -= DgvNoExistentes_CellEndEdit;
            }
            catch { }
            dgvNoExistentes.CellEndEdit += DgvNoExistentes_CellEndEdit;

            try { dgvNoExistentes.CellDoubleClick -= DgvNoExistentes_CellDoubleClick; } catch { }
            dgvNoExistentes.CellDoubleClick += DgvNoExistentes_CellDoubleClick;

            // Mensaje resumen
            if (items.All(i => i.Existe && i.CostoValido && i.UmCoincide))
            {
                lblMensaje.Text = "✔  Todas las partes coinciden en unidad y costo válido.";
                lblMensaje.ForeColor = Color.FromArgb(200, 240, 210);
            }
            else
            {
                lblMensaje.Text = "⚠  Se detectaron discrepancias: unidades diferentes o costo nulo/0.";
                lblMensaje.ForeColor = Color.FromArgb(255, 220, 180);
            }
        }

        private static void CargarGrid(DataGridView dgv, List<string> partes, string headerText, Color rowColor)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Name = "colParte",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            foreach (var parte in partes)
                dgv.Rows.Add(parte);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = rowColor;
        }

        private void btnCerrar_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void btnAgregar_Click(object? sender, System.EventArgs e)
        {
            try
            {
                int FindIndexInGrid(DataGridView dgv, params string[] keys)
                {
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        var name = (dgv.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                        var hdr = (dgv.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                        foreach (var k in keys)
                        {
                            var kk = k.ToLowerInvariant();
                            if (name.Contains(kk) || hdr.Contains(kk)) return i;
                        }
                    }
                    return -1;
                }

                var dt = new DataTable();
                dt.Columns.Add("Parte", typeof(string));
                dt.Columns.Add("Cantidad", typeof(decimal));
                dt.Columns.Add("UM Usuario", typeof(string));
                dt.Columns.Add("UM BD", typeof(string));
                dt.Columns.Add("Costo Unit.", typeof(decimal));
                dt.Columns.Add("Total Costo", typeof(decimal));

                int idxParte = FindIndexInGrid(dgvExistentes, "parte");
                int idxCantidad = FindIndexInGrid(dgvExistentes, "cantidad", "cant");
                int idxUMUsuario = FindIndexInGrid(dgvExistentes, "um usuario", "um_usuario");
                int idxUMBD = FindIndexInGrid(dgvExistentes, "um bd", "um_bd", "med");
                int idxCostoUnit = FindIndexInGrid(dgvExistentes, "costo unit", "costounit");
                int idxTotal = FindIndexInGrid(dgvExistentes, "total costo", "totalcosto", "total_costo");

                TotalCostoParaAgregar = 0m;

                foreach (DataGridViewRow row in dgvExistentes.Rows)
                {
                    if (row.IsNewRow) continue;
                    var dr = dt.NewRow();

                    dr["Parte"] = idxParte >= 0 ? (row.Cells[idxParte].Value?.ToString() ?? string.Empty) : string.Empty;

                    decimal cantidad = 0m;
                    if (idxCantidad >= 0) decimal.TryParse(row.Cells[idxCantidad].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cantidad);
                    dr["Cantidad"] = cantidad;

                    dr["UM Usuario"] = idxUMUsuario >= 0 ? (row.Cells[idxUMUsuario].Value?.ToString() ?? string.Empty) : string.Empty;
                    dr["UM BD"] = idxUMBD >= 0 ? (row.Cells[idxUMBD].Value?.ToString() ?? string.Empty) : string.Empty;

                    decimal costoUnit = 0m;
                    if (idxCostoUnit >= 0) decimal.TryParse(row.Cells[idxCostoUnit].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out costoUnit);
                    dr["Costo Unit."] = costoUnit;

                    decimal totalCosto = 0m;
                    if (idxTotal >= 0) decimal.TryParse(row.Cells[idxTotal].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out totalCosto);
                    dr["Total Costo"] = totalCosto;

                    TotalCostoParaAgregar += totalCosto;
                    dt.Rows.Add(dr);
                }

                ResultadoParaAgregar = dt;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

        // Nuevo: al hacer click en Agregar existentes, tomar todas las filas de dgvExistentes
        // y enviarlas al formulario padre (dgvRelsultados) utilizando la misma lógica que
        // el flujo original de btnAgregar, pero sin incluir filas desde dgvNoExistentes.
        private void btnAgregarExistentes_Click(object? sender, System.EventArgs e)
        {
            try
            {
                // Helper local para encontrar índice por nombre/encabezado
                int FindIndexInGrid(DataGridView dgv, params string[] keys)
                {
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        var name = (dgv.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                        var hdr = (dgv.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                        foreach (var k in keys)
                        {
                            var kk = k.ToLowerInvariant();
                            if (name.Contains(kk) || hdr.Contains(kk)) return i;
                        }
                    }
                    return -1;
                }

                // Definir esquema de salida consistente para el formulario padre
                var dt = new DataTable();
                dt.Columns.Add("Parte", typeof(string));
                dt.Columns.Add("Cantidad", typeof(decimal));
                dt.Columns.Add("UM Usuario", typeof(string));
                dt.Columns.Add("UM BD", typeof(string));
                dt.Columns.Add("Costo Unit.", typeof(decimal));
                dt.Columns.Add("Total Costo", typeof(decimal));

                // Buscar índices fuente en dgvExistentes según nombres/encabezados
                int idxParte = FindIndexInGrid(dgvExistentes, "parte");
                int idxCantidad = FindIndexInGrid(dgvExistentes, "cantidad", "cant");
                int idxUMUsuario = FindIndexInGrid(dgvExistentes, "um usuario", "um_usuario", "um usuario");
                int idxUMBD = FindIndexInGrid(dgvExistentes, "um bd", "um_bd", "med");
                int idxCostoUnit = FindIndexInGrid(dgvExistentes, "costo unit", "costounit", "costo unit.");
                int idxTotal = FindIndexInGrid(dgvExistentes, "total costo", "totalcosto", "total_costo");

                TotalCostoParaAgregar = 0m;

                // Obtener ids del padre preferentemente (propiedades públicas)
                int idRazonVal = 0, idEmpresaVal = 0;
                var parent = this.Owner as FrmCalculoInventarios;
                try { if (parent != null) { idRazonVal = parent.SelectedIdRazon; idEmpresaVal = parent.SelectedIdEmpresa; } } catch { }

                // Si no hay ids, intentar extraerlos de los combo boxes del padre (SelectedValue puede ser string)
                try
                {
                    if (idRazonVal == 0 && parent != null)
                    {
                        var cr = parent.Controls.Find("cmbRazonSocial", true).FirstOrDefault() as ComboBox;
                        if (cr != null && cr.SelectedValue != null)
                        {
                            if (!int.TryParse(cr.SelectedValue.ToString(), out idRazonVal)) idRazonVal = 0;
                        }
                    }
                    if (idEmpresaVal == 0 && parent != null)
                    {
                        var ce = parent.Controls.Find("cmbEmpresa", true).FirstOrDefault() as ComboBox;
                        if (ce != null && ce.SelectedValue != null)
                        {
                            if (!int.TryParse(ce.SelectedValue.ToString(), out idEmpresaVal)) idEmpresaVal = 0;
                        }
                    }
                }
                catch { }

                foreach (DataGridViewRow row in dgvExistentes.Rows)
                {
                    if (row.IsNewRow) continue;
                    var dr = dt.NewRow();

                    // Parte
                    dr["Parte"] = idxParte >= 0 ? (row.Cells[idxParte].Value?.ToString() ?? string.Empty) : string.Empty;

                    // Cantidad
                    decimal cantidad = 0m;
                    if (idxCantidad >= 0) decimal.TryParse(row.Cells[idxCantidad].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cantidad);
                    dr["Cantidad"] = cantidad;

                    // UM Usuario
                    dr["UM Usuario"] = idxUMUsuario >= 0 ? (row.Cells[idxUMUsuario].Value?.ToString() ?? string.Empty) : string.Empty;

                    // UM BD
                    dr["UM BD"] = idxUMBD >= 0 ? (row.Cells[idxUMBD].Value?.ToString() ?? string.Empty) : string.Empty;

                    // Costo Unit.
                    decimal costoUnit = 0m;
                    if (idxCostoUnit >= 0) decimal.TryParse(row.Cells[idxCostoUnit].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out costoUnit);
                    dr["Costo Unit."] = costoUnit;

                    // Total Costo
                    decimal totalCosto = 0m;
                    if (idxTotal >= 0) decimal.TryParse(row.Cells[idxTotal].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out totalCosto);
                    dr["Total Costo"] = totalCosto;

                    TotalCostoParaAgregar += totalCosto;

                    dt.Rows.Add(dr);
                }

                // Preparar el resultado para que el formulario padre lo procese al cerrar el diálogo
                ResultadoParaAgregar = dt;
                TotalCostoParaAgregar = TotalCostoParaAgregar;

                // Cerrar el diálogo con OK para que el padre obtenga ResultadoParaAgregar y lo muestre
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar existentes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nuevo: Agregar fila seleccionada o marcada desde dgvNoExistentes a dgvExistentes
        // Este método verifica la fila (llamando al servicio) y, si es válida, la mueve.
        private async void btnAgregarFila_Click_1(object? sender, System.EventArgs e)
        {
            try
            {
                // Recolectar filas seleccionadas (soporta selección múltiple por filas o celdas)
                var filasSeleccionadas = new List<DataGridViewRow>();
                var filasSet = new HashSet<DataGridViewRow>();

                // Helper local para encontrar índices por nombre/encabezado
                int FindIndex(DataGridView dgv, params string[] keys)
                {
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        var name = (dgv.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                        var hdr = (dgv.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                        foreach (var k in keys)
                        {
                            var kk = k.ToLowerInvariant();
                            if (name.Contains(kk) || hdr.Contains(kk)) return i;
                        }
                    }
                    return -1;
                }

                if (dgvNoExistentes.SelectedRows != null && dgvNoExistentes.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgvNoExistentes.SelectedRows)
                    {
                        if (!row.IsNewRow && filasSet.Add(row)) filasSeleccionadas.Add(row);
                    }
                }

                if (dgvNoExistentes.SelectedCells != null && dgvNoExistentes.SelectedCells.Count > 0)
                {
                    foreach (DataGridViewCell cell in dgvNoExistentes.SelectedCells)
                    {
                        if (cell?.OwningRow != null && !cell.OwningRow.IsNewRow && filasSet.Add(cell.OwningRow))
                            filasSeleccionadas.Add(cell.OwningRow);
                    }
                }

                // Si no hay selección explícita, asumimos operación "Agregar automát. válidas"
                bool autoAgregarValidas = false;
                if (filasSeleccionadas.Count == 0)
                {
                    // Recopilar todas las filas del grid de diferencias para verificar en lote
                    foreach (DataGridViewRow r in dgvNoExistentes.Rows)
                    {
                        if (!r.IsNewRow && filasSet.Add(r)) filasSeleccionadas.Add(r);
                    }
                    autoAgregarValidas = true;
                }

                // Si aún no hay filas (posible en grids vacíos), intentar usar la fila actual
                if (filasSeleccionadas.Count == 0 && dgvNoExistentes.CurrentRow != null && !dgvNoExistentes.CurrentRow.IsNewRow)
                {
                    filasSeleccionadas.Add(dgvNoExistentes.CurrentRow);
                }

                if (filasSeleccionadas.Count == 0)
                {
                    MessageBox.Show("Selecciona una o varias filas en 'Diferencias detectadas' para agregar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Remapear columnas: buscar índices en dgvNoExistentes por nombre/encabezado

                int idxParte = FindIndex(dgvNoExistentes, "parte");
                int idxCantidad = FindIndex(dgvNoExistentes, "cantidad", "cant");
                int idxUMUsuario = FindIndex(dgvNoExistentes, "um usuario", "um_usuario", "um usuario");

                if (idxParte == -1) { MessageBox.Show("No se encontró la columna 'Parte'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                // Asegurar que cualquier edición en curso se confirme antes de leer valores
                try
                {
                    if (dgvNoExistentes.IsCurrentCellInEditMode) dgvNoExistentes.EndEdit();
                    dgvNoExistentes.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
                catch { }

                // Preparar entradas para verificación en lote
                var entradas = new System.Collections.Generic.List<(string Parte, decimal Cantidad, string Unidad)>();
                var filasVerificables = new List<DataGridViewRow>();

                // Índices de columnas en dgvNoExistentes (si existen)
                int idxParteNo = FindIndex(dgvNoExistentes, "parte");
                int idxCantidadNo = FindIndex(dgvNoExistentes, "cantidad", "cant");
                int idxUMUsuarioNo = FindIndex(dgvNoExistentes, "um usuario", "um_usuario");
                int idxUMBDNo = FindIndex(dgvNoExistentes, "um bd", "um_bd", "med");

                foreach (var row in filasSeleccionadas)
                {
                    // Validaciones básicas: las 4 columnas deben contener valores válidos para auto-agregar
                    string parteSel = idxParteNo >= 0 ? row.Cells[idxParteNo].Value?.ToString() ?? string.Empty : string.Empty;
                    string unidadUsuarioSel = idxUMUsuarioNo >= 0 ? row.Cells[idxUMUsuarioNo].Value?.ToString() ?? string.Empty : string.Empty;
                    string unidadBDSel = idxUMBDNo >= 0 ? row.Cells[idxUMBDNo].Value?.ToString() ?? string.Empty : string.Empty;

                    decimal cantidadSel = 1m;
                    if (idxCantidadNo >= 0)
                    {
                        var cv = row.Cells[idxCantidadNo].Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(cv)) decimal.TryParse(cv, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cantidadSel);
                    }

                    // Si estamos en modo autoAgregarValidas, saltar filas con campos inválidos
                    if (autoAgregarValidas)
                    {
                        bool ok = true;
                        if (string.IsNullOrWhiteSpace(parteSel)) ok = false;
                        if (cantidadSel <= 0m) ok = false;
                        if (string.IsNullOrWhiteSpace(unidadUsuarioSel)) ok = false;
                        if (string.IsNullOrWhiteSpace(unidadBDSel)) ok = false;

                        if (!ok)
                        {
                            // Marcar visualmente como incompleta para que el usuario la corrija
                            try { row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205); } catch { }
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(parteSel)) continue;

                    unidadUsuarioSel = unidadUsuarioSel.Trim().ToUpperInvariant();

                    entradas.Add((parteSel.Trim(), cantidadSel, unidadUsuarioSel));
                    filasVerificables.Add(row);
                }

                if (entradas.Count == 0)
                {
                    MessageBox.Show("No hay filas válidas seleccionadas para verificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Obtener ids: preferir propiedades asignadas por el padre, si no, obtener desde Owner
                int idRazon = this.SelectedIdRazon, idEmpresa = this.SelectedIdEmpresa;
                if (idRazon == 0 || idEmpresa == 0)
                {
                    if (this.Owner is FrmCalculoInventarios parent)
                    {
                        idRazon = parent.SelectedIdRazon; idEmpresa = parent.SelectedIdEmpresa;
                    }
                }
                if (idRazon == 0 || idEmpresa == 0)
                {
                    MessageBox.Show("Selecciona razón social y empresa en el formulario principal antes de agregar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var svc = new VerificacionPartesService();
                var res = await svc.VerificarPartesConCantidadAsync(idRazon, idEmpresa, entradas);

                if (res.Items.Count == 0) { MessageBox.Show("No se obtuvo respuesta de verificación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                var filasAEliminar = new List<DataGridViewRow>();
                int agregadas = 0;
                int revisadas = System.Math.Min(res.Items.Count, filasVerificables.Count);

                for (int i = 0; i < revisadas; i++)
                {
                    var det = res.Items[i];
                    var filaOrigen = filasVerificables[i];

                    if (det.Existe && det.UmCoincide)
                    {
                        // Construir fila para dgvExistentes remapeando por columnas objetivo
                        var targetValues = new object[dgvExistentes.Columns.Count];
                        for (int ci = 0; ci < dgvExistentes.Columns.Count; ci++)
                        {
                            var col = dgvExistentes.Columns[ci];
                            var name = (col.Name ?? string.Empty).ToLowerInvariant();
                            var hdr = (col.HeaderText ?? string.Empty).ToLowerInvariant();

                            if (name.Contains("parte") || hdr.Contains("parte")) targetValues[ci] = det.Parte;
                            else if (name.Contains("cantidad") || hdr.Contains("cantidad") || hdr.Contains("cant")) targetValues[ci] = det.Cantidad.ToString("N3");
                            else if (name.Contains("umusuario") || hdr.Contains("um usuario") || hdr.Contains("um_usuario")) targetValues[ci] = det.UnidadUsuario;
                            else if (name.Contains("umbd") || hdr.Contains("um bd") || hdr.Contains("um_bd") || hdr.Contains("med")) targetValues[ci] = det.MedComercial;
                            else if (name.Contains("costounit") || hdr.Contains("costo unit") || hdr.Contains("costo unit.")) targetValues[ci] = det.CostoUnitario.ToString("N3");
                            else if (name.Contains("totalcosto") || hdr.Contains("total costo") || hdr.Contains("total_costo")) targetValues[ci] = det.TotalCosto.ToString("N3");
                            else
                            {
                                // Intentar copiar desde la fila original si existe una columna equivalente
                                int srcIdx = FindIndex(dgvNoExistentes, col.Name ?? col.HeaderText ?? string.Empty);
                                if (srcIdx >= 0) targetValues[ci] = filaOrigen.Cells[srcIdx].Value ?? DBNull.Value;
                                else targetValues[ci] = DBNull.Value;
                            }
                        }

                        int newIdx = dgvExistentes.Rows.Add(targetValues);
                        var newRow = dgvExistentes.Rows[newIdx];

                        // Marcar columnas internas ocultas si están presentes
                        try { if (newRow.Cells["colExiste"] != null) newRow.Cells["colExiste"].Value = true; } catch { }
                        try { if (newRow.Cells["colUmCoincide"] != null) newRow.Cells["colUmCoincide"].Value = true; } catch { }

                        filasAEliminar.Add(filaOrigen);
                        agregadas++;
                    }
                    else
                    {
                        // Mantener resaltado de diferencia
                        try { filaOrigen.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236); } catch { }
                    }
                }

                // Eliminar filas agregadas desde el final para no romper índices
                for (int i = dgvNoExistentes.Rows.Count - 1; i >= 0; i--)
                {
                    var row = dgvNoExistentes.Rows[i];
                    if (filasAEliminar.Contains(row))
                        dgvNoExistentes.Rows.RemoveAt(i);
                }

                // Si no se agregó ninguna fila válida, mostrar detalle de por qué (debug amigable)
                if (agregadas == 0)
                {
                    try
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("Ninguna de las filas seleccionadas cumple los criterios (existencia y UM coincidente). Detalle:");
                        for (int i = 0; i < res.Items.Count && i < entradas.Count; i++)
                        {
                            var d = res.Items[i];
                            var sent = entradas[i].Unidad;
                            sb.AppendLine($"{d.Parte}: Existe={d.Existe}, UmCoincide={d.UmCoincide}, UM_BD='{d.MedComercial}', UM_Enviada='{sent}'");
                        }
                        MessageBox.Show(sb.ToString(), "No válido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch { }
                }

                // Actualizar contadores y mensaje
                try
                {
                    lblTotalExistentes.Text = dgvExistentes.Rows.Count.ToString();
                    lblTotalNoExistentes.Text = dgvNoExistentes.Rows.Count.ToString();
                    lblTotalNoExistentes.ForeColor = dgvNoExistentes.Rows.Count == 0 ? Color.FromArgb(30, 150, 70) : Color.FromArgb(192, 57, 43);
                    lblMensaje.Text = $"Se agregaron {agregadas} fila(s) válidas desde diferencias. Usa 'Agregar existentes' para enviar los resultados al formulario principal.";
                    lblMensaje.ForeColor = Color.FromArgb(200, 240, 210);

                    // Reflejar inmediatamente en el formulario padre las filas existentes acumuladas
                    if (agregadas > 0 && this.Owner is FrmCalculoInventarios parent)
                    {
                        int FindIndexInGrid(DataGridView dgv, params string[] keys)
                        {
                            for (int i = 0; i < dgv.Columns.Count; i++)
                            {
                                var name = (dgv.Columns[i].Name ?? string.Empty).ToLowerInvariant();
                                var hdr = (dgv.Columns[i].HeaderText ?? string.Empty).ToLowerInvariant();
                                foreach (var k in keys)
                                {
                                    var kk = k.ToLowerInvariant();
                                    if (name.Contains(kk) || hdr.Contains(kk)) return i;
                                }
                            }
                            return -1;
                        }

                        var dtParent = new DataTable();
                        dtParent.Columns.Add("Parte", typeof(string));
                        dtParent.Columns.Add("Cantidad", typeof(decimal));
                        dtParent.Columns.Add("UM Usuario", typeof(string));
                        dtParent.Columns.Add("UM BD", typeof(string));
                        dtParent.Columns.Add("Costo Unit.", typeof(decimal));
                        dtParent.Columns.Add("Total Costo", typeof(decimal));

                        int idxParteE = FindIndexInGrid(dgvExistentes, "parte");
                        int idxCantidadE = FindIndexInGrid(dgvExistentes, "cantidad", "cant");
                        int idxUMUserE = FindIndexInGrid(dgvExistentes, "um usuario", "um_usuario");
                        int idxUMBDE = FindIndexInGrid(dgvExistentes, "um bd", "med");
                        int idxCostoUnitE = FindIndexInGrid(dgvExistentes, "costo unit", "costounit");
                        int idxTotalE = FindIndexInGrid(dgvExistentes, "total costo", "totalcosto");

                        decimal totalParaEnviar = 0m;
                        foreach (DataGridViewRow rr in dgvExistentes.Rows)
                        {
                            if (rr.IsNewRow) continue;
                            var dr = dtParent.NewRow();
                            dr["Parte"] = idxParteE >= 0 ? (rr.Cells[idxParteE].Value?.ToString() ?? string.Empty) : string.Empty;

                            decimal cantidadVal = 0m;
                            if (idxCantidadE >= 0) decimal.TryParse(rr.Cells[idxCantidadE].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cantidadVal);
                            dr["Cantidad"] = cantidadVal;

                            dr["UM Usuario"] = idxUMUserE >= 0 ? (rr.Cells[idxUMUserE].Value?.ToString() ?? string.Empty) : string.Empty;
                            dr["UM BD"] = idxUMBDE >= 0 ? (rr.Cells[idxUMBDE].Value?.ToString() ?? string.Empty) : string.Empty;

                            decimal costoUnitVal = 0m;
                            if (idxCostoUnitE >= 0) decimal.TryParse(rr.Cells[idxCostoUnitE].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out costoUnitVal);
                            dr["Costo Unit."] = costoUnitVal;

                            decimal totalVal = 0m;
                            if (idxTotalE >= 0) decimal.TryParse(rr.Cells[idxTotalE].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out totalVal);
                            dr["Total Costo"] = totalVal;

                            totalParaEnviar += totalVal;
                            dtParent.Rows.Add(dr);
                        }

                        if (dtParent.Rows.Count > 0)
                        {
                            try { parent.AppendResultadoParaAgregar(dtParent, totalParaEnviar); } catch { }
                        }
                    }
                }
                catch { }

                if (agregadas == 0)
                {
                    MessageBox.Show("Ninguna de las filas seleccionadas cumple los criterios (existencia y UM coincidente).", "No válido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar fila(s): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nuevo: agrega automáticamente todas las filas válidas reutilizando la lógica central.
        private void btnAgregarValidas_Click(object? sender, System.EventArgs e)
        {
            try
            {
                try { dgvNoExistentes.ClearSelection(); } catch { }
                btnAgregarFila_Click_1(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar válidas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblHeaderExistentes_Click(object sender, EventArgs e)
        {

        }

   
    }
}
