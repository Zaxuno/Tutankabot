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
//Autores: Sergio Uría e Iyan Sanz
//Descripción: Esta clase está dedicada a la descarga de los vídeos de youtube, así como realizar las conversiones a mp3
//=============================================================
namespace Buscadores
{


    public class Buscador
    {
        
        public String textoABuscar;                     //Almacena el texto a buscar en youtube
        public int resultados;                          //Almacena el número de resultados a buscar
        public YouTubeRequestSettings settings;         //Inicializamos las YouTubeSettings
        public YouTubeRequest request;                  //Inicializamos las YouTubeRequest
        public IDialogContext context;                  //Este es el contexto de la aplicación
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

        //Este método busca y devuelve la información del/os vídeo/s
        public VideoInformation BuscarVideos()
        {

            //El primero almacenará la información de los resultados, el segundo la lista de resultados
            VideoInformation retorno = null;
            VideoSearch items = new VideoSearch();

            //Recorremos el método SearchQuery, al cual le pasamos el texto a buscar y el número de resultados
            foreach (VideoInformation item in items.SearchQuery(this.textoABuscar, this.resultados))
            {
                //Si los resultados son mayores que 0
                if (this.resultados > 0) { retorno = item; this.resultados--; }
                else break;
            }
           
            return retorno;
        }

        //Este método descarga el vídeo en base a un link, le pasamos también el contexto para poder enviar los mensajes a la conversación
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
                //Si el vídeo no existe en el directorio esribimos el archivo con los Bytes del video
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

                    //Este método se encarga de la conversión a mp3
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


        //Este método se encarga de ejecutar un proceso para la conversión del vídeo
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