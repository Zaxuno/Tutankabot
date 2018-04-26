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
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {
                await Task.Factory.StartNew(() => Conversation.SendAsync(activity, () => new Dialogs.RootLuisDialog()));
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
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