using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarMarket_Proyecto
{
    

    public partial class Factura : Form
    {
        private float anchoOriginal;
        private float altoOriginal;
        public Publicacion PublicacionRecibida { get; set; }

        public Factura(Publicacion publicacion)
{
    InitializeComponent();
    PublicacionRecibida = publicacion;
}


        // desde aqui el codigo se encarga de minimizar, y cerrar y aparte mantener el tamaño en caso de que se minimice y se maximice la ventana
        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pbMaximizar_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal; // lo regresa a su tamaño normal
            }
            else
            {
                this.WindowState = FormWindowState.Maximized; // lo vuelve pantalla completa
            }
        }

        private void Factura_Load(object sender, EventArgs e)
        {
            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;

            lvFactura.Items.Clear();

            lvFactura.Items.Add("Marca: " + PublicacionRecibida.GetVehiculo().GetMarca());
            lvFactura.Items.Add("Modelo: " + PublicacionRecibida.GetVehiculo().GetModelo());
            lvFactura.Items.Add("Año: " + PublicacionRecibida.GetVehiculo().GetAño());
            lvFactura.Items.Add("Precio: " + PublicacionRecibida.GetVehiculo().GetPrecioVenta());
            lvFactura.Items.Add("Vendedor: " + PublicacionRecibida.GetVendedor().GetNombre());
        }

        private void Factura_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (anchoOriginal == 0 || altoOriginal == 0)
            {
                return;
            }

            float factorX = this.ClientSize.Width / anchoOriginal;
            float factorY = this.ClientSize.Height / altoOriginal;

            if (factorX <= 0 || factorY <= 0 || float.IsInfinity(factorX) || float.IsInfinity(factorY))
            {
                return;
            }

            EscalarControles(this.Controls, factorX, factorY);

            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;
        }
        private void EscalarControles(Control.ControlCollection controles, float factorX, float factorY)
        {
            foreach (Control control in controles)
            {
                control.Left = (int)(control.Left * factorX);
                control.Top = (int)(control.Top * factorY);
                control.Width = (int)(control.Width * factorX);
                control.Height = (int)(control.Height * factorY);

                if (control.Font != null)
                {
                    control.Font = new Font(control.Font.FontFamily,
                        control.Font.Size * Math.Min(factorX, factorY),
                        control.Font.Style);
                }

                if (control.HasChildren)
                {
                    EscalarControles(control.Controls, factorX, factorY);
                }
            }
            // hasta aqui se trabaja las escalas de la ventana
        }

        private void pbVolverC_Click(object sender, EventArgs e)
        {
            Compra formCompra = new Compra();
            formCompra.Show();
            this.Close();
        }
    }
}
