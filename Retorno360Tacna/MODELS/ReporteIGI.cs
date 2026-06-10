namespace Retorno360Tacna.MODELS
{
    /// <summary>
    /// Modelo base para reportes que requieren conexión a bases de datos específicas
    /// </summary>
    public abstract class ReporteBase
    {
        public string BaseDatos { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    /// <summary>
    /// Modelo para el reporte de IGI Pagado
    /// </summary>
    public class ReporteIGIPagado : ReporteBase
    {
        public int IdPedimento { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Pedimento { get; set; } = string.Empty;
        public DateTime? FechaPago { get; set; }
        public decimal IGI_Pagado { get; set; }
        public decimal IGI_Calculado { get; set; }
        public decimal IVA_Pagado { get; set; }
        public string FormaPago_IGI { get; set; } = string.Empty;
        public string FormaPago_IVA { get; set; } = string.Empty;
        public string EstatusGlosa { get; set; } = string.Empty;
        public string EstatusOrigen { get; set; } = string.Empty;

        /// <summary>
        /// Calcula la diferencia (ahorro) entre IGI Calculado y Pagado
        /// Valor positivo = ahorro (se pagó menos de lo calculado)
        /// Si la forma de pago es '0' (crédito), se invierte el signo para reflejar deuda (valor negativo)
        /// </summary>
        public decimal DiferenciaIGI
        {
            get
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(FormaPago_IGI) && FormaPago_IGI.Trim() == "5")
                    {
                        // Para forma de pago 0, mostrar la diferencia como negativa (no hubo pago real)
                        return IGI_Pagado - IGI_Calculado;
                    }
                }
                catch { }
                return IGI_Calculado - IGI_Pagado;
            }
        }
    }

    /// <summary>
    /// Modelo para el resumen del reporte de IGI
    /// </summary>
    public class ResumenIGI
    {
        public decimal TotalIGI_Pagado { get; set; }
        public decimal TotalIGI_Calculado { get; set; }
        public decimal TotalIVA_Pagado { get; set; }
        public decimal DiferenciaTotal => TotalIGI_Calculado - TotalIGI_Pagado;
        public int TotalPedimentos { get; set; }
        public int PedimentosCargadosGlosa { get; set; }
        public int PedimentosNoCargados => TotalPedimentos - PedimentosCargadosGlosa;
    }

    /// <summary>
    /// Resumen por forma de pago IGI (resultado similar al GROUP BY del query de detalle)
    /// </summary>
    public class ResumenFormaPagoIGI
    {
        public string FormaPago { get; set; } = string.Empty;
        public int TotalPedimentos { get; set; }
        public int TotalPartidas { get; set; }
        public decimal TotalIGI_Pagado { get; set; }
        public decimal TotalIGI_Calculado { get; set; }
        public decimal Diferencia { get; set; }
    }
}
