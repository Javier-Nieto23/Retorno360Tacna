using Retorno360Tacna.MODELS;
using System.Globalization;
using System.Linq;
using System;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetallePedimentos : Form
    {
        private readonly List<ReporteIGIPagado> pedimentos;
        private readonly string mesSeleccionado;
        private readonly string formaPago;
        private readonly string tipoReporte;

        // Filtro para mostrar solo pedimentos posiblemente RT (IGI_Pagado == 0 && IGI_Calculado == 0)
        private bool filtrarRT = false;
        private Button? btnFiltrarRT;

        private string currentValue = "0";
        private string operation = "";
        private decimal firstOperand = 0;
        private bool isNewEntry = true;
        private bool isClosing = false;

        public FrmDetallePedimentos(List<ReporteIGIPagado> pedimentos, string mes, string formaPago, string tipoReporte = "IGI")
        {
            InitializeComponent();
            this.pedimentos = pedimentos ?? new List<ReporteIGIPagado>();
            this.mesSeleccionado = mes;
            this.formaPago = formaPago;
            this.tipoReporte = tipoReporte;

            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void FrmDetallePedimentos_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            CargarDatos();
            ActualizarTitulo();
            ConfigurarCalculadora();

            // Crear botón de filtro RT dinámicamente solo para IGI y formas de pago 0 o 5
            if (tipoReporte == "IGI")
            {
                var formaPagoNorm = (formaPago ?? string.Empty).Trim();
                if ((formaPagoNorm == "0" || formaPagoNorm == "5") && btnFiltrarRT == null)
                {
                    btnFiltrarRT = new Button();
                    // Slightly wider visual button for better visibility
                    // Texto inicial: 'No RT' (al hacer click se ocultarán los RT); cuando el filtro esté activo mostrar 'Agregar RT'
                    btnFiltrarRT.Text = "No RT";
                    btnFiltrarRT.Size = new Size(90, 40);
                    btnFiltrarRT.TextAlign = ContentAlignment.MiddleCenter;
                    btnFiltrarRT.Padding = new Padding(0);
                    btnFiltrarRT.TabStop = false;
                    btnFiltrarRT.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    btnFiltrarRT.Click += BtnFiltrarRT_Click;
                    // Estética consistente con otros botones de cabecera
                    btnFiltrarRT.BackColor = Color.FromArgb(39, 174, 96);
                    btnFiltrarRT.Cursor = Cursors.Hand;
                    btnFiltrarRT.FlatAppearance.BorderSize = 0;
                    btnFiltrarRT.FlatStyle = FlatStyle.Flat;
                    btnFiltrarRT.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    btnFiltrarRT.ForeColor = Color.White;
                    btnFiltrarRT.FlatAppearance.MouseOverBackColor = Color.FromArgb(33, 150, 83);
                    btnFiltrarRT.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 130, 75);

                    // Posicionar dentro del panelHeader junto a los botones existentes (btnCalculadora/btnCerrar)
                    try
                    {
                        if (btnCalculadora != null)
                        {
                            int x = btnCalculadora.Left - btnFiltrarRT.Width - 12; // más espacio entre botones
                            int y = Math.Max(0, (panelHeader.Height - btnFiltrarRT.Height) / 2);
                            btnFiltrarRT.Location = new Point(x, y);
                        }
                        else
                        {
                            // Fallback al borde derecho del panel
                            int x = Math.Max(10, panelHeader.Width - btnFiltrarRT.Width - 110);
                            int y = Math.Max(0, (panelHeader.Height - btnFiltrarRT.Height) / 2);
                            btnFiltrarRT.Location = new Point(x, y);
                        }
                    }
                    catch
                    {
                        btnFiltrarRT.Location = new Point(Math.Max(10, this.ClientSize.Width - btnFiltrarRT.Width - 20), 10);
                    }

                    panelHeader.Controls.Add(btnFiltrarRT);
                }
            }
        }

        private void ConfigurarGrid()
        {
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.ReadOnly = true;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.MultiSelect = false;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);
            dgvDetalle.BorderStyle = BorderStyle.FixedSingle;
            dgvDetalle.EnableHeadersVisualStyles = false;
            dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetalle.ColumnHeadersDefaultCellStyle.Font = new Font(dgvDetalle.Font.FontFamily, 10, FontStyle.Bold);
            dgvDetalle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetalle.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvDetalle.DefaultCellStyle.Padding = new Padding(5, 3, 5, 3);
            dgvDetalle.RowTemplate.Height = 30;
        }

        private void CargarDatos()
        {
            var dt = new System.Data.DataTable();

            if (tipoReporte == "IGI")
            {
                dt.Columns.Add("FECHA", typeof(string));
                dt.Columns.Add("PEDIMENTO", typeof(string));
                dt.Columns.Add("IGI PAGADO", typeof(decimal));
                dt.Columns.Add("IGI CALCULADO", typeof(decimal));
                dt.Columns.Add("DIFERENCIA", typeof(decimal));
                dt.Columns.Add("FORMA DE PAGO", typeof(string));

                // Extraer año y mes del mesSeleccionado intentando varias culturas
                DateTime mesDt;
                bool mesValido = DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out mesDt)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out mesDt)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", new CultureInfo("es-PE"), DateTimeStyles.None, out mesDt)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out mesDt);

                var formaPagoNorm = (formaPago ?? string.Empty).Trim();

                var pedimentosFiltrados = pedimentos
                    .Where(p =>
                        p.FechaPago.HasValue &&
                        (
                            (mesValido && p.FechaPago.Value.Year == mesDt.Year && p.FechaPago.Value.Month == mesDt.Month)
                            || (!mesValido && p.FechaPago.Value.ToString("MMMM yyyy").Equals(mesSeleccionado, StringComparison.OrdinalIgnoreCase))
                        )
                        && ((p.FormaPago_IGI ?? string.Empty).Trim() == formaPagoNorm)
                    )
                    .OrderBy(p => p.FechaPago)
                    .ThenBy(p => p.Pedimento);

                // Aplicar filtro RT si está activado: eliminar pedimentos donde ambos IGI_Pagado y IGI_Calculado son 0
                // Mantener los pedimentos donde al menos uno de los campos no es cero
                var pedimentosFiltradosFinal = filtrarRT
                    ? pedimentosFiltrados.Where(p => !(p.IGI_Pagado == 0m && p.IGI_Calculado == 0m))
                    : pedimentosFiltrados;

                foreach (var pedimento in pedimentosFiltradosFinal)
                {
                    // Para forma de pago '5' (crédito) igualar IGI_Pagado a 0 en el detalle
                    var forma = (pedimento.FormaPago_IGI ?? string.Empty).Trim();
                    decimal displayIGIPagado = forma == "5" ? 0m : pedimento.IGI_Pagado;

                    // Diferencia: Calculado - Pagado (positivo = ahorro)
                    // Para forma '5' invertimos la diferencia para reflejar deuda
                    decimal diferencia;
                    if (forma == "5")
                        diferencia = displayIGIPagado - pedimento.IGI_Calculado; // negativo
                    else
                        diferencia = pedimento.IGI_Calculado - displayIGIPagado;

                    dt.Rows.Add(
                        pedimento.FechaPago?.ToString("dd/MM/yyyy") ?? "",
                        pedimento.Pedimento,
                        displayIGIPagado,
                        pedimento.IGI_Calculado,
                        diferencia,
                        pedimento.FormaPago_IGI
                    );
                }
            }
            else
            {
                dt.Columns.Add("FECHA", typeof(string));
                dt.Columns.Add("PEDIMENTO", typeof(string));
                dt.Columns.Add("IVA PAGADO", typeof(decimal));
                dt.Columns.Add("FORMA DE PAGO", typeof(string));

                // IVA: mismo tratamiento de fecha y forma de pago
                DateTime mesDtIva;
                bool mesValidoIva = DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out mesDtIva)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out mesDtIva)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", new CultureInfo("es-PE"), DateTimeStyles.None, out mesDtIva)
                    || DateTime.TryParseExact(mesSeleccionado, "MMMM yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out mesDtIva);

                var formaPagoNormIva = (formaPago ?? string.Empty).Trim();

                var pedimentosFiltrados = pedimentos
                    .Where(p =>
                        p.FechaPago.HasValue &&
                        (
                            (mesValidoIva && p.FechaPago.Value.Year == mesDtIva.Year && p.FechaPago.Value.Month == mesDtIva.Month)
                            || (!mesValidoIva && p.FechaPago.Value.ToString("MMMM yyyy").Equals(mesSeleccionado, StringComparison.OrdinalIgnoreCase))
                        )
                        && ((p.FormaPago_IVA ?? string.Empty).Trim() == formaPagoNormIva)
                    )
                    .OrderBy(p => p.FechaPago)
                    .ThenBy(p => p.Pedimento);

                foreach (var pedimento in pedimentosFiltrados)
                {
                    dt.Rows.Add(
                        pedimento.FechaPago?.ToString("dd/MM/yyyy") ?? "",
                        pedimento.Pedimento,
                        pedimento.IVA_Pagado,
                        pedimento.FormaPago_IVA
                    );
                }
            }

            dgvDetalle.DataSource = dt;
            FormatearColumnas();
            lblResumen.Text = $"Total Pedimentos: {dt.Rows.Count}";
        }

        private void FormatearColumnas()
        {
            if (dgvDetalle.Columns.Count == 0)
                return;

            if (dgvDetalle.Columns["FECHA"] != null)
            {
                dgvDetalle.Columns["FECHA"].Width = 100;
                dgvDetalle.Columns["FECHA"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvDetalle.Columns["PEDIMENTO"] != null)
            {
                dgvDetalle.Columns["PEDIMENTO"].Width = 150;
                dgvDetalle.Columns["PEDIMENTO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvDetalle.Columns["PEDIMENTO"].DefaultCellStyle.Font = new Font(dgvDetalle.Font.FontFamily, 9, FontStyle.Bold);
            }

            if (tipoReporte == "IGI")
            {
                if (dgvDetalle.Columns["IGI PAGADO"] != null)
                {
                    dgvDetalle.Columns["IGI PAGADO"].DefaultCellStyle.Format = "C2";
                    dgvDetalle.Columns["IGI PAGADO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvDetalle.Columns["IGI PAGADO"].Width = 130;
                }

                if (dgvDetalle.Columns["IGI CALCULADO"] != null)
                {
                    dgvDetalle.Columns["IGI CALCULADO"].DefaultCellStyle.Format = "C2";
                    dgvDetalle.Columns["IGI CALCULADO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvDetalle.Columns["IGI CALCULADO"].Width = 130;
                }

                if (dgvDetalle.Columns["DIFERENCIA"] != null)
                {
                    dgvDetalle.Columns["DIFERENCIA"].DefaultCellStyle.Format = "C2";
                    dgvDetalle.Columns["DIFERENCIA"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvDetalle.Columns["DIFERENCIA"].DefaultCellStyle.Font = new Font(dgvDetalle.Font.FontFamily, 9, FontStyle.Bold);
                    dgvDetalle.Columns["DIFERENCIA"].Width = 120;

                    foreach (DataGridViewRow row in dgvDetalle.Rows)
                    {
                        if (row.Cells["DIFERENCIA"].Value != null)
                        {
                            decimal diferencia = Convert.ToDecimal(row.Cells["DIFERENCIA"].Value);
                            // Verde para ahorro (positivo), Rojo para sobrepago (negativo)
                            if (diferencia > 0)
                            {
                                row.Cells["DIFERENCIA"].Style.ForeColor = Color.FromArgb(39, 174, 96); // Verde
                            }
                            else if (diferencia < 0)
                            {
                                row.Cells["DIFERENCIA"].Style.ForeColor = Color.FromArgb(192, 57, 43); // Rojo
                            }
                        }
                    }
                }
            }
            else
            {
                if (dgvDetalle.Columns["IVA PAGADO"] != null)
                {
                    dgvDetalle.Columns["IVA PAGADO"].DefaultCellStyle.Format = "C2";
                    dgvDetalle.Columns["IVA PAGADO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvDetalle.Columns["IVA PAGADO"].Width = 130;
                }
            }

            if (dgvDetalle.Columns["FORMA DE PAGO"] != null)
            {
                dgvDetalle.Columns["FORMA DE PAGO"].Width = 110;
                dgvDetalle.Columns["FORMA DE PAGO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void ActualizarTitulo()
        {
            string tipo = tipoReporte == "IGI" ? "IGI" : "IVA";
            lblTitulo.Text = $"Detalle de Pedimentos - {tipo}";
            lblTitulo.Text += $" | {mesSeleccionado} | FP-{formaPago}";
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (isClosing) return;
            isClosing = true;
            this.Close();
        }

        private void ConfigurarCalculadora()
        {
            this.KeyPreview = true;
            this.KeyDown += FrmDetallePedimentos_KeyDown;
        }

        private void BtnFiltrarRT_Click(object? sender, EventArgs e)
        {
            filtrarRT = !filtrarRT;
            if (btnFiltrarRT != null)
            {
                // Si el filtro está activado (se ocultaron los RT) mostramos la opción para "Agregar RT" (restaurar)
                btnFiltrarRT.Text = filtrarRT ? "Agregar RT" : "No RT";
            }

            // Recargar datos aplicando/quitando filtro
            CargarDatos();
        }

        private void btnCalculadora_Click(object sender, EventArgs e)
        {
            panelCalculadora.Visible = !panelCalculadora.Visible;
            if (panelCalculadora.Visible)
            {
                panelCalculadora.BringToFront();
                txtDisplay.Focus();
                ResetearCalculadora();
            }
        }

        private void btnCalcCerrar_Click(object sender, EventArgs e)
        {
            panelCalculadora.Visible = false;
        }

        private void btnNumber_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string numero = btn.Text;

            if (isNewEntry)
            {
                currentValue = numero == "." ? "0." : numero;
                isNewEntry = false;
            }
            else
            {
                if (numero == "." && currentValue.Contains("."))
                    return;

                currentValue = currentValue == "0" && numero != "." ? numero : currentValue + numero;
            }

            txtDisplay.Text = currentValue;
        }

        private void btnOperator_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (!string.IsNullOrEmpty(operation) && !isNewEntry)
            {
                CalcularResultado();
            }

            operation = btn.Text;
            firstOperand = decimal.Parse(currentValue);
            isNewEntry = true;
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            CalcularResultado();
            operation = "";
            isNewEntry = true;
        }

        private void CalcularResultado()
        {
            if (string.IsNullOrEmpty(operation) || isNewEntry)
                return;

            decimal secondOperand = decimal.Parse(currentValue);
            decimal resultado = 0;

            switch (operation)
            {
                case "+":
                    resultado = firstOperand + secondOperand;
                    break;
                case "-":
                    resultado = firstOperand - secondOperand;
                    break;
                case "*":
                    resultado = firstOperand * secondOperand;
                    break;
                case "/":
                    if (secondOperand != 0)
                        resultado = firstOperand / secondOperand;
                    else
                    {
                        txtDisplay.Text = "Error";
                        ResetearCalculadora();
                        return;
                    }
                    break;
            }

            currentValue = resultado.ToString();
            txtDisplay.Text = currentValue;
            firstOperand = resultado;
        }

        private void btnCalcClear_Click(object sender, EventArgs e)
        {
            ResetearCalculadora();
        }

        private void ResetearCalculadora()
        {
            currentValue = "0";
            operation = "";
            firstOperand = 0;
            isNewEntry = true;
            txtDisplay.Text = "0";
        }

        private void FrmDetallePedimentos_KeyDown(object sender, KeyEventArgs e)
        {
            if (!panelCalculadora.Visible)
                return;

            e.Handled = true;

            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                int numero = e.KeyCode - Keys.NumPad0;
                SimularClickNumero(numero.ToString());
            }
            else if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                int numero = e.KeyCode - Keys.D0;
                SimularClickNumero(numero.ToString());
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Divide:
                        btnCalcDivide.PerformClick();
                        break;
                    case Keys.Multiply:
                        btnCalcMultiply.PerformClick();
                        break;
                    case Keys.Subtract:
                        btnCalcSubtract.PerformClick();
                        break;
                    case Keys.Add:
                        btnCalcAdd.PerformClick();
                        break;
                    case Keys.Enter:
                        btnCalcEquals.PerformClick();
                        break;
                    case Keys.Decimal:
                        btnCalcDecimal.PerformClick();
                        break;
                    case Keys.Escape:
                        btnCalcClear.PerformClick();
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
        }

        private void txtDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            FrmDetallePedimentos_KeyDown(sender, e);
        }

        private void SimularClickNumero(string numero)
        {
            if (isNewEntry)
            {
                currentValue = numero;
                isNewEntry = false;
            }
            else
            {
                currentValue = currentValue == "0" ? numero : currentValue + numero;
            }
            txtDisplay.Text = currentValue;
        }

        private Point mouseLocation;
        private bool isDragging = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            panelHeader.MouseDown += PanelHeader_MouseDown;
            panelHeader.MouseMove += PanelHeader_MouseMove;
            panelHeader.MouseUp += PanelHeader_MouseUp;
            lblTitulo.MouseDown += PanelHeader_MouseDown;
            lblTitulo.MouseMove += PanelHeader_MouseMove;
            lblTitulo.MouseUp += PanelHeader_MouseUp;
        }

        private void PanelHeader_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseLocation = e.Location;
            }
        }

        private void PanelHeader_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - mouseLocation.X;
                newLocation.Y += e.Y - mouseLocation.Y;
                this.Location = newLocation;
            }
        }

        private void PanelHeader_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }
    }
}
