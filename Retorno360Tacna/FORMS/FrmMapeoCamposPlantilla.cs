using Retorno360Tacna.MODELS;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Diálogo que muestra los campos de la plantilla y permite al usuario
    /// indicar cuál columna del Excel mensual corresponde a cada uno.
    /// </summary>
    public sealed class FrmMapeoCamposPlantilla : Form
    {
        // Resultado: clave = campo de la plantilla, valor = columna del Excel elegida
        public Dictionary<string, string> Mapeo { get; } = new();

        private readonly List<(Label lbl, ComboBox cmb)> _filas = new();

        public FrmMapeoCamposPlantilla(
            PlantillaInventarioConfig plantilla,
            IReadOnlyList<string> columnasExcel)
        {
            Text            = "Relacionar campos con la plantilla";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Font            = new Font("Segoe UI", 9F);
            BackColor       = Color.FromArgb(245, 247, 250);

            var campos = plantilla.CamposPlantilla().ToList();
            int formHeight = 130 + campos.Count * 58;
            ClientSize = new Size(480, formHeight);

            // ── Encabezado ────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text      = "Relacionar campos de la plantilla",
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 78, 121),
                AutoSize  = true,
                Location  = new Point(20, 16)
            };
            var lblSub = new Label
            {
                Text      = $"Plantilla: {System.IO.Path.GetFileName(plantilla.RutaArchivo)}",
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(20, 42)
            };
            Controls.Add(lblTitulo);
            Controls.Add(lblSub);

            // ── Filas de mapeo ────────────────────────────────────────────────
            int y = 70;
            foreach (string campo in campos)
            {
                var lbl = new Label
                {
                    Text      = campo,
                    Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 60, 70),
                    AutoSize  = true,
                    Location  = new Point(20, y + 4)
                };
                var cmb = new ComboBox
                {
                    DropDownStyle     = ComboBoxStyle.DropDownList,
                    Font              = new Font("Segoe UI", 9F),
                    Location          = new Point(200, y),
                    Size              = new Size(260, 25),
                    FormattingEnabled = true
                };
                cmb.Items.AddRange(columnasExcel.Cast<object>().ToArray());
                // Pre-seleccionar si coincide el nombre exacto
                int coincide = columnasExcel.ToList().FindIndex(
                    c => string.Equals(c, campo, System.StringComparison.OrdinalIgnoreCase));
                cmb.SelectedIndex = coincide >= 0 ? coincide : (cmb.Items.Count > 0 ? 0 : -1);

                Controls.Add(lbl);
                Controls.Add(cmb);
                _filas.Add((lbl, cmb));
                y += 52;
            }

            // ── Botones ───────────────────────────────────────────────────────
            var panelBot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(237, 242, 247)
            };

            var btnAceptar = new Button
            {
                Text             = "Aceptar",
                DialogResult     = DialogResult.OK,
                Font             = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor        = Color.White,
                BackColor        = Color.FromArgb(22, 163, 74),
                FlatStyle        = FlatStyle.Flat,
                Size             = new Size(120, 36),
                Location         = new Point(340, 8),
                Cursor           = Cursors.Hand
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (_, _) => ConfirmarMapeo();

            var btnCancelar = new Button
            {
                Text         = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Font         = new Font("Segoe UI", 10F),
                ForeColor    = Color.White,
                BackColor    = Color.FromArgb(100, 116, 139),
                FlatStyle    = FlatStyle.Flat,
                Size         = new Size(110, 36),
                Location     = new Point(220, 8),
                Cursor       = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            panelBot.Controls.Add(btnAceptar);
            panelBot.Controls.Add(btnCancelar);
            Controls.Add(panelBot);

            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }

        private void ConfirmarMapeo()
        {
            Mapeo.Clear();
            foreach (var (lbl, cmb) in _filas)
            {
                string columnaElegida = cmb.SelectedItem?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(columnaElegida))
                    Mapeo[lbl.Text] = columnaElegida;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
