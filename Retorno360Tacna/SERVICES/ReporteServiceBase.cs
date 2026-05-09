using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Servicio base abstracto para reportes
    /// Implementa el patrón Template Method para reutilizar lógica de conexión
    /// </summary>
    public abstract class ReporteServiceBase
    {
        protected readonly ConexionInfo conexionPrincipal;
        protected readonly Dictionary<string, ConexionExternaInfo> cacheConexiones;

        protected ReporteServiceBase(ConexionInfo conexion)
        {
            conexionPrincipal = conexion;
            cacheConexiones = new Dictionary<string, ConexionExternaInfo>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene las razones sociales disponibles
        /// </summary>
        public List<RazonSocial> ObtenerRazonesSociales()
        {
            var razones = new List<RazonSocial>();

            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT DISTINCT 
                        IdRazon,
                        NOMBRE_RAZON,
                        DB
                    FROM RAZONXTABLA
                    WHERE NOMBRE_RAZON IS NOT NULL AND DB IS NOT NULL
                    ORDER BY NOMBRE_RAZON";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, cn);
                cn.Open();

                using var reader = cmd.ExecuteReader();
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
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razones sociales: {ex.Message}", ex);
            }

            return razones;
        }

        /// <summary>
        /// Obtiene las bases de datos asociadas a una razón social por ID
        /// </summary>
        public List<string> ObtenerBasesDatosRazon(int idRazon)
        {
            var bases = new List<string>();

            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT DISTINCT NT.NOMBRE_TABLA 
                    FROM NOM_TABLARAZON NT
                    INNER JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
                    WHERE NT.IdRazon = @IdRazon 
                      AND NT.NOMBRE_TABLA IS NOT NULL
                    ORDER BY NT.NOMBRE_TABLA";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                cn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bases.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos: {ex.Message}", ex);
            }

            return bases;
        }

        /// <summary>
        /// Obtiene el ID de la razón social desde una base de datos específica
        /// </summary>
        public int ObtenerIdRazonDesdeBaseDatos(string baseDatos)
        {
            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT TOP 1 IdRazon 
                    FROM NOM_TABLARAZON 
                    WHERE NOMBRE_TABLA = @BaseDatos";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);
                cn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }

                throw new Exception($"No se encontró la razón social para la base de datos '{baseDatos}'");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener IdRazon desde base de datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene una razón social específica por ID, incluyendo su base de datos de TR_GLOSA
        /// </summary>
        public RazonSocial ObtenerRazonSocial(int idRazon)
        {
            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT IdRazon, NOMBRE_RAZON, DB 
                    FROM RAZONXTABLA 
                    WHERE IdRazon = @IdRazon";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                cn.Open();

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new RazonSocial
                    {
                        IdRazon = reader.GetInt32(0),
                        NombreRazon = reader.GetString(1),
                        BaseDatosOrigen = reader.GetString(2)
                    };
                }

                throw new Exception($"No se encontró la razón social con ID {idRazon}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razón social: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene las bases de datos de una razón social con información de conexión externa
        /// Lee directamente de NOM_TABLARAZON y cruza con Conexiones si es necesario
        /// </summary>
        public List<ConexionExternaInfo> ObtenerBasesDatosConConexion(int idRazon)
        {
            var basesDatos = new List<ConexionExternaInfo>();

            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT 
                        NT.NOMBRE_TABLA AS BaseDatos,
                        R.ConnExterna,
                        R.IdConexion,
                        C.NombreConexion,
                        C.Servidor,
                        C.UsuarioSQL,
                        C.PasswordSQL
                    FROM NOM_TABLARAZON NT
                    LEFT JOIN RAZONXTABLA R ON R.DB = NT.NOMBRE_TABLA AND R.IdRazon = NT.IdRazon
                    LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
                    WHERE NT.IdRazon = @IdRazon AND NT.NOMBRE_TABLA IS NOT NULL
                    ORDER BY NT.NOMBRE_TABLA";

                using var cn = conexion.ObtenerConexion();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                cn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var info = new ConexionExternaInfo
                    {
                        BaseDatos = reader.GetString(0)
                    };

                    // ConnExterna
                    if (!reader.IsDBNull(1))
                    {
                        string connExterna = reader.GetString(1);
                        info.TieneConexionExterna = connExterna.Equals("S", StringComparison.OrdinalIgnoreCase);
                    }

                    // IdConexion
                    if (!reader.IsDBNull(2))
                    {
                        info.IdConexion = reader.GetInt32(2);
                    }

                    // Si tiene conexión externa, leer la información de la conexión
                    if (info.TieneConexionExterna && info.IdConexion.HasValue)
                    {
                        if (!reader.IsDBNull(3)) info.NombreConexion = reader.GetString(3);
                        if (!reader.IsDBNull(4)) info.Servidor = reader.GetString(4);
                        if (!reader.IsDBNull(5)) info.UsuarioSQL = reader.GetString(5);
                        if (!reader.IsDBNull(6)) info.PasswordSQL = reader.GetString(6);
                    }

                    basesDatos.Add(info);

                    // Log de diagnóstico
                    System.Diagnostics.Debug.WriteLine($"Base encontrada: {info.BaseDatos}, ConexionExterna: {info.TieneConexionExterna}");

                    // Cachear la información
                    if (!cacheConexiones.ContainsKey(info.BaseDatos))
                    {
                        cacheConexiones[info.BaseDatos] = info;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total de bases de datos encontradas para IdRazon {idRazon}: {basesDatos.Count}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos con conexión: {ex.Message}", ex);
            }

            return basesDatos;
        }

        /// <summary>
        /// Obtiene la información de conexión externa para una base de datos
        /// Busca primero en RAZONXTABLA (bases origen/glosa) y luego en NOM_TABLARAZON (bases seleccionables)
        /// Template Method: puede ser sobrescrito por clases derivadas
        /// </summary>
        protected virtual ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
        {
            // Verificar cache
            if (cacheConexiones.TryGetValue(baseDatos, out var conexionCacheada))
            {
                return conexionCacheada;
            }

            var conexionExterna = new ConexionExternaInfo { BaseDatos = baseDatos };

            try
            {
                var conexion = new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                using var cn = conexion.ObtenerConexion();
                cn.Open();

                bool encontrado = false;

                // PASO 1: Buscar en RAZONXTABLA (bases origen/glosa)
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

                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlRazonXTabla, cn))
                {
                    cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        encontrado = true;

                        string? connExterna = reader.IsDBNull(0) ? null : reader.GetString(0);
                        conexionExterna.TieneConexionExterna = connExterna?.Trim().Equals("S", StringComparison.OrdinalIgnoreCase) == true;

                        if (!reader.IsDBNull(1))
                        {
                            conexionExterna.IdConexion = reader.GetInt32(1);
                        }

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

                // PASO 2: Si no se encontró en RAZONXTABLA, buscar en NOM_TABLARAZON
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

                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlNomTablaRazon, cn))
                    {
                        cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);

                        using var reader = cmd.ExecuteReader();
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

                // Guardar en cache
                cacheConexiones[baseDatos] = conexionExterna;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener conexión externa para '{baseDatos}': {ex.Message}", ex);
            }

            return conexionExterna;
        }

        /// <summary>
        /// Obtiene la conexión apropiada para una base de datos
        /// Template Method: reutilizable por todas las clases derivadas
        /// </summary>
        protected Conexion ObtenerConexionParaBaseDatos(string baseDatos)
        {
            var infoConexionExterna = ObtenerConexionExterna(baseDatos);

            if (infoConexionExterna.UsarConexionPrincipal)
            {
                // Base de datos en servidor principal
                return new Conexion(
                    conexionPrincipal.Servidor ?? string.Empty,
                    conexionPrincipal.UsuarioSQL ?? string.Empty,
                    conexionPrincipal.PasswordSQL ?? string.Empty,
                    baseDatos
                );
            }
            else
            {
                // Base de datos en servidor externo
                return new Conexion(
                    infoConexionExterna.Servidor ?? string.Empty,
                    infoConexionExterna.UsuarioSQL ?? string.Empty,
                    infoConexionExterna.PasswordSQL ?? string.Empty,
                    baseDatos
                );
            }
        }

        /// <summary>
        /// Limpia el cache de conexiones
        /// </summary>
        public void LimpiarCache()
        {
            cacheConexiones.Clear();
        }
    }
}
