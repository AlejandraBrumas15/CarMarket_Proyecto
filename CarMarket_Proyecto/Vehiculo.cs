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
        private double precioOriginal;
        private double precioVenta;
        private double kilometraje;
        private string color;
        private string detalles;

        public Vehiculo(string marca, string modelo, int año, double precioOriginal, double precioVenta, double kilometraje, string color, string detalles)
        {
            this.marca = marca;
            this.modelo = modelo;
            this.año = año;
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
        public double GetPrecioOriginal() { return precioOriginal; }
        public double GetPrecioVenta() { return precioVenta; }
        public double GetKilometraje() { return kilometraje; }
        public string GetColor() { return color; }
        public string GetDetalles() { return detalles; }

        // Set
        public void SetMarca(string marca) { this.marca = marca; }
        public void SetModelo(string modelo) { this.modelo = modelo; }
        public void SetAño(int año) { this.año = año; }
        public void SetPrecioOriginal(double precioOriginal) { this.precioOriginal = precioOriginal; }
        public void SetPrecioVenta(double precioVenta) { this.precioVenta = precioVenta; }
        public void SetKilometraje(double kilometraje) { this.kilometraje = kilometraje; }
        public void SetColor(string color) { this.color = color; }
        public void SetDetalles(string detalles) { this.detalles = detalles; }

        // Método que calcula el precio de mercado según la devaluación por años
        public double CalcularPrecioDevaluado()
        {
            int añosTranscurridos = DateTime.Now.Year - año;
            double tasaDevaluacionAnual = 0.10; // 10% por año

            double precioDevaluado = precioOriginal * Math.Pow((1 - tasaDevaluacionAnual), añosTranscurridos);

            return precioDevaluado;
        }
    }
}
