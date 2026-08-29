using CalculateAngleViaDistanceIronNest.Calculate;
using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.Utilitys;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace CalculateAngleViaDistanceIronNest.Runtime {
    class AppRuntime(AppState state) {
        private readonly AppState _state = state;
        private Dictionary<string, CommandRegistry.CommandInfo> _commandMap;
        public void SetCommandMap(Dictionary<string, CommandRegistry.CommandInfo> commandMap)
            => _commandMap = commandMap;

        public async Task Run() {
#if RELEASE
            await Info();
#endif

            AnsiConsole.MarkupLine("Use /help for list of commands");
            while (true) {
                try {
                    CustomReadLine();
                }
                catch (Exception ex) {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
#if DEBUG
                    AnsiConsole.WriteException(ex);
#endif
                }
            }
        }
        public string CustomReadLine() {
            string input = AnsiConsole.Ask<string>("> ");
            if (input != null && input.StartsWith('/')) {
                HandleCommand(input);
            }
            return input;
        }


        void HandleCommand(string commandLine) {
            var parts = commandLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            if (_commandMap.TryGetValue(parts[0].ToLower(), out var info)) info.Handler(parts);
            else AnsiConsole.MarkupLine("[red]Unknown or incomplete command.[/]");
        }

        public void Output(float km, int charges, float hozAngle, Gun gunSelected) {
            float velAngle = Calculator.CalcAngle(km, charges);
            float timeTravel = Calculator.CalcTimeTravel(km, charges);
            float? hozAngleConvert = (hozAngle > 0) ? hozAngle : null;

            _state.SaveNewAngle(velAngle, hozAngleConvert, charges, gunSelected, timeTravel);


            if (_state.alwaysShowList) _state.ReturnSavedListTable();
        }

        private static async Task Info() {
            var table = new Table()
                .HideHeaders()
                .Border(TableBorder.None);

            string[] fullVersion = Utility.GetVersion();
            string version = fullVersion[0];
            string hash = fullVersion[1];


            var (isOutdated, lastVersion) = await UpdateChecker.CheckForUpdates(version);
            string updateInfo = isOutdated ? $"[red](Outdated, update available: {lastVersion})[/]" : "[green](Up to date)[/]";

            table.AddColumn("Key");
            table.AddColumn("Value");

            table.AddRow("[blue]Version:[/]", $"{version} ({hash}) {updateInfo}");

            AnsiConsole.Write(table);
        }
    }
}