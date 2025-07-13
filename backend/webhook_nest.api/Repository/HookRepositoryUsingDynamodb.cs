using Amazon.DynamoDBv2.DataModel;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;

namespace webhook_nest.api.Repository;

public class HookRepositoryUsingDynamodb : IHook
{
    private readonly IDynamoDBContext _context;

    public HookRepositoryUsingDynamodb(IDynamoDBContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Payload payload)
    {
        
        await _context.SaveAsync(payload);
    }

    public async Task<Payload?> GetByIdAsync(string pk, string sk)
    {
        return await _context.LoadAsync<Payload>(pk, sk);
    }

    public async Task DeleteAsync(string pk, string sk)
    {
        await _context.DeleteAsync<Payload>(pk, sk);
    }
}