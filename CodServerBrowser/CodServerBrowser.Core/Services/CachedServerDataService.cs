﻿using System.Net.Http.Json;
using Flurl;
using CodServerBrowser.Core.Matchmaking.Models;
using CodServerBrowser.Core.Models;
using CodServerBrowser.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodServerBrowser.Core.Services
{
    public sealed class CachedServerDataService : IPlaylistService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CachedServerDataService> _logger;

        public CachedServerDataService(HttpClient httpClient, ILogger<CachedServerDataService> logger, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public Task<IReadOnlyList<ServerData>?> GetServerDataList(CancellationToken cancellationToken)
        {
            return _memoryCache.GetOrCreateAsync<IReadOnlyList<ServerData>>("ServerDataList", async (entry) =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);

                _logger.LogDebug("Fetching server data list...");
                ServerData[]? serverData = await _httpClient.GetFromJsonAsync<ServerData[]>("servers/data", cancellationToken);

                return serverData is null ? Array.Empty<ServerData>().AsReadOnly() : serverData.AsReadOnly();
            });
        }

        public Task<Playlist?> GetDefaultPlaylist(CancellationToken cancellationToken)
        {
            return _memoryCache.GetOrCreateAsync("DefaultPlaylist", (entry) =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10);

                _logger.LogDebug("Fetching default playlist...");
                return _httpClient.GetFromJsonAsync<Playlist>("playlists/default", cancellationToken);
            });
        }

        public Task<Playlist?> GetPlaylistById(string id, CancellationToken cancellationToken)
        {
            return _memoryCache.GetOrCreateAsync("Playlist_" + id, async (entry) =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2);

                _logger.LogDebug("Fetching playlist {playlistId}...", id);
                Playlist? playlist = await _httpClient.GetFromJsonAsync<Playlist?>(Url.Combine("playlists", id), cancellationToken);

                return playlist;
            });
        }

        public Task<IReadOnlyList<Playlist>?> GetPlaylists(CancellationToken cancellationToken)
        {
            return _memoryCache.GetOrCreateAsync<IReadOnlyList<Playlist>?>("Playlists", async (entry) =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2);

                _logger.LogDebug("Fetching all playlists...");
                List<Playlist>? playlists = await _httpClient.GetFromJsonAsync<List<Playlist>>("playlists", cancellationToken);

                if (playlists is not null)
                {
                    _logger.LogInformation("Fetched {numPlaylists} playlists", playlists.Count);
                }

                return playlists?.AsReadOnly();
            })!;
        }
    }
}
