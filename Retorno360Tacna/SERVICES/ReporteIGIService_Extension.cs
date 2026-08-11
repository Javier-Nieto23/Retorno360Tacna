using Retorno360Tacna.MODELS;
using System.Linq;
using System.Data;

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

            // Columnas específicas para IGI (definir antes de agregar filas)
            dt.Columns.Add("MES", typeof(string));
            dt.Columns.Add("IGI PAGADO", typeof(decimal));
            dt.Columns.Add("IGI CALCULADO", typeof(decimal));
            dt.Columns.Add("DIFERENCIA", typeof(decimal));
            dt.Columns.Add("FORMA DE PAGO IGI", typeof(string));

            if (!reportes.Any())
                return dt;

            // Recalcular agrupado por MES y FORMA DE PAGO IGI usando partidas crudas para asegurar suma correcta
            // Determinar meses a procesar a partir de los reportes
            var meses = reportes
                .Where(r => r.FechaPago.HasValue)
                .Select(r => new DateTime(r.FechaPago.Value.Year, r.FechaPago.Value.Month, 1))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Cache de partidas por base+mes para evitar múltiples consultas
            var cachePartidas = new Dictionary<(string BaseDb, DateTime Mes), List<DatoDetalleIGI>>();

            // Bases involucradas en el conjunto de reportes
            var basesInvolucradas = reportes.Select(r => r.BaseDatos).Distinct().ToList();

            foreach (var mes in meses)
            {
                var fechaInicio = mes;
                var fechaFin = mes.AddMonths(1).AddDays(-1);

                // Recolectar todas las partidas para el mes de todas las bases
                var partidasMes = new List<DatoDetalleIGI>();

                foreach (var baseDb in basesInvolucradas)
                {
                    var key = (baseDb, mes);
                    if (!cachePartidas.TryGetValue(key, out var partidasBase))
                    {
                        try
                        {
                            partidasBase = ObtenerPartidasPorBase(baseDb, fechaInicio, fechaFin) ?? new List<DatoDetalleIGI>();
                        }
                        catch
                        {
                            partidasBase = new List<DatoDetalleIGI>();
                        }

                        cachePartidas[key] = partidasBase;
                    }

                    if (partidasBase.Any())
                    {
                        // Filtrar partidas que correspondan a IGI (según query original: Gl_FPagoAdvalorem IN 0,5,21)
                        partidasMes.AddRange(partidasBase.Where(p => (p.Gl_FPagoAdvalorem ?? string.Empty).Trim() == "0" || (p.Gl_FPagoAdvalorem ?? string.Empty).Trim() == "5" || (p.Gl_FPagoAdvalorem ?? string.Empty).Trim() == "21"));
                    }
                }

                if (!partidasMes.Any())
                    continue;

                // Agrupar por forma de pago IGI dentro del mes
                var agrupado = partidasMes
                    .GroupBy(p => (p.Gl_FPagoAdvalorem ?? string.Empty).Trim())
                    .Select(g => new
                    {
                        FormaPago = string.IsNullOrEmpty(g.Key) ? string.Empty : g.Key,
                        IGI_Pagado = g.Sum(x => x.Gl_ImporteADvalorem),
                        IGI_Calculado = g.Sum(x => x.IGI_CalculadoDetalle)
                    })
                    .OrderBy(x => x.FormaPago)
                    .ToList();

                foreach (var grp in agrupado)
                {
                    decimal igiPagado = grp.IGI_Pagado;
                    decimal igiCalculado = grp.IGI_Calculado;

                    decimal diferencia;
                    if (!string.IsNullOrWhiteSpace(grp.FormaPago) && grp.FormaPago.Trim() == "5")
                    {
                        // Para forma 5, IGI_Pagado se considera 0 en reportes (regla de negocio)
                        diferencia = 0m - igiCalculado;
                        igiPagado = 0m;
                    }
                    else
                    {
                        diferencia = igiCalculado - igiPagado;
                    }

                    dt.Rows.Add(
                        mes.ToString("MMMM yyyy"),
                        igiPagado,
                        igiCalculado,
                        diferencia,
                        grp.FormaPago
                    );
                }
            }


            // Terminada la agregación a partir de partidas (método preciso), devolvemos el DataTable.
            // Eliminamos el bloque que anteriormente agregaba filas a partir del agrupado de reportes
            // para evitar duplicidad y asegurar que los valores vengan de las partidas reales.

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
