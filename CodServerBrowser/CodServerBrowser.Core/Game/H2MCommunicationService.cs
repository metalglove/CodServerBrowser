﻿using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CodServerBrowser.Core.Game.Models;
using CodServerBrowser.Core.Services;
using CodServerBrowser.Core.Settings;
using CodServerBrowser.Core.Utilities;
using Microsoft.Extensions.Logging;
using Nogic.WritableOptions;

namespace CodServerBrowser.Core.Game
{
    public sealed class H2MCommunicationService : IDisposable
    {
        // Mod executable file names (to automatically find game file in directory)
        private static readonly string[] GAME_EXECUTABLE_NAMES = ["hmw-mod.exe", "h2m-mod.exe", "h2m-revived.exe"];

        // Strings to match game / mod window titles
        private static readonly string[] H2M_WINDOW_TITLE_STRINGS = ["h2m-mod", "HorizonMW"];

        //Windows API constants
        private const int WM_CHAR = 0x0102; // Message code for sending a character
        private const int WM_KEYDOWN = 0x0100; // Message code for key down
        private const int WM_KEYUP = 0x0101;   // Message code for key up

        private readonly IWritableOptions<H2MLauncherSettings> _h2mLauncherSettings;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly ILogger<H2MCommunicationService> _logger;
        private readonly IDisposable? _optionsChangeRegistration;

        public IGameCommunicationService GameCommunication { get; }
        public IGameDetectionService GameDetection { get; }

        public H2MCommunicationService(IErrorHandlingService errorHandlingService, IWritableOptions<H2MLauncherSettings> options,
            ILogger<H2MCommunicationService> logger, IGameCommunicationService gameCommunicationService, IGameDetectionService gameDetectionService)
        {
            _errorHandlingService = errorHandlingService;
            _h2mLauncherSettings = options;
            _logger = logger;
            GameCommunication = gameCommunicationService;
            GameDetection = gameDetectionService;

            if (options.CurrentValue.AutomaticGameDetection)
            {
                GameDetection.StartGameDetection();
            }

            _optionsChangeRegistration = options.OnChange((settings, _) =>
            {
                if (!settings.AutomaticGameDetection && GameDetection.IsGameDetectionRunning)
                {
                    GameDetection.StopGameDetection();
                }
                else if (settings.AutomaticGameDetection && !GameDetection.IsGameDetectionRunning)
                {
                    GameDetection.StartGameDetection();
                }

                if (!settings.GameMemoryCommunication && GameCommunication.IsGameCommunicationRunning)
                {
                    GameCommunication.StopGameCommunication();
                }
                else if (settings.GameMemoryCommunication && !GameCommunication.IsGameCommunicationRunning
                    && GameDetection.DetectedGame is not null)
                {
                    GameCommunication.StartGameCommunication(GameDetection.DetectedGame.Process);
                }
            });
        }

        //Windows API functions to send input to a window
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProc lpfn, nint lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();


        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        private static IEnumerable<nint> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<nint>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, nint.Zero);

