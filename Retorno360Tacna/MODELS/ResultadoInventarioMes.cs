using ClosedXML.Excel;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Retorno360Tacna.FORMS
{
    public enum TipoInventario
    {
        MateriaPrima,
        ActivoFijo
    }

    public enum TipoOperacion
    {
        SumarColumna,
        MultiplicarColumnas
    }

    public sealed class ResultadoInventarioMes
    {
        public int NumeroMes { get; set; }
        public string Mes { get; set; } = string.Empty;
        public TipoInventario TipoInventario { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public string NombreArchivo => string.IsNullOrEmpty(RutaArchivo) ? string.Empty : Path.GetFileName(RutaArchivo);
        public string Hoja { get; set; } = string.Empty;

        public TipoOperacion Operacion { get; set; }
        public string CampoTotal { get; set; } = string.Empty;
        public string CampoA { get; set; } = string.Empty;
        public string CampoB { get; set; } = string.Empty;

        public decimal Total { get; private set; }
        public int FilasProcesadas { get; private set; }
        public string? Error { get; private set; }
        public bool TieneError => !string.IsNullOrEmpty(Error);

        public ResultadoInventarioMes() { }

        public ResultadoInventarioMes(
            int numeroMes,
            string mes,
            TipoInventario tipoInventario,
            string rutaArchivo,
            string hoja,
            TipoOperacion operacion,
            string campoTotal = "",
            string campoA = "",
            string campoB = "")
        {
            NumeroMes = numeroMes;
            Mes = mes;
            TipoInventario = tipoInventario;
            RutaArchivo = rutaArchivo;
            Hoja = hoja;
            Operacion = operacion;
            CampoTotal = campoTotal;
            CampoA = campoA;
            CampoB = campoB;
        }

        public bool Calcular()
        {
            Error = null;
            Total = 0m;
            FilasProcesadas = 0;

            try
            {
                if (string.IsNullOrEmpty(RutaArchivo) || !File.Exists(RutaArchivo))
                    throw new FileNotFoundException("El archivo no existe o la ruta es invalida.", RutaArchivo);

                if (string.IsNullOrEmpty(Hoja))
                    throw new InvalidOperationException("Debe indicar la hoja a analizar.");

                using XLWorkbook workbook = new XLWorkbook(RutaArchivo);
                IXLWorksheet? hoja = workbook.Worksheets
                    .FirstOrDefault(ws => string.Equals(ws.Name, Hoja, StringComparison.OrdinalIgnoreCase));

                if (hoja == null || hoja.IsEmpty())
                    throw new InvalidOperationException($"La hoja '{Hoja}' no existe o esta vacia.");

                Total = Operacion switch
                {
                    TipoOperacion.SumarColumna => SumarColumna(hoja, CampoTotal),
                    TipoOperacion.MultiplicarColumnas => MultiplicarColumnas(hoja, CampoA, CampoB),
                    _ => throw new InvalidOperationException("Tipo de operacion no soportado.")
                };

                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Total = 0m;
                return false;
            }
        }

        private decimal SumarColumna(IXLWorksheet hoja, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(nombreCampo))
                throw new InvalidOperationException("Debe indicar el campo a sumar.");

            var (filaEncabezado, columna) = UbicarCampo(hoja, nombreCampo);

            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? filaEncabezado;

            decimal suma = 0m;
            for (int fila = filaEncabezado + 1; fila <= ultimaFila; fila++)
            {
                if (TryLeerNumero(hoja.Cell(fila, columna), out decimal valor))
                {
                    suma += valor;
                    FilasProcesadas++;
                }
            }

            return suma;
        }

        private decimal MultiplicarColumnas(IXLWorksheet hoja, string nombreCampoA, string nombreCampoB)
        {
            if (string.IsNullOrWhiteSpace(nombreCampoA) || string.IsNullOrWhiteSpace(nombreCampoB))
                throw new InvalidOperationException("Debe indicar ambos campos a multiplicar.");

            var (filaEncabezadoA, columnaA) = UbicarCampo(hoja, nombreCampoA);
            var (_, columnaB) = UbicarCampo(hoja, nombreCampoB);

            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? filaEncabezadoA;

            decimal total = 0m;
            for (int fila = filaEncabezadoA + 1; fila <= ultimaFila; fila++)
            {
                bool tieneA = TryLeerNumero(hoja.Cell(fila, columnaA), out decimal valorA);
                bool tieneB = TryLeerNumero(hoja.Cell(fila, columnaB), out decimal valorB);

                if (tieneA && tieneB)
                {
                    total += valorA * valorB;
                    FilasProcesadas++;
                }
            }

            return total;
        }

        private static (int fila, int columna) UbicarCampo(IXLWorksheet hoja, string nombreCampo)
        {
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
            int filaMaxima = Math.Min(ultimaFila, 20);

            for (int fila = 1; fila <= filaMaxima; fila++)
            {
                IXLRow row = hoja.Row(fila);
                var celda = row.CellsUsed()
                    .FirstOrDefault(c => string.Equals(c.GetString().Trim(), nombreCampo.Trim(), StringComparison.OrdinalIgnoreCase));

                if (celda != null)
                    return (fila, celda.Address.ColumnNumber);
            }

            throw new InvalidOperationException($"No se encontro el campo '{nombreCampo}' en la hoja '{hoja.Name}'.");
        }

        private static bool TryLeerNumero(IXLCell celda, out decimal valor)
        {
            valor = 0m;

            if (celda == null || celda.IsEmpty())
                return false;

            if (celda.DataType == XLDataType.Number)
            {
                valor = (decimal)celda.GetDouble();
                return true;
            }

            string texto = celda.GetString().Trim();
            return decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
        }
    }
}
