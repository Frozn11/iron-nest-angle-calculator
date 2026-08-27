using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Runtime;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.JsonSaveLoad;
using CalculateAngleViaDistanceIronNest.Utilitys;
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

            if (!args.Contains("--relaunched") && !Utility.IsAlreadyInGoodTerminal() && IsLegacyConsoleMode(getJson)) {
                if (Utility.TryRelaunchInBetterTerminal(args)) return;
                // no better terminal found fall through and run in whatever we have
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
                    if (AnsiConsole.Confirm("[yellow]Do you want to use the modern console mode?[/]")) {
                        jsonSave.useLegacyConsole = true;
                        JsonManger.SaveJson(jsonSave);
                        return true;
                    }
                    else {
                        jsonSave.useLegacyConsole = false;
                        JsonManger.SaveJson(jsonSave);
                        return false;
                    }
                }
            }
        }
    }
}

