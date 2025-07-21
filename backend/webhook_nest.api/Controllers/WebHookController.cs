using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

namespace webhook_nest.api.Controllers
{
    [Route("api/v1/webhook")]
    [ApiController]
    public class WebHookController : ControllerBase
    {
        private readonly IWebHook webhookService;
        private readonly ILogger<WebHookController> logger;
        private readonly IAmazonDynamoDB dynamoDbClient;

        public WebHookController(IWebHook webhookService, ILogger<WebHookController> logger, IAmazonDynamoDB dynamoDbClient)
        {
            this.webhookService = webhookService;
            this.logger = logger;
            this.dynamoDbClient = dynamoDbClient;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }




        [HttpGet("getwebhook/{id}")]
        public async Task<IActionResult> Get(string id)
        {
           // id = "b5bacb99-2fda-4414-b92c-aaffdff7c8ab";
            try
            {
                JObject result = await webhookService.GetByIdAsync<JObject>(id);
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting webhook with id: {Id}", id);
                return Problem(JsonConvert.SerializeObject(new { message  = "Error getting webhook with id: " + id, error = ex.Message }));
            }
        }

        [HttpPost("createwebhook")]
        public async Task<IActionResult> Create()
        {
            var result = await webhookService.Save();
            return result  == true  ? Ok(result) : Problem("Unable to create webhook link");
         
        }

        
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD")]
        [Route("updatewebhook/{id}")]
        public async Task<IActionResult> Update(string id)
        {
           
             

            try
            {
                var request = HttpContext.Request;
                var method = request.Method;


                var headers = request.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString());


                Dictionary<string, object>? payload = null;


                if (request.ContentLength > 0)
                {


                    // Enable buffering to allow multiple reads
                    request.EnableBuffering();

                    using var reader = new StreamReader(request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();

                    // Reset the position for other middleware
                    request.Body.Position = 0;

                    logger.LogDebug("Request body: {Body}", body);

                    if (!string.IsNullOrEmpty(body))
                    {

                        var jObject = JObject.Parse(body);
                        payload = jObject.ToObject<Dictionary<string, object>>();
                        logger.LogDebug("Successfully parsed JSON payload");

                    }
                }



                await webhookService.Update(id, method, headers, payload ?? new Dictionary<string, object>());


                return Ok(new { message = "Webhook updated successfully", id = id });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               return Problem(JsonConvert.SerializeObject(new { message = "Error updating webhook", error = e.Message }));
            }
            
          
        }
    }
}