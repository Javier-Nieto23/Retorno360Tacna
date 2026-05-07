namespace Retorno360Tacna.MODELS
{
    public class ConfiguracionUsuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal EscalaUI { get; set; } = 1.0m; // 1.0 = 100%, 1.25 = 125%, 1.5 = 150%
        public string TemaColor { get; set; } = "Default";
        public bool ModoOscuro { get; set; } = false;

        public ConfiguracionUsuario()
        {
        }

        public ConfiguracionUsuario(int idUsuario, string nombreUsuario, decimal escalaUI = 1.0m)
        {
            IdUsuario = idUsuario;
            NombreUsuario = nombreUsuario;
            EscalaUI = escalaUI;
        }
    }
}
