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

  
        public List<DatoDetalleIGI> ObtenerPartidasPorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new List<DatoDetalleIGI>();

            if (string.IsNullOrWhiteSpace(baseDatos))
                return resultado;

            try
            {
                // Determinar base de glosa (puede diferir según razón social)
                string baseGlosa = baseDatos;
                try
                {
                    int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
                    var razonSocial = ObtenerRazonSocial(idRazon);
                    if (!string.IsNullOrEmpty(razonSocial?.BaseDatosOrigen))
                        baseGlosa = razonSocial.BaseDatosOrigen;
                }
                catch
                {
                    baseGlosa = baseDatos;
                }

                Conexion conexionCliente = ObtenerConexionParaBaseDatos(baseDatos);
                Conexion conexionGlosa = ObtenerConexionParaBaseDatos(baseGlosa);

                var dictPedimentos = new Dictionary<string, (decimal IGI_CalculadoDetalle, string FormaPago_IGI)> (StringComparer.OrdinalIgnoreCase);

                // Consultar valores calculados (partidas) en la base cliente
                string sqlPartidas = $@"
SELECT
    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
    DI.FoP_Clave AS FormaPago_IGI,
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0)) AS IGI_CalculadoDetalle
FROM {SqlHelper.Quotename(baseDatos)}.dbo.Di_Pedimento DP
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)='98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
   AND FRA.Pai_Clave='MEX'
   AND FRA.Fra_TipoOper=0
WHERE IIF(DP.CLP_CLAVE='R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) BETWEEN @FechaInicio AND @FechaFin
  AND DI.FoP_Clave IN ('0','5','21')
GROUP BY
    DP.Adu_AduanaSecc,
    DP.AgP_Patente,
    DP.Pim_Folio,
    DI.FoP_Clave;";

                using (var cn = conexionCliente.ObtenerConexion())
                {
                    cn.Open();
                    using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlPartidas, cn);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 300;

                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var ped = rdr.IsDBNull(0) ? string.Empty : rdr.GetString(0);
                        var forma = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                        var igiCalc = rdr.IsDBNull(2) ? 0m : rdr.GetDecimal(2);

                        if (!string.IsNullOrEmpty(ped))
                        {
                            dictPedimentos[ped] = (igiCalc, forma ?? string.Empty);
                        }
                    }
                }

                // Consultar importes pagados en la base de glosa
                string sqlGlosa = $@"
SELECT
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
    TR.Gl_FPagoAdvalorem AS Gl_FPagoAdvalorem,
    SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) AS Gl_ImporteADvalorem
