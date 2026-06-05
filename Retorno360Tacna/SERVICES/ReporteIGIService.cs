using System;
using System.Collections.Generic;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System.Linq;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Nuevo esqueleto para ReporteIGIService. Se eliminó la implementación previa de IGI/pedimentos
    /// para reiniciar la funcionalidad desde cero.
    /// </summary>
    public partial class ReporteIGIService : ReporteServiceBase
    {
        public ReporteIGIService(ConexionInfo conexion) : base(conexion)
        {
        }

        // TODO: implementar desde cero la nueva funcionalidad de IGI.
        // Método placeholder que servirá como punto de partida.
        public void InicializarReporteIGI()
        {
            // Implementación nueva aquí
        }

        // Compatibilidad mínima: stubs para los métodos usados por la UI mientras se reimplementa IGI
        public List<ReporteIGIPagado> GenerarReporteIGIPorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            return new List<ReporteIGIPagado>();
        }

        public List<ReporteIGIPagado> GenerarReporteIGI(string baseDatos, DateTime fechaInicio, DateTime fechaFin, bool sinValidacionGlosa = false)
        {
            return new List<ReporteIGIPagado>();
        }

        public ResumenIGI GenerarResumen(List<ReporteIGIPagado> reportes)
        {
            if (reportes == null || !reportes.Any()) return new ResumenIGI();

            return new ResumenIGI
            {
                TotalIGI_Pagado = reportes.Sum(r => r.IGI_Pagado),
                TotalIGI_Calculado = reportes.Sum(r => r.IGI_Calculado),
                TotalIVA_Pagado = reportes.Sum(r => r.IVA_Pagado),
                TotalPedimentos = reportes.Count,
                PedimentosCargadosGlosa = reportes.Count(r => r.EstatusGlosa == "SI CARGADO")
            };
        }

        public List<ReporteIGIPagado> ObtenerDetallePorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            return new List<ReporteIGIPagado>();
        }

        public List<ReporteIGIPagado> ObtenerDetallePorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            return new List<ReporteIGIPagado>();
        }

        public List<DatoDetalleIGI> ObtenerPartidasPorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            return new List<DatoDetalleIGI>();
        }

        /// <summary>
        /// Ejecuta el flujo de resumen tal como el query suministrado por el usuario.
        /// Devuelve dos DataTables: (IGI, IVA) con las columnas equivalentes al query original.
        /// La lógica intenta resolver la conexión del cliente y la de glosa según la razón social
        /// asociada a la base cliente; si no se encuentra otra base de glosa, usa la misma base cliente.
        /// </summary>
        public (System.Data.DataTable IGI, System.Data.DataTable IVA) ObtenerResumenTablasPorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            // Helpers internos
            var tablaIGI = new System.Data.DataTable();
            tablaIGI.Columns.Add("Año", typeof(int));
            tablaIGI.Columns.Add("Mes", typeof(int));
            tablaIGI.Columns.Add("IGI_Pagado", typeof(decimal));
            tablaIGI.Columns.Add("IGI_Calculado", typeof(decimal));
            tablaIGI.Columns.Add("FormaPago_IGI", typeof(string));

            var tablaIVA = new System.Data.DataTable();
            tablaIVA.Columns.Add("Año", typeof(int));
            tablaIVA.Columns.Add("Mes", typeof(int));
            tablaIVA.Columns.Add("IVA_Pagado", typeof(decimal));
            tablaIVA.Columns.Add("FormaPago_IVA", typeof(string));

            try
            {
                // Determinar conexión cliente
                var conexionCliente = ObtenerConexionParaBaseDatos(baseDatos);

                // Determinar posible base de glosa para la misma razón social
                string baseGlosa = baseDatos;
                try
                {
                    int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
                    var bases = ObtenerBasesDatosConConexion(idRazon);
                    // Preferir una base distinta a la base cliente que contenga 'ABLE' o 'GLOSA', si existe
                    var candidata = bases.FirstOrDefault(b => !string.Equals(b.BaseDatos, baseDatos, StringComparison.OrdinalIgnoreCase)
                        && (b.BaseDatos.IndexOf("ABLE", StringComparison.OrdinalIgnoreCase) >= 0 || b.BaseDatos.IndexOf("GLOSA", StringComparison.OrdinalIgnoreCase) >= 0));
                    if (candidata == null)
                    {
                        // fallback: cualquier base distinta
                        candidata = bases.FirstOrDefault(b => !string.Equals(b.BaseDatos, baseDatos, StringComparison.OrdinalIgnoreCase));
                    }
                    if (candidata != null)
                    {
                        baseGlosa = candidata.BaseDatos;
                    }
                }
                catch
                {
                    // Si no podemos determinar razon social, usar la misma base para glosa
                    baseGlosa = baseDatos;
                }

                var conexionGlosa = ObtenerConexionParaBaseDatos(baseGlosa);

                // Cargar pedimentos del cliente
                var pedimentosCliente = new List<(string Pedimento, DateTime? FechaPago, decimal IGI_Calculado, string FormaPago)>();
                using (var cn = conexionCliente.ObtenerConexion())
                {
                    cn.Open();
                    string sqlCliente = $@"
SELECT 
    DP.Pim_Consecutivo AS iDPedimento,
    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
    IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago) AS FechaPago,
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,
    DI.FoP_Clave AS FormaPago_IGI
FROM {SqlHelper.Quotename(baseDatos)}.dbo.Di_Pedimento DP
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Di_PedimentoDet DI ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Ca_Farancelaria FRA ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion)
    AND FRA.Pai_Clave = 'MEX'
    AND FRA.Fra_TipoOper = 0
