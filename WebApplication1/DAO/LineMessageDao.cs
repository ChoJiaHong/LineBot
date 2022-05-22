using System.Collections.Generic;

namespace WebApplication1.DAO
{
    public class LineMessageDao
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        
        public class Root
        {
            public string destination { get; set; }
            public List<Event> events { get; set; }
        }
        public class DeliveryContext
        {
            public bool isRedelivery { get; set; }
        }

        public class Event
        {
            public WebhookEventType type { get; set; }
            public Message message { get; set; }
            public object timestamp { get; set; }
            public Source source { get; set; }
            public string replyToken { get; set; }
            public string mode { get; set; }
            public string webhookEventId { get; set; }
            public DeliveryContext deliveryContext { get; set; }
        }

        public class Message
        {
            public LineMessageType type { get; set; }
            public string id { get; set; }
            public string text { get; set; }
        }

        public class Source
        {
            public WebhookEventType type { get; set; }
            public string userId { get; set; }
        }
    }
    public enum WebhookEventType
    {
        Message,
        Follow,
        Unfollow,
    }
    public enum LineMessageType
    {
        text,
        sticker,
        image,
        video,
        audio,
        template,
        flex,
        location
    }
}