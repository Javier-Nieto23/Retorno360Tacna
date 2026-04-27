using System.Collections.Generic;
using System.Linq;

namespace Retorno360Tacna.MODELS
{
    public class ValidadorBasesDatos
    {
        private static readonly Dictionary<string, List<string>> BasesPermitidas = new()
        {
            {
                "TJ-SQLSRV03", new List<string>
                {
                    "SEERT_Able", "SEERT_Acme", "SEERT_Ameramark", "SEERT_Bi",
                    "SEERT_Craftech", "SEERT_Eddy", "SEERT_Falltech", "SEERT_Foampro",
                    "SEERT_Goodyear", "SEERT_Hogue", "SEERT_Inventusnexergy", "SEERT_Kasa",
                    "SEERT_Magleby", "SEERT_Masterpiece", "SEERT_Mww", "SEERT_Nativa",
                    "SEERT_Pertronix", "SEERT_Printful", "SEERT_Prospot", "SEERT_Seatcovers",
                    "SEERT_Standard", "SEERT_Stanton", "SEERT_Tacna", "SEERT_Tpc",
                    "SEERT_Unimacts", "SEERT_Universal", "SEERT_Vinventions", "SEERT_Aldila",
                    "SEERT_Atlantic", "SEERT_Avocado", "SEERT_Cellpoint", "SEERT_Cevians",
                    "SEERT_Chemdiv", "SEERT_Denso", "SEERT_Inovar", "SEERT_Jicafoods",
                    "SEERT_Lippert", "SEERT_Markwins", "SEERT_Nail", "SEERT_Standardfiber",
                    "SEERT_Vapotherm", "SEERT_Arroyo", "SEERT_Bajatec", "SEERT_Crissair",
                    "SEERT_Dosgringos", "SEERT_Hightek", "SEERT_Glue", "SEERT_Lawrence",
                    "SEERT_Integra", "SEERT_Tacna-ai", "SEERT_Eevelle", "SEERT_Segue",
                    "SEERT_Spektrum", "SEERT_Ryte", "SEERT_Desiccare2020", "SEERT_Infinimex",
                    "SEERT_Online", "SEERT_Bst", "SEERT_Mam", "SEERT_Effective",
                    "SEERT_Prettywoman", "SEERT_Thermofab", "SEERT_Centerpiecebf",
                    "SEERT_Desiccare", "SEERT_Jacuzzi", "SEERT_Jacuzzi_", "SEERT_Eurotec",
                    "SEERT_Modelo", "SEERT_Todco", "SEERT_Bajafur"
                }
            }
        };

        public static List<string> FiltrarBasesDatos(string servidor, List<string> basesDatos)
        {
            if (string.IsNullOrWhiteSpace(servidor))
                return basesDatos;

            if (BasesPermitidas.TryGetValue(servidor, out List<string>? permitidas))
            {
                return basesDatos
                    .Where(bd => permitidas.Contains(bd))
                    .OrderBy(bd => bd)
                    .ToList();
            }

            return basesDatos;
        }

        public static bool EsBaseDatosPermitida(string servidor, string baseDatos)
        {
            if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(baseDatos))
                return false;

            if (BasesPermitidas.TryGetValue(servidor, out List<string>? permitidas))
            {
                return permitidas.Contains(baseDatos);
            }

            return true;
        }

        public static List<string> ObtenerBasesPermitidas(string servidor)
        {
            if (BasesPermitidas.TryGetValue(servidor, out List<string>? permitidas))
            {
                return new List<string>(permitidas);
            }

            return new List<string>();
        }

        public static void AgregarBasesPermitidas(string servidor, List<string> bases)
        {
            if (BasesPermitidas.ContainsKey(servidor))
            {
                BasesPermitidas[servidor] = bases;
            }
            else
            {
                BasesPermitidas.Add(servidor, bases);
            }
        }

        public static bool ServidorTieneRestricciones(string servidor)
        {
            return BasesPermitidas.ContainsKey(servidor);
        }
    }
}
