using Retorno360Tacna.MODELS;
using Retorno360Tacna.FORMS;

namespace Retorno360Tacna
{
    public partial class CargaDePantalla : Form
    {
        private CargadorInicial? cargador;
        private Usuario? usuario;
        private ConexionInfo? conexion;

        public CargaDePantalla()
        {
            InitializeComponent();
        }

        public CargaDePantalla(Usuario usuarioActual, ConexionInfo conexionActual)
        {
            InitializeComponent();
            usuario = usuarioActual;
            conexion = conexionActual;
            cargador = new CargadorInicial(700);

            cargador.MensajeCambio += Cargador_MensajeCambio;
            cargador.CargaCompleta += Cargador_CargaCompleta;
        }

        private async void CargaDePantalla_Load(object sender, EventArgs e)
        {
            if (cargador != null)
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

        private void AbrirMainMenu()
        {
            if (usuario != null && conexion != null)
            {
                Retorno360Tacna.FORMS.MainMenu mainMenu = new Retorno360Tacna.FORMS.MainMenu(usuario, conexion);
                mainMenu.Show();
            }
        }

        public Usuario? ObtenerUsuario() => usuario;
        public ConexionInfo? ObtenerConexion() => conexion;
    }
}
