using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Servicio especializado para reportes de IGI Pagado
    /// Hereda de ReporteServiceBase para reutilizar lógica de conexiones
    /// </summary>
    public class ReporteIGIService : ReporteServiceBase
    {
        public ReporteIGIService(ConexionInfo conexion) : base(conexion)
        {
        }

        /// <summary>
        /// Genera el reporte de IGI Pagado para una base de datos específica
        /// </summary>
        public List<ReporteIGIPagado> GenerarReporteIGI(string baseDatos, DateTime fechaInicio, DateTime fechaFin, bool sinValidacionGlosa = false)
        {
            if (sinValidacionGlosa)
            {
                return GenerarReporteIGISinGlosa(baseDatos, fechaInicio, fechaFin);
            }
            else
            {
                return GenerarReporteIGIConGlosa(baseDatos, fechaInicio, fechaFin);
            }
        }

        /// <summary>
        /// Genera el reporte de IGI Pagado para todas las bases de datos de una razón social
        /// Obtiene datos de todas las bases y los agrupa al final por pedimento
        /// </summary>
        public List<ReporteIGIPagado> GenerarReporteIGIPorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultados = new List<ReporteIGIPagado>();

            try
            {
                // Paso 1: Obtener la razón social y su base de TR_GLOSA
                var razonSocial = ObtenerRazonSocial(idRazon);
                string baseDatosGlosa = razonSocial.BaseDatosOrigen;

                System.Diagnostics.Debug.WriteLine($"Razón Social: {razonSocial.NombreRazon}");
                System.Diagnostics.Debug.WriteLine($"Base de datos TR_GLOSA: {baseDatosGlosa}");

                // Paso 2: Obtener todas las bases de datos con su información de conexión
                var basesDatosConConexion = ObtenerBasesDatosConConexion(idRazon);

                if (!basesDatosConConexion.Any())
                {
                    throw new Exception("No se encontraron bases de datos para la razón social seleccionada.");
                }

                // Paso 3: Obtener datos RAW de todas las bases (sin agrupar todavía)
                // IMPORTANTE: Ahora usamos TR_GLOSA desde baseDatosGlosa para todas las consultas
                var datosDetalleCompleto = new List<DatoDetalleIGI>();

                foreach (var conexionInfo in basesDatosConConexion)
                {
                    try
                    {
                        // Obtener la conexión apropiada para la base de pedimentos
                        Conexion conexionPedimentos;

                        if (conexionInfo.UsarConexionPrincipal)
                        {
                            conexionPedimentos = new Conexion(
                                conexionPrincipal.Servidor ?? string.Empty,
                                conexionPrincipal.UsuarioSQL ?? string.Empty,
                                conexionPrincipal.PasswordSQL ?? string.Empty,
                                conexionInfo.BaseDatos
                            );
                        }
                        else
                        {
                            conexionPedimentos = new Conexion(
                                conexionInfo.Servidor ?? string.Empty,
                                conexionInfo.UsuarioSQL ?? string.Empty,
                                conexionInfo.PasswordSQL ?? string.Empty,
                                conexionInfo.BaseDatos
                            );
                        }

                        // Obtener conexión para la base de TR_GLOSA
                        var conexionInfoGlosa = ObtenerConexionExterna(baseDatosGlosa);
                        Conexion conexionGlosa;

                        if (conexionInfoGlosa.UsarConexionPrincipal)
                        {
                            conexionGlosa = new Conexion(
                                conexionPrincipal.Servidor ?? string.Empty,
                                conexionPrincipal.UsuarioSQL ?? string.Empty,
                                conexionPrincipal.PasswordSQL ?? string.Empty,
                                baseDatosGlosa
                            );
                        }
                        else
                        {
                            conexionGlosa = new Conexion(
                                conexionInfoGlosa.Servidor ?? string.Empty,
                                conexionInfoGlosa.UsuarioSQL ?? string.Empty,
                                conexionInfoGlosa.PasswordSQL ?? string.Empty,
                                baseDatosGlosa
                            );
                        }

                        // Obtener datos RAW (detalles sin agrupar) con JOIN cruzado
                        var datosBase = ObtenerDatosDetalleConJoinCruzado(
                            conexionInfo.BaseDatos,      // Base de Di_Pedimento
                            baseDatosGlosa,              // Base de TR_GLOSA
                            fechaInicio,
                            fechaFin,
                            conexionPedimentos,
                            conexionGlosa
                        );

                        // Log de diagnóstico
                        System.Diagnostics.Debug.WriteLine($"Base: {conexionInfo.BaseDatos} - Registros obtenidos: {datosBase.Count}");

                        datosDetalleCompleto.AddRange(datosBase);
                    }
                    catch (Exception ex)
                    {
                        // Log detallado del error pero continuar con las demás bases
                        var mensajeError = $"Error consultando {conexionInfo.BaseDatos}: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine(mensajeError);
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                        // También podríamos guardar esto en una lista de errores para mostrarlo al usuario
                        // Por ahora solo continuamos con las demás bases
                    }
                }

                if (!datosDetalleCompleto.Any())
                {
                    throw new Exception("No se encontraron registros en ninguna base de datos de la razón social.");
                }

                System.Diagnostics.Debug.WriteLine($"Total de registros detalle obtenidos: {datosDetalleCompleto.Count}");

                // Paso 4: Agrupar TODOS los datos por pedimento (de todas las bases)
                resultados = AgruparDatosPorPedimento(datosDetalleCompleto);

                System.Diagnostics.Debug.WriteLine($"Total de pedimentos agrupados: {resultados.Count}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte por razón social: {ex.Message}", ex);
            }

            return resultados;
        }


        /// <summary>
        /// Genera el reporte validando contra TR_GLOSA (solo pedimentos cargados en Glosa)
        /// </summary>
        private List<ReporteIGIPagado> GenerarReporteIGIConGlosa(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultados = new List<ReporteIGIPagado>();

            try
            {
                var conexion = ObtenerConexionParaBaseDatos(baseDatos);

                string sql = @"
                    SELECT 
                        DI.Pim_Consecutivo AS iDPedimento,
                        Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento,
                        TR.Gl_FecPagoReal AS FechaPago,
                        TR.Gl_ImporteADvalorem AS IGI_Pagado,
                        SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,
                        TR.Gl_ImporteIVA AS IVA_Pagado,
                        TR.Gl_FPagoAdvalorem AS FormaPago_IGI,
                        TR.Gl_FPagoIVA AS FormaPago_IVA,
                        CASE 
                            WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
                            ELSE 'NO CARGADO'
                        END AS EstatusGlosa,
                        CASE 
                            WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
                            ELSE 'NO ZIP'
                        END AS EstatusOrigen
                    FROM Di_Pedimento DP
                    INNER JOIN Di_PedimentoDet DI
                        ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                    LEFT JOIN TR_GLOSA TR
                        ON TR.Gl_Pedimento = DP.Pim_Folio
                        AND TR.Gl_Aduana = DP.Adu_AduanaSecc
                        AND TR.Gl_Patente = DP.AgP_Patente
                        AND YEAR(IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago)) = YEAR(CONVERT(DATE,TR.Gl_FecPagoReal))
                        AND DI.Pid_Secuencia = TR.GL_SEC
                        AND TR.Gl_TOper = 1
                        AND TR.Gl_OrigenZipGlosa = 'S'
                    INNER JOIN Ca_Farancelaria FRA
                        ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
                        AND FRA.Pai_Clave = 'MEX' 
                        AND FRA.Fra_TipoOper = 0
                    WHERE 
                        CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
                        AND (
                            TR.Gl_FPagoIVA IN ('5','21') 
                            OR TR.Gl_FPagoAdvalorem IN ('0','21')
                        )
                    GROUP BY  
                        DI.Pim_Consecutivo,
                        Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento,
                        TR.Gl_FecPagoReal,
                        TR.Gl_ImporteADvalorem,
                        TR.Gl_ImporteIVA,
                        TR.Gl_FPagoAdvalorem,
                        TR.Gl_FPagoIVA,
                        CASE 
                            WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
                            ELSE 'NO CARGADO'
                        END,
                        CASE 
                            WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
                            ELSE 'NO ZIP'
                        END";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new SqlCommand(sql, cn);

                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                cn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // Crear un registro por cada partida/detalle
                    var reporte = new ReporteIGIPagado
                    {
                        BaseDatos = baseDatos,
                        IdPedimento = reader.GetInt32(0),
                        Pedimento = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        FechaPago = LeerFechaPago(reader, 2),
                        IGI_Pagado = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                        IGI_Calculado = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                        IVA_Pagado = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        FormaPago_IGI = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        FormaPago_IVA = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        EstatusGlosa = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        EstatusOrigen = reader.IsDBNull(9) ? "NO ZIP" : reader.GetString(9)
                    };

                    // Agregar cada registro (ya con IGI_Calculado desde el SUM del query)
                    resultados.Add(reporte);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte IGI para '{baseDatos}': {ex.Message}", ex);
            }

            return resultados;
        }

        /// <summary>
        /// Genera el reporte SIN validar contra TR_GLOSA (todos los pedimentos del cliente)
        /// </summary>
        private List<ReporteIGIPagado> GenerarReporteIGISinGlosa(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultados = new List<ReporteIGIPagado>();

            try
            {
                var conexion = ObtenerConexionParaBaseDatos(baseDatos);

                string sql = @"
                    SELECT 
                        DP.Pim_Consecutivo AS iDPedimento,
                        DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                        MAX(TR.Gl_FecPagoReal) AS FechaPago,
                        SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) AS IGI_Pagado,
                        SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,
                        SUM(ISNULL(TR.Gl_ImporteIVA,0)) AS IVA_Pagado,
                        MAX(TR.Gl_FPagoAdvalorem) AS FormaPago_IGI,
                        MAX(TR.Gl_FPagoIVA) AS FormaPago_IVA,
                        CASE 
                            WHEN MAX(TR.Gl_Pedimento) IS NOT NULL THEN 'SI CARGADO'
                            ELSE 'NO CARGADO'
                        END AS EstatusGlosa,
                        CASE 
                            WHEN MAX(TR.Gl_OrigenZipGlosa) = 'S' THEN 'ZIP'
                            ELSE 'NO ZIP'
                        END AS EstatusOrigen
                    FROM Di_Pedimento DP
                    INNER JOIN Di_PedimentoDet DI
                        ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                    LEFT JOIN TR_GLOSA TR
                        ON TR.Gl_Pedimento = DP.Pim_Folio
                        AND TR.Gl_Aduana = DP.Adu_AduanaSecc
                        AND TR.Gl_Patente = DP.AgP_Patente
                        AND YEAR(IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago)) = YEAR(CONVERT(DATE,TR.Gl_FecPagoReal))
                        AND DI.Pid_Secuencia = TR.GL_SEC
                        AND TR.Gl_TOper = 1
                        AND TR.Gl_OrigenZipGlosa = 'S'
                    INNER JOIN Ca_Farancelaria FRA
                        ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
                        AND FRA.Pai_Clave = 'MEX' 
                        AND FRA.Fra_TipoOper = 0
                    WHERE 
                        CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
                        AND (
                            TR.Gl_FPagoIVA IN ('5','21') 
                            OR TR.Gl_FPagoAdvalorem IN ('5','21')
                        )
                    GROUP BY  
                        DP.Pim_Consecutivo,
                        DP.Adu_AduanaSecc,
                        DP.AgP_Patente,
                        DP.Pim_Folio";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new SqlCommand(sql, cn);

                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                cn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var reporte = new ReporteIGIPagado
                    {
                        BaseDatos = baseDatos,
                        IdPedimento = reader.GetInt32(0),
                        Pedimento = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        FechaPago = LeerFechaPago(reader, 2),
                        IGI_Pagado = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                        IGI_Calculado = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                        IVA_Pagado = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        FormaPago_IGI = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        FormaPago_IVA = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        EstatusGlosa = reader.IsDBNull(8) ? "NO CARGADO" : reader.GetString(8),
                        EstatusOrigen = reader.IsDBNull(9) ? "NO ZIP" : reader.GetString(9)
                    };

                    resultados.Add(reporte);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte IGI sin Glosa para '{baseDatos}': {ex.Message}", ex);
            }

            return resultados;
        }


        /// <summary>
        /// Genera el resumen del reporte
        /// </summary>
        public ResumenIGI GenerarResumen(List<ReporteIGIPagado> reportes)
        {
            if (reportes == null || !reportes.Any())
            {
                return new ResumenIGI();
            }

            return new ResumenIGI
            {
                TotalIGI_Pagado = reportes.Sum(r => r.IGI_Pagado),
                TotalIGI_Calculado = reportes.Sum(r => r.IGI_Calculado),
                TotalIVA_Pagado = reportes.Sum(r => r.IVA_Pagado),
                TotalPedimentos = reportes.Count,
                PedimentosCargadosGlosa = reportes.Count(r => r.EstatusGlosa == "SI CARGADO")
            };
        }

        /// <summary>
        /// Obtiene datos con JOIN cruzado: TR_GLOSA de una base, Di_Pedimento de otra
        /// Soporta servidores diferentes usando estrategia de consultas separadas
        /// </summary>
        private List<DatoDetalleIGI> ObtenerDatosDetalleConJoinCruzado(
            string baseDatosPedimentos, 
            string baseDatosGlosa, 
            DateTime fechaInicio, 
            DateTime fechaFin,
            Conexion conexionPedimentos,
            Conexion conexionGlosa)
        {
            var datosDetalle = new List<DatoDetalleIGI>();

            try
            {
                // Obtener información de conexión para validar servidores
                var conexionInfoPedimentos = ObtenerConexionExterna(baseDatosPedimentos);
                var conexionInfoGlosa = ObtenerConexionExterna(baseDatosGlosa);

                // Determinar servidor de cada base
                string servidorPedimentos = conexionInfoPedimentos.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoPedimentos.Servidor)
                    ? conexionInfoPedimentos.Servidor
                    : conexionPrincipal.Servidor ?? string.Empty;

                string servidorGlosa = conexionInfoGlosa.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoGlosa.Servidor)
                    ? conexionInfoGlosa.Servidor
                    : conexionPrincipal.Servidor ?? string.Empty;

                // Validar si están en el mismo servidor
                bool mismoServidor = ValidarSiMismaConexion(
                    servidorPedimentos,
                    servidorGlosa,
                    conexionInfoPedimentos.IdConexion,
                    conexionInfoGlosa.IdConexion
                );

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n🔍 VALIDACIÓN IGI - CONEXIONES:");
                System.Diagnostics.Debug.WriteLine($"   Base Pedimentos: {baseDatosPedimentos}");
                System.Diagnostics.Debug.WriteLine($"   Servidor Pedimentos: {servidorPedimentos} (IdConexion: {conexionInfoPedimentos.IdConexion})");
                System.Diagnostics.Debug.WriteLine($"   Base Glosa: {baseDatosGlosa}");
                System.Diagnostics.Debug.WriteLine($"   Servidor Glosa: {servidorGlosa} (IdConexion: {conexionInfoGlosa.IdConexion})");
                System.Diagnostics.Debug.WriteLine($"   ¿Mismo servidor?: {(mismoServidor ? "SÍ" : "NO")}");
