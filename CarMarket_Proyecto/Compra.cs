using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarMarket_Proyecto
{
    public partial class Compra : Form
    {
        private float anchoOriginal;
        private float altoOriginal;
        public Compra()
        {
            InitializeComponent();
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

        private void Compra_Load(object sender, EventArgs e)
        {
            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;

            DataGridViewCheckBoxColumn columnaCheck = new DataGridViewCheckBoxColumn();
            columnaCheck.Name = "Seleccionar";
            columnaCheck.HeaderText = "";
            dtgCompra.Columns.Add(columnaCheck);

            dtgCompra.Columns.Add("Marca", "Marca");
            dtgCompra.Columns.Add("Modelo", "Modelo");
            dtgCompra.Columns.Add("Año", "Año");
            dtgCompra.Columns.Add("PrecioVenta", "Precio Vendedor");
            dtgCompra.Columns.Add("PrecioMercado", "Precio Mercado");
            dtgCompra.Columns.Add("Estafa", "Advertencia");
            try
            {
                DatosTemporales.ListaPublicaciones =
                    ObtenerPublicacionesDesdeBD();

                MostrarPublicaciones(
                    DatosTemporales.ListaPublicaciones
                );
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los vehículos.\n\n" +
                    ex.Message,
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            dtgCompra.CellContentClick += dtgCompra_CellContentClick;

        }

        private void Compra_Resize(object sender, EventArgs e)
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
        }

        private List<Publicacion> ObtenerPublicacionesDesdeBD()
        {
            List<Publicacion> publicaciones =
                new List<Publicacion>();

            using (SqlConnection conexion =
                ConexionBD.ObtenerConexion())
            {
                using (SqlCommand comando =
                    new SqlCommand(
                        "dbo.sp_ListarPublicaciones",
                        conexion))
                {
                    comando.CommandType =
                        CommandType.StoredProcedure;

                    conexion.Open();

                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            int idPublicacion =
                                Convert.ToInt32(
                                    lector["IdPublicacion"]);

                            string nombreVendedor =
                                lector["NombreVendedor"].ToString();

                            string telefonoVendedor =
                                lector["TelefonoVendedor"].ToString();

                            Usuario vendedor = new Usuario(
                                nombreVendedor,
                                0,
                                "",
                                telefonoVendedor,
                                ""
                            );

                            Vehiculo vehiculo = new Vehiculo(
                                lector["Marca"].ToString(),
                                lector["Modelo"].ToString(),
                                Convert.ToInt32(lector["Anio"]),
                                lector["TipoVehiculo"].ToString(),
                                Convert.ToDouble(
                                    lector["PrecioOriginal"]),
                                Convert.ToDouble(
                                    lector["PrecioVenta"]),
                                Convert.ToDouble(
                                    lector["Kilometraje"]),
                                lector["Color"].ToString(),
                                lector["Detalles"].ToString()
                            );

                            Publicacion publicacion =
                                new Publicacion(
                                    idPublicacion,
                                    vendedor,
                                    vehiculo,
                                    Convert.ToDateTime(
                                        lector["FechaPublicacion"]),
                                    lector["Descripcion"].ToString(),
                                    true
                                );

                            publicaciones.Add(publicacion);
                        }
                    }
                }
            }

            return publicaciones;
        }
        //metodos de mostrar en el gridview 
        private void MostrarPublicaciones(List<Publicacion> publicaciones)
        {
            dtgCompra.Rows.Clear();

            foreach (Publicacion pub in publicaciones)
            {
                if (pub.GetDisponible())
                {

                    int indice = dtgCompra.Rows.Add(
            false,
                    pub.GetVehiculo().GetMarca(),
                    pub.GetVehiculo().GetModelo(),
                    pub.GetVehiculo().GetAño(),
                    pub.GetVehiculo().GetPrecioVenta(),
                    pub.GetVehiculo().CalcularPrecioDevaluado(),
                    pub.GetVehiculo().ObtenerMensajeEstafa()
                );
                    dtgCompra.Rows[indice].Tag = pub; // guarda el objeto completo en esa fila
                }
            }
        }

        private double? ConvertirPrecio(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            texto = texto.Replace("$", "").Trim();

            double precio;

            if (double.TryParse(texto, out precio))
                return precio;

            return null;
        }

        private List<Publicacion> BuscarPublicaciones(List<Publicacion> publicaciones,
                                              string marca,
                                              string tipo,
                                              int? año,
                                              double? precioMaximo)
        {
            List<Publicacion> resultado = new List<Publicacion>();

            foreach (Publicacion pub in publicaciones)
            {
                Vehiculo vehiculo = pub.GetVehiculo();

                if (!pub.GetDisponible())
                    continue;

                if (!string.IsNullOrWhiteSpace(marca) &&
                    vehiculo.GetMarca() != marca)
                    continue;

                if (!string.IsNullOrWhiteSpace(tipo) &&
                    vehiculo.GetTipoCarro() != tipo)
                    continue;

                if (año.HasValue &&
                    vehiculo.GetAño() != año.Value)
                    continue;

                if (precioMaximo.HasValue &&
                    vehiculo.GetPrecioVenta() > precioMaximo.Value)
                    continue;

                resultado.Add(pub);
            }

            return resultado;
        }


        private void pbVolver_Click(object sender, EventArgs e)
        {
            PantallaInicio pantallaInicio = new PantallaInicio();
            pantallaInicio.Show();
            this.Hide();
        }

        private void btFiltrar_Click(object sender, EventArgs e)
        {
            string marca = cbMarcaC.Text;
            string tipo = cbTipoC.Text;

            int? año = null;
            if (!string.IsNullOrWhiteSpace(cbAñoC.Text))
            {
                int añoConvertido;
                if (int.TryParse(cbAñoC.Text, out añoConvertido))
                {
                    año = añoConvertido;
                }
            }

            double? precio = null;

            if (!string.IsNullOrWhiteSpace(cbPrecioC.Text))
            {
                string[] partes = cbPrecioC.Text.Split('-');

                if (partes.Length == 2)
                {
                    precio = ConvertirPrecio(partes[1]);
                }
            }

            List<Publicacion> resultados = BuscarPublicaciones(
                DatosTemporales.ListaPublicaciones,
                marca,
                tipo,
                año,
                precio);

            MostrarPublicaciones(resultados);

            if (resultados.Count == 0)
            {
                MessageBox.Show("No se encontraron vehículos.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Publicacion publicacionSeleccionada = null;

            foreach (DataGridViewRow fila in dtgCompra.Rows)
            {
                bool estaMarcado = Convert.ToBoolean(fila.Cells["Seleccionar"].Value);

                if (estaMarcado)
                {
                    publicacionSeleccionada = (Publicacion)fila.Tag;
                    break;
                }
            }

            if (publicacionSeleccionada == null)
            {
                MessageBox.Show("Selecciona un vehículo antes de continuar");
                return;
            }

            Factura formFactura = new Factura(publicacionSeleccionada); // pasas la publicación al constructor
            formFactura.Show();
            this.Hide();
        }

        private void dtgCompra_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dtgCompra.Columns["Seleccionar"].Index && e.RowIndex >= 0)
            {
                foreach (DataGridViewRow fila in dtgCompra.Rows)
                {
                    if (fila.Index != e.RowIndex)
                    {
                        fila.Cells["Seleccionar"].Value = false;
                    }
                }
            }
        }
    }
}