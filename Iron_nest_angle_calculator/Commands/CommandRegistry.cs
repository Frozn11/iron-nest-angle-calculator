using CalculateAngleViaDistanceIronNest.Calculate;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.Runtime;
using CalculateAngleViaDistanceIronNest.Utilitys;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CalculateAngleViaDistanceIronNest.Commands {
    class CommandRegistry {
        private readonly AppState _state;
        private readonly AppRuntime _runtime;
        public CommandRegistry(AppState state, AppRuntime runtime) {
            _state = state;
            _runtime = runtime;
        }
        private (string[] Aliases, CommandInfo Info)[] _commandsHolder;
        public record CommandInfo(Action<string[]> Handler, string Usage, string Description, bool Hidden = false);
        public Dictionary<string, CommandInfo> BuildCommandMap() {
            _commandsHolder = new (string[], CommandInfo)[] {
                (new[] { "/help", "/h" }, new(HandleHelp, "/help", "shows this list of commands")),
                (new[] { "/remove", "/rem" }, new(HandleRemove, "/remove <index>", "gives an option to remove one of saved angles from a list")),
                (new[] { "/list" }, new(HandleList, "/list", "shows list of saved angles")),
                (new[] { "/savelist" }, new(HandleSaveList, "/savelist <true/false>", "enables/disables saving angles, but keeps old angles")),
                (new[] { "/alwaysshowlist", "/alwsl" }, new(HandleAlwaysShowList, "/alwaysshowlist <true/false>", "if set to true shows saved list every time")),
                (new[] { "/setmaxlist" }, new(HandleSetMaxList, "/setmaxlist <number>", "makes so it removes old saved angles when it gets bigger than max size list")),
                (new[] { "/calculatemode", "/calcmode", "/cm" }, new(HandleCalcMode, "/calcmode", "starts calculation mode")),
                (new[] { "/calculate", "/calc", "/c" }, new(HandleCalculate, "/calc <km> <charges> <gun>", "fast calculation")),
            };
            return _commandsHolder
                .SelectMany(c => c.Aliases.Select(alias => (alias, c.Info)))
                .ToDictionary(x => x.alias, x => x.Info);
        }

        // Command Handlers
        void HandleHelp(string[] parts) {
            AnsiConsole.MarkupLine("--------Commands--------");
            foreach (var (_, info) in _commandsHolder) {
                if (info.Hidden) continue;
                AnsiConsole.MarkupLine($" {info.Usage} - {info.Description}");
            }
            AnsiConsole.MarkupLine("------------------------");
        }

        void HandleRemove(string[] parts) {
            string arg = parts.Length > 1 ? parts[1].Trim() : null;
            if (_state.savedAnglesList.Count == 0) {
                AnsiConsole.MarkupLine("[red]The list is empty.[/]");
                return;
            }

            if (string.IsNullOrEmpty(arg) || !int.TryParse(arg, out int index)) {
                AnsiConsole.MarkupLine(_state.ReturnSavedListPlaneText());
                AnsiConsole.MarkupLine("Enter an index to remove selected item");
                string input = _runtime.CustomReadLine();
                if (!int.TryParse(input, out index)) {
                    AnsiConsole.MarkupLine("[red]Invalid index.[/]");
                    return;
                }
            }

            if (index >= 0 && index < _state.savedAnglesList.Count) {
                _state.savedAnglesList.RemoveAt(index); // was Remove(list[index]) — RemoveAt avoids the extra lookup
                AnsiConsole.MarkupLine("[green]Item removed.[/]");
            }
            else {
                AnsiConsole.MarkupLine("[red]Invalid index.[/]");
            }
        }

        void HandleList(string[] parts) => AnsiConsole.MarkupLine(_state.ReturnSavedListPlaneText());

        void HandleSaveList(string[] parts) {
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            if (Utility.TryParseBool(args, out bool saveListValue)) {
                _state.saveList = saveListValue;
            }
            else {
                AnsiConsole.MarkupLine("[red]wrong value[/]");
            }
        }
        void HandleAlwaysShowList(string[] parts) {
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            if (Utility.TryParseBool(args, out bool alwaysShowListValue)) {
                _state.alwaysShowList = alwaysShowListValue;
            }
            else {
                AnsiConsole.MarkupLine("[red]wrong value[/]");
            }
        }
        void HandleSetMaxList(string[] parts) {
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            if (int.TryParse(args, out int value) && value > 0) {
                _state.maxSaveList = value;
                while (_state.savedAnglesList.Count > _state.maxSaveList) {
                    _state.savedAnglesList.RemoveAt(0);
                }
                AnsiConsole.MarkupLine($"[green]New max list set to: {_state.maxSaveList}[/]");
            }
            else {
                AnsiConsole.MarkupLine("[red]Error[/]");
            }
        }

        void HandleCalcMode(string[] parts) {
            while (true) {
                Gun gunResult = ReadGun();
                AnsiConsole.MarkupLine($"[green]Selected gun: {gunResult}[/]");
                float hozAngle = ReadHozAngle();
                float km = ReadDistance();
                int charges = ReadCharges(km);

                _runtime.Output(km, charges, hozAngle, gunResult);
            }
        }

        void HandleCalculate(string[] parts) {
            if (parts.Length < 3 ||
                    !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float km) ||
                    !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int charges)) {
                AnsiConsole.MarkupLine("[red]Invalid or missing arguments. Usage: /calculate <km> <charges> <gun>[/]");
                return;
            }

            Gun gun = Gun.None;
            if (parts.Length >= 4 && !Enum.TryParse(parts[3], true, out gun)) {
                AnsiConsole.MarkupLine("[red]Invalid gun value.[/]");
                return;
            }
            if (!Utility.CheckDistanceLimit(km)) {
                AnsiConsole.MarkupLine($"[red]{Utility.GetDistanceLimitText(km)} {Utility.F(km)}[/]");
                return;
            }
            _runtime.Output(km: km, charges: charges, hozAngle: -1f, gunSelected: gun);
        }

        Gun ReadGun() {
            // Console.WriteLine("Select Gun: Left(L) or Right(R) (can be skipped if not needed)");
            var promt = new SelectionPrompt<Gun>()
                .Title("Select Gun: Left(L) or Right(R) (can be skipped if not needed):")
                .UseConverter(gun => gun == Gun.None ? "Skip" : gun.ToString())
                .AddChoices(Gun.None, Gun.Left, Gun.Right);
            return AnsiConsole.Prompt(promt);
        }

        static float ReadHozAngle() {
            return AnsiConsole.Prompt(
                new TextPrompt<float>("Set horizontal angle from 0.00 to 360.00 (can be skipped if not needed):")
                    .Culture(CultureInfo.InvariantCulture)
                    .DefaultValue(-1f)
                    .ShowDefaultValue(false)
                    .Validate(angle => angle switch {
                        <= -1 => ValidationResult.Success(), // skip sentinel
                        _ when !Utility.CheckHozAngleLimit(angle) => ValidationResult.Error($"[red]{Utility.GetHozAngleLimitText(angle)} {Utility.F(angle)}[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }

        static float ReadDistance() {
            return AnsiConsole.Prompt(
                new TextPrompt<float>("Enter distance in km (min: 0.0005 km, max: 30.00 km):")
                    .Culture(CultureInfo.InvariantCulture)
                    .Validate(km => km switch {
                        _ when !Utility.CheckDistanceLimit(km) => ValidationResult.Error($"[red]{Utility.GetDistanceLimitText(km)} {Utility.F(km)}[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }

        static int ReadCharges(float km) {
            // minCharges assumes km <= 30 (enforced by ReadDistance) GetMinCharges
            // would return -1 otherwise, which this code doesn't currently handle.
            int minCharges = Calculator.GetMinCharges(km);
            return AnsiConsole.Prompt(
                new TextPrompt<int>($"Enter amount of charges (min: {minCharges}, max: 6):")
                    .Validate(c => c switch {
                        _ when c < minCharges => ValidationResult.Error($"[red]Too small, min charges are {minCharges}[/]"),
                        _ when !Utility.CheckChargeLimit(c) => ValidationResult.Error($"[red]{Utility.GetChargeLimitText(c)}[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }
    }
}
