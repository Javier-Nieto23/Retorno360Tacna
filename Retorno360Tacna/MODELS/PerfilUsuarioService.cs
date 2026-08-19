using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    public class PerfilUsuarioService
    {
        private readonly Conexion conexion;

        public PerfilUsuarioService()
        {
            conexion = new Conexion(); // fija a RetornoMaster (constructor sin parámetros)
        }

        // Empresas (tablas) asociadas a una razón social, desde
        // RetornoMaster.NOM_TABLARAZON (IdTabla, NOMBRE_TABLA, IdRazon).
        public List<EmpresaRazon> ObtenerEmpresasDeRazon(int idRazon)
        {
            var resultado = new List<EmpresaRazon>();

            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT IdTabla, NOMBRE_TABLA
                FROM NOM_TABLARAZON
                WHERE IdRazon = @idRazon
                ORDER BY NOMBRE_TABLA", cn);
            cmd.Parameters.AddWithValue("@idRazon", idRazon);

            cn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultado.Add(new EmpresaRazon
                {
                    IdTabla = reader.GetInt32(0),
                    NombreTabla = reader.GetString(1)
                });
            }

            return resultado;
        }

        public List<PerfilUsuarioItem> ObtenerPerfil(int idUsuario)
        {
            var resultado = new List<PerfilUsuarioItem>();

            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT id_Consecutivo, idUsuario, idRazonSocial, idempresa
                FROM PerfilUsuario
                WHERE idUsuario = @idUsuario
                ORDER BY idRazonSocial, idempresa", cn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

            cn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultado.Add(new PerfilUsuarioItem
                {
                    IdConsecutivo = reader.GetInt32(0),
                    IdUsuario = reader.GetInt32(1),
                    IdRazonSocial = reader.GetInt32(2),
                    IdEmpresa = reader.GetInt32(3)
                });
            }

            return resultado;
        }

        // Reemplaza por completo las empresas del usuario para UNA razón social.
        // empresasSeleccionadas = lista de IdTabla (de NOM_TABLARAZON).
        public void GuardarPerfilRazon(int idUsuario, int idRazonSocial, List<int> empresasSeleccionadas)
        {
            using var cn = conexion.ObtenerConexion();
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                using (var cmdDelete = new SqlCommand(@"
                    DELETE FROM PerfilUsuario
                    WHERE idUsuario = @idUsuario AND idRazonSocial = @idRazonSocial", cn, tx))
                {
                    cmdDelete.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmdDelete.Parameters.AddWithValue("@idRazonSocial", idRazonSocial);
                    cmdDelete.ExecuteNonQuery();
                }

                foreach (var idEmpresa in empresasSeleccionadas.Distinct())
                {
                    using var cmdInsert = new SqlCommand(@"
                        INSERT INTO PerfilUsuario (idUsuario, idRazonSocial, idempresa)
                        VALUES (@idUsuario, @idRazonSocial, @idempresa)", cn, tx);
                    cmdInsert.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmdInsert.Parameters.AddWithValue("@idRazonSocial", idRazonSocial);
                    cmdInsert.Parameters.AddWithValue("@idempresa", idEmpresa);
                    cmdInsert.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void EliminarRazonDelPerfil(int idUsuario, int idRazonSocial)
        {
            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(@"
                DELETE FROM PerfilUsuario
                WHERE idUsuario = @idUsuario AND idRazonSocial = @idRazonSocial", cn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@idRazonSocial", idRazonSocial);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        public Dictionary<int, List<int>> ObtenerEmpresasPorRazon(int idUsuario)
        {
            return ObtenerPerfil(idUsuario)
                .GroupBy(p => p.IdRazonSocial)
                .ToDictionary(g => g.Key, g => g.Select(p => p.IdEmpresa).ToList());
        }

        // Razones sociales que el usuario tiene guardadas en su perfil (sin duplicados).
        public List<int> ObtenerRazonesPermitidas(int idUsuario)
        {
            return ObtenerPerfil(idUsuario)
                .Select(p => p.IdRazonSocial)
                .Distinct()
                .ToList();
        }

        // Empresas (IdTabla de NOM_TABLARAZON) que el usuario tiene guardadas
        // para una razón social específica.
        public List<int> ObtenerEmpresasPermitidas(int idUsuario, int idRazonSocial)
        {
            return ObtenerPerfil(idUsuario)
                .Where(p => p.IdRazonSocial == idRazonSocial)
                .Select(p => p.IdEmpresa)
                .ToList();
        }

        // Igual que ObtenerRazonesPermitidas, pero devuelve el objeto RazonSocial
        // completo (con NombreRazon) en vez de solo el IdRazon, listo para
        // enlazar directamente a un ComboBox. RAZONXTABLA vive en RetornoMaster,
        // igual que PerfilUsuario, por eso se resuelve en un solo JOIN.
        public List<RazonSocial> ObtenerRazonesSocialesDePerfil(int idUsuario)
        {
            var resultado = new List<RazonSocial>();

            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT DISTINCT R.IdRazon, R.Nombre_Razon
                FROM PerfilUsuario P
                INNER JOIN RAZONXTABLA R ON R.IdRazon = P.idRazonSocial
                WHERE P.idUsuario = @idUsuario
                ORDER BY R.Nombre_Razon", cn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

            cn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultado.Add(new RazonSocial
                {
                    IdRazon = reader.GetInt32(0),
                    NombreRazon = reader.GetString(1)
                });
            }

            return resultado;
        }

        // Nombres reales de base (NOM_TABLARAZON.NOMBRE_TABLA) que el usuario
        // tiene guardados en su perfil para una razón social específica.
        // Reutiliza ObtenerEmpresasPermitidas + ObtenerEmpresasDeRazon para no
        // duplicar la consulta a NOM_TABLARAZON.
        public List<string> ObtenerBasesDatosDePerfilPorRazon(int idUsuario, int idRazonSocial)
        {
            var idsPermitidos = ObtenerEmpresasPermitidas(idUsuario, idRazonSocial);
            if (idsPermitidos.Count == 0)
                return new List<string>();

            return ObtenerEmpresasDeRazon(idRazonSocial)
                .Where(e => idsPermitidos.Contains(e.IdTabla))
                .Select(e => e.NombreTabla)
                .ToList();
        }

        // Igual que ObtenerBasesDatosDePerfilPorRazon, pero devuelve el objeto
        // EmpresaRazon completo (IdTabla + NombreTabla) en vez de solo el
        // nombre. Úsalo cuando necesites el IdTabla numérico (por ejemplo,
        // para buscar configuración de plantilla por empresa), no solo el
        // nombre real de la base para consultas SQL.
        public List<EmpresaRazon> ObtenerEmpresasDePerfilPorRazon(int idUsuario, int idRazonSocial)
        {
            var idsPermitidos = ObtenerEmpresasPermitidas(idUsuario, idRazonSocial);
            if (idsPermitidos.Count == 0)
                return new List<EmpresaRazon>();

            return ObtenerEmpresasDeRazon(idRazonSocial)
                .Where(e => idsPermitidos.Contains(e.IdTabla))
                .ToList();
        }

        // No se usa en SeleccionEmpresacs actualmente, queda disponible por si
        // se necesita una vista de administración más adelante.
        public List<Usuario> ObtenerUsuarios()
        {
            var resultado = new List<Usuario>();

            using var cn = conexion.ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT IdUsuario, NombreUsuario
                FROM Usuarios
                ORDER BY NombreUsuario", cn);

            cn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultado.Add(new Usuario
                {
                    IdUsuario = reader.GetInt32(0),
                    NombreCompleto = reader.GetString(1)
                });
            }

            return resultado;
        }
    }
}