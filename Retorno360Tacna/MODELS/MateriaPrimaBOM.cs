using System;

namespace Retorno360Tacna.MODELS
{
    public class MateriaPrimaBOM
    {
        public int Par_Consecutivo { get; set; }
        public string BaseDatosOrigenConsulta { get; set; } = string.Empty;
        public string Par_NoParte { get; set; } = string.Empty;
        public string Par_DescripcionEsp { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public DateTime? Par_InsercionFecha { get; set; }
        public string EstatusComponente { get; set; } = string.Empty;
        public string DetallePedimentosGlosa { get; set; } = string.Empty;
        public string DetallePedimentosInfo { get; set; } = string.Empty;
        public List<DetallePedimentoParte> PedimentosRelacionados { get; set; } = new();
    }

    public class DetallePedimentoParte
    {
        public string Pedimento { get; set; } = string.Empty;
        public string TipoOperacion { get; set; } = string.Empty;
        public string ClavePedimento { get; set; } = string.Empty;
        public int CantidadPartidasMismaParte { get; set; }
    }
}
