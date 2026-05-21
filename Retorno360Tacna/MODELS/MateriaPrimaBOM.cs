using System;

namespace Retorno360Tacna.MODELS
{
    public class MateriaPrimaBOM
    {
        public string Par_NoParte { get; set; } = string.Empty;
        public string Par_DescripcionEsp { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public DateTime? Par_InsercionFecha { get; set; }
        public string EstatusComponente { get; set; } = string.Empty;
    }
}
