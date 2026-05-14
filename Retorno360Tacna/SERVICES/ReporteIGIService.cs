using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Servicio especializado para reportes de IGI Pagado
    /// Hereda de ReporteServiceBase para reutilizar lógica de conexiones
    /// </summary>
    public partial class ReporteIGIService : ReporteServiceBase
    {
        public ReporteIGIService(ConexionInfo conexion) : base(conexion)
        {
        }

        /// <summary>
        /// Genera el reporte de IGI Pagado para una base de datos específica
        /// Usa exclusivamente el método optimizado con tablas temporales
        /// </summary>
        public List<ReporteIGIPagado> GenerarReporteIGI(string baseDatos, DateTime fechaInicio, DateTime fechaFin, bool sinValidacionGlosa = false)
        {
            var resultados = new List<ReporteIGIPagado>();

            try
            {
                // Paso 1: Obtener el IdRazon desde la base de datos seleccionada
                int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);

                // Paso 2: Obtener la razón social y su base de TR_GLOSA
                var razonSocial = ObtenerRazonSocial(idRazon);
                string baseDatosGlosa = razonSocial.BaseDatosOrigen;

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n>> GenerarReporteIGI - MÉTODO OPTIMIZADO CON TABLAS TEMPORALES:");
                System.Diagnostics.Debug.WriteLine($"   >> Base Pedimentos seleccionada: {baseDatos}");
                System.Diagnostics.Debug.WriteLine($"   >> IdRazon obtenido: {idRazon}");
                System.Diagnostics.Debug.WriteLine($"   >> Razón Social: {razonSocial.NombreRazon}");
                System.Diagnostics.Debug.WriteLine($"   >> Base TR_GLOSA: {baseDatosGlosa}");
#endif

                // Paso 3: Obtener conexiones para ambas bases
                var conexionPedimentos = ObtenerConexionParaBaseDatos(baseDatos);
                var conexionGlosa = ObtenerConexionParaBaseDatos(baseDatosGlosa);

                // Paso 4: Ejecutar query optimizado con tablas temporales
                resultados = ObtenerDatosAgrupadosConJoinCruzado(
                    baseDatos,
                    baseDatosGlosa,
                    fechaInicio,
                    fechaFin,
                    conexionPedimentos,
                    conexionGlosa
                );

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"   >> Total de registros obtenidos: {resultados.Count}\n");
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte IGI para '{baseDatos}': {ex.Message}", ex);
            }

            return resultados;
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

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n?? GenerarReporteIGIPorRazonSocial - NUEVA LÓGICA CON GROUP BY:");
                System.Diagnostics.Debug.WriteLine($"   Razón Social: {razonSocial.NombreRazon}");
                System.Diagnostics.Debug.WriteLine($"   Base de datos TR_GLOSA: {baseDatosGlosa}");
#endif

                // Paso 2: Obtener todas las bases de datos con su información de conexión
                var basesDatosConConexion = ObtenerBasesDatosConConexion(idRazon);

                if (!basesDatosConConexion.Any())
                {
                    throw new Exception("No se encontraron bases de datos para la razón social seleccionada.");
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"   Total bases de datos: {basesDatosConConexion.Count}");
#endif

                // Paso 3: Para cada base de datos, ejecutar el query con GROUP BY directamente
                foreach (var conexionInfo in basesDatosConConexion)
                {
                    try
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"\n   ?? Procesando base: {conexionInfo.BaseDatos}");
#endif

                        // Obtener la conexión apropiada para la base de pedimentos
                        // Usando el método correcto que resuelve conexiones externas
                        var conexionPedimentos = ObtenerConexionParaBaseDatos(conexionInfo.BaseDatos);

                        // Obtener conexión para la base de TR_GLOSA
                        var conexionGlosa = ObtenerConexionParaBaseDatos(baseDatosGlosa);

                        // ? NUEVA LÓGICA: Ejecutar GROUP BY directamente en cada base
                        var resultadosBase = ObtenerDatosAgrupadosConJoinCruzado(
                            conexionInfo.BaseDatos,      // Base de Di_Pedimento
                            baseDatosGlosa,              // Base de TR_GLOSA
                            fechaInicio,
                            fechaFin,
                            conexionPedimentos,
                            conexionGlosa
                        );

