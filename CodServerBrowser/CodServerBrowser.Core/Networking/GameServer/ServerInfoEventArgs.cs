using System.Net;
using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Networking.GameServer
{
    public class ServerInfoEventArgs<TServer, TResponse> : EventArgs
         where TServer : IServerConnectionDetails
    {
        public required TResponse ServerInfo { get; init; }

        public required TServer Server { get; init; }
    }
}