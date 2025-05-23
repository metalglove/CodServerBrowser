﻿using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Matchmaking.Models
{
    public record Playlist
    {
        private int _serverCount = 0;

        public required string Id { get; init; }

        public required string Name { get; init; }

        public string? Description { get; init; }

        public List<string> GameModes { get; init; } = [];

        public List<string> MapPacks { get; init; } = [];

        public List<ServerConnectionDetails>? Servers { get; init; } = [];

        public int ServerCount
        {
            get => Servers?.Count ?? _serverCount;
            init => _serverCount = ServerCount;
        }

        public int CurrentPlayerCount { get; init; }
    }
}
