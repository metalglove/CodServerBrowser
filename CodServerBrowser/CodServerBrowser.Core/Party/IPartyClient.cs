﻿using CodServerBrowser.Core.Models;

namespace CodServerBrowser.Core.Party;

public interface IPartyClient
{
    Task OnUserJoinedParty(string id, string playerName);
    Task OnUserLeftParty(string id);
    Task OnUserNameChanged(string id, string newPlayerName);
    Task OnPartyClosed();
    Task OnKickedFromParty();

    Task OnLeaderChanged(string oldLeaderId, string newLeaderId);

    /// <summary>
    /// Called when the party leader joined a server.
    /// </summary>
    Task OnServerChanged(SimpleServerInfo server);

    Task OnConnectionRejected(string reason);
}
