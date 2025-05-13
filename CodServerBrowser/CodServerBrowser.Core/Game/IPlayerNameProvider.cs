namespace CodServerBrowser.Core.Game
{
    public interface IPlayerNameProvider
    {
        string PlayerName { get; }

        event Action<string, string>? PlayerNameChanged;
    }
}