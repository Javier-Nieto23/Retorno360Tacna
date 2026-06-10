using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.HELPERS
{
    internal static class DataGridViewManualCopyHelper
    {
        private static readonly HashSet<DataGridView> gridsConfigurados = new();
        private static readonly HashSet<Control> contenedoresConfigurados = new();
        private static readonly Dictionary<DataGridView, TextBox> editoresTemporales = new();

        public static void ConfigurarControles(Control contenedor)
        {
            if (contenedor == null)
                return;

            AplicarRecursivo(contenedor);
        }

        public static void Configurar(DataGridView grid)
        {
            if (grid == null || gridsConfigurados.Contains(grid))
                return;

            gridsConfigurados.Add(grid);
            grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            grid.CellMouseClick += Grid_CellMouseClick;
            grid.Scroll += Grid_CerrarEditor;
            grid.SizeChanged += Grid_CerrarEditor;
            grid.Leave += Grid_CerrarEditor;
            grid.ColumnWidthChanged += Grid_ColumnWidthChanged;
            grid.RowHeightChanged += Grid_RowHeightChanged;
            grid.Disposed += Grid_Disposed;
        }

        private static void AplicarRecursivo(Control control)
        {
            ConfigurarContenedor(control);

            if (control is DataGridView grid)
            {
                Configurar(grid);
            }

            foreach (Control child in control.Controls)
            {
                AplicarRecursivo(child);
            }
        }

        private static void ConfigurarContenedor(Control control)
        {
            if (!contenedoresConfigurados.Add(control))
                return;

            control.ControlAdded += Control_ControlAdded;
            control.Disposed += Control_Disposed;
        }

        private static void Control_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control == null)
                return;

            AplicarRecursivo(e.Control);
        }

        private static void Control_Disposed(object? sender, EventArgs e)
        {
            if (sender is Control control)
            {
                contenedoresConfigurados.Remove(control);
            }
        }

        private static void Grid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (sender is not DataGridView grid || e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell is DataGridViewButtonCell || cell is DataGridViewCheckBoxCell || cell is DataGridViewImageCell)
                return;

            try
            {
                grid.ClearSelection();
                cell.Selected = true;
                grid.CurrentCell = cell;
                MostrarEditorTemporalCelda(grid, e.RowIndex, e.ColumnIndex);
            }
            catch
            {
            }
        }

        private static void MostrarEditorTemporalCelda(DataGridView grid, int rowIndex, int columnIndex)
        {
            CerrarEditorTemporal(grid);

            Rectangle rect = grid.GetCellDisplayRectangle(columnIndex, rowIndex, true);
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            string texto = grid.Rows[rowIndex].Cells[columnIndex].Value?.ToString() ?? string.Empty;
            int anchoDisponible = Math.Max(120, grid.ClientSize.Width - rect.X - 5);
            int anchoEditor = Math.Min(Math.Max(rect.Width + 20, 220), anchoDisponible);
            int altoEditor = texto.Contains(Environment.NewLine) || texto.Length > 80
                ? Math.Min(120, Math.Max(rect.Height + 40, 70))
                : rect.Height + 6;

            var editor = new TextBox
            {
                Parent = grid,
                Multiline = altoEditor > rect.Height + 10,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = grid.DefaultCellStyle.Font ?? grid.Font,
                Text = texto,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Bounds = new Rectangle(rect.X, rect.Y, anchoEditor, altoEditor),
                ScrollBars = ScrollBars.Both,
                ShortcutsEnabled = true,
                WordWrap = false
            };

            editor.KeyDown += Editor_KeyDown;
            editor.LostFocus += Editor_LostFocus;
            editor.BringToFront();
            editor.Focus();
            editor.SelectAll();

            editoresTemporales[grid] = editor;
        }

        private static void Editor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox editor || editor.Parent is not DataGridView grid)
                return;

            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CerrarEditorTemporal(grid);
            }
        }

        private static void Editor_LostFocus(object? sender, EventArgs e)
        {
            if (sender is TextBox editor && editor.Parent is DataGridView grid)
            {
                CerrarEditorTemporal(grid);
            }
        }

        private static void Grid_CerrarEditor(object? sender, EventArgs e)
        {
            if (sender is DataGridView grid)
            {
                CerrarEditorTemporal(grid);
            }
        }

        private static void Grid_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
        {
            if (sender is DataGridView grid)
            {
                CerrarEditorTemporal(grid);
            }
        }

        private static void Grid_RowHeightChanged(object? sender, DataGridViewRowEventArgs e)
        {
            if (sender is DataGridView grid)
            {
                CerrarEditorTemporal(grid);
            }
        }

        private static void Grid_Disposed(object? sender, EventArgs e)
        {
            if (sender is not DataGridView grid)
                return;

            CerrarEditorTemporal(grid);
            gridsConfigurados.Remove(grid);
        }

        private static void CerrarEditorTemporal(DataGridView grid)
        {
            if (!editoresTemporales.TryGetValue(grid, out var editor))
                return;

            editor.KeyDown -= Editor_KeyDown;
            editor.LostFocus -= Editor_LostFocus;

            if (editor.Parent != null)
            {
                editor.Parent.Controls.Remove(editor);
            }

            editor.Dispose();
            editoresTemporales.Remove(grid);
        }
    }
}
