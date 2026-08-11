using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace Retorno360Tacna.MODELS
{
    //<summary>
    //Encapsula el analisis de un archivo Excel de layout:
    //sus hojas y campos (encabezados) de cada hoja.
    //</summary>
        public sealed class ExcelLayoutModel
        {
            public string RutaArchivo { get; set; } = string.Empty;
            public List<string> Hojas { get; set; } = new();
            public List<string> Campos { get; set; } = new();


    

            public ExcelLayoutModel()
            {
            }

            public ExcelLayoutModel(string rutaArchivo)
            {
                CargarArchivo(rutaArchivo);
            }

            //<summary>
            //cargar un nuevo archivo y detecta sus hojas. limpia el estado anterior.
            //</summary>

            public void CargarArchivo(string rutaArchivo)
            {
                if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
                    throw new FileNotFoundException("El archivo no existe o la ruta es inválida.", nameof(rutaArchivo));

                RutaArchivo = rutaArchivo;
                Campos.Clear();

                using XLWorkbook workbook = new XLWorkbook(rutaArchivo);

                Hojas = workbook.Worksheets
                    .Select(ws => ws.Name)
                    .ToList();
            }

        //<summary>
        //analiza una hoja especifica del archivo ya cargado y llena campos. 
        //</summary>
        public List<string> AnalizarHoja(string nombreHoja)
        {
            if (string.IsNullOrEmpty(RutaArchivo))
                throw new InvalidOperationException("Debe cargar un archivo antes de analizar una hoja. ");

            using XLWorkbook workbook = new XLWorkbook(RutaArchivo);
            IXLWorksheet? hoja = workbook.Worksheets.FirstOrDefault(ws => string.Equals(ws.Name, nombreHoja, StringComparison.OrdinalIgnoreCase));
            if (hoja == null || hoja.IsEmpty())
            {
                Campos = new List<string>();
                return Campos;
            }
            int filaEncabezado = DetectarFilaEncabezado(hoja);
            if (filaEncabezado == 0)
            {
                Campos = new List<string>();
                return Campos;
            }

            IXLRow row = hoja.Row(filaEncabezado);
            int ultimaColumna = row.LastCellUsed()?.Address.ColumnNumber ?? 0;

            var campos = new List<string>();
            for (int col = 1; col <= ultimaColumna; col++)
            {
                string valor = hoja.Cell(filaEncabezado, col).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(valor))
                {
                    campos.Add(valor);
                }
            }

            Campos = campos;
            return Campos; 

            
        }

        private static int DetectarFilaEncabezado(IXLWorksheet hoja)
        {
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
            int filaMaxima = Math.Min(ultimaFila, 20);

            for (int fila = 1; fila <= filaMaxima; fila++)
            {
                IXLRow row = hoja.Row(fila);
                int celdasConTexto = row.CellsUsed()
                    .Count(c => c.DataType == XLDataType.Text && !string.IsNullOrWhiteSpace(c.GetString()));

                if (celdasConTexto >= 2)
                {
                    return fila;
                }
            }

            return ultimaFila > 0 ? 1 : 0;
        }
    }
}

