﻿using System.Text.Json.Serialization;
using Flurl;

namespace CodServerBrowser.Core.Settings
{
    public sealed record MatchmakingSettings
    {
        public string MatchmakingServerUrl { get; init; } = "";

        [JsonIgnore]
        public string QueueingHubUrl => Url.Combine(MatchmakingServerUrl, "Queue");

        [JsonIgnore]
        public string PartyHubUrl => Url.Combine(MatchmakingServerUrl, "Party");

        [JsonIgnore]
        public string ServerDataUrl => Url.Combine(MatchmakingServerUrl, "servers/data");

        public bool UseRandomCliendId { get; init; } = false;
    }
}
