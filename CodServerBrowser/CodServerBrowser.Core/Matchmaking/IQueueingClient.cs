using CodServerBrowser.Core.Matchmaking.Models;
using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Matchmaking
{
    public interface IQueueingClient
    {
        Task<bool> NotifyJoin(JoinServerInfo serverInfo, CancellationToken cancellationToken);

        Task OnQueuePositionChanged(int queuePosition, int queueSize);

        Task OnRemovedFromQueue(DequeueReason reason);

        Task OnAddedToQueue(JoinServerInfo serverInfo);
    }
}
