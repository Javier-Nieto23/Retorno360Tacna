using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    public class CumplimientoAnexosService
    {
        private readonly ReporteIGIService reporteService;

        public CumplimientoAnexosService(ConexionInfo conexion)
        {
            reporteService = new ReporteIGIService(conexion);
        }

        public List<RazonSocial> ObtenerRazonesSociales()
        {
            return reporteService.ObtenerRazonesSociales();
        }

        public List<string> ObtenerBasesDatosRazon(int idRazon)
        {
            return reporteService.ObtenerBasesDatosRazon(idRazon);
        }

        public DataTable GenerarPreview(int idRazon, string razonSocial, string? baseSeleccionada, DateTime fechaInicio, DateTime fechaFin)
        {
            var tabla = CrearTablaPreview();
            var bases = !string.IsNullOrWhiteSpace(baseSeleccionada)
                ? new List<string> { baseSeleccionada }
                : ObtenerBasesDatosRazon(idRazon);

            foreach (var mes in ObtenerMesesEnRango(fechaInicio, fechaFin))
            {
                var inicioMes = new DateTime(mes.Year, mes.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                foreach (var baseDb in bases)
                {
                    var conciliacion = reporteService.ObtenerConciliacionIGI(baseDb, inicioMes, finMes);

                    var detalleIGI = conciliacion.DetalleIGI.Rows.Cast<DataRow>().ToList();
                    var detalleIVA = conciliacion.DetalleIVA.Rows.Cast<DataRow>().ToList();

                    int operaciones = detalleIGI
                        .Select(r => r["Pedimento"]?.ToString()?.Trim() ?? string.Empty)
                        .Concat(detalleIVA.Select(r => r["Pedimento"]?.ToString()?.Trim() ?? string.Empty))
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count();

                    decimal igiPagado = detalleIGI
                        .Where(r => string.Equals(r["FormaPago_IGI"]?.ToString()?.Trim(), "0", StringComparison.OrdinalIgnoreCase))
                        .Sum(r => ConvertirDecimal(r, "IGI_Pagado"));

                    decimal igiCalculado = detalleIGI
                        .Sum(r => ConvertirDecimal(r, "IGI_Calculado"));

                    decimal ahorroIgi = detalleIGI
                        .Where(r => string.Equals(r["FormaPago_IGI"]?.ToString()?.Trim(), "5", StringComparison.OrdinalIgnoreCase))
                        .Sum(r => ConvertirDecimal(r, "IGI_Calculado"));

                    decimal pagoIva = detalleIVA
                        .Where(r => string.Equals(r["FormaPago_IVA"]?.ToString()?.Trim(), "0", StringComparison.OrdinalIgnoreCase))
                        .Sum(r => ConvertirDecimal(r, "IVA_Pagado"));

                    decimal ahorroIva = detalleIVA
                        .Where(r => string.Equals(r["FormaPago_IVA"]?.ToString()?.Trim(), "21", StringComparison.OrdinalIgnoreCase))
                        .Sum(r => ConvertirDecimal(r, "IVA_Pagado"));

                    tabla.Rows.Add(
                        razonSocial,
                        baseDb,
                        inicioMes,
                        operaciones,
                        igiPagado,
                        igiCalculado,
                        ahorroIgi,
                        pagoIva,
                        ahorroIva);
                }
            }

            return tabla;
        }

        private static DataTable CrearTablaPreview()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("RAZON_SOCIAL", typeof(string));
            tabla.Columns.Add("PLANTA", typeof(string));
            tabla.Columns.Add("PERIODO", typeof(DateTime));
            tabla.Columns.Add("OPERACIONES", typeof(int));
            tabla.Columns.Add("IGI_PAGADO", typeof(decimal));
            tabla.Columns.Add("IGI_CALCULADO", typeof(decimal));
            tabla.Columns.Add("AHORRO_IGI", typeof(decimal));
            tabla.Columns.Add("PAGO_IVA", typeof(decimal));
            tabla.Columns.Add("AHORRO_IVA", typeof(decimal));
            return tabla;
        }

        private static decimal ConvertirDecimal(DataRow row, string columna)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(row[columna]);
        }

        private static List<DateTime> ObtenerMesesEnRango(DateTime inicio, DateTime fin)
        {
            var meses = new List<DateTime>();
            var actual = new DateTime(inicio.Year, inicio.Month, 1);
            var ultimo = new DateTime(fin.Year, fin.Month, 1);

            while (actual <= ultimo)
            {
                meses.Add(actual);
                actual = actual.AddMonths(1);
            }

            return meses;
        }
    }
}
