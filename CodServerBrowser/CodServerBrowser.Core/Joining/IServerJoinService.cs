using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Joining;

public interface IServerJoinService
{
    ISimpleServerInfo? LastServer { get; }
    bool IsJoining { get; }

    event Action<ISimpleServerInfo, JoinKind>? ServerJoined;

    Task<JoinServerResult> JoinLastServer();
    Task<JoinServerResult> JoinServer(ISimpleServerInfo server, string? password, JoinKind kind);
    Task<JoinServerResult> JoinServer(IServerInfo serverInfo, JoinKind kind);
}
