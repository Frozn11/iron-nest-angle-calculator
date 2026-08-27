using CalculateAngleViaDistanceIronNest.Calculate;
using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.JsonSaveLoad;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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
        private static AppState _state;
        private static Dictionary<string, CommandRegistry.CommandInfo> _commandMap;
        private static readonly CancellationTokenSource _cts = new();
        public static bool QuitRequested => _cts.IsCancellationRequested;

        public static InputResult CustomReadLine() {
            Console.Write("> ");
            string input = Console.ReadLine();
            if (input != null && input.ToLower().StartsWith("q"))
                return new InputResult(input, InputStatus.Quit);
            if (input != null && input.StartsWith("/")) {
                HandleCommand(input);
                return new InputResult(input, InputStatus.Command);
            }
            return new InputResult(input, InputStatus.Normal);
        }

        static void HandleCommand(string commandLine) {
            string[] parts = commandLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            string command = parts[0].ToLower();

            if (_commandMap.TryGetValue(command, out var info)) info.Handler(parts);
            else AnsiConsole.MarkupLine("[red]Unknown or incomplete command.[/]");
        }

        public static bool TryParseBool(string input, out bool result) {
            result = false;
            if (string.IsNullOrEmpty(input)) return false;

            input = input.Trim().ToLower();
            if (input == "true" || input == "1") {
                result = true;
                return true;
            }
            if (input == "false" || input == "0") {
                result = false;
                return true;
            }
            return false;
        }

        static void Logic() {
            Console.WriteLine("Use /help for list of commands");
            while (true) {
                var result = CustomReadLine();
            }
        }

        public static void Output(float km, int charges, float hozAngle, Gun gunSelected) {
            float velAngle = Calculator.CalcAngle(km, charges);
            float timeTravel = Calculator.CalcTimeTravel(km, charges);

            float? hozAngleConvert = (hozAngle > 0) ? hozAngle : null;
            _state.SaveNewAngle(velAngle, hozAngleConvert, charges, gunSelected, timeTravel);

            AnsiConsole.MarkupLine(string.Concat(Enumerable.Repeat("-", 40)));
            AnsiConsole.MarkupLine($"  {gunSelected} gun,\n" +
                $"  vertical angle {velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\n" +
                $"  horizontal angle {hozAngleConvert?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) ?? "null"},\n" +
                $"  charges {charges},\n" +
                $"  time to travel {timeTravel.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} secondes");

            if (_state.alwaysShowList) AnsiConsole.MarkupLine(_state.ReturnSavedListPlaneText());
        }


        static void Main(string[] args) {
            if (args.Length > 0 && args[0] == "--build-release") {
                BuildReleaseApp.Run();
                return;
            }

            //// Only try to relaunch once — avoid infinite relaunch loop
            //if (!args.Contains("--relaunched") && IsWindowsTerminalAvailable() && !IsAlreadyInWindowsTerminal()) {
            //    RelaunchInWindowsTerminal(args);
            //    return;
            //}
            JsonManger.CheckFolderFile();

            _state = new AppState();
            var commandRegistry = new CommandRegistry(_state);
            _commandMap = commandRegistry.BuildCommandMap();
            Logic();
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
