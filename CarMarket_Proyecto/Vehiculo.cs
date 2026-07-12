using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMarket_Proyecto
{
    public class Vehiculo
    {
        private string marca;
        private string modelo;
        private int año;
        private string tipoCarro;
        private double precioOriginal;
        private double precioVenta;
        private double kilometraje;
        private string color;
        private string detalles;

        public Vehiculo(string marca, string modelo, int año, string tipoCarro, double precioOriginal, double precioVenta, double kilometraje, string color, string detalles)
        {
            this.marca = marca;
            this.modelo = modelo;
            this.año = año;
            this.tipoCarro = tipoCarro;
            this.precioOriginal = precioOriginal;
            this.precioVenta = precioVenta;
            this.kilometraje = kilometraje;
            this.color = color;
            this.detalles = detalles;
        }

        // Get
        public string GetMarca() { return marca; }
        public string GetModelo() { return modelo; }
        public int GetAño() { return año; }
        public string GetTipoCarro() { return tipoCarro; }
        public double GetPrecioOriginal() { return precioOriginal; }
        public double GetPrecioVenta() { return precioVenta; }
        public double GetKilometraje() { return kilometraje; }
        public string GetColor() { return color; }
        public string GetDetalles() { return detalles; }

        // Set
        public void SetMarca(string marca) { this.marca = marca; }
        public void SetModelo(string modelo) { this.modelo = modelo; }
        public void SetAño(int año) { this.año = año; }
        public void SetTipoCarro(string tipoCarro) { this.tipoCarro = tipoCarro; }
        public void SetPrecioOriginal(double precioOriginal) { this.precioOriginal = precioOriginal; }
        public void SetPrecioVenta(double precioVenta) { this.precioVenta = precioVenta; }
        public void SetKilometraje(double kilometraje) { this.kilometraje = kilometraje; }
        public void SetColor(string color) { this.color = color; }
        public void SetDetalles(string detalles) { this.detalles = detalles; }

        // Método que calcula el precio de mercado según la devaluación por años
        public double CalcularPrecioDevaluado()
        {

            int añosTranscurridos = DateTime.Now.Year - año;
            double precioActual = precioOriginal;

            for (int i = 1; i <= añosTranscurridos; i++)
            {
                double tasaDevaluacion;

                if (i == 1)
                {
                    tasaDevaluacion = 0.20; // primer año: 20%
                }
                else if (i >= 2 && i <= 5)
                {
                    tasaDevaluacion = 0.15; // años 2 al 5: 15% anual
                }
                else
                {
                    tasaDevaluacion = 0.10; // año 6 en adelante: 10% anual
                }

                precioActual = precioActual * (1 - tasaDevaluacion);
            }

            return precioActual;
        }

        // Determina si el precio pedido supera el 15% del precio de mercado
        public bool EsPosibleEstafa()
        {
            double precioMercado = CalcularPrecioDevaluado();
            double limitePermitido = precioMercado * 1.15;

            return precioVenta > limitePermitido;
        }

        // Devuelve un mensaje para mostrar en pantalla según el resultado de EsPosibleEstafa()
        public string ObtenerMensajeEstafa()
        {
            if (EsPosibleEstafa())
            {
                return "este precio es mucho mayor al valor real de mercado, podría tratarse de una estafa.";
            }
            else
            {
                return "Precio dentro del rango normal del mercado.";
            }

        }
    }
}
