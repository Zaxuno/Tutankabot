using Google.YouTube;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.IO;
using VideoLibrary;
using Weedapopbot.Dialogs;
using System.Diagnostics;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;

//=============================================================
//Autores: Sergio Ur�a e Iyan Sanz
//Descripci�n: Esta clase est� dedicada a la descarga de los v�deos de youtube, as� como realizar las conversiones a mp3
//=============================================================
namespace Buscadores
{


    public class Buscador
    {
        
        public String textoABuscar;                     //Almacena el texto a buscar en youtube
        public IDialogContext contexto;                 //Este es el contexto de la aplicaci�n
        public String linkVideoEncontrado;              //Almacena el link http:// del video de youtube
        public String tituloVideo;                      //Contiene el nombre (titulo) del video de youtube
        public Int16 calidad;                           //Contiene el numero de la calidad
        public String tipo;                             //Contiene el tipo de descarga
        public String rutaDestino;                      //Contiene la ruta con el t�tulo del v�deo y la calidad
        public String rutaDestinoOriginal;                      //Contiene la ruta original con el t�tulo del v�deo y la calidad
        public String ffmpegExe;                        //Ruta del conversor
        public YouTubeService youtube;                  //Inicializamos YouTube

        //Constructor al que hay que pasarle el contexto de la conversaci�n y el texto para buscar
        public Buscador(IDialogContext contexto, String busqueda,Int16 calidad, String tipo)
        {

            youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyDO3IdHkAc7WMI8NCke7xM_MJfpFHrEg2Y"
            });

            this.contexto = contexto;
            this.calidad = calidad;
            this.tipo = tipo;

            //Establecemos las variables
            textoABuscar = busqueda;

            //Devolvemos el resultado de la busqueda, en la posicion 0 se encuentra el link y en la posicion 1 el titulo
            String[] video = BuscarVideos();
            linkVideoEncontrado = video[0];
            tituloVideo = Dialogos.NormalizarTexto(video[1]);

            rutaDestino = rutaDestinoOriginal = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"temp\" + (tituloVideo + calidad).Replace(" ","");
            ffmpegExe = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"bin\ffmpeg.exe";
        }

        //Este m�todo busca y devuelve la informaci�n del video encontrado
        private String[] BuscarVideos()
        {
            String[] informacion = new String[2];

            SearchResource.ListRequest listRequest = youtube.Search.List("snippet");
            listRequest.Q = textoABuscar;

            SearchListResponse searchResponse = listRequest.Execute();

            List<String> videos = new List<String>();

            foreach (SearchResult searchResult in searchResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        informacion[0] = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                        informacion[1] = searchResult.Snippet.Title;
                        break;
                }
            }

            return informacion;



            /*VideoSearch items = new VideoSearch();
            VideoInformation informacion = new VideoInformation();
            int resultados = 0;
            int maxResultados = 1;

            //Recorremos el m�todo SearchQuery, al cual le pasamos el texto a buscar y el n�mero de p�ginas en las que buscar
            //El primero almacenar� la informaci�n de los resultados, el segundo la lista de resultados
            foreach (VideoInformation item in items.SearchQuery(textoABuscar, 1))
            {
                if (resultados < maxResultados) { informacion = item; resultados++; contexto.PostAsync(textoABuscar); } else { break; }
            }

            return informacion;*/
        }

        //Este m�todo descarga el v�deo en base a un link, le pasamos tambi�n el contexto para poder enviar los mensajes a la conversaci�n
        public string DescargarVideo()
        {
            try
            {

                //Enviamos un mensaje a la conversaci�n
                contexto.PostAsync(Dialogos.MensajeAleatorio(Dialogos.msg_Descargando));

                //Llamamos a la clase YouTube
                YouTube youTube = YouTube.Default;
                //Obtenemos el objeto del Video
                YouTubeVideo video = youTube.GetVideo(linkVideoEncontrado);
                
                //Si el v�deo no existe en el directorio esribimos el archivo con los Bytes del video
                if (!File.Exists(rutaDestino+".mp4"))
                {
                    //Escribimos el archivo con los bytes del video
                    File.WriteAllBytes(rutaDestino + ".mp4", video.GetBytes());
                }

                if (tipo.Equals("video") || tipo.Equals("tutorial"))
                {
                    rutaDestino = "https://weedmebot.azurewebsites.net/" + @"temp/" + (tituloVideo + calidad).Replace(" ", "");
                    return rutaDestino + ".mp4";
                }

                //Si el archivo de audio no existe
                if (!File.Exists(rutaDestino + ".mp3"))
                {
                    //Este m�todo se encarga de la conversi�n a mp3
                    Execute(ffmpegExe, String.Format("-i {0} -f mp3 -ab {1} -vn {2}", rutaDestino+".mp4", calidad, rutaDestino + ".mp3"));
                }

                //Reasignamos la ruta
                rutaDestino = "https://weedmebot.azurewebsites.net/" + @"temp/" + (tituloVideo + calidad).Replace(" ", "");

                //Si nos pide un pideo retornamos la url del v�deo
                return rutaDestino + ".mp3";
            }
            catch
            {
                //En caso de error devolvemos un mensaje
                return Dialogos.msg_ErrorDescargaAudio;
            }
        }


        //Este m�todo se encarga de ejecutar un proceso para la conversi�n del v�deo
        private string Execute(string exePath, string parameters)
        {
            string result = String.Empty;

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = parameters;

                p.Start();
                p.WaitForExit();

                result = p.StandardOutput.ReadToEnd();
            }

            return result;
        }

    }

}