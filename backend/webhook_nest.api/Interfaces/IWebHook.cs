using Newtonsoft.Json.Linq;
using webhook_nest.api.Models;

namespace webhook_nest.api.Interfaces;

public interface IWebHook
{
    Task<Payload?> GetByIdAsync(string id);
    Task<bool> Save();
    Task Update(string id, string method, Dictionary<string, string> headers, Dictionary<string, object>? payload);
}