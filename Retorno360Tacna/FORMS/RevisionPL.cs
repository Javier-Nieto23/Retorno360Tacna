using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Amazon.Runtime.Internal.Auth;

namespace Retorno360Tacna.FORMS
{
    public partial class RevisionPL : Form
    {

        #region Atributos 
        //private List<RazonSocial> razonesSociales;
        private MODELS.Usuario? usuarioActual;
        
        //private PerfilService? perfilService;
        //private ReporteService? reporteService;
        #endregion


        #region Metodos 

        public async void CargarRazonesSociales()
        {
            
     

        }





        #endregion 


        public RevisionPL()
        {
            InitializeComponent();
        }

        private void RevisionPL_Load(object sender, EventArgs e)
        {

        }
    }
}
