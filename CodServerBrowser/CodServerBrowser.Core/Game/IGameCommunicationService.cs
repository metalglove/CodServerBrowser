using System.Diagnostics;

using CodServerBrowser.Core.Game.Models;

namespace CodServerBrowser.Core.Game;

public interface IGameCommunicationService : IDisposable
{
    GameState CurrentGameState { get; }
    Process? GameProcess { get; }
    bool IsGameCommunicationRunning { get; }

    event Action<GameState>? GameStateChanged;
    event Action<Process> Started;
    event Action<Exception?> Stopped;

    void StartGameCommunication(Process process);
    void StopGameCommunication();

    Task<IReadOnlyDictionary<int, string>> GetInGameMapsAsync();
}