WHERE 
    IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago) BETWEEN @FechaInicio AND @FechaFin
GROUP BY  
    DP.Pim_Consecutivo,
    DP.Adu_AduanaSecc,
    DP.AgP_Patente,
    DP.Pim_Folio,
    IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago),
    DI.FoP_Clave
";

                    using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlCliente, cn);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string ped = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                        DateTime? fp = rdr.IsDBNull(2) ? (DateTime?)null : rdr.GetDateTime(2);
                        decimal igiCalc = rdr.IsDBNull(3) ? 0m : rdr.GetDecimal(3);
                        string fpago = rdr.IsDBNull(4) ? string.Empty : rdr.GetString(4);
                        pedimentosCliente.Add((ped, fp, igiCalc, fpago));
                    }
                }

                // Cargar pedimentos de glosa IGI
                var pedimentosGlosaIGI = new List<(string Pedimento, DateTime FechaPago, decimal IGI_Pagado, string FormaPago)>();
                using (var cn = conexionGlosa.ObtenerConexion())
                {
                    cn.Open();
                    string sqlGlosaIGI = $@"
SELECT 
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
    CONVERT(DATE, TR.Gl_FecPagoReal) AS FechaPago,
    SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) AS IGI_Pagado,
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI
FROM {SqlHelper.Quotename(baseGlosa)}.DBO.TR_GLOSA TR
WHERE 
    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
    AND TR.Gl_TOper = 1
    AND TR.Gl_OrigenZipGlosa = 'S'
    AND (
        TR.Gl_FPagoAdvalorem IN ('0','5')
    )
GROUP BY  
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO
    ,CONVERT(DATE, TR.Gl_FecPagoReal)
    ,TR.Gl_FPagoAdvalorem
