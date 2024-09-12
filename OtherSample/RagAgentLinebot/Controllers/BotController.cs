using DotNetLineBotSdk.Helpers;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageEvent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RagAgentLinebot.Models;
using System.Net.Http;
using System.Text;

namespace RagAgentLinebot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private string _channel_Access_Token;
        private MultiRagAgent _multiRagAgent;

        public BotController(IConfiguration configuration, MultiRagAgent multiRagAgent)
        {
            _channel_Access_Token = configuration["LineChannel_Access_Token"];
            _multiRagAgent = multiRagAgent;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var replyEvent = new ReplyEvent(_channel_Access_Token);
            var req = this.HttpContext.Request;
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var lineReceMsg = ReceivedMessageConvert.ReceivedMessage(body);

            try
            {
                if (lineReceMsg != null && lineReceMsg.Events[0].Type == WebhookEventType.message.ToString())
                {
                    var chatId = lineReceMsg.Events[0].Source.UserId;
                    Console.WriteLine("chatId: " + chatId);
                    await SendLoadingAsync(chatId, 5);

                    var user_msg = lineReceMsg.Events[0].Message.Text;
                    var ans = await _multiRagAgent.ChatCompletionAgentAsync(user_msg);

                    //if (ans == string.Empty)
                    //{
                    //    ans = "法規知識庫目前無相關參考資料";
                    //}

                    if (lineReceMsg.Events[0].Message.Type == MessageType.text.ToString())
                    {
                        Console.WriteLine("User: " + user_msg);
                        Console.WriteLine("Bot: " + ans);
                        await replyEvent.ReplyAsync(lineReceMsg.Events[0].ReplyToken,
                                                   new List<IMessage>() { new TextMessage(ans) });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Ok();
            }
            return Ok();
        }

        public async Task SendLoadingAsync(string chatId, int seconds)
        {
            var url = "https://api.line.me/v2/bot/chat/loading/start";
            var payload = new
            {
                chatId = chatId,
                loadingSeconds = seconds
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_channel_Access_Token}");
            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
