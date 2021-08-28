using EfacturaCruz.Entidad;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using HtmlAgilityPack;
using System.Linq;
using Tesseract;
namespace EfacturaCruz
{
    public class ConsultasFe :IDisposable
    {
        #region Sunat
        private readonly string URL_SUNAT_CONSULTA = "https://e-consultaruc.sunat.gob.pe/cl-ti-itmrconsruc/jcrS00Alias";
        private readonly string URL_SUNAT_REPRESENTANTES_LEG = "https://e-consultaruc.sunat.gob.pe/cl-ti-itmrconsruc/jcrS00Alias";
        #endregion
        #region ReniecJne
        private readonly string URL_CODIGO_TOKEN = "https://aplicaciones007.jne.gob.pe/srop_publico/Consulta/Afiliado";
        private readonly string URL_DATOS_PERSONA = "https://aplicaciones007.jne.gob.pe/srop_publico/Consulta/api/AfiliadoApi/GetNombresCiudadano";
        #endregion
        #region RutasHtmlSunat
        // ruta relativa para seleccionar un nodo en particular
        private string XpathPanelPrincipal = "//div[@class='panel panel-primary']/div[2]";
        private string XpathRuc_RazonSocial = "//div[@class='col-sm-7']/h4[@class='list-group-item-heading']";
        private string XpathGlobal = "//div[@class='col-sm-7']/p[@class='list-group-item-text']";
        // xPath para Fecha de Inscripcion y Fecha de Inicio de Actividades
        private string XpathFechaInscripcion = "//div[@class='row']/div[2]/p";
        private string XpathInicioActividades = "//div[@class='row']/div[4]/p";
        // xPath para extraer el Html de Ssitema de Emision de Comprobante y Actividad de ComercioExterior
        private string XpathSistemaEmisionComprobante = "//div[@class='row']/div[2]/p";
        private string XpathComercioExterior = "//div[@class='row']/div[4]/p";
        // Xpath para Tablas
        private string XpathTable = "//table[@class='table tblResultado']";
        private string XpathTable_Represent_Legal = "//table[@class='table']";
        #endregion
        #region Sbs_TipoDeCambio
        private readonly string URL_GET_SBS = "https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioPromedio.aspx";
        private readonly string XPATH_TABLA_DATOS = "//table[@class='rgMasterTable']/tbody";
        private readonly string FECHA_TIPODECAMBIO = "//span[@id='ctl00_cphContent_lblFecha']";
        #endregion


