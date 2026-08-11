using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.Data;

namespace Retorno360Tacna.SERVICES
{
    public class CatalogoPartesService : ReporteServiceBase
    {
        public CatalogoPartesService(ConexionInfo conexionInfo) : base(conexionInfo)
        {
        }

        private ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
        {
            return base.ObtenerConexionExterna(baseDatos);
        }

        private string ObtenerBaseGlosa(string baseDatosCliente)
        {
            try
            {
                int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatosCliente);
                var razonSocial = ObtenerRazonSocial(idRazon);

                if (!string.IsNullOrWhiteSpace(razonSocial.BaseDatosOrigen))
                    return razonSocial.BaseDatosOrigen.Trim();
            }
            catch
            {
            }

            return baseDatosCliente;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOM(string nombreBaseDatos, string tipoClave, DateTime fechaInicio, DateTime fechaFin)
        {
            var materiaPrima = new List<MateriaPrimaBOM>();
            var detallePedimentos = ObtenerDetallePedimentosPorParte(nombreBaseDatos, fechaInicio, fechaFin);

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
                    cp.Par_Consecutivo,
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
                                    Par_Consecutivo = reader["Par_Consecutivo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Par_Consecutivo"]),
                                    BaseDatosOrigenConsulta = nombreBaseDatos,
                                    Par_NoParte = reader["Par_NoParte"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    Clave = reader["Clave"]?.ToString() ?? string.Empty,
                                    Par_InsercionFecha = reader["Par_InsercionFecha"] == DBNull.Value ? null : (DateTime?)reader["Par_InsercionFecha"],
                                    EstatusComponente = reader["EstatusComponente"]?.ToString() ?? string.Empty,
                                    DetallePedimentosGlosa = "NO",
                                    DetallePedimentosInfo = string.Empty
                                };