#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"      ? Pedimentos agrupados: {resultadosBase.Count}");
#endif

                        resultados.AddRange(resultadosBase);
                    }
                    catch (Exception ex)
                    {
                        // Log detallado del error pero continuar con las demás bases
                        var mensajeError = $"Error consultando {conexionInfo.BaseDatos}: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"      ?? {mensajeError}");
                        System.Diagnostics.Debug.WriteLine($"      StackTrace: {ex.StackTrace}");
                    }
                }

                if (!resultados.Any())
                {
                    throw new Exception("No se encontraron registros en ninguna base de datos de la razón social.");
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n   ? Total de pedimentos consolidados: {resultados.Count}\n");
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar reporte por razón social: {ex.Message}", ex);
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

            // ? VARIABLES PARA ALMACENAR INFORMACI?N DE SERVIDORES PARA EL JOIN
            string servidorBasePedimentos = string.Empty;
            string servidorBaseTRGlosa = string.Empty;
            string usuarioBasePedimentos = string.Empty;
            string usuarioBaseTRGlosa = string.Empty;
            string nombreBasePedimentos = baseDatosPedimentos;
            string nombreBaseTRGlosa = baseDatosGlosa;

            try
            {
                // PASO 1: Obtener información de conexión para ambas bases
                var conexionInfoPedimentos = ObtenerConexionExterna(baseDatosPedimentos);
                var conexionInfoGlosa = ObtenerConexionExterna(baseDatosGlosa);

                // PASO 2: Determinar de qué servidor viene cada base de datos
                string servidorPedimentos = conexionInfoPedimentos.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoPedimentos.Servidor)
                    ? conexionInfoPedimentos.Servidor
                    : conexionPrincipal.Servidor ?? string.Empty;

                string servidorGlosa = conexionInfoGlosa.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoGlosa.Servidor)
                    ? conexionInfoGlosa.Servidor
                    : conexionPrincipal.Servidor ?? string.Empty;

                // PASO 3: Determinar las credenciales para cada base
                string usuarioPedimentos = conexionInfoPedimentos.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoPedimentos.UsuarioSQL)
                    ? conexionInfoPedimentos.UsuarioSQL
                    : conexionPrincipal.UsuarioSQL ?? string.Empty;

                string usuarioGlosa = conexionInfoGlosa.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoGlosa.UsuarioSQL)
                    ? conexionInfoGlosa.UsuarioSQL
                    : conexionPrincipal.UsuarioSQL ?? string.Empty;

                // ? GUARDAR EN VARIABLES PARA USO POSTERIOR EN EL JOIN
                servidorBasePedimentos = servidorPedimentos;
                servidorBaseTRGlosa = servidorGlosa;
                usuarioBasePedimentos = usuarioPedimentos;
                usuarioBaseTRGlosa = usuarioGlosa;

                // PASO 4: Validar si están en el mismo servidor
                bool mismoServidor = ValidarSiMismaConexion(
                    servidorPedimentos,
                    servidorGlosa,
                    conexionInfoPedimentos.IdConexion,
                    conexionInfoGlosa.IdConexion
                );

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n?? VALIDACIÓN IGI - ANÁLISIS DE CONEXIONES:");
                System.Diagnostics.Debug.WriteLine($"\n   ?? BASE DE PEDIMENTOS SELECCIONADA: {nombreBasePedimentos}");
                System.Diagnostics.Debug.WriteLine($"      +- Servidor: {servidorBasePedimentos}");
                System.Diagnostics.Debug.WriteLine($"      +- Usuario SQL: {usuarioBasePedimentos}");
                System.Diagnostics.Debug.WriteLine($"      +- IdConexion: {(conexionInfoPedimentos.IdConexion?.ToString() ?? "NULL (usa conexión principal)")}");
                System.Diagnostics.Debug.WriteLine($"      +- ConnectionString: {conexionPedimentos.GetConnectionString()}");

                System.Diagnostics.Debug.WriteLine($"\n   ?? BASE DE TR_GLOSA DE LA RAZÓN: {nombreBaseTRGlosa}");
                System.Diagnostics.Debug.WriteLine($"      +- Servidor: {servidorBaseTRGlosa}");
                System.Diagnostics.Debug.WriteLine($"      +- Usuario SQL: {usuarioBaseTRGlosa}");
                System.Diagnostics.Debug.WriteLine($"      +- IdConexion: {(conexionInfoGlosa.IdConexion?.ToString() ?? "NULL (usa conexión principal)")}");
                System.Diagnostics.Debug.WriteLine($"      +- ConnectionString: {conexionGlosa.GetConnectionString()}");

                System.Diagnostics.Debug.WriteLine($"\n   ?? ANÁLISIS DE SERVIDORES:");
                System.Diagnostics.Debug.WriteLine($"      +- ¿Mismo servidor?: {(mismoServidor ? "? SÍ" : "? NO")}");
                System.Diagnostics.Debug.WriteLine($"      +- ¿Mismo usuario?: {(usuarioBasePedimentos == usuarioBaseTRGlosa ? "? SÍ" : "? NO")}");
                                System.Diagnostics.Debug.WriteLine($"      +- Estrategia: {(mismoServidor ? "JOIN DIRECTO" : "CONSULTAS SEPARADAS")}");

                System.Diagnostics.Debug.WriteLine($"\n   ?? VARIABLES GUARDADAS PARA JOIN:");
                System.Diagnostics.Debug.WriteLine($"      +- Servidor Pedimentos: {servidorBasePedimentos}");
                System.Diagnostics.Debug.WriteLine($"      +- Servidor TR_Glosa: {servidorBaseTRGlosa}");
                System.Diagnostics.Debug.WriteLine($"      +- Usuario Pedimentos: {usuarioBasePedimentos}");
                System.Diagnostics.Debug.WriteLine($"      +- Usuario TR_Glosa: {usuarioBaseTRGlosa}");
