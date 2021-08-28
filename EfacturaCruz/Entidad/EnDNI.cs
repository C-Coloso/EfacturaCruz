using System;
using System.Runtime.Serialization;

namespace EfacturaCruz.Entidad
{
    public class EnDNI
    {
        

        [DataContract]
        public class DatoJNESolictud
        {
            [DataMember]
            public string CODDNI { get; set; }
        }
        public class DatoJNERespuesta
        {
            public string data { get; set; }
            public bool success { get; set; }
            public string mensaje { get; set; }

        }

        public class DatoExterno1Solicitud
        {
            public string _token { get; set; }
            public string dni { get; set; }
        }
    }
}
