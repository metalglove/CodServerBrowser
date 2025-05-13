using CodServerBrowser.Core.Matchmaking.Models;
using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Matchmaking
{
    public interface IMatchmakingHub
    {
        Task<bool> JoinQueue(JoinServerInfo serverInfo);

        Task JoinAck(bool successful);

        Task LeaveQueue();

        Task<bool> SearchMatch(MatchSearchCriteria searchPreferences, string playlistId);

        Task<bool> UpdateSearchSession(MatchSearchCriteria searchPreferences, List<ServerPing> serverPings);
    }
}