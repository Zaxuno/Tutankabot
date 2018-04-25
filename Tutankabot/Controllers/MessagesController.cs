using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Weedapopbot.Dialogs;

namespace Weedapopbot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static bool chiste = false;
        public static bool descarga = false;
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {

                await Task.Factory.StartNew(() => Conversation.SendAsync(activity, () => new Dialogs.RootLuisDialog()));
                /*
                if (Dialogos.ComprobarMensaje(activity.Text, Dialogos.ord_Cuentame, Dialogos.ord_Chiste))
                {
                    if (!chiste)
                    {
                        await Task.Factory.StartNew(() => Conversation.SendAsync(activity, () => new Dialogs.RootDialog()));
                        chiste = true;
                    }
                }
                else if (Dialogos.ComprobarMensaje(activity.Text, Dialogos.ord_DescargaM, Dialogos.ord_Musica))
                {
                    if (!descarga)
                    {
                        await Task.Factory.StartNew(() => Conversation.SendAsync(activity, () => new Dialogs.RootDialog()));
                        descarga = true;
                    }
                }
                else
                {
                    await Task.Factory.StartNew(() => Conversation.SendAsync(activity, () => new Dialogs.RootDialog()));
                }


                await Task.Factory.StartNew(() => IniciarTemporizador(5));
                */

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private void IniciarTemporizador(int v)
        {
            Thread.Sleep(v * 1000);
            chiste = false;
            descarga = false;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {

            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}