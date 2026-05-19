using System;

namespace Retorno360Tacna.MODELS
{
    public class DetalleComponente
    {
        public string Par_NoParte { get; set; } = string.Empty;
        public string Par_DescripcionEsp { get; set; } = string.Empty;
        public DateTime? Par_InsercionFecha { get; set; }
        public string ExisteEnBOM { get; set; } = string.Empty;
    }
}
