using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;

namespace Retorno360Tacna.SERVICES
{
    public class CatalogoPartesService : ReporteServiceBase
    {
        public CatalogoPartesService(ConexionInfo conexionInfo) : base(conexionInfo)
        {
        }

        private ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
        {
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

                            if (!reader.IsDBNull(0))
                            {
                                conexionExterna.IdConexion = reader.GetInt32(0);
                                conexionExterna.TieneConexionExterna = true;
                            }
                            else
                            {
                                conexionExterna.TieneConexionExterna = false;
                            }

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

                cacheConexiones[baseDatos] = conexionExterna;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener información de conexión para '{baseDatos}': {ex.Message}", ex);
            }

            return conexionExterna;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOM(string nombreBaseDatos, string tipoClave, DateTime fechaInicio, DateTime fechaFin)
        {
            var materiaPrima = new List<MateriaPrimaBOM>();

            string query = @"
                -- Tabla temporal con componentes vigentes en BOM
                DECLARE @componentesVigentesenbom TABLE (
                    componente VARCHAR(100)
                );

                INSERT INTO @componentesVigentesenbom (componente)
                SELECT DISTINCT par_nopartehijo 
                FROM ca_bom WITH (NOLOCK)
                WHERE GETDATE() BETWEEN bom_fechaini AND bom_fechafin;

                -- Variables de fecha
                DECLARE @FechaInicio DATE = @ParamFechaInicio;
                DECLARE @FechaFin DATE = @ParamFechaFin;

                -- Consulta principal de materia prima
                SELECT 
                    cp.Par_NoParte,
                    cp.Par_DescripcionEsp,
                    cp.Tim_Clave AS Clave,
                    cp.Par_InsercionFecha,
                    CASE 
                        WHEN cp.Par_NoParte IN (SELECT componente FROM @componentesVigentesenbom)
                        THEN 'VIGENTE EN BOM'
                        ELSE 'NO ESTA EN BOM'
                    END AS EstatusComponente
                FROM Ca_Parte AS cp WITH (NOLOCK)
                WHERE 
                    cp.Tim_Clave = @TipoClave
                    AND cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin
                ORDER BY 
                    cp.Par_InsercionFecha,
                    cp.Par_NoParte
                OPTION (MAXDOP 4)";

            ConexionExternaInfo? infoConexion = null;

            try
            {
                infoConexion = ObtenerConexionExterna(nombreBaseDatos);
                Conexion conexion;

                if (infoConexion.UsarConexionPrincipal)
                {
                    conexion = new Conexion(
                        conexionPrincipal.Servidor ?? string.Empty,
                        conexionPrincipal.UsuarioSQL ?? string.Empty,
                        conexionPrincipal.PasswordSQL ?? string.Empty,
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
                        cmd.CommandTimeout = 300;

                        // Agregar parámetros
                        cmd.Parameters.AddWithValue("@TipoClave", tipoClave);
                        cmd.Parameters.AddWithValue("@ParamFechaInicio", fechaInicio.Date);
                        cmd.Parameters.AddWithValue("@ParamFechaFin", fechaFin.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var mp = new MateriaPrimaBOM
                                {
                                    Par_NoParte = reader["Par_NoParte"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    Clave = reader["Clave"]?.ToString() ?? string.Empty,
                                    Par_InsercionFecha = reader["Par_InsercionFecha"] == DBNull.Value ? null : (DateTime?)reader["Par_InsercionFecha"],
                                    EstatusComponente = reader["EstatusComponente"]?.ToString() ?? string.Empty
                                };

                                materiaPrima.Add(mp);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string servidor = infoConexion?.UsarConexionPrincipal == true 
                    ? conexionPrincipal.Servidor ?? "desconocido"
                    : infoConexion?.Servidor ?? "desconocido";

                string mensajeError = sqlEx.Number == -2 
                    ? $"Timeout al conectar con el servidor '{servidor}'. Verifica que el servidor esté disponible y accesible."
                    : $"Error de SQL al obtener materia prima BOM: {sqlEx.Message}";
                throw new Exception(mensajeError, sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materia prima BOM: {ex.Message}", ex);
            }

            return materiaPrima;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOMMultiple(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var materiaPrima = new List<MateriaPrimaBOM>();

            string query = @"
                -- Tabla temporal con componentes vigentes en BOM
                DECLARE @componentesVigentesenbom TABLE (
                    componente VARCHAR(100)
                );

                INSERT INTO @componentesVigentesenbom (componente)
                SELECT DISTINCT par_nopartehijo 
                FROM ca_bom WITH (NOLOCK)
                WHERE GETDATE() BETWEEN bom_fechaini AND bom_fechafin;

                -- Variables de fecha
                DECLARE @FechaInicio DATE = @ParamFechaInicio;
                DECLARE @FechaFin DATE = @ParamFechaFin;

                -- Consulta principal con múltiples tipos de clave
                SELECT 
                    cp.Par_NoParte,
                    cp.Par_DescripcionEsp,
                    cp.Tim_Clave AS Clave,
                    cp.Par_InsercionFecha,
                    CASE 
                        WHEN cp.Par_NoParte IN (SELECT componente FROM @componentesVigentesenbom)
                        THEN 'VIGENTE EN BOM'
                        ELSE 'NO ESTA EN BOM'
                    END AS EstatusComponente
                FROM Ca_Parte AS cp WITH (NOLOCK)
                WHERE 
                    cp.Tim_Clave IN ('EQ','MAQ','SUB','RT')
                    AND cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin
                ORDER BY 
                    cp.Par_InsercionFecha,
                    cp.Par_NoParte
                OPTION (MAXDOP 4)";

            ConexionExternaInfo? infoConexion = null;

            try
            {
                infoConexion = ObtenerConexionExterna(nombreBaseDatos);
                Conexion conexion;

                if (infoConexion.UsarConexionPrincipal)
                {
                    conexion = new Conexion(
                        conexionPrincipal.Servidor ?? string.Empty,
                        conexionPrincipal.UsuarioSQL ?? string.Empty,
                        conexionPrincipal.PasswordSQL ?? string.Empty,
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
                        cmd.CommandTimeout = 300;

                        // Agregar parámetros de fecha
                        cmd.Parameters.AddWithValue("@ParamFechaInicio", fechaInicio.Date);
                        cmd.Parameters.AddWithValue("@ParamFechaFin", fechaFin.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var mp = new MateriaPrimaBOM
                                {
                                    Par_NoParte = reader["Par_NoParte"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    Clave = reader["Clave"]?.ToString() ?? string.Empty,
                                    Par_InsercionFecha = reader["Par_InsercionFecha"] == DBNull.Value ? null : (DateTime?)reader["Par_InsercionFecha"],
                                    EstatusComponente = reader["EstatusComponente"]?.ToString() ?? string.Empty
                                };

                                materiaPrima.Add(mp);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string servidor = infoConexion?.UsarConexionPrincipal == true 
                    ? conexionPrincipal.Servidor ?? "desconocido"
                    : infoConexion?.Servidor ?? "desconocido";

                string mensajeError = sqlEx.Number == -2 
                    ? $"Timeout al conectar con el servidor '{servidor}'. Verifica que el servidor esté disponible y accesible."
                    : $"Error de SQL al obtener materia prima BOM: {sqlEx.Message}";
                throw new Exception(mensajeError, sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materia prima BOM: {ex.Message}", ex);
            }

            return materiaPrima;
        }
    }
}
