using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;
using System.Data;

namespace Retorno360Tacna.SERVICES
{
    public class RetornoService
    {
        private readonly ConexionInfo conexionInfo;

        public RetornoService(ConexionInfo conexion)
        {
            conexionInfo = conexion;
        }

        public ResultadoRetorno CalcularRetorno(
            int idRazon,
            string baseDatosSeleccionada,
            DateTime fechaInicio,
            DateTime fechaFin,
            bool incluirMateriaPrima)
        {
            try
            {
                // Obtener información de la razón social
                RazonSocial razonInfo = ObtenerRazonSocial(idRazon);
                
                if (razonInfo == null || string.IsNullOrEmpty(razonInfo.BaseDatosOrigen))
                {
                    throw new Exception($"No se encontró la razón social o su base de datos origen.");
                }

                // Validar pedimentos entre bases de datos
                var pedimentosValidos = ValidarPedimentosCruzados(
                    baseDatosSeleccionada, 
                    razonInfo.BaseDatosOrigen, 
                    fechaInicio, 
                    fechaFin
                );

                if (!pedimentosValidos.Any())
                {
                    throw new Exception($"No se encontraron pedimentos coincidentes entre {baseDatosSeleccionada} y {razonInfo.BaseDatosOrigen}");
                }

                // Calcular valores usando los pedimentos validados
                decimal importado = ObtenerImportacionesValidadas(
                    razonInfo.BaseDatosOrigen, 
                    pedimentosValidos.Where(p => p.Tipo == "IMPORTACION").ToList(),
                    fechaInicio, 
                    fechaFin
                );

                decimal exportado = ObtenerExportacionesValidadas(
                    razonInfo.BaseDatosOrigen, 
                    pedimentosValidos.Where(p => p.Tipo == "EXPORTACION").ToList(),
                    fechaInicio, 
                    fechaFin, 
                    incluirMateriaPrima
                );

                decimal porcentaje = CalculadoraRetorno.CalcularPorcentajeRetorno(importado, exportado);

                // Contar pedimentos validados
                int cantImportacion = pedimentosValidos.Count(p => p.Tipo == "IMPORTACION");
                int cantExportacion = pedimentosValidos.Count(p => p.Tipo == "EXPORTACION");

                var resultado = new ResultadoRetorno
                {
                    RazonSocial = razonInfo.NombreRazon,
                    BaseDatos = baseDatosSeleccionada,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    ValorImportado = importado,
                    ValorExportado = exportado,
                    PorcentajeRetorno = porcentaje,
                    FechaCalculo = DateTime.Now,
                    IncluyeMateriaPrima = incluirMateriaPrima,
                    CantidadPedimentosImportacion = cantImportacion,
                    CantidadPedimentosExportacion = cantExportacion,
                    TotalPedimentosValidados = cantImportacion + cantExportacion
                };

                GuardarHistorico(resultado);

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular retorno: {ex.Message}", ex);
            }
        }

        public ResultadoRetorno CalcularRetornoPorRazonSocial(
            int idRazon,
            DateTime fechaInicio,
            DateTime fechaFin,
            bool incluirMateriaPrima)
        {
            try
            {
                // Obtener información de la razón social
                RazonSocial razonInfo = ObtenerRazonSocial(idRazon);

                if (razonInfo == null || string.IsNullOrEmpty(razonInfo.NombreRazon))
                {
                    throw new Exception($"No se encontró la razón social.");
                }

                // Obtener TODAS las bases de datos asociadas a esta razón en GLOSAXRAZON
                List<string> basesDatos = ObtenerBasesDatosDeGlosaxRazon(razonInfo.NombreRazon);

                if (!basesDatos.Any())
                {
                    throw new Exception($"No se encontraron bases de datos en GLOSAXRAZON para la razón social: {razonInfo.NombreRazon}");
                }

                // Acumuladores para sumar resultados de todas las BDs
                decimal totalImportado = 0;
                decimal totalExportado = 0;
                int totalPedimentosImp = 0;
                int totalPedimentosExp = 0;

                // Iterar sobre cada base de datos (como el cursor del SP)
                foreach (string baseDatos in basesDatos)
                {
                    try
                    {
                        var resultadoParcial = CalcularPorBaseDatos(
                            baseDatos, 
                            fechaInicio, 
                            fechaFin, 
                            incluirMateriaPrima
                        );

                        totalImportado += resultadoParcial.importado;
                        totalExportado += resultadoParcial.exportado;
                        totalPedimentosImp += resultadoParcial.cantImportaciones;
                        totalPedimentosExp += resultadoParcial.cantExportaciones;
                    }
                    catch (Exception ex)
                    {
                        // Log error pero continúa con las demás BDs
                        System.Diagnostics.Debug.WriteLine($"Error en BD {baseDatos}: {ex.Message}");
                    }
                }

                decimal porcentaje = CalculadoraRetorno.CalcularPorcentajeRetorno(totalImportado, totalExportado);

                var resultado = new ResultadoRetorno
                {
                    RazonSocial = razonInfo.NombreRazon,
                    BaseDatos = string.Join(", ", basesDatos), // Múltiples BDs
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    ValorImportado = totalImportado,
                    ValorExportado = totalExportado,
                    PorcentajeRetorno = porcentaje,
                    FechaCalculo = DateTime.Now,
                    IncluyeMateriaPrima = incluirMateriaPrima,
                    CantidadPedimentosImportacion = totalPedimentosImp,
                    CantidadPedimentosExportacion = totalPedimentosExp,
                    TotalPedimentosValidados = totalPedimentosImp + totalPedimentosExp
                };

                GuardarHistorico(resultado);

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular retorno por razón social: {ex.Message}", ex);
            }
        }

