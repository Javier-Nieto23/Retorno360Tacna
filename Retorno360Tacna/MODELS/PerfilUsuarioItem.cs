namespace Retorno360Tacna.MODELS
{
    public class PerfilUsuarioItem
    {
        public int IdConsecutivo { get; set; }
        public int IdUsuario { get; set; }
        public int IdRazonSocial { get; set; }
        public int IdEmpresa { get; set; } // FK a NOM_TABLARAZON.IdTabla
    }
}