namespace Retorno360Tacna.MODELS
{
    public class CargadorInicial
    {
        public event EventHandler<string>? MensajeCambio;
        public event EventHandler? CargaCompleta;

        private readonly List<string> mensajesCarga;
        private readonly int tiempoEntreMensajes;
        private readonly List<Action>? tareasPreCarga;

        public CargadorInicial(int tiempoMilisegundos = 500)
        {
            tiempoEntreMensajes = tiempoMilisegundos;
            mensajesCarga = new List<string>
            {
                "Iniciando sistema...",
                "Verificando conexión al servidor...",
                "Cargando configuración...",
                "Preparando interfaz gráfica...",
                "Inicializando menú principal...",
                "Cargando recursos...",
                "Configurando componentes...",
                "Finalizando carga..."
            };
        }

        public CargadorInicial(List<string> mensajesPersonalizados, int tiempoMilisegundos = 500)
        {
            tiempoEntreMensajes = tiempoMilisegundos;
            mensajesCarga = mensajesPersonalizados ?? new List<string> { "Cargando..." };
        }

        /// <summary>
        /// Inicia la carga con pre-carga opcional de tareas
        /// </summary>
        public async Task IniciarCargaAsync(List<Action>? tareasParaPrecargar = null)
        {
            int totalPasos = mensajesCarga.Count;
            int pasoActual = 0;

            foreach (var mensaje in mensajesCarga)
            {
                pasoActual++;
                MensajeCambio?.Invoke(this, mensaje);

                // En ciertos pasos, ejecutar tareas de pre-carga
                if (tareasParaPrecargar != null && pasoActual == 4)
                {
                    // Paso 4: "Preparando interfaz gráfica..."
                    // Pre-cargar componentes de la UI aquí
                    // IMPORTANTE: Ejecutar en el MISMO thread (UI thread) no en Task.Run
                    foreach (var tarea in tareasParaPrecargar)
                    {
                        try
                        {
                            tarea?.Invoke();
                        }
                        catch { /* Ignorar errores en pre-carga */ }
                    }
                }

                await Task.Delay(tiempoEntreMensajes);
            }

            CargaCompleta?.Invoke(this, EventArgs.Empty);
        }

        public List<string> ObtenerMensajes()
        {
            return new List<string>(mensajesCarga);
        }

        public void AgregarMensaje(string mensaje)
        {
            mensajesCarga.Add(mensaje);
        }

        public void LimpiarMensajes()
        {
            mensajesCarga.Clear();
        }
    }
}