#endif

                if (mismoServidor)
                {
                    // JOIN directo entre bases en el mismo servidor
                    datosDetalle = ObtenerDatosConJoinDirecto(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos);
                }
                else
                {
                    // Estrategia de consultas separadas para servidores diferentes
                    datosDetalle = ObtenerDatosConConsultasSeparadas(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener datos con JOIN cruzado entre {baseDatosPedimentos} y {baseDatosGlosa}: {ex.Message}", ex);
            }

            return datosDetalle;
        }

        /// <summary>
        /// Obtiene datos usando JOIN directo cuando las bases están en el mismo servidor
        /// </summary>
        private List<DatoDetalleIGI> ObtenerDatosConJoinDirecto(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos)
        {
            var datosDetalle = new List<DatoDetalleIGI>();

            string sql = $@"
                SELECT 
                    DP.Pim_Consecutivo,
                    DP.Adu_AduanaSecc,
                    DP.AgP_Patente,
                    DP.Pim_Folio,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) AS Pim_FechaPago,
                    TR.Gl_FecPagoReal,
                    ISNULL(TR.Gl_ImporteADvalorem, 0) AS Gl_ImporteADvalorem,
                    ISNULL(ROUND((ISNULL(DI.Pid_ValorAdu, 0) * ISNULL(FRA.Fra_AdvGral, 0)) / 100, 0), 0) AS IGI_Calculado_Detalle,
                    ISNULL(TR.Gl_ImporteIVA, 0) AS Gl_ImporteIVA,
                    TR.Gl_FPagoAdvalorem,
                    TR.Gl_FPagoIVA,
                    TR.Gl_Pedimento,
                    TR.Gl_OrigenZipGlosa
                FROM [{baseDatosPedimentos}].dbo.Di_Pedimento DP
                INNER JOIN [{baseDatosPedimentos}].dbo.Di_PedimentoDet DI
                    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                LEFT JOIN [{baseDatosGlosa}].dbo.TR_GLOSA TR
                    ON TR.Gl_Pedimento = DP.Pim_Folio
                    AND TR.Gl_Aduana = DP.Adu_AduanaSecc
                    AND TR.Gl_Patente = DP.AgP_Patente
                    AND YEAR(IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) = YEAR(CONVERT(DATE, TR.Gl_FecPagoReal))
                    AND DI.Pid_Secuencia = TR.GL_SEC
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                INNER JOIN [{baseDatosPedimentos}].dbo.Ca_Farancelaria FRA
                    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion, 2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
                    AND FRA.Pai_Clave = 'MEX'
                    AND FRA.Fra_TipoOper = 0
                WHERE 
                    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin";

            using var cn = conexionPedimentos.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var dato = new DatoDetalleIGI
                {
                    BaseDatos = baseDatosPedimentos,
                    Pim_Consecutivo = reader.GetInt32(0),
                    Adu_AduanaSecc = reader.GetString(1),
                    AgP_Patente = reader.GetString(2),
                    Pim_Folio = reader.GetString(3),
                    Pim_FechaPago = LeerFechaPago(reader, 4),
                    Gl_FecPagoReal = LeerFechaPago(reader, 5),
                    Gl_ImporteADvalorem = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                    IGI_CalculadoDetalle = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                    Gl_ImporteIVA = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                    Gl_FPagoAdvalorem = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Gl_FPagoIVA = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Gl_Pedimento = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Gl_OrigenZipGlosa = reader.IsDBNull(12) ? null : reader.GetString(12)
                };

                datosDetalle.Add(dato);
            }

            return datosDetalle;
        }

        /// <summary>
        /// Obtiene datos usando consultas separadas cuando las bases están en servidores diferentes
        /// Similar a la estrategia usada en RetornoService para validación multi-servidor
        /// </summary>
        private List<DatoDetalleIGI> ObtenerDatosConConsultasSeparadas(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos,
            Conexion conexionGlosa)
        {
            var datosDetalle = new List<DatoDetalleIGI>();

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"\n🔀 IGI - VALIDACIÓN MULTI-SERVIDOR");
            System.Diagnostics.Debug.WriteLine($"   📌 Estrategia: Consultas separadas + validación en memoria");
