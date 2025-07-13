using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webhook_nest.api.Interfaces;
using System.Text.Json;

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

            object? payload = null;

            if (request.Body.CanRead)
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        payload = JsonSerializer.Deserialize<object>(body);
                    }
                    catch
                    {
                        // If JSON parsing fails, use the raw string
                        payload = body;
                    }
                }
            }

            await webhookService.Update(id, method, headers, payload ?? new { });
            return Ok();
        }
    }
}