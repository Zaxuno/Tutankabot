using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Buscadores;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using YoutubeSearch;

namespace Weedapopbot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        private bool descargarMusica;
        private bool escribirMusica;
        private String musicaABuscar;

        public Task StartAsync(IDialogContext context)
        {
            //Llamamos a la tarea encargada de comprobar lo escrito
            context.Wait(SeleccionDeOpciones);
            return Task.CompletedTask;
        }

        private async Task SeleccionDeOpciones(IDialogContext context, IAwaitable<object> result)
        {
            //Creamos la actividad
            Activity activity = await result as Activity;

            //Si pregunta por un chiste
            if (Dialogos.ComprobarMensaje(activity.Text, Dialogos.ord_Cuentame, Dialogos.ord_Chiste))
            {
                await context.PostAsync(Dialogos.MensajeAleatorio(Dialogos.lst_Chiste));
            }
            //Si pregunta por una descarga de musica
            else if (Dialogos.ComprobarMensaje(activity.Text, Dialogos.ord_DescargaM, Dialogos.ord_Musica))
            {
                escribirMusica = true;
                await context.PostAsync(String.Format(Dialogos.msg_EscribeCancion));
            }
            else
            {
                if (escribirMusica)
                {
                    if (!descargarMusica)
                    {
                        descargarMusica = true;
                        musicaABuscar = activity.Text;
                        await context.PostAsync(Dialogos.MenuCalidadCancion(ref context));
                    }
                    else
                    {
                        try
                        {
                            String calidad = "320kbps";

                            if(Dialogos.ComprobarMensaje(ref calidad, Dialogos.ord_CalidadAudio))
                            {
                                ArrayList listaArgumentos = new ArrayList { context, activity, calidad };            //Creamos una lista de argumentos para enviar al método
                                BackgroundWorker bw_DMusica = new BackgroundWorker                          //Creamos una tarea en segundo plano
                                {
                                    WorkerSupportsCancellation = true                                        //Permitimos que se pueda cancelar con bw.CancelAsync()
                                };
                                bw_DMusica.DoWork += Bw_DMusica_IniciarTarea;                               //Definimos cual es el método que iniciará la tarea
                                bw_DMusica.RunWorkerAsync(listaArgumentos);                                 //Mandamos iniciar la tarea mandandole nuestra lista de argumentos
                            }



                        }
                        catch (Exception e)
                        {
                            await context.PostAsync(String.Format(Dialogos.msg_ErrorDescargaAudio));
                        }

                        descargarMusica = false;
                        escribirMusica = false;
                    }

                }
                else { await context.PostAsync(Dialogos.MenuInicial(ref context)); }
               
            }

            context.Wait(SeleccionDeOpciones);


        }

        private void Bw_DMusica_IniciarTarea(object sender, DoWorkEventArgs e)
        {

            //Hacemos un "CAST" a los argumentos para indicar que es un ArrayList
            ArrayList listaArgumentos = (ArrayList)e.Argument;

            //Desmenuzamos la lista
            IDialogContext context = (IDialogContext)listaArgumentos[0];
            Activity activity = (Activity)listaArgumentos[1];
            String calidad = (String)listaArgumentos[2];

            //Enviamos un mensaje que será generado por el metodo encargado de lanzar las descargas y compresion
            context.PostAsync(MessageReceivedAsync_Musica(context, musicaABuscar, activity.From.Id,e, calidad));
        }

        private IMessageActivity MessageReceivedAsync_Musica(IDialogContext context, String busqueda, String profile, DoWorkEventArgs e, String calidad)
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
                bw_DDelMusica.RunWorkerAsync(listaArgumentos);                                 //Mandamos iniciar la tarea mandandole nuestra lista de argumentos

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

        }
    }
}