FROM {SqlHelper.Quotename(baseGlosa)}.dbo.TR_GLOSA TR
WHERE CONVERT(date, TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
  AND TR.Gl_TOper = 1
  AND TR.Gl_OrigenZipGlosa = 'S'
  AND TR.Gl_FPagoAdvalorem IN ('0','5','21')
GROUP BY
    TR.GL_ADUANA,
    TR.GL_PATENTE,
    TR.GL_PEDIMENTO,
    TR.Gl_FPagoAdvalorem;";

                var listaGlosa = new List<(string Pedimento, string Forma, decimal Importe)>();
                using (var cn2 = conexionGlosa.ObtenerConexion())
                {
                    cn2.Open();
                    using var cmd2 = new Microsoft.Data.SqlClient.SqlCommand(sqlGlosa, cn2);
                    cmd2.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd2.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd2.CommandTimeout = 300;

                    using var rdr2 = cmd2.ExecuteReader();
                    while (rdr2.Read())
                    {
                        var ped = rdr2.IsDBNull(0) ? string.Empty : rdr2.GetString(0);
                        var forma = rdr2.IsDBNull(1) ? string.Empty : rdr2.GetString(1);
                        var imp = rdr2.IsDBNull(2) ? 0m : rdr2.GetDecimal(2);
                        listaGlosa.Add((ped, forma ?? string.Empty, imp));
                    }
                }

                // Combinar: por cada entrada de glosa crear un DatoDetalleIGI (usamos IGI calculado si existe para el pedimento)
                foreach (var g in listaGlosa)
                {
                    dictPedimentos.TryGetValue(g.Pedimento, out var info);
                    var igiCalc = info.IGI_CalculadoDetalle;
                    var detalle = new DatoDetalleIGI
                    {
                        Gl_ImporteADvalorem = g.Importe,
                        IGI_CalculadoDetalle = igiCalc,
                        Gl_FPagoAdvalorem = g.Forma ?? string.Empty
                    };
                    resultado.Add(detalle);
                }

                // Para pedimentos que tienen cálculo pero no tienen glosa, añadimos con importe 0 y forma tomada de la partida
                foreach (var kv in dictPedimentos)
                {
                    // Si ya se agregó por glosa, omitimos (ya contiene la forma)
                    bool existe = listaGlosa.Any(x => string.Equals(x.Pedimento, kv.Key, StringComparison.OrdinalIgnoreCase));
                    if (existe) continue;

                    var detalle = new DatoDetalleIGI
                    {
                        Gl_ImporteADvalorem = 0m,
                        IGI_CalculadoDetalle = kv.Value.IGI_CalculadoDetalle,
                        Gl_FPagoAdvalorem = kv.Value.FormaPago_IGI ?? string.Empty
                    };
                    resultado.Add(detalle);
                }
            }
            catch
            {
                // En caso de error devolver lista vacía para no interrumpir flujo principal
            }

            return resultado;
        }

        /// <summary>
        /// Estructura para devolver los 4 resultsets del query unificado
        /// </summary>
        public class ResultadoConciliacion
        {
            public System.Data.DataTable DetalleIGI { get; set; } = new System.Data.DataTable();
            public System.Data.DataTable ResumenIGI { get; set; } = new System.Data.DataTable();
            public System.Data.DataTable DetalleIVA { get; set; } = new System.Data.DataTable();
            public System.Data.DataTable ResumenIVA { get; set; } = new System.Data.DataTable();
            public bool FaltaGlosaIVA { get; set; }
            public string BaseDatosGlosa { get; set; } = string.Empty;
        }

        /// <summary>
        /// Ejecuta el query unificado de conciliación IGI/IVA y devuelve las 4 tablas resultantes:
        /// 1. ConciliacionIGI (detalle por pedimento)
        /// 2. ResumenIGI (agrupado por forma de pago)
        /// 3. ConciliacionIVA (detalle IVA)
        /// 4. ResumenIVA (agrupado por forma de pago IVA)
        /// </summary>
        public ResultadoConciliacion ObtenerConciliacionIGI(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new ResultadoConciliacion();

            try
            {
                if (string.IsNullOrWhiteSpace(baseDatos))
                {
                    throw new ArgumentException("El nombre de la base de datos no puede estar vacío", nameof(baseDatos));
                }

                if (fechaFin < fechaInicio)
                {
                    throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio");
                }

                Conexion conexionCliente;
                ConexionExternaInfo infoCliente;

                try
                {
                    conexionCliente = ObtenerConexionParaBaseDatos(baseDatos);
                    infoCliente = ObtenerConexionExterna(baseDatos);
                }
                catch (Exception exConexion)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"XX Error al obtener conexión: {exConexion.Message}");
#endif
                    throw new Exception($"Error al obtener conexión para la base de datos '{baseDatos}': {exConexion.Message}", exConexion);
                }

                if (conexionCliente == null)
                {
                    throw new Exception($"No se pudo establecer la conexión para la base de datos '{baseDatos}'");
                }

                string baseGlosa = baseDatos;
                try
                {
                    int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
                    var razonSocial = ObtenerRazonSocial(idRazon);

                    if (!string.IsNullOrEmpty(razonSocial.BaseDatosOrigen))
                    {
                        baseGlosa = razonSocial.BaseDatosOrigen;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($">> Base de glosa desde RAZONXTABLA.DB: {baseGlosa}");
#endif
                    }
                }
                catch (Exception exRazon)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"XX Error al determinar base de glosa: {exRazon.Message}");
#endif
                    baseGlosa = baseDatos;
                }

                Conexion conexionGlosa;
                ConexionExternaInfo infoGlosa;

                try
                {
                    conexionGlosa = ObtenerConexionParaBaseDatos(baseGlosa);
                    infoGlosa = ObtenerConexionExterna(baseGlosa);
                }
                catch (Exception exGlosa)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"XX Error al obtener conexión de glosa: {exGlosa.Message}");
