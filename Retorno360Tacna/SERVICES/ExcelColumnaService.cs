using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ClosedXML.Excel;

namespace Retorno360Tacna.SERVICES
{
    public class ExcelColumnaService
    {
        public List<string> ObtenerHojas(string rutaArchivo)
        {
            try
            {
                using var wb = new XLWorkbook(rutaArchivo);
                return wb.Worksheets.Select(ws => ws.Name).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public List<string> ObtenerColumnas(string rutaArchivo, string nombreHoja)
        {
            try
            {
                using var wb = new XLWorkbook(rutaArchivo);
                var ws = wb.Worksheet(nombreHoja);
                var headerRow = ws.FirstRowUsed();
                if (headerRow == null) return new List<string>();
                return headerRow.CellsUsed().Select(c => c.GetString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public decimal CalcularSumaSimple(string rutaArchivo, string nombreHoja, string nombreColumna)
        {
            try
            {
                using var wb = new XLWorkbook(rutaArchivo);
                var ws = wb.Worksheet(nombreHoja);
                var header = ws.FirstRowUsed();
                if (header == null) return 0;
                var cell = header.CellsUsed().FirstOrDefault(c => string.Equals(c.GetString(), nombreColumna, StringComparison.OrdinalIgnoreCase));
                if (cell == null) return 0;
                int colIndex = cell.Address.ColumnNumber;
                decimal suma = 0;
                foreach (var dataRow in ws.RowsUsed().Skip(1))
                {
                    var valCell = dataRow.Cell(colIndex);
                    if (valCell == null) continue;
                    if (valCell.DataType == XLDataType.Number)
                    {
                        if (double.TryParse(valCell.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dd)) suma += (decimal)dd;
                    }
                    else
                    {
                        if (decimal.TryParse(valCell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) suma += d;
                    }
                }
                return suma;
            }
            catch
            {
                return 0;
            }
        }

        public decimal CalcularSumaProducto(string rutaArchivo, string nombreHoja, string columnaA, string columnaB)
        {
            try
            {
                using var wb = new XLWorkbook(rutaArchivo);
                var ws = wb.Worksheet(nombreHoja);
                var header = ws.FirstRowUsed();
                if (header == null) return 0;
                var cellA = header.CellsUsed().FirstOrDefault(c => string.Equals(c.GetString(), columnaA, StringComparison.OrdinalIgnoreCase));
                var cellB = header.CellsUsed().FirstOrDefault(c => string.Equals(c.GetString(), columnaB, StringComparison.OrdinalIgnoreCase));
                if (cellA == null || cellB == null) return 0;
                int colA = cellA.Address.ColumnNumber;
                int colB = cellB.Address.ColumnNumber;
                decimal suma = 0;
                foreach (var dataRow in ws.RowsUsed().Skip(1))
                {
                    var aCell = dataRow.Cell(colA);
                    var bCell = dataRow.Cell(colB);
                    decimal a = 0, b = 0;
                    if (aCell != null)
                    {
                        if (aCell.DataType == XLDataType.Number)
                        {
                            if (double.TryParse(aCell.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var da)) a = (decimal)da;
                        }
                        else decimal.TryParse(aCell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
                    }
                    if (bCell != null)
                    {
                        if (bCell.DataType == XLDataType.Number)
                        {
                            if (double.TryParse(bCell.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var db)) b = (decimal)db;
                        }
                        else decimal.TryParse(bCell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out b);
                    }
                    suma += a * b;
                }
                return suma;
            }
            catch
            {
                return 0;
            }
        }

        public void ExportarConsolidado(List<MODELS.MesArchivoItem> items, string rutaSalida)
        {
            // Export simple Excel consolidado: Mes | Archivo | Modo | Total
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Consolidado");
            ws.Cell(1, 1).Value = "Mes";
            ws.Cell(1, 2).Value = "Archivo";
            ws.Cell(1, 3).Value = "Modo";
            ws.Cell(1, 4).Value = "Total";
            int row = 2;
            foreach (var it in items)
            {
                ws.Cell(row, 1).Value = it.Mes;
                ws.Cell(row, 2).Value = it.NombreArchivo;
                ws.Cell(row, 3).Value = it.ModoTexto;
                ws.Cell(row, 4).Value = it.Total;
                row++;
            }
            wb.SaveAs(rutaSalida);
        }
    }
}