        /// <summary>
        /// Método asíncrono que obtiene datos de personas de nacionalidad peruana 
        /// tales como (Nombres,Ap.Paterno,Ap.Materno)
        /// </summary>
        /// <param name="NroDni">Número de documento peruano que debe tener 8 digitos</param>
        /// <returns>Un objeto de tipo persona</returns>
        public async Task<Persona> ConsultarDniAsync(string NroDni)
        {
            Persona persona;
            int tipoRespuesta = 2;
            string numeroDNI = NroDni;
            CuTexto oCuTexto = new CuTexto();
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler controladorMensaje = new HttpClientHandler();
            controladorMensaje.CookieContainer = cookies;
            controladorMensaje.UseCookies = true;
            using (HttpClient cliente = new HttpClient(controladorMensaje))
            {
                cliente.DefaultRequestHeaders.Add("Host", "aplicaciones007.jne.gob.pe");
                cliente.DefaultRequestHeaders.Add("Referer", "https://www.google.com/");
                cliente.DefaultRequestHeaders.Add("sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"91\", \"Google Chrome\";v=\"91\"");
                cliente.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                cliente.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                cliente.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Safari/537.36");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                       SecurityProtocolType.Tls12;
                persona = new Persona();
                try
                {
                    using (HttpResponseMessage resultadoConsultaToken = await cliente.GetAsync(URL_CODIGO_TOKEN))
                    {
                        if (resultadoConsultaToken.IsSuccessStatusCode)
                        {
                            string contenidoHTML = await resultadoConsultaToken.Content.ReadAsStringAsync();
                            string nombreInicio = "pTokenCookie('";
                            string nombreFin = "'";
                            string pTokenCookie = oCuTexto.ExtraerContenidoEntreTagString(contenidoHTML, 0, nombreInicio, nombreFin);
                            nombreInicio = "pTokenForm('";
                            nombreFin = "'";
                            string pTokenForm = oCuTexto.ExtraerContenidoEntreTagString(contenidoHTML, 0, nombreInicio, nombreFin);
                            cliente.DefaultRequestHeaders.Remove("Referer");
                            cliente.DefaultRequestHeaders.Remove("Sec-Fetch-Dest");
                            cliente.DefaultRequestHeaders.Remove("Sec-Fetch-Mode");
                            cliente.DefaultRequestHeaders.Remove("Sec-Fetch-Site");

                            cliente.DefaultRequestHeaders.Add("Origin", "https://aplicaciones007.jne.gob.pe");
                            cliente.DefaultRequestHeaders.Add("Referer", "https://aplicaciones007.jne.gob.pe/srop_publico/Consulta/Afiliado");
                            cliente.DefaultRequestHeaders.Add("RequestVerificationToken", string.Format("{0}:{1}", pTokenCookie, pTokenForm));
                            cliente.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                            cliente.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                            cliente.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                            cliente.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                            EnDNI.DatoJNESolictud oEnDatoJNESolictud = new EnDNI.DatoJNESolictud();
                            oEnDatoJNESolictud.CODDNI = numeroDNI;
                            string Json = Utilidades.SerializarAJson(oEnDatoJNESolictud);
                            StringContent sc = new StringContent(Json, Encoding.UTF8, "application/json");

                            using (HttpResponseMessage resultadoConsultaDatos = await cliente.PostAsync(URL_DATOS_PERSONA, sc))
                            {
                                if (resultadoConsultaDatos.IsSuccessStatusCode)
                                {
                                    string JNERespuesta = await resultadoConsultaDatos.Content.ReadAsStringAsync();
                                    EnDNI.DatoJNERespuesta odatoJNERespuesta = new EnDNI.DatoJNERespuesta();
                                    odatoJNERespuesta = (EnDNI.DatoJNERespuesta)Utilidades.DeserializarJson(JNERespuesta, odatoJNERespuesta);
                                    string contenido = odatoJNERespuesta.data;
                                    persona.Mensaje = odatoJNERespuesta.mensaje;
                                    string[] arrContenido = contenido.Split('|');
                                    if (arrContenido[0] == "")
                                    {
                                        persona.Mensaje = persona.Mensaje == ""
                                            ? string.Format("No existe el número DNI {0}", numeroDNI)
                                            : string.Format("No existe el número DNI {0}\r\nDetalle: {1}", numeroDNI,
                                                persona.Mensaje);
                                    }
                                    else
                                    {
                                        persona.Apaterno = arrContenido[0];
                                        persona.Amaterno = arrContenido[1];
                                        persona.Nombres = arrContenido[2];
                                        persona.Dni = NroDni;
                                        tipoRespuesta = odatoJNERespuesta.success ? 1 : 2;
                                    }
                                }
                                else
                                {
                                    persona.Mensaje = await resultadoConsultaDatos.Content.ReadAsStringAsync();
                                    persona.Mensaje =
                                        string.Format(
                                            "Ocurrió un inconveniente al consultar los datos del DNI {0}.\r\nDetalle:{1}",
                                            numeroDNI, persona.Mensaje);
                                }
                            }
                        }
                        else
                        {
                            persona.Mensaje = await resultadoConsultaToken.Content.ReadAsStringAsync();
                            persona.Mensaje =
                                string.Format(
                                    "Ocurrió un inconveniente al consultar el número de DNI {0}.\r\nDetalle:{1}",
                                    numeroDNI, persona.Mensaje);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {

                    throw;
                }

            }
            return persona;

        }


        /// <summary>
        /// Método asíncrono que obtiene los datos de un contribuyente de la sunat
        /// sea persona natural(Nro de Ruc Empieza con 10) o persona jurídica(Nro de Ruc empieza con 20) 
        /// </summary>
        /// <param name="NroRuc">Número de ruc Sunat que debe tener 11 dígitos</param>
        /// <returns>Un objeto de tipo EnSunat que contiene los principales datos
        ///  de un contribuyente
        /// </returns>
        public async Task<EnSUNAT> ConsultarRucSunatAsync(string NroRuc)
        {  
            EnSUNAT oSunatDatos = new EnSUNAT();
            string ruc = NroRuc;
            CuTexto oCuTexto = new CuTexto();
            CookieContainer cookies = new CookieContainer();

            HttpClientHandler controladorMensaje = new HttpClientHandler();
            controladorMensaje.CookieContainer = cookies;
            controladorMensaje.UseCookies = true;
            using (HttpClient cliente = new HttpClient(controladorMensaje))
            {
                cliente.DefaultRequestHeaders.Add("Host", "e-consultaruc.sunat.gob.pe");
                cliente.DefaultRequestHeaders.Add("sec-ch-ua",
                    " \" Not A;Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
                cliente.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                cliente.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                cliente.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                cliente.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                       SecurityProtocolType.Tls12;
                await Task.Delay(100);
                using (HttpResponseMessage resultadoConsulta = await cliente.GetAsync(new Uri(URL_SUNAT_CONSULTA)))
                {

                    if (resultadoConsulta.IsSuccessStatusCode)
                    {
                        await Task.Delay(100);

                        cliente.DefaultRequestHeaders.Remove("Sec-Fetch-Site");
                        var rec = await resultadoConsulta.Content.ReadAsStringAsync();
                        cliente.DefaultRequestHeaders.Add("Origin", "https://e-consultaruc.sunat.gob.pe");
                        cliente.DefaultRequestHeaders.Add("Referer", URL_SUNAT_CONSULTA);
                        cliente.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");

                        string numeroDNI = "12345678"; // cualquier número DNI que exista en SUNAT.
                        // Pueden aprovechar este "bug" para consultar también mediante DNI a la SUNAT
                        var lClaveValor = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("accion", "consPorTipdoc"),
                            new KeyValuePair<string, string>("razSoc", ""),
                            new KeyValuePair<string, string>("nroRuc", ""),
                            new KeyValuePair<string, string>("nrodoc", numeroDNI),
                            new KeyValuePair<string, string>("contexto", "ti-it"),
                            new KeyValuePair<string, string>("modo", "1"),
                            new KeyValuePair<string, string>("search1", ""),
                            new KeyValuePair<string, string>("rbtnTipo", "2"),
                            new KeyValuePair<string, string>("tipdoc", "1"),
                            new KeyValuePair<string, string>("search2", numeroDNI),
                            new KeyValuePair<string, string>("search3", ""),
                            new KeyValuePair<string, string>("codigo", ""),
                        };
                        FormUrlEncodedContent contenido = new FormUrlEncodedContent(lClaveValor);
                        using (HttpResponseMessage resultadoConsultaRandom = await cliente.PostAsync(URL_SUNAT_CONSULTA, contenido))
                        {
                            if (resultadoConsultaRandom.IsSuccessStatusCode)
                            {

                                await Task.Delay(100);
                                string contenidoHTML = await resultadoConsultaRandom.Content.ReadAsStringAsync();
                                string numeroRandom = oCuTexto.ExtraerContenidoEntreTagString(contenidoHTML, 0, "name=\"numRnd\" value=\"", "\">");

                                lClaveValor = new List<KeyValuePair<string, string>>
                                {
                                    new KeyValuePair<string, string>("accion", "consPorRuc"),
                                    new KeyValuePair<string, string>("actReturn", "1"),
                                    new KeyValuePair<string, string>("nroRuc", ruc),
                                    new KeyValuePair<string, string>("numRnd", numeroRandom),
                                    new KeyValuePair<string, string>("modo", "1")
                                };
                                // Por si cae en el primer intento por el código "Unauthorized", en el buble se va a intentar hasta 3 veces "nConsulta"
                                int cConsulta = 0;
                                int nConsulta = 3;
                                HttpStatusCode codigoEstado = HttpStatusCode.Unauthorized;
                                while (cConsulta < nConsulta && codigoEstado == HttpStatusCode.Unauthorized)
                                
                                {
                                    contenido = new FormUrlEncodedContent(lClaveValor);
                                    using (HttpResponseMessage resultadoConsultaDatos = await cliente.PostAsync(URL_SUNAT_CONSULTA, contenido))
                                    {
                                        codigoEstado = resultadoConsultaDatos.StatusCode;
                                        if (resultadoConsultaDatos.IsSuccessStatusCode)
                                        {
                                            contenidoHTML = await resultadoConsultaDatos.Content.ReadAsStringAsync();
                                            //contenidoHTML = WebUtility.HtmlDecode(contenidoHTML);
                                            oSunatDatos = await ExtraerDatosRucSunat(contenidoHTML);


                                            //#region Obtener los datos del RUC   
                                            //oSunatDatos = ExtraerDatosRucSunat(contenidoHTML);

                                            //if (oSunatDatos.TipoRespuesta == 1)
                                            //{

                                            //    oSunatDatos.MensajeObserv =
                                            //        string.Format("Se realizó exitosamente la consulta del número de Ruc {0}",
                                            //            ruc);
                                            //}
                                            //else
                                            //{
                                            //    //aqui poner el mensaje de erro de consulta
                                            //    // No se pudo realizar la consulta del número de RUC xxxxxx

                                            //}
                                            //#endregion
                                        }

                                    }

                                    cConsulta++;
                                }

                            }
                        }
                    }

                }
            }
            return oSunatDatos;
        }

        
        
        public async Task<Sunedu> GetInfoSunedu(string Dni)
        {
            // Extraer el token de la pagina Utilando una peticion Get
            Sunedu osunedu = new Sunedu();
            string Url = "https://enlinea.sunedu.gob.pe/verificainscripcion";
            string UrlImgCaptcha = "https://enlinea.sunedu.gob.pe/simplecaptcha";
            //string CaptchaActualizar = "https://enlinea.sunedu.gob.pe/simplecaptcha?date=";

            string UrlConsultaPost = "https://enlinea.sunedu.gob.pe/consulta";
            byte[] bytesImagen;
            // XPath para html tags
            string Xpath_Token = "//input[@id='token']";
            string Xpath_Opcion = "//input[@id='opcion']";
            string pathFileImagen = "/fotoperu.png";
            try
            {
                HttpClient hcliente = new HttpClient();

                string ContentHtml = await hcliente.GetStringAsync(Url);
                // Obtenemos el Token 
                string _Token = GetTagHtml(Xpath_Token, ContentHtml);
                string _Opcion = GetTagHtml(Xpath_Opcion, ContentHtml);
                var captcha = await LeerCaptchaImagen(hcliente, UrlImgCaptcha);
                if (captcha.Length > 5)
                {
                    captcha = captcha.Substring(0, 5);
                }


                var lClaveValor = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("doc",Dni),
                        new KeyValuePair<string, string>("opcion",_Opcion),
                        new KeyValuePair<string, string>("_token",_Token),
                        new KeyValuePair<string, string>("icono",""),
                        new KeyValuePair<string, string>("captcha",captcha)
                    };
                hcliente.DefaultRequestHeaders.Add("Host", "enlinea.sunedu.gob.pe");
                hcliente.DefaultRequestHeaders.Add("Origin", "https://enlinea.sunedu.gob.pe");
                hcliente.DefaultRequestHeaders.Add("Referer", "https://enlinea.sunedu.gob.pe/verificainscripcion");
                var rp = await Utilidades.SolicitudPost(hcliente, UrlConsultaPost, lClaveValor);
                hcliente.Dispose();
                Console.WriteLine(rp);
                var rep = 45;   
         
                

            }
            catch (Exception ex)
            {
              


            }

            return osunedu;
        }
        private async Task<string> LeerCaptchaImagen(HttpClient hcliente, string UrlCaptcha)
        {
            //string DataPath = "";
            string TextoResultado = null;
            byte[] bytesImagen = null;
            float porcentaje = 0;
            int numIntentos = 1;

            
            //hcliente.DefaultRequestHeaders.Add("Sec-Fetch-Dest","image");

            try
            {
                while (porcentaje <= 0.70)
                {
                   HttpResponseMessage resp = await hcliente.GetAsync(UrlCaptcha);
                    if (resp.IsSuccessStatusCode)
                    {
                        bytesImagen = await resp.Content.ReadAsByteArrayAsync();
                    }
                   using (var engine = new TesseractEngine(@".\tessdata", "eng", EngineMode.Default))
                   {
                      using (var img = Pix.LoadFromMemory(bytesImagen))
                      {
                            using (var page = engine.Process(img))
                            {

                                TextoResultado = page.GetText().Trim();
                                porcentaje = page.GetMeanConfidence();
                                numIntentos++;

                            }


                      }

                   }
                    if (porcentaje >= 0.70)
                    {
                        break;
                    }
                    else
                    {
                        // Modificar Url original
                       //string auxUrl = UrlCaptcha+"?date=" + DateTime.Now.Millisecond;

                        UrlCaptcha = "https://enlinea.sunedu.gob.pe/simplecaptcha?date=" + DateTime.Now.Millisecond;
                    }

                }
                    

                
                
                return TextoResultado;

            }
            catch (Exception ex)
            {
                return null;

            }

        }
        private string GetTagHtml(string Xpath,string HtmlCadena)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlCadena);
            var resp = doc.DocumentNode.SelectSingleNode(Xpath);
            if (resp != null)
            {
                //string r = resp.GetAttributeValue("value", "XXX");
                return resp.Attributes["value"].Value.Trim();
            }
            return null;


        }

