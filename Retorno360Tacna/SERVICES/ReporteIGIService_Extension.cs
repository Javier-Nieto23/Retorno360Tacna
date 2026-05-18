using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Extensión del servicio ReporteIGIService con métodos adicionales para agrupación por mes
    /// </summary>
    public partial class ReporteIGIService
    {
        /// <summary>
        /// Exporta los datos de IGI a DataTable agrupado por MES y FORMA DE PAGO IGI
        /// </summary>
        public System.Data.DataTable ConvertirADataTableIGI(List<ReporteIGIPagado> reportes)
        {
            var dt = new System.Data.DataTable();

            // Columnas específicas para IGI
            dt.Columns.Add("MES", typeof(string));
            dt.Columns.Add("IGI PAGADO", typeof(decimal));
            dt.Columns.Add("IGI CALCULADO", typeof(decimal));
            dt.Columns.Add("DIFERENCIA", typeof(decimal));
            dt.Columns.Add("FORMA DE PAGO IGI", typeof(string));

            if (!reportes.Any())
                return dt;

            // Agrupar por MES y FORMA DE PAGO IGI
            var agrupadoPorMesIGI = reportes
                .Where(r => r.FechaPago.HasValue && (r.FormaPago_IGI == "0" || r.FormaPago_IGI == "5"))
                .GroupBy(r => new
                {
                    Año = r.FechaPago.Value.Year,
                    Mes = r.FechaPago.Value.Month,
                    FormaPago = r.FormaPago_IGI
                })
                .Select(g => new
                {
                    MesTexto = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM yyyy"),
                    MesOrden = new DateTime(g.Key.Año, g.Key.Mes, 1),
                    g.Key.FormaPago,
                    IGI_Pagado = g.Sum(r => r.IGI_Pagado),
                    IGI_Calculado = g.Sum(r => r.IGI_Calculado)
                })
                .OrderBy(g => g.MesOrden)
                .ThenBy(g => g.FormaPago);

            foreach (var grupo in agrupadoPorMesIGI)
            {
                // Calcular diferencia: Calculado - Pagado (positivo = ahorro)
                decimal diferencia = grupo.IGI_Calculado - grupo.IGI_Pagado;

                dt.Rows.Add(
                    grupo.MesTexto,
                    grupo.IGI_Pagado,
                    grupo.IGI_Calculado,
                    diferencia,
                    grupo.FormaPago
                );
            }

            return dt;
        }

        /// <summary>
        /// Exporta los datos de IVA a DataTable agrupado por MES y FORMA DE PAGO IVA
        /// </summary>
        public System.Data.DataTable ConvertirADataTableIVA(List<ReporteIGIPagado> reportes)
        {
            var dt = new System.Data.DataTable();

            // Columnas específicas para IVA
            dt.Columns.Add("MES", typeof(string));
            dt.Columns.Add("IVA PAGADO", typeof(decimal));
            dt.Columns.Add("FORMA DE PAGO IVA", typeof(string));

            if (!reportes.Any())
                return dt;

            // Agrupar por MES y FORMA DE PAGO IVA
            var agrupadoPorMesIVA = reportes
                .Where(r => r.FechaPago.HasValue && (r.FormaPago_IVA == "0" || r.FormaPago_IVA == "21"))
                .GroupBy(r => new
                {
                    Año = r.FechaPago.Value.Year,
                    Mes = r.FechaPago.Value.Month,
                    FormaPago = r.FormaPago_IVA
                })
                .Select(g => new
                {
                    MesTexto = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM yyyy"),
                    MesOrden = new DateTime(g.Key.Año, g.Key.Mes, 1),
                    g.Key.FormaPago,
                    IVA_Pagado = g.Sum(r => r.IVA_Pagado)
                })
                .OrderBy(g => g.MesOrden)
                .ThenBy(g => g.FormaPago);

            foreach (var grupo in agrupadoPorMesIVA)
            {
                dt.Rows.Add(
                    grupo.MesTexto,
                    grupo.IVA_Pagado,
                    grupo.FormaPago
                );
            }

            return dt;
        }
    }
}
