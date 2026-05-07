using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System.Data;
using System.Linq;
using System.Net;

namespace Retorno360Tacna.SERVICES
{
    public class RetornoService
    {
        private readonly ConexionInfo conexionInfo;
        // Cache de conexiones externas por base de datos
        private readonly Dictionary<string, ConexionExternaInfo> cacheConexionesExternas;

        public RetornoService(ConexionInfo conexion)
        {
            conexionInfo = conexion;
            cacheConexionesExternas = new Dictionary<string, ConexionExternaInfo>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Obtiene la información de conexión externa para una base de datos específica
        /// Busca primero en RAZONXTABLA (bases origen/glosa) y luego en NOM_TABLARAZON (bases seleccionables)
        /// </summary>
        private ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
        {
            // Verificar cache primero
            if (cacheConexionesExternas.TryGetValue(baseDatos, out var conexionCacheada))
            {
                return conexionCacheada;
            }

            var conexionExterna = new ConexionExternaInfo { BaseDatos = baseDatos };

            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                // PASO 1: Buscar en RAZONXTABLA (bases origen/glosa - comportamiento actual)
                string sqlRazonXTabla = @"
                    SELECT TOP 1 
                        R.ConnExterna,
                        R.IdConexion,
                        C.NombreConexion,
                        C.Servidor,
                        C.UsuarioSQL,
                        C.PasswordSQL
                    FROM RAZONXTABLA R
                    LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
                    WHERE R.DB = @BaseDatos
                    ORDER BY R.IdRazon";

                using (SqlConnection cn = conexion.ObtenerConexion())
                {
                    cn.Open();

                    bool encontrado = false;

                    // Intentar en RAZONXTABLA primero
                    using (SqlCommand cmd = new SqlCommand(sqlRazonXTabla, cn))
                    {
                        cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                encontrado = true;

                                // Leer ConnExterna
                                string? connExterna = reader.IsDBNull(0) ? null : reader.GetString(0);
                                conexionExterna.TieneConexionExterna = connExterna?.Trim().Equals("S", StringComparison.OrdinalIgnoreCase) == true;

                                // Leer IdConexion
                                if (!reader.IsDBNull(1))
                                {
                                    conexionExterna.IdConexion = reader.GetInt32(1);
                                }

                                // Leer datos de la tabla Conexiones (si existe el JOIN)
                                if (!reader.IsDBNull(2))
                                {
                                    conexionExterna.NombreConexion = reader.GetString(2);
                                    conexionExterna.Servidor = reader.IsDBNull(3) ? null : reader.GetString(3);
                                    conexionExterna.UsuarioSQL = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    conexionExterna.PasswordSQL = reader.IsDBNull(5) ? null : reader.GetString(5);
                                }

                                #if DEBUG
                                System.Diagnostics.Debug.WriteLine($"📡 Conexión encontrada en RAZONXTABLA para '{baseDatos}'");
                                #endif
                            }
                        }
                    }

                    // PASO 2: Si no se encontró en RAZONXTABLA, buscar en NOM_TABLARAZON
                    // NOM_TABLARAZON NO tiene campo ConnExterna, solo IdConexion
                    if (!encontrado)
                    {
                        string sqlNomTablaRazon = @"
                            SELECT TOP 1 
                                N.IdConexion,
                                C.NombreConexion,
                                C.Servidor,
                                C.UsuarioSQL,
                                C.PasswordSQL
                            FROM NOM_TABLARAZON N
                            LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
                            WHERE N.NOMBRE_TABLA = @BaseDatos
                            ORDER BY N.IdTabla";

                        using (SqlCommand cmd = new SqlCommand(sqlNomTablaRazon, cn))
                        {
                            cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    encontrado = true;

                                    // Leer IdConexion (si es NULL = conexión principal, si tiene valor = externa)
                                    if (!reader.IsDBNull(0))
                                    {
                                        conexionExterna.IdConexion = reader.GetInt32(0);
                                        conexionExterna.TieneConexionExterna = true;
                                    }
                                    else
                                    {
                                        conexionExterna.TieneConexionExterna = false;
                                    }

                                    // Leer datos de la tabla Conexiones (si existe el JOIN)
                                    if (!reader.IsDBNull(1))
                                    {
                                        conexionExterna.NombreConexion = reader.GetString(1);
                                        conexionExterna.Servidor = reader.IsDBNull(2) ? null : reader.GetString(2);
                                        conexionExterna.UsuarioSQL = reader.IsDBNull(3) ? null : reader.GetString(3);
                                        conexionExterna.PasswordSQL = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    }

                                    #if DEBUG
                                    System.Diagnostics.Debug.WriteLine($"📡 Conexión encontrada en NOM_TABLARAZON para '{baseDatos}'");
                                    #endif
                                }
                            }
                        }
                    }

