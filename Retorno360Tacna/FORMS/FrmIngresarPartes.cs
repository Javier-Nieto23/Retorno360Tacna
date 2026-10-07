using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Ventana para que el usuario ingrese los números de parte a verificar.
    /// </summary>
    public partial class FrmIngresarPartes : Form
    {
        /// <summary>
        /// Texto ingresado por el usuario tras confirmar el diálogo.
        /// </summary>
        public string? PartesIngresadas { get; private set; }
        // Mes seleccionado por el usuario en esta ventana (clave original)
        public string? MesSeleccionado { get; private set; }

        // Item auxiliar para mostrar texto legible en el ComboBox manteniendo la clave original
        private class MonthItem
        {
            public string Key { get; set; } = string.Empty;
            public string Display { get; set; } = string.Empty;
            // DateKey representa el primer día del mes cuando es posible (fallback DateTime.MinValue)
            public DateTime DateKey { get; set; } = DateTime.MinValue;
            public override string ToString() => Display;
        }

        // lista completa de MonthItem cargada por CargarMeses
        private System.Collections.Generic.List<MonthItem> _allMonthItems = new();

        public FrmIngresarPartes()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Carga la lista de meses que el caller proporciona para que el usuario seleccione uno.
        /// </summary>
        public void CargarMeses(System.Collections.Generic.IEnumerable<string> meses, string? seleccion = null)
        {
            cmbMes.Items.Clear();
            // Mantener consistencia visual tras reubicación de controles.
            cmbMes.Visible = true;
            cmbMes.BringToFront();

            // Usar DataSource con DisplayMember/ValueMember para asegurar que el texto se muestre correctamente
            try { cmbMes.DrawItem -= CmbMes_DrawItem; } catch { }
            cmbMes.DrawMode = DrawMode.Normal;
            cmbMes.DataSource = null;

            if (meses == null) return;

            var itemsList = new System.Collections.Generic.List<MonthItem>();
            foreach (var raw in meses)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                string display = raw;
                try
                {
                    var src = raw.Trim();
                    string ym = string.Empty;
                    if (src.Length >= 7 && System.Text.RegularExpressions.Regex.IsMatch(src.Substring(0, 7), "^\\d{4}-\\d{2}$"))
                        ym = src.Substring(0, 7);
                    else
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(src, "(\\d{4}-\\d{2})");
                        if (m.Success) ym = m.Groups[1].Value;
                    }

                    if (!string.IsNullOrWhiteSpace(ym))
                    {
                        var parts = ym.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int y) && int.TryParse(parts[1], out int mo))
                        {
                            if (mo >= 1 && mo <= 12)
                            {
                                var cult = new System.Globalization.CultureInfo("es-ES");
                                var nombre = cult.DateTimeFormat.GetMonthName(mo).ToUpper(cult);
                                // mostrar solo el nombre del mes en el combo; el año se selecciona en cmbAno
                                display = nombre;
                            }
                        }
                    }
                }
                catch { display = raw; }

                DateTime dateKey = DateTime.MinValue;
                try
                {
                    var m = System.Text.RegularExpressions.Regex.Match(raw, "(\\d{4}-(\\d{2}))");
                    if (m.Success)
                    {
                        var parts = m.Groups[1].Value.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int yy) && int.TryParse(parts[1], out int mm))
                        {
                            if (mm >= 1 && mm <= 12)
                                dateKey = new DateTime(yy, mm, 1);
                        }
                    }
                }
                catch { }

                var it = new MonthItem { Key = raw, Display = display, DateKey = dateKey };
                itemsList.Add(it);
            }
            // ordenar y eliminar duplicados por Key
            var ordered = itemsList
                .GroupBy(i => i.Key)
                .Select(g => g.First())
                .OrderBy(i => i.DateKey == DateTime.MinValue ? DateTime.MaxValue : i.DateKey)
                .ThenBy(i => i.Display)
                .ToList();

            // guardar lista completa para filtrados posteriores
            _allMonthItems = ordered;

            // poblar combo de años
            cmbAno.Items.Clear();
            var anos = _allMonthItems.Where(i => i.DateKey != DateTime.MinValue).Select(i => i.DateKey.Year).Distinct().OrderBy(y => y).ToList();
            if (anos.Count == 0)
            {
                var now = DateTime.Now.Year;
                for (int y = now - 1; y <= now + 1; y++) cmbAno.Items.Add(y.ToString());
            }
            else
            {
                foreach (var y in anos) cmbAno.Items.Add(y.ToString());
            }

            // suscribir evento SelectedIndexChanged de cmbAno (evitar duplicados)
            try { cmbAno.SelectedIndexChanged -= CmbAno_SelectedIndexChanged; } catch { }
            cmbAno.SelectedIndexChanged += CmbAno_SelectedIndexChanged;

            // seleccionar por defecto año/mes si se indicó 'seleccion' en formato yyyy-MM
            int selYear = -1; string selKey = null;
            if (!string.IsNullOrWhiteSpace(seleccion))
            {
                var m = System.Text.RegularExpressions.Regex.Match(seleccion, "(\\d{4})-(\\d{2})");
                if (m.Success)
                {
                    int.TryParse(m.Groups[1].Value, out selYear);
                    selKey = seleccion;
                }
            }

            if (cmbAno.Items.Count > 0)
            {
                if (selYear != -1 && cmbAno.Items.Cast<object>().Any(x => x.ToString() == selYear.ToString()))
                    cmbAno.SelectedItem = selYear.ToString();
                else
                    cmbAno.SelectedIndex = 0;
            }

            // poblar meses para el año seleccionado
            UpdateMonthsForSelectedYear();

            // si se indicó selección, intentar seleccionar el mes (por Key)
            if (!string.IsNullOrWhiteSpace(selKey))
            {
                for (int i = 0; i < cmbMes.Items.Count; i++)
                {
                    var obj = cmbMes.Items[i];
                    if (obj is MonthItem mi && mi.Key == selKey)
                    {
                        cmbMes.SelectedIndex = i;
                        break;
                    }
                }
            }

            // seleccionar el valor si se indicó
            if (!string.IsNullOrWhiteSpace(seleccion))
            {
                for (int i = 0; i < itemsList.Count; i++)
                {
                    if (itemsList[i].Key == seleccion)
                    {
                        cmbMes.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (itemsList.Count > 0 && cmbMes.SelectedIndex == -1)
                cmbMes.SelectedIndex = 0;
        }

        private void CmbAno_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                UpdateMonthsForSelectedYear();
            }
            catch { }
        }

        private void UpdateMonthsForSelectedYear()
        {
            try
            {
                if (_allMonthItems == null) return;

                int year = -1;
                if (cmbAno.SelectedItem != null && int.TryParse(cmbAno.SelectedItem.ToString(), out int y))
                    year = y;

                var filtered = _allMonthItems
                    .Where(i => i.DateKey != DateTime.MinValue && (year == -1 || i.DateKey.Year == year))
                    .OrderBy(i => i.DateKey.Month)
                    .ThenBy(i => i.Display)
                    .ToList();

                if (filtered.Count == 0)
                    filtered = _allMonthItems.Where(i => i.DateKey == DateTime.MinValue).OrderBy(i => i.Display).ToList();

                cmbMes.DataSource = null;
                cmbMes.DataSource = filtered;
                cmbMes.DisplayMember = nameof(MonthItem.Display);
                cmbMes.ValueMember = nameof(MonthItem.Key);

                if (cmbMes.Items.Count > 0 && cmbMes.SelectedIndex == -1)
                    cmbMes.SelectedIndex = 0;
            }
            catch { }
        }

        private void CmbMes_DrawItem(object? sender, DrawItemEventArgs e)
        {
            try
            {
                e.DrawBackground();
                var combo = sender as ComboBox;
                string text = string.Empty;
                if (e.Index >= 0 && combo != null && combo.Items.Count > e.Index)
                {
                    if (combo.Items[e.Index] is MonthItem mi)
                        text = mi.Display ?? string.Empty;
                    else
                        text = combo.Items[e.Index]?.ToString() ?? string.Empty;
                }
                else if (combo != null && combo.SelectedItem is MonthItem sel)
                {
                    text = sel.Display ?? string.Empty;
                }

                using (var b = new SolidBrush(Color.FromArgb(40, 40, 40)))
                    e.Graphics.DrawString(text, this.Font, b, e.Bounds);

                e.DrawFocusRectangle();
            }
            catch { }
        }

        private void btnVerificar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartes.Text))
            {
                MessageBox.Show("Ingrese al menos un número de parte antes de continuar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PartesIngresadas = txtPartes.Text.Trim();
            // Construir clave yyyy-MM a partir de cmbAno + cmbMes si es posible
            try
            {
                string? yearStr = cmbAno.SelectedItem?.ToString();
                if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out int year))
                {
                    if (cmbMes.SelectedItem is MonthItem mitem && mitem.DateKey != DateTime.MinValue)
                    {
                        MesSeleccionado = new DateTime(year, mitem.DateKey.Month, 1).ToString("yyyy-MM");
                    }
                    else if (cmbMes.SelectedItem is MonthItem mitem2 && !string.IsNullOrWhiteSpace(mitem2.Key))
                    {
                        // intentar extraer mes desde la Key
                        var mm = System.Text.RegularExpressions.Regex.Match(mitem2.Key, "\\d{4}-(\\d{2})");
                        if (mm.Success && int.TryParse(mm.Groups[1].Value, out int mo))
                            MesSeleccionado = new DateTime(year, mo, 1).ToString("yyyy-MM");
                        else
                            MesSeleccionado = $"{year}-{(mitem2.DateKey != DateTime.MinValue ? mitem2.DateKey.Month.ToString("00") : "01")}";
                    }
                    else
                    {
                        // fallback: usar año seleccionado y mes 01
                        MesSeleccionado = $"{year}-01";
                    }
                }
                else
                {
                    // sin año seleccionado, intentar mantener key existente
                    if (cmbMes.SelectedItem is MonthItem mi)
                        MesSeleccionado = mi.Key;
                    else
                        MesSeleccionado = cmbMes.SelectedItem?.ToString();
                }
            }
            catch
            {
                if (cmbMes.SelectedItem is MonthItem mi)
                    MesSeleccionado = mi.Key;
                else
                    MesSeleccionado = cmbMes.SelectedItem?.ToString();
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PartesIngresadas = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }


    }
}
