﻿namespace CodServerBrowser.Core.Game
{
    public interface IMapsProvider
    {
        IReadOnlySet<string> InstalledMaps { get; }

        event Action<IMapsProvider>? MapsChanged;
    }
}