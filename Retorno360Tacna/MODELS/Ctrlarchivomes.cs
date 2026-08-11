using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.CONTROLS
{
    /// <summary>
    /// Fila individual del consolidado: permite elegir el mes, subir su archivo Excel,
    /// elegir la hoja, el modo de cálculo (suma simple o producto A×B) y el/los campo(s)
    /// correspondientes. Se construye completamente en código, sin archivo .Designer.
    /// </summary>
    public class CtrlArchivoMes : UserControl
    {
        public MesArchivoItem Item { get; } = new MesArchivoItem();

        /// <summary>Se dispara cada vez que el total de esta fila se recalcula.</summary>
        public event EventHandler? TotalActualizado;

        private readonly ExcelColumnaService _excelService = new ExcelColumnaService();
        private List<string> _columnasDisponibles = new List<string>();

        private static readonly string[] Meses =
        {
            "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
        };

        private readonly ComboBox cboMes = new ComboBox();
        private readonly Button btnSeleccionarArchivo = new Button();
        private readonly Label lblArchivo = new Label();
        private readonly Label lblHoja = new Label();
        private readonly ComboBox cboHoja = new ComboBox();

        private readonly ComboBox cboModo = new ComboBox();

        private readonly Label lblCampoSimple = new Label();
        private readonly ComboBox cboCampoSimple = new ComboBox();

        private readonly Label lblCampoA = new Label();
        private readonly ComboBox cboCampoA = new ComboBox();
        private readonly Label lblCampoB = new Label();
        private readonly ComboBox cboCampoB = new ComboBox();

        private readonly Label lblTotal = new Label();

        public CtrlArchivoMes(int numeroFila)
        {
            ConstruirUi(numeroFila);
        }

        private void ConstruirUi(int numeroFila)
        {
            Size = new Size(940, 84);
            Margin = new Padding(0, 0, 0, 6);
            BorderStyle = BorderStyle.FixedSingle;

            Label lblNumero = new Label
            {
                Text = $"#{numeroFila}",
                Location = new Point(8, 12),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold)
            };

            Label lblMes = new Label { Text = "Mes:", Location = new Point(45, 14), AutoSize = true };
            cboMes.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMes.Items.AddRange(Meses);
            cboMes.Location = new Point(85, 10);
            cboMes.Width = 100;
            cboMes.SelectedIndexChanged += (s, e) => Item.Mes = cboMes.SelectedItem?.ToString() ?? string.Empty;

            btnSeleccionarArchivo.Text = "Seleccionar Excel...";
            btnSeleccionarArchivo.Location = new Point(195, 8);
            btnSeleccionarArchivo.Width = 135;
            btnSeleccionarArchivo.Click += BtnSeleccionarArchivo_Click;

            lblArchivo.Text = "Sin archivo seleccionado";
            lblArchivo.Location = new Point(340, 14);
            lblArchivo.AutoSize = false;
            lblArchivo.Width = 230;
            lblArchivo.ForeColor = Color.DimGray;

            lblHoja.Text = "Hoja:";
            lblHoja.Location = new Point(575, 14);
            lblHoja.AutoSize = true;
            lblHoja.Visible = false;

            cboHoja.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHoja.Location = new Point(610, 10);
            cboHoja.Width = 150;
            cboHoja.Visible = false;
            cboHoja.SelectedIndexChanged += CboHoja_SelectedIndexChanged;

            cboModo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModo.Items.AddRange(new object[] { "Suma simple de un campo", "Multiplicar Campo A × Campo B y sumar" });
            cboModo.Location = new Point(45, 46);
            cboModo.Width = 250;
            cboModo.SelectedIndexChanged += CboModo_SelectedIndexChanged;

            lblCampoSimple.Text = "Campo a sumar:";
            lblCampoSimple.Location = new Point(305, 50);
            lblCampoSimple.AutoSize = true;

            cboCampoSimple.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCampoSimple.Location = new Point(405, 46);
            cboCampoSimple.Width = 150;
            cboCampoSimple.SelectedIndexChanged += (s, e) =>
            {
                Item.CampoSimple = cboCampoSimple.SelectedItem?.ToString();
                RecalcularSiEsPosible();
            };

            lblCampoA.Text = "Campo A:";
            lblCampoA.Location = new Point(305, 50);
            lblCampoA.AutoSize = true;
            lblCampoA.Visible = false;

            cboCampoA.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCampoA.Location = new Point(365, 46);
            cboCampoA.Width = 120;
            cboCampoA.Visible = false;
            cboCampoA.SelectedIndexChanged += (s, e) =>
            {
                Item.CampoA = cboCampoA.SelectedItem?.ToString();
                RecalcularSiEsPosible();
            };

            lblCampoB.Text = "Campo B:";
            lblCampoB.Location = new Point(495, 50);
            lblCampoB.AutoSize = true;
            lblCampoB.Visible = false;

            cboCampoB.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCampoB.Location = new Point(555, 46);
            cboCampoB.Width = 120;
            cboCampoB.Visible = false;
            cboCampoB.SelectedIndexChanged += (s, e) =>
            {
                Item.CampoB = cboCampoB.SelectedItem?.ToString();
                RecalcularSiEsPosible();
            };

            lblTotal.Text = "Total: --";
            lblTotal.Location = new Point(770, 30);
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font(Font, FontStyle.Bold);

            Controls.AddRange(new Control[]
            {
                lblNumero, lblMes, cboMes, btnSeleccionarArchivo, lblArchivo,
                lblHoja, cboHoja, cboModo,
                lblCampoSimple, cboCampoSimple,
                lblCampoA, cboCampoA, lblCampoB, cboCampoB,
                lblTotal
            });

            cboMes.SelectedIndex = Math.Min(numeroFila - 1, Meses.Length - 1);
            cboModo.SelectedIndex = 0; // Suma simple por defecto
        }

        private void BtnSeleccionarArchivo_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialogo = new OpenFileDialog
            {
                Filter = "Archivos de Excel (*.xlsx)|*.xlsx",
                Title = "Seleccione el archivo Excel de este mes"
            };

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                Item.RutaArchivo = dialogo.FileName;
                lblArchivo.Text = Item.NombreArchivo;

                List<string> hojas = _excelService.ObtenerHojas(dialogo.FileName);

                cboHoja.Items.Clear();
                cboCampoSimple.Items.Clear();
                cboCampoA.Items.Clear();
                cboCampoB.Items.Clear();
                Item.Total = 0;
                Item.Calculado = false;
                lblTotal.Text = "Total: --";

                if (hojas.Count == 0)
                {
                    MessageBox.Show(this, "El archivo no contiene hojas válidas.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblHoja.Visible = false;
                    cboHoja.Visible = false;
                    return;
                }

                lblHoja.Visible = true;
                cboHoja.Visible = true;
                cboHoja.Items.AddRange(hojas.ToArray());
                cboHoja.SelectedIndex = 0; // dispara CboHoja_SelectedIndexChanged
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("No fue posible leer el archivo Excel.",
                    "Consolidado mensual", ex, "Selección y análisis de archivo Excel");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CboHoja_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboHoja.SelectedItem is not string nombreHoja || string.IsNullOrWhiteSpace(Item.RutaArchivo))
                return;

            Item.HojaSeleccionada = nombreHoja;

            try
            {
                Cursor = Cursors.WaitCursor;
                _columnasDisponibles = _excelService.ObtenerColumnas(Item.RutaArchivo, nombreHoja);

                cboCampoSimple.Items.Clear();
                cboCampoA.Items.Clear();
                cboCampoB.Items.Clear();

                if (_columnasDisponibles.Count == 0)
                {
                    MessageBox.Show(this, "La hoja seleccionada no tiene columnas detectables.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cboCampoSimple.Items.AddRange(_columnasDisponibles.ToArray());
                cboCampoA.Items.AddRange(_columnasDisponibles.ToArray());
                cboCampoB.Items.AddRange(_columnasDisponibles.ToArray());

                if (cboCampoSimple.Items.Count > 0) cboCampoSimple.SelectedIndex = 0;
                if (cboCampoA.Items.Count > 0) cboCampoA.SelectedIndex = 0;
                if (cboCampoB.Items.Count > 1) cboCampoB.SelectedIndex = 1;
                else if (cboCampoB.Items.Count > 0) cboCampoB.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError("No fue posible analizar la hoja seleccionada.",
                    "Consolidado mensual", ex, "Análisis de hoja Excel");
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            RecalcularSiEsPosible();
        }

        private void CboModo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool esSumaSimple = cboModo.SelectedIndex == 0;
            Item.Modo = esSumaSimple ? ModoCalculoMes.SumaSimple : ModoCalculoMes.ProductoAB;

            lblCampoSimple.Visible = esSumaSimple;
            cboCampoSimple.Visible = esSumaSimple;

            lblCampoA.Visible = !esSumaSimple;
            cboCampoA.Visible = !esSumaSimple;
            lblCampoB.Visible = !esSumaSimple;
            cboCampoB.Visible = !esSumaSimple;

            RecalcularSiEsPosible();
        }

        private void InitializeComponent()
        {

        }

        private void RecalcularSiEsPosible()
        {
            if (!Item.EstaCompleto())
            {
                Item.Calculado = false;
                lblTotal.Text = "Total: --";
                return;
            }

            try
            {
                Item.Total = Item.Modo == ModoCalculoMes.SumaSimple
                    ? _excelService.CalcularSumaSimple(Item.RutaArchivo, Item.HojaSeleccionada!, Item.CampoSimple!)
                    : _excelService.CalcularSumaProducto(Item.RutaArchivo, Item.HojaSeleccionada!, Item.CampoA!, Item.CampoB!);

                Item.Calculado = true;
                lblTotal.Text = $"Total: {Item.Total:N2}";
                TotalActualizado?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Item.Calculado = false;
                lblTotal.Text = "Total: error";
                ErrorMessageHelper.ShowError("No fue posible calcular el total de esta fila.",
                    "Consolidado mensual", ex, "Cálculo de total por fila");
            }
        }
    }
}