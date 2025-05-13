using CodServerBrowser.Core.Models;
using CodServerBrowser.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CodServerBrowser.Core.Services
{
    public abstract class CachedMasterServerService(IMemoryCache memoryCache, string cacheKey) : IMasterServerService
    {
        protected IMemoryCache Cache { get; } = memoryCache;
        protected string CacheKey { get; } = cacheKey;

        public abstract Task<IReadOnlySet<ServerConnectionDetails>> FetchServersAsync(CancellationToken cancellationToken);

        public Task<IReadOnlySet<ServerConnectionDetails>> GetServersAsync(CancellationToken cancellationToken)
        {
            if (Cache.TryGetValue<IReadOnlySet<ServerConnectionDetails>>(CacheKey, out var cachedServers) && cachedServers is not null)
            {
                return Task.FromResult(cachedServers);
            }

            return FetchServersAsync(cancellationToken);
        }
    }
}
