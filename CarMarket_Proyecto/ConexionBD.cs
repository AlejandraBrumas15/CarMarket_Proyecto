using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CarMarket_Proyecto
{
    public static class ConexionBD
    {
        public static SqlConnection ObtenerConexion()
        {
            string cadena = Environment.GetEnvironmentVariable(
                "CARMARKET_DB_CONNECTION"
            );

            if (string.IsNullOrWhiteSpace(cadena))
            {
                cadena = ConfigurationManager
                    .ConnectionStrings["CarMarketDB"]
                    .ConnectionString;
            }

            return new SqlConnection(cadena);
        }

        public static bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();
                    mensaje = "Conexión exitosa con CarMarketDB.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = "No se pudo conectar con la base de datos.\n\n"
                        + ex.Message;

                return false;
            }
        }
    }
}