HAVING SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) > 0 ";

                    using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlGlosaIGI, cn);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string ped = rdr.IsDBNull(0) ? string.Empty : rdr.GetString(0);
                        DateTime fp = rdr.IsDBNull(1) ? DateTime.MinValue : rdr.GetDateTime(1);
                        decimal igiPag = rdr.IsDBNull(2) ? 0m : rdr.GetDecimal(2);
                        string fpago = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);
                        pedimentosGlosaIGI.Add((ped, fp, igiPag, fpago));
                    }
                }

                // Cargar pedimentos de glosa IVA
                var pedimentosGlosaIVA = new List<(string Pedimento, DateTime FechaPago, decimal IVA_Pagado, string FormaPago)>();
                using (var cn = conexionGlosa.ObtenerConexion())
                {
                    cn.Open();
                    string sqlGlosaIVA = $@"
SELECT 
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
    CONVERT(DATE, TR.Gl_FecPagoReal) AS FechaPago,
    SUM(ISNULL(TR.Gl_ImporteIVA,0)) AS IVA_Pagado,
    TR.Gl_FPagoIVA AS FormaPago_IVA
FROM {SqlHelper.Quotename(baseGlosa)}.DBO.TR_GLOSA TR
WHERE 
    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
    AND TR.Gl_TOper = 1
    AND TR.Gl_OrigenZipGlosa = 'S'
    AND (
        TR.Gl_FPagoIVA IN ('0','21') 
    )
GROUP BY  
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO
    ,CONVERT(DATE, TR.Gl_FecPagoReal)
    ,TR.Gl_FPagoIVA
HAVING SUM(ISNULL(TR.Gl_ImporteIVA,0)) > 0
";

                    using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlGlosaIVA, cn);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string ped = rdr.IsDBNull(0) ? string.Empty : rdr.GetString(0);
                        DateTime fp = rdr.IsDBNull(1) ? DateTime.MinValue : rdr.GetDateTime(1);
                        decimal ivaPag = rdr.IsDBNull(2) ? 0m : rdr.GetDecimal(2);
                        string fpago = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);
                        pedimentosGlosaIVA.Add((ped, fp, ivaPag, fpago));
                    }
                }

                // Calcular tabla IGI: join entre pedimentosGlosaIGI y pedimentosCliente por Pedimento y FormaPago
                var joinIGI = from g in pedimentosGlosaIGI
                              join c in pedimentosCliente on new { g.Pedimento, g.FormaPago } equals new { Pedimento = c.Pedimento, FormaPago = c.FormaPago }
                              group new { g, c } by new { Año = g.FechaPago.Year, Mes = g.FechaPago.Month, Forma = g.FormaPago } into grp
                              select new
                              {
                                  grp.Key.Año,
                                  grp.Key.Mes,
                                  Forma = grp.Key.Forma,
                                  IGI_Pagado = grp.Sum(x => x.g.IGI_Pagado),
                                  IGI_Calculado = grp.Sum(x => x.c.IGI_Calculado)
                              };

                foreach (var row in joinIGI.OrderBy(r => r.Año).ThenBy(r => r.Mes).ThenBy(r => r.Forma))
                {
                    tablaIGI.Rows.Add(row.Año, row.Mes, row.IGI_Pagado, row.IGI_Calculado, row.Forma);
                }

                // Calcular tabla IVA: join entre pedimentosGlosaIVA y distinct pedimentosCliente
                var pedimentosClienteDistinct = pedimentosCliente.Select(p => p.Pedimento).Distinct().ToHashSet();

                var joinIVA = from g in pedimentosGlosaIVA
                              where pedimentosClienteDistinct.Contains(g.Pedimento)
                              group g by new { Año = g.FechaPago.Year, Mes = g.FechaPago.Month, Forma = g.FormaPago } into grp
                              select new
                              {
                                  grp.Key.Año,
                                  grp.Key.Mes,
                                  Forma = grp.Key.Forma,
                                  IVA_Pagado = grp.Sum(x => x.IVA_Pagado)
                              };

                foreach (var row in joinIVA.OrderBy(r => r.Año).ThenBy(r => r.Mes).ThenBy(r => r.Forma))
                {
                    tablaIVA.Rows.Add(row.Año, row.Mes, row.IVA_Pagado, row.Forma);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen por base: {ex.Message}", ex);
            }

            return (tablaIGI, tablaIVA);
        }
    }
}

















































































































































































































































































































































































































































































































