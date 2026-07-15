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
            cbMarca.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAño.DropDownStyle = ComboBoxStyle.DropDownList;

            txtModelo.MaxLength = 80;
            txtPrecioOriginal.MaxLength = 13;
            txtPrecioVenta.MaxLength = 13;
            txtKilometro.MaxLength = 13;
            txtColor.MaxLength = 40;
            txtDetalles.MaxLength = 1000;

            txtPrecioOriginal.KeyPress += Validaciones.Decimal_KeyPress;
            txtPrecioVenta.KeyPress += Validaciones.Decimal_KeyPress;
            txtKilometro.KeyPress += Validaciones.Decimal_KeyPress;
            txtColor.KeyPress += Validaciones.SoloLetras_KeyPress;

            txtModelo.Validating += txtModelo_Validating;
            txtPrecioOriginal.Validating += txtPrecioOriginal_Validating;
            txtPrecioVenta.Validating += txtPrecioVenta_Validating;
            txtKilometro.Validating += txtKilometro_Validating;
            txtColor.Validating += txtColor_Validating;
            txtDetalles.Validating += txtDetalles_Validating;

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

            if (cbMarca.SelectedIndex < 0 ||
                cbTipo.SelectedIndex < 0 ||
                cbAño.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(modelo) ||
                string.IsNullOrWhiteSpace(color) ||
                string.IsNullOrWhiteSpace(detalles) ||
                string.IsNullOrWhiteSpace(txtPrecioOriginal.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioVenta.Text) ||
                string.IsNullOrWhiteSpace(txtKilometro.Text))
            {
                MessageBox.Show(
                    "Complete todos los campos obligatorios.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!Validaciones.EsModeloValido(modelo))
            {
                MessageBox.Show(
                    "El modelo contiene caracteres no permitidos.",
                    "Modelo incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtModelo.Focus();
                return;
            }

            if (!short.TryParse(cbAño.Text, out año) ||
                !Validaciones.EsAnioVehiculoValido(año))
            {
                MessageBox.Show(
                    "Seleccione un año válido.",
                    "Año incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                cbAño.Focus();
                return;
            }

            if (!Validaciones.TryDecimalPositivo(
                txtPrecioOriginal.Text,
                out precioOriginal))
            {
                MessageBox.Show(
                    "Ingrese un precio original válido y mayor que cero.",
                    "Precio incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPrecioOriginal.Focus();
                return;
            }

            if (!Validaciones.TryDecimalPositivo(
                txtPrecioVenta.Text,
                out precioVenta))
            {
                MessageBox.Show(
                    "Ingrese un precio de venta válido y mayor que cero.",
                    "Precio incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPrecioVenta.Focus();
                return;
            }

            if (!Validaciones.TryDecimalNoNegativo(
                txtKilometro.Text,
                out kilometraje))
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

            if (!Validaciones.EsColorValido(color))
            {
                MessageBox.Show(
                    "El color debe contener solamente letras.",
                    "Color incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtColor.Focus();
                return;
            }

            if (!Validaciones.EsDescripcionValida(detalles))
            {
                MessageBox.Show(
                    "La descripción debe tener entre 10 y 1000 caracteres.",
                    "Descripción incorrecta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtDetalles.Focus();
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
                    Validaciones.ObtenerMensajeSql(ex),
                    "No se pudo publicar el vehículo",
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
        private void txtModelo_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsModeloValido(txtModelo.Text))
            {
                MessageBox.Show("Modelo inválido.");
                e.Cancel = true;
            }
        }
        private void txtPrecioOriginal_Validating(object sender, CancelEventArgs e)
        {
            decimal precio;

            if (!Validaciones.TryDecimalPositivo(
                txtPrecioOriginal.Text,
                out precio))
            {
                MessageBox.Show("Precio original inválido.");
                e.Cancel = true;
            }
        }
        private void txtPrecioVenta_Validating(object sender, CancelEventArgs e)
        {
            decimal precio;

            if (!Validaciones.TryDecimalPositivo(
                txtPrecioVenta.Text,
                out precio))
            {
                MessageBox.Show("Precio de venta inválido.");
                e.Cancel = true;
            }
        }
        private void txtKilometro_Validating(object sender, CancelEventArgs e)
        {
            decimal km;

            if (!Validaciones.TryDecimalNoNegativo(
                txtKilometro.Text,
                out km))
            {
                MessageBox.Show("Kilometraje inválido.");
                e.Cancel = true;
            }
        }
        private void txtColor_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsColorValido(txtColor.Text))
            {
                MessageBox.Show("Color inválido.");
                e.Cancel = true;
            }
        }
        private void txtDetalles_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsDescripcionValida(txtDetalles.Text))
            {
                MessageBox.Show("La descripción debe tener entre 10 y 1000 caracteres.");
                e.Cancel = true;
            }
        }
    }
}
