using System;
using System.Collections.Generic;

namespace CarMarket_Proyecto
{
    public class BusquedaVehiculos
    {
        public List<Publicacion> BuscarPublicaciones(
            List<Publicacion> publicaciones,
            string marca,
            string tipo,
            int? año,
            double? precioMaximo)
        {
            List<Publicacion> resultado = new List<Publicacion>();

            foreach (Publicacion pub in publicaciones)
            {
                if (!pub.GetDisponible())
                    continue;

                Vehiculo vehiculo = pub.GetVehiculo();

                bool cumple = true;

                if (!string.IsNullOrWhiteSpace(marca))
                {
                    if (vehiculo.GetMarca() != marca)
                        cumple = false;
                }

                if (!string.IsNullOrWhiteSpace(tipo))
                {
                    if (vehiculo.GetTipoCarro() != tipo)
                        cumple = false;
                }

                if (año.HasValue)
                {
                    if (vehiculo.GetAño() != año.Value)
                        cumple = false;
                }

                if (precioMaximo.HasValue)
                {
                    if (vehiculo.GetPrecioVenta() > precioMaximo.Value)
                        cumple = false;
                }

                if (cumple)
                    resultado.Add(pub);
            }

            return resultado;
        }

        public double? ConvertirPrecio(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            switch (texto)
            {
                case "1000 - 5000":
                    return 5000;

                case "5000 - 10000":
                    return 10000;

                case "10000 - 15000":
                    return 15000;

                case "15000 - 20000":
                    return 20000;

                default:
                    return null;
            }
        }
    }
}
