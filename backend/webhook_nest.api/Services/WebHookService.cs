using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;
using Microsoft.Extensions.Logging;

namespace webhook_nest.api.Services;

public class WebHookService : IWebHook
{
    private readonly IHook _hook;
    private readonly string _baseUrl;
    private readonly ILogger<WebHookService> _logger;
    private const string Sk = "WEBHOOK";

    public WebHookService(IConfiguration config, IHook hook, ILogger<WebHookService> logger)
    {
        this._hook = hook;
        this._logger = logger;
        _baseUrl = config["URL"] ?? "http://localhost";
        _logger.LogInformation("WebHookService initialized with base URL: {BaseUrl}", _baseUrl);
    }

    public async Task<T> GetByIdAsync<T>(string id)
    {
        try
        {
            string pk = $"WEBHOOK#{id}";
            _logger.LogInformation("Getting webhook with pk: {Pk}, sk: {Sk}", pk, Sk);
            T result = await _hook.GetByIdAsync<T>(pk, Sk);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> Save()
    {
        try
        {
            string id = Guid.NewGuid().ToString();
            _logger.LogInformation("Creating new webhook with id: {Id}", id);

            var data = Payload.format($"WEBHOOK#{id}", Sk, $"{_baseUrl}/{id}", null, null,
                expiresAt: DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds());

            await _hook.SaveAsync(data);
            _logger.LogInformation("Successfully saved webhook with id: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving webhook");
            throw;
        }
    }

    public async Task Update(string id, string method, Dictionary<string, string> headers, Dictionary<string, object>? payload)
    {
        try
        {
            _logger.LogInformation("Updating webhook with id: {Id}, method: {Method}", id, method);
            _logger.LogDebug("Headers: {@Headers}", headers);
            _logger.LogDebug("Payload: {@Payload}", payload);

            // Create the payload data with proper null handling
            // For updates, we don't set a URL since we're updating an existing webhook
            var data = Payload.format(
                pk: $"WEBHOOK#{id}",
                sk: Sk,
                url: string.Empty, // Use empty string instead of null
                headers: headers,
                method: method,
                data: payload,
                expiresAt: null);

            _logger.LogInformation("Created payload object with pk: {Pk}, sk: {Sk}", data.Pk, data.Sk);

            await _hook.SaveAsync(data);
            _logger.LogInformation("Successfully updated webhook with id: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating webhook with id: {Id}", id);
            throw;
        }
    }
}