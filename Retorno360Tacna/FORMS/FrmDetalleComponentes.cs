using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmDetalleComponentes : Form
    {
        private readonly List<ComponenteBOM> componentesOriginales;
        private List<ComponenteBOM> componentesFiltrados;
        private readonly string _baseDatos;
        private readonly DateTime _fechaActual;

        // Paginación
        private const int REGISTROS_POR_PAGINA = 70;
        private const int UMBRAL_PAGINACION = 10000;
        private int paginaActual = 1;
        private int totalPaginas = 1;
        private bool usarPaginacion = false;

        public FrmDetalleComponentes(List<ComponenteBOM> componentes, string baseDatos, DateTime fechaActual)
        {
            InitializeComponent();
            componentesOriginales = componentes;
            componentesFiltrados = componentes;
            _baseDatos = baseDatos;
            _fechaActual = fechaActual;

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
                ConfigurarPaginacion();
                MostrarComponentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarPaginacion()
        {
            usarPaginacion = componentesOriginales.Count > UMBRAL_PAGINACION;

            if (usarPaginacion)
            {
                totalPaginas = (int)Math.Ceiling((double)componentesOriginales.Count / REGISTROS_POR_PAGINA);
                paginaActual = 1;

                // Mostrar controles de paginación
                panelPaginacion.Visible = true;
                ActualizarControlesPaginacion();
            }
            else
            {
                // Ocultar controles de paginación
                panelPaginacion.Visible = false;
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

            dgvDetalles.Columns.Clear();

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Par_NoPartePadre",
                HeaderText = "NO. PARTE PADRE",
                DataPropertyName = "Par_NoPartePadre",
                FillWeight = 30
            });

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Componente",
                HeaderText = "COMPONENTE",
                DataPropertyName = "Componente",
                FillWeight = 40
            });

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EstatusComponente",
                HeaderText = "ESTATUS",
                DataPropertyName = "EstatusComponente",
                FillWeight = 30,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });
        }

        private void MostrarComponentes()
        {
            List<ComponenteBOM> componentesAMostrar;

            if (usarPaginacion && string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                // Aplicar paginación solo si no hay búsqueda activa
                int inicio = (paginaActual - 1) * REGISTROS_POR_PAGINA;
                componentesAMostrar = componentesFiltrados.Skip(inicio).Take(REGISTROS_POR_PAGINA).ToList();
            }
            else
            {
                // Sin paginación o con búsqueda activa
                componentesAMostrar = componentesFiltrados;
            }

            dgvDetalles.DataSource = null;
            dgvDetalles.DataSource = componentesAMostrar;

            // Colorear filas según estatus
            foreach (DataGridViewRow row in dgvDetalles.Rows)
            {
                if (row.DataBoundItem is ComponenteBOM componente)
                {
                    if (componente.EstatusComponente == "VIGENTE EN BOM")
                    {
                        row.Cells["EstatusComponente"].Style.ForeColor = Color.FromArgb(39, 174, 96);
                    }
                    else
                    {
                        row.Cells["EstatusComponente"].Style.ForeColor = Color.FromArgb(231, 76, 60);
                    }
                }
            }

            ActualizarResumen();

            if (usarPaginacion && string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                ActualizarControlesPaginacion();
            }
        }

        private void ActualizarControlesPaginacion()
        {
            lblPaginaInfo.Text = $"Página {paginaActual} de {totalPaginas} ({REGISTROS_POR_PAGINA:N0} registros por página)";
            lblTotalRegistros.Text = $"Total de registros: {componentesFiltrados.Count:N0}";

            btnPrimeraPagina.Enabled = paginaActual > 1;
            btnPaginaAnterior.Enabled = paginaActual > 1;
            btnPaginaSiguiente.Enabled = paginaActual < totalPaginas;
            btnUltimaPagina.Enabled = paginaActual < totalPaginas;
        }

        private void ActualizarResumen()
        {
            int totalComponentes = componentesFiltrados.Count;
            int vigentes = componentesFiltrados.Count(d => d.EstatusComponente == "VIGENTE EN BOM");
            int noVigentes = componentesFiltrados.Count(d => d.EstatusComponente == "NO ESTA EN BOM");

            if (usarPaginacion && string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                int inicio = (paginaActual - 1) * REGISTROS_POR_PAGINA + 1;
                int fin = Math.Min(paginaActual * REGISTROS_POR_PAGINA, totalComponentes);

                lblResumen.Text = totalComponentes == componentesOriginales.Count
                    ? $"Mostrando {inicio:N0} - {fin:N0} de {totalComponentes:N0} | Vigentes: {vigentes:N0} | No Vigentes: {noVigentes:N0}"
                    : $"Mostrando {inicio:N0} - {fin:N0} de {totalComponentes:N0} (Filtrado de {componentesOriginales.Count:N0}) | Vigentes: {vigentes:N0} | No Vigentes: {noVigentes:N0}";
            }
            else
            {
                lblResumen.Text = totalComponentes == componentesOriginales.Count
                    ? $"Total Componentes: {totalComponentes:N0} | Vigentes: {vigentes:N0} | No Vigentes: {noVigentes:N0}"
                    : $"Mostrando: {totalComponentes:N0} de {componentesOriginales.Count:N0} | Vigentes: {vigentes:N0} | No Vigentes: {noVigentes:N0}";
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string filtro = txtBuscar.Text.ToLower();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    componentesFiltrados = componentesOriginales;

                    // Restaurar paginación si está habilitada
                    if (usarPaginacion)
                    {
                        totalPaginas = (int)Math.Ceiling((double)componentesFiltrados.Count / REGISTROS_POR_PAGINA);
                        paginaActual = 1;
                        panelPaginacion.Visible = true;
                    }
                }
                else
                {
                    componentesFiltrados = componentesOriginales.Where(d =>
                        d.Par_NoPartePadre.ToLower().Contains(filtro) ||
                        d.Componente.ToLower().Contains(filtro) ||
                        d.EstatusComponente.ToLower().Contains(filtro)
                    ).ToList();

                    // Ocultar paginación durante búsqueda
                    if (usarPaginacion)
                    {
                        panelPaginacion.Visible = false;
                    }
                }

                MostrarComponentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrimeraPagina_Click(object sender, EventArgs e)
        {
            paginaActual = 1;
            MostrarComponentes();
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                MostrarComponentes();
            }
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                MostrarComponentes();
            }
        }

        private void btnUltimaPagina_Click(object sender, EventArgs e)
        {
            paginaActual = totalPaginas;
            MostrarComponentes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
