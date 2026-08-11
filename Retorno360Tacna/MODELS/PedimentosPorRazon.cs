namespace Retorno360Tacna.MODELS
{
    public class PedimentosPorRazon
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string BaseDatosOrigen { get; set; } = string.Empty;
        public int CantidadImportaciones { get; set; }
        public int CantidadExportaciones { get; set; }
        public int TotalPedimentos { get; set; }
        public decimal ValorImportaciones { get; set; }
        public decimal ValorExportaciones { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
