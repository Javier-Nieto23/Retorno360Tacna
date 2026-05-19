using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetalleComponentes : Form
    {
        private readonly CatalogoPartesService _service;
        private readonly string _baseDatos;
        private readonly DateTime _fechaInicio;
        private readonly DateTime _fechaFin;
        private List<DetalleComponente> detallesActuales = new List<DetalleComponente>();

        public FrmDetalleComponentes(CatalogoPartesService service, string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            InitializeComponent();
            _service = service;
            _baseDatos = baseDatos;
            _fechaInicio = fechaInicio;
            _fechaFin = fechaFin;

            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void FrmDetalleComponentes_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarDataGridView();
                CargarDetalles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarDataGridView()
        {
            dgvDetalles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalles.AutoGenerateColumns = false;
            dgvDetalles.AllowUserToAddRows = false;
            dgvDetalles.AllowUserToDeleteRows = false;
            dgvDetalles.ReadOnly = true;
            dgvDetalles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalles.MultiSelect = false;
            dgvDetalles.BackgroundColor = Color.White;
            dgvDetalles.BorderStyle = BorderStyle.None;
            dgvDetalles.RowHeadersVisible = false;
            dgvDetalles.EnableHeadersVisualStyles = false;
            dgvDetalles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);
            dgvDetalles.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 204, 113);
            dgvDetalles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetalles.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvDetalles.DefaultCellStyle.Padding = new Padding(5, 3, 5, 3);
            dgvDetalles.RowTemplate.Height = 30;

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Par_NoParte",
                HeaderText = "NO. PARTE",
                DataPropertyName = "Par_NoParte",
                FillWeight = 20
            });

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Par_DescripcionEsp",
                HeaderText = "DESCRIPCIÓN",
                DataPropertyName = "Par_DescripcionEsp",
                FillWeight = 50
            });

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Par_InsercionFecha",
                HeaderText = "FECHA INSERCIÓN",
                DataPropertyName = "Par_InsercionFecha",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Format = "dd/MM/yyyy"
                }
            });

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExisteEnBOM",
                HeaderText = "EN BOM",
                DataPropertyName = "ExisteEnBOM",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });
        }

        private void CargarDetalles()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                detallesActuales = _service.ObtenerDetalleComponentes(_baseDatos, _fechaInicio, _fechaFin);

                dgvDetalles.DataSource = null;
                dgvDetalles.DataSource = detallesActuales;

                ActualizarResumen();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ActualizarResumen()
        {
            int totalPartes = detallesActuales.Count;
            int enBOM = detallesActuales.Count(d => d.ExisteEnBOM == "SI");
            int noEnBOM = detallesActuales.Count(d => d.ExisteEnBOM == "NO");

            lblResumen.Text = $"Total Partes MP: {totalPartes:N0} | En BOM: {enBOM:N0} | No en BOM: {noEnBOM:N0}";
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string filtro = txtBuscar.Text.ToLower();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    dgvDetalles.DataSource = null;
                    dgvDetalles.DataSource = detallesActuales;
                    ActualizarResumen();
                }
                else
                {
                    var filtrados = detallesActuales.Where(d =>
                        d.Par_NoParte.ToLower().Contains(filtro) ||
                        d.Par_DescripcionEsp.ToLower().Contains(filtro) ||
                        d.ExisteEnBOM.ToLower().Contains(filtro)
                    ).ToList();

                    dgvDetalles.DataSource = null;
                    dgvDetalles.DataSource = filtrados;

                    int totalPartes = filtrados.Count;
                    int enBOM = filtrados.Count(d => d.ExisteEnBOM == "SI");
                    lblResumen.Text = $"Mostrando: {totalPartes:N0} de {detallesActuales.Count:N0} | En BOM: {enBOM:N0}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
