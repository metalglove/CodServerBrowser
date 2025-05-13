using CommunityToolkit.Mvvm.Messaging.Messages;

using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Joining;

public class JoinRequestMessage(ISimpleServerInfo server, string? password, JoinKind kind) : AsyncRequestMessage<JoinServerResult>
{
    public ISimpleServerInfo Server { get; } = server;

    public string? Password { get; } = password;

    public JoinKind Kind { get; } = kind;
}
