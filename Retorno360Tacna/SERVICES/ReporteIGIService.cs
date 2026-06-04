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
        /// Genera resumen por forma de pago IGI a partir del detalle por partidas
        /// Reproduce la lógica del query: totaliza IGI_Pagado e IGI_Calculado por forma de pago
        /// Para forma de pago '5' se iguala IGI_Pagado a 0 antes de totalizar
        /// </summary>
        public List<ResumenFormaPagoIGI> ObtenerResumenPorFormaPagoIGI(List<DatoDetalleIGI> partidas)
        {
            var resultado = new List<ResumenFormaPagoIGI>();
            if (partidas == null || !partidas.Any()) return resultado;

            var agrupado = partidas
                .GroupBy(p => (p.Gl_FPagoAdvalorem ?? string.Empty).Trim())
                .Select(g => new
                {
                    Forma = string.IsNullOrEmpty(g.Key) ? "" : g.Key,
                    TotalPedimentos = g.Select(x => x.Pim_Folio).Distinct().Count(),
                    TotalPartidas = g.Count(),
                    TotalIGI_Pagado = g.Sum(x => x.Gl_ImporteADvalorem),
                    TotalIGI_Calculado = g.Sum(x => x.IGI_CalculadoDetalle)
                });

            foreach (var item in agrupado)
            {
                var totalPagado = item.TotalIGI_Pagado;
                // Si la forma es '5' (credito) igualar pagado a 0
                if (item.Forma == "5") totalPagado = 0m;

                resultado.Add(new ResumenFormaPagoIGI
                {
                    FormaPago = item.Forma,
                    TotalPedimentos = item.TotalPedimentos,
                    TotalPartidas = item.TotalPartidas,
                    TotalIGI_Pagado = totalPagado,
                    TotalIGI_Calculado = item.TotalIGI_Calculado,
                    Diferencia = totalPagado - item.TotalIGI_Calculado
                });
            }

            return resultado.OrderBy(r => r.FormaPago).ToList();
        }

        /// <summary>
        /// Endpoint público: obtiene el resumen por forma de pago IGI para una base de pedimentos
        /// Reutiliza ObtenerPartidasPorBase() y luego agrega/totaliza respetando la regla FP-5 = IGI_Pagado 0
        /// </summary>
        public List<ResumenFormaPagoIGI> ObtenerResumenPorFormaPagoIGIPorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var partidas = ObtenerPartidasPorBase(baseDatos, fechaInicio, fechaFin);
                return ObtenerResumenPorFormaPagoIGI(partidas);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener resumen por forma de pago para base {baseDatos}: {ex.Message}");
                return new List<ResumenFormaPagoIGI>();
            }
        }

        /// <summary>
        /// Endpoint público: obtiene el resumen por forma de pago IGI para todas las bases de una razón social
        /// Recorre las bases de la razón y consolida las partidas antes de totalizar
        /// </summary>
        public List<ResumenFormaPagoIGI> ObtenerResumenPorFormaPagoIGIPorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var allPartidas = new List<DatoDetalleIGI>();
                var bases = ObtenerBasesDatosConConexion(idRazon);
                if (!bases.Any()) return new List<ResumenFormaPagoIGI>();

                foreach (var b in bases)
                {
                    try
                    {
                        var partidas = ObtenerPartidasPorBase(b.BaseDatos, fechaInicio, fechaFin);
                        if (partidas != null && partidas.Any())
                            allPartidas.AddRange(partidas);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener partidas para base {b.BaseDatos}: {ex.Message}");
                    }
                }

                return ObtenerResumenPorFormaPagoIGI(allPartidas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen por razón social: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el detalle por razón social (varias bases) para mostrar pedimentos individuales
        /// Recorre todas las bases de la razón y consolida pedimentos detallados agrupados por pedimento
        /// </summary>
        public List<ReporteIGIPagado> ObtenerDetallePorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var resultados = new List<ReporteIGIPagado>();

                // Obtener bases de datos asociadas a la razón social
                var bases = ObtenerBasesDatosConConexion(idRazon);
                if (!bases.Any())
                    return resultados;

                // Obtener la base de TR_GLOSA de la razón
                var razon = ObtenerRazonSocial(idRazon);
                string baseDatosGlosa = razon.BaseDatosOrigen;

                foreach (var b in bases)
                {
                    try
                    {
                        var conexionPedimentos = ObtenerConexionParaBaseDatos(b.BaseDatos);
                        var conexionGlosa = ObtenerConexionParaBaseDatos(baseDatosGlosa);

                        var datosDetalle = ObtenerDatosDetalleConJoinCruzado(b.BaseDatos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);
                        var agrupados = AgruparDatosPorPedimento(datosDetalle);
                        resultados.AddRange(agrupados);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error obteniendo detalle para base {b.BaseDatos}: {ex.Message}");
                    }
                }

                // Consolidar por pedimento (mismo pedimento puede venir de varias bases)
                var final = resultados
                    .GroupBy(r => r.Pedimento)
                    .Select(g => g.First())
                    .ToList();

                return final;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalle por razón social: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el detalle por pedimento (no agrupado) para una base de pedimentos específica
        /// Usado para mostrar el detalle en el formulario cuando se hace doble click en una agrupación
        /// </summary>
        public List<ReporteIGIPagado> ObtenerDetallePorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
                var razonSocial = ObtenerRazonSocial(idRazon);
                string baseDatosGlosa = razonSocial.BaseDatosOrigen;

                var conexionPedimentos = ObtenerConexionParaBaseDatos(baseDatos);
                var conexionGlosa = ObtenerConexionParaBaseDatos(baseDatosGlosa);

                // Obtener registros detalle (por secuencia/fracción)
                var datosDetalle = ObtenerDatosDetalleConJoinCruzado(baseDatos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);

                // Agrupar por pedimento para generar ReporteIGIPagado por pedimento
                var agrupados = AgruparDatosPorPedimento(datosDetalle);
                return agrupados;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalle por base {baseDatos}: {ex.Message}", ex);
            }
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

        // Helper: valida si dos conexiones/server info representan la misma conexión
        private bool ValidarSiMismaConexion(string servidorA, string servidorB, int? idConexionA, int? idConexionB)
        {
            // si ambos tienen IdConexion y son iguales -> misma
            if (idConexionA.HasValue && idConexionB.HasValue)
                return idConexionA.Value == idConexionB.Value;

            // si uno tiene IdConexion y el otro no -> distintos
            if (idConexionA.HasValue || idConexionB.HasValue)
                return false;

            // si ninguno tiene IdConexion, comparar servidor string
            return string.Equals(servidorA ?? string.Empty, servidorB ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // Facade: reutiliza la lógica ya implementada para obtener datos agrupados
        // Si las implementaciones completas están en otros métodos, este método las llamará.
        private List<ReporteIGIPagado> ObtenerDatosAgrupadosConJoinCruzado(
            string baseDatosPedimentos,
            string baseDatosGlosa,
            DateTime fechaInicio,
            DateTime fechaFin,
            Conexion conexionPedimentos,
            Conexion conexionGlosa)
        {
            // Reutilizar la lógica que ya existe en métodos más abajo del archivo.
            // Determinamos si están en el mismo servidor y delegamos.
            var infoPed = ObtenerConexionExterna(baseDatosPedimentos);
            var infoGlo = ObtenerConexionExterna(baseDatosGlosa);

            string servidorPedimentos = infoPed.TieneConexionExterna && !string.IsNullOrEmpty(infoPed.Servidor) ? infoPed.Servidor : conexionPrincipal.Servidor ?? string.Empty;
            string servidorGlosa = infoGlo.TieneConexionExterna && !string.IsNullOrEmpty(infoGlo.Servidor) ? infoGlo.Servidor : conexionPrincipal.Servidor ?? string.Empty;

            bool mismoServidor = ValidarSiMismaConexion(servidorPedimentos, servidorGlosa, infoPed.IdConexion, infoGlo.IdConexion);

            List<DatoDetalleIGI> detalles;
            if (mismoServidor)
            {
                detalles = ObtenerDatosConJoinDirecto(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, servidorPedimentos, infoPed.TieneConexionExterna ? infoPed.UsuarioSQL ?? string.Empty : conexionPrincipal.UsuarioSQL ?? string.Empty);
            }
            else
            {
                detalles = ObtenerDatosConConsultasSeparadas(baseDatosPedimentos, baseDatosGlosa, fechaInicio, fechaFin, conexionPedimentos, conexionGlosa);
            }

            // Agrupar por pedimento y devolver ReporteIGIPagado
            try
            {
                return AgruparDatosPorPedimento(detalles);
            }
            catch
            {
                return new List<ReporteIGIPagado>();
            }
        }

        // Stub: obtiene datos de TR_GLOSA para un pedimento (multi-servidor path)
        // Devuelve lista de objetos con campos esperados por el código más arriba.
        private List<(int Secuencia, decimal ImporteADvalorem, decimal ImporteIVA, DateTime? FechaPago, string FormaPagoIGI, string FormaPagoIVA, string Pedimento, string OrigenZip)> ObtenerDatosGlosaParaPedimento(
            string baseDatosGlosa,
            string aduana,
            string patente,
            string folio,
            DateTime? fechaPago,
            Conexion conexionGlosa)
        {
            var lista = new List<(int, decimal, decimal, DateTime?, string, string, string, string)>();

            try
            {
                // Query mínimo para obtener secuencia e importes desde TR_GLOSA
                string sql = $@"
                    SELECT ISNULL(TR.GL_SEC,0) AS Secuencia,
                           ISNULL(TR.Gl_ImporteADvalorem,0) AS ImporteADvalorem,
                           ISNULL(TR.Gl_ImporteIVA,0) AS ImporteIVA,
                           CONVERT(DATE, TR.Gl_FecPagoReal) AS FechaPago,
                           ISNULL(TR.Gl_FPagoAdvalorem,'') AS FormaPagoIGI,
                           ISNULL(TR.Gl_FPagoIVA,'') AS FormaPagoIVA,
                           ISNULL(TR.Gl_Pedimento,'') AS Pedimento,
                           ISNULL(TR.Gl_OrigenZipGlosa,'') AS OrigenZip
                    FROM [{baseDatosGlosa}].dbo.TR_GLOSA TR
                    WHERE TR.Gl_Aduana = @Aduana
                      AND TR.Gl_Patente = @Patente
                      AND TR.Gl_Pedimento = @Folio
                      AND (@FechaPago IS NULL OR CONVERT(DATE,TR.Gl_FecPagoReal) = @FechaPago)
                      AND TR.Gl_TOper = 1";

                using var cn = conexionGlosa.ObtenerConexion();
                using var cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@Aduana", aduana ?? string.Empty);
                cmd.Parameters.AddWithValue("@Patente", patente ?? string.Empty);
                cmd.Parameters.AddWithValue("@Folio", folio ?? string.Empty);
                cmd.Parameters.AddWithValue("@FechaPago", fechaPago.HasValue ? (object)fechaPago.Value.Date : DBNull.Value);

                cn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add((
                        reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)),
                        reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)),
                        LeerFechaPago(reader, 3),
                        reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                    ));
                }
            }
            catch
            {
                // en caso de error devolver vacío
            }

            return lista;
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
                System.Diagnostics.Debug.WriteLine($"      +- Servidor: {servidorGlosa}");
                System.Diagnostics.Debug.WriteLine($"      +- Usuario SQL: {usuarioGlosa}");
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

            // Usar query preciso por partida conforme especificado por el usuario
            string sql = $@"
                SELECT
                    DP.Pim_Consecutivo AS iDPedimento,
                    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                    DI.Pid_Secuencia AS Partida,
                    ISNULL(TR.Gl_ImporteADvalorem,0) AS IGI_Pagado,
                    ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0) AS IGI_Calculado,
                    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,
                    CONVERT(DATE,TR.Gl_FecPagoReal) AS FechaPago
                FROM [{baseDatosPedimentos}].dbo.Di_Pedimento DP
                INNER JOIN [{baseDatosPedimentos}].dbo.Di_PedimentoDet DI
                    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                LEFT JOIN [{baseDatosGlosa}].dbo.TR_GLOSA TR
                    ON TR.Gl_Pedimento = DP.Pim_Folio
                    AND TR.Gl_Aduana = DP.Adu_AduanaSecc
                    AND TR.Gl_Patente = DP.AgP_Patente
                    AND DI.Pid_Secuencia = TR.GL_SEC
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                INNER JOIN [{baseDatosPedimentos}].dbo.Ca_Farancelaria FRA
                    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion, 2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
                    AND FRA.Pai_Clave = 'MEX'
                    AND FRA.Fra_TipoOper = 0
                WHERE
                    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
                    AND TR.Gl_FPagoAdvalorem IN ('0','5','21')";

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
                // Columnas: 0=iDPedimento,1=Pedimento,2=Partida,3=IGI_Pagado,4=IGI_Calculado,5=FormaPago_IGI,6=FechaPago
                var ped = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var partes = (ped ?? string.Empty).Split('-');

                var dato = new DatoDetalleIGI
                {
                    BaseDatos = baseDatosPedimentos,
                    Pim_Consecutivo = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Adu_AduanaSecc = partes.Length > 0 ? partes[0] : string.Empty,
                    AgP_Patente = partes.Length > 1 ? partes[1] : string.Empty,
                    Pim_Folio = partes.Length > 2 ? partes[2] : string.Empty,
                    Pid_Secuencia = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Gl_FecPagoReal = LeerFechaPago(reader, 6),
                    Gl_ImporteADvalorem = LeerDecimal(reader, 3),
                    IGI_CalculadoDetalle = LeerDecimal(reader, 4),
                    Gl_FPagoAdvalorem = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
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

            // PASO 2: Obtener partidas desde la base de pedimentos y consultar TR_GLOSA agregada desde la base de glosa
            // Ejecutar una consulta en la base de pedimentos que obtiene el IGI calculado por partida
            var partidasPedimento = new List<(int IdPedimento, string Pedimento, int Partida, decimal IGI_Calculado, DateTime? FechaPago)>();

            string sqlPartidas = $@"
                SELECT
                    DP.Pim_Consecutivo AS iDPedimento,
                    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
                    DI.Pid_Secuencia AS Partida,
                    ISNULL(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0), 0) AS IGI_Calculado,
                    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) AS FechaPago
                FROM [{baseDatosPedimentos}].dbo.Di_Pedimento DP
                INNER JOIN [{baseDatosPedimentos}].dbo.Di_PedimentoDet DI
                    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
                INNER JOIN [{baseDatosPedimentos}].dbo.Ca_Farancelaria FRA
                    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
                    AND FRA.Pai_Clave = 'MEX'
                    AND FRA.Fra_TipoOper = 0
                WHERE CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin";

            using (var cnPed = conexionPedimentos.ObtenerConexion())
            using (var cmdPart = new SqlCommand(sqlPartidas, cnPed))
            {
                cmdPart.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmdPart.Parameters.AddWithValue("@FechaFin", fechaFin);
                cnPed.Open();
                using var rdr = cmdPart.ExecuteReader();
                while (rdr.Read())
                {
                    partidasPedimento.Add((
                        rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0),
                        rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                        rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2),
                        rdr.IsDBNull(3) ? 0 : Convert.ToDecimal(rdr.GetValue(3)),
                        LeerFechaPago(rdr, 4)
                    ));
                }
            }

            if (!partidasPedimento.Any())
                return datosDetalle;

            // Consultar TR_GLOSA en la base de glosa agregando por Pedimento + Partida
            var glosaDict = new Dictionary<(string Pedimento, int Partida), (decimal IGI_Pagado, decimal IVA_Pagado, string FormaPago, DateTime? FechaPago, string OrigenZip)>();

            string sqlGlosa = $@"
                SELECT
                    TR.GL_SEC AS Partida,
                    TR.GL_ADUANA + '-' + TR.GL_PATENTE + '-' + TR.GL_PEDIMENTO AS Pedimento,
                    SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) AS IGI_Pagado,
                    SUM(ISNULL(TR.Gl_ImporteIVA,0)) AS IVA_Pagado,
                    ISNULL(TR.Gl_FPagoAdvalorem,'') AS FormaPago_IGI,
                    CONVERT(DATE,TR.Gl_FecPagoReal) AS FechaPago,
                    ISNULL(TR.Gl_OrigenZipGlosa,'') AS OrigenZip
                FROM [{baseDatosGlosa}].dbo.TR_GLOSA TR
                WHERE CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
                    AND TR.Gl_TOper = 1
                    AND TR.Gl_OrigenZipGlosa = 'S'
                    AND TR.Gl_FPagoAdvalorem IN ('0','5','21')
                GROUP BY TR.GL_SEC, TR.GL_ADUANA, TR.GL_PATENTE, TR.GL_PEDIMENTO, TR.Gl_FPagoAdvalorem, CONVERT(DATE,TR.Gl_FecPagoReal), TR.Gl_OrigenZipGlosa";

            using (var cnGlo = conexionGlosa.ObtenerConexion())
            using (var cmdGlo = new SqlCommand(sqlGlosa, cnGlo))
            {
                cmdGlo.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmdGlo.Parameters.AddWithValue("@FechaFin", fechaFin);
                cnGlo.Open();
                using var rdrG = cmdGlo.ExecuteReader();
                while (rdrG.Read())
                {
                    var ped = rdrG.IsDBNull(1) ? string.Empty : rdrG.GetString(1);
                    var part = rdrG.IsDBNull(0) ? 0 : rdrG.GetInt32(0);
                    glosaDict[(ped, part)] = (
                        rdrG.IsDBNull(2) ? 0 : Convert.ToDecimal(rdrG.GetValue(2)),
                        rdrG.IsDBNull(3) ? 0 : Convert.ToDecimal(rdrG.GetValue(3)),
                        rdrG.IsDBNull(4) ? string.Empty : rdrG.GetString(4),
                        LeerFechaPago(rdrG, 5),
                        rdrG.IsDBNull(6) ? string.Empty : rdrG.GetString(6)
                    );
                }
            }

            // Combinar partidasPedimento con glosaDict
            foreach (var p in partidasPedimento)
            {
                var key = (p.Pedimento ?? string.Empty, p.Partida);
                bool tieneGlosa = glosaDict.TryGetValue(key, out var g);

                // Si existe glosa, aplicar la regla: si la forma de pago es '5' (CRÉDITO) entonces IGI pagado = 0
                decimal igiPagado = 0m;
                decimal ivaPagado = 0m;
                string formaPago = string.Empty;
                DateTime? fechaPago = null;
                string origenZip = null;

                if (tieneGlosa)
                {
                    igiPagado = g.IGI_Pagado;
                    ivaPagado = g.IVA_Pagado;
                    formaPago = g.FormaPago ?? string.Empty;
                    fechaPago = g.FechaPago;
                    origenZip = g.OrigenZip;

                    if (formaPago == "5")
                    {
                        // Para forma 5, el importe pagado se considera 0 según la regla de negocio
                        igiPagado = 0m;
                    }
                }

                var diferencia = igiPagado - p.IGI_Calculado;
                var estatus = Math.Abs((double)diferencia) > 1 ? "DIFERENCIA" : "OK";

                var d = new DatoDetalleIGI
                {
                    BaseDatos = baseDatosPedimentos,
                    Pim_Consecutivo = p.IdPedimento,
                    Adu_AduanaSecc = (p.Pedimento ?? string.Empty).Split('-').FirstOrDefault() ?? string.Empty,
                    AgP_Patente = (p.Pedimento ?? string.Empty).Split('-').Skip(1).FirstOrDefault() ?? string.Empty,
                    Pim_Folio = (p.Pedimento ?? string.Empty).Split('-').Skip(2).FirstOrDefault() ?? string.Empty,
                    Pid_Secuencia = p.Partida,
                    Pim_FechaPago = p.FechaPago,
                    Gl_FecPagoReal = fechaPago,
                    Gl_ImporteADvalorem = igiPagado,
                    Gl_ImporteIVA = ivaPagado,
                    IGI_CalculadoDetalle = p.IGI_Calculado,
                    DiferenciaIGI = diferencia,
                    EstatusIGI = estatus,
                    Gl_FPagoAdvalorem = formaPago,
                    Gl_FPagoIVA = string.Empty,
                    Gl_Pedimento = tieneGlosa ? p.Pedimento : null,
                    Gl_OrigenZipGlosa = origenZip,
                    EstatusGlosa = tieneGlosa ? "SI CARGADO" : "NO CARGADO",
                };

                datosDetalle.Add(d);
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"   ? Total registros detalle obtenidos: {datosDetalle.Count}\n");
#endif

            return datosDetalle;
        }

        /// <summary>
        /// Obtiene los detalles de un pedimento con información por partida
        /// Devuelve secuencia, IGI calculado, valor aduana, fracción y tasa IGI
        /// </summary>
        private List<(int Secuencia, decimal IGI_Calculado, decimal ValorAdu, string Fraccion, decimal TasaIGI)> ObtenerDetallesPedimento(
            string baseDatos,
            int consecutivo,
            Conexion conexion)
        {
            var detalles = new List<(int Secuencia, decimal IGI_Calculado, decimal ValorAdu, string Fraccion, decimal TasaIGI)>();

            string sql = $@"
                SELECT 
                    DI.Pid_Secuencia,
                    ISNULL(ROUND((ISNULL(DI.Pid_ValorAdu, 0) * ISNULL(FRA.Fra_AdvGral, 0)) / 100, 0), 0) AS IGI_Calculado,
                    ISNULL(DI.Pid_ValorAdu, 0) AS ValorAdu,
                    IIF(LEFT(DI.Fra_Fraccion,2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion) AS Fraccion,
                    ISNULL(FRA.Fra_AdvGral, 0) AS TasaIGI
                FROM [{baseDatos}].dbo.Di_PedimentoDet DI
                INNER JOIN [{baseDatos}].dbo.Ca_Farancelaria FRA
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
                    LeerDecimal(reader, 1),
                    LeerDecimal(reader, 2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    LeerDecimal(reader, 4)
                ));
            }

            return detalles;
        }

        /// <summary>
        /// Obtiene detalle a nivel de partidas para una base de pedimentos
        /// Usa query directo cuando las bases están en el mismo servidor; en multi-servidor
        /// combina pedimentos + partidas y valida contra TR_GLOSA de forma equivalente.
        /// </summary>
        public List<DatoDetalleIGI> ObtenerPartidasPorBase(string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultados = new List<DatoDetalleIGI>();

            int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
            var razon = ObtenerRazonSocial(idRazon);
            string baseDatosGlosa = razon.BaseDatosOrigen;

            var conexionPedimentos = ObtenerConexionParaBaseDatos(baseDatos);
            var conexionGlosa = ObtenerConexionParaBaseDatos(baseDatosGlosa);

            // Determinar si están en el mismo servidor
            var conexionInfoPedimentos = ObtenerConexionExterna(baseDatos);
            var conexionInfoGlosa = ObtenerConexionExterna(baseDatosGlosa);
            string servidorPedimentos = conexionInfoPedimentos.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoPedimentos.Servidor)
                ? conexionInfoPedimentos.Servidor
                : conexionPrincipal.Servidor ?? string.Empty;
            string servidorGlosa = conexionInfoGlosa.TieneConexionExterna && !string.IsNullOrEmpty(conexionInfoGlosa.Servidor)
                ? conexionInfoGlosa.Servidor
                : conexionPrincipal.Servidor ?? string.Empty;

            bool mismoServidor = ValidarSiMismaConexion(servidorPedimentos, servidorGlosa, conexionInfoPedimentos.IdConexion, conexionInfoGlosa.IdConexion);

            if (mismoServidor)
            {
                // Ejecutar query proporcionado (con prefijos de base)
                string sql = $@"
SELECT 
    DP.Pim_Consecutivo AS iDPedimento,
    DP.Adu_AduanaSecc + '-' + DP.AgP_Patente + '-' + DP.Pim_Folio AS Pedimento,
    DI.Pid_Secuencia AS Partida,
    TR.Gl_FecPagoReal AS FechaPago,
    ISNULL(TR.Gl_ImporteADvalorem,0) AS IGI_Pagado,
    ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0) AS IGI_Calculado,
    ISNULL(TR.Gl_ImporteADvalorem,0) - ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0) AS Diferencia_IGI,
    CASE WHEN ABS(ISNULL(TR.Gl_ImporteADvalorem,0) - ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100.0, 0)) > 1 THEN 'DIFERENCIA' ELSE 'OK' END AS EstatusIGI,
    ISNULL(TR.Gl_ImporteIVA,0) AS Gl_ImporteIVA,
    DI.Pid_ValorAdu AS ValorAduana,
    IIF(LEFT(DI.Fra_Fraccion,2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion) AS Fraccion,
    FRA.Fra_AdvGral AS TasaIGI,
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,
    TR.Gl_FPagoIVA AS FormaPago_IVA,
    CASE WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO' ELSE 'NO CARGADO' END AS EstatusGlosa,
    CASE WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP' ELSE 'NO ZIP' END AS EstatusOrigen
FROM [{baseDatos}].dbo.Di_Pedimento DP
INNER JOIN [{baseDatos}].dbo.Di_PedimentoDet DI ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
LEFT JOIN [{baseDatosGlosa}].dbo.TR_GLOSA TR ON TR.Gl_Pedimento = DP.Pim_Folio
    AND TR.Gl_Aduana = DP.Adu_AduanaSecc
    AND TR.Gl_Patente = DP.AgP_Patente
    AND YEAR(IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) = YEAR(CONVERT(DATE,TR.Gl_FecPagoReal))
    AND DI.Pid_Secuencia = TR.GL_SEC
    AND TR.Gl_TOper = 1
    AND TR.Gl_OrigenZipGlosa = 'S'
INNER JOIN [{baseDatos}].dbo.Ca_Farancelaria FRA ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2) = '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion)
    AND FRA.Pai_Clave = 'MEX'
    AND FRA.Fra_TipoOper = 0
                WHERE CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
    AND (
        -- Seguir lógica del query original: incluir formas de pago ADVALOREM 0, 5 y 21
        TR.Gl_FPagoAdvalorem IN ('0','5','21')
    )
