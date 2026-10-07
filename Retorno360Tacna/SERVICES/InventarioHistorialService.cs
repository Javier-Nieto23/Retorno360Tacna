using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.FORMS;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Retorno360Tacna.FORMS.FrmCalculoInventarios;

namespace Retorno360Tacna.SERVICES
{
    public class InventarioHistorialService
    {
        private readonly Conexion conexion;

        public InventarioHistorialService()
        {
            conexion = new Conexion();
        }

        // Guardar una lista de resultados de inventario calculados en la base de datos si el mes no existe previamente
        public void GuardarResultados(IEnumerable<ResultadoInventarioMes> resultados)
        {
            using var en = conexion.ObtenerConexion();
            en.Open();

            foreach (var item in resultados)
            {
                if (item.TieneError) continue;

                object valorMes = ResolverValorMes(item);

                using var cmdVerificar = new SqlCommand(
                    "SELECT COUNT(1) FROM InventariosHistorial WHERE mes = @mes AND idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial", en);
                cmdVerificar.Parameters.AddWithValue("@mes", valorMes);
                cmdVerificar.Parameters.AddWithValue("@idEmpresa", item.idEmpresa);
                cmdVerificar.Parameters.AddWithValue("@RazonSocial", item.IdRazonSocial);

                int existe = (int)cmdVerificar.ExecuteScalar();
                if (existe > 0) continue;

                EjecutarInsert(en, item, valorMes);
            }
        }

        public async Task GuardarResultadosAsync(IEnumerable<ResultadoInventarioMes> resultados)
        {
            using var en = conexion.ObtenerConexion();
            await en.OpenAsync();

            foreach (var item in resultados)
            {
                if (item.TieneError) continue;

                object valorMes = ResolverValorMes(item);

                using var cmdVerificar = new SqlCommand(
                    "SELECT COUNT(1) FROM InventariosHistorial WHERE mes = @mes AND idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial", en);
                cmdVerificar.Parameters.AddWithValue("@mes", valorMes);
                cmdVerificar.Parameters.AddWithValue("@idEmpresa", item.idEmpresa);
                cmdVerificar.Parameters.AddWithValue("@RazonSocial", item.IdRazonSocial);

                int existe = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());
                if (existe > 0) continue;

                await EjecutarInsertAsync(en, item, valorMes);
            }
        }

        private static object ResolverValorMes(ResultadoInventarioMes item)
        {
            if (DateTime.TryParse(item.Mes, out var fechaParsed))
                return fechaParsed;

            int mesNumero = 1;
            if (item.NumeroMes > 0 && item.NumeroMes <= 12)
                mesNumero = item.NumeroMes;
            else if (int.TryParse(item.Mes, out int parsedNum) && parsedNum >= 1 && parsedNum <= 12)
                mesNumero = parsedNum;

            return new DateTime(DateTime.Now.Year, mesNumero, 1);
        }

