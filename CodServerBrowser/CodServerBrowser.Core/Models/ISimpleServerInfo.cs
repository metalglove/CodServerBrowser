namespace CodServerBrowser.Core.Models;

/// <summary>
/// Provides simple server info (IP, port and name)
/// </summary>
public interface ISimpleServerInfo : IServerConnectionDetails
{
    public string ServerName { get; }
}