                                if (detallePedimentos.TryGetValue(mp.Par_Consecutivo, out var detalles))
                                {
                                    mp.PedimentosRelacionados = detalles;
                                    mp.DetallePedimentosGlosa = detalles.Count > 0 ? "SI" : "NO";
                                    mp.DetallePedimentosInfo = string.Join(" ; ", detalles.Select(d => $"{d.Pedimento} | {d.TipoOperacion} | Clave: {d.ClavePedimento} | Cantidad: {d.CantidadPartidasMismaParte}"));
                                }

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
            var detallePedimentos = ObtenerDetallePedimentosPorParte(nombreBaseDatos, fechaInicio, fechaFin);

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
                    cp.Par_Consecutivo,
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
                    cp.Tim_Clave IN ('MP','EQ','MAQ','SUB','RT','AUX','PT')
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
                                    Par_Consecutivo = reader["Par_Consecutivo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Par_Consecutivo"]),
                                    BaseDatosOrigenConsulta = nombreBaseDatos,
                                    Par_NoParte = reader["Par_NoParte"]?.ToString() ?? string.Empty,
                                    Par_DescripcionEsp = reader["Par_DescripcionEsp"]?.ToString() ?? string.Empty,
                                    Clave = reader["Clave"]?.ToString() ?? string.Empty,
                                    Par_InsercionFecha = reader["Par_InsercionFecha"] == DBNull.Value ? null : (DateTime?)reader["Par_InsercionFecha"],
                                    EstatusComponente = reader["EstatusComponente"]?.ToString() ?? string.Empty,
                                    DetallePedimentosGlosa = "NO",
                                    DetallePedimentosInfo = string.Empty
                                };

                                if (detallePedimentos.TryGetValue(mp.Par_Consecutivo, out var detalles))
                                {
                                    mp.PedimentosRelacionados = detalles;
                                    mp.DetallePedimentosGlosa = detalles.Count > 0 ? "SI" : "NO";
                                    mp.DetallePedimentosInfo = string.Join(" ; ", detalles.Select(d => $"{d.Pedimento} | {d.TipoOperacion} | Clave: {d.ClavePedimento} | Cantidad: {d.CantidadPartidasMismaParte}"));
                                }

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

        private Dictionary<int, List<DetallePedimentoParte>> ObtenerDetallePedimentosPorParte(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new Dictionary<int, List<DetallePedimentoParte>>();

            string queryCliente = @"
                SELECT
                    dpd.Par_Consecutivo,
                    dp.Adu_AduanaSecc + '-' + dp.AgP_Patente + '-' + dp.Pim_Folio AS Pedimento,
                    dp.CLP_CLAVE AS ClavePedimento,
                    COUNT(*) AS CantidadPartidasMismaParte
                FROM Di_PedimentoDet dpd WITH (NOLOCK)
                INNER JOIN Di_Pedimento dp WITH (NOLOCK)
                    ON dp.Pim_Consecutivo = dpd.Pim_Consecutivo
                WHERE CONVERT(DATE, IIF(dp.CLP_CLAVE='R1', dp.Pim_FechaPagoR1, dp.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin
                GROUP BY
                    dpd.Par_Consecutivo,
                    dp.Adu_AduanaSecc,
                    dp.AgP_Patente,
                    dp.Pim_Folio,
                    dp.CLP_CLAVE
                ORDER BY Pedimento;";

            string queryGlosa = @"
                SELECT DISTINCT
                    Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento AS Pedimento,
                    Gl_TOper
                FROM TR_Glosa WITH (NOLOCK)
                WHERE CONVERT(DATE, Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin;";

            ConexionExternaInfo? infoConexion = null;
            string baseGlosa = ObtenerBaseGlosa(nombreBaseDatos);

            try
            {
                infoConexion = ObtenerConexionExterna(nombreBaseDatos);
                var detallesCliente = new List<(int ParConsecutivo, string Pedimento, string ClavePedimento, int Cantidad)>();
                var mapaGlosa = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

                using (var connCliente = ObtenerConexionParaBaseDatos(nombreBaseDatos).ObtenerConexion())
                {
                    connCliente.Open();
                    using var cmdCliente = new SqlCommand(queryCliente, connCliente);
                    cmdCliente.CommandTimeout = 300;
                    cmdCliente.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmdCliente.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                    using var readerCliente = cmdCliente.ExecuteReader();
                    while (readerCliente.Read())
                    {
                        detallesCliente.Add((
                            readerCliente["Par_Consecutivo"] == DBNull.Value ? 0 : Convert.ToInt32(readerCliente["Par_Consecutivo"]),
                            readerCliente["Pedimento"]?.ToString() ?? string.Empty,
                            readerCliente["ClavePedimento"]?.ToString() ?? string.Empty,
                            readerCliente["CantidadPartidasMismaParte"] == DBNull.Value ? 0 : Convert.ToInt32(readerCliente["CantidadPartidasMismaParte"])
                        ));
                    }
                }

                using (var connGlosa = ObtenerConexionParaBaseDatos(baseGlosa).ObtenerConexion())
                {
                    connGlosa.Open();
                    using var cmdGlosa = new SqlCommand(queryGlosa, connGlosa);
                    cmdGlosa.CommandTimeout = 300;
                    cmdGlosa.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmdGlosa.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                    using var readerGlosa = cmdGlosa.ExecuteReader();
                    while (readerGlosa.Read())
                    {
                        string pedimento = readerGlosa["Pedimento"]?.ToString() ?? string.Empty;
                        int tipoOper = readerGlosa["Gl_TOper"] == DBNull.Value ? 0 : Convert.ToInt32(readerGlosa["Gl_TOper"]);

                        if (!mapaGlosa.TryGetValue(pedimento, out var tipos))
                        {
                            tipos = new HashSet<int>();
                            mapaGlosa[pedimento] = tipos;
                        }

                        tipos.Add(tipoOper);
                    }
                }

                foreach (var detalleCliente in detallesCliente)
                {
                    if (!mapaGlosa.TryGetValue(detalleCliente.Pedimento, out var tiposOperacion))
                        continue;

                    foreach (var tipoOper in tiposOperacion.OrderBy(x => x))
                    {
                        var detalle = new DetallePedimentoParte
                        {
                            Pedimento = detalleCliente.Pedimento,
                            TipoOperacion = tipoOper == 1 ? "Importación" : tipoOper == 2 ? "Exportación" : $"Operación {tipoOper}",
                            ClavePedimento = detalleCliente.ClavePedimento,
                            CantidadPartidasMismaParte = detalleCliente.Cantidad
                        };

                        if (!resultado.TryGetValue(detalleCliente.ParConsecutivo, out var lista))
                        {
                            lista = new List<DetallePedimentoParte>();
                            resultado[detalleCliente.ParConsecutivo] = lista;
                        }

                        lista.Add(detalle);
                    }
                }
            }
            catch
            {
                return new Dictionary<int, List<DetallePedimentoParte>>();
            }

            return resultado;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOMPorRazonSocial(int idRazon, string tipoClave, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new List<MateriaPrimaBOM>();
            string nombreRazon = ObtenerRazonSocial(idRazon).NombreRazon;
            List<string> basesDatos = ObtenerBasesDatosRazon(idRazon);

            foreach (string baseDatos in basesDatos.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var datos = ObtenerMateriaPrimaBOM(baseDatos, tipoClave, fechaInicio, fechaFin);

                foreach (var item in datos)
                {
                    item.RazonSocialOrigen = nombreRazon;
                }

                resultado.AddRange(datos);
            }

            return resultado;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOMMultiplePorRazonSocial(int idRazon, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new List<MateriaPrimaBOM>();
            string nombreRazon = ObtenerRazonSocial(idRazon).NombreRazon;
            List<string> basesDatos = ObtenerBasesDatosRazon(idRazon);

            foreach (string baseDatos in basesDatos.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var datos = ObtenerMateriaPrimaBOMMultiple(baseDatos, fechaInicio, fechaFin);

                foreach (var item in datos)
                {
                    item.RazonSocialOrigen = nombreRazon;
                }

                resultado.AddRange(datos);
            }

            return resultado;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOMPorTodasLasRazones(string tipoClave, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new List<MateriaPrimaBOM>();
            var basesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var razon in ObtenerRazonesSociales().OrderBy(x => x.NombreRazon, StringComparer.OrdinalIgnoreCase))
            {
                List<string> basesDatos = ObtenerBasesDatosRazon(razon.IdRazon);

                foreach (string baseDatos in basesDatos.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!basesProcesadas.Add(baseDatos))
                        continue;

                    var datos = ObtenerMateriaPrimaBOM(baseDatos, tipoClave, fechaInicio, fechaFin);

                    foreach (var item in datos)
                    {
                        item.RazonSocialOrigen = razon.NombreRazon;
                    }

                    resultado.AddRange(datos);
                }
            }

            return resultado;
        }

        public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOMMultiplePorTodasLasRazones(DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = new List<MateriaPrimaBOM>();
            var basesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var razon in ObtenerRazonesSociales().OrderBy(x => x.NombreRazon, StringComparer.OrdinalIgnoreCase))
            {
                List<string> basesDatos = ObtenerBasesDatosRazon(razon.IdRazon);

                foreach (string baseDatos in basesDatos.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!basesProcesadas.Add(baseDatos))
                        continue;

                    var datos = ObtenerMateriaPrimaBOMMultiple(baseDatos, fechaInicio, fechaFin);

                    foreach (var item in datos)
                    {
                        item.RazonSocialOrigen = razon.NombreRazon;
                    }

                    resultado.AddRange(datos);
                }
            }

            return resultado;
        }
    }
}
