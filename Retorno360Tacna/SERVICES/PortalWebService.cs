using System;
using System.Threading.Tasks;
using Npgsql;
using Retorno360Tacna.SERVICES;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    public static class PortalWebService
    {
        /// <summary>
        /// Inserta el resultado de porcentaje de retorno en la base de datos del portal web.
        /// Crea la tabla si no existe (es idempotente).
        /// </summary>
        public static async Task<bool> GuardarResultadoRetornoAsync(ResultadoRetorno resultado)
        {
            try
            {
                var connString = ConfiguracionService.GetRailwayConnectionString();
                if (string.IsNullOrWhiteSpace(connString))
                    return false;

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                // Crear tabla si no existe
                var createTable = @"CREATE TABLE IF NOT EXISTS retorno_porcentaje (
                                                id SERIAL PRIMARY KEY,
                                                razon_social TEXT,
                                                base_datos TEXT,
                                                fecha_inicio DATE,
                                                fecha_fin DATE,
                                                valor_importado NUMERIC,
                                                valor_exportado NUMERIC,
                                                porcentaje_retorno NUMERIC,
                                                incluye_materia_prima BOOLEAN,
                                                fecha_calculo TIMESTAMP,
                                                pedimentos_importacion INTEGER,
                                                pedimentos_exportacion INTEGER,
                                                total_pedimentos INTEGER,
                                                created_at TIMESTAMP DEFAULT NOW()
                                            );";

                await using (var cmdCreate = new NpgsqlCommand(createTable, conn))
                {
                    await cmdCreate.ExecuteNonQueryAsync();
                }

                var insertSql = @"INSERT INTO retorno_porcentaje
(razon_social, base_datos, fecha_inicio, fecha_fin, valor_importado, valor_exportado, porcentaje_retorno, incluye_materia_prima, fecha_calculo, pedimentos_importacion, pedimentos_exportacion, total_pedimentos)
VALUES
(@razon, @base, @fini, @ffin, @vimp, @vexp, @pct, @mat, @fcalc, @pimp, @pexp, @total);";

                await using (var cmd = new NpgsqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@razon", resultado.RazonSocial ?? string.Empty);
                    cmd.Parameters.AddWithValue("@base", resultado.BaseDatos ?? string.Empty);
                    cmd.Parameters.AddWithValue("@fini", resultado.FechaInicio);
                    cmd.Parameters.AddWithValue("@ffin", resultado.FechaFin);
                    cmd.Parameters.AddWithValue("@vimp", resultado.ValorImportado);
                    cmd.Parameters.AddWithValue("@vexp", resultado.ValorExportado);
                    cmd.Parameters.AddWithValue("@pct", resultado.PorcentajeRetorno);
                    cmd.Parameters.AddWithValue("@mat", resultado.IncluyeMateriaPrima);
                    cmd.Parameters.AddWithValue("@fcalc", resultado.FechaCalculo);
                    cmd.Parameters.AddWithValue("@pimp", resultado.CantidadPedimentosImportacion);
                    cmd.Parameters.AddWithValue("@pexp", resultado.CantidadPedimentosExportacion);
                    cmd.Parameters.AddWithValue("@total", resultado.TotalPedimentosValidados);

                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PortalWebService] Error al guardar resultado: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Guarda el resultado del reporte IGI/IVA (resumen + detalle) en la base de datos del portal.
        /// Crea tablas igi_reporte e igi_reporte_detalle si no existen.
        /// </summary>
        public static async Task<bool> GuardarReporteIGIAsync(System.Collections.Generic.List<Retorno360Tacna.MODELS.ReporteIGIPagado> reporte, Retorno360Tacna.MODELS.ResumenIGI resumen, string razonSocial, string baseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var connString = ConfiguracionService.GetRailwayConnectionString();
                if (string.IsNullOrWhiteSpace(connString))
                    return false;

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                // Crear tablas si no existen
                var createReporte = @"CREATE TABLE IF NOT EXISTS igi_reporte (
                                            id SERIAL PRIMARY KEY,
                                            razon_social TEXT,
                                            base_datos TEXT,
                                            fecha_inicio DATE,
                                            fecha_fin DATE,
                                            total_igi_pagado NUMERIC,
                                            total_igi_calculado NUMERIC,
                                            total_iva_pagado NUMERIC,
                                            diferencia_total NUMERIC,
                                            total_pedimentos INTEGER,
                                            fecha_calculo TIMESTAMP,
                                            created_at TIMESTAMP DEFAULT NOW()
                                        );";

                var createDetalle = @"CREATE TABLE IF NOT EXISTS igi_reporte_detalle (
                                            id SERIAL PRIMARY KEY,
                                            reporte_id INTEGER REFERENCES igi_reporte(id) ON DELETE CASCADE,
                                            base_datos TEXT,
                                            id_pedimento INTEGER,
                                            pedimento TEXT,
                                            fecha_pago DATE,
                                            igi_pagado NUMERIC,
                                            igi_calculado NUMERIC,
                                            diferencia_igi NUMERIC,
                                            iva_pagado NUMERIC,
                                            forma_pago_igi TEXT,
                                            forma_pago_iva TEXT,
                                            estatus_glosa TEXT,
                                            estatus_origen TEXT
                                        );";

                await using (var cmdCreate = new NpgsqlCommand(createReporte, conn))
                {
                    await cmdCreate.ExecuteNonQueryAsync();
                }
                await using (var cmdCreate2 = new NpgsqlCommand(createDetalle, conn))
                {
                    await cmdCreate2.ExecuteNonQueryAsync();
                }

                // Insertar resumen y obtener id
                var insertReporte = @"INSERT INTO igi_reporte (razon_social, base_datos, fecha_inicio, fecha_fin, total_igi_pagado, total_igi_calculado, total_iva_pagado, diferencia_total, total_pedimentos, fecha_calculo)
VALUES (@razon, @base, @fini, @ffin, @tigi, @tcalc, @tiva, @diff, @total, @fcalc) RETURNING id;";

                int reporteId;
                await using (var tx = await conn.BeginTransactionAsync())
                {
                    await using (var cmd = new NpgsqlCommand(insertReporte, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@razon", razonSocial ?? string.Empty);
                        cmd.Parameters.AddWithValue("@base", baseDatos ?? string.Empty);
                        cmd.Parameters.AddWithValue("@fini", fechaInicio);
                        cmd.Parameters.AddWithValue("@ffin", fechaFin);
                        cmd.Parameters.AddWithValue("@tigi", resumen?.TotalIGI_Pagado ?? 0m);
                        cmd.Parameters.AddWithValue("@tcalc", resumen?.TotalIGI_Calculado ?? 0m);
                        cmd.Parameters.AddWithValue("@tiva", resumen?.TotalIVA_Pagado ?? 0m);
                        cmd.Parameters.AddWithValue("@diff", resumen != null ? (resumen.TotalIGI_Calculado - resumen.TotalIGI_Pagado) : 0m);
                        cmd.Parameters.AddWithValue("@total", resumen?.TotalPedimentos ?? 0);
                        cmd.Parameters.AddWithValue("@fcalc", DateTime.UtcNow);

                        var obj = await cmd.ExecuteScalarAsync();
                        reporteId = Convert.ToInt32(obj);
                    }

                    // Insertar detalle
                    var insertDetalle = @"INSERT INTO igi_reporte_detalle (reporte_id, base_datos, id_pedimento, pedimento, fecha_pago, igi_pagado, igi_calculado, diferencia_igi, iva_pagado, forma_pago_igi, forma_pago_iva, estatus_glosa, estatus_origen)
VALUES (@rid, @base, @idp, @ped, @fpay, @pag, @calc, @dif, @iva, @fpg, @fpiva, @estg, @esto);";

                    if (reporte != null)
                    {
                        foreach (var r in reporte)
                        {
                            await using var cmdDet = new NpgsqlCommand(insertDetalle, conn, tx);
                            cmdDet.Parameters.AddWithValue("@rid", reporteId);
                            cmdDet.Parameters.AddWithValue("@base", r.BaseDatos ?? string.Empty);
                            cmdDet.Parameters.AddWithValue("@idp", r.IdPedimento);
                            cmdDet.Parameters.AddWithValue("@ped", r.Pedimento ?? string.Empty);
                            cmdDet.Parameters.AddWithValue("@fpay", r.FechaPago.HasValue ? (object)r.FechaPago.Value : DBNull.Value);
                            cmdDet.Parameters.AddWithValue("@pag", r.IGI_Pagado);
                            cmdDet.Parameters.AddWithValue("@calc", r.IGI_Calculado);
                            cmdDet.Parameters.AddWithValue("@dif", r.DiferenciaIGI);
                            cmdDet.Parameters.AddWithValue("@iva", r.IVA_Pagado);
                            cmdDet.Parameters.AddWithValue("@fpg", r.FormaPago_IGI ?? string.Empty);
                            cmdDet.Parameters.AddWithValue("@fpiva", r.FormaPago_IVA ?? string.Empty);
                            cmdDet.Parameters.AddWithValue("@estg", r.EstatusGlosa ?? string.Empty);
                            cmdDet.Parameters.AddWithValue("@esto", r.EstatusOrigen ?? string.Empty);

                            await cmdDet.ExecuteNonQueryAsync();
                        }
                    }

                    await tx.CommitAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PortalWebService] Error al guardar reporte IGI: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Guarda registros del reporte ANEXOS en la tabla 'anexos' de PostgreSQL.
        /// Inserta filas solo si no existe combinación (mes, año, razon_social, planta).
        /// </summary>
        public static async Task<bool> GuardarAnexosAsync(System.Data.DataTable tabla, string razonSocial)
        {
            try
            {
                var connString = ConfiguracionService.GetRailwayConnectionString();
                if (string.IsNullOrWhiteSpace(connString))
                    return false;

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                var create = @"CREATE TABLE IF NOT EXISTS anexos (
                        id SERIAL PRIMARY KEY,
                        mes TEXT,
                        anio INTEGER,
                        planta TEXT,
                        total_np NUMERIC,
                        altas_np NUMERIC,
                        vigente_bom NUMERIC,
                        pct_base_limpia NUMERIC,
                        pct_retorno_cubierto NUMERIC,
                        razon_social TEXT,
                        fecha_calculo TIMESTAMP DEFAULT NOW()
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS ux_anexos_unico ON anexos(LOWER(razon_social), LOWER(planta), mes, anio);
                    ";

                await using (var cmdCreate = new NpgsqlCommand(create, conn))
                {
                    await cmdCreate.ExecuteNonQueryAsync();
                }

                // Use explicit existence check per row to avoid relying on ON CONFLICT semantics
                var selectExists = @"SELECT 1 FROM anexos WHERE razon_social = @razon AND planta = @planta AND mes = @mes AND anio = @anio LIMIT 1;";
                var insertSql = @"INSERT INTO anexos (mes, anio, planta, total_np, altas_np, vigente_bom, pct_base_limpia, pct_retorno_cubierto, razon_social)
                    VALUES (@mes, @anio, @planta, @total, @altas, @vigente, @pctbase, @pctr, @razon);";

                await using var tx = await conn.BeginTransactionAsync();
                try
                {
                    foreach (System.Data.DataRow row in tabla.Rows)
                    {
                        var mesVal = row.Table.Columns.Contains("MES") ? (object)row["MES"] : (object)DBNull.Value;
                        var anioVal = row.Table.Columns.Contains("AÑO") ? (object)row["AÑO"] : (object)DBNull.Value;
                        var plantaVal = row.Table.Columns.Contains("PLANTA") ? (object)row["PLANTA"] : (object)DBNull.Value;

                        // comprobar existencia
                        await using (var cmdCheck = new NpgsqlCommand(selectExists, conn, tx))
                        {
                            cmdCheck.Parameters.AddWithValue("@razon", razonSocial ?? string.Empty);
                            cmdCheck.Parameters.AddWithValue("@planta", plantaVal ?? string.Empty);
                            cmdCheck.Parameters.AddWithValue("@mes", mesVal ?? (object)DBNull.Value);
                            cmdCheck.Parameters.AddWithValue("@anio", anioVal ?? (object)DBNull.Value);

                            var exists = await cmdCheck.ExecuteScalarAsync();
                            if (exists != null)
                                continue; // ya existe, saltar
                        }

                        // insertar
                        await using var cmdIns = new NpgsqlCommand(insertSql, conn, tx);
                        cmdIns.Parameters.AddWithValue("@mes", row.Table.Columns.Contains("MES") ? (object)row["MES"] : (object)DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@anio", row.Table.Columns.Contains("AÑO") ? (object)row["AÑO"] : (object)DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@planta", row.Table.Columns.Contains("PLANTA") ? (object)row["PLANTA"] : (object)DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@total", row.Table.Columns.Contains("TOTAL_NP") ? (object)row["TOTAL_NP"] : 0m);
                        cmdIns.Parameters.AddWithValue("@altas", row.Table.Columns.Contains("ALTAS_NP") ? (object)row["ALTAS_NP"] : 0m);
                        cmdIns.Parameters.AddWithValue("@vigente", row.Table.Columns.Contains("VIGENTE_BOM") ? (object)row["VIGENTE_BOM"] : 0m);
                        cmdIns.Parameters.AddWithValue("@pctbase", row.Table.Columns.Contains("PCT_BASE_LIMPIA") ? (object)row["PCT_BASE_LIMPIA"] : 0m);
                        cmdIns.Parameters.AddWithValue("@pctr", row.Table.Columns.Contains("PCT_RETORNOS_CUBIERTOS") ? (object)row["PCT_RETORNOS_CUBIERTOS"] : 0m);
                        cmdIns.Parameters.AddWithValue("@razon", razonSocial ?? string.Empty);

                        await cmdIns.ExecuteNonQueryAsync();
                    }

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PortalWebService] Error GuardarAnexosAsync: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Guarda registros del reporte de cumplimiento en la tabla 'cumplimiento' de PostgreSQL.
        /// Inserta filas solo si no existe combinación (razon_social, planta, periodo).
        /// </summary>
        public static async Task<bool> GuardarCumplimientoAnexosAsync(System.Data.DataTable tabla)
        {
            try
            {
                var connString = ConfiguracionService.GetRailwayConnectionString();
                if (string.IsNullOrWhiteSpace(connString))
                    return false;

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                var create = @"CREATE TABLE IF NOT EXISTS cumplimiento (
                        id SERIAL PRIMARY KEY,
                        razon_social TEXT,
                        planta TEXT,
                        periodo TIMESTAMP,
                        operaciones INTEGER,
                        igi_pagado NUMERIC,
                        igi_calculado NUMERIC,
                        ahorro_igi NUMERIC,
                        pago_iva NUMERIC,
                        ahorro_iva NUMERIC,
                        fecha_calculo TIMESTAMP DEFAULT NOW()
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS ux_cumplimiento_unico ON cumplimiento(LOWER(razon_social), LOWER(planta), periodo);
                    ";

                await using (var cmdCreate = new NpgsqlCommand(create, conn))
                {
                    await cmdCreate.ExecuteNonQueryAsync();
                }

                var selectExists = @"SELECT 1
                    FROM cumplimiento
                    WHERE razon_social = @razon
                      AND planta = @planta
                      AND periodo = @periodo
                    LIMIT 1;";

                var insertSql = @"INSERT INTO cumplimiento
                    (razon_social, planta, periodo, operaciones, igi_pagado, igi_calculado, ahorro_igi, pago_iva, ahorro_iva)
                    VALUES (@razon, @planta, @periodo, @operaciones, @igiPagado, @igiCalculado, @ahorroIgi, @pagoIva, @ahorroIva);";

                await using var tx = await conn.BeginTransactionAsync();
                try
                {
                    foreach (System.Data.DataRow row in tabla.Rows)
                    {
                        var razonSocial = row.Table.Columns.Contains("RAZON_SOCIAL") ? row["RAZON_SOCIAL"]?.ToString() ?? string.Empty : string.Empty;
                        var planta = row.Table.Columns.Contains("PLANTA") ? row["PLANTA"]?.ToString() ?? string.Empty : string.Empty;
                        var periodo = row.Table.Columns.Contains("PERIODO") ? row["PERIODO"] : DBNull.Value;

                        await using (var cmdCheck = new NpgsqlCommand(selectExists, conn, tx))
                        {
                            cmdCheck.Parameters.AddWithValue("@razon", razonSocial);
                            cmdCheck.Parameters.AddWithValue("@planta", planta);
                            cmdCheck.Parameters.AddWithValue("@periodo", periodo);

                            var exists = await cmdCheck.ExecuteScalarAsync();
                            if (exists != null)
                                continue;
                        }

                        await using var cmdInsert = new NpgsqlCommand(insertSql, conn, tx);
                        cmdInsert.Parameters.AddWithValue("@razon", razonSocial);
                        cmdInsert.Parameters.AddWithValue("@planta", planta);
                        cmdInsert.Parameters.AddWithValue("@periodo", periodo);
                        cmdInsert.Parameters.AddWithValue("@operaciones", row.Table.Columns.Contains("OPERACIONES") ? row["OPERACIONES"] : 0);
                        cmdInsert.Parameters.AddWithValue("@igiPagado", row.Table.Columns.Contains("IGI_PAGADO") ? row["IGI_PAGADO"] : 0m);
                        cmdInsert.Parameters.AddWithValue("@igiCalculado", row.Table.Columns.Contains("IGI_CALCULADO") ? row["IGI_CALCULADO"] : 0m);
                        cmdInsert.Parameters.AddWithValue("@ahorroIgi", row.Table.Columns.Contains("AHORRO_IGI") ? row["AHORRO_IGI"] : 0m);
                        cmdInsert.Parameters.AddWithValue("@pagoIva", row.Table.Columns.Contains("PAGO_IVA") ? row["PAGO_IVA"] : 0m);
                        cmdInsert.Parameters.AddWithValue("@ahorroIva", row.Table.Columns.Contains("AHORRO_IVA") ? row["AHORRO_IVA"] : 0m);

                        await cmdInsert.ExecuteNonQueryAsync();
                    }

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PortalWebService] Error GuardarCumplimientoAnexosAsync: {ex}");
                return false;
            }
        }
    }
}
