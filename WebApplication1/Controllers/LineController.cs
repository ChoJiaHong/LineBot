using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using isRock.LineBot;
using Line.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.DAO;

using WebApplication1.DAO;
using WebApplication1.Service;

namespace WebApplication1.Controllers
{
    
    [Route("api/[controller]")]
    public class LineController : isRock.LineBot.LineWebHookControllerBase
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly LineBotConfig _lineBotConfig;

        private readonly IConfiguration _configuration;
        private readonly Plate _plate;
        private readonly IWebHostEnvironment _webHostEnvironment;


        private readonly string StaticFilePath ;
        // private readonly HttpContext _httpContext;

        public LineController(IMemoryCache memoryCache,
            LineBotConfig lineBotConfig,IConfiguration configuration
            ,Plate plate,IWebHostEnvironment webHostEnvironment)
        {
            //_httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            //_httpContext = _httpContextAccessor.HttpContext;
            _memoryCache = memoryCache;
            _lineBotConfig = lineBotConfig;
            _configuration = configuration;
            _plate = plate;
            _webHostEnvironment = webHostEnvironment;
            StaticFilePath = lineBotConfig.staticFilePath;
        }

        /*[HttpPost("run")] 
        public async Task<IActionResult> host()
        {
            try
            {
                var events =
                    WebhookRequestMessageHelper.GetWebhookEventsAsync(_httpContext.Request,
                        _lineBotConfig.channelSecret);
                var lineMessagingClient = new LineMessagingClient(_lineBotConfig.accessToken);
                var lineBotApp = new LineBotApp(lineMessagingClient);
                await lineBotApp.RunAsync(await events);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return Ok();
        }*/
        [HttpPost]
        public IActionResult Post()
        {
            var  _messageClient = new LineMessagingClient(_lineBotConfig.accessToken);
            try
            {
                this.ChannelAccessToken = _lineBotConfig.accessToken;
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                var msg = LineEvent.message.text;
                
                String number;
                CacheData _data;

                switch (LineEvent.message.type)
                {
                    case "text":
                        if (Regex.IsMatch(LineEvent.message.text, @"^[0-9]{3}$"))
                        {
                            var plate = LineEvent.message.text;
                            CacheData data;
                            if (!_memoryCache.TryGetValue(plate, out data))
                            {
                                msg = "資料已經過期";
                            }
                            else
                            {
                                if (data.type == "string")
                                {
                                    msg = data.value;
                                }
                                else if (data.type == "image")
                                {
                                    var pah = StaticFilePath + data.value;
                                    isRock.LineBot.ImageMessage imageMsg = new isRock.LineBot.ImageMessage(
                                        new Uri(StaticFilePath + data.value)
                                        , new Uri(StaticFilePath + data.value));
                                    this.ReplyMessage(LineEvent.replyToken, imageMsg);

                                    return Ok();
                                }
                                else 
                                {
                                    msg = StaticFilePath + data.value;
                                }
                            }
                        }
                        else
                        {
                            lock (_plate)
                            {
                                if (_memoryCache.TryGetValue(_plate.ToString(), out _data))
                                {
                                    msg = "儲存空間已滿，請稍等";
                                    break;
                                }

                                _plate.Number = _plate.Number + 1;
                                if (_plate.Number == 1000)
                                {
                                    _plate.Number = 0;
                                }

                                number = _plate.Number.ToString();
                            }

                            number = number.PadLeft(3, '0');
                            _memoryCache.Set(number.ToString(), new CacheData()
                            {
                                type = "string",
                                value = LineEvent.message.text
                            }, TimeSpan.FromSeconds(30));
                            msg = number;
                        }

                        break;
                    case "image":
                        lock (_plate)
                        {
                            if (_memoryCache.TryGetValue(_plate.ToString(), out _data))
                            {
                                msg = "儲存空間已滿，請稍等";
                                break;
                            }

                            _plate.Number = _plate.Number + 1;
                            if (_plate.Number == 1000)
                            {
                                _plate.Number = 0;
                            }

                            number = _plate.Number.ToString();
                        }

                        number = number.PadLeft(3, '0');
                        var byteArray = isRock.LineBot.Utility.GetUserUploadedContent(LineEvent.message.id
                            , _lineBotConfig.accessToken);
                        
                        var webRootPath = _webHostEnvironment.WebRootPath;
                        var dirPath = webRootPath + "/image/";
                        var path = $@"{dirPath}{number}.jpg";

                        using (var stream = new MemoryStream(byteArray))
                        {
                            
                            using (var fs = System.IO.File.Create(path))
                            {
                                stream.CopyTo(fs);
                            }
                        }
                        
                        _memoryCache.Set(number.ToString(), new CacheData()
                        {
                            type = "image",
                            fileContentType = "image/jpg",
                            value = "/image/"+number+".jpg",
                        }, TimeSpan.FromSeconds(300));

                        FileExtension.DeleteFile(path);
                        msg = number;
                        break;
                    
                }

                this.ReplyMessage(LineEvent.replyToken,msg );
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
                return Ok();
            }
        }
        
        
        
    }
}