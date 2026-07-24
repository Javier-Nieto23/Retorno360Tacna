using Amazon.S3.Model;
using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Retorno360Tacna.SERVICES
{
    // variables unicas del sistema de contabilidad
    public sealed class R2FolderOption
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public sealed class ContabilidadResultadoFila
    {
        public int Año { get; set; }
        public int MesOrden { get; set; }
        public string Mes { get; set; } = string.Empty;
        public string Archivo { get; set; } = string.Empty;
        public string ColumnaAnalizada { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public bool NoDisponible { get; set; }
    }

    public sealed class ContabilidadProcesoResultado
    {
        public List<ContabilidadResultadoFila> Registros { get; set; } = new();
        public int ArchivosAnalizados { get; set; }
        public int ArchivosProcesados { get; set; }
        public int ArchivosOmitidos { get; set; }

        public int MesesFaltantes {  get; set; }
    }


  
    public sealed class ContabilidadR2Service
    {
        private readonly CloudflareR2Service r2Service;
        private static readonly CultureInfo CulturaEs = CultureInfo.GetCultureInfo("es-ES");
        private static readonly Dictionary<string, int> Meses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["enero"] = 1,
            ["ene"] = 1,
            ["january"] = 1,
            ["jan"] = 1,
            ["febrero"] = 2,
            ["feb"] = 2,
            ["february"] = 2,
            ["marzo"] = 3,
            ["mar"] = 3,
            ["march"] = 3,
            ["abril"] = 4,
            ["abr"] = 4,
            ["april"] = 4,
            ["mayo"] = 5,
            ["may"] = 5,
            ["junio"] = 6,
            ["jun"] = 6,
            ["june"] = 6,
            ["julio"] = 7,
            ["jul"] = 7,
            ["july"] = 7,
            ["agosto"] = 8,
            ["ago"] = 8,
            ["august"] = 8,
            ["aug"] = 8,
            ["septiembre"] = 9,
            ["sep"] = 9,
            ["sept"] = 9,
            ["september"] = 9,
            ["octubre"] = 10,
            ["oct"] = 10,
            ["october"] = 10,
            ["noviembre"] = 11,
            ["nov"] = 11,
            ["november"] = 11,
            ["diciembre"] = 12,
            ["dic"] = 12,
            ["dec"] = 12,
            ["december"] = 12
        };

        public ContabilidadR2Service(string bucketName)
        {
            r2Service = new CloudflareR2Service(bucketName);
        }

        public Task<List<R2FolderOption>> ObtenerRazonesSocialesAsync()
        {
            return ObtenerSubcarpetasAsync(string.Empty);
        }

        public Task<List<R2FolderOption>> ObtenerEmpresasAsync(string razonSocialPrefix)
        {
            return ObtenerSubcarpetasAsync(razonSocialPrefix);
        }

        public async Task<List<R2FolderOption>> ObtenerAniosAsync(string empresaPrefix)
        {
            string prefix = NormalizarPrefix(empresaPrefix);
            List<CloudflareR2Service.R2FileInfo> archivos = await r2Service.ListFileDetailsAsync(prefix);

            return archivos
                .Where(a => a.Key.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                .Where(a => !Path.GetFileName(a.Key).StartsWith("~$", StringComparison.OrdinalIgnoreCase))
                .Select(a => InferirAnio(a.Key, string.Empty, a.LastModifiedUtc))
                .Where(anio => anio > 0)
                .Distinct()
                .OrderByDescending(anio => anio)
                .Select(anio => new R2FolderOption
                {
                    DisplayName = anio.ToString(CultureInfo.InvariantCulture),
                    Prefix = prefix
                })
                .ToList();
        }

        public async Task<ContabilidadProcesoResultado> ProcesarArchivosAsync(string empresaPrefix, string anioSeleccionado, string columnaObjetivo)
        {
            var resultado = new ContabilidadProcesoResultado();
            string prefix = NormalizarPrefix(empresaPrefix);
            string directorioTemporal = Path.Combine(Path.GetTempPath(), "Retorno360Tacna", "Contabilidad", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directorioTemporal);

            try
            {
                List<CloudflareR2Service.R2FileInfo> archivos = await r2Service.ListFileDetailsAsync(prefix);
                List<CloudflareR2Service.R2FileInfo> archivosExcel = archivos
                    .Where(a => a.Key.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    .Where(a => !Path.GetFileName(a.Key).StartsWith("~$", StringComparison.OrdinalIgnoreCase))
                    .Where(a => InferirAnio(a.Key, anioSeleccionado, a.LastModifiedUtc).ToString(CultureInfo.InvariantCulture) == anioSeleccionado)
                    .OrderBy(a => a.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                resultado.ArchivosAnalizados = archivosExcel.Count;

                foreach (CloudflareR2Service.R2FileInfo archivo in archivosExcel)
                {
                    string extension = Path.GetExtension(archivo.Key);
                    string archivoTemporal = Path.Combine(directorioTemporal, $"{Guid.NewGuid():N}{extension}");

                    try
                    {
                        await r2Service.DownloadFileAsync(archivo.Key, archivoTemporal);

                        if (!TryProcesarArchivo(archivoTemporal, archivo, anioSeleccionado, columnaObjetivo, out ContabilidadResultadoFila? fila))
                        {
                            resultado.ArchivosOmitidos++;
                            continue;
                        }

                        resultado.Registros.Add(fila);
                        resultado.ArchivosProcesados++;
                    }
                    catch
                    {
                        resultado.ArchivosOmitidos++;
                    }
                    finally
                    {
                        if (File.Exists(archivoTemporal))
                        {
                            File.Delete(archivoTemporal);
                        }
                    }
                }
            }
            finally
            {
                if (Directory.Exists(directorioTemporal))
                {
                    Directory.Delete(directorioTemporal, true);
                }
            }

            if (int.TryParse(anioSeleccionado, out int añoInt))
            {
                HashSet<int> mesesExistentes = resultado.Registros
                    .Select(r => r.MesOrden)
                    .ToHashSet();

                resultado.MesesFaltantes = 12 - mesesExistentes.Count; 

                for (int mes = 1; mes <= 12; mes++)
                {
                    if (mesesExistentes.Contains(mes))
                        continue;

                    resultado.Registros.Add(new ContabilidadResultadoFila
                    {
                        Año = añoInt,
                        MesOrden = mes,
                        Mes = $"{mes:00} - {CulturaEs.DateTimeFormat.GetMonthName(mes)}",
                        Archivo = "(No subido)",
                        ColumnaAnalizada = columnaObjetivo,
                        Total = 0m,
                        NoDisponible = true
                    });
                }
            }

            resultado.Registros = resultado.Registros
                .OrderBy(r => r.Año)
                .ThenBy(r => r.MesOrden)
                .ThenBy(r => r.Archivo, StringComparer.OrdinalIgnoreCase)
                .ToList();


            return resultado;
        }

        public void ExportarResultadosExcel(IEnumerable<ContabilidadResultadoFila> resultados, string rutaArchivo)
        {
            using XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet hoja = workbook.Worksheets.Add("Contabilidad");

            hoja.Cell(1, 1).Value = "Año";
            hoja.Cell(1, 2).Value = "Mes";
            hoja.Cell(1, 3).Value = "Archivo";
            hoja.Cell(1, 4).Value = "ColumnaAnalizada";
            hoja.Cell(1, 5).Value = "Total";

            int fila = 2;
            foreach (ContabilidadResultadoFila resultado in resultados)
            {
                hoja.Cell(fila, 1).Value = resultado.Año;
                hoja.Cell(fila, 2).Value = resultado.Mes;
                hoja.Cell(fila, 3).Value = resultado.Archivo;
                hoja.Cell(fila, 4).Value = resultado.ColumnaAnalizada;
                hoja.Cell(fila, 5).Value = resultado.Total;
                hoja.Cell(fila, 5).Style.NumberFormat.Format = "#,##0.00";


                if (resultado.NoDisponible)
                {
                    IXLRange filaRango = hoja.Range(fila ,1, fila, 5 );// Rango de la fila actual
                    filaRango.Style.Font.FontColor = XLColor.Red;
                    filaRango.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 235, 235);
                }
                ++fila;// segundo incremento de fila para la siguiente iteración
            }

            IXLRange rango = hoja.Range(1, 1, Math.Max(1, fila - 1), 5);
            rango.CreateTable();
            hoja.Row(1).Style.Font.Bold = true;
            hoja.Columns().AdjustToContents();
            hoja.SheetView.FreezeRows(1);
            workbook.SaveAs(rutaArchivo);
        }

        private async Task<List<R2FolderOption>> ObtenerSubcarpetasAsync(string prefix)
        {
            List<string> carpetas = await r2Service.ListFoldersAsync(NormalizarPrefix(prefix));
            return carpetas
                .Select(carpeta => new R2FolderOption
                {
                    Prefix = carpeta,
                    DisplayName = ObtenerUltimoSegmento(carpeta)
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.DisplayName))
                .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizarPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return string.Empty;

            return prefix.Trim().TrimEnd('/') + "/";
        }

        private static string ObtenerUltimoSegmento(string ruta)
        {
            return ruta.TrimEnd('/').Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        }

        private static bool TryProcesarArchivo(string rutaArchivo, CloudflareR2Service.R2FileInfo archivo, string anioSeleccionado, string columnaObjetivo, out ContabilidadResultadoFila? fila)
        {
            fila = null;

            using XLWorkbook workbook = new XLWorkbook(rutaArchivo);
            IXLWorksheet? hoja = workbook.Worksheets.FirstOrDefault(ws => !ws.IsEmpty());
            if (hoja == null)
                return false;

            if (!TryEncontrarColumna(hoja, columnaObjetivo, out int filaEncabezado, out int indiceColumna))
                return false;

            decimal total = 0m;
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? filaEncabezado;

            for (int filaActual = filaEncabezado + 1; filaActual <= ultimaFila; filaActual++)
            {
                IXLCell celda = hoja.Cell(filaActual, indiceColumna);
                if (TryObtenerDecimal(celda, out decimal valor))
                {
                    total += valor;
                }
            }

            int año = InferirAnio(archivo.Key, anioSeleccionado, archivo.LastModifiedUtc);
            int mes = InferirMes(archivo.Key, archivo.LastModifiedUtc, anioSeleccionado);
            string nombreMes = $"{mes:00} - {CulturaEs.DateTimeFormat.GetMonthName(mes)}";

            fila = new ContabilidadResultadoFila
            {
                Año = año,
                MesOrden = mes,
                Mes = nombreMes,
                Archivo = Path.GetFileName(archivo.Key),
                ColumnaAnalizada = columnaObjetivo,
                Total = total
            };

            return true;
        }

        private static bool TryEncontrarColumna(IXLWorksheet hoja, string columnaObjetivo, out int filaEncabezado, out int indiceColumna)
        {
            filaEncabezado = 0;
            indiceColumna = 0;
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
            int filaMaxima = Math.Min(ultimaFila, 20);

            for (int fila = 1; fila <= filaMaxima; fila++)
            {
                IXLRow row = hoja.Row(fila);
                foreach (IXLCell celda in row.CellsUsed())
                {
                    string encabezado = celda.GetString().Trim();
                    if (string.Equals(encabezado, columnaObjetivo.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        filaEncabezado = fila;
                        indiceColumna = celda.Address.ColumnNumber;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryObtenerDecimal(IXLCell celda, out decimal valor)
        {
            valor = 0m;

            if (celda.IsEmpty())
                return false;

            if (celda.DataType == XLDataType.Number)
            {
                valor = Convert.ToDecimal(celda.GetDouble(), CultureInfo.InvariantCulture);
                return true;
            }

            string texto = celda.GetFormattedString().Trim();
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            texto = texto.Replace("$", string.Empty)
                         .Replace("€", string.Empty)
                         .Replace("£", string.Empty)
                         .Replace(" ", string.Empty);

            bool negativoPorParentesis = texto.StartsWith("(") && texto.EndsWith(")");
            texto = texto.Trim('(', ')');

            NumberStyles estilos = NumberStyles.AllowDecimalPoint |
                                   NumberStyles.AllowThousands |
                                   NumberStyles.AllowLeadingSign;

            if (decimal.TryParse(texto, estilos, CulturaEs, out valor) ||
                decimal.TryParse(texto, estilos, CultureInfo.InvariantCulture, out valor) ||
                decimal.TryParse(texto, estilos, CultureInfo.CurrentCulture, out valor))
            {
                if (negativoPorParentesis)
                {
                    valor *= -1;
                }

                return true;
            }

            return false;
        }

        private static int InferirAnio(string key, string anioSeleccionado, DateTime fechaModificacionUtc)
        {
            string nombreArchivo = Path.GetFileNameWithoutExtension(key);
            Match match = Regex.Match(nombreArchivo, "^(\\d{4})-(\\d{2})");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int anioArchivo))
                return anioArchivo;

            if (int.TryParse(anioSeleccionado, out int añoSeleccionadoParseado))
                return añoSeleccionadoParseado;

            return fechaModificacionUtc.Year;
        }

        private static int InferirMes(string key, DateTime fechaModificacionUtc, string anioSeleccionado)
        {
            string nombreArchivo = Path.GetFileNameWithoutExtension(key);
            Match matchInicio = Regex.Match(nombreArchivo, "^(\\d{4})-(\\d{2})");
            if (matchInicio.Success && int.TryParse(matchInicio.Groups[2].Value, out int mesInicio) && mesInicio >= 1 && mesInicio <= 12)
                return mesInicio;

            string rutaSinExtension = nombreArchivo;
            IEnumerable<string> tokens = key
                .Replace('\\', '/')
                .Split(new[] { '/', '_', '-', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Concat(rutaSinExtension.Split(new[] { '_', '-', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries));

            foreach (string tokenOriginal in tokens.Reverse())
            {
                string token = tokenOriginal.Trim().ToLowerInvariant();
                if (token == anioSeleccionado.Trim().ToLowerInvariant())
                    continue;

                if (Meses.TryGetValue(token, out int mesPorNombre))
                    return mesPorNombre;

                if (Regex.IsMatch(token, "^\\d{1,2}$") && int.TryParse(token, out int mesNumerico) && mesNumerico >= 1 && mesNumerico <= 12)
                    return mesNumerico;
            }

            return fechaModificacionUtc.Month is >= 1 and <= 12 ? fechaModificacionUtc.Month : 1;
        }
    }
}
