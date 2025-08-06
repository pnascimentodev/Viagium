using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Viagium.Services.Auth.Affiliate
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string token, DateTime expiresAt);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }

    public class InMemoryTokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

        public Task AddToBlacklistAsync(string token, DateTime expiresAt)
        {
            _blacklist[token] = expiresAt;
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklistedAsync(string token)
        {
            if (_blacklist.TryGetValue(token, out var expiresAt))
            {
                if (DateTime.UtcNow < expiresAt)
                    return Task.FromResult(true);
                _blacklist.TryRemove(token, out _);
            }
            return Task.FromResult(false);
        }
    }
}
