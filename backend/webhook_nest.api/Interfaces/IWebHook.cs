using Newtonsoft.Json.Linq;
using webhook_nest.api.Models;

namespace webhook_nest.api.Interfaces;

public interface IWebHook
{
    Task<T?> GetByIdAsync<T>(string id);
    Task<(string id, string url)> Save();
    Task Update(string id, string method, Dictionary<string, string> headers, Dictionary<string, object>? payload);
    Task<List<T>> GetWebhookEvents<T>(string id);
}