using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Runtime;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.JsonSaveLoad;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.Linq;

namespace CalculateAngleViaDistanceIronNest {
    public enum InputStatus { Normal, Quit, Command }

    public readonly struct InputResult {
        public string Value { get; }
        public InputStatus Status { get; }
        public InputResult(string value, InputStatus status) {
            Value = value;
            Status = status;
        }
    }

    class Program {
        static void Main(string[] args) {

            if (args.Length > 0 && args[0] == "--build-release") {
                BuildReleaseApp.Run();
                return;
            }
            JsonSave getJson = JsonManger.GetLoadManger();
            if (getJson == null) {
                JsonManger.CheckFolderFile();
                getJson = JsonManger.GetLoadManger();
            }
            if (!args.Contains("--relaunched") && OperatingSystem.IsWindows()
                && IsWindowsTerminalAvailable() && !IsAlreadyInWindowsTerminal() && IsLegacyConsoleMode(getJson)) {
                RelaunchInWindowsTerminal(args);
                return;
            }
            var state = new AppState();
            var runtime = new AppRuntime(state);
            var commandRegistry = new CommandRegistry(state, runtime);
            runtime.SetCommandMap(commandRegistry.BuildCommandMap());
            runtime.Run();
        }
        static bool IsLegacyConsoleMode(JsonSave jsonSave) {
            if (jsonSave.useLegacyConsole != null) {
                return jsonSave.useLegacyConsole.Value;
            }
            else {
                while (true) {
                    AnsiConsole.MarkupLine("[yellow]Do you want to use the modern console mode? (y/n)[/]");
                    string input = Console.ReadLine()?.Trim().ToLower();
                    if (input == "y" || input == "yes") {
                        jsonSave.useLegacyConsole = true;
                        JsonManger.SaveJson(jsonSave);
                        return true;
                    }
                    else if (input == "n" || input == "no") {
                        jsonSave.useLegacyConsole = false;
                        JsonManger.SaveJson(jsonSave);
                        return false;
                    }
                    else {
                        AnsiConsole.MarkupLine("[red]Invalid input. Please enter 'y' or 'n'.[/]");
                    }
                }
            }
        }
        static bool IsWindowsTerminalAvailable() {
            try {
                var psi = new ProcessStartInfo {
                    FileName = "where",
                    Arguments = "wt.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc.WaitForExit();
                return proc.ExitCode == 0;
            }
            catch {
                return false;
            }
        }

        static bool IsAlreadyInWindowsTerminal() {
            // Windows Terminal sets this env var for processes running inside it
            return Environment.GetEnvironmentVariable("WT_SESSION") != null;
        }

        static void RelaunchInWindowsTerminal(string[] args) {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            var psi = new ProcessStartInfo {
                FileName = "wt.exe",
                Arguments = $"\"{exePath}\" --relaunched",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