        private List<string> ObtenerBasesDatosDeGlosaxRazon(string razonSocial)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT DB FROM GLOSAXRAZON WHERE RAZON = @Razon";
                List<string> basesDatos = new List<string>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Razon", razonSocial);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            basesDatos.Add(reader.GetString(0));
                        }
                    }
                }

                return basesDatos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos de GLOSAXRAZON: {ex.Message}", ex);
            }
        }

        private (decimal importado, decimal exportado, int cantImportaciones, int cantExportaciones) 
            CalcularPorBaseDatos(string baseDatos, DateTime fechaInicio, DateTime fechaFin, bool incluirMateriaPrima)
        {
            try
            {
                // Query de IMPORTACIONES (igual al SP)
                string sqlImportaciones = $@"
                SELECT 
                    ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0) AS ValorComercial,
                    COUNT(DISTINCT Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento) AS CantidadPedimentos
                FROM [{baseDatos}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = 
                       Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                  AND Gl_CveDocto IN ('IN', 'V1')
                  AND Gl_TOper = 1
                  AND ide.Identificador IS NULL
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin";

                // Query de EXPORTACIONES (igual al SP con filtro materia prima)
                string filtroMateriaPrima = incluirMateriaPrima ? "" : "AND idePT.IDENTIFICADOR IS NOT NULL";

                string sqlExportaciones = $@"
                SELECT 
                    ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0) AS ValorComercial,
                    COUNT(DISTINCT Gl_Aduana + '-' + Gl_Patente + '-' + Gl_Pedimento) AS CantidadPedimentos
                FROM [{baseDatos}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ideAF ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ideAF.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('DE')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador, Partida 
                    FROM [{baseDatos}].dbo.TR_GlosaPartidaIdentifica 
                    WHERE Identificador IN ('PT')
                ) idePT ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = idePT.Pedimento 
                    AND idePT.Partida = PAR.Gl_Sec
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = 
                       Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                  AND Gl_CveDocto IN ('RT', 'V1')
                  AND Gl_TOper = 2
                  AND ide.Identificador IS NULL
                  AND ideAF.Identificador IS NULL
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @FechaInicio AND @FechaFin
                  {filtroMateriaPrima}";

                decimal valorImportado = 0;
                int cantImportaciones = 0;
                decimal valorExportado = 0;
                int cantExportaciones = 0;

                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    baseDatos
                );

                using (SqlConnection cn = conexion.ObtenerConexion())
                {
                    cn.Open();
                    cn.ChangeDatabase(baseDatos);

                    // Ejecutar query de importaciones
                    using (SqlCommand cmd = new SqlCommand(sqlImportaciones, cn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                valorImportado = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                cantImportaciones = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            }
                        }
                    }

                    // Ejecutar query de exportaciones
                    using (SqlCommand cmd = new SqlCommand(sqlExportaciones, cn))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                valorExportado = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                cantExportaciones = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            }
                        }
                    }
                }

                return (valorImportado, valorExportado, cantImportaciones, cantExportaciones);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular datos de BD {baseDatos}: {ex.Message}", ex);
            }
        }

        private decimal ObtenerImportacionesDirectas(string baseDatos, DateTime ini, DateTime fin)
        {
            try
            {
                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatos}].dbo.TR_Glosa G
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 R1
                    ON G.Gl_Aduana + '-' + G.Gl_Patente + '-' + G.Gl_Pedimento =
                       R1.Aduana_Ant + '-' + R1.Patente_Ant + '-' + R1.Pedimento_Ant
                WHERE R1.Pedimento_Ant IS NULL
                  AND G.Gl_CveDocto IN ('IN', 'V1')
                  AND G.Gl_TOper = 1
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin";

                return EjecutarDecimalDirecto(baseDatos, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener importaciones directas: {ex.Message}", ex);
            }
        }

        private decimal ObtenerExportacionesDirectas(string baseDatos, DateTime ini, DateTime fin, bool incluirMateriaPrima)
        {
            try
            {
                string filtroMateriaPrima = "";
                if (!incluirMateriaPrima)
                {
                    filtroMateriaPrima = "AND IDEPT.Identificador IS NOT NULL";
                }

                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatos}].dbo.TR_Glosa G
                LEFT JOIN [{baseDatos}].dbo.TR_GlosaR1 R1
                    ON G.Gl_Aduana + '-' + G.Gl_Patente + '-' + G.Gl_Pedimento =
                       R1.Aduana_Ant + '-' + R1.Patente_Ant + '-' + R1.Pedimento_Ant
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador
                    FROM [{baseDatos}].dbo.TR_GlosaIdentifica
                    WHERE Identificador = 'PT'
                ) IDEPT ON G.Gl_Aduana + '-' + G.Gl_Patente + '-' + G.Gl_Pedimento = IDEPT.Pedimento
                WHERE R1.Pedimento_Ant IS NULL
                  AND G.Gl_CveDocto IN ('RT', 'V1')
                  AND G.Gl_TOper = 2
                  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin
                  {filtroMateriaPrima}";

                return EjecutarDecimalDirecto(baseDatos, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener exportaciones directas: {ex.Message}", ex);
            }
        }

        public List<RazonSocial> ObtenerRazonesSociales()
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT IdRazon, NOMBRE_RAZON, DB FROM RAZONXTABLA ORDER BY NOMBRE_RAZON";
                List<RazonSocial> razones = new List<RazonSocial>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            razones.Add(new RazonSocial
                            {
                                IdRazon = reader.GetInt32(0),
                                NombreRazon = reader.GetString(1),
                                BaseDatosOrigen = reader.GetString(2)
                            });
                        }
                    }
                }

                return razones;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razones sociales: {ex.Message}", ex);
            }
        }

        public List<BaseDatosRazon> ObtenerBasesDatosRazon(int idRazon)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT IdTabla, NOMBRE_TABLA, IdRazon FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
                List<BaseDatosRazon> bases = new List<BaseDatosRazon>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bases.Add(new BaseDatosRazon
                            {
                                IdTabla = reader.GetInt32(0),
                                NombreTabla = reader.GetString(1),
                                IdRazon = reader.GetInt32(2)
                            });
                        }
                    }
                }

                return bases;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bases de datos de la razón: {ex.Message}", ex);
            }
        }

        private RazonSocial ObtenerRazonSocial(int idRazon)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = "SELECT IdRazon, NOMBRE_RAZON, DB FROM RAZONXTABLA WHERE IdRazon = @IdRazon";

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdRazon", idRazon);
                    cn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new RazonSocial
                            {
                                IdRazon = reader.GetInt32(0),
                                NombreRazon = reader.GetString(1),
                                BaseDatosOrigen = reader.GetString(2)
                            };
                        }
                    }
                }

                throw new Exception("Razón social no encontrada");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razón social: {ex.Message}", ex);
            }
        }

        private List<PedimentoComparacion> ValidarPedimentosCruzados(
            string baseDatosSeleccionada,
            string baseDatosOrigen,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    baseDatosSeleccionada
                );

                string sql = $@"
                SELECT
                    X.Tipo,
                    X.Aduana,
                    X.Patente,
                    X.Pedimento,
                    X.FechaPago
                FROM
                (
                    SELECT DISTINCT
                        'IMPORTACION' AS Tipo,
                        DI.Adu_AduanaSecc AS Aduana,
                        DI.AgP_Patente AS Patente,
                        DI.Pim_Folio AS Pedimento,
                        DI.Pim_FechaPago AS FechaPago
                    FROM Di_Pedimento DI
                    WHERE DI.Pim_FechaPago >= @FechaInicio
                      AND DI.Pim_FechaPago <= @FechaFin
                      AND EXISTS (
                          SELECT 1 
                          FROM [{baseDatosOrigen}].dbo.TR_Glosa G
                          WHERE G.Gl_Aduana = DI.Adu_AduanaSecc
                            AND G.Gl_Patente = DI.AgP_Patente
                            AND G.Gl_Pedimento = DI.Pim_Folio
                            AND G.Gl_TOper = 1
                      )

                    UNION ALL

                    SELECT DISTINCT
                        'EXPORTACION' AS Tipo,
                        DE.Adu_AduanaSecc AS Aduana,
                        DE.AgP_Patente AS Patente,
                        DE.Pex_Folio AS Pedimento,
                        DE.Pex_FechaPago AS FechaPago
                    FROM De_Pedimento DE
                    WHERE DE.Pex_FechaPago >= @FechaInicio
                      AND DE.Pex_FechaPago <= @FechaFin
                      AND EXISTS (
                          SELECT 1 
                          FROM [{baseDatosOrigen}].dbo.TR_Glosa G
                          WHERE G.Gl_Aduana = DE.Adu_AduanaSecc
                            AND G.Gl_Patente = DE.AgP_Patente
                            AND G.Gl_Pedimento = DE.Pex_Folio
                            AND G.Gl_TOper = 2
                      )
                ) X
                ORDER BY X.Tipo, X.Aduana, X.Patente, X.Pedimento";

                List<PedimentoComparacion> pedimentos = new List<PedimentoComparacion>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedimentos.Add(new PedimentoComparacion
                            {
                                Tipo = reader.GetString(0),
                                Aduana = reader.GetString(1),
                                Patente = reader.GetString(2),
                                Pedimento = reader.GetString(3),
                                FechaPago = reader.GetDateTime(4),
                                ExisteEnGlosa = true
                            });
                        }
                    }
                }

                return pedimentos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar pedimentos cruzados: {ex.Message}", ex);
            }
        }

        private decimal ObtenerImportacionesValidadas(
            string baseDatosOrigen,
            List<PedimentoComparacion> pedimentosValidos,
            DateTime ini,
            DateTime fin)
        {
            try
            {
                if (!pedimentosValidos.Any())
                    return 0;

                string pedimentosIn = string.Join(",", 
                    pedimentosValidos.Select(p => $"'{p.Aduana}-{p.Patente}-{p.Pedimento}'"));

                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatosOrigen}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN [{baseDatosOrigen}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                AND Gl_CveDocto IN ('IN', 'V1')
                AND Gl_TOper = 1
                AND ide.Identificador IS NULL
                AND Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento IN ({pedimentosIn})
                AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin";

                return EjecutarDecimalDirecto(baseDatosOrigen, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener importaciones validadas: {ex.Message}", ex);
            }
        }

        private decimal ObtenerExportacionesValidadas(
            string baseDatosOrigen,
            List<PedimentoComparacion> pedimentosValidos,
            DateTime ini,
            DateTime fin,
            bool incluirMateriaPrima)
        {
            try
            {
                if (!pedimentosValidos.Any())
                    return 0;

                string filtroMateriaPrima = incluirMateriaPrima ? "" : "AND idePT.IDENTIFICADOR IS NOT NULL";
                string pedimentosIn = string.Join(",", 
                    pedimentosValidos.Select(p => $"'{p.Aduana}-{p.Patente}-{p.Pedimento}'"));

                string sql = $@"
                SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
                FROM [{baseDatosOrigen}].dbo.TR_Glosa par
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('AF')
                ) ideAF ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ideAF.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaIdentifica 
                    WHERE Identificador IN ('DE')
                ) ide ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = ide.Pedimento
                LEFT JOIN (
                    SELECT DISTINCT Pedimento, Identificador, Partida 
                    FROM [{baseDatosOrigen}].dbo.TR_GlosaPartidaIdentifica 
                    WHERE Identificador IN ('PT')
                ) idePT ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = idePT.Pedimento 
                    AND idePT.Partida = PAR.Gl_Sec
                LEFT JOIN [{baseDatosOrigen}].dbo.TR_GlosaR1 r1 
                    ON Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento = Aduana_Ant + '-' + Patente_Ant + '-' + r1.Pedimento_Ant
                WHERE (Pedimento_Ant IS NULL)
                AND Gl_CveDocto IN ('RT', 'V1')
                AND Gl_TOper = 2
                AND ide.Identificador IS NULL
                AND ideAF.Identificador IS NULL
                AND Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento IN ({pedimentosIn})
                AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin
                {filtroMateriaPrima}";

                return EjecutarDecimalDirecto(baseDatosOrigen, sql, ini, fin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener exportaciones validadas: {ex.Message}", ex);
            }
        }

        private decimal EjecutarDecimalDirecto(string baseDatos, string sql, DateTime ini, DateTime fin)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    baseDatos
                );

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ini", ini.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));
                    cmd.CommandTimeout = 120;

                    cn.Open();
                    var resultado = cmd.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        return 0;

                    return Convert.ToDecimal(resultado);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar consulta: {ex.Message}", ex);
            }
        }

        private void GuardarHistorico(ResultadoRetorno resultado)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'HistoricoRetorno' AND xtype = 'U')
                    BEGIN
                        CREATE TABLE HistoricoRetorno (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            RazonSocial VARCHAR(200),
                            BaseDatos VARCHAR(100),
                            FechaInicio DATE,
                            FechaFin DATE,
                            ValorImportado DECIMAL(28,2),
                            ValorExportado DECIMAL(28,2),
                            PorcentajeRetorno DECIMAL(28,2),
                            IncluyeMateriaPrima BIT,
                            FechaCalculo DATETIME
                        )
                    END

                    INSERT INTO HistoricoRetorno 
                    (RazonSocial, BaseDatos, FechaInicio, FechaFin, ValorImportado, ValorExportado, PorcentajeRetorno, IncluyeMateriaPrima, FechaCalculo)
                    VALUES 
                    (@RazonSocial, @BaseDatos, @FechaInicio, @FechaFin, @ValorImportado, @ValorExportado, @PorcentajeRetorno, @IncluyeMateriaPrima, @FechaCalculo)";

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@RazonSocial", resultado.RazonSocial);
                    cmd.Parameters.AddWithValue("@BaseDatos", resultado.BaseDatos);
                    cmd.Parameters.AddWithValue("@FechaInicio", resultado.FechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", resultado.FechaFin);
                    cmd.Parameters.AddWithValue("@ValorImportado", resultado.ValorImportado);
                    cmd.Parameters.AddWithValue("@ValorExportado", resultado.ValorExportado);
                    cmd.Parameters.AddWithValue("@PorcentajeRetorno", resultado.PorcentajeRetorno);
                    cmd.Parameters.AddWithValue("@IncluyeMateriaPrima", resultado.IncluyeMateriaPrima);
                    cmd.Parameters.AddWithValue("@FechaCalculo", resultado.FechaCalculo);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar histórico: {ex.Message}", ex);
            }
        }

        public List<ResultadoRetorno> ObtenerHistorico(string? razonSocial = null, DateTime? desde = null, DateTime? hasta = null)
        {
            try
            {
                Conexion conexion = new Conexion(
                    conexionInfo.Servidor ?? string.Empty,
                    conexionInfo.UsuarioSQL ?? string.Empty,
                    conexionInfo.PasswordSQL ?? string.Empty,
                    "RetornoMaster"
                );

                string sql = @"
                    SELECT RazonSocial, BaseDatos, FechaInicio, FechaFin, 
                           ValorImportado, ValorExportado, PorcentajeRetorno, 
                           IncluyeMateriaPrima, FechaCalculo
                    FROM HistoricoRetorno
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(razonSocial))
                    sql += " AND RazonSocial = @RazonSocial";

                if (desde.HasValue)
                    sql += " AND FechaCalculo >= @Desde";

                if (hasta.HasValue)
                    sql += " AND FechaCalculo <= @Hasta";

                sql += " ORDER BY FechaCalculo DESC";

                List<ResultadoRetorno> resultados = new List<ResultadoRetorno>();

                using (SqlConnection cn = conexion.ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(razonSocial))
                        cmd.Parameters.AddWithValue("@RazonSocial", razonSocial);

                    if (desde.HasValue)
                        cmd.Parameters.AddWithValue("@Desde", desde.Value);

                    if (hasta.HasValue)
                        cmd.Parameters.AddWithValue("@Hasta", hasta.Value);

                    cn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(new ResultadoRetorno
                            {
                                RazonSocial = reader.GetString(0),
                                BaseDatos = reader.GetString(1),
                                FechaInicio = reader.GetDateTime(2),
                                FechaFin = reader.GetDateTime(3),
                                ValorImportado = reader.GetDecimal(4),
                                ValorExportado = reader.GetDecimal(5),
                                PorcentajeRetorno = reader.GetDecimal(6),
                                IncluyeMateriaPrima = reader.GetBoolean(7),
                                FechaCalculo = reader.GetDateTime(8)
                            });
                        }
                    }
                }

                return resultados;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener histórico: {ex.Message}", ex);
            }
        }
    }

    public class ResultadoRetorno
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string BaseDatos { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal ValorImportado { get; set; }
        public decimal ValorExportado { get; set; }
        public decimal PorcentajeRetorno { get; set; }
        public bool IncluyeMateriaPrima { get; set; }
        public DateTime FechaCalculo { get; set; }
        public int CantidadPedimentosImportacion { get; set; }
        public int CantidadPedimentosExportacion { get; set; }
        public int TotalPedimentosValidados { get; set; }
    }
}
