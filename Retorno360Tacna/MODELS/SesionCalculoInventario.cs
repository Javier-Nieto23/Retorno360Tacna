using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Retorno360Tacna.FORMS;

namespace Retorno360Tacna.MODELS
{
    
        //<summary>
        //Mantiene el estado de la sesion de captura dentro de FrmCalculoInventarios:
        //cuantos meses se van a calcular y los resultados ya calculados.
        //</summary>
        public sealed class SesionCalculoInventario
        {
            public int CantidadMesesObjetivo { get; private set; }
            public BindingList<ResultadoInventarioMes> Resultados { get; } = new();

            public void Iniciar(int cantidadMeses)
            {
                CantidadMesesObjetivo = cantidadMeses;
                Resultados.Clear();
            }

            public int MesesCompletados =>
                Resultados
                    .Where(r => !r.TieneError)
                    .Select(r => new { r.TipoInventario, r.NumeroMes })
                    .Distinct()
                    .Count();

            public bool SesionCompleta => CantidadMesesObjetivo > 0 && MesesCompletados >= CantidadMesesObjetivo;
        }
    
}
