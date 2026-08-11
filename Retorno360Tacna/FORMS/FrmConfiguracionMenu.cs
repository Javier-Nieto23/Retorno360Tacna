using System;
using System.Windows.Forms;
using Retorno360Tacna.MODELS;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmConfiguracionMenu : Form
    {
        private readonly ConexionInfo? conexionActual;
        private readonly Usuario? usuarioActual;

        public event EventHandler? AbrirConexionesSolicitado;
        public event EventHandler? AbrirUsuariosSolicitado;
        public event EventHandler? AbrirPlantillaSolicitado;



        public FrmConfiguracionMenu(ConexionInfo? conexion = null, Usuario? usuario = null)
        {
            InitializeComponent();
            conexionActual = conexion;
            usuarioActual = usuario;
        }


        private void AbrirMenuConfiguracion()
        {
            // 1. Instanciamos el menú de configuración
            var menu = new FrmConfiguracionMenu(conexionActual, usuarioActual);

            // 2. Si el usuario hace clic en "Usuarios", abrimos FrmConfiguracion
            menu.AbrirUsuariosSolicitado += (s, e) => {
                var frmConfigUsuarios = new FrmConfiguracion(conexionActual!, usuarioActual);

                // 3. CONEXIÓN DE RETORNO: Cuando se pulse salir en FrmConfiguracion, volvemos a mostrar este menú
                frmConfigUsuarios.RegresarSolicitado += (sender, args) => {
                    AbrirMenuConfiguracion(); // Vuelve a cargar el menú principal de configuración
                };

                MostrarEnPanel(frmConfigUsuarios);
            };

            // Mostramos el menú inicial en el panel contenedor
            MostrarEnPanel(menu);
        }

        // Método auxiliar para incrustar los formularios en el panel
        private void MostrarEnPanel(Form formularioHijo)
        {
           // panelContenedor.Controls.Clear(); // Limpia la vista anterior
            formularioHijo.TopLevel = false;
            formularioHijo.FormBorderStyle = FormBorderStyle.None;
            formularioHijo.Dock = DockStyle.Fill;
           // panelContenedor.Controls.Add(formularioHijo);
            formularioHijo.Show();
        }


        private void BtnConexiones_Click(object? sender, EventArgs e)
        {
            AbrirConexionesSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnUsuarios_Click(object? sender, EventArgs e)
        {
            AbrirUsuariosSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnPlantilla_Click(object? sender, EventArgs e)
        {
            AbrirPlantillaSolicitado?.Invoke(this, EventArgs.Empty);
        }
    }
}