        private static void EjecutarInsert(SqlConnection en, ResultadoInventarioMes item, object valorMes)
        {
            using var cmd = new SqlCommand(@"
                    INSERT INTO InventariosHistorial 
                    (mes, TipoInventario, Operacion, CampoTotal, CampoA, CampoB, Total, idEmpresa, RazonSocial)
                    VALUES 
                    (@mes, @TipoInventario, @Operacion, @CampoTotal, @CampoA, @CampoB, @Total, @idEmpresa, @RazonSocial)", en);
            AgregarParametrosInsert(cmd, item, valorMes);
            cmd.ExecuteNonQuery();
        }

        private static async Task EjecutarInsertAsync(SqlConnection en, ResultadoInventarioMes item, object valorMes)
        {
            using var cmd = new SqlCommand(@"
                    INSERT INTO InventariosHistorial 
                    (mes, TipoInventario, Operacion, CampoTotal, CampoA, CampoB, Total, idEmpresa, RazonSocial)
                    VALUES 
                    (@mes, @TipoInventario, @Operacion, @CampoTotal, @CampoB, @CampoB, @Total, @idEmpresa, @RazonSocial)", en);
            AgregarParametrosInsert(cmd, item, valorMes);
            await cmd.ExecuteNonQueryAsync();
        }

        private static void AgregarParametrosInsert(SqlCommand cmd, ResultadoInventarioMes item, object valorMes)
        {
            cmd.Parameters.AddWithValue("@mes", valorMes);
            cmd.Parameters.AddWithValue("@TipoInventario", item.TipoInventario.ToString());
            cmd.Parameters.AddWithValue("@Operacion", item.Operacion.ToString());
            cmd.Parameters.AddWithValue("@CampoTotal", item.CampoTotal.ToString());
            cmd.Parameters.AddWithValue("@CampoA", string.IsNullOrEmpty(item.CampoA) ? (object)DBNull.Value : item.CampoA);
            cmd.Parameters.AddWithValue("@CampoB", string.IsNullOrEmpty(item.CampoB) ? (object)DBNull.Value : item.CampoB);
            cmd.Parameters.AddWithValue("@Total", item.Total);
            cmd.Parameters.AddWithValue("@idEmpresa", item.idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", item.IdRazonSocial);
        }

        public List<InventarioHistorialItem> ObtenerHistorico(int idEmpresa, int idRazonSocial)
        {
            var lista = new List<InventarioHistorialItem>();

            using var en = conexion.ObtenerConexion();
            en.Open();

            using var cmd = new SqlCommand(@"
                    SELECT mes, TipoInventario, Operacion, CampoTotal, CampoA, CampoB, Total, idEmpresa, RazonSocial
                    FROM InventariosHistorial
                    WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial
                    ORDER BY mes", en);

            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(MapearItem(reader));

            return lista;
        }

        public async Task<List<InventarioHistorialItem>> ObtenerHistoricoAsync(int idEmpresa, int idRazonSocial)
        {
            var lista = new List<InventarioHistorialItem>();

            using var en = conexion.ObtenerConexion();
            await en.OpenAsync();

            using var cmd = new SqlCommand(@"
                    SELECT mes, TipoInventario, Operacion, CampoTotal, CampoA, CampoB, Total, idEmpresa, RazonSocial
                    FROM InventariosHistorial
                    WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial
                    ORDER BY mes", en);

            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                lista.Add(MapearItem(reader));

            return lista;
        }

        private static InventarioHistorialItem MapearItem(SqlDataReader reader)
        {
            DateTime fechaMes = (DateTime)reader["mes"];
            return new InventarioHistorialItem
            {
                NumeroMes = fechaMes.Month,
                Mes = fechaMes.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")),
                TipoInventario = reader["TipoInventario"].ToString() ?? "",
                Operacion = reader["Operacion"].ToString() ?? "",
                CampoTotal = reader["CampoTotal"].ToString() ?? "",
                CampoA = reader["CampoA"] as string,
                CampoB = reader["CampoB"] as string,
                Total = reader["Total"] is DBNull ? 0m : Convert.ToDecimal(reader["Total"]),
                IdEmpresa = reader["idEmpresa"] is DBNull ? 0 : Convert.ToInt32(reader["idEmpresa"]),
                IdRazonSocial = reader["RazonSocial"] is DBNull ? 0 : Convert.ToInt32(reader["RazonSocial"])
            };
        }

        public void ActualizarTotal(int idEmpresa, int idRazonSocial, int numeroMes, int anio, decimal nuevoTotal)
        {
            using var en = conexion.ObtenerConexion();
            en.Open();
            var fecha = new DateTime(anio, numeroMes, 1);
            using var cmd = new SqlCommand(
                @"UPDATE InventariosHistorial SET Total = @Total
                  WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial AND mes = @mes", en);
            cmd.Parameters.AddWithValue("@Total", nuevoTotal);
            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);
            cmd.Parameters.AddWithValue("@mes", fecha);
            cmd.ExecuteNonQuery();
        }

        public async Task ActualizarTotalAsync(int idEmpresa, int idRazonSocial, int numeroMes, int anio, decimal nuevoTotal)
        {
            using var en = conexion.ObtenerConexion();
            await en.OpenAsync();
            var fecha = new DateTime(anio, numeroMes, 1);
            using var cmd = new SqlCommand(
                @"UPDATE InventariosHistorial SET Total = @Total
                  WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial AND mes = @mes", en);
            cmd.Parameters.AddWithValue("@Total", nuevoTotal);
            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);
            cmd.Parameters.AddWithValue("@mes", fecha);
            await cmd.ExecuteNonQueryAsync();
        }

        public void EliminarRegistro(int idEmpresa, int idRazonSocial, int numeroMes, int anio)
        {
            using var en = conexion.ObtenerConexion();
            en.Open();
            var fecha = new DateTime(anio, numeroMes, 1);
            using var cmd = new SqlCommand(
                @"DELETE FROM InventariosHistorial
                  WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial AND mes = @mes", en);
            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);
            cmd.Parameters.AddWithValue("@mes", fecha);
            cmd.ExecuteNonQuery();
        }

        public async Task EliminarRegistroAsync(int idEmpresa, int idRazonSocial, int numeroMes, int anio)
        {
            using var en = conexion.ObtenerConexion();
            await en.OpenAsync();
            var fecha = new DateTime(anio, numeroMes, 1);
            using var cmd = new SqlCommand(
                @"DELETE FROM InventariosHistorial
                  WHERE idEmpresa = @idEmpresa AND RazonSocial = @RazonSocial AND mes = @mes", en);
            cmd.Parameters.AddWithValue("@idEmpresa", idEmpresa);
            cmd.Parameters.AddWithValue("@RazonSocial", idRazonSocial);
            cmd.Parameters.AddWithValue("@mes", fecha);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}