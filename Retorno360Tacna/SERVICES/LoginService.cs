using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    public class LoginService
    {
        private Conexion? conexion;

        public LoginService()
        {
            conexion = new Conexion();
        }

        public LoginService(Conexion conexionCustom)
        {
            conexion = conexionCustom;
        }

        public List<ConexionInfo> ObtenerConexiones()
        {
            List<ConexionInfo> conexiones = new List<ConexionInfo>();

            try
            {
                using (SqlConnection conn = conexion!.ObtenerConexion())
                {
                    conn.Open();
                    string query = "SELECT IdConexion, NombreConexion, Servidor, UsuarioSQL, PasswordSQL, TipoMotor, Activo FROM Conexiones WHERE Activo = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                conexiones.Add(new ConexionInfo
                                {
                                    IdConexion = reader.GetInt32(0),
                                    NombreConexion = reader.GetString(1),
                                    Servidor = reader.GetString(2),
                                    UsuarioSQL = reader.GetString(3),
                                    PasswordSQL = reader.GetString(4),
                                    TipoMotor = reader.GetString(5),
                                    Activo = reader.GetBoolean(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener conexiones: {ex.Message}");
            }

            return conexiones;
        }


        public Usuario? ValidarUsuario(string userAlias, string passwordHash)
        {
            try
            {
                using (SqlConnection conn = conexion!.ObtenerConexion())
                {
                    conn.Open();
                    string query = @"
                        SELECT u.IdUsuario, u.UserAlias, u.NombreUsuario, u.ApellidoUsuario, u.Activo, u.IdRol, r.NombreRol 
                        FROM Usuarios u
                        INNER JOIN Roles r ON u.IdRol = r.IdRol
                        WHERE u.UserAlias = @Usuario AND u.PasswordHash = @Password AND u.Activo = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", userAlias);
                        cmd.Parameters.AddWithValue("@Password", passwordHash);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nombreUsuario = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                string apellidoUsuario = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                int idRol = reader.GetInt32(5);
                                string nombreRol = reader.IsDBNull(6) ? "" : reader.GetString(6);

                                return new Usuario
                                {
                                    IdUsuario = reader.GetInt32(0),
                                    NombreUsuario = reader.GetString(1),
                                    NombreCompleto = $"{nombreUsuario} {apellidoUsuario}".Trim(),
                                    Activo = reader.GetBoolean(4),
                                    IdRol = idRol,
                                    NombreRol = nombreRol
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar usuario: {ex.Message}");
            }
        }
    }
}
