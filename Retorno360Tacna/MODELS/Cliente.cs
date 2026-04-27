namespace Retorno360Tacna.MODELS
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public string? RazonSocial { get; set; }
        public string? Ruc { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }
}