#endif
                    throw new Exception($"Error al obtener conexión para la base de glosa '{baseGlosa}': {exGlosa.Message}", exGlosa);
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n=== EJECUTANDO CONCILIACION IGI/IVA ===");
                System.Diagnostics.Debug.WriteLine($"Base Cliente lógica: {baseDatos}");
                System.Diagnostics.Debug.WriteLine($"Base Glosa lógica:   {baseGlosa}");
                System.Diagnostics.Debug.WriteLine($"Conexión física cliente: {(infoCliente.TieneConexionExterna ? infoCliente.Servidor : conexionPrincipal.Servidor)} / {baseDatos}");
                System.Diagnostics.Debug.WriteLine($"Conexión física glosa:   {(infoGlosa.TieneConexionExterna ? infoGlosa.Servidor : conexionPrincipal.Servidor)} / {baseGlosa}");
                System.Diagnostics.Debug.WriteLine($"Rango: {fechaInicio:yyyy-MM-dd} a {fechaFin:yyyy-MM-dd}");
#endif

                var pedimentosCliente = new List<(string Pedimento, DateTime FechaPago, string ClpClave, string FormaPagoIGI, decimal IGICalculado)>();
                var pedimentosGlosaIGI = new List<(string Pedimento, DateTime FechaPago, string FormaPagoIGI, decimal IGIPagado)>();
                var pedimentosGlosaIVA = new List<(string Pedimento, DateTime FechaPago, string FormaPagoIVA, decimal IVAPagado)>();

                string sqlCliente = $@"
SELECT
    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
    CONVERT(date, IIF(DP.CLP_CLAVE='R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) AS FechaPago,
    DP.CLP_CLAVE AS Clave,
    DI.FoP_Clave AS FormaPago_IGI,
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0)) AS IGI_Calculado
FROM {SqlHelper.Quotename(baseDatos)}.dbo.Di_Pedimento DP
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
INNER JOIN {SqlHelper.Quotename(baseDatos)}.dbo.Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)='98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
   AND FRA.Pai_Clave='MEX'
   AND FRA.Fra_TipoOper=0
