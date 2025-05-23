﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CodServerBrowser.Core.Settings;
using CodServerBrowser.Core.Game.Models;
using CodServerBrowser.Core.Utilities;

using Microsoft.Extensions.Logging;

using Nogic.WritableOptions;

namespace CodServerBrowser.Core.Game
{
    public sealed class H2MGameDetectionService : IGameDetectionService
    {
        private readonly ILogger<H2MGameDetectionService> _logger;
        private readonly IWritableOptions<H2MLauncherSettings> _h2mLauncherSettings;
        private readonly IGameCommunicationService _gameCommunicationService;

        private const int GAME_DETECTION_POLLING_INTERVAL = 1000;

        private readonly object _gameDetectionLockObj = new();
        private CancellationTokenSource _gameDetectionCancellation = new();
        private Task? _gameDetectionTask;
        private bool _isRunning;

        public H2MGameDetectionService(
            ILogger<H2MGameDetectionService> logger,
            IWritableOptions<H2MLauncherSettings> h2mLauncherSettings,
            IGameCommunicationService gameCommunicationService)
        {
            _logger = logger;
            _h2mLauncherSettings = h2mLauncherSettings;
            _gameCommunicationService = gameCommunicationService;
        }

        public DetectedGame? DetectedGame { get; private set; }

        [MemberNotNullWhen(true, nameof(_gameDetectionTask))]
        public bool IsGameDetectionRunning => _gameDetectionTask != null && _isRunning;


        public event Action<DetectedGame>? GameDetected;

        public event Action? GameExited;

        public event Action<Exception?>? Error;

        public void StartGameDetection()
        {
            if (IsGameDetectionRunning)
            {
                return;
            }

            lock (_gameDetectionLockObj)
            {
                if (IsGameDetectionRunning)
                {
                    return;
                }

                _logger.LogDebug("Starting game detection...");

                _gameDetectionCancellation = new();
                _gameDetectionTask = Task.Run(
                    function: () => GameDetectionLoop(OnGameDetected, OnGameExited, cancellationToken: _gameDetectionCancellation.Token)
                                        .ContinueWith(t =>
                                        {
                                            _isRunning = false;
                                            if (t.IsFaulted)
                                            {
                                                _logger.LogError(t.Exception, "Game detection loop terminated with error:");
                                                Error?.Invoke(t.Exception);
                                            }
                                            else if (t.IsCanceled)
                                            {
                                                _logger.LogInformation("Game detection loop canceled.");
                                            }
                                            else
                                            {
                                                _logger.LogInformation("Game detection loop terminated.");
                                            }
                                        }),
                    cancellationToken: _gameDetectionCancellation.Token
                 );

                _isRunning = true;
                _logger.LogDebug("Game detection started");
            }
        }

        public void StopGameDetection()
        {
            if (!IsGameDetectionRunning)
            {
                return;
            }

            _logger.LogDebug("Stopping game detection...");

            lock (_gameDetectionLockObj)
            {
                try
                {
                    _gameDetectionCancellation.Cancel();
                    _gameDetectionTask.Wait();
                    _gameDetectionTask = null;

                    _logger.LogInformation("Game detection stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while stopping game detection");
                }
            }
        }

        private void OnGameDetected(DetectedGame detectedGame)
        {
            DetectedGame = detectedGame;

            _logger.LogInformation("Detected game {gameProcessName} (v{gameVersion})",
                detectedGame.Process.ProcessName, detectedGame.Version.ToString());

            if (string.IsNullOrEmpty(_h2mLauncherSettings.CurrentValue.MWRLocation))
            {
                _logger.LogDebug("Game location empty, setting to {gameLocation}", detectedGame.FileName);

                _h2mLauncherSettings.Update(settings =>
                {
                    return settings with
                    {
                        MWRLocation = detectedGame.FileName
                    };
                });
            }

            if (_h2mLauncherSettings.CurrentValue.GameMemoryCommunication)
            {
                _gameCommunicationService.StartGameCommunication(detectedGame.Process);
            }

            GameDetected?.Invoke(detectedGame);
        }

        private void OnGameExited()
        {
            _logger.LogInformation("Game process exited");

            DetectedGame = null;
            if (_gameCommunicationService.IsGameCommunicationRunning)
            {
                _gameCommunicationService.StopGameCommunication();
            }
            GameExited?.Invoke();
        }

        private static async Task GameDetectionLoop(
            Action<DetectedGame> onGameDetected, Action onGameExited, bool detectOnce = false, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Process? process = H2MCommunicationService.FindH2MModProcess();
                if (process is null || process.MainModule is null)
                {
                    goto delay;
                }

                string fileName = process.MainModule.FileName;
                string? gameDir = Path.GetDirectoryName(fileName);

                if (string.IsNullOrEmpty(gameDir) || !File.Exists(Path.Combine(gameDir, Constants.GAME_EXECUTABLE_NAME)))
                {
                    goto delay;
                }

                // game dir found

                FileVersionInfo version = FileVersionInfo.GetVersionInfo(fileName);

                onGameDetected(new DetectedGame(process, fileName, gameDir, version));

                try
                {
                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                    // sometimes throws when process is killed
                }

                // process terminated
                onGameExited();

                if (detectOnce)
                {
                    return;
                }

            delay: await Task.Delay(GAME_DETECTION_POLLING_INTERVAL, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