#endif

                if (mismoServidor)
                {
                    // JOIN directo entre bases en el mismo servidor
                    datosDetalle = ObtenerDatosConJoinDirecto(
                        baseDatosPedimentos, 
                        baseDatosGlosa, 
                        fechaInicio, 
                        fechaFin, 
                        conexionPedimentos,
                        servidorPedimentos,
                        usuarioPedimentos
                    );
                }
                                else
                {
                    // Estrategia de consultas separadas para servidores diferentes
                    // Usando las variables guardadas de ambos servidores (conexionPedimentos y conexionGlosa)
                    datosDetalle = ObtenerDatosConConsultasSeparadas(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n   ? RESUMEN DEL JOIN:");
                System.Diagnostics.Debug.WriteLine($"      +- Registros obtenidos: {datosDetalle.Count}");
                System.Diagnostics.Debug.WriteLine($"      +- Base Pedimentos: [{nombreBasePedimentos}] en servidor [{servidorBasePedimentos}]");
                System.Diagnostics.Debug.WriteLine($"      +- Base TR_Glosa: [{nombreBaseTRGlosa}] en servidor [{servidorBaseTRGlosa}]");
#endif
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
            Conexion conexionPedimentos,
            string servidorPedimentos,
            string usuarioPedimentos)
        {
            var datosDetalle = new List<DatoDetalleIGI>();

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"\n?? EJECUTANDO JOIN DIRECTO:");
            System.Diagnostics.Debug.WriteLine($"   Servidor: {servidorPedimentos}");
            System.Diagnostics.Debug.WriteLine($"   Usuario: {usuarioPedimentos}");
            System.Diagnostics.Debug.WriteLine($"   Base Pedimentos: [{baseDatosPedimentos}]");
            System.Diagnostics.Debug.WriteLine($"   Base Glosa: [{baseDatosGlosa}]");
#endif

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

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"\n   ?? SQL JOIN DIRECTO:");
            System.Diagnostics.Debug.WriteLine($"   {sql.Substring(0, Math.Min(500, sql.Length))}...");
            System.Diagnostics.Debug.WriteLine($"\n   ? Abriendo conexión y ejecutando query...");
