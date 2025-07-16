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

        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                var environmentInfo = new
                {
                    region = Environment.GetEnvironmentVariable("REGION"),
                    stage = Environment.GetEnvironmentVariable("STAGE"),
                    tableName = Environment.GetEnvironmentVariable("TABLE_NAME"),
                    timestamp = DateTime.UtcNow
                };

                logger.LogInformation("Test endpoint called with environment: {@EnvironmentInfo}", environmentInfo);

                return Ok(new
                {
                    message = "Test endpoint working",
                    environment = environmentInfo
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, new { error = "Test endpoint error", message = ex.Message });
            }
        }

        [HttpGet("test-dynamodb")]
        public async Task<IActionResult> TestDynamoDB()
        {
            try
            {
                logger.LogInformation("Testing DynamoDB connection");

                // Test basic DynamoDB operations
                var listTablesRequest = new ListTablesRequest();
                var listTablesResponse = await dynamoDbClient.ListTablesAsync(listTablesRequest);

                var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "WebHooks";
                var tableExists = listTablesResponse.TableNames.Contains(tableName);

                var result = new
                {
                    message = "DynamoDB connection test",
                    region = Environment.GetEnvironmentVariable("REGION"),
                    tableName = tableName,
                    tableExists = tableExists,
                    availableTables = listTablesResponse.TableNames,
                    timestamp = DateTime.UtcNow
                };

                logger.LogInformation("DynamoDB test completed: {@Result}", result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing DynamoDB connection");
                return StatusCode(500, new { error = "DynamoDB test failed", message = ex.Message });
            }
        }

        [HttpGet("getwebhook/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            id = "b5bacb99-2fda-4414-b92c-aaffdff7c8ab";
            try
            {
                JObject result = await webhookService.GetByIdAsync<JObject>(id);
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting webhook with id: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("createwebhook")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var result = await webhookService.Save();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating webhook");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("updatewebhook/{id}")]
        public async Task<IActionResult> Update(string id)
        {
            try
            {
                logger.LogInformation("Update webhook request received for id: {Id}", id);

                var request = HttpContext.Request;
                var method = request.Method;
                logger.LogDebug("Request method: {Method}", method);

                var headers = request.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString());
                logger.LogDebug("Request headers: {@Headers}", headers);

                Dictionary<string, object>? payload = null;

                // Check if request has a body
                if (request.ContentLength > 0)
                {
                    logger.LogDebug("Request has body with length: {ContentLength}", request.ContentLength);

                    // Enable buffering to allow multiple reads
                    request.EnableBuffering();

                    using var reader = new StreamReader(request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();

                    // Reset the position for other middleware
                    request.Body.Position = 0;

                    logger.LogDebug("Request body: {Body}", body);

                    if (!string.IsNullOrEmpty(body))
                    {
                        try
                        {
                            var jObject = JObject.Parse(body);
                            payload = jObject.ToObject<Dictionary<string, object>>();
                            logger.LogDebug("Successfully parsed JSON payload");
                        }
                        catch (Exception jsonEx)
                        {
                            logger.LogWarning(jsonEx, "Failed to parse JSON body, using raw data");
                            // If JSON parsing fails, create a simple object with the raw string
                            payload = new Dictionary<string, object> { ["rawData"] = body };
                        }
                    }
                }
                else
                {
                    logger.LogDebug("Request has no body");
                }

                logger.LogInformation("Calling webhook service update with id: {Id}, method: {Method}", id, method);
                await webhookService.Update(id, method, headers, payload);

                logger.LogInformation("Successfully updated webhook with id: {Id}", id);
                return Ok(new { message = "Webhook updated successfully", id = id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating webhook with id: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}