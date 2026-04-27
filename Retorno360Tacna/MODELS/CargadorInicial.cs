namespace Retorno360Tacna.MODELS
{
    public class CargadorInicial
    {
        public event EventHandler<string>? MensajeCambio;
        public event EventHandler? CargaCompleta;

        private readonly List<string> mensajesCarga;
        private readonly int tiempoEntreMensajes;

        public CargadorInicial(int tiempoMilisegundos = 800)
        {
            tiempoEntreMensajes = tiempoMilisegundos;
            mensajesCarga = new List<string>
            {
                "Iniciando sistema...",
                "Cargando bases de datos...",
                "Verificando conexiones...",
                "Cargando Archivos...",
                "Cargando pedimentos...",
                "Procesando retornos...",
                "Configurando reportes...",
                "Preparando interfaz...",
                "Cargando Usuario...",
                "Finalizando carga..."
            };
        }

        public CargadorInicial(List<string> mensajesPersonalizados, int tiempoMilisegundos = 800)
        {
            tiempoEntreMensajes = tiempoMilisegundos;
            mensajesCarga = mensajesPersonalizados ?? new List<string> { "Cargando..." };
        }

        public async Task IniciarCargaAsync()
        {
            foreach (var mensaje in mensajesCarga)
            {
                MensajeCambio?.Invoke(this, mensaje);
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
