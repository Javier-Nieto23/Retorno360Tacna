using System;

namespace Retorno360Tacna.MODELS
{
    /// <summary>
    /// Información de conexión externa para razones sociales
    /// Basado en RAZONXTABLA.ConnExterna e IdConexion
    /// </summary>
    public class ConexionExternaInfo
    {
        public string BaseDatos { get; set; } = string.Empty;
        public bool TieneConexionExterna { get; set; }
        public int? IdConexion { get; set; }

        // Información de la conexión (de tabla Conexiones)
        public string? Servidor { get; set; }
        public string? UsuarioSQL { get; set; }
        public string? PasswordSQL { get; set; }
        public string? NombreConexion { get; set; }

        public ConexionExternaInfo()
        {
        }

        /// <summary>
        /// Indica si debe usar la conexión principal o una externa
        /// </summary>
        public bool UsarConexionPrincipal => !TieneConexionExterna || IdConexion == null;
    }
}
