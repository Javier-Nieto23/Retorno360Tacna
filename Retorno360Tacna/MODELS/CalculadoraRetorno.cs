using System;

namespace Retorno360Tacna.MODELS
{
    public static class CalculadoraRetorno
    {
        public static decimal CalcularPorcentajeRetorno(decimal valorImportado, decimal valorExportado)
        {
            if (valorImportado <= 0)
                return 0;

            return Math.Round((valorExportado / valorImportado) * 100, 2);
        }

        public static decimal CalcularPorcentajeRetorno(decimal valorImportado, decimal valorExportado, int decimales)
        {
            if (valorImportado <= 0)
                return 0;

            return Math.Round((valorExportado / valorImportado) * 100, decimales);
        }

        public static ResultadoPorcentaje CalcularConDetalle(decimal valorImportado, decimal valorExportado)
        {
            decimal porcentaje = CalcularPorcentajeRetorno(valorImportado, valorExportado);
            decimal diferencia = valorExportado - valorImportado;
            
            EstadoRetorno estado;
            if (porcentaje >= 100)
                estado = EstadoRetorno.Completo;
            else if (porcentaje >= 80)
                estado = EstadoRetorno.BuenAvance;
            else if (porcentaje >= 50)
                estado = EstadoRetorno.Avance;
            else if (porcentaje > 0)
                estado = EstadoRetorno.Bajo;
            else
                estado = EstadoRetorno.SinRetorno;

            return new ResultadoPorcentaje
            {
                ValorImportado = valorImportado,
                ValorExportado = valorExportado,
                Porcentaje = porcentaje,
                Diferencia = diferencia,
                Estado = estado,
                PorcentajeFaltante = Math.Max(0, 100 - porcentaje)
            };
        }

        public static string ObtenerMensajeEstado(EstadoRetorno estado)
        {
            return estado switch
            {
                EstadoRetorno.Completo => "Retorno completo (100% o más)",
                EstadoRetorno.BuenAvance => "Buen avance de retorno (80-99%)",
                EstadoRetorno.Avance => "Avance moderado (50-79%)",
                EstadoRetorno.Bajo => "Retorno bajo (1-49%)",
                EstadoRetorno.SinRetorno => "Sin retorno",
                _ => "Estado desconocido"
            };
        }

        public static (decimal importado, decimal exportado, decimal porcentaje) CalcularTotales(
            IEnumerable<(decimal importado, decimal exportado)> operaciones)
        {
            decimal totalImportado = 0;
            decimal totalExportado = 0;

            foreach (var operacion in operaciones)
            {
                totalImportado += operacion.importado;
                totalExportado += operacion.exportado;
            }

            decimal porcentaje = CalcularPorcentajeRetorno(totalImportado, totalExportado);

            return (totalImportado, totalExportado, porcentaje);
        }
    }

    public class ResultadoPorcentaje
    {
        public decimal ValorImportado { get; set; }
        public decimal ValorExportado { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Diferencia { get; set; }
        public decimal PorcentajeFaltante { get; set; }
        public EstadoRetorno Estado { get; set; }

        public override string ToString()
        {
            return $"Importado: ${ValorImportado:N2} | Exportado: ${ValorExportado:N2} | Porcentaje: {Porcentaje:N2}% | Estado: {Estado}";
        }
    }

    public enum EstadoRetorno
    {
        SinRetorno = 0,
        Bajo = 1,
        Avance = 2,
        BuenAvance = 3,
        Completo = 4
    }
}
