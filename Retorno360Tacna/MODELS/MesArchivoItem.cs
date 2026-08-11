using System;
using System.IO;

namespace Retorno360Tacna.MODELS
{
    public enum ModoCalculoMes
    {
        SumaSimple = 0,
        ProductoAB = 1
    }

    public class MesArchivoItem
    {
        public string Mes { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string? HojaSeleccionada { get; set; }

        public string? CampoSimple { get; set; }
        public string? CampoA { get; set; }
        public string? CampoB { get; set; }

        public ModoCalculoMes Modo { get; set; } = ModoCalculoMes.SumaSimple;

        public decimal Total { get; set; }
        public bool Calculado { get; set; }

        public string NombreArchivo => string.IsNullOrWhiteSpace(RutaArchivo) ? string.Empty : Path.GetFileName(RutaArchivo);

        public string ModoTexto => Modo == ModoCalculoMes.SumaSimple ? "Suma simple" : "Producto A×B";

        public bool EstaCompleto()
        {
            if (string.IsNullOrWhiteSpace(RutaArchivo)) return false;
            if (string.IsNullOrWhiteSpace(HojaSeleccionada)) return false;
            return Modo == ModoCalculoMes.SumaSimple
                ? !string.IsNullOrWhiteSpace(CampoSimple)
                : !string.IsNullOrWhiteSpace(CampoA) && !string.IsNullOrWhiteSpace(CampoB);
        }
    }
}
