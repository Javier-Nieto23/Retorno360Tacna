namespace Retorno360Tacna.MODELS
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }
        public string? NombreCompleto { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