#endif

            // PASO 1: Obtener pedimentos de la base seleccionada
            var pedimentosBase = new List<(int Consecutivo, string Aduana, string Patente, string Folio, DateTime? FechaPago)>();

            string sqlPedimentos = $@"
                SELECT DISTINCT
                    DP.Pim_Consecutivo,
                    DP.Adu_AduanaSecc,
                    DP.AgP_Patente,
                    DP.Pim_Folio,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) AS Pim_FechaPago
                FROM Di_Pedimento DP
                WHERE CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin";

            using (var cnPedimentos = conexionPedimentos.ObtenerConexion())
            using (var cmdPedimentos = new SqlCommand(sqlPedimentos, cnPedimentos))
            {
                cmdPedimentos.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmdPedimentos.Parameters.AddWithValue("@FechaFin", fechaFin);

                cnPedimentos.Open();

                using var readerPedimentos = cmdPedimentos.ExecuteReader();
                while (readerPedimentos.Read())
                {
                    pedimentosBase.Add((
                        readerPedimentos.GetInt32(0),
                        readerPedimentos.GetString(1),
                        readerPedimentos.GetString(2),
                        readerPedimentos.GetString(3),
                        LeerFechaPago(readerPedimentos, 4)
                    ));
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   📋 Pedimentos encontrados en {baseDatosPedimentos}: {pedimentosBase.Count}");
#endif

            if (!pedimentosBase.Any())
            {
                return datosDetalle;
            }

            // PASO 2: Procesar en lotes y obtener detalles con validación contra TR_GLOSA
            const int tamañoLote = 50;
            int totalLotes = (int)Math.Ceiling(pedimentosBase.Count / (double)tamañoLote);

            for (int i = 0; i < totalLotes; i++)
            {
                var lote = pedimentosBase.Skip(i * tamañoLote).Take(tamañoLote).ToList();

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"   🔍 Procesando lote {i + 1}/{totalLotes} ({lote.Count} pedimentos)");
#endif

                // Por cada pedimento del lote, obtener sus detalles y validar contra TR_GLOSA
                foreach (var pedimento in lote)
                {
                    try
                    {
                        // Obtener detalles del pedimento con fracción arancelaria
                        var detallesPedimento = ObtenerDetallesPedimento(
                            baseDatosPedimentos,
                            pedimento.Consecutivo,
                            conexionPedimentos
                        );

                        // Validar contra TR_GLOSA
                        var datosGlosa = ObtenerDatosGlosaParaPedimento(
                            baseDatosGlosa,
                            pedimento.Aduana,
                            pedimento.Patente,
                            pedimento.Folio,
                            pedimento.FechaPago,
                            conexionGlosa
                        );

                        // Combinar datos
                        foreach (var detalle in detallesPedimento)
                        {
                            var datoGlosa = datosGlosa.FirstOrDefault(g => g.Secuencia == detalle.Secuencia);
                            bool tieneGlosa = datoGlosa.Secuencia != 0; // Si Secuencia es 0, no se encontró

                            var datoDetalle = new DatoDetalleIGI
                            {
                                BaseDatos = baseDatosPedimentos,
                                Pim_Consecutivo = pedimento.Consecutivo,
                                Adu_AduanaSecc = pedimento.Aduana,
                                AgP_Patente = pedimento.Patente,
                                Pim_Folio = pedimento.Folio,
                                Pim_FechaPago = pedimento.FechaPago,
                                Gl_FecPagoReal = tieneGlosa ? datoGlosa.FechaPago : null,
                                Gl_ImporteADvalorem = tieneGlosa ? datoGlosa.ImporteADvalorem : 0,
                                IGI_CalculadoDetalle = detalle.IGI_Calculado,
                                Gl_ImporteIVA = tieneGlosa ? datoGlosa.ImporteIVA : 0,
                                Gl_FPagoAdvalorem = tieneGlosa ? datoGlosa.FormaPagoIGI : string.Empty,
                                Gl_FPagoIVA = tieneGlosa ? datoGlosa.FormaPagoIVA : string.Empty,
                                Gl_Pedimento = tieneGlosa ? datoGlosa.Pedimento : null,
                                Gl_OrigenZipGlosa = tieneGlosa ? datoGlosa.OrigenZip : null
                            };

                            datosDetalle.Add(datoDetalle);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"   ⚠️ Error procesando pedimento {pedimento.Folio}: {ex.Message}");
                    }
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ✅ Total registros detalle obtenidos: {datosDetalle.Count}\n");
#endif

            return datosDetalle;
        }

        /// <summary>
        /// Obtiene los detalles de un pedimento con cálculo de IGI
        /// </summary>
        private List<(int Secuencia, decimal IGI_Calculado)> ObtenerDetallesPedimento(
            string baseDatos,
            int consecutivo,
            Conexion conexion)
        {
            var detalles = new List<(int Secuencia, decimal IGI_Calculado)>();

            string sql = $@"
                SELECT 
                    DI.Pid_Secuencia,
                    ISNULL(ROUND((ISNULL(DI.Pid_ValorAdu, 0) * ISNULL(FRA.Fra_AdvGral, 0)) / 100, 0), 0) AS IGI_Calculado
                FROM Di_PedimentoDet DI
                INNER JOIN Ca_Farancelaria FRA
                    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion, 2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
                    AND FRA.Pai_Clave = 'MEX'
                    AND FRA.Fra_TipoOper = 0
                WHERE DI.Pim_Consecutivo = @Consecutivo";

            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Consecutivo", consecutivo);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                detalles.Add((
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                ));
            }

            return detalles;
        }

        /// <summary>
        /// Obtiene datos de TR_GLOSA para un pedimento específico
        /// </summary>
        private List<(int Secuencia, DateTime? FechaPago, decimal ImporteADvalorem, decimal ImporteIVA, string FormaPagoIGI, string FormaPagoIVA, string Pedimento, string OrigenZip)> ObtenerDatosGlosaParaPedimento(
            string baseDatosGlosa,
            string aduana,
            string patente,
            string folio,
            DateTime? fechaPago,
            Conexion conexionGlosa)
        {
            var datosGlosa = new List<(int Secuencia, DateTime? FechaPago, decimal ImporteADvalorem, decimal ImporteIVA, string FormaPagoIGI, string FormaPagoIVA, string Pedimento, string OrigenZip)>();

            string sql = $@"
                SELECT 
                    TR.GL_SEC,
                    TR.Gl_FecPagoReal,
                    ISNULL(TR.Gl_ImporteADvalorem, 0) AS Gl_ImporteADvalorem,
                    ISNULL(TR.Gl_ImporteIVA, 0) AS Gl_ImporteIVA,
                    TR.Gl_FPagoAdvalorem,
                    TR.Gl_FPagoIVA,
                    TR.Gl_Pedimento,
                    TR.Gl_OrigenZipGlosa
                FROM TR_GLOSA TR
                WHERE TR.Gl_Pedimento = @Folio
                    AND TR.Gl_Aduana = @Aduana
                    AND TR.Gl_Patente = @Patente
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                    AND (@FechaPago IS NULL OR YEAR(CONVERT(DATE, TR.Gl_FecPagoReal)) = YEAR(@FechaPago))";

            using var cn = conexionGlosa.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Folio", folio);
            cmd.Parameters.AddWithValue("@Aduana", aduana);
            cmd.Parameters.AddWithValue("@Patente", patente);
            cmd.Parameters.AddWithValue("@FechaPago", fechaPago.HasValue ? (object)fechaPago.Value : DBNull.Value);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                datosGlosa.Add((
                    reader.GetInt32(0),
                    LeerFechaPago(reader, 1),
                    reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                    reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                ));
            }

            return datosGlosa;
        }

        /// <summary>
        /// Valida si dos conexiones apuntan al mismo servidor
        /// </summary>
        private bool ValidarSiMismaConexion(string servidor1, string servidor2, int? idConexion1, int? idConexion2)
        {
            // Si tienen el mismo IdConexion (y no es null), son la misma conexión
            if (idConexion1.HasValue && idConexion2.HasValue && idConexion1 == idConexion2)
            {
                return true;
            }

            // Si ambos tienen IdConexion NULL, usan conexión principal (mismo servidor)
            if (!idConexion1.HasValue && !idConexion2.HasValue)
            {
                return true;
            }

            // Comparar nombres de servidor (normalizando)
            string srv1 = NormalizarNombreServidor(servidor1);
            string srv2 = NormalizarNombreServidor(servidor2);

            return srv1.Equals(srv2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normaliza el nombre del servidor para comparación
        /// </summary>
        private string NormalizarNombreServidor(string servidor)
        {
            if (string.IsNullOrWhiteSpace(servidor))
                return string.Empty;

            // Remover espacios
            servidor = servidor.Trim();

            // Convertir localhost a IP
            if (servidor.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                servidor.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
                servidor.Equals(".", StringComparison.OrdinalIgnoreCase))
            {
                return "127.0.0.1";
            }

            return servidor;
        }

        /// <summary>
        /// Lee el campo de fecha de pago manejando tanto DateTime como varchar
        /// </summary>
        private DateTime? LeerFechaPago(SqlDataReader reader, int columnIndex)
        {
            if (reader.IsDBNull(columnIndex))
                return null;

            try
            {
                // Intentar leer como DateTime directamente
                return reader.GetDateTime(columnIndex);
            }
            catch (InvalidCastException)
            {
                // Si falla, intentar leer como string y convertir
                try
                {
                    string fechaStr = reader.GetString(columnIndex);
                    if (DateTime.TryParse(fechaStr, out DateTime fecha))
                    {
                        return fecha;
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Agrupa todos los datos detalle por pedimento (de todas las bases de datos)
        /// </summary>
        private List<ReporteIGIPagado> AgruparDatosPorPedimento(List<DatoDetalleIGI> datosDetalle)
        {
            var resultados = new List<ReporteIGIPagado>();

            try
            {
                // Agrupar por pedimento completo (puede venir de múltiples bases)
                var grupos = datosDetalle
                    .GroupBy(d => new 
                    { 
                        d.Pim_Consecutivo,
                        d.Adu_AduanaSecc,
                        d.AgP_Patente,
                        d.Pim_Folio,
                        PedimentoKey = d.PedimentoCompleto
                    });

                foreach (var grupo in grupos)
                {
                    var primeraBase = grupo.First().BaseDatos;

                    // Usar fecha de glosa si existe, sino usar fecha del pedimento
                    var fechaPago = grupo.Where(g => g.Gl_FecPagoReal.HasValue)
                                        .Select(g => g.Gl_FecPagoReal)
                                        .FirstOrDefault()
                                    ?? grupo.Where(g => g.Pim_FechaPago.HasValue)
                                            .Select(g => g.Pim_FechaPago)
                                            .FirstOrDefault();

                    // Sumar todos los valores del grupo
                    var reporte = new ReporteIGIPagado
                    {
                        BaseDatos = primeraBase,
                        IdPedimento = grupo.Key.Pim_Consecutivo,
                        Pedimento = grupo.Key.PedimentoKey,
                        FechaPago = fechaPago,
                        IGI_Pagado = grupo.Sum(g => g.Gl_ImporteADvalorem),
                        IGI_Calculado = grupo.Sum(g => g.IGI_CalculadoDetalle),
                        IVA_Pagado = grupo.Sum(g => g.Gl_ImporteIVA),
                        FormaPago_IGI = grupo.Where(g => !string.IsNullOrEmpty(g.Gl_FPagoAdvalorem))
                                            .Select(g => g.Gl_FPagoAdvalorem)
                                            .FirstOrDefault() ?? string.Empty,
                        FormaPago_IVA = grupo.Where(g => !string.IsNullOrEmpty(g.Gl_FPagoIVA))
                                            .Select(g => g.Gl_FPagoIVA)
                                            .FirstOrDefault() ?? string.Empty,
                        EstatusGlosa = grupo.Any(g => !string.IsNullOrEmpty(g.Gl_Pedimento)) ? "SI CARGADO" : "NO CARGADO",
                        EstatusOrigen = grupo.Any(g => g.Gl_OrigenZipGlosa == "S") ? "ZIP" : "NO ZIP"
                    };

                    resultados.Add(reporte);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agrupar datos por pedimento: {ex.Message}", ex);
            }

            return resultados;
        }


        /// <summary>
        /// Exporta el reporte a DataTable para uso en DataGridView
        /// </summary>
        public System.Data.DataTable ConvertirADataTable(List<ReporteIGIPagado> reportes)
        {
            var dt = new System.Data.DataTable();

            dt.Columns.Add("Base Datos", typeof(string));
            dt.Columns.Add("ID Pedimento", typeof(int));
            dt.Columns.Add("Pedimento", typeof(string));
            dt.Columns.Add("Fecha Pago", typeof(DateTime));
            dt.Columns.Add("IGI Pagado", typeof(decimal));
            dt.Columns.Add("IGI Calculado", typeof(decimal));
            dt.Columns.Add("Diferencia IGI", typeof(decimal));
            dt.Columns.Add("IVA Pagado", typeof(decimal));
            dt.Columns.Add("Forma Pago IGI", typeof(string));
            dt.Columns.Add("Forma Pago IVA", typeof(string));
            dt.Columns.Add("Estatus Glosa", typeof(string));

            foreach (var reporte in reportes)
            {
                dt.Rows.Add(
                    reporte.BaseDatos,
                    reporte.IdPedimento,
                    reporte.Pedimento,
                    reporte.FechaPago ?? (object)DBNull.Value,
                    reporte.IGI_Pagado,
                    reporte.IGI_Calculado,
                    reporte.DiferenciaIGI,
                    reporte.IVA_Pagado,
                    reporte.FormaPago_IGI,
                    reporte.FormaPago_IVA,
                    reporte.EstatusGlosa
                );
            }

            return dt;
        }
    }
}
