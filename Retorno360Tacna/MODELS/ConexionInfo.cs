namespace Retorno360Tacna.MODELS
{
    public class ConexionInfo
    {
        public int IdConexion { get; set; }
        public string? NombreConexion { get; set; }
        public string? Servidor { get; set; }
        public string? UsuarioSQL { get; set; }
        public string? PasswordSQL { get; set; }
        public string? TipoMotor { get; set; }
        public bool Activo { get; set; }

        public override string ToString()
        {
            return NombreConexion ?? string.Empty;
        }
    }
}
