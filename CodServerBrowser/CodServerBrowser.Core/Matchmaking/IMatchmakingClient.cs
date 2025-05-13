using CodServerBrowser.Core.Matchmaking.Models;

namespace CodServerBrowser.Core.Matchmaking
{
    public interface IMatchmakingClient
    {
        Task OnMatchmakingEntered(MatchmakingMetadata metadata);
        Task OnSearchMatchUpdate(IEnumerable<SearchMatchResult> searchMatchResults);
        Task OnMetadataUpdate(MatchmakingMetadata metadata);
        Task OnMatchFound(string hostName, SearchMatchResult matchResult);
        Task OnRemovedFromMatchmaking(MatchmakingError reason);
    }
}
