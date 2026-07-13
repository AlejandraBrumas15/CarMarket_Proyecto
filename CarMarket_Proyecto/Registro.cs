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
            string email = txtEmailR.Text.Trim();
            string telefono = txtNumR.Text.Trim();
            string contraseña = txtContraseñaR.Text;

            int edad;

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(contraseña))
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

            if (contraseña != txtConfirmContraseña.Text)
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
                    ex.Message,
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
    }
}
