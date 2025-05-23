﻿using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Networking.GameServer
{
    public interface IGameServerInfoService<TServer> where TServer : IServerConnectionDetails
    {
        Task<GameServerInfo?> GetInfoAsync(TServer server, CancellationToken cancellationToken);

        IAsyncEnumerable<(TServer server, GameServerInfo? info)> GetAllInfoAsync(
            IEnumerable<TServer> servers,
            int requestTimeoutInMs = 10000,
            CancellationToken cancellationToken = default);

        Task<IAsyncEnumerable<(TServer server, GameServerInfo? info)>> GetInfoAsync(
            IEnumerable<TServer> servers,
            bool sendSynchronously = false,
            int requestTimeoutInMs = 10000,
            CancellationToken cancellationToken = default);

        Task<Task> SendGetInfoAsync(
            IEnumerable<TServer> servers,
            Action<ServerInfoEventArgs<TServer, GameServerInfo>> onInfoResponse,
            int timeoutInMs = 10000,
            CancellationToken cancellationToken = default);
    }
}