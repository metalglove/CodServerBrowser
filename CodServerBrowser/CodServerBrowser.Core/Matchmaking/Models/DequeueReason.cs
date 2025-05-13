namespace CodServerBrowser.Core.Matchmaking.Models
{
    public enum DequeueReason
    {
        Unknown,
        UserLeave,
        MaxJoinAttemptsReached,
        JoinTimeout,
        Disconnect,
        Joined,
        JoinFailed,
        PartyLeave,
        PartyClosed,
    }
}
