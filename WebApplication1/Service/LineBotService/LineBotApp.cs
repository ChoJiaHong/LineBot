using System.Collections.Generic;
using System.Threading.Tasks;
using Line.Messaging;
using Line.Messaging.Webhooks;

namespace WebApplication1.Service.LineBotService
{
    public class LineBotApp : WebhookApplication
    {
        private readonly LineMessagingClient _messagingClient;
        public LineBotApp(LineMessagingClient lineMessagingClient)
        {
            _messagingClient = lineMessagingClient;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            var result = null as List<ISendMessage>;

            switch (ev.Message)
            {
                //文字訊息
                case TextEventMessage textMessage:
                {
                    //頻道Id
                    var channelId = ev.Source.Id;
                    //使用者Id
                    var userId = ev.Source.UserId;
                    
                    //回傳 hellow
                    result = new List<ISendMessage>
                    {
                        new TextMessage("hellow")
                    };
                }
                    break;
            }

            if (result != null)
                await _messagingClient.ReplyMessageAsync(ev.ReplyToken, result);
        }
    }
}