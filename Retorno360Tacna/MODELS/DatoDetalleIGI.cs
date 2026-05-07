namespace Retorno360Tacna.MODELS
{
    /// <summary>
    /// Modelo para almacenar datos a nivel de detalle de pedimento (sin agrupar)
    /// Se usa para recolectar información de múltiples bases de datos antes de agrupar
    /// </summary>
    internal class DatoDetalleIGI
    {
        public string BaseDatos { get; set; } = string.Empty;
        public int Pim_Consecutivo { get; set; }
        public string Adu_AduanaSecc { get; set; } = string.Empty;
        public string AgP_Patente { get; set; } = string.Empty;
        public string Pim_Folio { get; set; } = string.Empty;

        // Fecha del pedimento (siempre disponible)
        public DateTime? Pim_FechaPago { get; set; }

        // Fecha de la glosa (puede ser null si no hay TR_GLOSA)
        public DateTime? Gl_FecPagoReal { get; set; }

        public decimal Gl_ImporteADvalorem { get; set; }
        public decimal IGI_CalculadoDetalle { get; set; }
        public decimal Gl_ImporteIVA { get; set; }
        public string Gl_FPagoAdvalorem { get; set; } = string.Empty;
        public string Gl_FPagoIVA { get; set; } = string.Empty;
        public string? Gl_Pedimento { get; set; }
        public string? Gl_OrigenZipGlosa { get; set; }

        /// <summary>
        /// Identificador único del pedimento (para agrupar)
        /// </summary>
        public string PedimentoCompleto => $"{Adu_AduanaSecc}-{AgP_Patente}-{Pim_Folio}";
    }
}
