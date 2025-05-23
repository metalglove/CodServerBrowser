﻿using CodServerBrowser.Core.Matchmaking.Models;

namespace CodServerBrowser.Core.Services
{
    /// <summary>
    /// Service that provides methods to get playlists.
    /// </summary>
    public interface IPlaylistService
    {
        Task<Playlist?> GetDefaultPlaylist(CancellationToken cancellationToken);
        Task<Playlist?> GetPlaylistById(string id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Playlist>?> GetPlaylists(CancellationToken cancellationToken);
    }
}