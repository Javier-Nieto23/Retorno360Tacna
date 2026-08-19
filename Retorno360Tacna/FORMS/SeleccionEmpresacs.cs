using Retorno360Tacna.CNX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.SERVICES;

namespace Retorno360Tacna.FORMS
{
    // Controles esperados en el diseñador:
    //   comboBox del label1 (Usuario, solo informativo, deshabilitado) -> cmbUsuario
    //   comboBox del label2 (Razón Social)                             -> cmbRazon
    //   comboBox del label3 (Empresa)                                  -> cmbEmpresa
    //   label4 (instrucciones)
    //   button "Agregar fila"                                          -> btnFila
    //   DataGridView de preview                                        -> dtgPreview
    //   button "Confirmar" (guarda TODO el preview)                    -> btnConfirmar
    //   button "Cancelar"                                              -> btnCancelar
    public partial class SeleccionEmpresacs : Form
    {
        private readonly ConexionInfo? conexion;
        private readonly Usuario? usuarioActual;
        private readonly CumplimientoAnexosService cumplimientoService;
        private readonly PerfilUsuarioService perfilService;
        private readonly BindingList<PerfilPreviewItem> filasPreview = new();

        // El formulario se embebe dentro de panelContenido (MainMenu), no es
        // modal, así que Close() no "regresa" a nada: solo lo destruye. En
        // vez de eso, se avisa al padre para que vuelva a mostrar el menú.
        public event EventHandler? RegresarSolicitado;

        public SeleccionEmpresacs()
        {
            InitializeComponent();
            conexion = null;
            usuarioActual = null;
            cumplimientoService = null!;
            perfilService = null!;
        }

        public SeleccionEmpresacs(ConexionInfo conexionInfo, Usuario usuario) : this()
        {
            conexion = conexionInfo;
            usuarioActual = usuario;
            cumplimientoService = new CumplimientoAnexosService(conexionInfo);
            perfilService = new PerfilUsuarioService(); // fija a RetornoMaster

            label1.Text = "Usuario";
            label2.Text = "Razón Social";
            label3.Text = "Empresa";
            label4.Text = "Selecciona la razón social y la empresa que deseas agregar al preview";

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                MostrarUsuarioActual();
                CargarRazonesSociales();
                ConfigurarGridPreview();
            }
        }

        private void MostrarUsuarioActual()
        {
            // El combo queda solo como referencia visual: se fija al usuario
            // logueado y se deshabilita, ya no se elige de una lista.
            cmbUsuario.DataSource = new List<Usuario> { usuarioActual! };
            cmbUsuario.DisplayMember = "NombreCompleto";
            cmbUsuario.ValueMember = "IdUsuario";
            cmbUsuario.SelectedIndex = 0;
            cmbUsuario.Enabled = false;
        }

        private void CargarRazonesSociales()
        {
            try
            {
                var razones = cumplimientoService.ObtenerRazonesSociales();
                cmbRazon.DataSource = razones;
                cmbRazon.DisplayMember = "NombreRazon";
                cmbRazon.ValueMember = "IdRazon";
                cmbRazon.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbRazon_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRazon.SelectedItem is RazonSocial razon)
                CargarEmpresasDeRazon(razon.IdRazon);
            else
                cmbEmpresa.DataSource = null;
        }

        private void CargarEmpresasDeRazon(int idRazon)
        {
            try
            {
                var empresas = perfilService.ObtenerEmpresasDeRazon(idRazon);

                cmbEmpresa.DataSource = empresas;
                cmbEmpresa.DisplayMember = "NombreTabla";
                cmbEmpresa.ValueMember = "IdTabla";
                cmbEmpresa.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                cmbEmpresa.DataSource = null;
                MessageBox.Show($"Error al cargar empresas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarGridPreview()
        {
            dtgPreview.AutoGenerateColumns = false;
            dtgPreview.Columns.Clear();
            dtgPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PerfilPreviewItem.NombreRazon),
                HeaderText = "Razón Social",
                Width = 280,
                ReadOnly = true
            });
            dtgPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PerfilPreviewItem.NombreEmpresa),
                HeaderText = "Empresa",
                Width = 220,
                ReadOnly = true
            });

            dtgPreview.DataSource = filasPreview;
            dtgPreview.AllowUserToAddRows = false;
            dtgPreview.AllowUserToDeleteRows = false;
            dtgPreview.ReadOnly = true;
            dtgPreview.MultiSelect = false;
            dtgPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void btnFila_Click(object? sender, EventArgs e)
        {
            if (cmbRazon.SelectedItem is not RazonSocial razon)
            {
                MessageBox.Show("Seleccione una razón social.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbEmpresa.SelectedItem is not EmpresaRazon empresa)
            {
                MessageBox.Show("Seleccione una empresa.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool yaExiste = filasPreview.Any(f => f.IdRazon == razon.IdRazon && f.IdEmpresa == empresa.IdTabla);
            if (yaExiste)
            {
                MessageBox.Show("Esa combinación de razón social y empresa ya está en la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            filasPreview.Add(new PerfilPreviewItem
            {
                IdRazon = razon.IdRazon,
                NombreRazon = razon.NombreRazon,
                IdEmpresa = empresa.IdTabla,
                NombreEmpresa = empresa.NombreTabla
            });
        }

        // Permite quitar una fila del preview seleccionándola y presionando Supr.
        // Conectar el evento KeyDown de dtgPreview a este método desde el diseñador.
        private void dtgPreview_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || dtgPreview.CurrentRow == null)
                return;

            if (dtgPreview.CurrentRow.DataBoundItem is PerfilPreviewItem item)
            {
                filasPreview.Remove(item);
            }
        }

        private void btnConfirmar_Click(object? sender, EventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("No hay un usuario activo en la sesión.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (filasPreview.Count == 0)
            {
                MessageBox.Show("Agregue al menos una fila antes de confirmar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var grupos = filasPreview.GroupBy(f => f.IdRazon);

                foreach (var grupo in grupos)
                {
                    var idsEmpresas = grupo.Select(g => g.IdEmpresa).ToList();
                    perfilService.GuardarPerfilRazon(usuarioActual.IdUsuario, grupo.Key, idsEmpresas);
                }

                MessageBox.Show("Perfil guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                filasPreview.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar perfil: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            RegresarSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private sealed class PerfilPreviewItem
        {
            public int IdRazon { get; set; }
            public string NombreRazon { get; set; } = string.Empty;
            public int IdEmpresa { get; set; }
            public string NombreEmpresa { get; set; } = string.Empty;
        }
    }
}