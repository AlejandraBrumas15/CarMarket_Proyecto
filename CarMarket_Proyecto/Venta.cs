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
    public partial class Venta : Form
    {
        private float anchoOriginal;
        private float altoOriginal;

        public Venta()
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

        private void Venta_Load(object sender, EventArgs e)
        {
            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;
        }

        private void Venta_Resize(object sender, EventArgs e)
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
        //hasta aqui


        private void pbVolver_Click(object sender, EventArgs e)
        {
            PantallaInicio pantallaInicio = new PantallaInicio();
            pantallaInicio.Show();
            this.Hide();
        }

        private void btPublicar_Click(object sender, EventArgs e)
        {
            if (DatosTemporales.IdUsuarioActual <= 0)
            {
                MessageBox.Show(
                    "Debe iniciar sesión antes de publicar un vehículo.",
                    "Sesión requerida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string marca = cbMarca.Text.Trim();
            string modelo = txtModelo.Text.Trim();
            string tipo = cbTipo.Text.Trim();
            string color = txtColor.Text.Trim();
            string detalles = txtDetalles.Text.Trim();

            short año;
            decimal precioOriginal;
            decimal precioVenta;
            decimal kilometraje;

            if (string.IsNullOrWhiteSpace(marca) ||
                string.IsNullOrWhiteSpace(modelo) ||
                string.IsNullOrWhiteSpace(tipo) ||
                string.IsNullOrWhiteSpace(color))
            {
                MessageBox.Show(
                    "Complete todos los datos obligatorios.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!short.TryParse(cbAño.Text, out año))
            {
                MessageBox.Show(
                    "Seleccione un año válido.",
                    "Año incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!decimal.TryParse(
                    txtPrecioOriginal.Text,
                    out precioOriginal) ||
                precioOriginal <= 0)
            {
                MessageBox.Show(
                    "Ingrese un precio original válido.",
                    "Precio incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPrecioOriginal.Focus();
                return;
            }

            if (!decimal.TryParse(
                    txtPrecioVenta.Text,
                    out precioVenta) ||
                precioVenta <= 0)
            {
                MessageBox.Show(
                    "Ingrese un precio de venta válido.",
                    "Precio incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPrecioVenta.Focus();
                return;
            }

            if (!decimal.TryParse(
                    txtKilometro.Text,
                    out kilometraje) ||
                kilometraje < 0)
            {
                MessageBox.Show(
                    "Ingrese un kilometraje válido.",
                    "Kilometraje incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtKilometro.Focus();
                return;
            }

            try
            {
                using (SqlConnection conexion =
                    ConexionBD.ObtenerConexion())
                {
                    using (SqlCommand comando =
                        new SqlCommand(
                            "dbo.sp_PublicarVehiculo",
                            conexion))
                    {
                        comando.CommandType =
                            CommandType.StoredProcedure;

                        comando.Parameters.Add(
                            "@IdVendedor",
                            SqlDbType.Int
                        ).Value = DatosTemporales.IdUsuarioActual;

                        comando.Parameters.Add(
                            "@Marca",
                            SqlDbType.NVarChar,
                            50
                        ).Value = marca;

                        comando.Parameters.Add(
                            "@Modelo",
                            SqlDbType.NVarChar,
                            80
                        ).Value = modelo;

                        comando.Parameters.Add(
                            "@Anio",
                            SqlDbType.SmallInt
                        ).Value = año;

                        comando.Parameters.Add(
                            "@TipoVehiculo",
                            SqlDbType.NVarChar,
                            30
                        ).Value = tipo;

                        SqlParameter parametroPrecioOriginal =
                            comando.Parameters.Add(
                                "@PrecioOriginal",
                                SqlDbType.Decimal
                            );

                        parametroPrecioOriginal.Precision = 12;
                        parametroPrecioOriginal.Scale = 2;
                        parametroPrecioOriginal.Value = precioOriginal;

                        SqlParameter parametroPrecioVenta =
                            comando.Parameters.Add(
                                "@PrecioVenta",
                                SqlDbType.Decimal
                            );

                        parametroPrecioVenta.Precision = 12;
                        parametroPrecioVenta.Scale = 2;
                        parametroPrecioVenta.Value = precioVenta;

                        SqlParameter parametroKilometraje =
                            comando.Parameters.Add(
                                "@Kilometraje",
                                SqlDbType.Decimal
                            );

                        parametroKilometraje.Precision = 12;
                        parametroKilometraje.Scale = 2;
                        parametroKilometraje.Value = kilometraje;

                        comando.Parameters.Add(
                            "@Color",
                            SqlDbType.NVarChar,
                            40
                        ).Value = color;

                        comando.Parameters.Add(
                            "@Detalles",
                            SqlDbType.NVarChar,
                            1000
                        ).Value = string.IsNullOrWhiteSpace(detalles)
                            ? (object)DBNull.Value
                            : detalles;

                        conexion.Open();

                        object resultado = comando.ExecuteScalar();

                        int idPublicacion =
                            Convert.ToInt32(resultado);

                        MessageBox.Show(
                            "Vehículo publicado correctamente.\n" +
                            "Número de publicación: " +
                            idPublicacion,
                            "Publicación completada",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }

                LimpiarCampos();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "No se pudo publicar el vehículo.\n\n" +
                    ex.Message,
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error inesperado.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LimpiarCampos()
        {
            cbTipo.SelectedIndex = -1;
            cbMarca.SelectedIndex = -1;
            txtModelo.Clear();
            cbAño.SelectedIndex = -1;
            txtPrecioOriginal.Clear();
            txtPrecioVenta.Clear();
            txtKilometro.Clear();
            txtColor.Clear();
            txtDetalles.Clear();
        }
    }
}
