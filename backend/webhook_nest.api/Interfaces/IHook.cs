using webhook_nest.api.Models;

namespace webhook_nest.api.Interfaces;

public interface IHook
{
    Task SaveAsync(Payload payload);


    Task update(Payload payload);

    Task<T?> GetByIdAsync<T>(string pk, string sk);

    Task<List<T>> GetWebHooks<T>(string pk);

    Task DeleteAsync(string pk, string sk);
}