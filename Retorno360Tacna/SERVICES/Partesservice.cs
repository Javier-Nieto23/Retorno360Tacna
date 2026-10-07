using System.Data;
using Microsoft.Data.SqlClient;
using Retorno360Tacna.CNX;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Servicio encargado de consultar el catálogo de partes (CA_PARTE) de la
    /// base de datos de la empresa seleccionada, incluyendo costos, fracciones
    /// arancelarias e identificadores asociados.
    /// Hereda de ReporteServiceBase para enrutar automáticamente al servidor
    /// correcto (principal o externo) según NOM_TABLARAZON / Conexiones.
    /// </summary>
    public class PartesService : ReporteServiceBase
    {
        public PartesService(ConexionInfo conexion) : base(conexion)
        {
        }

        /// <summary>
        /// Obtiene el catálogo de partes de tipo "PT" (Producto Terminado) para
        /// la base de datos indicada, conectándose al servidor correcto según
        /// dónde esté alojada esa base de datos (principal o externo).
        /// </summary>
        /// <param name="baseDatos">Nombre real de la base de datos de la empresa seleccionada.</param>
        /// <param name="timeoutSegundos">Tiempo máximo de espera para la consulta.</param>
        public DataTable ObtenerPartes(string baseDatos, int timeoutSegundos = 120)
        {
            if (string.IsNullOrWhiteSpace(baseDatos))
                throw new ArgumentException("Debe especificar una base de datos válida.", nameof(baseDatos));

            const string query = @"
                SELECT
                    (CA_PARTE.PAR_NOPARTE) as ""NO. PARTE"",
                    (CA_PARTE.PAR_DESCRIPCIONESP) as ""DESCRIPCION ESP."",
                    (CA_PARTE.PAR_DESCRIPCIONING) as ""DESCRIPCION ING."",
                    (CA_PARTE.PAI_ORIGEN) as ""PAIS ORIGEN"",
                    (CA_PARTE.MED_CLAVE) as ""UM"",
                    (vDivCostos.PAC_COSTO) as ""COSTO UNITARIO"",
                    (CA_PARTE.TIM_CLAVE) as ""TIPO MATERIAL"",
                    (CA_PARTE.PAR_PESOUNIT) as ""PESO UNIT."",
                    (vDivCostos.PAGA) as ""COSTO PAGA"",
                    (vDivCostos.COSTOMO) as ""COSTO MO PAGA"",
                    (vFracciones.FRA_FRACCIONMEX) as ""FRACCION MEX."",
                    (vFracciones.FRA_FRACCIONUSA1) as ""FRACCION IMP. USA"",
                    (vFracciones.FRA_FRACCIONUSA2) as ""FRACCION EXP. USA"",
                    (CA_PARTEIDENTIFICA.IDE_CLAVE) as ""IDENTIFICADOR (IDENTIFICA.)"",
                    (CA_PARTE.TRA_CLAVE) as ""TRATADO"",
                    (PAR_TIPOTASAIMP20.CoB_TextoCampo) as ""TIPO TASA IMP."",
                    (vDivCostos.NOPAGA) as ""COSTO NO PAGA""
                FROM CA_PARTE
                LEFT OUTER JOIN CA_PARTEIDENTIFICA ON CA_PARTE.PAR_CONSECUTIVO = CA_PARTEIDENTIFICA.PAR_CONSECUTIVO
                LEFT OUTER JOIN vDivCostos ON CA_PARTE.PAR_CONSECUTIVO = vDivCostos.PAR_CONSECUTIVO
                LEFT OUTER JOIN vFracciones ON CA_PARTE.PAR_CONSECUTIVO = vFracciones.PAR_CONSECUTIVO
                LEFT OUTER JOIN (
                    SELECT CoB_TextoCampo, CoB_LetraCampo
                    FROM Cf_Combobox
                    WHERE CoB_TagForma = 3 AND CoB_Campo = 'PAR_TIPOTASAIMP'
                ) PAR_TIPOTASAIMP20 ON PAR_TIPOTASAIMP20.CoB_LetraCampo = CA_PARTE.PAR_TIPOTASAIMP
                WHERE CA_PARTE.TIM_CLAVE = 'PT';";

            // Enruta automáticamente: servidor principal o externo, según
            // NOM_TABLARAZON.IdConexion resuelto en ReporteServiceBase.
            Conexion conexionResuelta = ObtenerConexionParaBaseDatos(baseDatos);

            var tabla = new DataTable("Partes");

            using (SqlConnection cn = conexionResuelta.ObtenerConexion())
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.CommandTimeout = timeoutSegundos;
                cn.Open();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(tabla);
            }

            return tabla;
        }
    }
}