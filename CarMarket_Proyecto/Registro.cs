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
    public partial class Registro : Form
    {
        private float anchoOriginal;
        private float altoOriginal;
        public Registro()
        {
            InitializeComponent();

            txtNombreR.MaxLength = 100;
            txtEdad.MaxLength = 3;
            txtEmailR.MaxLength = 150;
            txtNumR.MaxLength = 20;
            txtContraseñaR.MaxLength = 200;
            txtConfirmContraseña.MaxLength = 200;

            txtNombreR.KeyPress += Validaciones.SoloLetras_KeyPress;
            txtEdad.KeyPress += Validaciones.SoloNumeros_KeyPress;
            txtNumR.KeyPress += Validaciones.Telefono_KeyPress;

            this.AcceptButton = btRegistrar;

            txtNombreR.Validating += txtNombreR_Validating;
            txtEdad.Validating += txtEdad_Validating;
            txtEmailR.Validating += txtEmailR_Validating;
            txtNumR.Validating += txtNumR_Validating;
            txtContraseñaR.Validating += txtContraseñaR_Validating;
            txtConfirmContraseña.Validating += txtConfirmContraseña_Validating;
        }

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

        private void Registro_Load(object sender, EventArgs e)
        {
            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;
        }

        private void Registro_Resize(object sender, EventArgs e)
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

        private void btRegistrar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombreR.Text.Trim();
            string email = txtEmailR.Text.Trim().ToLower();
            string telefono = txtNumR.Text.Trim();
            string contraseña = txtContraseñaR.Text;
            string confirmacion = txtConfirmContraseña.Text;

            int edad;

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(txtEdad.Text) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(contraseña) ||
                string.IsNullOrWhiteSpace(confirmacion))
            {
                MessageBox.Show(
                    "Debe completar todos los campos.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!int.TryParse(txtEdad.Text, out edad))
            {
                MessageBox.Show(
                    "La edad debe ser un número entero.",
                    "Edad incorrecta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtEdad.Focus();
                return;
            }

            if (contraseña != confirmacion)
            {
                MessageBox.Show(
                    "Las contraseñas no coinciden.",
                    "Contraseña incorrecta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtConfirmContraseña.Clear();
                txtConfirmContraseña.Focus();
                return;
            }

            Usuario nuevoUsuario = new Usuario(
                nombre,
                edad,
                email,
                telefono,
                contraseña
            );

            string mensajeValidacion;

            if (!nuevoUsuario.ValidarDatosRegistro(
                out mensajeValidacion))
            {
                MessageBox.Show(
                    mensajeValidacion,
                    "Datos incorrectos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                using (SqlConnection conexion = ConexionBD.ObtenerConexion())
                {
                    using (SqlCommand comando =
                        new SqlCommand("dbo.sp_RegistrarUsuario", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(
                            "@Nombre",
                            SqlDbType.NVarChar,
                            100
                        ).Value = nombre;

                        comando.Parameters.Add(
                            "@Edad",
                            SqlDbType.Int
                        ).Value = edad;

                        comando.Parameters.Add(
                            "@Email",
                            SqlDbType.NVarChar,
                            150
                        ).Value = email;

                        comando.Parameters.Add(
                            "@Telefono",
                            SqlDbType.NVarChar,
                            20
                        ).Value = telefono;

                        comando.Parameters.Add(
                            "@Contrasena",
                            SqlDbType.NVarChar,
                            200
                        ).Value = contraseña;

                        conexion.Open();

                        int idUsuario =
                            Convert.ToInt32(comando.ExecuteScalar());

                        MessageBox.Show(
                            "Usuario registrado correctamente.\n" +
                            "Código de usuario: " + idUsuario,
                            "Registro completado",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }

                Form1 formLogin = new Form1();
                formLogin.Show();
                this.Hide();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    Validaciones.ObtenerMensajeSql(ex),
                    "No se pudo registrar el usuario",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error inesperado.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        // Validación del nombre
        private void txtNombreR_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsNombreValido(txtNombreR.Text))
            {
                MessageBox.Show("Ingrese un nombre válido.");
                e.Cancel = true;
            }
        }
        // Validación de la edad
        private void txtEdad_Validating(object sender, CancelEventArgs e)
        {
            int edad;

            if (!int.TryParse(txtEdad.Text, out edad))
            {
                MessageBox.Show("La edad debe ser numérica.");
                e.Cancel = true;
                return;
            }

            if (edad < 18 || edad > 120)
            {
                MessageBox.Show("Edad fuera del rango permitido.");
                e.Cancel = true;
            }
        }
        // Validación del email
        private void txtEmailR_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsCorreoValido(txtEmailR.Text))
            {
                MessageBox.Show("Correo electrónico inválido.");
                e.Cancel = true;
            }
        }
        // Validación de LA CONTRASEÑA  
        private void txtContraseñaR_Validating(object sender, CancelEventArgs e)
        {
            string mensaje;

            if (!Validaciones.EsContrasenaSegura(
                txtContraseñaR.Text,
                out mensaje))
            {
                MessageBox.Show(mensaje);
                e.Cancel = true;
            }
        }
        // Validación de la confirmación de la contraseña
        private void txtConfirmContraseña_Validating(object sender, CancelEventArgs e)
        {
            if (txtContraseñaR.Text != txtConfirmContraseña.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                e.Cancel = true;
            }
        }
        // Validación del número telefónico
        private void txtNumR_Validating(object sender, CancelEventArgs e)
        {
            if (!Validaciones.EsTelefonoValido(txtNumR.Text))
            {
                MessageBox.Show("Número telefónico inválido.");
                e.Cancel = true;
            }
        }
    }
}
