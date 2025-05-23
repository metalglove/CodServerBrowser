﻿using System.Net;
using CodServerBrowser.Core.Networking;
using CodServerBrowser.Core.IW4MAdmin;
using CodServerBrowser.Core.IW4MAdmin.Models;
using CodServerBrowser.Core.Models;
using CodServerBrowser.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CodServerBrowser.Core.Services
{
    public class H2MServersService(
        IServiceScopeFactory serviceScopeFactory,
        IErrorHandlingService errorHandlingService,
        IEndpointResolver endpointResolver,
        IMemoryCache memoryCache) : CachedMasterServerService(memoryCache, "H2M_SERVERS")
    {
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly IEndpointResolver _endpointResolver = endpointResolver;

        private readonly HashSet<ServerConnectionDetails> _servers = [];

        public override async Task<IReadOnlySet<ServerConnectionDetails>> FetchServersAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<IW4MServerInstance>? instances = null;

            IServiceScope scope = _serviceScopeFactory.CreateScope();
            try
            {
                IIW4MAdminMasterService iw4mAdminMasterService = scope.ServiceProvider.GetRequiredService<IIW4MAdminMasterService>();
                instances = await iw4mAdminMasterService.GetAllServerInstancesAsync(cancellationToken);

                if (instances is not null)
                {
                    IEnumerable<IW4MServer> filteredServers = instances
                        .SelectMany(instance => instance.Servers)
                        .Where(server => server.Game == "H2M");

                    IReadOnlyDictionary<IPEndPoint, IW4MServer> endpointMap = await _endpointResolver.CreateEndpointServerMap(
                        filteredServers, cancellationToken);

                    IEnumerable<ServerConnectionDetails> ipv4Servers = endpointMap.Keys.Where(key =>
                                key.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork ||
                                key.Address.IsIPv4MappedToIPv6)
                            .Select(ep => new ServerConnectionDetails(ep.Address.GetRealAddress().ToString(), ep.Port));

                    _servers.Clear();
                    foreach (ServerConnectionDetails server in ipv4Servers)
                    {
                        _servers.Add(server);
                    }

                    Cache.Set(CacheKey, _servers, TimeSpan.FromMinutes(5));
                }
            }
            catch (Exception ex)
            {
                _errorHandlingService.HandleException(ex, "Unable to fetch the servers details at this time. Please try again later.");
            }
            finally
            {
                scope.Dispose();
            }

            return _servers;
        }
    }
}
