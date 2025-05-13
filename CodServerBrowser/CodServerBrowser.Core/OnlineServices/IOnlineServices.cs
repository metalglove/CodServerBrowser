using CodServerBrowser.Core.OnlineServices.Authentication;
using CodServerBrowser.Core.Matchmaking.Models;

namespace CodServerBrowser.Core.OnlineServices;

/// <summary>
/// Provides general state of the online services.
/// </summary>
public interface IOnlineServices
{
    /// <summary>
    /// Gets the client context used for the online services. 
    /// </summary>
    ClientContext ClientContext { get; }

    /// <summary>
    /// Whether the party service is connected.
    /// </summary>
    bool IsPartyServiceConnected { get; }

    /// <summary>
    /// Whether the queueing / matchmaking service is connected.
    /// </summary>
    bool IsQueueingServiceConnected { get; }

    /// <summary>
    /// The current client state synchronized with the server.
    /// </summary>
    PlayerState State { get; }

    event Action<PlayerState, PlayerState>? StateChanged;
}
