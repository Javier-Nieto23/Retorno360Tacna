using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Retorno360Tacna.SERVICES
{
    public class VerificacionPartesService
    {
        public class ResultadoVerificacion
        {
            public List<string> PartesExistentes { get; set; } = new();
            public List<string> PartesNoExistentes { get; set; } = new();
        }

        public class DetalleParte
        {
            public string Parte { get; set; } = string.Empty;
            public bool Existe { get; set; }
            public decimal Cantidad { get; set; }
            public string UnidadUsuario { get; set; } = string.Empty;
            public string MedComercial { get; set; } = string.Empty;
            public decimal CostoUnitario { get; set; }
            public decimal TotalCosto { get; set; }
            public bool UmCoincide { get; set; }
            public bool CostoValido { get; set; }
        }

        public class ResultadoVerificacionDetalle
        {
            public List<DetalleParte> Items { get; set; } = new();
        }

        private string ObtenerCadenaConexion(int idRazon, int idEmpresa)
        {
            Conexion cnxBase = new Conexion();
            string querySql = @"
                SELECT c.Servidor, n.NOMBRE_TABLA AS BaseDatos, c.UsuarioSQL, c.PasswordSQL, c.Activo
                FROM NOM_TABLARAZON n
                INNER JOIN Conexiones c ON n.IdConexion = c.IdConexion
                WHERE n.IdTabla = @IdEmpresa AND n.IdRazon = @IdRazon";

            using (SqlConnection conn = new SqlConnection(cnxBase.GetConnectionString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(querySql, conn);
                cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    bool activo = reader["Activo"] != DBNull.Value && Convert.ToInt32(reader["Activo"]) == 1;
                    string servidor = reader["Servidor"]?.ToString() ?? string.Empty;

                    if (activo && !string.IsNullOrWhiteSpace(servidor))
                    {
                        string baseDatos = reader["BaseDatos"]?.ToString() ?? string.Empty;
                        string usuario = reader["UsuarioSQL"]?.ToString() ?? string.Empty;
                        string password = reader["PasswordSQL"]?.ToString() ?? string.Empty;

                        return $"Server={servidor};Database={baseDatos};User Id={usuario};Password={password};TrustServerCertificate=True;";
                    }
                }
            }

            return cnxBase.GetConnectionString();
        }

        private async Task<string> ObtenerCadenaConexionAsync(int idRazon, int idEmpresa)
        {
            Conexion cnxBase = new Conexion();
            string querySql = @"
                SELECT c.Servidor, n.NOMBRE_TABLA AS BaseDatos, c.UsuarioSQL, c.PasswordSQL, c.Activo
                FROM NOM_TABLARAZON n
                INNER JOIN Conexiones c ON n.IdConexion = c.IdConexion
                WHERE n.IdTabla = @IdEmpresa AND n.IdRazon = @IdRazon";

            using (SqlConnection conn = new SqlConnection(cnxBase.GetConnectionString()))
            {
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(querySql, conn);
                cmd.Parameters.AddWithValue("@IdEmpresa", idEmpresa);
                cmd.Parameters.AddWithValue("@IdRazon", idRazon);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    bool activo = reader["Activo"] != DBNull.Value && Convert.ToInt32(reader["Activo"]) == 1;
                    string servidor = reader["Servidor"]?.ToString() ?? string.Empty;

                    if (activo && !string.IsNullOrWhiteSpace(servidor))
                    {
                        string baseDatos = reader["BaseDatos"]?.ToString() ?? string.Empty;
                        string usuario = reader["UsuarioSQL"]?.ToString() ?? string.Empty;
                        string password = reader["PasswordSQL"]?.ToString() ?? string.Empty;

                        return $"Server={servidor};Database={baseDatos};User Id={usuario};Password={password};TrustServerCertificate=True;";
                    }
                }
            }

            return cnxBase.GetConnectionString();
        }

        // Método público que reutiliza la lógica interna para obtener la cadena de conexión
        public async Task<string> ObtenerCadenaConexionPublicAsync(int idRazon, int idEmpresa)
        {
            return await ObtenerCadenaConexionAsync(idRazon, idEmpresa);
        }

        /// <summary>
        /// Verifica partes incluyendo cantidad y unidad de medida, consulta costo y unidad comercial en la base del cliente.
        /// Formato esperado: lista de tuplas (parte, cantidad, unidadUsuario).
        /// </summary>
        public async Task<ResultadoVerificacionDetalle> VerificarPartesConCantidadAsync(int idRazon, int idEmpresa, List<(string Parte, decimal Cantidad, string Unidad)> entradas, List<decimal?> costosUnitariosPorFila = null)
        {
            var resultado = new ResultadoVerificacionDetalle();
            if (entradas == null || entradas.Count == 0)
                return resultado;

            string connectionString = await ObtenerCadenaConexionAsync(idRazon, idEmpresa);

            // Usar un CTE con VALUES para pasar parte+UM y obtener directamente Par, UMBD y costo
            var partesUnicas = entradas.Select(e => (Parte: e.Parte?.Trim() ?? string.Empty, UM: e.Unidad?.Trim() ?? string.Empty))
                                        .Where(t => !string.IsNullOrWhiteSpace(t.Parte))
                                        .GroupBy(t => t.Parte, StringComparer.OrdinalIgnoreCase)
                                        .Select(g => g.First())
                                        .ToList();

            if (partesUnicas.Count == 0)
            {
                // Rellenar con entradas como no existentes
                foreach (var e in entradas)
                {
                    resultado.Items.Add(new DetalleParte
                    {
                        Parte = e.Parte,
                        Cantidad = e.Cantidad,
                        UnidadUsuario = e.Unidad ?? string.Empty,
                        Existe = false,
                        MedComercial = string.Empty,
                        CostoUnitario = 0m,
                        TotalCosto = 0m,
                        UmCoincide = false,
                        CostoValido = false
                    });
                }

                return resultado;
            }

            // Si la lista es muy grande, dividir en lotes para no exceder el límite de parámetros de SQL Server (2100)
            var mapResult = new Dictionary<string, (string? UmEncontrada, decimal? Costo, bool Existe)>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                const int maxParams = 2000; // margen por seguridad
                const int paramsPerItem = 2; // parte + um
                int maxItemsPerBatch = Math.Max(1, maxParams / paramsPerItem); // e.g., 1000

                // skipCostos indica si debemos OMITIR la consulta de costos desde la BD.
                // Si se proporcionaron costosUnitariosPorFila NO debemos omitir (skipCostos = false).
                bool skipCostos = costosUnitariosPorFila == null;
                for (int start = 0; start < partesUnicas.Count; start += maxItemsPerBatch)
                {
                    var batch = partesUnicas.Skip(start).Take(maxItemsPerBatch).ToList();
                    var parametros = new List<string>();
                    var parametrosParams = new List<SqlParameter>();
                    for (int i = 0; i < batch.Count; i++)
                    {
                        string pParte = "@p_par" + i;
                        string pUm = "@p_um" + i;
                        parametros.Add($"({pParte}, {pUm})");
                        parametrosParams.Add(new SqlParameter(pParte, batch[i].Parte));
                        parametrosParams.Add(new SqlParameter(pUm, batch[i].UM));
                    }

                    string sql;
                    if (skipCostos)
                    {
                        sql = $@"
                                WITH PartesSolicitadas AS
                                (
                                    SELECT *
                                    FROM (VALUES {string.Join(", ", parametros)}) AS P(Par_NoParte, UM)
                                )

                                SELECT
                                    P.Par_NoParte AS Parte_Solicitada,
                                    P.UM AS UM_Solicitada,
                                    CA.Par_NoParte AS Parte_Encontrada,
                                    CA.Med_Comercial AS UM_Encontrada
                                FROM PartesSolicitadas P
                                LEFT JOIN Ca_Parte CA
                                    ON CA.Par_NoParte = P.Par_NoParte
                                ORDER BY P.Par_NoParte;";
                    }
                    else
                    {
                        sql = $@"
                                WITH PartesSolicitadas AS
                                (
                                    SELECT *
                                    FROM (VALUES {string.Join(", ", parametros)}) AS P(Par_NoParte, UM)
                                )

                                SELECT
                                    P.Par_NoParte AS Parte_Solicitada,
                                    P.UM AS UM_Solicitada,
                                    CA.Par_NoParte AS Parte_Encontrada,
                                    CA.Med_Comercial AS UM_Encontrada,
                                    CAC.Pac_Costo AS Costo
                                FROM PartesSolicitadas P
                                LEFT JOIN Ca_Parte CA
                                    ON CA.Par_NoParte = P.Par_NoParte
                                LEFT JOIN Ca_ParteCosto CAC
                                    ON CA.Par_Consecutivo = CAC.Par_Consecutivo
                                ORDER BY P.Par_NoParte;";
                    }

                    using SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parametrosParams.ToArray());
                    cmd.CommandTimeout = 300;

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string parteSolicitada = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
                        string umSolicitada = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
                        string parteEncontrada = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Trim();
                        string umEncontrada = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
                        decimal? costo = null;
                        if (!skipCostos)
                        {
                            costo = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);
                        }

                        if (!mapResult.TryGetValue(parteSolicitada, out var cur))
                        {
                            cur = (UmEncontrada: string.IsNullOrWhiteSpace(umEncontrada) ? null : umEncontrada, Costo: costo, Existe: !string.IsNullOrWhiteSpace(parteEncontrada));
                        }
                        else
                        {
                            // Preferir primer costo no nulo
                            if (cur.Costo == null && costo != null)
                                cur.Costo = costo;
                            if (!cur.Existe && !string.IsNullOrWhiteSpace(parteEncontrada))
                                cur.Existe = true;
                            if (string.IsNullOrWhiteSpace(cur.UmEncontrada) && !string.IsNullOrWhiteSpace(umEncontrada))
                                cur.UmEncontrada = umEncontrada;
                        }

                        mapResult[parteSolicitada] = cur;
                    }
                }

            }

            // Construir resultados respetando las entradas originales (puede haber duplicados con distintas cantidades)
            foreach (var e in entradas)
            {
                var detalle = new DetalleParte
                {
                    Parte = e.Parte,
                    Cantidad = e.Cantidad,
                    UnidadUsuario = e.Unidad ?? string.Empty,
                    Existe = false,
                    MedComercial = string.Empty,
                    CostoUnitario = 0m,
                    TotalCosto = 0m,
                    UmCoincide = false,
                    CostoValido = false
                };

                if (mapResult.TryGetValue(e.Parte, out var info))
                {
                    detalle.Existe = info.Existe;
                    detalle.MedComercial = info.UmEncontrada ?? string.Empty;
                    if (info.Costo != null)
                    {
                        detalle.CostoUnitario = info.Costo.Value;
                        detalle.TotalCosto = decimal.Round(info.Costo.Value * e.Cantidad, 3);
                        detalle.CostoValido = Math.Abs(info.Costo.Value) > 0.0009m;
                    }

                    if (!string.IsNullOrWhiteSpace(detalle.MedComercial) && !string.IsNullOrWhiteSpace(detalle.UnidadUsuario))
                    {
                        detalle.UmCoincide = string.Equals(detalle.MedComercial.Trim(), detalle.UnidadUsuario.Trim(), StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        detalle.UmCoincide = false;
                    }
                }

                resultado.Items.Add(detalle);
            }

            return resultado;
        }
    }
}
