using System;
using System.Collections.Generic;
using System.Text;

namespace Retorno360Tacna.MODELS
{
    public class EmpresaRazon
    {

        public int IdTabla { get; set; }
        public string NombreTabla { get; set; } = string.Empty;

        public override string ToString() => NombreTabla;
    }
}
