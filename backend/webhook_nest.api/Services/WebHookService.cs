using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace webhook_nest.api.Services;

public class WebHookService : IWebHook
{
    private readonly IHook _hook;
    private readonly string _baseUrl;
    private readonly ILogger<WebHookService> _logger;
    private const string PreFix = "WEBHOOK";
    private static long ExpiryTime => DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

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
            string pk = $"{PreFix}#{id}";
            _logger.LogInformation("Getting webhook with pk: {Pk}, sk: {Sk}", pk, PreFix);
            T result = await _hook.GetByIdAsync<T>(pk, PreFix);

            // Remove pk and sk properties from the result
            if (result is null) return default(T);

            if (typeof(T) != typeof(object)) return default(T);

            var json = JsonConvert.SerializeObject(result);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (dictionary == null) throw new Exception("Error Deserialize Dictionary ");

            dictionary.Remove("pk");
            dictionary.Remove("sk");
            dictionary.Remove("expiresAt");
            result = (T)(object)dictionary;




            var response = new
            {
                id = id,
                url = dictionary["url"]?.ToString()
            };


            return (T)(object)response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook with id: {Id}", id);
            throw;
        }
    }


    public async Task<List<T>> GetWebhookEvents<T>(string id)
    {
        try
        {
            _logger.LogInformation("Getting webhook events for id: {Id}", id);
            List<T> result = await _hook.GetWebHooks<T>(id);


            if (result == null && !result.Any()) return new List<T>();

            var cleanedResults = new List<T>();
            foreach (var item in result)
            {

                if (typeof(T) != typeof(object)) throw new Exception("Error Deserialize Dictionary ");

                var serialize = JsonConvert.SerializeObject(item);
                var ConvertToDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialize);

                if (ConvertToDictionary == null) throw new Exception("Error converting to dictionary");

                ConvertToDictionary.Remove("pk");
                ConvertToDictionary.Remove("sk");
                ConvertToDictionary.Remove("GSI1PK");
                ConvertToDictionary.Remove("GSI1SK");
                ConvertToDictionary.Remove("expiresAt");

                _logger.LogInformation("Final cleaned dictionary: {Dict}", JsonConvert.SerializeObject(ConvertToDictionary));
                cleanedResults.Add((T)(object)ConvertToDictionary);


            }
            return cleanedResults;



        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook events for id: {Id}", id);
            throw;
        }
    }

    public async Task<(string id, string url)> Save()
    {

        string id = Guid.NewGuid().ToString();


        var data = Payload.format($"WEBHOOK#{id}", PreFix, $"{_baseUrl}/{id}", null, null,
            expiresAt: ExpiryTime,
            createdAt: DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        await _hook.SaveAsync(data);
        _logger.LogInformation("Successfully saved webhook with id: {data}", JsonConvert.SerializeObject(data));


        return (id, data.Url);


    }

    public async Task Update(string id, string method, Dictionary<string, string> headers, Dictionary<string, object>? payload)
    {

        _logger.LogInformation("Updating webhook with id: {Id}, method: {Method}", id, method);
        _logger.LogDebug("Headers: {@Headers}", headers);
        _logger.LogDebug("Payload: {@Payload}", payload);

        var data = Payload.format(
            pk: $"EVENT#{Guid.NewGuid()}",
            sk: $"WEBHOOK#{id}",
            url: string.Empty, // Use empty string instead of null
            headers: headers,
            method: method,
            data: payload,
            expiresAt: ExpiryTime,
            createdAt: DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        _logger.LogInformation("Created payload object with pk: {Pk}, sk: {Sk},  Data: {data}", data.Pk, data.Sk, JsonConvert.SerializeObject(data));

        await _hook.update(data);


    }
}