WHERE IIF(DP.CLP_CLAVE='R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) BETWEEN @FechaInicio AND @FechaFin
  AND DI.FoP_Clave IN ('0','5')
GROUP BY
    DP.Adu_AduanaSecc,
    DP.AgP_Patente,
    DP.Pim_Folio,
    CONVERT(date, IIF(DP.CLP_CLAVE='R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)),
    DP.CLP_CLAVE,
    DI.FoP_Clave;";

                string sqlGlosaIGI = $@"
SELECT
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
    CONVERT(date, TR.Gl_FecPagoReal) AS FechaPago,
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,
    SUM(ISNULL(TR.Gl_ImporteADvalorem, 0)) AS IGI_Pagado
FROM {SqlHelper.Quotename(baseGlosa)}.dbo.TR_GLOSA TR
WHERE CONVERT(date, TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
  AND TR.Gl_TOper = 1
  AND TR.Gl_OrigenZipGlosa = 'S'
  AND TR.Gl_FPagoAdvalorem IN ('0','5')
GROUP BY
    TR.GL_ADUANA,
    TR.GL_PATENTE,
    TR.GL_PEDIMENTO,
    CONVERT(date, TR.Gl_FecPagoReal),
    TR.Gl_FPagoAdvalorem;";

                string sqlGlosaIVA = $@"
SELECT
    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
    CONVERT(date, TR.Gl_FecPagoReal) AS FechaPago,
    TR.Gl_FPagoIVA AS FormaPago_IVA,
    SUM(ISNULL(TR.Gl_ImporteIVA, 0)) AS IVA_Pagado
FROM {SqlHelper.Quotename(baseGlosa)}.dbo.TR_GLOSA TR
WHERE CONVERT(date, TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
  AND TR.Gl_TOper = 1
  AND TR.Gl_OrigenZipGlosa = 'S'
  AND TR.Gl_FPagoIVA IN ('0','21')
GROUP BY
    TR.GL_ADUANA,
    TR.GL_PATENTE,
    TR.GL_PEDIMENTO,
    CONVERT(date, TR.Gl_FecPagoReal),
    TR.Gl_FPagoIVA
HAVING SUM(ISNULL(TR.Gl_ImporteIVA, 0)) > 0;";

                try
                {
                    using (var cnCliente = conexionCliente.ObtenerConexion())
                    {
                        cnCliente.Open();

                        using var cmdCliente = new Microsoft.Data.SqlClient.SqlCommand(sqlCliente, cnCliente);
                        cmdCliente.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmdCliente.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmdCliente.CommandTimeout = 300;

                        using var readerCliente = cmdCliente.ExecuteReader();
                        while (readerCliente.Read())
                        {
                            string formaPagoIGI = readerCliente.IsDBNull(3) ? string.Empty : readerCliente.GetString(3).Trim();

                            if (formaPagoIGI == "0" || formaPagoIGI == "5")
                            {
                                pedimentosCliente.Add((
                                    readerCliente.IsDBNull(0) ? string.Empty : readerCliente.GetString(0),
                                    readerCliente.IsDBNull(1) ? DateTime.MinValue : readerCliente.GetDateTime(1),
                                    readerCliente.IsDBNull(2) ? string.Empty : readerCliente.GetString(2).Trim(),
                                    formaPagoIGI,
                                    readerCliente.IsDBNull(4) ? 0m : readerCliente.GetDecimal(4)
                                ));
                            }
                        }
                    }

                    using (var cnGlosa = conexionGlosa.ObtenerConexion())
                    {
                        cnGlosa.Open();

                        using (var cmdGlosaIGI = new Microsoft.Data.SqlClient.SqlCommand(sqlGlosaIGI, cnGlosa))
                        {
                            cmdGlosaIGI.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                            cmdGlosaIGI.Parameters.AddWithValue("@FechaFin", fechaFin);
                            cmdGlosaIGI.CommandTimeout = 300;

                            using var readerGlosaIGI = cmdGlosaIGI.ExecuteReader();
                            while (readerGlosaIGI.Read())
                            {
                                pedimentosGlosaIGI.Add((
                                    readerGlosaIGI.IsDBNull(0) ? string.Empty : readerGlosaIGI.GetString(0),
                                    readerGlosaIGI.IsDBNull(1) ? DateTime.MinValue : readerGlosaIGI.GetDateTime(1),
                                    readerGlosaIGI.IsDBNull(2) ? string.Empty : readerGlosaIGI.GetString(2),
                                    readerGlosaIGI.IsDBNull(3) ? 0m : readerGlosaIGI.GetDecimal(3)
                                ));
                            }
                        }

                        using var cmdGlosaIVA = new Microsoft.Data.SqlClient.SqlCommand(sqlGlosaIVA, cnGlosa);
                        cmdGlosaIVA.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmdGlosaIVA.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmdGlosaIVA.CommandTimeout = 300;

                        using var readerGlosaIVA = cmdGlosaIVA.ExecuteReader();
                        while (readerGlosaIVA.Read())
                        {
                            pedimentosGlosaIVA.Add((
                                readerGlosaIVA.IsDBNull(0) ? string.Empty : readerGlosaIVA.GetString(0),
                                readerGlosaIVA.IsDBNull(1) ? DateTime.MinValue : readerGlosaIVA.GetDateTime(1),
                                readerGlosaIVA.IsDBNull(2) ? string.Empty : readerGlosaIVA.GetString(2),
                                readerGlosaIVA.IsDBNull(3) ? 0m : readerGlosaIVA.GetDecimal(3)
                            ));
                        }
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"XX Error al ejecutar consulta de conciliación:");
                    System.Diagnostics.Debug.WriteLine($"   Mensaje: {sqlEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"   Número: {sqlEx.Number}");
                    System.Diagnostics.Debug.WriteLine($"   Línea: {sqlEx.LineNumber}");
                    System.Diagnostics.Debug.WriteLine($"   Procedimiento: {sqlEx.Procedure}");
#endif
                    throw new Exception($"Error al ejecutar consulta de conciliación: {sqlEx.Message} (Error SQL: {sqlEx.Number}, Línea: {sqlEx.LineNumber})", sqlEx);
                }

                var detalleIGI = new System.Data.DataTable();
                detalleIGI.Columns.Add("Pedimento", typeof(string));
                detalleIGI.Columns.Add("FechaPago", typeof(DateTime));
                detalleIGI.Columns.Add("Clave", typeof(string));
                detalleIGI.Columns.Add("FormaPago_IGI", typeof(string));
                detalleIGI.Columns.Add("IGI_Pagado", typeof(decimal));
                detalleIGI.Columns.Add("IGI_Calculado", typeof(decimal));
                detalleIGI.Columns.Add("Diferencia_IGI", typeof(decimal));
                detalleIGI.Columns.Add("Estatus", typeof(string));

                var resumenIGI = new System.Data.DataTable();
                resumenIGI.Columns.Add("Año", typeof(int));
                resumenIGI.Columns.Add("Mes", typeof(int));
                resumenIGI.Columns.Add("FormaPago_IGI", typeof(string));
                resumenIGI.Columns.Add("IGI_Pagado", typeof(decimal));
                resumenIGI.Columns.Add("IGI_Calculado", typeof(decimal));
                resumenIGI.Columns.Add("Diferencia_IGI", typeof(decimal));

                var detalleIVA = new System.Data.DataTable();
                detalleIVA.Columns.Add("Pedimento", typeof(string));
                detalleIVA.Columns.Add("FechaPago", typeof(DateTime));
                detalleIVA.Columns.Add("Clave", typeof(string));
                detalleIVA.Columns.Add("FormaPago_IVA", typeof(string));
                detalleIVA.Columns.Add("IVA_Pagado", typeof(decimal));

                var resumenIVA = new System.Data.DataTable();
                resumenIVA.Columns.Add("Año", typeof(int));
                resumenIVA.Columns.Add("Mes", typeof(int));
                resumenIVA.Columns.Add("FormaPago_IVA", typeof(string));
                resumenIVA.Columns.Add("IVA_Pagado", typeof(decimal));

                var glosaIGIPorClave = pedimentosGlosaIGI
                    .GroupBy(x => x.Pedimento + "|" + x.FormaPagoIGI, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                foreach (var cliente in pedimentosCliente.OrderBy(x => x.Pedimento).ThenBy(x => x.FormaPagoIGI))
                {
                    string clave = cliente.Pedimento + "|" + cliente.FormaPagoIGI;
                    decimal igiPagadoReal = 0m;

                    if (glosaIGIPorClave.TryGetValue(clave, out var coincidencias) && coincidencias.Count > 0)
                    {
                        igiPagadoReal = coincidencias.Sum(x => x.IGIPagado);
                    }

                    decimal igiPagadoMostrado = cliente.FormaPagoIGI == "5" ? 0m : igiPagadoReal;
                    decimal diferencia = cliente.IGICalculado - igiPagadoMostrado;
                    string estatus = Math.Abs(cliente.IGICalculado - igiPagadoReal) > 1 ? "DIFERENCIA" : "OK";

                    detalleIGI.Rows.Add(
                        cliente.Pedimento,
                        cliente.FechaPago,
                        cliente.ClpClave,
                        cliente.FormaPagoIGI,
                        igiPagadoMostrado,
                        cliente.IGICalculado,
                        diferencia,
                        estatus);
                }

                foreach (System.Data.DataRow r in detalleIGI.Rows)
                {
                    // iteración forzada para evitar resolución errónea de AsEnumerable por PLINQ
                }

                var resumenIGIGroups = detalleIGI.Rows.Cast<System.Data.DataRow>()
                    .Where(r =>
                    {
                        var forma = Convert.ToString(r["FormaPago_IGI"])?.Trim();
                        return forma == "0" || forma == "5";
                    })
                    .GroupBy(r => new
                    {
                        Anio = ((DateTime)r["FechaPago"]).Year,
                        Mes = ((DateTime)r["FechaPago"]).Month,
                        Forma = Convert.ToString(r["FormaPago_IGI"])?.Trim() ?? string.Empty
                    })
                    .OrderBy(g => g.Key.Anio)
                    .ThenBy(g => g.Key.Mes)
                    .ThenBy(g => g.Key.Forma);

                foreach (var grp in resumenIGIGroups)
                {
                    decimal igiPagado = grp.Sum(r => Convert.ToDecimal(r["IGI_Pagado"]));
                    decimal igiCalculado = grp.Sum(r => Convert.ToDecimal(r["IGI_Calculado"]));
                    decimal diferencia = grp.Sum(r => Convert.ToDecimal(r["Diferencia_IGI"]));

                    resumenIGI.Rows.Add(grp.Key.Anio, grp.Key.Mes, grp.Key.Forma, igiPagado, igiCalculado, diferencia);
                }

                var pedimentosClienteDistinct = pedimentosCliente
                    .Select(x => x.Pedimento)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var clavePedimentoPorNumero = pedimentosCliente
                    .GroupBy(x => x.Pedimento, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ClpClave).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                        StringComparer.OrdinalIgnoreCase);

                bool existeGlosaIVAParaPedimentos = pedimentosGlosaIVA
                    .Any(x => pedimentosClienteDistinct.Contains(x.Pedimento));

                foreach (var row in pedimentosGlosaIVA
                    .Where(x => pedimentosClienteDistinct.Contains(x.Pedimento))
                    .OrderBy(x => x.Pedimento)
                    .ThenBy(x => x.FormaPagoIVA))
                {
                    string clpClave = clavePedimentoPorNumero.TryGetValue(row.Pedimento, out var clave) ? clave : string.Empty;
                    detalleIVA.Rows.Add(row.Pedimento, row.FechaPago, clpClave, row.FormaPagoIVA, row.IVAPagado);
                }

                var resumenIVAGroups = detalleIVA.Rows.Cast<System.Data.DataRow>()
                    .GroupBy(r => new
                    {
                        Anio = ((DateTime)r["FechaPago"]).Year,
                        Mes = ((DateTime)r["FechaPago"]).Month,
                        Forma = Convert.ToString(r["FormaPago_IVA"]) ?? string.Empty
                    })
                    .OrderBy(g => g.Key.Anio)
                    .ThenBy(g => g.Key.Mes)
                    .ThenBy(g => g.Key.Forma);

                foreach (var grp in resumenIVAGroups)
                {
                    decimal ivaPagado = grp.Sum(r => Convert.ToDecimal(r["IVA_Pagado"]));
                    resumenIVA.Rows.Add(grp.Key.Anio, grp.Key.Mes, grp.Key.Forma, ivaPagado);
                }

                resultado.DetalleIGI = detalleIGI;
                resultado.ResumenIGI = resumenIGI;
                resultado.DetalleIVA = detalleIVA;
                resultado.ResumenIVA = resumenIVA;
                resultado.BaseDatosGlosa = baseGlosa;
                resultado.FaltaGlosaIVA = pedimentosClienteDistinct.Count > 0 && !existeGlosaIVAParaPedimentos;

#if DEBUG
                System.Diagnostics.Debug.WriteLine($">> Pedimentos cliente: {pedimentosCliente.Count} registros");
                System.Diagnostics.Debug.WriteLine($">> Pedimentos glosa IGI: {pedimentosGlosaIGI.Count} registros");
                System.Diagnostics.Debug.WriteLine($">> Pedimentos glosa IVA: {pedimentosGlosaIVA.Count} registros");
                System.Diagnostics.Debug.WriteLine($">> Detalle IGI: {resultado.DetalleIGI.Rows.Count} registros");
                System.Diagnostics.Debug.WriteLine($">> Resumen IGI: {resultado.ResumenIGI.Rows.Count} formas de pago");
                System.Diagnostics.Debug.WriteLine($">> Detalle IVA: {resultado.DetalleIVA.Rows.Count} registros");
                System.Diagnostics.Debug.WriteLine($">> Resumen IVA: {resultado.ResumenIVA.Rows.Count} formas de pago");
#endif
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\nXX ERROR SQL en ObtenerConciliacionIGI:");
                System.Diagnostics.Debug.WriteLine($"   Mensaje: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"   Número de error: {sqlEx.Number}");
                System.Diagnostics.Debug.WriteLine($"   Línea: {sqlEx.LineNumber}");
                System.Diagnostics.Debug.WriteLine($"   Procedimiento: {sqlEx.Procedure}");
                System.Diagnostics.Debug.WriteLine($"   Servidor: {sqlEx.Server}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {sqlEx.StackTrace}");
#endif
                throw new Exception($"Error SQL al obtener conciliación IGI/IVA: {sqlEx.Message} (Número: {sqlEx.Number}, Línea: {sqlEx.LineNumber})", sqlEx);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\nXX ERROR GENERAL en ObtenerConciliacionIGI:");
                System.Diagnostics.Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
#endif
                throw new Exception($"Error al obtener conciliación IGI/IVA: {ex.Message}", ex);
            }

            return resultado;
        }

       
 
        
    }
}
