using System.Diagnostics;

namespace CodServerBrowser.Core.Game.Models
{
    public record DetectedGame(Process Process, string FileName, string GameDir, FileVersionInfo Version);
}
