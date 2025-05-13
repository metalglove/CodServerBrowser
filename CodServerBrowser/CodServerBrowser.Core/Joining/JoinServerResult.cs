namespace CodServerBrowser.Core.Joining;

public enum JoinServerResult
{
    None,
    MissingMap,
    ServerFull,
    JoinFailed,
    QueueJoined,
    QueueUnavailable,
    GameNotRunning,
    NoPassword,
    ForceJoinSuccess,
    Success,
    AlreadyJoining
}
