using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmReportesInventario : Form
    {
        private async Task CargarCarpetasCloudflareAsync(string prefix = "")
        {
            System.Diagnostics.Debug.WriteLine("[CargarCarpetasCloudflareAsync] Limpiando ListView");
            lvCarpetas.Items.Clear();
            // Si no hay prefijo, solo mostrar la carpeta principal (por ejemplo, "pdfs")
            var carpetas = await cloudflareService.ListFoldersAsync(prefix);
            if (string.IsNullOrEmpty(prefix))
            {
                // Log de todos los valores crudos devueltos por Cloudflare
                System.Diagnostics.Debug.WriteLine("--- Carpetas crudas devueltas por Cloudflare ---");
                foreach (var carpeta in carpetas)
                {
                    System.Diagnostics.Debug.WriteLine($"Crudo: '{carpeta}'");
                }
                System.Diagnostics.Debug.WriteLine("-----------------------------------------------");

                // Filtrar carpetas únicas por nombre
                var nombresUnicos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int countAntes = lvCarpetas.Items.Count;
                System.Diagnostics.Debug.WriteLine($"[CargarCarpetasCloudflareAsync] Antes de agregar: {countAntes} items en ListView");
                foreach (var carpeta in carpetas)
                {
                    var nombre = Path.GetFileName(carpeta.TrimEnd('/', '\\'));
                    System.Diagnostics.Debug.WriteLine($"Carpeta encontrada: '{carpeta}' | Nombre usado: '{nombre}'");
                    if (!string.IsNullOrWhiteSpace(nombre) && !nombresUnicos.Contains(nombre))
                    {
                        nombresUnicos.Add(nombre);
                        var item = new ListViewItem(nombre, "folder")
                        {
                            Tag = carpeta,
                            ToolTipText = carpeta
                        };
                        lvCarpetas.Items.Add(item);
                    }
                }
                int countDespues = lvCarpetas.Items.Count;
                System.Diagnostics.Debug.WriteLine($"[CargarCarpetasCloudflareAsync] Después de agregar: {countDespues} items en ListView");
                lblTotalCarpetas.Text = $"Total de carpetas encontradas: {nombresUnicos.Count}";
            }
            else
            {
                // Si hay prefijo, mostrar archivos dentro de la carpeta
                var files = await cloudflareService.ListFilesAsync(prefix + "/");
                foreach (var file in files)
                {
                    var item = new ListViewItem(Path.GetFileName(file), "file")
                    {
                        Tag = file,
                        ToolTipText = file
                    };
                    lvCarpetas.Items.Add(item);
                }
                lblTotalCarpetas.Text = $"Total de archivos encontrados: {files.Count}";
            }
        }

        // Método para descargar archivo seleccionado
        private async Task DescargarArchivoSeleccionadoAsync()
        {
            if (lvCarpetas.SelectedItems.Count == 0)
                return;
            var item = lvCarpetas.SelectedItems[0];
            if (item.ImageKey == "file" && item.Tag is string fileKey)
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.FileName = Path.GetFileName(fileKey);
                    sfd.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        await cloudflareService.DownloadFileAsync(fileKey, sfd.FileName);
                        MessageBox.Show("Archivo descargado correctamente.", "Descarga", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }
}
