﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeSearch;

namespace Weedapopbot.Dialogs
{
    public class Dialogos
    {
        //Ruta donde se aloja (web)
        public static String rutaRoot = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

        //Mensajes
        public static String msg_Bienvenida = "Hola {0}, mi nombre es Tutankabot, estoy aquí para ayudarte, pero mis limitaciones ahora mismo se basan en contar chistes y descargar música.\n¡Pideme cualquiera de las dos!";
        public static String msg_EscribeCancion = "Dime el nombre de la canción junto con su artista.";
        public static String msg_CalidadCancion = "Selecciona la calidad de la canción";
        public static String msg_ErrorDescargaAudio = "Me he encontrado con un fallo y he tropezado.";
        public static String msg_DescargaMensaje = "En 5 minutos borraré el mp3 de mi memoria ¡Descárgalo!.";


        public static String[] msg_Descargando = File.ReadAllLines(rutaRoot + @"\textos\msg_Descargando.txt");

        //Ordenes
        public static String[] ord_Chiste = File.ReadAllLines(rutaRoot + @"\textos\ord_Chiste.txt");
        public static String[] ord_Cuentame = File.ReadAllLines(rutaRoot + @"\textos\ord_Cuentame.txt");
        public static String[] ord_DescargaM = File.ReadAllLines(rutaRoot + @"\textos\ord_DescargaM.txt");
        public static String[] ord_Musica = File.ReadAllLines(rutaRoot + @"\textos\ord_Musica.txt");
        public static String[] ord_CalidadAudio = {"128","192","256","320"};

        //Chistes
        public static String[] lst_Chiste = File.ReadAllLines(rutaRoot + @"\textos\lst_Chistes.txt");

        public static object Keys { get; private set; }

        //Generador de mensajes aleatorios
        public static String MensajeAleatorio(String[] mensajes)
        {
            Random random = new Random();
            int numero = random.Next(0, mensajes.Count());
            return mensajes[numero].Replace('\t', '\n');
        }

        public static String QuitarAcentos(String str)
        {
            str = str.ToLower();
            str.Replace('\t', '\n');
            str = str.Replace("á", "a");
            str = str.Replace("ä", "a");
            str = str.Replace("â", "a");
            str = str.Replace("à", "a");
            str = str.Replace("é", "e");
            str = str.Replace("ë", "e");
            str = str.Replace("ê", "e");
            str = str.Replace("è", "e");
            str = str.Replace("í", "i");
            str = str.Replace("ï", "i");
            str = str.Replace("î", "i");
            str = str.Replace("ì", "i");
            str = str.Replace("ó", "o");
            str = str.Replace("ö", "o");
            str = str.Replace("ô", "o");
            str = str.Replace("ò", "o");
            str = str.Replace("ú", "u");
            str = str.Replace("ü", "u");
            str = str.Replace("û", "u");
            str = str.Replace("ù", "u");
            return str;
        }

        //Comprueba si un mensaje contiene alguna orden
        public static Boolean ComprobarMensaje(String mensaje, String[] orden1)
        {
            mensaje = QuitarAcentos(mensaje);
            foreach (String str1 in orden1)
            {
                if (mensaje.Contains(str1)) return true;
            }

            return false;
        }

        //Comprueba si un mensaje contiene alguna orden
        public static Boolean ComprobarMensaje(ref String calidad, String[] orden1)
        {
            calidad = QuitarAcentos(calidad);
            foreach (String str1 in orden1)
            {
                if (calidad.Contains(str1)) { calidad = str1;  return true; }
            }

            return false;
        }

        //Comprueba si un mensaje contiene alguna orden seguida de otra
        public static Boolean ComprobarMensaje(String mensaje, String[] orden1, String[] orden2)
        {
            mensaje = QuitarAcentos(mensaje);
            foreach (String str1 in orden1)
            {
                foreach (String str2 in orden2)
                {
                    if (mensaje.Contains(str1) && mensaje.Contains(str2)) return true;
                }
            }

            return false;
        }

        //Adjunta un archivo de audio
        public static Attachment AdjuntarAudio(String ruta, VideoInformation video)
        {
            /*var audioCard = new AudioCard
            {
                Title = video.Title,
                Shareable = true,
                Subtitle = video.Author + video.Duration,
                Text = video.Description,
                Image = new ThumbnailUrl
                {
                    Url = video.Thumbnail
                },
                Media = new List<MediaUrl>
        {
            new MediaUrl()
            {
                Url = "https://weedmebot.azurewebsites.net/temp/"+ruta
            }
        },
                Buttons = new List<CardAction>
        {
            new CardAction()
            {
                Title = "Ver la versión en vídeo",
                Type = ActionTypes.OpenUrl,
                Value = video.Url
            }
        }
            };

            return audioCard.ToAttachment();*/



           Attachment attachment = new Attachment
            {
                ContentUrl = "https://weedmebot.azurewebsites.net/temp/" + ruta,
                ContentType = "audio/mp3",
                Content = video.Description,
                Name = NormalizarTexto(video.Title),
            };
            return attachment;
        }

        //Este método se encarga de normalizar los textos
        public static string NormalizarTexto(string texto)
        {
            return System.Web.HttpUtility.HtmlDecode(texto.Normalize());
        }

        //IMessageActivity (Mensaje) para el menu inicial
        public static IMessageActivity MenuInicial(ref IDialogContext context)
        {
            IMessageActivity mensaje = context.MakeMessage();
            mensaje.Type = "message";

            List<CardAction> botones = BotonesMenuInicial();

            HeroCard plCard = new HeroCard()
            {
                Title = "¡Selecciona una opción!",
                Buttons = botones,
            };
     
            Attachment plAttachment = plCard.ToAttachment();
            mensaje.Attachments.Add(plAttachment);

            //Layout tipo lista
            mensaje.AttachmentLayout = "list";
            return mensaje;

        }

        public static IMessageActivity MenuCalidadCancion(ref IDialogContext context)
        {
            IMessageActivity mensaje = context.MakeMessage();
            mensaje.Type = "message";

            List<CardAction> botones = BotonesMenuCalidadCancion();

            HeroCard plCard = new HeroCard()
            {
                Title = "Selecciona la calidad del audio",
                Buttons = botones,
            };

            Attachment plAttachment = plCard.ToAttachment();
            mensaje.Attachments.Add(plAttachment);

            //Layout tipo lista
            mensaje.AttachmentLayout = "list";
            return mensaje;

        }

        private static List<CardAction> BotonesMenuInicial()

        {
            List<CardAction> botones = new List<CardAction>();

            CardAction btnChiste = new CardAction()
            {
                Type = "imBack",
                Title = "Chiste",
                Value = "Cuéntame un chiste"
            };

            CardAction btnDescargar = new CardAction()
            {
                Type = "imBack",
                Title = "Bajar Música",
                Value = "Descargar música"
            };

            botones.Add(btnChiste);
            botones.Add(btnDescargar);

            return botones;
        }

        private static List<CardAction> BotonesMenuCalidadCancion()

        {
            List<CardAction> botones = new List<CardAction>();

            CardAction btn256 = new CardAction()
            {
                Type = "imBack",
                Title = "256kbps",
                Value = "256kbps"
            };

            CardAction btn320 = new CardAction()
            {
                Type = "imBack",
                Title = "320kbps",
                Value = "320kbps"
            };
            
            botones.Add(btn256);
            botones.Add(btn320);

            return botones;
        }


    }
}