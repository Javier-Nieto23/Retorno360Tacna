using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Retorno360Tacna.SERVICES
{
    public sealed class InventarioPorEmpresaResult
    {
        public int IdRazon { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public DataTable Tabla { get; set; } = new DataTable();
    }

    public class InventarioQueryService
    {
        /// </summary>
        public async Task CrearTablaCatalogoIfNotExistsAsync(CancellationToken cancellation = default)
        {
            var sql = @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Catalogo_Partes')
BEGIN
    CREATE TABLE dbo.Catalogo_Partes (
        id INT IDENTITY(1,1) PRIMARY KEY,
        id_consecutivo INT NULL,
        id_empresa INT NULL,
        par_NoParte NVARCHAR(500) NULL,
        Par_DescripcionIng NVARCHAR(500) NULL,
        Par_DescripcionEsp NVARCHAR(500) NULL,
        Fra_FraccionMex INT NULL,
        Med_Clave NVARCHAR(200) NULL,
        Pac_FechaFin DATETIME NULL,
        Par_PesoUnit FLOAT NULL,
        Pac_UMPeso NVARCHAR(100) NULL,
        Tim_Clave NVARCHAR(100) NULL,
        Pai_Origen NVARCHAR(100) NULL,
        Tco_Clave NVARCHAR(100) NULL,
        Par_Activo NVARCHAR(50) NULL,
        JobId NVARCHAR(100) NULL,
        FechaCarga DATETIME NULL,
        IdRazon INT NULL
    );

    CREATE INDEX IX_CatalogoPartes_IdEmpresa ON dbo.Catalogo_Partes(id_empresa);
    CREATE INDEX IX_CatalogoPartes_TimClave ON dbo.Catalogo_Partes(Tim_Clave);
    CREATE INDEX IX_CatalogoPartes_ParNoParte ON dbo.Catalogo_Partes(par_NoParte);
END";

            var conexion = new Conexion();
            using var conn = conexion.ObtenerConexion();
            await conn.OpenAsync(cancellation).ConfigureAwait(false);
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
        }

        /// <summary>
        /// Vuelca resultados a la tabla Catalogo_Partes usando SqlBulkCopy.
        /// </summary>
        public async Task VolcarResultadosATablaCatalogoAsync(List<InventarioPorEmpresaResult> resultados, string jobId = null, CancellationToken ct = default)
        {
            if (resultados == null || resultados.Count == 0) return;

            var conexion = new Conexion();
            using var conn = conexion.ObtenerConexion();
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var tran = conn.BeginTransaction();
            try
            {
                // Limpiar filas previas del job si se especificó
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    using var cmdDel = new SqlCommand("DELETE FROM dbo.Catalogo_Partes WHERE JobId = @JobId", conn, tran);
                    cmdDel.Parameters.AddWithValue("@JobId", jobId);
                    await cmdDel.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                var dtBulk = new DataTable();
                dtBulk.Columns.Add("id_consecutivo", typeof(int));
                dtBulk.Columns.Add("id_empresa", typeof(int));
                dtBulk.Columns.Add("par_NoParte", typeof(string));
                dtBulk.Columns.Add("Par_DescripcionIng", typeof(string));
                dtBulk.Columns.Add("Par_DescripcionEsp", typeof(string));
                dtBulk.Columns.Add("Fra_FraccionMex", typeof(int));
                dtBulk.Columns.Add("Med_Clave", typeof(string));
                dtBulk.Columns.Add("Pac_FechaFin", typeof(DateTime));
                dtBulk.Columns.Add("Par_PesoUnit", typeof(double));
                dtBulk.Columns.Add("Pac_UMPeso", typeof(string));
                dtBulk.Columns.Add("Tim_Clave", typeof(string));
                dtBulk.Columns.Add("Pai_Origen", typeof(string));
                dtBulk.Columns.Add("Tco_Clave", typeof(string));
                dtBulk.Columns.Add("Par_Activo", typeof(string));
                dtBulk.Columns.Add("JobId", typeof(string));
                dtBulk.Columns.Add("FechaCarga", typeof(DateTime));
                dtBulk.Columns.Add("IdRazon", typeof(int));

                foreach (var res in resultados)
                {
                    int idEmpresa = res.IdEmpresa;
                    int idRazon = res.IdRazon;
                    if (res.Tabla == null || res.Tabla.Rows.Count == 0) continue;

                    foreach (DataRow row in res.Tabla.Rows)
                    {
                        var dr = dtBulk.NewRow();
                        dr["id_consecutivo"] = row.Table.Columns.Contains("Par_Consecutivo") && row["Par_Consecutivo"] != DBNull.Value ? Convert.ToInt32(row["Par_Consecutivo"]) : 0;
                        dr["id_empresa"] = idEmpresa;
                        dr["par_NoParte"] = SafeString(row, "Par_NoParte");
                        dr["Par_DescripcionIng"] = SafeString(row, "Par_DescripcionIng");
                        dr["Par_DescripcionEsp"] = SafeString(row, "Par_DescripcionEsp");
                        dr["Fra_FraccionMex"] = row.Table.Columns.Contains("Fra_FraccionMex") && row["Fra_FraccionMex"] != DBNull.Value ? Convert.ToInt32(row["Fra_FraccionMex"]) : (object)DBNull.Value;
                        dr["Med_Clave"] = SafeString(row, "Med_Clave");
                        dr["Pac_FechaFin"] = row.Table.Columns.Contains("Pac_FechaFin") && row["Pac_FechaFin"] != DBNull.Value ? (DateTime)row["Pac_FechaFin"] : (object)DBNull.Value;
                        dr["Par_PesoUnit"] = row.Table.Columns.Contains("Par_PesoUnit") && row["Par_PesoUnit"] != DBNull.Value ? Convert.ToDouble(row["Par_PesoUnit"]) : (object)DBNull.Value;
                        dr["Pac_UMPeso"] = SafeString(row, "Par_UMPeso") ?? SafeString(row, "Pac_UMPeso");
                        dr["Tim_Clave"] = SafeString(row, "Tim_Clave");
                        dr["Pai_Origen"] = SafeString(row, "Pai_Origen");
                        dr["Tco_Clave"] = SafeString(row, "Tco_Clave");
                        dr["Par_Activo"] = SafeString(row, "Par_Activo");
                        dr["JobId"] = jobId ?? string.Empty;
                        dr["FechaCarga"] = DateTime.UtcNow;
                        dr["IdRazon"] = idRazon;
                        dtBulk.Rows.Add(dr);
                    }
                }

                using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran);
                bulk.DestinationTableName = "dbo.Catalogo_Partes";
                bulk.BulkCopyTimeout = 600;
                bulk.BatchSize = 1000;

                // Mapear columnas (omitir id identity)
                bulk.ColumnMappings.Add("id_consecutivo", "id_consecutivo");
                bulk.ColumnMappings.Add("id_empresa", "id_empresa");
                bulk.ColumnMappings.Add("par_NoParte", "par_NoParte");
                bulk.ColumnMappings.Add("Par_DescripcionIng", "Par_DescripcionIng");
                bulk.ColumnMappings.Add("Par_DescripcionEsp", "Par_DescripcionEsp");
                bulk.ColumnMappings.Add("Fra_FraccionMex", "Fra_FraccionMex");
                bulk.ColumnMappings.Add("Med_Clave", "Med_Clave");
                bulk.ColumnMappings.Add("Pac_FechaFin", "Pac_FechaFin");
                bulk.ColumnMappings.Add("Par_PesoUnit", "Par_PesoUnit");
                bulk.ColumnMappings.Add("Pac_UMPeso", "Pac_UMPeso");
                bulk.ColumnMappings.Add("Tim_Clave", "Tim_Clave");
                bulk.ColumnMappings.Add("Pai_Origen", "Pai_Origen");
                bulk.ColumnMappings.Add("Tco_Clave", "Tco_Clave");
                bulk.ColumnMappings.Add("Par_Activo", "Par_Activo");
                bulk.ColumnMappings.Add("JobId", "JobId");
                bulk.ColumnMappings.Add("FechaCarga", "FechaCarga");
                bulk.ColumnMappings.Add("IdRazon", "IdRazon");
                bulk.ColumnMappings.Add("id_empresa", "id_empresa");
                bulk.ColumnMappings.Add("par_NoParte", "par_NoParte");
                bulk.ColumnMappings.Add("Par_DescripcionIng", "Par_DescripcionIng");
                bulk.ColumnMappings.Add("Par_DescripcionEsp", "Par_DescripcionEsp");
                bulk.ColumnMappings.Add("Fra_FraccionMex", "Fra_FraccionMex");
                bulk.ColumnMappings.Add("Med_Clave", "Med_Clave");
                bulk.ColumnMappings.Add("Pac_FechaFin", "Pac_FechaFin");
                bulk.ColumnMappings.Add("Par_PesoUnit", "Par_PesoUnit");
                bulk.ColumnMappings.Add("Pac_UMPeso", "Pac_UMPeso");
                bulk.ColumnMappings.Add("Tim_Clave", "Tim_Clave");
                bulk.ColumnMappings.Add("Pai_Origen", "Pai_Origen");
                bulk.ColumnMappings.Add("Tco_Clave", "Tco_Clave");
                bulk.ColumnMappings.Add("Par_Activo", "Par_Activo");
                bulk.ColumnMappings.Add("JobId", "JobId");
                bulk.ColumnMappings.Add("FechaCarga", "FechaCarga");
                bulk.ColumnMappings.Add("IdRazon", "IdRazon");

                await bulk.WriteToServerAsync(dtBulk, ct).ConfigureAwait(false);

                tran.Commit();
            }
            catch
            {
                try { tran.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// Lee los resultados volcados en dbo.Catalogo_Partes para un JobId y los agrupa por IdRazon+IdEmpresa
        /// devolviendo una lista compatible con InventarioPorEmpresaResult.
        /// </summary>
        public async Task<List<InventarioPorEmpresaResult>> ObtenerResultadosDesdeCatalogoPorJobIdAsync(string jobId, CancellationToken ct = default)
        {
            var lista = new List<InventarioPorEmpresaResult>();
            if (string.IsNullOrWhiteSpace(jobId)) return lista;

            var conexion = new Conexion();
            using var conn = conexion.ObtenerConexion();
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var sql = @"SELECT id_consecutivo, id_empresa, par_NoParte, Par_DescripcionIng, Par_DescripcionEsp, Fra_FraccionMex, Med_Clave, Pac_FechaFin, Par_PesoUnit, Pac_UMPeso, Tim_Clave, Pai_Origen, Tco_Clave, Par_Activo, IdRazon
                           FROM dbo.Catalogo_Partes WHERE JobId = @JobId ORDER BY IdRazon, id_empresa, id_consecutivo";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@JobId", jobId);
                using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

                var grupos = new Dictionary<string, InventarioPorEmpresaResult>(StringComparer.Ordinal);

                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    int idRazon = reader["IdRazon"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IdRazon"]);
                    int idEmpresa = reader["id_empresa"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_empresa"]);
                    var key = $"{idRazon}_{idEmpresa}";

                    if (!grupos.TryGetValue(key, out var item))
                    {
                        item = new InventarioPorEmpresaResult
                        {
                            IdRazon = idRazon,
                            IdEmpresa = idEmpresa,
                            NombreEmpresa = string.Empty,
                            Tabla = new DataTable()
                        };

                        item.Tabla.Columns.Add("Par_Consecutivo", typeof(int));
                        item.Tabla.Columns.Add("Par_NoParte", typeof(string));
                        item.Tabla.Columns.Add("Par_DescripcionIng", typeof(string));
                        item.Tabla.Columns.Add("Par_DescripcionEsp", typeof(string));
                        item.Tabla.Columns.Add("Fra_FraccionMex", typeof(int));
                        item.Tabla.Columns.Add("Med_Clave", typeof(string));
                        item.Tabla.Columns.Add("Pac_FechaFin", typeof(DateTime));
                        item.Tabla.Columns.Add("Par_PesoUnit", typeof(double));
                        item.Tabla.Columns.Add("Par_UMPeso", typeof(string));
                        item.Tabla.Columns.Add("Tim_Clave", typeof(string));
                        item.Tabla.Columns.Add("Pai_Origen", typeof(string));
                        item.Tabla.Columns.Add("Tco_Clave", typeof(string));
                        item.Tabla.Columns.Add("Par_Activo", typeof(string));

                        grupos[key] = item;
                    }

                    var row = item.Tabla.NewRow();
                    row["Par_Consecutivo"] = reader["id_consecutivo"] == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(reader["id_consecutivo"]);
                    row["Par_NoParte"] = reader["par_NoParte"] == DBNull.Value ? string.Empty : reader["par_NoParte"].ToString();
                    row["Par_DescripcionIng"] = reader["Par_DescripcionIng"] == DBNull.Value ? string.Empty : reader["Par_DescripcionIng"].ToString();
                    row["Par_DescripcionEsp"] = reader["Par_DescripcionEsp"] == DBNull.Value ? string.Empty : reader["Par_DescripcionEsp"].ToString();
                    row["Fra_FraccionMex"] = reader["Fra_FraccionMex"] == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(reader["Fra_FraccionMex"]);
                    row["Med_Clave"] = reader["Med_Clave"] == DBNull.Value ? string.Empty : reader["Med_Clave"].ToString();
                    row["Pac_FechaFin"] = reader["Pac_FechaFin"] == DBNull.Value ? (object)DBNull.Value : Convert.ToDateTime(reader["Pac_FechaFin"]);
                    row["Par_PesoUnit"] = reader["Par_PesoUnit"] == DBNull.Value ? (object)DBNull.Value : Convert.ToDouble(reader["Par_PesoUnit"]);
                    row["Par_UMPeso"] = reader["Pac_UMPeso"] == DBNull.Value ? string.Empty : reader["Pac_UMPeso"].ToString();
                    row["Tim_Clave"] = reader["Tim_Clave"] == DBNull.Value ? string.Empty : reader["Tim_Clave"].ToString();
                    row["Pai_Origen"] = reader["Pai_Origen"] == DBNull.Value ? string.Empty : reader["Pai_Origen"].ToString();
                    row["Tco_Clave"] = reader["Tco_Clave"] == DBNull.Value ? string.Empty : reader["Tco_Clave"].ToString();
                    row["Par_Activo"] = reader["Par_Activo"] == DBNull.Value ? string.Empty : reader["Par_Activo"].ToString();
                    item.Tabla.Rows.Add(row);
                }

                // Rellenar NombreEmpresa desde NOM_TABLARAZON
                foreach (var kv in grupos)
                {
                    var it = kv.Value;
                    try
                    {
                        using var cmd2 = new SqlCommand("SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon AND IdTabla = @IdTabla", conn);
                        cmd2.Parameters.AddWithValue("@IdRazon", it.IdRazon);
                        cmd2.Parameters.AddWithValue("@IdTabla", it.IdEmpresa);
                        var obj = await cmd2.ExecuteScalarAsync(ct).ConfigureAwait(false);
                        it.NombreEmpresa = obj == null || obj == DBNull.Value ? $"Empresa_{it.IdEmpresa}" : obj.ToString() ?? $"Empresa_{it.IdEmpresa}";
                    }
                    catch
                    {
                        it.NombreEmpresa = $"Empresa_{it.IdEmpresa}";
                    }
                    lista.Add(it);
                }
            }

            return lista;
        }

        /// <summary>
        /// Elimina filas antiguas por JobId.
        /// </summary>
        public async Task LimpiarPorJobIdAsync(string jobId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(jobId)) return;

            var conexion = new Conexion();
            using var conn = conexion.ObtenerConexion();
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new SqlCommand("DELETE FROM dbo.Catalogo_Partes WHERE JobId = @JobId", conn);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        private static string SafeString(DataRow row, string col)
        {
            if (row == null || row.Table == null) return string.Empty;
            if (!row.Table.Columns.Contains(col)) return string.Empty;
            return row[col] == DBNull.Value ? string.Empty : row[col].ToString()?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Ejecuta el query de inventario por cada empresa indicada y devuelve una tabla con resultados por empresa.
        /// </summary>
        public async Task<List<InventarioPorEmpresaResult>> ConsultarInventariosPorEmpresasAsync(List<(int IdRazon, int IdEmpresa, string NombreEmpresa)> empresas, CancellationToken cancellation = default)
        {
            var resultados = new List<InventarioPorEmpresaResult>();

            if (empresas == null || empresas.Count == 0) return resultados;

            var svcVerif = new VerificacionPartesService();

            // Limitar concurrencia para evitar abrir demasiadas conexiones simultáneas
            int maxConcurrency = Math.Max(2, Environment.ProcessorCount * 2);
            using (var sem = new System.Threading.SemaphoreSlim(maxConcurrency))
            {
                var tasks = new List<Task<InventarioPorEmpresaResult>>();

                // SQL usado en FrmCatalogoPartes para inventario por empresa
                string sql = @"select  cp.Par_Consecutivo, cp.Par_NoParte, cp.Par_DescripcionIng, cp.Par_DescripcionEsp, fa.Fra_FraccionMex, cp.Med_Clave, cpc.Pac_Costo, cpc.Pac_FechaFin, cp.Par_PesoUnit, cp.Par_UMPeso, cp.Tim_Clave, cp.Pai_Origen, cpc.Tco_Clave, cp.Par_Activo
                                                from Ca_Parte cp
                                                inner join Ca_ParteCosto cpc on cp.Par_Consecutivo = cpc.Par_Consecutivo
                                                inner join vFracciones fa on fa.Par_Consecutivo = cp.Par_Consecutivo";

                foreach (var e in empresas)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }

                    // Capturar variable local
                    var empresa = e;

                    var task = Task.Run(async () =>
                    {
                        await sem.WaitAsync(cancellation).ConfigureAwait(false);
                        try
                        {
                            if (cancellation.IsCancellationRequested)
                                return new InventarioPorEmpresaResult
                                {
                                    IdEmpresa = empresa.IdEmpresa,
                                    IdRazon = empresa.IdRazon,
                                    NombreEmpresa = empresa.NombreEmpresa,
                                    Tabla = new DataTable()
                                };

                            try
                            {
                                string connStr = await svcVerif.ObtenerCadenaConexionPublicAsync(empresa.IdRazon, empresa.IdEmpresa).ConfigureAwait(false);
                                if (string.IsNullOrWhiteSpace(connStr))
                                {
                                    return new InventarioPorEmpresaResult
                                    {
                                        IdEmpresa = empresa.IdEmpresa,
                                        IdRazon = empresa.IdRazon,
                                        NombreEmpresa = empresa.NombreEmpresa,
                                        Tabla = new DataTable()
                                    };
                                }

                                var dt = new DataTable();

                                // Ejecutar la carga en hilo de background porque SqlDataAdapter.Fill es síncrono
                                await Task.Run(() =>
                                {
                                    using (var conn = new SqlConnection(connStr))
                                    using (var cmd = new SqlCommand(sql, conn))
                                    using (var da = new SqlDataAdapter(cmd))
                                    {
                                        da.Fill(dt);
                                    }
                                }, cancellation).ConfigureAwait(false);

                                return new InventarioPorEmpresaResult
                                {
                                    IdEmpresa = empresa.IdEmpresa,
                                    IdRazon = empresa.IdRazon,
                                    NombreEmpresa = empresa.NombreEmpresa,
                                    Tabla = dt
                                };
                            }
                            catch
                            {
                                return new InventarioPorEmpresaResult
                                {
                                    IdEmpresa = empresa.IdEmpresa,
                                    IdRazon = empresa.IdRazon,
                                    NombreEmpresa = empresa.NombreEmpresa,
                                    Tabla = new DataTable()
                                };
                            }
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, cancellation);

                    tasks.Add(task);
                }

                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                resultados.AddRange(results);
            }

            return resultados;
        }
    }
}