        private async Task<EnSUNAT> ExtraerDatosRucSunat(string contenidoHTML)
        {
            EnSUNAT oEnSunat = new EnSUNAT();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(contenidoHTML);
            try
            {
                var NodoDatos = doc.DocumentNode.SelectSingleNode(XpathPanelPrincipal).ChildNodes.Where(x => x.Name == "div").ToArray();
                for (int i = 0; i < NodoDatos.Length; i++)
                {
                    HtmlDocument hdoc = new HtmlDocument();
                    string html = NodoDatos[i].InnerHtml;
                    hdoc.LoadHtml(html);
                    // Con una condicional evaluamos si el total de filas de Nodos es igual a 16 que corresponderia
                    // a un ruc con persona juridica o total de filas 17 que corresponderia a una persona natural
                    // que el Ruc comienza con 10
                    if (NodoDatos.Length == 16)
                    {
                        switch (i)
                        {
                            case 0:
                                string auxHtml = hdoc.DocumentNode.SelectSingleNode(XpathRuc_RazonSocial).InnerText.Trim();
                                var resul = auxHtml.Split('-');
                                oEnSunat.NumeroRUC = resul[0].Trim();
                                oEnSunat.RazonSocial = resul[1].Trim();
                                break;
                            case 1:
                                oEnSunat.TipoContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 2:
                                oEnSunat.NombreComercial = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 3:
                                oEnSunat.FechaInscripcion = hdoc.DocumentNode.SelectSingleNode(XpathFechaInscripcion).InnerText.Trim();
                                oEnSunat.FechaInicioActividades = hdoc.DocumentNode.SelectSingleNode(XpathInicioActividades).InnerText.Trim();
                                break;
                            case 4:
                                oEnSunat.EstadoContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 5:
                                oEnSunat.CondicionContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 6:
                                oEnSunat.DomicilioFiscal = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 7:
                                oEnSunat.SistemaEmisionComprobante = hdoc.DocumentNode.SelectSingleNode(XpathSistemaEmisionComprobante).InnerText.Trim();
                                oEnSunat.ActividadComercioExterior = hdoc.DocumentNode.SelectSingleNode(XpathComercioExterior).InnerText.Trim();
                                break;
                            case 8:
                                oEnSunat.SistemaContabilidiad = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;

                            case 9:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.ActividadesEconomicas = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                else oEnSunat.ActividadesEconomicas = new List<string> { "-" };

                                break;

                            case 10:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.ComprobantesPagoAutImpresion = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                else oEnSunat.ComprobantesPagoAutImpresion = new List<string> { "No hay datos" };
                                break;

                            // posicion 11 = Sistema de Emision Electronica
                            case 11:
                                // Aca puede ocurrir 2 casos:
                                // 1. Si tiene datos el tag a buscar seria un <Table> y recorrer sus Elementos o filas <tr>
                                // 2. Si no tiene datos entonces solo se buscaria el tag <div class='col-sm-7'> y que no contenga nodos Hijos
                                string XpathEmisionElectronica = "//div[@class='col-sm-7']/table";
                                if (hdoc.DocumentNode.SelectSingleNode(XpathEmisionElectronica) != null)
                                {
                                    // si entra aca es or que hay un tag de tipo <table> y recorremos la tabla
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathEmisionElectronica).OuterHtml;
                                    oEnSunat.SistemasEmisionElectronica = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);

                                }
                                else
                                {
                                    string r = hdoc.DocumentNode.SelectSingleNode("//div[@class='col-sm-7']").InnerText.Trim();
                                    oEnSunat.SistemasEmisionElectronica = new List<string> { r };

                                }

                                break;
                            // posicion 12 = Emisor electronico desde
                            case 12:
                                oEnSunat.EmisorElectrónicoDesde = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            // Posicion 13 = Comprobantes Electronicos
                            case 13:
                                oEnSunat.ComprobantesElectronicos = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            //posicion 14 = Afiliado PLE
                            case 14:
                                oEnSunat.AfiliadoPLEDesde = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            // Posicion 15 =  Padrones Sunat Lista
                            case 15:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.Padrones = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                break;

                        }



                    }
                    else if (NodoDatos.Length == 17)
                    {
                        switch (i)
                        {
                            case 0:
                                string auxHtml = hdoc.DocumentNode.SelectSingleNode(XpathRuc_RazonSocial).InnerText.Trim();
                                var resul = auxHtml.Split('-');
                                oEnSunat.NumeroRUC = resul[0].Trim();
                                oEnSunat.RazonSocial = resul[1].Trim();
                                break;
                            case 1:
                                oEnSunat.TipoContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 2:
                                oEnSunat.TipoDeDocumento = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 3:
                                oEnSunat.NombreComercial = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 4:
                                oEnSunat.FechaInscripcion = hdoc.DocumentNode.SelectSingleNode(XpathFechaInscripcion).InnerText.Trim();
                                oEnSunat.FechaInicioActividades = hdoc.DocumentNode.SelectSingleNode(XpathInicioActividades).InnerText.Trim();
                                break;
                            case 5:
                                oEnSunat.EstadoContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 6:
                                oEnSunat.CondicionContribuyente = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 7:
                                oEnSunat.DomicilioFiscal = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            case 8:
                                oEnSunat.SistemaEmisionComprobante = hdoc.DocumentNode.SelectSingleNode(XpathSistemaEmisionComprobante).InnerText.Trim();
                                oEnSunat.ActividadComercioExterior = hdoc.DocumentNode.SelectSingleNode(XpathComercioExterior).InnerText.Trim();
                                break;
                            case 9:
                                oEnSunat.SistemaContabilidiad = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            // posicion 10 = Actividades Economicas
                            case 10:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.ActividadesEconomicas = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                else oEnSunat.ActividadesEconomicas = new List<string> { "-" };
                                break;
                            // posicion 11 = Comprobantes de Pago c/aut. de impresión (F. 806 u 816)
                            case 11:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.ComprobantesPagoAutImpresion = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                else oEnSunat.ComprobantesPagoAutImpresion = new List<string> { "No hay datos" };
                                break;
                            // posicion 12 = Sistema de Emision Electronica
                            case 12:
                                // Aca puede ocurrir 2 casos:
                                // 1. Si tiene datos el tag a buscar seria un <Table> y recorrer sus Elementos o filas <tr>
                                // 2. Si no tiene datos entonces solo se buscaria el tag <div class='col-sm-7'> y que no contenga nodos Hijos
                                string XpathEmisionElectronica = "//div[@class='col-sm-7']/table";
                                if (hdoc.DocumentNode.SelectSingleNode(XpathEmisionElectronica) != null)
                                {
                                    // si entra aca es or que hay un tag de tipo <table> y recorremos la tabla
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathEmisionElectronica).OuterHtml;
                                    oEnSunat.SistemasEmisionElectronica = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);

                                }
                                else
                                {
                                    string r = hdoc.DocumentNode.SelectSingleNode("//div[@class='col-sm-7']").InnerText.Trim();
                                    oEnSunat.SistemasEmisionElectronica = new List<string> { r };

                                }
                                //var aux2 = hdoc.DocumentNode.SelectSingleNode(XpathEmisionElectronica);

                                break;
                            // posicion 13 = Emisor electronico desde
                            case 13:
                                oEnSunat.EmisorElectrónicoDesde = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            // Posicion 14 = Comprobantes Electronicos
                            case 14:
                                oEnSunat.ComprobantesElectronicos = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            //posicion 15 = Afiliado PLE
                            case 15:
                                oEnSunat.AfiliadoPLEDesde = hdoc.DocumentNode.SelectSingleNode(XpathGlobal).InnerText.Trim();
                                break;
                            // Posicion 16 =  Padrones Sunat
                            case 16:
                                if (hdoc.DocumentNode.SelectSingleNode(XpathTable) != null)
                                {
                                    string Htmlaux = hdoc.DocumentNode.SelectSingleNode(XpathTable).OuterHtml;
                                    oEnSunat.Padrones = ExtraerDataDeHtmlTable(XpathTable, Htmlaux);
                                }
                                break;

                        }
                    }

                }


                /// DATOS DE REPRENTANTES LEGALES
                /// 
                // Si es un Ruc de persona Juridica(20xxxxxxxxx)se debera Extraer también informacion de sus
                // Representante(s) Legal(es)
                if (oEnSunat.NumeroRUC.Length == 11 && oEnSunat.NumeroRUC.StartsWith("20"))
                {
                    using (var hcliente = new HttpClient())
                    {
                        var lClaveValorAux = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("accion","getRepLeg"),
                            new KeyValuePair<string, string>("contexto","ti-it"),
                            new KeyValuePair<string, string>("modo","1"),
                            new KeyValuePair<string, string>("desRuc",oEnSunat.RazonSocial),
                            new KeyValuePair<string, string>("nroRuc",oEnSunat.NumeroRUC)
                        };
                        var respHttpPost = await hcliente.PostAsync(URL_SUNAT_REPRESENTANTES_LEG, new FormUrlEncodedContent(lClaveValorAux));
                        if (respHttpPost.IsSuccessStatusCode)
                        {
                            var CadenaHtml = await respHttpPost.Content.ReadAsStringAsync();
                            var Matriz_Tabla = ExtraerDatos_Tabla_Con_Thead(XpathTable_Represent_Legal, CadenaHtml);
                            List<RepresentanteLegal> rp = new List<RepresentanteLegal>();
                            foreach (var filas in Matriz_Tabla)
                            {
                                rp.Add(new RepresentanteLegal()
                                {
                                    TipoDocumento = filas[0].Trim(),
                                    NroDocumento = filas[1].Trim(),
                                    NombreRepresentante = filas[2].Trim(),
                                    Cargo = filas[3].Trim(),
                                    FechaDesde = filas[4].Trim()
                                });
                            }
                            oEnSunat.oRepresentanteLegal = rp;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                //string cadenaExpcetion = "Hola mundo esto es una excepcion";
                //Console.BackgroundColor = ConsoleColor.Green;
                //Console.WriteLine("Entro al Catch");
                //Console.ForegroundColor = ConsoleColor.Red;
                //System.Console.WriteLine(cadenaExpcetion);
                //Console.ReadKey();

                
            }
            
            

            return oEnSunat;
        }
      
        private async Task<string> SolicitudPost(string Url,List<KeyValuePair<string,string>> lClaveValor)
        {
            //Variables locales
            string ContenidoHtml = null; 
            //
            using (HttpClient hcliente = new HttpClient())
            {
                FormUrlEncodedContent ContenidoSolicitud = new FormUrlEncodedContent(lClaveValor);
                var resp = await hcliente.PostAsync(Url, ContenidoSolicitud);
                if (resp.IsSuccessStatusCode)
                {
                    ContenidoHtml = await resp.Content.ReadAsStringAsync();
                }
                return ContenidoHtml;
            }

        }
        

        public List<List<string>> ExtraerDatos_Tabla_Con_Thead(string Xpath,string ContenidoHtml)
        {

            // T objetoP;
            // T objetoP = Objeto;
            ContenidoHtml = WebUtility.HtmlDecode(ContenidoHtml); 
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(ContenidoHtml);
            var Lista_Valores_Tabla_HTML = doc.DocumentNode.SelectSingleNode(Xpath)
               .Descendants("tr")
               .Skip(1)
               //.Where(tr => tr.Elements("td").Count() > 1)
               .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
               .ToList();
            

             return Lista_Valores_Tabla_HTML;
            //List<T> rp2 = new List<T>();
            //foreach (var filas in Lista_Valores_Tabla_HTML)
            //{
            //    Type tipo = objetoP.GetType();
                
                
            //}

        }
        public List<List<string>> ExtraerDatos_Tabla_Sin_Thead(string Xpath, string ContenidoHtml)
        {

            ContenidoHtml = WebUtility.HtmlDecode(ContenidoHtml);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(ContenidoHtml);
            var Lista_Valores_Tabla_HTML = doc.DocumentNode.SelectSingleNode(Xpath)
               .Descendants("tr")
               //.Skip(1)
               //.Where(tr => tr.Elements("td").Count() > 1)
               .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
               .ToList();
            return Lista_Valores_Tabla_HTML;
        }


        private List<string> ExtraerDataDeHtmlTable(string Xpath, string ContenidoHtml)
        {
            List<string> ListaValorFilasHtml = new List<string>();
            ContenidoHtml = WebUtility.HtmlDecode(ContenidoHtml);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(ContenidoHtml);
            var Lista_Valores_Tabla_HTML = doc.DocumentNode.SelectSingleNode(Xpath)
               .Descendants("tr")
               //.Skip(1)
               //.Where(tr => tr.Elements("td").Count() > 1)
               .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
               .ToList();
            
            foreach (var matriz in Lista_Valores_Tabla_HTML)
            {
                foreach (var valor in matriz)
                {
                    ListaValorFilasHtml.Add(valor.Trim());
                }
            }


            return ListaValorFilasHtml;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        //var lClaveValor = new List<KeyValuePair<string, string>>
        //{
        //    new KeyValuePair<string, string>("doc",Dni),
        //    new KeyValuePair<string, string>("opcion",_Opcion),
        //    new KeyValuePair<string, string>("_token",_Token),
        //    new KeyValuePair<string, string>("icono",""),
        //    new KeyValuePair<string, string>("captcha",r)
        //};
        //// Hacer soliciud Post
        //var t = await SolicitudPost(UrlConsultaPost, lClaveValor);

    }
}
