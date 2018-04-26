using Buscadores;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Weedapopbot.Dialogs;
using YoutubeSearch;

namespace Weedapopbot.Intents
{
    public class Descarga
    {
        public IList<EntityRecommendation> Entidades;
        public String Titulo, Artista, Tipo, Calidad;
        public Boolean Visualizar;
        public int CalidadAudio, CalidadVideo;
        private IDialogContext context;

        private String[] TiposAudio = { "musica", "audio", "mp3", "cancion" };
        private String TipoAudio = "audio";
        private String[] TiposVideo = { "video","tutorial" };
        private String TipoVideo = "video";

        private String[] TiposVideoTutorial = { "tuto", "guia", "tutorial", "aprende" };
        private String TipoVideoTutorial = "tutorial";

        private String[] calidadesAudioAlta = { "alta", "full", "320", "buena" };
        private String calidadAudioAlta = "320";
        private String[] calidadesAudioMedia = { "media", "normal", "256"};
        private String calidadAudioMedia = "256";
        private String[] calidadesAudioBaja = { "baja", "128" };
        private String calidadAudioBaja = "128";

        private String[] calidadesVideoAlta = { "alta", "full hd", "1080"};
        private String calidadVideoAlta = "1080";
        private String[] calidadesVideoMedia = { "media", "hd", "720" };
        private String calidadVideoMedia = "720";
        private String[] calidadesVideoBaja = { "baja", "480" };
        private String calidadVideoBaja = "480";

        public Descarga(LuisResult result, IDialogContext context)
        {
            Entidades = result.Entities;
            this.context = context;
            ObtenerEntidades();
            ObtenerTipos();
            ObtenerCalidad();
        }

        public String ObtenerEntidad(String entidad)
        {
            entidad = "Descarga::" + entidad;
            foreach (EntityRecommendation entidadEncontrada in Entidades)
            {
                if (entidadEncontrada.Type.ToLower().Equals(entidad)) return entidadEncontrada.Entity;
            }
            return null;
        }

        private void ObtenerEntidades()
        {
            foreach (EntityRecommendation entidadEncontrada in Entidades)
            {
                switch (entidadEncontrada.Type)
                {
                    case "Descarga::Titulo":
                        Titulo = Dialogos.QuitarAcentos(entidadEncontrada.Entity);
                        break;
                    case "Descarga::Artista":
                        Artista = Dialogos.QuitarAcentos(entidadEncontrada.Entity);
                        break;
                    case "Descarga::Tipo":
                        Tipo = Dialogos.QuitarAcentos(entidadEncontrada.Entity);
                        break;
                    case "Descarga::Calidad":
                        Calidad = Dialogos.QuitarAcentos(entidadEncontrada.Entity);
                        break;
                    case "Descarga::Visualizar":
                        Visualizar = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ObtenerTipos()
        {
            if (TiposVideoTutorial.Contains(Tipo)) Tipo = TipoVideoTutorial;
            if (TiposVideo.Contains(Tipo) || Visualizar) Tipo = TipoVideo;
            else Tipo = TipoAudio;
        }

        private void ObtenerCalidad()
        {


            if (Tipo.Equals(TipoVideo))
            {
                try
                {
                    CalidadVideo = Convert.ToInt16(Regex.Replace(Calidad, @"[^\d]", ""));
                    Calidad = Regex.Replace(Calidad, @"[^\d]", "");
                }
                catch {}

                if (calidadesVideoMedia.Contains(Calidad)) Calidad = calidadVideoMedia;
                else if (calidadesVideoBaja.Contains(Calidad)) Calidad = calidadVideoBaja;
                else Calidad = calidadVideoAlta;
            }

            else
            {
                try
                {
                    CalidadAudio = Convert.ToInt16(Regex.Replace(Calidad, @"[^\d]", ""));
                    Calidad = Regex.Replace(Calidad, @"[^\d]", "");
                }
                catch {}

                if (calidadesAudioMedia.Contains(Calidad)) Calidad = calidadAudioMedia;
                else if (calidadesAudioBaja.Contains(Calidad)) Calidad = calidadAudioBaja;
                else Calidad = calidadAudioAlta;
            }
        }

        public void EmpezarDescarga()
        {
            if (esBuscable())
            {
                //ArrayList listaArgumentos = new ArrayList { context, result, calidad };
                BackgroundWorker bw_Descargar = new BackgroundWorker();
                bw_Descargar.DoWork += Bw_Descargar_IniciarTarea;
                bw_Descargar.RunWorkerAsync();
            }

        }

        private void Bw_Descargar_IniciarTarea(object sender, DoWorkEventArgs e)
        {
            IMessageActivity mensajeDelAdjunto = context.MakeMessage();
            try
            {
                //Preparamos la busqueda
                String busqueda = Titulo + " " + Artista;
                if (Tipo.Equals("tutorial")) busqueda = Tipo + " " + busqueda;

                //Creamos el objeto Buscador encargado de buscar y descargar
                Buscador buscador = new Buscador(context,busqueda,Convert.ToInt16(Calidad),Tipo);
                String ruta = buscador.linkVideoEncontrado;
    
                //Obtenemos la ruta donde se ha descargado de manera temporal el mp3
                if (!Visualizar)
                {
                    ruta = buscador.DescargarVideo();
                    Attachment adjunto;
                    //Creamos el adjunto con la ruta y la url del vídeo
                    if (Tipo.Equals("video")) adjunto = Dialogos.Adjuntar(ruta, buscador.tituloVideo, "video/mp4");
                    else adjunto = Dialogos.Adjuntar(ruta, buscador.tituloVideo, "audio/mp3");

                    //Ahora añadimos nuestros adjuntos al mensaje
                    mensajeDelAdjunto.Attachments = new List<Attachment> { adjunto };
                    mensajeDelAdjunto.Text = Dialogos.msg_DescargaMensaje;
                }
                else { mensajeDelAdjunto.Text = ruta; }

                

                ArrayList listaArgumentos = new ArrayList { buscador.rutaDestinoOriginal };
                BackgroundWorker bw_Eliminar_IniciarTarea = new BackgroundWorker();
                bw_Eliminar_IniciarTarea.DoWork += Bw_Eliminar_IniciarTarea;                               //Definimos cual es el método que iniciará la tarea
                bw_Eliminar_IniciarTarea.RunWorkerAsync(listaArgumentos);                             //Mandamos iniciar la tarea mandandole nuestra lista de argumentos
            }
            catch
            {
                //En caso de producirse un error en la descarga o conversión
                mensajeDelAdjunto.Text = Dialogos.msg_ErrorDescargaAudio;
            }

            context.PostAsync(mensajeDelAdjunto);
        }

        private void Bw_Eliminar_IniciarTarea(object sender, DoWorkEventArgs e)
        {
            //Hacemos un "CAST" a los argumentos para indicar que es un ArrayList
            ArrayList listaArgumentos = (ArrayList)e.Argument;

            String ruta = (String)listaArgumentos[0];
            Thread.Sleep(150000);
            
            if (Tipo.Equals("video")) File.Delete(ruta+".mp4");
            else File.Delete(ruta + ".mp3");
        }

        public Boolean esBuscable()
        {
            if (Titulo == "" && Artista == "") return false;
            return true;
        }

    }
}