using Retorno360Tacna.FORMS;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static Retorno360Tacna.FORMS.FrmCalculoInventarios;
using static Retorno360Tacna.FORMS.FrmVistaPreviaExcel;

namespace Retorno360Tacna.MODELS
{
    public partial class UcMesInventario : UserControl
    {
        private readonly ExcelLayoutModel _layoutModel = new();
        public ResultadoInventarioMes? Resultado { get; private set; }
        public int NumeroPanel { get; }

        // Empresa activa: la fija FrmCalculoInventarios antes de crear los paneles
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int IdEmpresaActiva { get; set; }

        public event EventHandler? ResultadoActualizado;

        public UcMesInventario(int numeroPanel)
        {
            InitializeComponent();
            NumeroPanel = numeroPanel;
            lblNumero.Text = $"#{numeroPanel}";
            CargarMeses();
            CargarTiposOperacion();
        }

        

        private void CargarMeses()
        {
            cmbMes.DataSource = new List<string>
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };
        }

        private void CargarTiposOperacion()
        {
                cmbTipoOperacion.DataSource = new List<string>
                {
                    "Multiplicar Campo A x Campo B y sumar",
                    "Suma simple de un campo"
                };
        }


        private void btnSeleccionarExcel_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Excel|*.xlsx;*.xls" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            _layoutModel.CargarArchivo(ofd.FileName);
            lblArchivo.Text      = Path.GetFileName(ofd.FileName);
            lblArchivo.ForeColor = Color.Black;

            // Intentar usar plantilla configurada para la empresa activa
            var plantilla = PlantillaInventarioServicio.ObtenerParaEmpresa(IdEmpresaActiva);

            if (plantilla != null && plantilla.EstaConfigurada &&
                plantilla.CamposPlantilla().Any())
            {
                // Obtener columnas del Excel cargado con la hoja de la plantilla
                List<string> columnasExcel;
                try
                {
                    columnasExcel = _layoutModel.AnalizarHoja(plantilla.Hoja);
                }
                catch
                {
                    // Si la hoja no existe en el Excel del usuario, dejar flujo manual
                    columnasExcel = new List<string>();
                }

                if (columnasExcel.Count > 0)
                {
                    using var dlg = new FrmMapeoCamposPlantilla(plantilla, columnasExcel);
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    // Aplicar mapeo: rellenar combos con las columnas mapeadas y calcular
                    AplicarMapeoDePlantilla(plantilla, dlg.Mapeo, columnasExcel);
                    return;
                }
            }

