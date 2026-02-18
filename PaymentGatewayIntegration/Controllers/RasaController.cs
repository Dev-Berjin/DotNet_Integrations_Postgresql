using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace PaymentGatewayIntegration.Controllers
{
    public class RasaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RasaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult RasaChat()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            var client = _httpClientFactory.CreateClient();

            var rasaBody = new
            {
                sender = "mvc_user",
                message = request.Message
            };

            var content = new StringContent(
                JsonSerializer.Serialize(rasaBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                "http://localhost:5005/webhooks/rest/webhook",
                content
            );

            var result = await response.Content.ReadAsStringAsync();

            var reply = ParseRasaResponse(result);

            return Json(new { reply });
        }

        private string ParseRasaResponse(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return "No response from bot.";

            using var doc = JsonDocument.Parse(json);

            StringBuilder sb = new StringBuilder();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("text", out var text))
                {
                    sb.AppendLine(text.GetString());
                }
            }

            return sb.ToString();
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
