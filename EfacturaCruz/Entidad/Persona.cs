using System;
using System.Collections.Generic;
using System.Text;

namespace EfacturaCruz.Entidad
{
    public class Persona : IDisposable
    {
        public string Nombres { get; set; }
        public string Apaterno { get; set; }
        public string Amaterno { get; set; }
        public string Dni { get; set; }
        public string Fecha_Nacimiento { get; set; }
        /*
         * [Edad] propiedad calculada de la Fecha Actual - FechaNacimiento
         */
        private string Edad { get; set; }
        public string Mensaje { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