            // Flujo manual: cargar hojas normalmente
            cmbHoja.DataSource = _layoutModel.Hojas;
        }

        private void AplicarMapeoDePlantilla(
            PlantillaInventarioConfig plantilla,
            System.Collections.Generic.Dictionary<string, string> mapeo,
            List<string> columnasExcel)
        {
            // Seleccionar hoja de la plantilla
            cmbHoja.DataSource = _layoutModel.Hojas;
            int idxHoja = _layoutModel.Hojas.IndexOf(plantilla.Hoja);
            if (idxHoja >= 0) cmbHoja.SelectedIndex = idxHoja;

            // Tipo operación
            bool esMultiplicar = plantilla.Operacion == "MultiplicarColumnas";
            cmbTipoOperacion.SelectedIndex = esMultiplicar ? 0 : 1;

            // Rellenar combos con columnas del Excel
            cmbCampoA.DataSource     = new List<string>(columnasExcel);
            cmbCampoB.DataSource     = new List<string>(columnasExcel);
            cmbCampoTotal.DataSource = new List<string>(columnasExcel);

            if (esMultiplicar)
            {
                if (mapeo.TryGetValue(plantilla.CampoA, out string? colA))
                    SeleccionarSiExiste(cmbCampoA, colA);
                if (mapeo.TryGetValue(plantilla.CampoB, out string? colB))
                    SeleccionarSiExiste(cmbCampoB, colB);
            }
            else
            {
                if (mapeo.TryGetValue(plantilla.CampoTotal, out string? colTotal))
                    SeleccionarSiExiste(cmbCampoTotal, colTotal);
            }

            IntentarCalcular();
        }

        private static void SeleccionarSiExiste(ComboBox cmb, string valor)
        {
            int idx = (cmb.DataSource as List<string>)?.IndexOf(valor) ?? -1;
            if (idx >= 0) cmb.SelectedIndex = idx;
        }

        private void cmbHoja_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbHoja.SelectedItem == null) return;

            var campos = _layoutModel.AnalizarHoja(cmbHoja.SelectedItem.ToString()!);
            cmbCampoA.DataSource = new List<string>(campos);
            cmbCampoB.DataSource = new List<string>(campos);
            cmbCampoTotal.DataSource = new List<string>(campos);
        }

        private void cmbTipoOperacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esMultiplicar = cmbTipoOperacion.SelectedIndex == 0;
            cmbCampoA.Visible = esMultiplicar;
            cmbCampoB.Visible = esMultiplicar;
            lblCampoA.Visible = esMultiplicar;
            lblCampoB.Visible = esMultiplicar;
            cmbCampoTotal.Visible = !esMultiplicar;
            lblCampoTotal.Visible = !esMultiplicar;

            IntentarCalcular();
        }

        private void cmbCampoA_SelectedIndexChanged(object sender, EventArgs e) => IntentarCalcular();
        private void cmbCampoB_SelectedIndexChanged(object sender, EventArgs e) => IntentarCalcular();
        private void cmbCampoTotal_SelectedIndexChanged(object sender, EventArgs e) => IntentarCalcular();

        public void IntentarCalcular()
        {
            bool esMultiplicar = cmbTipoOperacion.SelectedIndex == 0;

            bool listoParaCalcular = esMultiplicar
                ? cmbCampoA.SelectedItem != null && cmbCampoB.SelectedItem != null
                : cmbCampoTotal.SelectedItem != null;

            if (string.IsNullOrEmpty(_layoutModel.RutaArchivo) || cmbHoja.SelectedItem == null || !listoParaCalcular)
                return;

            var resultado = new ResultadoInventarioMes(
                numeroMes: cmbMes.SelectedIndex + 1,
                mes: cmbMes.SelectedItem!.ToString()!,
                tipoInventario: TipoInventario.MateriaPrima,
                rutaArchivo: _layoutModel.RutaArchivo,
                hoja: cmbHoja.SelectedItem.ToString()!,
                operacion: esMultiplicar ? TipoOperacion.MultiplicarColumnas : TipoOperacion.SumarColumna,
                campoTotal: cmbCampoTotal.SelectedItem?.ToString() ?? "",
                campoA: cmbCampoA.SelectedItem?.ToString() ?? "",
                campoB: cmbCampoB.SelectedItem?.ToString() ?? ""
            );

            resultado.Calcular();
            Resultado = resultado;

            lblTotal.Text = resultado.TieneError ? "Error" : $"Total: {resultado.Total:N2}";

            ResultadoActualizado?.Invoke(this, EventArgs.Empty);
        }

        private void btnVerExcel_Click(object sender, EventArgs e)
        {
            // Obtenemos la ruta directamente desde _layoutModel
            string ruta = _layoutModel.RutaArchivo;

            // Validar que exista una ruta seleccionada
            if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
            {
                MessageBox.Show("Primero debe seleccionar un archivo Excel válido.",
                                "Archivo no seleccionado",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            string hojaSeleccionada = cmbHoja.SelectedItem?.ToString() ?? string.Empty;

            // Crear y mostrar la ventana emergente de vista previa
            FrmVistaPreviaExcel frmPreview = new FrmVistaPreviaExcel(ruta, hojaSeleccionada);
            frmPreview.Show();
        }


        public UcMesInventario()
        {
            InitializeComponent();
        }
    }
}
