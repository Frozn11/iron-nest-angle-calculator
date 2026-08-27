using CalculateAngleViaDistanceIronNest.Calculate;
using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Data;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace CalculateAngleViaDistanceIronNest.Runtime {
    class AppRuntime {
        private readonly AppState _state;
        private Dictionary<string, CommandRegistry.CommandInfo> _commandMap;
        public AppRuntime(AppState state) => _state = state;
        public void SetCommandMap(Dictionary<string, CommandRegistry.CommandInfo> commandMap)
            => _commandMap = commandMap;

        public void Run() {
            AnsiConsole.MarkupLine("Use /help for list of commands");
            while (true) {
                try {
                    var result = CustomReadLine();
                    if (result.Status == InputStatus.Quit) break;
                }
                catch (Exception ex) {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                #if DEBUG
                    AnsiConsole.WriteException(ex);
                #endif
                }
            }
        }

        public InputResult CustomReadLine() {
            AnsiConsole.Markup("> ");
            string input = Console.ReadLine();
            if (input != null && input.ToLower().StartsWith('q'))
                return new InputResult(input, InputStatus.Quit);
            if (input != null && input.StartsWith('/')) {
                HandleCommand(input);
                return new InputResult(input, InputStatus.Command);
            }
            return new InputResult(input, InputStatus.Normal);
        }

        void HandleCommand(string commandLine) {
            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            if (_commandMap.TryGetValue(parts[0].ToLower(), out var info)) info.Handler(parts);
            else AnsiConsole.MarkupLine("[red]Unknown or incomplete command.[/]");
        }

        public void Output(float km, int charges, float hozAngle, Gun gunSelected) {
            float velAngle = Calculator.CalcAngle(km, charges);
            float timeTravel = Calculator.CalcTimeTravel(km, charges);
            float? hozAngleConvert = (hozAngle > 0) ? hozAngle : null;

            var saved = _state.SaveNewAngle(velAngle, hozAngleConvert, charges, gunSelected, timeTravel);

            AnsiConsole.MarkupLine(new string('-', 40));
            AnsiConsole.MarkupLine(_state.ReturnSaveAngle(saved));

            if (_state.alwaysShowList) AnsiConsole.MarkupLine(_state.ReturnSavedListPlaneText());
        }
    }
}