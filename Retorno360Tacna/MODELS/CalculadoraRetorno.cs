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


    }
}

