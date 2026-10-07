using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmHistorialVerificacion : Form
    {
        private readonly int _idRazon;
        private readonly int _idEmpresa;
        private readonly DataTable? _tabla;

        public FrmHistorialVerificacion(int idRazon, int idEmpresa, DataTable? tabla)
        {
            InitializeComponent();
            _idRazon = idRazon;
            _idEmpresa = idEmpresa;
            _tabla = tabla;

            if (_tabla != null)
            {
                dgvHistorial.DataSource = _tabla;
                dgvHistorial.ReadOnly = false;
                dgvHistorial.AllowUserToAddRows = false;
            }
        }

        private async void btnVerificar_Click(object sender, EventArgs e)
        {
            if (_tabla == null)
            {
                MessageBox.Show("No hay datos para verificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Construir lista de entradas (parte, cantidad, unidad)
            var entradas = new List<(string Parte, decimal Cantidad, string Unidad)>();

            foreach (DataRow row in _tabla.Rows)
            {
                try
                {
                    string parte = string.Empty;
                    decimal cantidad = 1m;
                    string unidad = string.Empty;

                    // Buscar columnas frecuentes
                    if (_tabla.Columns.Contains("Parte")) parte = Convert.ToString(row["Parte"]) ?? string.Empty;
                    else if (_tabla.Columns.Contains("colParte")) parte = Convert.ToString(row["colParte"]) ?? string.Empty;
                    else if (_tabla.Columns.Count > 0)
                        parte = Convert.ToString(row[0]) ?? string.Empty;

                    if (_tabla.Columns.Contains("Cantidad")) cantidad = Convert.ToDecimal(row["Cantidad"]);
                    else if (_tabla.Columns.Contains("colCantidad")) cantidad = Convert.ToDecimal(row["colCantidad"]);
                    else if (_tabla.Columns.Contains("Cantidad ")) cantidad = Convert.ToDecimal(row["Cantidad "]); // guard

                    if (_tabla.Columns.Contains("UM_Usuario")) unidad = Convert.ToString(row["UM_Usuario"]) ?? string.Empty;
                    else if (_tabla.Columns.Contains("UM Usuario")) unidad = Convert.ToString(row["UM Usuario"]) ?? string.Empty;
                    else if (_tabla.Columns.Contains("colUMUsuario")) unidad = Convert.ToString(row["colUMUsuario"]) ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(parte)) continue;

                    entradas.Add((parte.Trim(), cantidad, unidad.Trim()));
                }
                catch
                {
                    // ignorar filas con error de parseo
                }
            }

            if (entradas.Count == 0)
            {
                MessageBox.Show("No se encontraron filas válidas para verificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var servicio = new VerificacionPartesService();
                var resultado = await servicio.VerificarPartesConCantidadAsync(_idRazon, _idEmpresa, entradas);

                using var frm = new FrmResultadoVerificacion(resultado);
                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
