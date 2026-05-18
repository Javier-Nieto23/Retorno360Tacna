using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.Data;

namespace Retorno360Tacna.SERVICES
{
    public class CatalogoPartesService
    {
        private readonly ConexionInfo conexionInfo;
        private readonly Dictionary<string, ConexionExternaInfo> cacheConexiones = new Dictionary<string, ConexionExternaInfo>();

        public CatalogoPartesService(ConexionInfo conexionInfo)
        {
            this.conexionInfo = conexionInfo;
        }

        public List<RazonSocial> ObtenerRazonesSociales()
        {
            var razones = new List<RazonSocial>();

            try
            {
                var conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razones sociales: {ex.Message}", ex);
            }

            return razones;
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

        private ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
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
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
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

                using (var cmd = new SqlCommand(sqlRazonXTabla, cn))
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

                    using (var cmd = new SqlCommand(sqlNomTablaRazon, cn))
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
                        }
                    }
                }

                // Cachear resultado
                cacheConexiones[baseDatos] = conexionExterna;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener información de conexión para '{baseDatos}': {ex.Message}", ex);
            }

            return conexionExterna;
        }

        public List<ParteBOM> ObtenerCatalogoPartes(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var partes = new List<ParteBOM>();

            string query = @"
                SELECT 
                    cp.Par_NoParte AS NoPartePadre,
                    cp.Par_DescripcionEsp,
                    cp.Par_InsercionFecha,

                    MIN(cb.Bom_FechaIni) AS Bom_FechaInicio,
                    MAX(cb.Bom_FechaFin) AS Bom_FechaFin,

                    COUNT(cb.Par_NoParteHijo) AS TotalComponentes,

                    SUM(CASE WHEN cp.Tim_Clave = 'SUB' THEN 1 ELSE 0 END) AS TotalSUB,
                    SUM(CASE WHEN cp.Tim_Clave = 'MP'  THEN 1 ELSE 0 END) AS TotalMP,
                    SUM(CASE WHEN cp.Tim_Clave = 'EQ'  THEN 1 ELSE 0 END) AS TotalEQ,
                    SUM(CASE WHEN cp.Tim_Clave = 'RT'  THEN 1 ELSE 0 END) AS TotalRT,
                    SUM(CASE WHEN cp.Tim_Clave = 'EMP' THEN 1 ELSE 0 END) AS TotalEMP,
                    SUM(CASE WHEN cp.Tim_Clave = 'MAQ' THEN 1 ELSE 0 END) AS TotalMAQ,

                    SUM(CASE 
                            WHEN cp.Tim_Clave NOT IN ('SUB','MP','EQ','RT','EMP','MAQ') 
                                 OR cp.Tim_Clave IS NULL
                            THEN 1 
                            ELSE 0 
                        END) AS TotalOtros,

                    CASE 
                        WHEN COUNT(cb.Par_NoParteHijo) > 0 
                            THEN 'SI TIENE COMPONENTES'
                        ELSE 'NO TIENE COMPONENTES'
                    END AS EstatusBOM

                FROM Ca_Parte AS cp

                LEFT JOIN Ca_Bom AS cb
                    ON cp.Par_Consecutivo = cb.Par_Padre

                WHERE 
                    cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin

                GROUP BY 
                    cp.Par_NoParte,
                    cp.Par_DescripcionEsp,
                    cp.Par_InsercionFecha

                ORDER BY 
                    TotalComponentes DESC";

            try
            {
                // Obtener información de conexión para la base de datos
                var infoConexion = ObtenerConexionExterna(nombreBaseDatos);

                Conexion conexion;

                // Determinar si usar conexión principal o externa
                if (infoConexion.UsarConexionPrincipal)
                {
                    // Usar servidor principal
                    conexion = new Conexion(
                        conexionInfo.Servidor ?? string.Empty,
                        conexionInfo.UsuarioSQL ?? string.Empty,
                        conexionInfo.PasswordSQL ?? string.Empty,
                        nombreBaseDatos
                    );
                }
                else
                {
                    // Usar servidor externo
                    conexion = new Conexion(
                        infoConexion.Servidor ?? string.Empty,
                        infoConexion.UsuarioSQL ?? string.Empty,
                        infoConexion.PasswordSQL ?? string.Empty,
                        nombreBaseDatos
                    );
                }

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var parte = new ParteBOM
                                {
                                    NoPartePadre = reader["NoPartePadre"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    Par_InsercionFecha = reader["Par_InsercionFecha"] as DateTime?,
                                    Bom_FechaInicio = reader["Bom_FechaInicio"] as DateTime?,
                                    Bom_FechaFin = reader["Bom_FechaFin"] as DateTime?,
                                    TotalComponentes = reader["TotalComponentes"] != DBNull.Value ? Convert.ToInt32(reader["TotalComponentes"]) : 0,
                                    TotalSUB = reader["TotalSUB"] != DBNull.Value ? Convert.ToInt32(reader["TotalSUB"]) : 0,
                                    TotalMP = reader["TotalMP"] != DBNull.Value ? Convert.ToInt32(reader["TotalMP"]) : 0,
                                    TotalEQ = reader["TotalEQ"] != DBNull.Value ? Convert.ToInt32(reader["TotalEQ"]) : 0,
                                    TotalRT = reader["TotalRT"] != DBNull.Value ? Convert.ToInt32(reader["TotalRT"]) : 0,
                                    TotalEMP = reader["TotalEMP"] != DBNull.Value ? Convert.ToInt32(reader["TotalEMP"]) : 0,
                                    TotalMAQ = reader["TotalMAQ"] != DBNull.Value ? Convert.ToInt32(reader["TotalMAQ"]) : 0,
                                    TotalOtros = reader["TotalOtros"] != DBNull.Value ? Convert.ToInt32(reader["TotalOtros"]) : 0,
                                    EstatusBOM = reader["EstatusBOM"]?.ToString() ?? string.Empty
                                };

                                partes.Add(parte);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener catálogo de partes: {ex.Message}", ex);
            }

            return partes;
        }

        public DataTable ConvertirADataTable(List<ParteBOM> partes)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("NO PARTE", typeof(string));
            dt.Columns.Add("DESCRIPCIÓN", typeof(string));
            dt.Columns.Add("FECHA INSERCIÓN", typeof(DateTime));
            dt.Columns.Add("BOM INICIO", typeof(DateTime));
            dt.Columns.Add("BOM FIN", typeof(DateTime));
            dt.Columns.Add("TOTAL COMPONENTES", typeof(int));
            dt.Columns.Add("SUB", typeof(int));
            dt.Columns.Add("MP", typeof(int));
            dt.Columns.Add("EQ", typeof(int));
            dt.Columns.Add("RT", typeof(int));
            dt.Columns.Add("OTROS", typeof(int));
            dt.Columns.Add("ESTATUS BOM", typeof(string));

            foreach (var parte in partes)
            {
                DataRow row = dt.NewRow();
                row["NO PARTE"] = parte.NoPartePadre;
                row["DESCRIPCIÓN"] = parte.Par_DescripcionEsp;
                row["FECHA INSERCIÓN"] = parte.Par_InsercionFecha.HasValue ? (object)parte.Par_InsercionFecha.Value : DBNull.Value;
                row["BOM INICIO"] = parte.Bom_FechaInicio.HasValue ? (object)parte.Bom_FechaInicio.Value : DBNull.Value;
                row["BOM FIN"] = parte.Bom_FechaFin.HasValue ? (object)parte.Bom_FechaFin.Value : DBNull.Value;
                row["TOTAL COMPONENTES"] = parte.TotalComponentes;
                row["SUB"] = parte.TotalSUB;
                row["MP"] = parte.TotalMP;
                row["EQ"] = parte.TotalEQ;
                row["RT"] = parte.TotalRT;
                row["OTROS"] = parte.TotalOtros;
                row["ESTATUS BOM"] = parte.EstatusBOM;

                dt.Rows.Add(row);
            }

            return dt;
        }

        public List<DetalleComponente> ObtenerDetalleComponentes(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var detalles = new List<DetalleComponente>();

            string query = @"
                SELECT 
                    cp.Par_NoParte AS NoPartePadre,
                    cp.Par_DescripcionEsp,
                    cp.Par_InsercionFecha,

                    MIN(cb.Bom_FechaIni) AS Bom_FechaInicio,
                    MAX(cb.Bom_FechaFin) AS Bom_FechaFin,

                    COUNT(cb.Par_NoParteHijo) AS TotalComponentes,

                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'SUB' THEN 1 ELSE 0 END) AS TotalSUB,
                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'MP'  THEN 1 ELSE 0 END) AS TotalMP,
                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'EQ'  THEN 1 ELSE 0 END) AS TotalEQ,
                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'RT'  THEN 1 ELSE 0 END) AS TotalRT,
                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'EMP' THEN 1 ELSE 0 END) AS TotalEMP,
                    SUM(CASE WHEN cp_hijo.Tim_Clave = 'MAQ' THEN 1 ELSE 0 END) AS TotalMAQ,

                    SUM(CASE 
                            WHEN cp_hijo.Tim_Clave NOT IN ('SUB','MP','EQ','RT','EMP','MAQ') 
                                 OR cp_hijo.Tim_Clave IS NULL
                            THEN 1 
                            ELSE 0 
                        END) AS TotalOtros,

                    CASE 
                        WHEN COUNT(cb.Par_NoParteHijo) > 0 
                            THEN 'SI TIENE COMPONENTES'
                        ELSE 'NO TIENE COMPONENTES'
                    END AS EstatusBOM

                FROM Ca_Parte AS cp

                LEFT JOIN Ca_Bom AS cb
                    ON cp.Par_Consecutivo = cb.Par_Padre

                LEFT JOIN Ca_Parte AS cp_hijo
                    ON cb.Par_NoParteHijo = cp_hijo.Par_NoParte

                WHERE 
                    cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin

                GROUP BY 
                    cp.Par_NoParte,
                    cp.Par_DescripcionEsp,
                    cp.Par_InsercionFecha

                ORDER BY 
                    TotalComponentes DESC";

            try
            {
                var infoConexion = ObtenerConexionExterna(nombreBaseDatos);
                Conexion conexion;

                if (infoConexion.UsarConexionPrincipal)
                {
                    conexion = new Conexion(
                        conexionInfo.Servidor ?? string.Empty,
                        conexionInfo.UsuarioSQL ?? string.Empty,
                        conexionInfo.PasswordSQL ?? string.Empty,
                        nombreBaseDatos
                    );
                }
                else
                {
                    conexion = new Conexion(
                        infoConexion.Servidor ?? string.Empty,
                        infoConexion.UsuarioSQL ?? string.Empty,
                        infoConexion.PasswordSQL ?? string.Empty,
                        nombreBaseDatos
                    );
                }

                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var detalle = new DetalleComponente
                                {
                                    NoPartePadre = reader["NoPartePadre"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    TotalComponentes = reader["TotalComponentes"] != DBNull.Value ? Convert.ToInt32(reader["TotalComponentes"]) : 0,
                                    TotalSUB = reader["TotalSUB"] != DBNull.Value ? Convert.ToInt32(reader["TotalSUB"]) : 0,
                                    TotalMP = reader["TotalMP"] != DBNull.Value ? Convert.ToInt32(reader["TotalMP"]) : 0,
                                    TotalEQ = reader["TotalEQ"] != DBNull.Value ? Convert.ToInt32(reader["TotalEQ"]) : 0,
                                    TotalRT = reader["TotalRT"] != DBNull.Value ? Convert.ToInt32(reader["TotalRT"]) : 0,
                                    TotalEMP = reader["TotalEMP"] != DBNull.Value ? Convert.ToInt32(reader["TotalEMP"]) : 0,
                                    TotalMAQ = reader["TotalMAQ"] != DBNull.Value ? Convert.ToInt32(reader["TotalMAQ"]) : 0,
                                    TotalOtros = reader["TotalOtros"] != DBNull.Value ? Convert.ToInt32(reader["TotalOtros"]) : 0,
                                    EstatusBOM = reader["EstatusBOM"]?.ToString() ?? string.Empty
                                };

                                detalles.Add(detalle);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalle de componentes: {ex.Message}", ex);
            }

            return detalles;
        }
    }
}
