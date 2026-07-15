using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarMarket_Proyecto
{
    public static class Validaciones
    {
        private const decimal ValorMaximoBD = 9999999999.99m;

        public static bool EsNombreValido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                nombre.Length < 2 ||
                nombre.Length > 100)
            {
                return false;
            }

            return Regex.IsMatch(
                nombre,
                @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$"
            );
        }

        public static bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo) ||
                correo.Length > 150)
            {
                return false;
            }

            return Regex.IsMatch(
                correo.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            );
        }

        public static bool EsTelefonoValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono) ||
                telefono.Length > 20)
            {
                return false;
            }

            bool caracteresValidos = telefono.All(c =>
                char.IsDigit(c) ||
                char.IsWhiteSpace(c) ||
                c == '-' ||
                c == '+' ||
                c == '(' ||
                c == ')'
            );

            int cantidadDigitos =
                telefono.Count(char.IsDigit);

            return caracteresValidos &&
                   cantidadDigitos >= 7 &&
                   cantidadDigitos <= 15;
        }

        public static bool EsContrasenaSegura(
            string contrasena,
            out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                mensaje = "La contraseña es obligatoria.";
                return false;
            }

            if (contrasena.Length < 8 ||
                contrasena.Length > 200)
            {
                mensaje =
                    "La contraseña debe tener entre 8 y 200 caracteres.";

                return false;
            }

            if (!contrasena.Any(char.IsUpper))
            {
                mensaje =
                    "La contraseña debe contener una letra mayúscula.";

                return false;
            }

            if (!contrasena.Any(char.IsLower))
            {
                mensaje =
                    "La contraseña debe contener una letra minúscula.";

                return false;
            }

            if (!contrasena.Any(char.IsDigit))
            {
                mensaje =
                    "La contraseña debe contener un número.";

                return false;
            }

            if (!contrasena.Any(c => !char.IsLetterOrDigit(c)))
            {
                mensaje =
                    "La contraseña debe contener un símbolo.";

                return false;
            }

            return true;
        }

        public static bool EsModeloValido(string modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo) ||
                modelo.Length > 80)
            {
                return false;
            }

            return Regex.IsMatch(
                modelo,
                @"^[\p{L}\p{N}][\p{L}\p{N}\s.\-/]*$"
            );
        }

        public static bool EsColorValido(string color)
        {
            if (string.IsNullOrWhiteSpace(color) ||
                color.Length > 40)
            {
                return false;
            }

            return Regex.IsMatch(
                color,
                @"^[\p{L}]+(?:[ \-][\p{L}]+)*$"
            );
        }

        public static bool EsDescripcionValida(string descripcion)
        {
            return !string.IsNullOrWhiteSpace(descripcion) &&
                   descripcion.Trim().Length >= 10 &&
                   descripcion.Trim().Length <= 1000;
        }

        public static bool EsAnioVehiculoValido(int anio)
        {
            return anio >= 1900 &&
                   anio <= DateTime.Now.Year + 1;
        }

        public static bool TryDecimalPositivo(
            string texto,
            out decimal valor)
        {
            return TryDecimal(texto, out valor) &&
                   valor > 0 &&
                   valor <= ValorMaximoBD;
        }

        public static bool TryDecimalNoNegativo(
            string texto,
            out decimal valor)
        {
            return TryDecimal(texto, out valor) &&
                   valor >= 0 &&
                   valor <= ValorMaximoBD;
        }

        private static bool TryDecimal(
            string texto,
            out decimal valor)
        {
            valor = 0;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            string limpio = texto.Trim();

            if (decimal.TryParse(
                limpio,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out valor))
            {
                return true;
            }

            limpio = limpio.Replace(',', '.');

            return decimal.TryParse(
                limpio,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out valor
            );
        }

        public static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            string normalizado = texto
                .Trim()
                .ToUpperInvariant()
                .Normalize(NormalizationForm.FormD);

            StringBuilder resultado = new StringBuilder();

            foreach (char caracter in normalizado)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(caracter);

                if (categoria !=
                    UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString()
                .Normalize(NormalizationForm.FormC);
        }

        public static string ObtenerMensajeSql(SqlException ex)
        {
            if (ex.Number == 50001 ||
                ex.Number == 2601 ||
                ex.Number == 2627)
            {
                return "Ya existe un usuario con ese correo.";
            }

            if (ex.Number == -2)
            {
                return "La conexión tardó demasiado. Intente nuevamente.";
            }

            if (ex.Number == 53 ||
                ex.Number == 40 ||
                ex.Number == 4060 ||
                ex.Number == 18456)
            {
                return "No se pudo conectar con la base de datos.";
            }

            if (ex.Number >= 50000)
            {
                return ex.Message;
            }

            return "Ocurrió un error al consultar la base de datos. " +
                   "Código: " + ex.Number;
        }

        public static void SoloLetras_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) &&
                        !char.IsLetter(e.KeyChar) &&
                        !char.IsWhiteSpace(e.KeyChar) &&
                        e.KeyChar != '-' &&
                        e.KeyChar != '\'';
        }

        public static void SoloNumeros_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) &&
                        !char.IsDigit(e.KeyChar);
        }

        public static void Telefono_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) &&
                        !char.IsDigit(e.KeyChar) &&
                        !char.IsWhiteSpace(e.KeyChar) &&
                        e.KeyChar != '-' &&
                        e.KeyChar != '+' &&
                        e.KeyChar != '(' &&
                        e.KeyChar != ')';
        }

        public static void Decimal_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) ||
                char.IsDigit(e.KeyChar))
            {
                return;
            }

            TextBox caja = sender as TextBox;

            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                caja != null &&
                !caja.Text.Contains(".") &&
                !caja.Text.Contains(","))
            {
                return;
            }

            e.Handled = true;
        }
    }
}
