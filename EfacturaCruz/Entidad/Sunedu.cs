using System;

namespace EfacturaCruz.Entidad
{
   public class Sunedu : IDisposable
   {
        private bool disposedValue;

        public string Graduado { get; set; }
        public string GradoTitulo { get; set; }
        public string Fecha_Diploma { get; set; }
        public string Modalidad_Estudios { get; set; }
        public string Institucion { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
