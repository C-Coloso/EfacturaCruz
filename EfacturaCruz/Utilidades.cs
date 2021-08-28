using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace EfacturaCruz
{
    public class Utilidades
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NroRuc"></param>
        /// <returns></returns>
        public static string SerializarAJson(Object objeto)
        {

            //Serializando Objetos  a JSON con la Clase DataContractJsonSerializer
            // Espacio de nombres o namespace System.runtime.Serialization.Json
            DataContractJsonSerializer js = new DataContractJsonSerializer(objeto.GetType());
            MemoryStream ms = new MemoryStream();
            js.WriteObject(ms, objeto);
            ms.Position = 0;

            StreamReader st = new StreamReader(ms);
            string ObParseado = st.ReadToEnd();
            // se cierra el streamReader
            st.Close();
            ms.Close();

            return ObParseado;
        }
        
        public static Object DeserializarJson(string JsonCadena, Object objeto)
        {
            DataContractJsonSerializer jsDeserializar = new DataContractJsonSerializer(objeto.GetType());
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(JsonCadena));
            Object obj = jsDeserializar.ReadObject(ms);
            //ms.Position = 0;
            ms.Close();

            return obj;
        }

        public static List<List<string>> ExtraerDataDeHtmlTable(string Xpath, string CadenaHtml)
        {
            //string xpath = "//table[@class='table table-striped table-scroll']";
            CadenaHtml = WebUtility.HtmlDecode(CadenaHtml);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(CadenaHtml);
            List<List<string>> Lista_Valores_Tabla_HTML = doc.DocumentNode.SelectSingleNode(Xpath)
               .Descendants("tr")
               .Skip(1)
               .Where(tr => tr.Elements("td").Count() > 1)
               .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
               .ToList();
            return Lista_Valores_Tabla_HTML;
        }
        public static string LeerTextoDeImagen(byte[] imagenBytes)
        {
            //string DataPath = "";
            string TextoResultado = null;
            float porcentaje = 0;
            int numIntentos = 1;
            try
            {
                while (porcentaje <= 0.50 && numIntentos <= 3)
                {
                    using (var engine = new TesseractEngine(@".\tessdata", "eng", EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromMemory(imagenBytes))
                        {

                            using (var page = engine.Process(img))
                            {

                                TextoResultado = page.GetText().Trim();
                                porcentaje = page.GetMeanConfidence();
                                numIntentos++;

                            }
                        }

                    }
                    if (porcentaje >= 0.50)
                    {
                        break;
                    }
                }
                return TextoResultado;

            }
            catch (Exception ex)
            {
                return null;

            }

        }
        public static async Task<string> SolicitudPost(HttpClient hCliente, string UrlPost, List<KeyValuePair<string, string>> lClaveValor)
        {
            using (var fcontenido = new FormUrlEncodedContent(lClaveValor))
            {
                HttpResponseMessage res = await hCliente.PostAsync(UrlPost, fcontenido);
                if (res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();


            }
            return null;

        }
    }
}
