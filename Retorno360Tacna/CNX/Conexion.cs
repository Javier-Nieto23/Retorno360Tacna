using Microsoft.Data.SqlClient;

namespace Retorno360Tacna.CNX
{
    public class Conexion
    {
        private string connectionString;

        public Conexion()
        {
            connectionString = "Server=172.20.20.26;Database=RetornoMaster;User Id=MedTiempos;Password=T3ch4dm1n;TrustServerCertificate=True;";
        }

        public Conexion(string servidor, string usuarioSQL, string passwordSQL, string baseDatos = "master")
        {
            connectionString = $"Server={servidor};Database={baseDatos};User Id={usuarioSQL};Password={passwordSQL};TrustServerCertificate=True;";
        }

        public SqlConnection ObtenerConexion()
        {
            try
            {
                SqlConnection conexion = new SqlConnection(connectionString);
                return conexion;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear la conexión: {ex.Message}");
            }
        }

        public bool ProbarConexion()
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetConnectionString()
        {
            return connectionString;
        }

        public List<string> ObtenerBasesDatos()
        {
            List<string> basesDatos = new List<string>();

            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    string query = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                basesDatos.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos: {ex.Message}");
            }

            return basesDatos;
        }
    }
}
