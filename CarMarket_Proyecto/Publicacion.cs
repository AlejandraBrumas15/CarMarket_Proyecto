using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMarket_Proyecto
{
    public class Publicacion
    {
        private Usuario vendedor;
        private Vehiculo vehiculo;
        private DateTime fechaPublicacion;
        private string descripcion;
        private bool disponible;

        public Publicacion(Usuario vendedor, Vehiculo vehiculo, DateTime fechaPublicacion, string descripcion, bool disponible)
        {
            this.vendedor = vendedor;
            this.vehiculo = vehiculo;
            this.fechaPublicacion = fechaPublicacion;
            this.descripcion = descripcion;
            this.disponible = disponible;
        }

        // Get
        public Usuario GetVendedor() { return vendedor; }
        public Vehiculo GetVehiculo() { return vehiculo; }
        public DateTime GetFechaPublicacion() { return fechaPublicacion; }
        public string GetDescripcion() { return descripcion; }
        public bool GetDisponible() { return disponible; }

        // Set
        public void SetVendedor(Usuario vendedor) { this.vendedor = vendedor; }
        public void SetVehiculo(Vehiculo vehiculo) { this.vehiculo = vehiculo; }
        public void SetDescripcion(string descripcion) { this.descripcion = descripcion; }
        public void SetDisponible(bool disponible) { this.disponible = disponible; }
    

    public void MarcarComoVendido()
        {
            disponible = false;
        }
    }
}

