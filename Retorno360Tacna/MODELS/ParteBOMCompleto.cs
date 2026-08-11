using System;

namespace Retorno360Tacna.MODELS
{
    public class ParteBOMCompleto
    {
        public string NoPartePadre { get; set; } = string.Empty;
        public string Par_DescripcionEsp { get; set; } = string.Empty;
        public DateTime? Par_InsercionFecha { get; set; }
        public DateTime? Bom_FechaInicio { get; set; }
        public DateTime? Bom_FechaFin { get; set; }
        public int TotalComponentes { get; set; }
        public int ComponentesVigentes { get; set; }
        public int ComponentesNoVigentes { get; set; }
        public string EstatusBOM { get; set; } = string.Empty;
    }
}
