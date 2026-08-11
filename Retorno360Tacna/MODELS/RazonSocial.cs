namespace Retorno360Tacna.MODELS
{
    public class RazonSocial
    {
        public int IdRazon { get; set; }
        public string NombreRazon { get; set; } = string.Empty;
        public string BaseDatosOrigen { get; set; } = string.Empty;

        public override string ToString()
        {
            return NombreRazon;
        }
    }

    public class PedimentoComparacion
    {
        public string Tipo { get; set; } = string.Empty;
        public string Aduana { get; set; } = string.Empty;
        public string Patente { get; set; } = string.Empty;
        public string Pedimento { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public bool ExisteEnGlosa { get; set; }
    }
}
