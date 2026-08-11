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
        /// Obtiene la información de conexión externa para una base de datos
        /// Lógica corregida:
        /// 1. Busca primero en NOM_TABLARAZON (bases seleccionables del cliente) con IdRazon correcto
        /// 2. Si no encuentra, busca en RAZONXTABLA (base de glosa)
        /// Template Method: puede ser sobrescrito por clases derivadas
        /// </summary>
        protected virtual ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
        {
            // Verificar cache
            if (cacheConexiones.TryGetValue(baseDatos, out var conexionCacheada))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"💾 Usando conexión cacheada para '{baseDatos}'");
#endif
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

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"\n🔍 Buscando conexión para '{baseDatos}'...");
#endif

                // PASO 1: Buscar en NOM_TABLARAZON (bases seleccionables - bases del cliente)
                // Esta tabla tiene IdConexion que indica el servidor donde está la base
                string sqlNomTablaRazon = @"
                    SELECT TOP 1 
                        NT.IdRazon,
                        NT.IdConexion,
                        C.NombreConexion,
                        C.Servidor,
                        C.UsuarioSQL,
                        C.PasswordSQL
                    FROM NOM_TABLARAZON NT
                    LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
                    WHERE NT.NOMBRE_TABLA = @BaseDatos
                    ORDER BY NT.IdTabla";

                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlNomTablaRazon, cn))
                {
                    cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        encontrado = true;
                        int idRazonEncontrado = reader.GetInt32(0);

#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"   ✅ Encontrado en NOM_TABLARAZON");
                        System.Diagnostics.Debug.WriteLine($"   📋 IdRazon: {idRazonEncontrado}");
#endif

                        // Leer IdConexion (si es NULL = conexión principal, si tiene valor = externa)
                        if (!reader.IsDBNull(1))
                        {
                            conexionExterna.IdConexion = reader.GetInt32(1);
                            conexionExterna.TieneConexionExterna = true;

#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"   🔗 IdConexion: {conexionExterna.IdConexion}");
#endif

                            // Leer datos de la tabla Conexiones (si existe el JOIN)
                            if (!reader.IsDBNull(2))
                            {
                                conexionExterna.NombreConexion = reader.GetString(2);
                                conexionExterna.Servidor = reader.IsDBNull(3) ? null : reader.GetString(3);
                                conexionExterna.UsuarioSQL = reader.IsDBNull(4) ? null : reader.GetString(4);
                                conexionExterna.PasswordSQL = reader.IsDBNull(5) ? null : reader.GetString(5);

#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"   🌐 Servidor: {conexionExterna.Servidor}");
                                System.Diagnostics.Debug.WriteLine($"   👤 Usuario: {conexionExterna.UsuarioSQL}");
#endif
                            }
                        }
                        else
                        {
                            // IdConexion es NULL = usar conexión principal
                            conexionExterna.TieneConexionExterna = false;
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"   🔌 IdConexion: NULL (usar conexión principal)");
#endif
                        }
                    }
                }

                // PASO 2: Si no se encontró en NOM_TABLARAZON, buscar en RAZONXTABLA (base de glosa)
                // Esta tabla tiene ConnExterna='S' o 'N' y también IdConexion
                if (!encontrado)
                {
                    string sqlRazonXTabla = @"
                        SELECT TOP 1 
                            R.IdRazon,
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
                            int idRazonEncontrado = reader.GetInt32(0);

#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"   ✅ Encontrado en RAZONXTABLA (base de glosa)");
                            System.Diagnostics.Debug.WriteLine($"   📋 IdRazon: {idRazonEncontrado}");
#endif

                            // Leer ConnExterna
                            string? connExterna = reader.IsDBNull(1) ? null : reader.GetString(1);

                            // Leer IdConexion
                            if (!reader.IsDBNull(2))
                            {
                                conexionExterna.IdConexion = reader.GetInt32(2);

                                // Si tiene IdConexion, es conexión externa
                                conexionExterna.TieneConexionExterna = true;

#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"   🔗 IdConexion: {conexionExterna.IdConexion}");
                                System.Diagnostics.Debug.WriteLine($"   📌 ConnExterna: {connExterna}");
#endif

                                // Leer datos de la tabla Conexiones
                                if (!reader.IsDBNull(3))
                                {
                                    conexionExterna.NombreConexion = reader.GetString(3);
                                    conexionExterna.Servidor = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    conexionExterna.UsuarioSQL = reader.IsDBNull(5) ? null : reader.GetString(5);
                                    conexionExterna.PasswordSQL = reader.IsDBNull(6) ? null : reader.GetString(6);

#if DEBUG
                                    System.Diagnostics.Debug.WriteLine($"   🌐 Servidor: {conexionExterna.Servidor}");
                                    System.Diagnostics.Debug.WriteLine($"   👤 Usuario: {conexionExterna.UsuarioSQL}");
#endif
                                }
                            }
                            else
                            {
                                // IdConexion es NULL = usar conexión principal
                                conexionExterna.TieneConexionExterna = false;
#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"   🔌 IdConexion: NULL (usar conexión principal)");
#endif
                            }
                        }
                    }
                }

#if DEBUG
                if (!encontrado)
                {
                    System.Diagnostics.Debug.WriteLine($"   ⚠️ '{baseDatos}' NO encontrada en ninguna tabla. Usando conexión principal.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"   ✅ Resultado: {(conexionExterna.TieneConexionExterna ? "EXTERNA" : "PRINCIPAL")}");
                }
                System.Diagnostics.Debug.WriteLine("");
#endif

                // Guardar en cache
                cacheConexiones[baseDatos] = conexionExterna;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"   ❌ Error: {ex.Message}\n");
#endif
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
    }
}
