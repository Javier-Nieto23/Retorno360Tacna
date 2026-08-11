using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    public partial class FrmSolicitud : Form
    {
        private readonly string archivo;
        private readonly string estado;
        private readonly string motivo;
        private readonly string empresa;
        private readonly string usuario;
        private readonly string fechaSolicitud;

        public FrmSolicitud(string archivo, string estado, string motivo, string empresa, string usuario, string fechaSolicitud)
        {
            InitializeComponent();
            this.archivo = archivo;
            this.estado = estado;
            this.motivo = motivo;
            this.empresa = empresa;
            this.usuario = usuario;
            this.fechaSolicitud = fechaSolicitud;

            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 560);
        }

        private void FrmSolicitud_Load(object sender, EventArgs e)
        {
            ConfigurarVista();
            CargarDetalle();
        }

        private void CargarDetalle()
        {
            var detalle = new List<DetalleSolicitudItem>
            {
                new("Archivo", archivo),
                new("Estado", estado),
                new("Empresa", empresa),
                new("Usuario", usuario),
                new("Fecha de solicitud", fechaSolicitud),
                new("Motivo", motivo)
            };

            dgvDetalle.DataSource = detalle;
            if (dgvDetalle.Columns.Contains(nameof(DetalleSolicitudItem.Campo)))
                dgvDetalle.Columns[nameof(DetalleSolicitudItem.Campo)].HeaderText = "Campo";
            if (dgvDetalle.Columns.Contains(nameof(DetalleSolicitudItem.Valor)))
                dgvDetalle.Columns[nameof(DetalleSolicitudItem.Valor)].HeaderText = "Detalle";

            if (dgvDetalle.Columns.Contains(nameof(DetalleSolicitudItem.Campo)))
                dgvDetalle.Columns[nameof(DetalleSolicitudItem.Campo)].FillWeight = 32;
            if (dgvDetalle.Columns.Contains(nameof(DetalleSolicitudItem.Valor)))
                dgvDetalle.Columns[nameof(DetalleSolicitudItem.Valor)].FillWeight = 68;

            dgvDetalle.ClearSelection();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
        }

        private sealed class DetalleSolicitudItem
        {
            public DetalleSolicitudItem(string campo, string valor)
            {
                Campo = campo;
                Valor = valor;
            }

            public string Campo { get; }
            public string Valor { get; }
        }
    }
}
