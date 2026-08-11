using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.HELPERS;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;

namespace Retorno360Tacna.SERVICES
{
    internal sealed class NotificacionService
    {
        private readonly Conexion conexion;

        public NotificacionService(ConexionInfo? conexionInfo = null)
        {
            conexion = conexionInfo != null
                && !string.IsNullOrWhiteSpace(conexionInfo.Servidor)
                && !string.IsNullOrWhiteSpace(conexionInfo.UsuarioSQL)
                && !string.IsNullOrWhiteSpace(conexionInfo.PasswordSQL)
                ? new Conexion(conexionInfo.Servidor, conexionInfo.UsuarioSQL, conexionInfo.PasswordSQL, "RetornoMaster")
                : new Conexion();
        }

        public void RegistrarNotificacion(string titulo, string descripcion, string direccion = "")
        {
            using SqlConnection conn = conexion.ObtenerConexion();
            conn.Open();

            const string query = @"
                INSERT INTO Notificaciones ([date], titulo, descripcion, direccion)
                VALUES (@Fecha, @Titulo, @Descripcion, @Direccion);";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@Titulo", titulo);
            cmd.Parameters.AddWithValue("@Descripcion", descripcion);
            cmd.Parameters.AddWithValue("@Direccion", direccion ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

        public int RegistrarNotificacionYObtenerId(string titulo, string descripcion, string direccion = "")
        {
            using SqlConnection conn = conexion.ObtenerConexion();
            conn.Open();

            const string query = @"
                INSERT INTO Notificaciones ([date], titulo, descripcion, direccion)
                OUTPUT INSERTED.id
                VALUES (@Fecha, @Titulo, @Descripcion, @Direccion);";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@Titulo", titulo);
            cmd.Parameters.AddWithValue("@Descripcion", descripcion);
            cmd.Parameters.AddWithValue("@Direccion", direccion ?? string.Empty);

            object? resultado = cmd.ExecuteScalar();
            return resultado == null || resultado == DBNull.Value ? 0 : Convert.ToInt32(resultado);
        }

        public void EliminarTodas()
        {
            using SqlConnection conn = conexion.ObtenerConexion();
            conn.Open();

            using SqlCommand cmd = new SqlCommand("DELETE FROM Notificaciones;", conn);
            cmd.ExecuteNonQuery();
        }

        public List<R2NotificationCenter.NotificationItem> ObtenerHistorial(int maximo = 200)
        {
            List<R2NotificationCenter.NotificationItem> historial = new();

            using SqlConnection conn = conexion.ObtenerConexion();
            conn.Open();

            const string query = @"
                SELECT TOP (@Top)
                    id,
                    [date],
                    titulo,
                    descripcion,
                    direccion
                FROM Notificaciones
                ORDER BY id DESC;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Top", maximo);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                historial.Add(new R2NotificationCenter.NotificationItem
                {
                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Fecha = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1),
                    Titulo = reader.IsDBNull(2) ? "Notificación" : reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Direccion = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Leida = true
                });
            }

            return historial;
        }
    }
}