#endif

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
            System.Diagnostics.Debug.WriteLine($"\n?? IGI - VALIDACIÓN MULTI-SERVIDOR");
            System.Diagnostics.Debug.WriteLine($"   ?? Estrategia: Consultas separadas + validación en memoria");
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
            System.Diagnostics.Debug.WriteLine($"   ?? Pedimentos encontrados en {baseDatosPedimentos}: {pedimentosBase.Count}");
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
                System.Diagnostics.Debug.WriteLine($"   ?? Procesando lote {i + 1}/{totalLotes} ({lote.Count} pedimentos)");
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
                        System.Diagnostics.Debug.WriteLine($"   ?? Error procesando pedimento {pedimento.Folio}: {ex.Message}");
                    }
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ? Total registros detalle obtenidos: {datosDetalle.Count}\n");
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
        /// Obtiene datos AGRUPADOS usando JOIN cruzado - Versión optimizada con GROUP BY en SQL
        /// Ejecuta el GROUP BY directamente en cada base de datos (más eficiente)
        /// Similar a la lógica del checkbox de consulta por base individual
        /// </summary>
        private List<ReporteIGIPagado> ObtenerDatosAgrupadosConJoinCruzado(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos,
            Conexion conexionGlosa)
        {
            var resultados = new List<ReporteIGIPagado>();

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
                System.Diagnostics.Debug.WriteLine($"\n?? ObtenerDatosAgrupadosConJoinCruzado:");
                System.Diagnostics.Debug.WriteLine($"   Base Pedimentos: {baseDatosPedimentos} (Servidor: {servidorPedimentos})");
                System.Diagnostics.Debug.WriteLine($"   Base Glosa: {baseDatosGlosa} (Servidor: {servidorGlosa})");
                System.Diagnostics.Debug.WriteLine($"   ¿Mismo servidor?: {(mismoServidor ? "SÍ" : "NO")}");
#endif

                if (mismoServidor)
                {
                    // JOIN directo con GROUP BY en SQL (mismo servidor)
                    resultados = ObtenerDatosAgrupadosConJoinDirecto(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos);
                }
                else
                {
                    // JOIN multi-servidor: ejecutar GROUP BY en pedimentos y luego validar con glosa
                    resultados = ObtenerDatosAgrupadosMultiServidor(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener datos agrupados entre {baseDatosPedimentos} y {baseDatosGlosa}: {ex.Message}", ex);
            }

            return resultados;
        }

        /// <summary>
        /// Obtiene datos agrupados usando JOIN directo cuando las bases están en el mismo servidor
        /// Ejecuta el GROUP BY directamente en SQL para máxima eficiencia
        /// Utiliza tablas temporales para hacer INNER JOIN entre pedimentos del cliente y TR_GLOSA
        /// </summary>
        private List<ReporteIGIPagado> ObtenerDatosAgrupadosConJoinDirecto(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos)
        {
            var resultados = new List<ReporteIGIPagado>();

            // Nuevo query usando tablas temporales para obtener solo pedimentos que coinciden
            string sql = $@"
                DECLARE @PedimentosCLIENTE TABLE (
                    iDPedimento INT,
                    Pedimento NVARCHAR(MAX),
                    FechaPago DATE,
                    IGI_Calculado DECIMAL(18,2),
                    FormaPago_IGI NVARCHAR(MAX)
                )
                INSERT INTO @PedimentosCLIENTE(iDPedimento, Pedimento, FechaPago, IGI_Calculado, FormaPago_IGI)
                SELECT 
                    DP.Pim_Consecutivo AS iDPedimento,
                    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                    IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago) AS FechaPago,
                    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,
                    Di.FoP_Clave AS FormaPago_IGI
                FROM [{baseDatosPedimentos}].dbo.Di_Pedimento DP
                INNER JOIN [{baseDatosPedimentos}].dbo.Di_PedimentoDet DI ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                INNER JOIN [{baseDatosPedimentos}].dbo.Ca_Farancelaria FRA ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
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
                    Di.FoP_Clave

                DECLARE @PedimentosGLOSAIGI TABLE (
                    Pedimento NVARCHAR(MAX),
                    FechaPago DATE,
                    IGI_Pagado DECIMAL(18,2),
                    FormaPago_IGI NVARCHAR(MAX)
                )
                INSERT INTO @PedimentosGLOSAIGI(Pedimento, FechaPago, IGI_Pagado, FormaPago_IGI)
                SELECT 
                    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
                    CONVERT(DATE, TR.Gl_FecPagoReal) AS FechaPago,
                    SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) AS IGI_Pagado,
                    TR.Gl_FPagoAdvalorem AS FormaPago_IGI
                FROM [{baseDatosGlosa}].DBO.TR_GLOSA TR
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
                HAVING SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) > 0 

                DECLARE @PedimentosGLOSAIVA TABLE (
                    Pedimento NVARCHAR(MAX),
                    FechaPago DATE,
                    IVA_Pagado DECIMAL(18,2),
                    FormaPago_IVA NVARCHAR(MAX) 
                )
                INSERT INTO @PedimentosGLOSAIVA(Pedimento, FechaPago, IVA_Pagado, FormaPago_IVA)
                SELECT 
                    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
                    CONVERT(DATE, TR.Gl_FecPagoReal) AS FechaPago,
                    SUM(ISNULL(TR.Gl_ImporteIVA,0)) AS IVA_Pagado,
                    TR.Gl_FPagoIVA AS FormaPago_IVA
                FROM [{baseDatosGlosa}].DBO.TR_GLOSA TR
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

                /*TABLA DE ANALISIS DE IGI*/
                SELECT
                    0 AS iDPedimento,
                    '' AS Pedimento,
                    GLOSA.FechaPago,
                    SUM(GLOSA.IGI_Pagado) AS IGI_Pagado,
                    SUM(CLIENT.IGI_Calculado) AS IGI_Calculado,
                    CAST(0 AS DECIMAL(18,2)) AS IVA_Pagado,
                    GLOSA.FormaPago_IGI AS FormaPago_IGI,
                    '' AS FormaPago_IVA,
                    'SI CARGADO' AS EstatusGlosa,
                    'ZIP' AS EstatusOrigen
                FROM @PedimentosGLOSAIGI GLOSA
                INNER JOIN @PedimentosCLIENTE CLIENT
                    ON CLIENT.Pedimento = GLOSA.Pedimento
                    AND CLIENT.FormaPago_IGI = GLOSA.FormaPago_IGI
                GROUP BY
                    GLOSA.FechaPago,
                    GLOSA.FormaPago_IGI

                /*TABLA DE ANALISIS DE IVA*/
                SELECT
                    0 AS iDPedimento,
                    '' AS Pedimento,
                    GLOSA.FechaPago,
                    CAST(0 AS DECIMAL(18,2)) AS IGI_Pagado,
                    CAST(0 AS DECIMAL(18,2)) AS IGI_Calculado,
                    SUM(GLOSA.IVA_Pagado) AS IVA_Pagado,
                    '' AS FormaPago_IGI,
                    GLOSA.FormaPago_IVA AS FormaPago_IVA,
                    'SI CARGADO' AS EstatusGlosa,
                    'ZIP' AS EstatusOrigen
                FROM @PedimentosGLOSAIVA GLOSA
                INNER JOIN (SELECT DISTINCT Pedimento FROM @PedimentosCLIENTE) CLIENT
                    ON CLIENT.Pedimento = GLOSA.Pedimento
                GROUP BY
                    GLOSA.FechaPago,
                    GLOSA.FormaPago_IVA";

            using var cn = conexionPedimentos.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

            cn.Open();

            using var reader = cmd.ExecuteReader();

            // Leer primer result set (IGI)
            while (reader.Read())
            {
                var reporte = new ReporteIGIPagado
                {
                    BaseDatos = baseDatosPedimentos,
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

            // Avanzar al segundo result set (IVA)
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var reporte = new ReporteIGIPagado
                    {
                        BaseDatos = baseDatosPedimentos,
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

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ? Pedimentos agrupados obtenidos: {resultados.Count}");
#endif

            return resultados;
        }

        /// <summary>
        /// Obtiene datos agrupados para multi-servidor
        /// Primero agrupa en la base de pedimentos, luego valida con glosa
        /// Separa en dos consultas: una para IGI (con FormaPago_IGI) y otra para IVA (solo Pedimento)
        /// </summary>
        private List<ReporteIGIPagado> ObtenerDatosAgrupadosMultiServidor(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos,
            Conexion conexionGlosa)
        {
            var resultados = new List<ReporteIGIPagado>();

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ?? Ejecutando estrategia multi-servidor con GROUP BY...");
#endif

            // PASO 1: Obtener pedimentos ÚNICOS agrupados desde la base de pedimentos
            var pedimentosUnicos = new Dictionary<string, (int IdPedimento, DateTime? FechaPago)>();

            string sqlPedimentosUnicos = $@"
                SELECT DISTINCT
                    DP.Pim_Consecutivo AS iDPedimento,
                    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) AS FechaPago
                FROM Di_Pedimento DP
                WHERE 
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) BETWEEN @FechaInicio AND @FechaFin";

            using (var cnPedimentos = conexionPedimentos.ObtenerConexion())
            using (var cmdPedimentos = new SqlCommand(sqlPedimentosUnicos, cnPedimentos))
            {
                cmdPedimentos.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmdPedimentos.Parameters.AddWithValue("@FechaFin", fechaFin);

                cnPedimentos.Open();

                using var readerPedimentos = cmdPedimentos.ExecuteReader();
                while (readerPedimentos.Read())
                {
                    int idPedimento = readerPedimentos.GetInt32(0);
                    string pedimento = readerPedimentos.IsDBNull(1) ? string.Empty : readerPedimentos.GetString(1);
                    DateTime? fechaPago = LeerFechaPago(readerPedimentos, 2);

                    if (!pedimentosUnicos.ContainsKey(pedimento))
                    {
                        pedimentosUnicos[pedimento] = (idPedimento, fechaPago);
                    }
                }
            }

            // PASO 2: Obtener datos de IGI agrupados por pedimento + FormaPago_IGI
            var datosIGI = ObtenerDatosIGIAgrupadosMultiServidor(baseDatosPedimentos, fechaInicio, fechaFin, conexionPedimentos);

            // PASO 3: Para cada dato de IGI, buscar en glosa filtrando por FormaPago_IGI
            foreach (var datoIGI in datosIGI)
            {
                try
                {
                    var datosGlosaIGI = ObtenerDatosGlosaIGIAgrupadosParaPedimento(
                        baseDatosGlosa,
                        datoIGI.Aduana,
                        datoIGI.Patente,
                        datoIGI.Folio,
                        datoIGI.FechaPago,
                        datoIGI.FormaPago_IGI,
                        conexionGlosa
                    );

                    // Solo agregar si hay datos válidos de glosa IGI
                    if (datosGlosaIGI.FormaPago_IGI == datoIGI.FormaPago_IGI && !string.IsNullOrEmpty(datosGlosaIGI.Pedimento))
                    {
                        // Crear reporte con datos de IGI
                        var reporte = new ReporteIGIPagado
                        {
                            BaseDatos = baseDatosPedimentos,
                            IdPedimento = datoIGI.IdPedimento,
                            Pedimento = datoIGI.Pedimento,
                            FechaPago = datosGlosaIGI.FechaPago ?? datoIGI.FechaPago,
                            IGI_Pagado = datosGlosaIGI.IGI_Pagado,
                            IGI_Calculado = datoIGI.IGI_Calculado,
                            IVA_Pagado = 0,  // Se llenará en el paso 4
                            FormaPago_IGI = datosGlosaIGI.FormaPago_IGI,
                            FormaPago_IVA = string.Empty,  // Se llenará en el paso 4
                            EstatusGlosa = "SI CARGADO",
                            EstatusOrigen = datosGlosaIGI.OrigenZip == "S" ? "ZIP" : "NO ZIP"
                        };

                        resultados.Add(reporte);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"   ?? Error procesando IGI pedimento {datoIGI.Folio}: {ex.Message}");
                }
            }

            // PASO 4: Obtener datos de IVA agrupados (solo por pedimento, sin FormaPago_IGI)
            foreach (var kvp in pedimentosUnicos)
            {
                string pedimento = kvp.Key;
                int idPedimento = kvp.Value.IdPedimento;
                DateTime? fechaPago = kvp.Value.FechaPago;

                string[] partesPedimento = pedimento.Split('-');
                if (partesPedimento.Length != 3) continue;

                string aduana = partesPedimento[0];
                string patente = partesPedimento[1];
                string folio = partesPedimento[2];

                try
                {
                    var datosGlosaIVA = ObtenerDatosGlosaIVAAgrupadosParaPedimento(
                        baseDatosGlosa,
                        aduana,
                        patente,
                        folio,
                        fechaPago,
                        conexionGlosa
                    );

                    // Solo agregar si hay datos válidos de IVA
                    if (!string.IsNullOrEmpty(datosGlosaIVA.Pedimento) && datosGlosaIVA.IVA_Pagado > 0)
                    {
                        // Crear reporte con datos de IVA
                        var reporte = new ReporteIGIPagado
                        {
                            BaseDatos = baseDatosPedimentos,
                            IdPedimento = idPedimento,
                            Pedimento = pedimento,
                            FechaPago = datosGlosaIVA.FechaPago ?? fechaPago,
                            IGI_Pagado = 0,
                            IGI_Calculado = 0,
                            IVA_Pagado = datosGlosaIVA.IVA_Pagado,
                            FormaPago_IGI = string.Empty,
                            FormaPago_IVA = datosGlosaIVA.FormaPago_IVA,
                            EstatusGlosa = "SI CARGADO",
                            EstatusOrigen = datosGlosaIVA.OrigenZip == "S" ? "ZIP" : "NO ZIP"
                        };

                        resultados.Add(reporte);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"   ?? Error procesando IVA pedimento {folio}: {ex.Message}");
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ? Total registros (IGI + IVA): {resultados.Count}");
#endif

            return resultados;
        }

        /// <summary>
        /// Obtiene datos de IGI agrupados por pedimento + FormaPago_IGI
        /// </summary>
        private List<(int IdPedimento, string Pedimento, string Aduana, string Patente, string Folio, DateTime? FechaPago, decimal IGI_Calculado, string FormaPago_IGI)> 
            ObtenerDatosIGIAgrupadosMultiServidor(
            string baseDatosPedimentos,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos)
        {
            var datosIGI = new List<(int, string, string, string, string, DateTime?, decimal, string)>();

            string sqlIGI = $@"
                SELECT 
                    DP.Pim_Consecutivo AS iDPedimento,
                    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                    DP.Adu_AduanaSecc,
                    DP.AgP_Patente,
                    DP.Pim_Folio,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) AS FechaPago,
                    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100, 0)) AS IGI_Calculado,
                    DI.FoP_Clave AS FormaPago_IGI
                FROM Di_Pedimento DP
                INNER JOIN Di_PedimentoDet DI
                    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                INNER JOIN Ca_Farancelaria FRA
                    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion, 2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
                    AND FRA.Pai_Clave = 'MEX'
                    AND FRA.Fra_TipoOper = 0
                WHERE 
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) BETWEEN @FechaInicio AND @FechaFin
                GROUP BY  
                    DP.Pim_Consecutivo,
                    DP.Adu_AduanaSecc,
                    DP.AgP_Patente,
                    DP.Pim_Folio,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago),
                    DI.FoP_Clave";

            using var cn = conexionPedimentos.ObtenerConexion();
            using var cmd = new SqlCommand(sqlIGI, cn);

            cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                datosIGI.Add((
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    LeerFechaPago(reader, 5),
                    reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                    reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                ));
            }

            return datosIGI;
        }

        /// <summary>
        /// Obtiene datos de TR_GLOSA para IGI agrupados por pedimento específico
        /// Filtra por FormaPago_IGI para que coincida con el del cliente
        /// Solo permite formas de pago 0 y 5 para IGI (según query original)
        /// </summary>
        private (DateTime? FechaPago, decimal IGI_Pagado, string FormaPago_IGI, string Pedimento, string OrigenZip) 
            ObtenerDatosGlosaIGIAgrupadosParaPedimento(
            string baseDatosGlosa,
            string aduana,
            string patente,
            string folio,
            DateTime? fechaPago,
            string formaPagoIGI,
            Conexion conexionGlosa)
        {
            string sql = $@"
                SELECT 
                    MAX(TR.Gl_FecPagoReal) AS FechaPago,
                    SUM(ISNULL(TR.Gl_ImporteADvalorem, 0)) AS IGI_Pagado,
                    MAX(TR.Gl_FPagoAdvalorem) AS FormaPago_IGI,
                    MAX(TR.Gl_Pedimento) AS Pedimento,
                    MAX(TR.Gl_OrigenZipGlosa) AS OrigenZip
                FROM TR_GLOSA TR
                WHERE TR.Gl_Pedimento = @Folio
                    AND TR.Gl_Aduana = @Aduana
                    AND TR.Gl_Patente = @Patente
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                    AND TR.Gl_FPagoAdvalorem = @FormaPagoIGI
                    AND TR.Gl_FPagoAdvalorem IN ('0','5')
                    AND (@FechaPago IS NULL OR YEAR(CONVERT(DATE, TR.Gl_FecPagoReal)) = YEAR(@FechaPago))
                HAVING SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) > 0";

            using var cn = conexionGlosa.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Folio", folio);
            cmd.Parameters.AddWithValue("@Aduana", aduana);
            cmd.Parameters.AddWithValue("@Patente", patente);
            cmd.Parameters.AddWithValue("@FormaPagoIGI", formaPagoIGI);
            cmd.Parameters.AddWithValue("@FechaPago", fechaPago.HasValue ? (object)fechaPago.Value : DBNull.Value);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    LeerFechaPago(reader, 0),
                    reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                );
            }

            // Si no hay datos de glosa IGI, retornar valores vacíos
            return (null, 0, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Obtiene datos de TR_GLOSA para IVA agrupados por pedimento específico
        /// NO filtra por FormaPago, solo por pedimento (según query original)
        /// Solo permite formas de pago 0 y 21 para IVA (según query original)
        /// </summary>
        private (DateTime? FechaPago, decimal IVA_Pagado, string FormaPago_IVA, string Pedimento, string OrigenZip) 
            ObtenerDatosGlosaIVAAgrupadosParaPedimento(
            string baseDatosGlosa,
            string aduana,
            string patente,
            string folio,
            DateTime? fechaPago,
            Conexion conexionGlosa)
        {
            string sql = $@"
                SELECT 
                    MAX(TR.Gl_FecPagoReal) AS FechaPago,
                    SUM(ISNULL(TR.Gl_ImporteIVA, 0)) AS IVA_Pagado,
                    MAX(TR.Gl_FPagoIVA) AS FormaPago_IVA,
                    MAX(TR.Gl_Pedimento) AS Pedimento,
                    MAX(TR.Gl_OrigenZipGlosa) AS OrigenZip
                FROM TR_GLOSA TR
                WHERE TR.Gl_Pedimento = @Folio
                    AND TR.Gl_Aduana = @Aduana
                    AND TR.Gl_Patente = @Patente
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                    AND TR.Gl_FPagoIVA IN ('0','21')
                    AND (@FechaPago IS NULL OR YEAR(CONVERT(DATE, TR.Gl_FecPagoReal)) = YEAR(@FechaPago))
                HAVING SUM(ISNULL(TR.Gl_ImporteIVA,0)) > 0";

            using var cn = conexionGlosa.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Folio", folio);
            cmd.Parameters.AddWithValue("@Aduana", aduana);
            cmd.Parameters.AddWithValue("@Patente", patente);
            cmd.Parameters.AddWithValue("@FechaPago", fechaPago.HasValue ? (object)fechaPago.Value : DBNull.Value);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    LeerFechaPago(reader, 0),
                    reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                );
            }

            // Si no hay datos de glosa IVA, retornar valores vacíos
            return (null, 0, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Obtiene datos de TR_GLOSA AGRUPADOS para un pedimento específico
        /// Filtra por FormaPago_IGI para que coincida con el del cliente
        /// Solo permite formas de pago 0 y 5 para IGI (según query original)
        /// </summary>
        private (DateTime? FechaPago, decimal IGI_Pagado, decimal IVA_Pagado, string FormaPago_IGI, string FormaPago_IVA, string Pedimento, string OrigenZip) 
            ObtenerDatosGlosaAgrupadosParaPedimento(
            string baseDatosGlosa,
            string aduana,
            string patente,
            string folio,
            DateTime? fechaPago,
            string formaPagoIGI,  // Nuevo parámetro
            Conexion conexionGlosa)
        {
            string sql = $@"
                SELECT 
                    MAX(TR.Gl_FecPagoReal) AS FechaPago,
                    SUM(ISNULL(TR.Gl_ImporteADvalorem, 0)) AS IGI_Pagado,
                    SUM(ISNULL(TR.Gl_ImporteIVA, 0)) AS IVA_Pagado,
                    MAX(TR.Gl_FPagoAdvalorem) AS FormaPago_IGI,
                    MAX(TR.Gl_FPagoIVA) AS FormaPago_IVA,
                    MAX(TR.Gl_Pedimento) AS Pedimento,
                    MAX(TR.Gl_OrigenZipGlosa) AS OrigenZip
                FROM TR_GLOSA TR
                WHERE TR.Gl_Pedimento = @Folio
                    AND TR.Gl_Aduana = @Aduana
                    AND TR.Gl_Patente = @Patente
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                    AND TR.Gl_FPagoAdvalorem = @FormaPagoIGI
                    AND TR.Gl_FPagoAdvalorem IN ('0','5')
                    AND (@FechaPago IS NULL OR YEAR(CONVERT(DATE, TR.Gl_FecPagoReal)) = YEAR(@FechaPago))
                HAVING SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) > 0";

            using var cn = conexionGlosa.ObtenerConexion();
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Folio", folio);
            cmd.Parameters.AddWithValue("@Aduana", aduana);
            cmd.Parameters.AddWithValue("@Patente", patente);
            cmd.Parameters.AddWithValue("@FormaPagoIGI", formaPagoIGI);
            cmd.Parameters.AddWithValue("@FechaPago", fechaPago.HasValue ? (object)fechaPago.Value : DBNull.Value);

            cn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    LeerFechaPago(reader, 0),
                    reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                    reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                );
            }

            // Si no hay datos de glosa, retornar valores vacíos
            return (null, 0, 0, string.Empty, string.Empty, string.Empty, string.Empty);
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

        /// <summary>
        /// Exporta el reporte a DataTable organizado por formas de pago (5 y 0)
        /// Sin las columnas IdPedimento y Pedimento
        /// Con filas de totales separadas por forma de pago
        /// </summary>
        public System.Data.DataTable ConvertirADataTablePorFormaPago(List<ReporteIGIPagado> reportes)
        {
            var dt = new System.Data.DataTable();

            // Columnas (sin ID Pedimento ni Pedimento)
            dt.Columns.Add("Sección", typeof(string));           // Para identificar forma de pago 5 o 0
            dt.Columns.Add("Fecha Pago", typeof(DateTime));
            dt.Columns.Add("IGI Pagado", typeof(decimal));
            dt.Columns.Add("IGI Calculado", typeof(decimal));
            dt.Columns.Add("Diferencia IGI", typeof(decimal));
            dt.Columns.Add("IVA Pagado", typeof(decimal));
            dt.Columns.Add("Forma Pago IGI", typeof(string));
            dt.Columns.Add("Forma Pago IVA", typeof(string));

            // Separar reportes por forma de pago IGI
            var reportesFormaPago5 = reportes.Where(r => r.FormaPago_IGI == "5").OrderBy(r => r.FechaPago).ToList();
            var reportesFormaPago0 = reportes.Where(r => r.FormaPago_IGI == "0" || (r.FormaPago_IGI != "5" && r.FormaPago_IGI != "21")).OrderBy(r => r.FechaPago).ToList();

            // ========== SECCIÓN: FORMA DE PAGO 5 ==========
            if (reportesFormaPago5.Any())
            {
                // Encabezado de sección
                dt.Rows.Add("--- FORMA DE PAGO 5 ---", DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty);

                foreach (var reporte in reportesFormaPago5)
                {
                    dt.Rows.Add(
                        string.Empty, // Sección vacía para datos regulares
                        reporte.FechaPago ?? (object)DBNull.Value,
                        reporte.IGI_Pagado,
                        reporte.IGI_Calculado,
                        reporte.DiferenciaIGI,
                        reporte.IVA_Pagado,
                        reporte.FormaPago_IGI,
                        reporte.FormaPago_IVA
                    );
                }

                // Totales de forma de pago 5
                var totalIGI_Pagado5 = reportesFormaPago5.Sum(r => r.IGI_Pagado);
                var totalIGI_Calculado5 = reportesFormaPago5.Sum(r => r.IGI_Calculado);
                var totalDiferencia5 = reportesFormaPago5.Sum(r => r.DiferenciaIGI);
                var totalIVA5 = reportesFormaPago5.Sum(r => r.IVA_Pagado);

                dt.Rows.Add(
                    "TOTAL FORMA DE PAGO 5",
                    DBNull.Value,
                    totalIGI_Pagado5,
                    totalIGI_Calculado5,
                    totalDiferencia5,
                    totalIVA5,
                    string.Empty,
                    string.Empty
                );

                // Fila vacía de separación
                dt.Rows.Add(string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty);
            }

            // ========== SECCIÓN: FORMA DE PAGO 0 (u otras) ==========
            if (reportesFormaPago0.Any())
            {
                // Encabezado de sección
                dt.Rows.Add("--- FORMA DE PAGO 0 ---", DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty);

                foreach (var reporte in reportesFormaPago0)
                {
                    dt.Rows.Add(
                        string.Empty,
                        reporte.FechaPago ?? (object)DBNull.Value,
                        reporte.IGI_Pagado,
                        reporte.IGI_Calculado,
                        reporte.DiferenciaIGI,
                        reporte.IVA_Pagado,
                        reporte.FormaPago_IGI,
                        reporte.FormaPago_IVA
                    );
                }

                // Totales de forma de pago 0
                var totalIGI_Pagado0 = reportesFormaPago0.Sum(r => r.IGI_Pagado);
                var totalIGI_Calculado0 = reportesFormaPago0.Sum(r => r.IGI_Calculado);
                var totalDiferencia0 = reportesFormaPago0.Sum(r => r.DiferenciaIGI);
                var totalIVA0 = reportesFormaPago0.Sum(r => r.IVA_Pagado);

                dt.Rows.Add(
                    "TOTAL FORMA DE PAGO 0",
                    DBNull.Value,
                    totalIGI_Pagado0,
                    totalIGI_Calculado0,
                    totalDiferencia0,
                    totalIVA0,
                    string.Empty,
                    string.Empty
                );

                // Fila vacía de separación
                dt.Rows.Add(string.Empty, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, string.Empty, string.Empty);
            }

            // ========== TOTAL GENERAL ==========
            var totalIGI_PagadoGeneral = reportes.Sum(r => r.IGI_Pagado);
            var totalIGI_CalculadoGeneral = reportes.Sum(r => r.IGI_Calculado);
            var totalDiferenciaGeneral = reportes.Sum(r => r.DiferenciaIGI);
            var totalIVAGeneral = reportes.Sum(r => r.IVA_Pagado);

            dt.Rows.Add(
                "--- TOTAL GENERAL ---",
                DBNull.Value,
                totalIGI_PagadoGeneral,
                totalIGI_CalculadoGeneral,
                totalDiferenciaGeneral,
                totalIVAGeneral,
                string.Empty,
                string.Empty
            );

            return dt;
        }
    }
}




























