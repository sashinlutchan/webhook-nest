using System.Runtime.CompilerServices;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;

namespace webhook_nest.api.Services;

public class WebHookService : IWebHook
{
    private IHook _hook;
    private readonly string _baseUrl;
    private const string Sk = "WEBHOOK";

    public WebHookService(IConfiguration config, IHook hook)
    {
        this._hook = hook;
        _baseUrl = config["URL"] ?? "http://localhost";
    }

    public async Task<Payload?> GetByIdAsync(string id)
    {
        string pk = $"WEBHOOK#{id}";
        return await _hook.GetByIdAsync(pk, Sk);
    }

    public async Task<bool> Save()
    {
        string id = Guid.NewGuid().ToString();
        var data = Payload.format($"WEBHOOK#{id}", Sk, $"{_baseUrl}/{id}", null, data: new { message = "Webhook created" },
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds());

        await _hook.SaveAsync(data);
        return true;
    }

    public async Task Update(string id, string method, Dictionary<string, string> headers, object payload)
    {
        var data = Payload.format($"WEBHOOK#{id}", Sk, null, headers, method, data: payload,
            expiresAt: null);

        await _hook.SaveAsync(data);
    }
}