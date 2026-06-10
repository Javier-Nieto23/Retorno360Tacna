using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using Retorno360Tacna.SERVICES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportesInventario : Form
    {
        private bool _inicializandoCombo = false;
    
        private readonly ConexionInfo conexionActual;
        private CatalogoPartesService catalogoService;
        // Eliminada la referencia a la carpeta local
        private CloudflareR2Service cloudflareService = new CloudflareR2Service("pdf-storage"); // Reemplaza por tu bucket real

        public FrmReportesInventario(ConexionInfo conexion)
        {
            InitializeComponent();
            conexionActual = conexion;
            catalogoService = new CatalogoPartesService(conexion);
            DataGridViewManualCopyHelper.ConfigurarControles(this);
        }

        private void InicializarImageList()
        {
            imageListCarpetas.Images.Clear();
            imageListCarpetas.ImageSize = new System.Drawing.Size(32, 32); // Forzar tamaño adecuado
            try
            {
                string folderIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "folder.png");
                if (File.Exists(folderIconPath))
                {
                    using (var bmp = new System.Drawing.Bitmap(folderIconPath))
                    {
                        var resized = new System.Drawing.Bitmap(bmp, imageListCarpetas.ImageSize);
                        imageListCarpetas.Images.Add("folder", resized);
                    }
                }
                else
                {
                    MessageBox.Show($"No se encontró el icono personalizado en: {folderIconPath}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    imageListCarpetas.Images.Add("folder", SystemIcons.WinLogo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el icono personalizado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                imageListCarpetas.Images.Add("folder", SystemIcons.WinLogo);
            }
            // Icono de archivo (puedes personalizarlo)
            imageListCarpetas.Images.Add("file", SystemIcons.Application);
        }

        private async void FrmReportesInventario_Load(object sender, EventArgs e)
        {
            CargarRazonesSociales();
            InicializarImageList();
            // Usar el mismo método del botón Actualizar para mostrar las carpetas correctamente
            btnActualizar_Click(btnActualizar, EventArgs.Empty);
        }


        // El método de verificación de carpeta local ya no es necesario

        private void CargarRazonesSociales()
        {
            try
            {
                var razones = catalogoService.ObtenerRazonesSociales();

                cboRazonSocial.DataSource = razones;
                cboRazonSocial.DisplayMember = "NombreRazon";
                cboRazonSocial.ValueMember = "IdRazon";

                if (razones.Any())
                {
                    cboRazonSocial.SelectedIndex = -1;
                }
                _inicializandoCombo = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar razones sociales: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                CargarCarpetasPorRazonSocial(razonSeleccionada.NombreRazon);
            }
            else if (cboRazonSocial.SelectedIndex == -1)
            {
                CargarTodasLasCarpetas();
            }
        }

        private string carpetaActual = null;

        private async void CargarTodasLasCarpetas()
        {
            carpetaActual = null;
            await CargarCarpetasCloudflareAsync();
        }

        private async void CargarSubcarpetas(string prefix)
        {
            carpetaActual = prefix;
            lvCarpetas.Items.Clear();
            var volverItem = new ListViewItem("⬅ Volver", "folder") { Tag = "__volver__" };
            lvCarpetas.Items.Add(volverItem);
            var subcarpetas = await cloudflareService.ListFoldersAsync(prefix + "/");
            if (subcarpetas.Any())
            {
                foreach (var subcarpeta in subcarpetas)
                {
                    var item = new ListViewItem(Path.GetFileName(subcarpeta), "folder")
                    {
                        Tag = subcarpeta,
                        ToolTipText = subcarpeta
                    };
                    lvCarpetas.Items.Add(item);
                }
                lblTotalCarpetas.Text = $"Total de subcarpetas encontradas: {subcarpetas.Count}";
            }
            else
            {
                lvCarpetas.Items.Add(new ListViewItem("No hay subcarpetas en esta carpeta."));
                lblTotalCarpetas.Text = "Total de subcarpetas encontradas: 0";
            }
        }

        private async void CargarCarpetasPorRazonSocial(string nombreRazon)
        {





            await CargarCarpetasCloudflareAsync();
        }

        private double CalcularSimilitud(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            str1 = str1.ToUpper().Replace(" ", "");
            str2 = str2.ToUpper().Replace(" ", "");

            int coincidencias = 0;
            int minLength = Math.Min(str1.Length, str2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (str1[i] == str2[i])
                    coincidencias++;
            }

            double similitud = (double)coincidencias / Math.Max(str1.Length, str2.Length);
            return similitud;
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (cboRazonSocial.SelectedItem is RazonSocial razonSeleccionada)
            {
                CargarCarpetasPorRazonSocial(razonSeleccionada.NombreRazon);
            }
            else
            {
                CargarTodasLasCarpetas();
            }
        }

        private void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            cboRazonSocial.SelectedIndex = -1;
            CargarTodasLasCarpetas();
        }

        private void btnAbrirCarpeta_Click(object sender, EventArgs e)
        {
            if (lvCarpetas.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione una carpeta de la lista.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rutaCompleta = lvCarpetas.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(rutaCompleta) || !Directory.Exists(rutaCompleta))
            {
                MessageBox.Show($"No se encontró la carpeta:\n{rutaCompleta}",
                    "Carpeta no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Process.Start("explorer.exe", rutaCompleta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la carpeta: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lvCarpetas_DoubleClick(object sender, EventArgs e)
        {
            if (lvCarpetas.SelectedItems.Count == 0)
                return;
            var item = lvCarpetas.SelectedItems[0];
            if (item.ImageKey == "folder" && item.Tag is string folderKey)
            {
                // Al hacer doble clic en una carpeta, mostrar sus archivos
                _ = CargarCarpetasCloudflareAsync(folderKey);
            }
            else if (item.ImageKey == "file")
            {
                // Al hacer doble clic en un archivo, descargarlo
                _ = DescargarArchivoSeleccionadoAsync();
            }
        }


        // Permitir descarga con botón derecho
        private async void lvCarpetas_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var item = lvCarpetas.GetItemAt(e.X, e.Y);
                if (item != null && item.ImageKey == "file")
                {
                    var menu = new ContextMenuStrip();
                    var descargar = new ToolStripMenuItem("Descargar archivo");
                    descargar.Click += async (s, ev) => await DescargarArchivoSeleccionadoAsync();
                    menu.Items.Add(descargar);
                    menu.Show(lvCarpetas, e.Location);
                }
            }
        }
        
    }
}
