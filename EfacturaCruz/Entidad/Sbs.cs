
using System;

namespace EfacturaCruz.Entidad
{
    public class Sbs : IDisposable
    {
        public string FechaTipoDeCambio { get; set; }
        public string Moneda { get; set; }
        public Moneda moneda { get; set; }
        public string Compra { get; set; }
        public string Venta { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
    
}