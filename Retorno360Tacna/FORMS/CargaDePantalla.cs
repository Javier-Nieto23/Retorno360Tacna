using Retorno360Tacna.MODELS;
using Retorno360Tacna.FORMS;

namespace Retorno360Tacna
{
    public partial class CargaDePantalla : Form
    {
        private CargadorInicial? cargador;
        private Usuario? usuario;
        private ConexionInfo? conexion;
        private FORMS.MainMenu? mainMenuPrecargado;

        public CargaDePantalla()
        {
            InitializeComponent();
        }

        public CargaDePantalla(Usuario usuarioActual, ConexionInfo conexionActual)
        {
            InitializeComponent();
            usuario = usuarioActual;
            conexion = conexionActual;
            cargador = new CargadorInicial(500); // Reducido a 500ms por mensaje

            cargador.MensajeCambio += Cargador_MensajeCambio;
            cargador.CargaCompleta += Cargador_CargaCompleta;
        }

        private async void CargaDePantalla_Load(object sender, EventArgs e)
        {
            if (cargador != null && usuario != null && conexion != null)
            {
                // Definir tareas de pre-carga
                var tareasPreCarga = new List<Action>
                {
                    // Pre-crear el MainMenu (sin mostrarlo aún)
                    () => {
                        if (usuario != null && conexion != null)
                        {
                            mainMenuPrecargado = new FORMS.MainMenu(usuario, conexion);

                            // Forzar la creación del handle (inicialización de controles)
                            // Esto carga los botones, paneles, etc. en segundo plano
                            var handle = mainMenuPrecargado.Handle;
                        }
                    }
                };

                await cargador.IniciarCargaAsync(tareasPreCarga);
            }
            else if (cargador != null)
            {
                await cargador.IniciarCargaAsync();
            }
        }

        private void Cargador_MensajeCambio(object? sender, string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => lblMensajeCarga.Text = mensaje));
            }
            else
            {
                lblMensajeCarga.Text = mensaje;
            }
        }

        private void Cargador_CargaCompleta(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => this.DialogResult = DialogResult.OK));
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        public Usuario? ObtenerUsuario() => usuario;
        public ConexionInfo? ObtenerConexion() => conexion;

        /// <summary>
        /// Obtiene el MainMenu pre-cargado (si existe)
        /// </summary>
        public Retorno360Tacna.FORMS.MainMenu? ObtenerMainMenuPrecargado() => mainMenuPrecargado;
    }
}
