using Retorno360Tacna.MODELS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Retorno360Tacna.FORMS
{
    /// <summary>
    /// Diálogo para seleccionar la razón social cuando el usuario tiene más de una en su perfil.
    /// </summary>
    public class FrmSeleccionRazonPerfil : Form
    {
        private Panel panelHeader;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Panel panelCuerpo;
        private ListBox lstRazones;
        private Label lblInstruccion;
        private Panel panelBotones;
        private Button btnAceptar;
        private Button btnCancelar;

        public RazonSocial? RazonSeleccionada { get; private set; }

        public FrmSeleccionRazonPerfil(List<RazonSocial> razones)
        {
            InicializarComponentes();
            lstRazones.DataSource = razones;
            lstRazones.DisplayMember = "NombreRazon";
            lstRazones.ValueMember = "IdRazon";
            if (lstRazones.Items.Count > 0)
                lstRazones.SelectedIndex = 0;
        }

        private void InicializarComponentes()
        {
            // Header
            panelHeader = new Panel
            {
                BackColor = Color.FromArgb(39, 174, 96),
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(20, 0, 20, 0)
            };

            lblTitulo = new Label
            {
                Text = "Seleccionar Razón Social",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Location = new Point(20, 10),
                Size = new Size(400, 28)
            };

            lblSubtitulo = new Label
            {
                Text = "Tu perfil contiene más de una razón social",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(220, 255, 220),
                AutoSize = false,
                Location = new Point(20, 40),
                Size = new Size(400, 20)
            };

            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Controls.Add(lblSubtitulo);

            // Cuerpo
            panelCuerpo = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            lblInstruccion = new Label
            {
                Text = "¿Qué razón social deseas consultar primero?",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(400, 22)
            };

            lstRazones = new ListBox
            {
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 52),
                Size = new Size(400, 160),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.One
            };
            lstRazones.DoubleClick += (s, e) => AceptarSeleccion();

            panelCuerpo.Controls.Add(lblInstruccion);
            panelCuerpo.Controls.Add(lstRazones);

            // Botones
            panelBotones = new Panel
            {
                BackColor = Color.FromArgb(236, 240, 241),
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(10)
            };

            btnAceptar = new Button
            {
                Text = "Consultar",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 174, 96),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 36),
                Location = new Point(10, 10),
                Cursor = Cursors.Hand
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (s, e) => AceptarSeleccion();

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(52, 73, 94),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 36),
                Location = new Point(150, 10),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);

            // Form
            Text = "Selección de Razón Social";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(450, 340);
            BackColor = Color.White;
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;

            Controls.Add(panelCuerpo);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
        }

        private void AceptarSeleccion()
        {
            if (lstRazones.SelectedItem is RazonSocial razon)
            {
                RazonSeleccionada = razon;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Selecciona una razón social para continuar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