            return handles;
        }

        private static IEnumerable<nint> EnumerateWindowHandles()
        {
            var handles = new List<nint>();

            EnumWindows((hWnd, lParam) => { handles.Add(hWnd); return true; }, nint.Zero);

            return handles;
        }

        private static string? GetWindowTitle(nint hWnd)
        {
            const int length = 256;
            StringBuilder sb = new(length);

            if (GetWindowText(hWnd, sb, length) > 0)
            {
                return sb.ToString();
            }

            return null;
        }

        private bool TryFindValidGameFile(out string fileName)
        {
            if (string.IsNullOrEmpty(_h2mLauncherSettings.CurrentValue.MWRLocation))
            {
                foreach (string exeFileName in GAME_EXECUTABLE_NAMES)
                {
                    // no location set, try relative path
                    fileName = Path.GetFullPath(exeFileName);
                    if (File.Exists(fileName))
                    {
                        return true;
                    }
                }
            }

            string userDefinedLocation = Path.GetFullPath(_h2mLauncherSettings.CurrentValue.MWRLocation);

            if (!Path.Exists(userDefinedLocation))
            {
                // neither dir or file exists
                fileName = userDefinedLocation;
                return false;
            }

            if (File.GetAttributes(userDefinedLocation).HasFlag(FileAttributes.Directory))
            {
                // is a directory, get full file name
                foreach (string exeFileName in GAME_EXECUTABLE_NAMES)
                {
                    fileName = Path.Combine(userDefinedLocation, exeFileName);
                    if (File.Exists(fileName))
                    {
                        return true;
                    }
                }
            }

            // is a file?
            fileName = userDefinedLocation;
            return File.Exists(userDefinedLocation);
        }

        public void LaunchH2MMod()
        {
            ReleaseCapture();

            try
            {
                // Check if the process is already running
                if (GameDetection.DetectedGame is not null)
                {
                    _errorHandlingService.HandleError($"{Path.GetFileName(GameDetection.DetectedGame.FileName)} is already running.");
                    return;
                }

                // Proceed to launch the process if it's not running
                if (TryFindValidGameFile(out string gameFileName) &&
                    !string.IsNullOrEmpty(gameFileName))
                {
                    ProcessStartInfo startInfo = new(gameFileName)
                    {
                        WorkingDirectory = Path.GetDirectoryName(gameFileName)
                    };

                    Process.Start(startInfo);
                }
                else
                {
                    _errorHandlingService.HandleException(
                        new FileNotFoundException("H2M executable was not found."),
                            $"The H2M executable could not be found at '{gameFileName}'!");
                }
            }
            catch (Exception ex)
            {
                _errorHandlingService.HandleException(ex, "Error launching h2m-mod.");
            }
        }

        public Task<bool> JoinServer(string ip, string port, string? password = null)
        {
            const string disconnectCommand = "disconnect";
            string connectCommand = $"connect {ip}:{port}";

            if (password is not null)
            {
                connectCommand += $";password {password}";
            }

            return ExecuteCommandAsync([disconnectCommand, connectCommand]);
        }

        public Task<bool> Disconnect()
        {
            return ExecuteCommandAsync(["disconnect"]);
        }

        private async Task<bool> ExecuteCommandAsync(string[] commands, bool bringGameWindowToForeground = true)
        {
            Process? h2mModProcess = FindH2MModProcess();
            if (h2mModProcess == null)
            {
                _errorHandlingService.HandleError("Could not find the h2m-mod terminal window.");
                return false;
            }
            
            nint conHostHandle = GetConsoleHandle(h2mModProcess);

            // Grab the handle of conhost or main window
            nint hWindow = conHostHandle == nint.Zero ? h2mModProcess.MainWindowHandle : conHostHandle;

            ReleaseCapture();

            // Open In Game Terminal Window
            SendMessage(hWindow, WM_KEYDOWN, 192, nint.Zero);

            foreach (string command in commands)
            {
                // Send the command to the terminal window
                foreach (char c in command)
                {
                    SendMessage(hWindow, WM_CHAR, c, nint.Zero);
                    await Task.Delay(1);
                }

                // Sleep for 1ms to allow the command to be processed
                await Task.Delay(1);

                // Simulate pressing the Enter key
                SendMessage(hWindow, WM_KEYDOWN, 13, nint.Zero);
                SendMessage(hWindow, WM_KEYUP, 13, nint.Zero);
            }

            // Close Terminal Window
            SendMessage(hWindow, WM_KEYDOWN, 192, nint.Zero);

            if (bringGameWindowToForeground)
            {
                // Set H2M to foreground window
                var hGameWindow = FindH2MModGameWindow(h2mModProcess);
                SetForegroundWindow(hGameWindow);
            }

            return true;
        }

        public nint GetGameWindowHandle()
        {
            if (GameDetection.DetectedGame is null)
            {
                return nint.Zero;
            }

            return FindH2MModGameWindow(GameDetection.DetectedGame.Process);
        }

        public static Process? FindH2MModProcess()
        {
            // find processes with matching title
            var processesWithTitle = Process.GetProcesses().Where(p =>
                H2M_WINDOW_TITLE_STRINGS.Any(str => p.MainWindowTitle.Contains(str, StringComparison.OrdinalIgnoreCase))).ToList();

            // find process that loaded H1 MP binary
            var gameProc = processesWithTitle.FirstOrDefault(p =>
                p.Modules.OfType<ProcessModule>().Any(m => m.ModuleName.Equals(Constants.GAME_EXECUTABLE_NAME)));

            return gameProc;
        }

        private static bool IsH2MModProcess(Process p)
        {
            return H2M_WINDOW_TITLE_STRINGS.Any(str => p.MainWindowTitle.Contains(str, StringComparison.OrdinalIgnoreCase)) &&
                p.Modules.OfType<ProcessModule>().Any(m => m.ModuleName.Equals(Constants.GAME_EXECUTABLE_NAME));
        }

        private static nint GetConsoleHandle(Process process)
        {
            // Now, check if this window handle is the console window for that process.
            // A reliable way is to try attaching to the console. If it succeeds, it's a console.
            if (AttachConsole((uint)process.Id))
            {
                try
                {
                    return GetConsoleWindow();
                }
                finally
                {
                    FreeConsole(); // Detach immediately
                }
            }

            return nint.Zero;
        }

        private static nint FindH2MModGameWindow(Process process)
        {
            // find game window
            foreach (nint hChild in EnumerateProcessWindowHandles(process.Id))
            {
                if (GetConsoleHandle(process) != hChild)
                {
                    // if its not the console, its probably the game window
                    return hChild;
                }
            }

            // otherwise return just the main window, whatever it is
            return process.MainWindowHandle;
        }

        public void StartGameCommunication()
        {
            if (GameCommunication.IsGameCommunicationRunning ||
                GameDetection.DetectedGame is not DetectedGame detectedGame ||
                detectedGame.Process.HasExited)
            {
                return;
            }

            GameCommunication.StartGameCommunication(detectedGame.Process);
        }

        public void Dispose()
        {
            _optionsChangeRegistration?.Dispose();
        }
    }
}
