using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetallePedimentos : Form
    {
        private readonly List<ReporteIGIPagado> pedimentos;
        private readonly string mesSeleccionado;
        private readonly string formaPago;
        private readonly string tipoReporte;

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

                var pedimentosFiltrados = pedimentos
                    .Where(p => p.FechaPago.HasValue &&
                                p.FechaPago.Value.ToString("MMMM yyyy") == mesSeleccionado &&
                                p.FormaPago_IGI == formaPago)
                    .OrderBy(p => p.FechaPago)
                    .ThenBy(p => p.Pedimento);

                foreach (var pedimento in pedimentosFiltrados)
                {
                    // Diferencia: Calculado - Pagado (positivo = ahorro)
                    decimal diferencia = pedimento.IGI_Calculado - pedimento.IGI_Pagado;

                    dt.Rows.Add(
                        pedimento.FechaPago?.ToString("dd/MM/yyyy") ?? "",
                        pedimento.Pedimento,
                        pedimento.IGI_Pagado,
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

                var pedimentosFiltrados = pedimentos
                    .Where(p => p.FechaPago.HasValue &&
                                p.FechaPago.Value.ToString("MMMM yyyy") == mesSeleccionado &&
                                p.FormaPago_IVA == formaPago)
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
