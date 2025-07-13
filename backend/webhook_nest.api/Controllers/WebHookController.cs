using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webhook_nest.api.Interfaces;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace webhook_nest.api.Controllers
{
    [Route("api/v1/webhook")]
    [ApiController]
    public class WebHookController : ControllerBase
    {
        private IWebHook webhookService;

        public WebHookController(IWebHook webhookService)
        {
            this.webhookService = webhookService;
        }

        [HttpGet("getwebhook/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await webhookService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("createwebhook")]
        public async Task<IActionResult> Create()
        {
            var result = await webhookService.Save();
            return Ok(result);
        }

        [HttpPost("updatewebhook/{id}")]
        public async Task<IActionResult> Update(string id)
        {
            var request = HttpContext.Request;
            var method = request.Method;

            var headers = request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            JObject? payload = null;

            if (request.Body.CanRead)
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        payload = JObject.Parse(body);
                    }
                    catch
                    {
                        // If JSON parsing fails, create a simple object with the raw string
                        payload = new JObject { ["rawData"] = body };
                    }
                }
            }

            await webhookService.Update(id, method, headers, payload?.ToObject<Dictionary<string, object>>());
            return Ok();
        }
    }
}