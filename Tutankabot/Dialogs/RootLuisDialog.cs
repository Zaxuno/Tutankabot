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
using Weedapopbot.Intents;
using YoutubeSearch;

namespace Weedapopbot.Dialogs
{
    [LuisModel("0f24c21a-8584-4bc0-ada6-b2db337355cb", "b22be5d78b32400794a401f0eb07ba79")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {

        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, no le he entendido...");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Chiste")]
        public async Task ChisteAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Dialogos.MensajeAleatorio(Dialogos.lst_Chiste));
            context.Wait(MessageReceived);
        }

        [LuisIntent("Descargar")]
        public async Task DescargarAsync(IDialogContext context, LuisResult result)
        {
            Descarga descarga = new Descarga(result, context);
            descarga.EmpezarDescarga();

            await context.PostAsync(String.Format("Titulo: {0}\n\nArtista: {1}\n\nCalidad: {2}\n\nTipo: {3}\n\nVisualizar: {4}", descarga.Titulo, descarga.Artista, descarga.Calidad, descarga.Tipo, descarga.Visualizar));

            context.Wait(MessageReceived);
        }

    }
}