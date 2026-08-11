using System;
using System.Windows.Forms;

namespace Retorno360Tacna.HELPERS
{
    internal static class ErrorMessageHelper
    {
        public static DialogResult ShowError(string mensaje, string titulo = "Error", Exception? exception = null, string? contexto = null)
        {
            string contextoFinal = string.IsNullOrWhiteSpace(contexto)
                ? titulo
                : contexto;

            Exception exceptionParaLog = exception ?? new Exception(mensaje);
            ErrorLogger.LogError(exceptionParaLog, contextoFinal);

            return MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
