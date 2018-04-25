using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Buscadores;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using YoutubeSearch;

namespace Weedapopbot.Dialogs
{
    [LuisModel("0f24c21a-8584-4bc0-ada6-b2db337355cb", "b22be5d78b32400794a401f0eb07ba79")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        String musicaABuscar;

        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, no le he entendido...");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Descargar")]
        public async Task ListarAsync(IDialogContext context, LuisResult result)
        {

            String tipo, titulo, artista, calidad;
            tipo = titulo = artista = calidad = "NaN";
            try
            {
                //Recorremos las entidades
                foreach (EntityRecommendation entidad in result.Entities)
                {
                    switch (entidad.Type)
                    {
                        case "Descarga::Titulo":
                            titulo = entidad.Entity;
                            break;
                        case "Descarga::Artista":
                            artista = entidad.Entity;
                            break;
                        case "Descarga::Tipo":
                            tipo = entidad.Entity;
                            break;
                        case "Descarga::Calidad":
                            calidad = entidad.Entity;
                            break;
                        default:
                            break;
                            
                    }
                }
            }
            catch { }
            titulo.Replace("/", "");
            artista.Replace("/", "");

            if (titulo != "NaN")
            {
                musicaABuscar = titulo;
                try { if (calidad == "NaN") calidad = "320"; } catch { calidad = "320";  } 
                ArrayList listaArgumentos = new ArrayList { context, result, calidad };               //Creamos una lista de argumentos para enviar al método
                BackgroundWorker bw_DMusica = new BackgroundWorker                                      //Creamos una tarea en segundo plano
                {
                    WorkerSupportsCancellation = true                                                   //Permitimos que se pueda cancelar con bw.CancelAsync()
                };
                bw_DMusica.DoWork += Bw_DMusica_IniciarTarea;                                           //Definimos cual es el método que iniciará la tarea
                bw_DMusica.RunWorkerAsync(listaArgumentos);
                }

            await context.PostAsync(String.Format("Titulo: {0}\n\nArtista: {1}\n\nCalidad: {2}\n\nTipo: {3}", titulo,artista,calidad,tipo));

            context.Wait(MessageReceived);
        }
        private void Bw_DMusica_IniciarTarea(object sender, DoWorkEventArgs e)
        {
            //Hacemos un "CAST" a los argumentos para indicar que es un ArrayList
            ArrayList listaArgumentos = (ArrayList)e.Argument;

            //Desmenuzamos la lista
            IDialogContext context = (IDialogContext)listaArgumentos[0];
            LuisResult luisResult = (LuisResult)listaArgumentos[1];
            String calidad = (String)listaArgumentos[2];

            //Enviamos un mensaje que será generado por el metodo encargado de lanzar las descargas y compresion
            context.PostAsync(MessageReceivedAsync_Musica(context, musicaABuscar, e, calidad));
        }

        private IMessageActivity MessageReceivedAsync_Musica(IDialogContext context, String busqueda, DoWorkEventArgs e, String calidad)
        {
            //Creamos un mensaje que se le enivará al usuario
            IMessageActivity mensajeDelAdjunto = context.MakeMessage();

            try
            {
                //Creamos el objeto Buscador encargado de buscar y descargar
                Buscador buscador = new Buscador(busqueda, 1, ref context);

                //Buscamos y cargamos el/los videos
                VideoInformation urlVideoEncontrado = buscador.BuscarVideos();

                //Obtenemos la ruta donde se ha descargado de manera temporal el mp3
                String rutaDelMP3 = buscador.DescargarVideo(urlVideoEncontrado.Url, calidad, true);

                //Creamos el adjunto con la ruta y la url del vídeo
                Attachment adjunto = Dialogos.AdjuntarAudio(rutaDelMP3, urlVideoEncontrado);

                //Ahora añadimos nuestros adjuntos al mensaje
                mensajeDelAdjunto.Attachments = new List<Attachment> { adjunto };

                ArrayList listaArgumentos = new ArrayList { adjunto };
                BackgroundWorker bw_DDelMusica = new BackgroundWorker                          //Creamos una tarea en segundo plano
                {
                    WorkerSupportsCancellation = true                                        //Permitimos que se pueda cancelar con bw.CancelAsync()
                };
                bw_DDelMusica.DoWork += bw_DDelMusica_IniciarTarea;                               //Definimos cual es el método que iniciará la tarea
                bw_DDelMusica.RunWorkerAsync(listaArgumentos);
                mensajeDelAdjunto.Text = Dialogos.msg_DescargaMensaje;                              //Mandamos iniciar la tarea mandandole nuestra lista de argumentos

                return mensajeDelAdjunto;
            }
            catch
            {
                //En caso de producirse un error en la descarga o conversión
                mensajeDelAdjunto.Text = Dialogos.msg_ErrorDescargaAudio;
            }

            return mensajeDelAdjunto;
        }


        private void bw_DDelMusica_IniciarTarea(object sender, DoWorkEventArgs e)
        {
            //Hacemos un "CAST" a los argumentos para indicar que es un ArrayList
            ArrayList listaArgumentos = (ArrayList)e.Argument;

            Attachment adjunto = (Attachment)listaArgumentos[0];
            Thread.Sleep(300000);
            String strFileDestination = (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"temp\" + adjunto.ContentUrl.Replace("https://weedmebot.azurewebsites.net/temp/", "")).Replace(" ", "");
            File.Delete(strFileDestination);
            
            //Eliminamos el archivo .mp4
            strFileDestination = strFileDestination.Replace(".mp3", "");
            File.Delete(strFileDestination);

        }
    }
}