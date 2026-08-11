


namespace Retorno360Tacna.MODELS
{
    /// <summary>
    /// Modelo para almacenar datos a nivel de detalle de pedimento (sin agrupar)
    /// Se usa para recolectar información de múltiples bases de datos antes de agrupar
    /// </summary>
    public class DatoDetalleIGI
    {


        // Fecha del pedimento (siempre disponible)


        public decimal Gl_ImporteADvalorem { get; set; }
        public decimal IGI_CalculadoDetalle { get; set; }

        public string Gl_FPagoAdvalorem { get; set; } = string.Empty;
      
    }
}
