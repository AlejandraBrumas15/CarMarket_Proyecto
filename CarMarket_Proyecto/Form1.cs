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
    public partial class Form1 : Form
    {
        private float anchoOriginal;
        private float altoOriginal;
        public Form1()
        {
            InitializeComponent();
            txtNombreI.MaxLength = 150;
            txtContraseñaI.MaxLength = 200;
            this.AcceptButton = btIniciar;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            anchoOriginal = this.ClientSize.Width;
            altoOriginal = this.ClientSize.Height;
        }

        private void Form1_Resize(object sender, EventArgs e)
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

        // acciones del boton iniciar y registrar

        private void btIniciar_Click(object sender, EventArgs e)
        {
            string email = txtNombreI.Text.Trim();
            string contraseña = txtContraseñaI.Text;

            if (string.IsNullOrWhiteSpace(email) ||
    string.IsNullOrWhiteSpace(contraseña))
            {
                MessageBox.Show(
                    "Ingrese el correo y la contraseña.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!Validaciones.EsCorreoValido(email))
            {
                MessageBox.Show(
                    "Ingrese un correo electrónico válido.",
                    "Correo incorrecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtNombreI.Focus();
                return;
            }

            if (contraseña.Length > 200)
            {
                MessageBox.Show(
                    "La contraseña supera la longitud permitida.",
                    "Contraseña incorrecta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtContraseñaI.Clear();
                txtContraseñaI.Focus();
                return;
            }

            try
            {
                using (SqlConnection conexion = ConexionBD.ObtenerConexion())
                {
                    using (SqlCommand comando =
                        new SqlCommand("dbo.sp_IniciarSesion", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(
                            "@Email",
                            SqlDbType.NVarChar,
                            150
                        ).Value = email;

                        comando.Parameters.Add(
                            "@Contrasena",
                            SqlDbType.NVarChar,
                            200
                        ).Value = contraseña;

                        conexion.Open();

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                int idUsuario =
                                    Convert.ToInt32(lector["IdUsuario"]);

                                string nombre =
                                    lector["Nombre"].ToString();

                                int edad =
                                    Convert.ToInt32(lector["Edad"]);

                                string correo =
                                    lector["Email"].ToString();

                                string telefono =
                                    lector["Telefono"].ToString();

                                DatosTemporales.IdUsuarioActual = idUsuario;

                                DatosTemporales.UsuarioActual = new Usuario(
                                    nombre,
                                    edad,
                                    correo,
                                    telefono,
                                    ""
                                );

                                MessageBox.Show(
                                    "Bienvenido, " + nombre + ".",
                                    "Inicio de sesión correcto",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );

                                PantallaInicio pantallaInicio =
                                    new PantallaInicio();

                                pantallaInicio.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "El correo o la contraseña son incorrectos.",
                                    "No se pudo iniciar sesión",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );

                                txtContraseñaI.Clear();
                                txtContraseñaI.Focus();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    Validaciones.ObtenerMensajeSql(ex),
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

        private void lkRegistrar_LinkClicked(
     object sender,
     LinkLabelLinkClickedEventArgs e)
        {
            Registro formRegistro = new Registro();
            formRegistro.Show();
            this.Hide();
        }
    }
}
