using System;

namespace EfacturaCruz.Entidad
{
    public class RepresentanteLegal :IDisposable
    {
        //public int id { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public string NombreRepresentante { get; set; }
        public string Cargo { get; set; }
        public string FechaDesde { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
           
        }
    }
}
