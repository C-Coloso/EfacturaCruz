using System;
using System.Collections.Generic;

namespace EfacturaCruz.Entidad
{
    public class EnSUNAT : IDisposable
    {
        public int TipoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        //Inicio de las propiedades del Contribuyente 
        public string NumeroRUC { get; set; }
        public string RazonSocial { get; set; }
        public string TipoContribuyente { get; set; }
        // Inicio Esta propiedad TipoDeDocumento solo aparece cuando se hace la consulta de un Ruc que comienza con 10
        public string TipoDeDocumento { get; set; }
        // Fin Estapropiedad TipoDeDocumento solo aparece cuando se hace la consulta de un Ruc que comienza con 10
        public string NombreComercial { get; set; }
        public string FechaInscripcion { get; set; }
        public string FechaInicioActividades { get; set; }
        public string EstadoContribuyente { get; set; }
        public string CondicionContribuyente { get; set; }
        public string DomicilioFiscal { get; set; }
        public string SistemaEmisionComprobante { get; set; }
        public string ActividadComercioExterior { get; set; }
        public string SistemaContabilidiad { get; set; }
        public List<string> ActividadesEconomicas { get; set; }
        public List<string> ComprobantesPagoAutImpresion{ get; set; }
        // Sistema de Emisión Electrónica
        public List<string> SistemasEmisionElectronica { get; set; }
        public string EmisorElectrónicoDesde { get; set; }
        // comprobante electronico sunat lo muestra en una sola cadena de texto
        public string ComprobantesElectronicos { get; set; }
        public string AfiliadoPLEDesde { get; set; }
        public List<string> Padrones { get; set; }
        //Fin de las propiedades del Contribuyente 


        // Propiedades agregados por Vladimir Cruz
        // Fecha: 19/07/2021 18:52 PM
        public string MensajeObserv { get; set; }
        public List<RepresentanteLegal> oRepresentanteLegal { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
