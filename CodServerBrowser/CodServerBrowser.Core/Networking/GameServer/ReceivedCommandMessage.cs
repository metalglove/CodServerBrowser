﻿using System.Net;
using CodServerBrowser.Core.Networking.GameServer;

namespace CodServerBrowser.Core.Networking.GameServer
{
    public readonly record struct ReceivedCommandMessage
    {
        public required CommandMessage Message { get; init; }

        public required string RawMessage { get; init; }

        public required DateTimeOffset Timestamp { get; init; }

        public required IPEndPoint RemoteEndPoint { get; init; }
    }
}