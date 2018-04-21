using Google.YouTube;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.IO;
using YoutubeSearch;
using VideoLibrary;
using Weedapopbot.Dialogs;
using System.Diagnostics;
using System.ComponentModel;
//=============================================================
//Autores: Sergio Ur�a e Iyan Sanz
//Descripci�n: Esta clase est� dedicada a la descarga de los v�deos de youtube, as� como realizar las conversiones a mp3
//=============================================================
namespace Buscadores
{


    public class Buscador
    {
        
        public String textoABuscar;                     //Almacena el texto a buscar en youtube
        public int resultados;                          //Almacena el n�mero de resultados a buscar
        public YouTubeRequestSettings settings;         //Inicializamos las YouTubeSettings
        public YouTubeRequest request;                  //Inicializamos las YouTubeRequest
        public IDialogContext context;                  //Este es el contexto de la aplicaci�n
        public BackgroundWorker bw;                     //El trabajador de tareas en segundo plano

        //Constructor principal
        public Buscador(string textoABuscar, int resultados, ref IDialogContext context){
            //Establecemos la API de YouTube
            this.settings = new YouTubeRequestSettings("Tutankabot", "AIzaSyDO3IdHkAc7WMI8NCke7xM_MJfpFHrEg2Y");
            this.request = new YouTubeRequest(settings);
            
            //Declaramos las variables
            this.textoABuscar = textoABuscar;
            this.resultados = resultados;
            this.context = context;
        }

        //Este m�todo busca y devuelve la informaci�n del/os v�deo/s
        public VideoInformation BuscarVideos()
        {

            //El primero almacenar� la informaci�n de los resultados, el segundo la lista de resultados
            VideoInformation retorno = null;
            VideoSearch items = new VideoSearch();

            //Recorremos el m�todo SearchQuery, al cual le pasamos el texto a buscar y el n�mero de resultados
            foreach (VideoInformation item in items.SearchQuery(this.textoABuscar, this.resultados))
            {
                //Si los resultados son mayores que 0
                if (this.resultados > 0) { retorno = item; this.resultados--; }
                else break;
            }
           
            return retorno;
        }

        //Este m�todo descarga el v�deo en base a un link, le pasamos tambi�n el contexto para poder enviar los mensajes a la conversaci�n
        public string DescargarVideo(String link)
        {
            try
            {
                context.PostAsync(Dialogos.MensajeAleatorio(Dialogos.msg_Descargando));
                //Llamamos a la clase YouTube
                YouTube youTube = YouTube.Default;
                //Obtenemos el objeto del Video
                YouTubeVideo video = youTube.GetVideo(link);

                //Generamos la ruta de descarga (video.FullName incluye .mp4)
                String strFileDestination = (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"temp\" + video.FullName).Replace(" ", "");
                //Si el v�deo no existe en el directorio esribimos el archivo con los Bytes del video
                if (!File.Exists(strFileDestination))
                {
                    //Escribimos el archivo con los bytes del video
                    File.WriteAllBytes(strFileDestination, video.GetBytes());
                }

                //Si no existe el archivo .mp3
                if (!File.Exists($"{strFileDestination}.mp3"))
                {

                    //Variable con la ruta del ejecutable de ffmpeg.exe
                    String ffmpegExe = (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"bin\ffmpeg.exe");

                    //Este m�todo se encarga de la conversi�n a mp3
                    Execute(ffmpegExe, String.Format("-i {0} -f mp3 -ab 256000 -vn {1}", strFileDestination, strFileDestination + ".mp3"));

                    //Eliminamos el archivo .mp4
                    File.Delete(strFileDestination);

                }

                //Devolvemos la ruta del enlace
                return (video.FullName+ ".mp3").Replace(" ", "");
            }
            catch
            {
                //Salimos de la tarea en segundo plano
                bw.CancelAsync();
                bw.Dispose();

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