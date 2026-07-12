using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMarket_Proyecto
{
    public class Usuario
    {
        private string nombre;
        private int edad;
        private string email;
        private string telefono;
        private string contraseña;

        public Usuario(string nombre, int edad, string email, string telefono, string contraseña)
        {
            this.nombre = nombre;
            this.edad = edad;
            this.email = email;
            this.telefono = telefono;
            this.contraseña = contraseña;
        }

        // Get
        public string GetNombre() { return nombre; }
        public int GetEdad() { return edad; }
        public string GetEmail() { return email; }
        public string GetTelefono() { return telefono; }
        public string GetContraseña() { return contraseña; }

        // Set
        public void SetNombre(string nombre) { this.nombre = nombre; }
        public void SetEdad(int edad) { this.edad = edad; }
        public void SetEmail(string email) { this.email = email; }
        public void SetTelefono(string telefono) { this.telefono = telefono; }
        public void SetContraseña(string contraseña) { this.contraseña = contraseña; }

        public bool ValidarCredenciales(string email, string contraseña)
        {
            return this.email == email && this.contraseña == contraseña;
        }
    }
}