                    #if DEBUG
                    if (!encontrado)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Base de datos '{baseDatos}' no encontrada en RAZONXTABLA ni en NOM_TABLARAZON. Usando conexión principal.");
                    }
                    else if (conexionExterna.TieneConexionExterna)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"📡 Conexión Externa para '{baseDatos}':\n" +
                            $"   Servidor: {conexionExterna.Servidor}\n" +
                            $"   IdConexion: {conexionExterna.IdConexion}"
                        );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"🔌 Base de datos '{baseDatos}' usará conexión principal (sin IdConexion)");
                    }
                    #endif
                }

                // Guardar en cache
                cacheConexionesExternas[baseDatos] = conexionExterna;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo conexión externa para '{baseDatos}': {ex.Message}");
                // Retornar objeto por defecto (usará conexión principal)
            }

            return conexionExterna;
        }

        /// <summary>
        /// Obtiene la información de conexión de una base de datos específica desde NOM_TABLARAZON
        /// Esta es la tabla que contiene las bases de datos seleccionables por el usuario
        /// Solo usa IdConexion para determinar el servidor de los pedimentos
        /// </summary>
        private ConexionExternaInfo ObtenerConexionDesdeNomTablaRazon(string baseDatos)
        {
            // Verificar cache primero (usar clave diferente para NOM_TABLARAZON)
            string cacheKey = $"NOM_{baseDatos}";
            if (cacheConexionesExternas.TryGetValue(cacheKey, out var conexionCacheada))
            {
                return conexionCacheada;
            }

            var conexionExterna = new ConexionExternaInfo { BaseDatos = baseDatos };

            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                // NOM_TABLARAZON solo tiene IdConexion (NO tiene ConnExterna)
                string sql = @"
                    SELECT TOP 1 
                        N.IdConexion,
                        C.NombreConexion,
                        C.Servidor,
                        C.UsuarioSQL,
                        C.PasswordSQL
                    FROM NOM_TABLARAZON N
                    LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
                    WHERE N.NOMBRE_TABLA = @BaseDatos
                    ORDER BY N.IdTabla";

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Leer IdConexion
                            if (!reader.IsDBNull(0))
                            {
                                conexionExterna.IdConexion = reader.GetInt32(0);
                                conexionExterna.TieneConexionExterna = true; // Si tiene IdConexion, usa conexión externa
                            }
                            else
                            {
                                conexionExterna.TieneConexionExterna = false; // NULL = usar conexión principal
                            }

                            // Leer datos de la tabla Conexiones (si existe el JOIN)
                            if (!reader.IsDBNull(1))
                            {
                                conexionExterna.NombreConexion = reader.GetString(1);
                                conexionExterna.Servidor = reader.IsDBNull(2) ? null : reader.GetString(2);
                                conexionExterna.UsuarioSQL = reader.IsDBNull(3) ? null : reader.GetString(3);
                                conexionExterna.PasswordSQL = reader.IsDBNull(4) ? null : reader.GetString(4);
                            }

                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine($"📋 Conexión encontrada en NOM_TABLARAZON para '{baseDatos}'");
                            if (conexionExterna.TieneConexionExterna)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"   📡 Conexión Externa:\n" +
                                    $"      Servidor: {conexionExterna.Servidor}\n" +
                                    $"      IdConexion: {conexionExterna.IdConexion}"
                                );
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"   🔌 Usará conexión principal (IdConexion es NULL)");
                            }
                            #endif
                        }
                        else
                        {
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine($"⚠️ Base de datos '{baseDatos}' no encontrada en NOM_TABLARAZON. Usando conexión principal.");
                            #endif
                        }
                    }
                }

                // Guardar en cache
                cacheConexionesExternas[cacheKey] = conexionExterna;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo conexión desde NOM_TABLARAZON para '{baseDatos}': {ex.Message}");
                // Retornar objeto por defecto (usará conexión principal)
            }

            return conexionExterna;
        }

        /// <summary>
        /// Obtiene la conexión SQL apropiada para una base de datos
        /// (principal o externa según RAZONXTABLA)
        /// </summary>
        private SqlConnection ObtenerConexionParaBaseDatos(string baseDatos)
        {
            var conexionExt = ObtenerConexionExterna(baseDatos);
            return ObtenerConexionParaBaseDatos(baseDatos, conexionExt);
        }

        /// <summary>
        /// Obtiene la conexión SQL apropiada usando información de conexión específica
        /// </summary>
        private SqlConnection ObtenerConexionParaBaseDatos(string baseDatos, ConexionExternaInfo conexionExt)
        {
            if (conexionExt.UsarConexionPrincipal)
            {
                // Usar conexión principal
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    baseDatos
                );
                return conexion.ObtenerConexion();
            }
            else
            {
                // Usar conexión externa
                if (string.IsNullOrWhiteSpace(conexionExt.Servidor) ||
                    string.IsNullOrWhiteSpace(conexionExt.UsuarioSQL) ||
                    string.IsNullOrWhiteSpace(conexionExt.PasswordSQL))
                {
                    throw new Exception(
                        $"La base de datos '{baseDatos}' requiere conexión externa (IdConexion: {conexionExt.IdConexion}), " +
                        "pero los datos de conexión están incompletos en la tabla Conexiones.\n" +
                        "Verifica que exista un registro válido con ese IdConexion."
                    );
                }

                Conexion conexion = new Conexion(
                    conexionExt.Servidor,
                    conexionExt.UsuarioSQL,
                    conexionExt.PasswordSQL,
                    baseDatos
                );

                return conexion.ObtenerConexion();
            }
        }

        /// <summary>
        /// Obtiene el nombre del servidor donde está alojada una base de datos
        /// </summary>
        private string ObtenerServidorDeBaseDatos(string baseDatos)
        {
            var conexionExt = ObtenerConexionExterna(baseDatos);

            if (conexionExt.UsarConexionPrincipal)
            {
                return conexionInfo.Servidor ?? "Servidor Principal";
            }
            else
            {
                return conexionExt.Servidor ?? "Servidor Desconocido";
            }
        }

        /// <summary>
        /// Compara dos servidores para determinar si son el mismo
        /// Considera tanto nombre como IP, ya que pueden estar en formatos diferentes
        /// </summary>
        private bool SonMismoServidor(string servidor1, string servidor2)
        {
            if (string.IsNullOrWhiteSpace(servidor1) || string.IsNullOrWhiteSpace(servidor2))
                return false;

            // Comparación exacta (case insensitive)
            if (servidor1.Equals(servidor2, StringComparison.OrdinalIgnoreCase))
                return true;

            // Intentar resolver IPs si uno es nombre y otro es IP
            try
            {
                // Obtener todas las direcciones IP de ambos servidores
                var ips1 = System.Net.Dns.GetHostAddresses(servidor1).Select(ip => ip.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var ips2 = System.Net.Dns.GetHostAddresses(servidor2).Select(ip => ip.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Si alguna IP coincide, son el mismo servidor
                if (ips1.Overlaps(ips2))
                    return true;

                // También verificar si servidor1 está en las IPs de servidor2 o viceversa
                if (ips1.Contains(servidor2) || ips2.Contains(servidor1))
                    return true;
            }
            catch
            {
                // Si falla la resolución DNS, solo usar comparación de string
                System.Diagnostics.Debug.WriteLine($"   ⚠️ No se pudo resolver DNS para comparar {servidor1} vs {servidor2}");
            }

            return false;
        }

        /// <summary>
        /// Limpia el cache de conexiones externas
        /// </summary>
        public void LimpiarCacheConexiones()
        {
            cacheConexionesExternas.Clear();
        }


        public ResultadoRetorno CalcularRetorno(
            int idRazon,
            string baseDatosSeleccionada,
            DateTime fechaInicio,
            DateTime fechaFin,
            bool incluirMateriaPrima,
            bool forzarCalculo = false)
        {
            try
            {
                // 🔍 DIAGNÓSTICO: Mostrar qué servidor se usará
                var conexionExt = ObtenerConexionExterna(baseDatosSeleccionada);
                string servidorUsado = ObtenerServidorDeBaseDatos(baseDatosSeleccionada);

                System.Diagnostics.Debug.WriteLine($"\n🔍 INICIO CÁLCULO DE RETORNO");
                System.Diagnostics.Debug.WriteLine($"   Base de datos seleccionada: {baseDatosSeleccionada}");
                System.Diagnostics.Debug.WriteLine($"   ¿Tiene conexión externa?: {conexionExt.TieneConexionExterna}");
                System.Diagnostics.Debug.WriteLine($"   IdConexion: {conexionExt.IdConexion}");
                System.Diagnostics.Debug.WriteLine($"   Servidor a usar: {servidorUsado}");

                // Obtener información de la razón social
                RazonSocial razonInfo = ObtenerRazonSocial(idRazon);

                if (razonInfo == null || string.IsNullOrEmpty(razonInfo.BaseDatosOrigen))
                {
                    throw new Exception($"No se encontró la razón social o su base de datos origen.");
                }

                System.Diagnostics.Debug.WriteLine($"   Base de datos origen: {razonInfo.BaseDatosOrigen}");

                var conexionOrigen = ObtenerConexionExterna(razonInfo.BaseDatosOrigen);
                string servidorOrigen = ObtenerServidorDeBaseDatos(razonInfo.BaseDatosOrigen);

                System.Diagnostics.Debug.WriteLine($"   ¿Origen tiene conexión externa?: {conexionOrigen.TieneConexionExterna}");
                System.Diagnostics.Debug.WriteLine($"   Servidor origen: {servidorOrigen}\n");

                // Validar pedimentos entre bases de datos
                var pedimentosValidos = ValidarPedimentosCruzados(
                    baseDatosSeleccionada,
                    razonInfo.BaseDatosOrigen,
                    fechaInicio,
                    fechaFin
                );

                if (!pedimentosValidos.Any())
                {
                    throw new Exception($"No se encontraron pedimentos coincidentes entre {baseDatosSeleccionada} y {razonInfo.BaseDatosOrigen}");
                }

                // Calcular valores usando los pedimentos validados
                decimal importado = ObtenerImportacionesValidadas(
                    razonInfo.BaseDatosOrigen,
                    pedimentosValidos.Where(p => p.Tipo == "IMPORTACION").ToList(),
                    fechaInicio,
                    fechaFin
                );

                decimal exportado = ObtenerExportacionesValidadas(
                    razonInfo.BaseDatosOrigen,
                    pedimentosValidos.Where(p => p.Tipo == "EXPORTACION").ToList(),
                    fechaInicio,
                    fechaFin,
                    incluirMateriaPrima
                );

                // Validar que existan tanto importaciones como exportaciones cargadas como ZIP
                // A menos que se fuerce el cálculo
                if (!forzarCalculo)
                {
                    if (importado == 0 && exportado > 0)
                    {
                        throw new Exception($"No se encontraron importaciones cargadas como ZIP en la glosa para el período seleccionado.\n\n" +
                            $"El cálculo de retorno requiere que existan tanto importaciones como exportaciones cargadas correctamente en el sistema.\n\n" +
                            $"Por favor, verifique que los pedimentos de importación estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                    }


                    if (exportado == 0 && importado > 0)
                    {
                        throw new Exception($"No se encontraron exportaciones cargadas como ZIP en la glosa para el período seleccionado.\n\n" +
                            $"El cálculo de retorno requiere que existan tanto importaciones como exportaciones cargadas correctamente en el sistema.\n\n" +
                            $"Por favor, verifique que los pedimentos de exportación estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                    }

                    if (importado == 0 && exportado == 0)
                    {
                        throw new Exception($"No se encontraron pedimentos (ni importaciones ni exportaciones) cargados como ZIP en la glosa para el período seleccionado.\n\n" +
                            $"El cálculo de retorno requiere que existan pedimentos cargados correctamente en el sistema.\n\n" +
                            $"Por favor, verifique que los pedimentos estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                    }
                }

                decimal porcentaje = CalculadoraRetorno.CalcularPorcentajeRetorno(importado, exportado);

                // Contar pedimentos validados
                int cantImportacion = pedimentosValidos.Count(p => p.Tipo == "IMPORTACION");
                int cantExportacion = pedimentosValidos.Count(p => p.Tipo == "EXPORTACION");

                var resultado = new ResultadoRetorno
                {
                    RazonSocial = razonInfo.NombreRazon,
                    BaseDatos = baseDatosSeleccionada,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    ValorImportado = importado,
                    ValorExportado = exportado,
                    PorcentajeRetorno = porcentaje,
                    FechaCalculo = DateTime.Now,
                    IncluyeMateriaPrima = incluirMateriaPrima,
                    CantidadPedimentosImportacion = cantImportacion,
                    CantidadPedimentosExportacion = cantExportacion,
                    TotalPedimentosValidados = cantImportacion + cantExportacion
                };

                GuardarHistorico(resultado);

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular retorno: {ex.Message}", ex);
            }
        }

        public ResultadoRetorno CalcularRetornoPorRazonSocial(
            int idRazon,
            DateTime fechaInicio,
            DateTime fechaFin,
            bool incluirMateriaPrima)
        {
            try
            {
                // Obtener información de la razón social
                RazonSocial razonInfo = ObtenerRazonSocial(idRazon);

                if (razonInfo == null || string.IsNullOrEmpty(razonInfo.NombreRazon))
                {
                    throw new Exception($"No se encontró la razón social.");
                }

                // ✅ NUEVO: Obtener TODAS las bases de datos asociadas a esta razón desde NOM_TABLARAZON
                // Ya no se usa GLOSAXRAZON, ahora todo se maneja desde RAZONXTABLA y NOM_TABLARAZON
                List<string> basesDatos = ObtenerBasesDatosRazon(idRazon);

                if (!basesDatos.Any())
                {
                    throw new Exception($"No se encontraron bases de datos en NOM_TABLARAZON para la razón social: {razonInfo.NombreRazon} (IdRazon: {idRazon})");
                }

                // Acumuladores para sumar resultados de todas las BDs
                decimal totalImportado = 0;
                decimal totalExportado = 0;
                int totalPedimentosImp = 0;
                int totalPedimentosExp = 0;

                // Iterar sobre cada base de datos (como el cursor del SP)
                foreach (string baseDatos in basesDatos)
                {
                    try
                    {
                        var resultadoParcial = CalcularPorBaseDatos(
                            baseDatos,
                            fechaInicio,
                            fechaFin,
                            incluirMateriaPrima
                        );

                        totalImportado += resultadoParcial.importado;
                        totalExportado += resultadoParcial.exportado;
                        totalPedimentosImp += resultadoParcial.cantImportaciones;
                        totalPedimentosExp += resultadoParcial.cantExportaciones;
                    }
                    catch (Exception ex)
                    {
                        // Log error pero continúa con las demás BDs
                        System.Diagnostics.Debug.WriteLine($"Error en BD {baseDatos}: {ex.Message}");
                    }
                }

                // Validar que existan tanto importaciones como exportaciones cargadas como ZIP
                if (totalImportado == 0 && totalExportado > 0)
                {
                    throw new Exception($"No se encontraron importaciones cargadas como ZIP en la glosa para el período seleccionado.\n\n" +
                        $"El cálculo de retorno requiere que existan tanto importaciones como exportaciones cargadas correctamente en el sistema.\n\n" +
                        $"Por favor, verifique que los pedimentos de importación estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                }

                if (totalExportado == 0 && totalImportado > 0)
                {
                    throw new Exception($"No se encontraron exportaciones cargadas como ZIP en la glosa para el período seleccionado.\n\n" +
                        $"El cálculo de retorno requiere que existan tanto importaciones como exportaciones cargadas correctamente en el sistema.\n\n" +
                        $"Por favor, verifique que los pedimentos de exportación estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                }

                if (totalImportado == 0 && totalExportado == 0)
                {
                    throw new Exception($"No se encontraron pedimentos (ni importaciones ni exportaciones) cargados como ZIP en la glosa para el período seleccionado.\n\n" +
                        $"El cálculo de retorno requiere que existan pedimentos cargados correctamente en el sistema.\n\n" +
                        $"Por favor, verifique que los pedimentos estén cargados como ZIP (Gl_OrigenZipGlosa = 'S') en TR_Glosa.");
                }

                decimal porcentaje = CalculadoraRetorno.CalcularPorcentajeRetorno(totalImportado, totalExportado);

                var resultado = new ResultadoRetorno
                {
                    RazonSocial = razonInfo.NombreRazon,
                    BaseDatos = string.Join(", ", basesDatos), // Múltiples BDs
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    ValorImportado = totalImportado,
                    ValorExportado = totalExportado,
                    PorcentajeRetorno = porcentaje,
                    FechaCalculo = DateTime.Now,
                    IncluyeMateriaPrima = incluirMateriaPrima,
                    CantidadPedimentosImportacion = totalPedimentosImp,
                    CantidadPedimentosExportacion = totalPedimentosExp,
                    TotalPedimentosValidados = totalPedimentosImp + totalPedimentosExp
                };

                GuardarHistorico(resultado);

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular retorno por razón social: {ex.Message}", ex);
            }
        }


        private (decimal importado, decimal exportado, int cantImportaciones, int cantExportaciones)
            CalcularPorBaseDatos(string baseDatos, DateTime fechaInicio, DateTime fechaFin, bool incluirMateriaPrima)
        {
            try
            {
                // Query de IMPORTACIONES (igual al SP)
                string sqlImportaciones = $@"
                SELECT 
                    ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0) AS ValorComercial,
                    COUNT(DISTINCT Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento) AS CantidadPedimentos
                FROM [{baseDatos}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = 
                       Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                  AND Gl_CveDocto IN ('IN', 'V1')
                  AND Gl_TOper = 1
                  AND ide.Identificador IS NULL
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin";

                // Query de EXPORTACIONES (igual al SP con filtro materia prima)
                string filtroMateriaPrima = incluirMateriaPrima ? "" : "AND idePT.IDENTIFICADOR IS NOT NULL";

                string sqlExportaciones = $@"
                SELECT 
                    ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0) AS ValorComercial,
                    COUNT(DISTINCT Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento) AS CantidadPedimentos
                FROM [{baseDatos}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ideAF ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ideAF.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('DE')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador, Partida 
                    FROM [{baseDatos}].dbo.TR_GlosaPartidaIdentifica 
                    WHERE Identificador IN ('PT')
                ) idePT ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = idePT.Pedimento 
                    AND idePT.Partida = PAR.Gl_Sec
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = 
                       Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                  AND Gl_CveDocto IN ('RT', 'V1')
                  AND Gl_TOper = 2
                  AND ide.Identificador IS NULL
                  AND ideAF.Identificador IS NULL
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin
                  {filtroMateriaPrima}";

                decimal valorImportado = 0;
                int cantImportaciones = 0;
                decimal valorExportado = 0;
                int cantExportaciones = 0;

                var conexionBase = ObtenerConexionDesdeNomTablaRazon(baseDatos);

                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n📊 CALCULAR POR BASE DE DATOS: {baseDatos}");
                System.Diagnostics.Debug.WriteLine($"   ═══════════════════════════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"   • Conexión externa: {(conexionBase.TieneConexionExterna ? "Sí" : "No")}");
                System.Diagnostics.Debug.WriteLine($"   • IdConexion: {conexionBase.IdConexion?.ToString() ?? "NULL (usa servidor principal)"}");
                System.Diagnostics.Debug.WriteLine($"   • Servidor: {conexionBase.Servidor ?? conexionInfo.Servidor ?? "Principal"}");
                System.Diagnostics.Debug.WriteLine($"   • Usuario: {conexionBase.UsuarioSQL ?? conexionInfo.UsuarioSQL ?? "N/A"}");
                System.Diagnostics.Debug.WriteLine($"   ═══════════════════════════════════════════════════════════\n");
                #endif

                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos, conexionBase))
                {
                    cn.Open();

                    // Ejecutar query de importaciones
                    using (SqlCommand cmd = new SqlCommand(sqlImportaciones, cn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                valorImportado = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                cantImportaciones = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            }
                        }
                    }

                    // Ejecutar query de exportaciones
                    using (SqlCommand cmd = new SqlCommand(sqlExportaciones, cn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                valorExportado = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                cantExportaciones = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            }
                        }
                    }
                }

                return (valorImportado, valorExportado, cantImportaciones, cantExportaciones);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular datos de BD {baseDatos}: {ex.Message}", ex);
            }
        }



        public List<RazonSocial> ObtenerRazonesSociales()
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT IdRazon, NOMBRE_RAZON, DB FROM RAZONXTABLA ORDER BY NOMBRE_RAZON";
                List<RazonSocial> razones = new List<RazonSocial>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            razones.Add(new RazonSocial
                            {
                                IdRazon = reader.GetInt32(0),
                                NombreRazon = reader.GetString(1),
                                BaseDatosOrigen = reader.GetString(2)
                            });
                        }
                    }
                }

                return razones;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razones sociales: {ex.Message}", ex);
            }
        }

        public List<string> ObtenerBasesDatosRazon(int idRazon)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
                List<string> basesDatos = new List<string>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            basesDatos.Add(reader.GetString(0));
                        }
                    }
                }

                return basesDatos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos de la razón: {ex.Message}", ex);
            }
        }

        private RazonSocial ObtenerRazonSocial(int idRazon)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT IdRazon, NOMBRE_RAZON, DB FROM RAZONXTABLA WHERE IdRazon = @IdRazon";

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new RazonSocial
                            {
                                IdRazon = reader.GetInt32(0),
                                NombreRazon = reader.GetString(1),
                                BaseDatosOrigen = reader.GetString(2)
                            };
                        }
                    }
                }

                throw new Exception("Razón social no encontrada");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razón social: {ex.Message}", ex);
            }
        }

        private List<PedimentoComparacion> ValidarPedimentosCruzados(
            string baseDatosSeleccionada,
            string baseDatosOrigen,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            try
            {
                // 🔍 PASO 1: Obtener información de conexión de la BASE SELECCIONADA (desde NOM_TABLARAZON)
                var conexionBaseSeleccionada = ObtenerConexionDesdeNomTablaRazon(baseDatosSeleccionada);
                string servidorSeleccionada = conexionBaseSeleccionada.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";

                // 🔍 PASO 2: Obtener información de conexión de la BASE ORIGEN/GLOSA (desde RAZONXTABLA)
                var conexionBaseOrigen = ObtenerConexionExterna(baseDatosOrigen);
                string servidorOrigen = conexionBaseOrigen.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";

                System.Diagnostics.Debug.WriteLine($"\n🔍 VALIDAR PEDIMENTOS CRUZADOS");
                System.Diagnostics.Debug.WriteLine($"   ═══════════════════════════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"   📋 BASE SELECCIONADA (Di_Pedimento/De_Pedimento):");
                System.Diagnostics.Debug.WriteLine($"      • Nombre: {baseDatosSeleccionada}");
                System.Diagnostics.Debug.WriteLine($"      • Tabla origen: NOM_TABLARAZON");
                System.Diagnostics.Debug.WriteLine($"      • Conexión externa: {(conexionBaseSeleccionada.TieneConexionExterna ? "Sí" : "No")}");
                System.Diagnostics.Debug.WriteLine($"      • IdConexion: {conexionBaseSeleccionada.IdConexion?.ToString() ?? "NULL (usa servidor principal)"}");
                System.Diagnostics.Debug.WriteLine($"      • Servidor resuelto: {conexionBaseSeleccionada.Servidor ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"      • Servidor final: {servidorSeleccionada}");
                System.Diagnostics.Debug.WriteLine($"      • Usuario: {conexionBaseSeleccionada.UsuarioSQL ?? conexionInfo.UsuarioSQL ?? "N/A"}");
                System.Diagnostics.Debug.WriteLine($"   ");
                System.Diagnostics.Debug.WriteLine($"   📊 BASE ORIGEN/GLOSA (TR_Glosa):");
                System.Diagnostics.Debug.WriteLine($"      • Nombre: {baseDatosOrigen}");
                System.Diagnostics.Debug.WriteLine($"      • Tabla origen: RAZONXTABLA");
                System.Diagnostics.Debug.WriteLine($"      • Conexión externa: {(conexionBaseOrigen.TieneConexionExterna ? "Sí" : "No")}");
                System.Diagnostics.Debug.WriteLine($"      • IdConexion: {conexionBaseOrigen.IdConexion?.ToString() ?? "NULL (usa servidor principal)"}");
                System.Diagnostics.Debug.WriteLine($"      • Servidor resuelto: {conexionBaseOrigen.Servidor ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"      • Servidor final: {servidorOrigen}");
                System.Diagnostics.Debug.WriteLine($"      • Usuario: {conexionBaseOrigen.UsuarioSQL ?? conexionInfo.UsuarioSQL ?? "N/A"}");
                System.Diagnostics.Debug.WriteLine($"   ═══════════════════════════════════════════════════════════");

                // ⚠️ VALIDACIÓN: Si están en servidores diferentes, usar estrategia alternativa

                // VALIDACIÓN 1: Comparar IdConexion (si son diferentes y ninguno es NULL, son servidores diferentes)
                bool mismoPorIdConexion = true;
                if (conexionBaseSeleccionada.IdConexion.HasValue && conexionBaseOrigen.IdConexion.HasValue)
                {
                    mismoPorIdConexion = conexionBaseSeleccionada.IdConexion.Value == conexionBaseOrigen.IdConexion.Value;
                }
                // ⚠️ IMPORTANTE: Si alguna de las dos NO tiene IdConexion, significa que usa conexión principal
                // Solo podemos considerarlas iguales si AMBAS usan conexión principal (ambas NULL)
                else if (conexionBaseSeleccionada.IdConexion.HasValue || conexionBaseOrigen.IdConexion.HasValue)
                {
                    // Una tiene IdConexion y la otra no → son conexiones diferentes
                    mismoPorIdConexion = false;
                }

                // VALIDACIÓN 2: Comparar servidores por IP/nombre
                bool mismoServidor = SonMismoServidor(servidorSeleccionada, servidorOrigen);

                // VALIDACIÓN 3: Comparar credenciales (usuario SQL)
                string usuarioSeleccionada = conexionBaseSeleccionada.UsuarioSQL ?? conexionInfo.UsuarioSQL ?? "";
                string usuarioOrigen = conexionBaseOrigen.UsuarioSQL ?? conexionInfo.UsuarioSQL ?? "";
                bool mismoUsuario = usuarioSeleccionada.Equals(usuarioOrigen, StringComparison.OrdinalIgnoreCase);

                // DECISIÓN FINAL: Son el mismo servidor Y misma conexión SOLO si TODAS las validaciones lo confirman
                // ⚠️ CAMBIO CRÍTICO: No basta con mismo servidor, deben tener las mismas credenciales
                bool realmenteMismaConexion = mismoPorIdConexion && mismoServidor && mismoUsuario;

                System.Diagnostics.Debug.WriteLine($"\n   🔍 COMPARACIÓN DE CONEXIONES:");
                System.Diagnostics.Debug.WriteLine($"      • Servidor base seleccionada: '{servidorSeleccionada}' (IdConexion: {conexionBaseSeleccionada.IdConexion?.ToString() ?? "NULL"})");
                System.Diagnostics.Debug.WriteLine($"      • Servidor base glosa: '{servidorOrigen}' (IdConexion: {conexionBaseOrigen.IdConexion?.ToString() ?? "NULL"})");
                System.Diagnostics.Debug.WriteLine($"      • Usuario base seleccionada: '{usuarioSeleccionada}'");
                System.Diagnostics.Debug.WriteLine($"      • Usuario base glosa: '{usuarioOrigen}'");
                System.Diagnostics.Debug.WriteLine($"      • ¿Mismo por IdConexion?: {(mismoPorIdConexion ? "SÍ" : "NO")}");
                System.Diagnostics.Debug.WriteLine($"      • ¿Mismo por IP/Nombre?: {(mismoServidor ? "SÍ" : "NO")}");
                System.Diagnostics.Debug.WriteLine($"      • ¿Mismo usuario SQL?: {(mismoUsuario ? "SÍ" : "NO")}");
                System.Diagnostics.Debug.WriteLine($"      • ✅ DECISIÓN FINAL: {(realmenteMismaConexion ? "MISMA CONEXIÓN (puede usar JOIN)" : "CONEXIONES DIFERENTES (usar estrategia multi-servidor)")}");

                if (!realmenteMismaConexion)
                {
                    System.Diagnostics.Debug.WriteLine($"\n   ⚠️ ADVERTENCIA: Las bases de datos NO comparten la misma conexión");
                    System.Diagnostics.Debug.WriteLine($"      • Base seleccionada: {baseDatosSeleccionada} → {servidorSeleccionada} (Usuario: {usuarioSeleccionada}, IdConexion: {conexionBaseSeleccionada.IdConexion?.ToString() ?? "NULL"})");
                    System.Diagnostics.Debug.WriteLine($"      • Base glosa: {baseDatosOrigen} → {servidorOrigen} (Usuario: {usuarioOrigen}, IdConexion: {conexionBaseOrigen.IdConexion?.ToString() ?? "NULL"})");
                    System.Diagnostics.Debug.WriteLine($"   ✅ Usando estrategia alternativa: Consultas separadas + validación en memoria");

                    // Estrategia para multi-servidor: obtener pedimentos de cada servidor por separado
                    return ValidarPedimentosCruzadosMultiServidor(
                        baseDatosSeleccionada,
                        baseDatosOrigen,
                        fechaInicio,
                        fechaFin
                    );
                }

                System.Diagnostics.Debug.WriteLine($"   ✅ Ambas bases comparten la MISMA CONEXIÓN: {servidorSeleccionada} (Usuario: {usuarioSeleccionada})");
                System.Diagnostics.Debug.WriteLine($"   ✅ Usando estrategia optimizada: JOIN directo en SQL");
                System.Diagnostics.Debug.WriteLine($"\n   📋 Query a ejecutar:");
                System.Diagnostics.Debug.WriteLine($"   Conexión: {servidorSeleccionada}");
                System.Diagnostics.Debug.WriteLine($"   Di_Pedimento y De_Pedimento de: [{baseDatosSeleccionada}]");
                System.Diagnostics.Debug.WriteLine($"   TR_Glosa de: [{baseDatosOrigen}]");
                System.Diagnostics.Debug.WriteLine($"   (Ambas deben estar en el mismo servidor: {servidorSeleccionada})\n");

                // ✅ USAR LA CONEXIÓN CORRECTA de la base seleccionada (ya resuelta desde NOM_TABLARAZON)
                string sql = $@"
                SELECT
                    X.Tipo,
                    X.Aduana,
                    X.Patente,
                    X.Pedimento,
                    X.FechaPago
                FROM
                (
                    SELECT DISTINCT
                        'IMPORTACION' AS Tipo,
                        DI.Adu_AduanaSecc AS Aduana,
                        DI.AgP_Patente AS Patente,
                        DI.Pim_Folio AS Pedimento,
                        DI.Pim_FechaPago AS FechaPago
                    FROM [{baseDatosSeleccionada}].dbo.Di_Pedimento DI
                    WHERE DI.Pim_FechaPago >= @FechaInicio
                      AND DI.Pim_FechaPago <= @FechaFin
                      AND EXISTS (
                          SELECT 1 
                          FROM [{baseDatosOrigen}].dbo.TR_Glosa G
                          WHERE G.Gl_Aduana = DI.Adu_AduanaSecc
                            AND G.Gl_Patente = DI.AgP_Patente
                            AND G.Gl_Pedimento = DI.Pim_Folio
                            AND G.Gl_TOper = 1
                      )

                    UNION ALL

                    SELECT DISTINCT
                        'EXPORTACION' AS Tipo,
                        DE.Adu_AduanaSecc AS Aduana,
                        DE.AgP_Patente AS Patente,
                        DE.Pex_Folio AS Pedimento,
                        DE.Pex_FechaPago AS FechaPago
                    FROM [{baseDatosSeleccionada}].dbo.De_Pedimento DE
                    WHERE DE.Pex_FechaPago >= @FechaInicio
                      AND DE.Pex_FechaPago <= @FechaFin
                      AND EXISTS (
                          SELECT 1 
                          FROM [{baseDatosOrigen}].dbo.TR_Glosa G
                          WHERE G.Gl_Aduana = DE.Adu_AduanaSecc
                            AND G.Gl_Patente = DE.AgP_Patente
                            AND G.Gl_Pedimento = DE.Pex_Folio
                            AND G.Gl_TOper = 2
                      )
                ) X
                ORDER BY X.Tipo, X.Aduana, X.Patente, X.Pedimento";

                List<PedimentoComparacion> pedimentos = new List<PedimentoComparacion>();

                // ✅ IMPORTANTE: Usar la conexión ya resuelta para la base seleccionada
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatosSeleccionada, conexionBaseSeleccionada))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedimentos.Add(new PedimentoComparacion
                            {
                                Tipo = reader.GetString(0),
                                Aduana = reader.GetString(1),
                                Patente = reader.GetString(2),
                                Pedimento = reader.GetString(3),
                                FechaPago = reader.GetDateTime(4),
                                ExisteEnGlosa = true
                            });
                        }
                    }
                }

                return pedimentos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar pedimentos cruzados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida pedimentos cruzados cuando las bases de datos están en servidores diferentes
        /// Obtiene los pedimentos de cada servidor por separado y los compara en memoria
        /// </summary>
        private List<PedimentoComparacion> ValidarPedimentosCruzadosMultiServidor(
            string baseDatosSeleccionada,
            string baseDatosOrigen,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            try
            {
                // 🔍 Obtener información de conexión de ambas bases
                var conexionBaseSeleccionada = ObtenerConexionDesdeNomTablaRazon(baseDatosSeleccionada);
                var conexionBaseOrigen = ObtenerConexionExterna(baseDatosOrigen);

                System.Diagnostics.Debug.WriteLine($"\n🔀 VALIDACIÓN MULTI-SERVIDOR (Estrategia de Variables)");
                System.Diagnostics.Debug.WriteLine($"   📌 NO se usarán JOINs entre servidores diferentes");
                System.Diagnostics.Debug.WriteLine($"   📌 Se capturarán pedimentos y luego se validarán con variables");

                var pedimentosValidos = new List<PedimentoComparacion>();

                // ═══════════════════════════════════════════════════════════════════
                // PASO 1: OBTENER PEDIMENTOS DE IMPORTACIÓN
                // ═══════════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine($"\n   📋 PASO 1A: Obtener IMPORTACIONES de {baseDatosSeleccionada}");

                var importaciones = ObtenerPedimentosImportacion(baseDatosSeleccionada, conexionBaseSeleccionada, fechaInicio, fechaFin);
                System.Diagnostics.Debug.WriteLine($"   ✅ {importaciones.Count} importaciones encontradas");

                if (importaciones.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"\n   📋 PASO 1B: Validar importaciones contra TR_Glosa en {baseDatosOrigen}");
                    var importacionesValidadas = ValidarPedimentosContraGlosa(
                        importaciones,
                        baseDatosOrigen,
                        conexionBaseOrigen,
                        tipoOperacion: 1, // Importación
                        fechaInicio,
                        fechaFin
                    );

                    System.Diagnostics.Debug.WriteLine($"   ✅ {importacionesValidadas.Count} importaciones VALIDADAS en glosa");
                    pedimentosValidos.AddRange(importacionesValidadas);
                }

                // ═══════════════════════════════════════════════════════════════════
                // PASO 2: OBTENER PEDIMENTOS DE EXPORTACIÓN
                // ═══════════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine($"\n   📋 PASO 2A: Obtener EXPORTACIONES de {baseDatosSeleccionada}");

                var exportaciones = ObtenerPedimentosExportacion(baseDatosSeleccionada, conexionBaseSeleccionada, fechaInicio, fechaFin);
                System.Diagnostics.Debug.WriteLine($"   ✅ {exportaciones.Count} exportaciones encontradas");

                if (exportaciones.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"\n   📋 PASO 2B: Validar exportaciones contra TR_Glosa en {baseDatosOrigen}");
                    var exportacionesValidadas = ValidarPedimentosContraGlosa(
                        exportaciones,
                        baseDatosOrigen,
                        conexionBaseOrigen,
                        tipoOperacion: 2, // Exportación
                        fechaInicio,
                        fechaFin
                    );

                    System.Diagnostics.Debug.WriteLine($"   ✅ {exportacionesValidadas.Count} exportaciones VALIDADAS en glosa");
                    pedimentosValidos.AddRange(exportacionesValidadas);
                }

                // ═══════════════════════════════════════════════════════════════════
                // RESUMEN FINAL
                // ═══════════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine($"\n   ══════════════════════════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"   📊 RESUMEN VALIDACIÓN MULTI-SERVIDOR:");
                System.Diagnostics.Debug.WriteLine($"      • Total pedimentos validados: {pedimentosValidos.Count}");
                System.Diagnostics.Debug.WriteLine($"      • Importaciones: {pedimentosValidos.Count(p => p.Tipo == "IMPORTACION")}");
                System.Diagnostics.Debug.WriteLine($"      • Exportaciones: {pedimentosValidos.Count(p => p.Tipo == "EXPORTACION")}");
                System.Diagnostics.Debug.WriteLine($"   ══════════════════════════════════════════════════════════\n");

                return pedimentosValidos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en validación multi-servidor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene solo pedimentos de IMPORTACIÓN de Di_Pedimento
        /// </summary>
        private List<PedimentoComparacion> ObtenerPedimentosImportacion(
            string baseDatos,
            ConexionExternaInfo conexionExternaInfo,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var pedimentos = new List<PedimentoComparacion>();

            string sql = $@"
                SELECT DISTINCT
                    Adu_AduanaSecc AS Aduana,
                    AgP_Patente AS Patente,
                    Pim_Folio AS Pedimento,
                    Pim_FechaPago AS FechaPago
                FROM [{baseDatos}].dbo.Di_Pedimento
                WHERE Pim_FechaPago >= @FechaInicio
                  AND Pim_FechaPago <= @FechaFin";

            try
            {
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos, conexionExternaInfo))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedimentos.Add(new PedimentoComparacion
                            {
                                Tipo = "IMPORTACION",
                                Aduana = reader.GetString(0),
                                Patente = reader.GetString(1),
                                Pedimento = reader.GetString(2),
                                FechaPago = reader.GetDateTime(3),
                                ExisteEnGlosa = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener importaciones de {baseDatos}: {ex.Message}", ex);
            }

            return pedimentos;
        }

        /// <summary>
        /// Obtiene solo pedimentos de EXPORTACIÓN de De_Pedimento
        /// </summary>
        private List<PedimentoComparacion> ObtenerPedimentosExportacion(
            string baseDatos,
            ConexionExternaInfo conexionExternaInfo,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var pedimentos = new List<PedimentoComparacion>();

            string sql = $@"
                SELECT DISTINCT
                    Adu_AduanaSecc AS Aduana,
                    AgP_Patente AS Patente,
                    Pex_Folio AS Pedimento,
                    Pex_FechaPago AS FechaPago
                FROM [{baseDatos}].dbo.De_Pedimento
                WHERE Pex_FechaPago >= @FechaInicio
                  AND Pex_FechaPago <= @FechaFin";

            try
            {
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos, conexionExternaInfo))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedimentos.Add(new PedimentoComparacion
                            {
                                Tipo = "EXPORTACION",
                                Aduana = reader.GetString(0),
                                Patente = reader.GetString(1),
                                Pedimento = reader.GetString(2),
                                FechaPago = reader.GetDateTime(3),
                                ExisteEnGlosa = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener exportaciones de {baseDatos}: {ex.Message}", ex);
            }

            return pedimentos;
        }

        /// <summary>
        /// Valida una lista de pedimentos contra TR_Glosa usando variables (sin JOIN entre servidores)
        /// Este método se conecta SOLO al servidor de la glosa
        /// </summary>
        private List<PedimentoComparacion> ValidarPedimentosContraGlosa(
            List<PedimentoComparacion> pedimentos,
            string baseDatosGlosa,
            ConexionExternaInfo conexionGlosa,
            int tipoOperacion,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var pedimentosValidados = new List<PedimentoComparacion>();

            if (pedimentos == null || pedimentos.Count == 0)
                return pedimentosValidados;

            try
            {
                // Procesar en lotes de 100 pedimentos para evitar problemas de parámetros
                int batchSize = 100;
                int totalBatches = (int)Math.Ceiling(pedimentos.Count / (double)batchSize);

                System.Diagnostics.Debug.WriteLine($"      📦 Procesando en {totalBatches} lote(s) de máximo {batchSize} pedimentos");

                for (int i = 0; i < pedimentos.Count; i += batchSize)
                {
                    var batch = pedimentos.Skip(i).Take(batchSize).ToList();
                    int batchNumber = (i / batchSize) + 1;

                    System.Diagnostics.Debug.WriteLine($"      🔍 Procesando lote {batchNumber}/{totalBatches} ({batch.Count} pedimentos)");

                    // Crear query con variables para este lote
                    var whereClauses = new List<string>();
                    using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatosGlosa, conexionGlosa))
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.CommandTimeout = 120;

                        int paramIndex = 0;
                        foreach (var ped in batch)
                        {
                            string paramAduana = $"@Aduana{paramIndex}";
                            string paramPatente = $"@Patente{paramIndex}";
                            string paramPedimento = $"@Pedimento{paramIndex}";

                            whereClauses.Add($"(Gl_Aduana = {paramAduana} AND Gl_Patente = {paramPatente} AND Gl_Pedimento = {paramPedimento})");

                            cmd.Parameters.AddWithValue(paramAduana, ped.Aduana);
                            cmd.Parameters.AddWithValue(paramPatente, ped.Patente);
                            cmd.Parameters.AddWithValue(paramPedimento, ped.Pedimento);

                            paramIndex++;
                        }

                        string sql = $@"
                            SELECT DISTINCT
                                Gl_Aduana,
                                Gl_Patente,
                                Gl_Pedimento
                            FROM [{baseDatosGlosa}].dbo.TR_Glosa
                            WHERE Gl_TOper = @TipoOper
                              AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin
                              AND Gl_OrigenZipGlosa = 'S'
                              AND (
                                {string.Join(" OR ", whereClauses)}
                              )";

                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@TipoOper", tipoOperacion);
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                        cn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var glosaPedimentos = new HashSet<string>();
                            while (reader.Read())
                            {
                                string key = $"{reader.GetString(0)}-{reader.GetString(1)}-{reader.GetString(2)}";
                                glosaPedimentos.Add(key);
                            }

                            // Marcar los pedimentos que existen en glosa
                            foreach (var ped in batch)
                            {
                                string key = $"{ped.Aduana}-{ped.Patente}-{ped.Pedimento}";
                                if (glosaPedimentos.Contains(key))
                                {
                                    ped.ExisteEnGlosa = true;
                                    pedimentosValidados.Add(ped);
                                }
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"         ✓ Validados: {batch.Count(p => p.ExisteEnGlosa)} de {batch.Count}");
                }

                return pedimentosValidados;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar pedimentos contra glosa en {baseDatosGlosa}: {ex.Message}", ex);
            }
        }

    /*

        /// <summary>
        /// Obtiene pedimentos de Di_Pedimento y De_Pedimento de una base de datos específica
        /// </summary>
        private List<PedimentoComparacion> ObtenerPedimentosDeBaseDatos(
            string baseDatos,
            ConexionExternaInfo conexionExternaInfo,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var pedimentos = new List<PedimentoComparacion>();

            string sql = $@"
                SELECT
                    'IMPORTACION' AS Tipo,
                    Adu_AduanaSecc AS Aduana,
                    AgP_Patente AS Patente,
                    Pim_Folio AS Pedimento,
                    Pim_FechaPago AS FechaPago
                FROM [{baseDatos}].dbo.Di_Pedimento
                WHERE Pim_FechaPago >= @FechaInicio
                  AND Pim_FechaPago <= @FechaFin

                UNION ALL

                SELECT
                    'EXPORTACION' AS Tipo,
                    Adu_AduanaSecc AS Aduana,
                    AgP_Patente AS Patente,
                    Pex_Folio AS Pedimento,
                    Pex_FechaPago AS FechaPago
                FROM [{baseDatos}].dbo.De_Pedimento
                WHERE Pex_FechaPago >= @FechaInicio
                  AND Pex_FechaPago <= @FechaFin";

            try
            {
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos, conexionExternaInfo))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 120;

                    cn.Open();

                    System.Diagnostics.Debug.WriteLine($"   🔌 Conexión abierta para {baseDatos}");
                    System.Diagnostics.Debug.WriteLine($"      Servidor: {cn.DataSource}");
                    System.Diagnostics.Debug.WriteLine($"      Base de datos: {cn.Database}");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedimentos.Add(new PedimentoComparacion
                            {
                                Tipo = reader.GetString(0),
                                Aduana = reader.GetString(1),
                                Patente = reader.GetString(2),
                                Pedimento = reader.GetString(3),
                                FechaPago = reader.GetDateTime(4),
                                ExisteEnGlosa = false
                            });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                var conexionInfo = ObtenerConexionExterna(baseDatos);
                throw new Exception(
                    $"Error al obtener pedimentos de {baseDatos}:\n" +
                    $"Mensaje SQL: {sqlEx.Message}\n\n" +
                    $"Configuración detectada:\n" +
                    $"  • Base de datos: {baseDatos}\n" +
                    $"  • Tiene conexión externa: {conexionInfo.TieneConexionExterna}\n" +
                    $"  • IdConexion: {conexionInfo.IdConexion?.ToString() ?? "NULL"}\n" +
                    $"  • Servidor configurado: {conexionInfo.Servidor ?? "Servidor principal"}\n\n" +
                    $"SOLUCIÓN:\n" +
                    $"Verifica que la base de datos '{baseDatos}' tenga configurado correctamente:\n" +
                    $"1. ConnExterna = 'S' en NOM_TABLARAZON\n" +
                    $"2. IdConexion con el servidor correcto en NOM_TABLARAZON\n" +
                    $"3. Usuario/contraseña correctos en la tabla Conexiones",
                    sqlEx
                );
            }

            return pedimentos;
        }

        /// <summary>
        /// Obtiene pedimentos de TR_Glosa de una base de datos específica
        /// </summary>
        private List<PedimentoComparacion> ObtenerPedimentosDeGlosa(
            string baseDatos,
            ConexionExternaInfo conexionExternaInfo,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var pedimentos = new List<PedimentoComparacion>();

            string sql = $@"
                SELECT DISTINCT
                    CASE 
                        WHEN Gl_TOper = 1 THEN 'IMPORTACION'
                        WHEN Gl_TOper = 2 THEN 'EXPORTACION'
                        ELSE 'OTRO'
                    END AS Tipo,
                    Gl_Aduana AS Aduana,
                    Gl_Patente AS Patente,
                    Gl_Pedimento AS Pedimento,
                    Gl_FecPagoReal AS FechaPago
                FROM [{baseDatos}].dbo.TR_Glosa
                WHERE Gl_TOper IN (1, 2)
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin
                  AND Gl_OrigenZipGlosa = 'S'";

            using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos, conexionExternaInfo))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                cmd.CommandTimeout = 120;

                cn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pedimentos.Add(new PedimentoComparacion
                        {
                            Tipo = reader.GetString(0),
                            Aduana = reader.GetString(1),
                            Patente = reader.GetString(2),
                            Pedimento = reader.GetString(3),
                            FechaPago = reader.GetDateTime(4),
                            ExisteEnGlosa = true
                        });
                    }
                }
            }

            return pedimentos;
        }

*/
        private decimal ObtenerImportacionesValidadas(
            string baseDatosOrigen,
            List<PedimentoComparacion> pedimentosValidos,
            DateTime ini,
            DateTime fin)
        {
            try
            {
                if (!pedimentosValidos.Any())
                    return 0;

                string servidorOrigen = ObtenerServidorDeBaseDatos(baseDatosOrigen);
                System.Diagnostics.Debug.WriteLine($"\n📊 OBTENER IMPORTACIONES VALIDADAS");
                System.Diagnostics.Debug.WriteLine($"   Base de datos: {baseDatosOrigen}");
                System.Diagnostics.Debug.WriteLine($"   Servidor: {servidorOrigen}");
                System.Diagnostics.Debug.WriteLine($"   Tabla TR_Glosa: [{baseDatosOrigen}].dbo.TR_Glosa");
                System.Diagnostics.Debug.WriteLine($"   Pedimentos a validar: {pedimentosValidos.Count}");

                string pedimentosIn = string.Join(",",
                    pedimentosValidos.Select(p => $"'{p.Aduana}-{p.Patente}-{p.Pedimento}'"));

                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatosOrigen}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN [{baseDatosOrigen}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                AND Gl_CveDocto IN ('IN', 'V1')
                AND Gl_TOper = 1
                AND ide.Identificador IS NULL
                AND Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento IN ({pedimentosIn})
                AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin
                AND Gl_OrigenZipGlosa = 'S'";

                return EjecutarDecimalDirecto(baseDatosOrigen, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener importaciones validadas: {ex.Message}", ex);
            }
        }

        private decimal ObtenerExportacionesValidadas(
            string baseDatosOrigen,
            List<PedimentoComparacion> pedimentosValidos,
            DateTime ini,
            DateTime fin,
            bool incluirMateriaPrima)
        {
            try
            {
                if (!pedimentosValidos.Any())
                    return 0;

                string servidorOrigen = ObtenerServidorDeBaseDatos(baseDatosOrigen);
                System.Diagnostics.Debug.WriteLine($"\n📊 OBTENER EXPORTACIONES VALIDADAS");
                System.Diagnostics.Debug.WriteLine($"   Base de datos: {baseDatosOrigen}");
                System.Diagnostics.Debug.WriteLine($"   Servidor: {servidorOrigen}");
                System.Diagnostics.Debug.WriteLine($"   Tabla TR_Glosa: [{baseDatosOrigen}].dbo.TR_Glosa");
                System.Diagnostics.Debug.WriteLine($"   Pedimentos a validar: {pedimentosValidos.Count}");
                System.Diagnostics.Debug.WriteLine($"   Incluir materia prima: {incluirMateriaPrima}");

                string filtroMateriaPrima = incluirMateriaPrima ? "" : "AND idePT.IDENTIFICADOR IS NOT NULL";
                string pedimentosIn = string.Join(",",
                    pedimentosValidos.Select(p => $"'{p.Aduana}-{p.Patente}-{p.Pedimento}'"));

                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatosOrigen}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ideAF ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ideAF.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('DE')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador, Partida 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaPartidaIdentifica 
                    WHERE Identificador IN ('PT')
                ) idePT ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = idePT.Pedimento 
                    AND idePT.Partida = PAR.Gl_Sec
                LEFT JOIN [{baseDatosOrigen}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                AND Gl_CveDocto IN ('RT', 'V1')
                AND Gl_TOper = 2
                AND ide.Identificador IS NULL
                AND ideAF.Identificador IS NULL
                AND Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento IN ({pedimentosIn})
                AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin
                AND Gl_OrigenZipGlosa = 'S'
                {filtroMateriaPrima}";

                return EjecutarDecimalDirecto(baseDatosOrigen, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener exportaciones validadas: {ex.Message}", ex);
            }
        }

        private decimal EjecutarDecimalDirecto(string baseDatos, string sql, DateTime ini, DateTime fin)
        {
            try
            {
                // ✅ USAR GESTOR DE CONEXIONES para enrutar al servidor correcto
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ini", ini.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));
                    cmd.CommandTimeout = 120;

                    cn.Open();

                    // 🔍 LOGGING: Mostrar servidor y base de datos real
                    System.Diagnostics.Debug.WriteLine($"   🔌 Conexión abierta:");
                    System.Diagnostics.Debug.WriteLine($"      Server: {cn.DataSource}");
                    System.Diagnostics.Debug.WriteLine($"      Database: {cn.Database}");

                    var resultado = cmd.ExecuteScalar();

                    System.Diagnostics.Debug.WriteLine($"   ✅ Query ejecutado exitosamente");
                    System.Diagnostics.Debug.WriteLine($"      Resultado: {resultado ?? "NULL"}");

                    if (resultado == null || resultado == DBNull.Value)
                        return 0;

                    return Convert.ToDecimal(resultado);
                }
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 4060)
            {
                // Error: Cannot open database
                string servidor = ObtenerServidorDeBaseDatos(baseDatos);
                throw new Exception(
                    $"❌ No se puede abrir la base de datos '{baseDatos}'.\n\n" +
                    $"Servidor: {servidor}\n\n" +
                    $"Posibles causas:\n" +
                    $"1. El usuario SQL no tiene permiso para acceder a esta base de datos\n" +
                    $"2. La base de datos no existe en el servidor {servidor}\n" +
                    $"3. Las credenciales del servidor secundario son incorrectas\n\n" +
                    $"Por favor, verifica la configuración en RetornoService.ConfigurarConexionesSecundarias()",
                    sqlEx
                );
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 18456)
            {
                // Error: Login failed
                string servidor = ObtenerServidorDeBaseDatos(baseDatos);
                throw new Exception(
                    $"❌ Error de autenticación SQL.\n\n" +
                    $"Servidor: {servidor}\n" +
                    $"Base de datos: {baseDatos}\n\n" +
                    $"El usuario SQL no puede autenticarse en el servidor.\n" +
                    $"Verifica las credenciales en RetornoService.ConfigurarConexionesSecundarias()",
                    sqlEx
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar consulta en '{baseDatos}': {ex.Message}", ex);
            }
        }

        private void GuardarHistorico(ResultadoRetorno resultado)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'HistoricoRetorno' AND xtype = 'U')
                    BEGIN
                        CREATE TABLE HistoricoRetorno (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            RazonSocial VARCHAR(200),
                            BaseDatos VARCHAR(700),
                            FechaInicio DATE,
                            FechaFin DATE,
                            ValorImportado DECIMAL(28,2),
                            ValorExportado DECIMAL(28,2),
                            PorcentajeRetorno DECIMAL(28,2),
                            IncluyeMateriaPrima BIT,
                            FechaCalculo DATETIME
                        )
                    END

                    INSERT INTO HistoricoRetorno 
                    (RazonSocial, BaseDatos, FechaInicio, FechaFin, ValorImportado, ValorExportado, PorcentajeRetorno, IncluyeMateriaPrima, FechaCalculo)
                    VALUES 
                    (@RazonSocial, @BaseDatos, @FechaInicio, @FechaFin, @ValorImportado, @ValorExportado, @PorcentajeRetorno, @IncluyeMateriaPrima, @FechaCalculo)";

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@RazonSocial", resultado.RazonSocial);
                    cmd.Parameters.AddWithValue("@BaseDatos", resultado.BaseDatos);
                    cmd.Parameters.AddWithValue("@FechaInicio", resultado.FechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", resultado.FechaFin);
                    cmd.Parameters.AddWithValue("@ValorImportado", resultado.ValorImportado);
                    cmd.Parameters.AddWithValue("@ValorExportado", resultado.ValorExportado);
                    cmd.Parameters.AddWithValue("@PorcentajeRetorno", resultado.PorcentajeRetorno);
                    cmd.Parameters.AddWithValue("@IncluyeMateriaPrima", resultado.IncluyeMateriaPrima);
                    cmd.Parameters.AddWithValue("@FechaCalculo", resultado.FechaCalculo);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar histórico: {ex.Message}", ex);
            }
        }

        public List<PedimentosPorRazon> ObtenerPedimentosUltimoMesPorRazon()
        {
            try
            {
                var resultado = new List<PedimentosPorRazon>();

                // Calcular fechas del último mes calendario completo
                DateTime hoy = DateTime.Now;
                DateTime primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);

                // Último mes completo: primer día del mes pasado a las 00:00:00
                DateTime fechaInicio = primerDiaMesActual.AddMonths(-1);

                // Último día del mes pasado a las 23:59:59
                DateTime fechaFin = primerDiaMesActual.AddSeconds(-1);

                // Obtener todas las razones sociales
                List<RazonSocial> razones = ObtenerRazonesSociales();

                foreach (var razon in razones)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(razon.BaseDatosOrigen))
                            continue;

                        var pedimentosRazon = new PedimentosPorRazon
                        {
                            RazonSocial = razon.NombreRazon,
                            BaseDatosOrigen = razon.BaseDatosOrigen
                        };

                        // Obtener cantidad y valor de importaciones
                        var datosImportacion = ObtenerDatosPedimentos(
                            razon.BaseDatosOrigen,
                            fechaInicio,
                            fechaFin,
                            "IMPORTACION"
                        );

                        pedimentosRazon.CantidadImportaciones = datosImportacion.cantidad;
                        pedimentosRazon.ValorImportaciones = datosImportacion.valor;

                        // Obtener cantidad y valor de exportaciones
                        var datosExportacion = ObtenerDatosPedimentos(
                            razon.BaseDatosOrigen,
                            fechaInicio,
                            fechaFin,
                            "EXPORTACION"
                        );

                        pedimentosRazon.CantidadExportaciones = datosExportacion.cantidad;
                        pedimentosRazon.ValorExportaciones = datosExportacion.valor;

                        pedimentosRazon.TotalPedimentos = pedimentosRazon.CantidadImportaciones + pedimentosRazon.CantidadExportaciones;
                        pedimentosRazon.ValorTotal = pedimentosRazon.ValorImportaciones + pedimentosRazon.ValorExportaciones;

                        // Solo agregar si tiene pedimentos
                        if (pedimentosRazon.TotalPedimentos > 0)
                        {
                            resultado.Add(pedimentosRazon);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Continuar con la siguiente razón si hay error
                        System.Diagnostics.Debug.WriteLine($"Error procesando razón {razon.NombreRazon}: {ex.Message}");
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener pedimentos del último mes: {ex.Message}", ex);
            }
        }

        private (int cantidad, decimal valor) ObtenerDatosPedimentos(
            string baseDatos,
            DateTime fechaInicio,
            DateTime fechaFin,
            string tipo)
        {
            try
            {
                string condicionTipo = tipo == "IMPORTACION"
                    ? "AND Gl_CveDocto IN ('IN', 'V1') AND Gl_TOper = 1"
                    : "AND Gl_CveDocto IN ('RT', 'V1') AND Gl_TOper = 2";

                string sql = $@"
                SELECT 
                    COUNT(DISTINCT Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento) as Cantidad,
                    ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0) as Valor
                FROM [{baseDatos}].dbo.TR_Glosa
                WHERE Gl_OrigenZipGlosa = 'S'
                {condicionTipo}
                AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin";

                // Usar el gestor de conexiones para obtener la conexión correcta
                using (SqlConnection cn = ObtenerConexionParaBaseDatos(baseDatos))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ini", fechaInicio.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@fin", fechaFin.ToString("yyyy-MM-dd"));
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int cantidad = reader.GetInt32(0);
                            decimal valor = reader.GetDecimal(1);
                            return (cantidad, valor);
                        }
                    }
                }

                return (0, 0);
            }
            catch (Exception ex)
            {
                string servidor = ObtenerServidorDeBaseDatos(baseDatos);
                throw new Exception($"Error al obtener datos de pedimentos {tipo} de {baseDatos} en servidor {servidor}: {ex.Message}", ex);
            }
        }
    }


    public class ResultadoRetorno
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string BaseDatos { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal ValorImportado { get; set; }
        public decimal ValorExportado { get; set; }
        public decimal PorcentajeRetorno { get; set; }
        public bool IncluyeMateriaPrima { get; set; }
        public DateTime FechaCalculo { get; set; }
        public int CantidadPedimentosImportacion { get; set; }
        public int CantidadPedimentosExportacion { get; set; }
        public int TotalPedimentosValidados { get; set; }
    }
}
