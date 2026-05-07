using System.Diagnostics;
using System.IO.Compression;
using Octokit;

namespace Retorno360Actualizador
{
    public partial class FrmActualizador : Form
    {
        private readonly string owner = "Javier-Nieto23";
        private readonly string repo = "Retorno360Tacna";
        private string directorioInstalacion = string.Empty;

        public FrmActualizador()
        {
            InitializeComponent();
            directorioInstalacion = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        }

        private async void FrmActualizador_Load(object sender, EventArgs e)
        {
            lblEstado.Text = "Iniciando actualización...";
            progressBar.Value = 0;
            btnFinalizar.Enabled = false;
            btnFinalizar.Visible = false;

            try
            {
                await ActualizarAplicacion();
            }
            catch (Exception ex)
            {
                MostrarError($"Error durante la actualización: {ex.Message}");
            }
        }

        private async Task ActualizarAplicacion()
        {
            try
            {
                lblEstado.Text = "Conectando con GitHub...";
                progressBar.Value = 10;
                await Task.Delay(500);

                var client = new GitHubClient(new ProductHeaderValue("Retorno360Actualizador"));

                lblEstado.Text = "Buscando última versión...";
                progressBar.Value = 20;

                // Obtener el último release
                var releases = await client.Repository.Release.GetAll(owner, repo);

                if (releases.Count == 0)
                {
                    MostrarError("No se encontraron versiones disponibles.");
                    return;
                }

                var latestRelease = releases[0];
                lblEstado.Text = $"Encontrada versión: {latestRelease.TagName}";
                progressBar.Value = 30;
                await Task.Delay(500);

                // Buscar el asset ZIP
                var zipAsset = latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                if (zipAsset == null)
                {
                    MostrarError("No se encontró archivo ZIP en el release.");
                    return;
                }

                lblEstado.Text = "Descargando actualización...";
                progressBar.Value = 40;

                // Descargar el ZIP
                string tempZipPath = Path.Combine(Path.GetTempPath(), "Retorno360_Update.zip");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Retorno360Actualizador");

                    var response = await httpClient.GetAsync(zipAsset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var downloadedBytes = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempZipPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                var progressPercentage = (int)((downloadedBytes * 30.0 / totalBytes) + 40);
                                progressBar.Value = Math.Min(progressPercentage, 70);
                                lblEstado.Text = $"Descargando: {downloadedBytes / 1024 / 1024:F2} MB / {totalBytes / 1024 / 1024:F2} MB";
                            }
                        }
                    }
                }

                lblEstado.Text = "Extrayendo archivos...";
                progressBar.Value = 75;
                await Task.Delay(500);

                // Crear directorio temporal para extracción
                string tempExtractPath = Path.Combine(Path.GetTempPath(), "Retorno360_Extract");
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
                Directory.CreateDirectory(tempExtractPath);

                // Extraer el ZIP
                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);
                progressBar.Value = 80;

                lblEstado.Text = "Instalando archivos...";

                // Copiar archivos al directorio de instalación
                await CopiarArchivos(tempExtractPath, directorioInstalacion);
                progressBar.Value = 95;

                // Limpiar archivos temporales
                lblEstado.Text = "Limpiando archivos temporales...";
                File.Delete(tempZipPath);
                Directory.Delete(tempExtractPath, true);

                progressBar.Value = 100;
                lblEstado.Text = "¡Actualización completada exitosamente!";
                lblEstado.ForeColor = Color.Green;

                btnFinalizar.Visible = true;
                btnFinalizar.Enabled = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el proceso de actualización: {ex.Message}", ex);
            }
        }

        private async Task CopiarArchivos(string origen, string destino)
        {
            var archivos = Directory.GetFiles(origen, "*.*", SearchOption.AllDirectories);
            int archivosProcesados = 0;

            foreach (var archivo in archivos)
            {
                var archivoRelativo = Path.GetRelativePath(origen, archivo);
                var archivoDestino = Path.Combine(destino, archivoRelativo);

                // Crear directorio si no existe
                var directorioDestino = Path.GetDirectoryName(archivoDestino);
                if (!string.IsNullOrEmpty(directorioDestino) && !Directory.Exists(directorioDestino))
                {
                    Directory.CreateDirectory(directorioDestino);
                }

                // Saltar el propio actualizador
                if (Path.GetFileName(archivo).Equals("Retorno360Actualizador.exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    // Si el archivo está en uso, intentar con un delay
                    int intentos = 0;
                    bool copiado = false;

                    while (!copiado && intentos < 3)
                    {
                        try
                        {
                            File.Copy(archivo, archivoDestino, true);
                            copiado = true;
                        }
                        catch (IOException)
                        {
                            intentos++;
                            await Task.Delay(1000);
                        }
                    }

                    if (!copiado)
                    {
                        lblEstado.Text = $"Advertencia: No se pudo copiar {Path.GetFileName(archivo)}";
                    }
                }
                catch
                {
                    // Continuar con el siguiente archivo si hay error
                }

                archivosProcesados++;
                var progreso = (int)((archivosProcesados * 15.0 / archivos.Length) + 80);
                progressBar.Value = Math.Min(progreso, 95);
                lblEstado.Text = $"Instalando: {archivosProcesados}/{archivos.Length} archivos";

                await Task.Delay(10); // Pequeño delay para actualizar UI
            }
        }

        private void MostrarError(string mensaje)
        {
            lblEstado.Text = mensaje;
            lblEstado.ForeColor = Color.Red;
            progressBar.Value = 0;
            btnFinalizar.Visible = true;
            btnFinalizar.Enabled = true;
            btnFinalizar.Text = "Cerrar";
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
