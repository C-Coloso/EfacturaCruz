using System;
using System.Collections.Generic;
using System.Text;

namespace EfacturaCruz
{
    class Operaciones
    {

    }
    public enum Operacion
    {
        EXITOSO,
        CANCELADO,
        ERROR,
        SUSPENDIDO,
        PERSONA_ENCONTRADA,
        RUC_NOEXISTE
    }
    public enum Moneda
    {
        DOLAR,
        EURO,
        YEN,
        YUAN
    }
    public enum Empresa
    {
        SUNAT,
        SBS,
        OCOÑA
    }

}
