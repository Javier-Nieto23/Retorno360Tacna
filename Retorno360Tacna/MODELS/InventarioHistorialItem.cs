namespace Retorno360Tacna.MODELS
{
    public class InventarioHistorialItem
    {
        public int NumeroMes { get; set; }
        public string Mes { get; set; } = string.Empty;
        public string TipoInventario { get; set; } = string.Empty;
        public string Operacion { get; set; } = string.Empty;
        public string CampoTotal { get; set; } = string.Empty;
        public string? CampoA { get; set; }
        public string? CampoB { get; set; }
        public decimal Total { get; set; }
        public int IdEmpresa { get; set; }
        public int IdRazonSocial { get; set; }
    }
}