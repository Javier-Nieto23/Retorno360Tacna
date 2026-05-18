using System;

namespace Retorno360Tacna.MODELS
{
    public class ParteBOM
    {
        public string NoPartePadre { get; set; } = string.Empty;
        public string Par_DescripcionEsp { get; set; } = string.Empty;
        public DateTime? Par_InsercionFecha { get; set; }
        public DateTime? Bom_FechaInicio { get; set; }
        public DateTime? Bom_FechaFin { get; set; }
        public int TotalComponentes { get; set; }
        public int TotalSUB { get; set; }
        public int TotalMP { get; set; }
        public int TotalEQ { get; set; }
        public int TotalRT { get; set; }
        public int TotalEMP { get; set; }
        public int TotalMAQ { get; set; }
        public int TotalOtros { get; set; }
        public string EstatusBOM { get; set; } = string.Empty;
    }
}
