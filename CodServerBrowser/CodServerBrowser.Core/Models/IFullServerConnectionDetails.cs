namespace CodServerBrowser.Core.Models;

public interface IFullServerConnectionDetails : IServerConnectionDetails
{
    public string? Password { get; }
}