ORDER BY DP.Pim_Folio, DI.Pid_Secuencia";

                using var cn = conexionPedimentos.ObtenerConexion();
                using var cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                cn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var d = new DatoDetalleIGI
                    {
                        BaseDatos = baseDatos,
                        Pim_Consecutivo = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Adu_AduanaSecc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Split('-')[0],
                        AgP_Patente = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Split('-')[1],
                        Pim_Folio = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Split('-')[2],
                        Pid_Secuencia = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        Gl_FecPagoReal = LeerFechaPago(reader, 3),
                        Gl_ImporteADvalorem = LeerDecimal(reader, 4),
                        IGI_CalculadoDetalle = LeerDecimal(reader, 5),
                        DiferenciaIGI = LeerDecimal(reader, 6),
                        EstatusIGI = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Gl_ImporteIVA = LeerDecimal(reader, 8),
                        ValorAduana = LeerDecimal(reader, 9),
                        Fraccion = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        TasaIGI = LeerDecimal(reader, 11),
                        Gl_FPagoAdvalorem = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                        Gl_FPagoIVA = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                        EstatusGlosa = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                        Gl_OrigenZipGlosa = reader.IsDBNull(15) ? string.Empty : reader.GetString(15)
                    };

                    resultados.Add(d);
                }
            }
            else
            {
                // Multi-servidor: iterar pedimentos y combinar partidas con glosa filtrada
                // Reutilizar la lista de pedimentos como en ObtenerDatosConConsultasSeparadas
                string sqlPedimentos = @"
                SELECT DISTINCT
                    DP.Pim_Consecutivo,
                    DP.Adu_AduanaSecc,
                    DP.AgP_Patente,
                    DP.Pim_Folio,
                    IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago) AS Pim_FechaPago
                FROM Di_Pedimento DP
                WHERE CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin";

                var pedimentosBase = new List<(int Consecutivo, string Aduana, string Patente, string Folio, DateTime? FechaPago)>();
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

                const int tamañoLote = 50;
                int totalLotes = (int)Math.Ceiling(pedimentosBase.Count / (double)tamañoLote);

                for (int i = 0; i < totalLotes; i++)
                {
                    var lote = pedimentosBase.Skip(i * tamañoLote).Take(tamañoLote).ToList();
                    foreach (var pedimento in lote)
                    {
                        try
                        {
                            var detalles = ObtenerDetallesPedimento(baseDatos, pedimento.Consecutivo, conexionPedimentos);
                            var datosGlosa = ObtenerDatosGlosaParaPedimento(baseDatosGlosa, pedimento.Aduana, pedimento.Patente, pedimento.Folio, pedimento.FechaPago, conexionGlosa);

                            // Filtrar glosa por formas solicitadas y por secuencia
                            foreach (var det in detalles)
                            {
                                // Buscar glosa por secuencia y por forma de pago IGI (0,5,21) según query original
                                var gl = datosGlosa.FirstOrDefault(g => g.Secuencia == det.Secuencia &&
                                    (g.FormaPagoIGI == "0" || g.FormaPagoIGI == "5" || g.FormaPagoIGI == "21"));

                                if (gl.Secuencia == 0) continue; // no coincidente

                                var d = new DatoDetalleIGI
                                {
                                    BaseDatos = baseDatos,
                                    Pim_Consecutivo = pedimento.Consecutivo,
                                    Adu_AduanaSecc = pedimento.Aduana,
                                    AgP_Patente = pedimento.Patente,
                                    Pim_Folio = pedimento.Folio,
                                    Pid_Secuencia = det.Secuencia,
                                    Pim_FechaPago = pedimento.FechaPago,
                                    Gl_FecPagoReal = gl.FechaPago,
                                    Gl_ImporteADvalorem = gl.ImporteADvalorem,
                                    IGI_CalculadoDetalle = det.IGI_Calculado,
                                    DiferenciaIGI = gl.ImporteADvalorem - det.IGI_Calculado,
                                    EstatusIGI = Math.Abs((double)(gl.ImporteADvalorem - det.IGI_Calculado)) > 1 ? "DIFERENCIA" : "OK",
                                    Gl_ImporteIVA = gl.ImporteIVA,
                                    ValorAduana = det.ValorAdu,
                                    Fraccion = det.Fraccion,
                                    TasaIGI = det.TasaIGI,
                                    Gl_FPagoAdvalorem = gl.FormaPagoIGI,
                                    Gl_FPagoIVA = gl.FormaPagoIVA,
                                    Gl_Pedimento = gl.Pedimento,
                                    Gl_OrigenZipGlosa = gl.OrigenZip
                                };

                                resultados.Add(d);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"   ?? Error procesando partidas pedimento {pedimento.Folio}: {ex.Message}");
                        }
                    }
                }
            }

            return resultados;
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
        /// Lee un valor decimal del reader manejando conversiones de tipo y valores NULL
        /// </summary>
        private decimal LeerDecimal(SqlDataReader reader, int columnIndex)
        {
            if (reader.IsDBNull(columnIndex))
                return 0;

            try
            {
                // Intentar obtener el valor del tipo que sea
                object value = reader.GetValue(columnIndex);
                
                // Convertir a decimal sin importar el tipo SQL original
                return Convert.ToDecimal(value);
            }
            catch
            {
                return 0;
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

















































































































































































































































































































































































































































































































