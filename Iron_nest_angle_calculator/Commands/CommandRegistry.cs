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
    class ExitException : Exception { }
    class CommandRegistry(AppState state, AppRuntime runtime) {
        private readonly AppState _state = state;
        private readonly AppRuntime _runtime = runtime;
        private (string[] Aliases, CommandInfo Info)[] _commandsHolder;
        public record CommandInfo(Action<string[]> Handler, string Usage, string Description, bool Hidden = false);
        public Dictionary<string, CommandInfo> BuildCommandMap() {
            _commandsHolder = [
                (["/help", "/h"], new(HandleHelp, "/help", "shows this list of commands")),
                (["/remove", "/rem"], new(HandleRemove, "/remove <index>", "gives an option to remove one of saved angles from a list")),
                (["/list"], new(HandleList, "/list", "shows list of saved angles")),
                (["/savelist"], new(HandleSaveList, "/savelist <true/false>", "enables/disables saving angles, but keeps old angles")),
                (["/setmaxlist"], new(HandleSetMaxList, "/setmaxlist <number>", "makes so it removes old saved angles when it gets bigger than max size list")),
                (["/calculatemode", "/calcmode", "/cm"], new(HandleCalcMode, "/calcmode", "starts calculation mode")),
                (["/calculate", "/calc", "/c"], new(HandleCalculate, "/calc <km> <charges> <gun>", "fast calculation")),
            ];
            return _commandsHolder
                .SelectMany(c => c.Aliases.Select(alias => (alias, c.Info)))
                .ToDictionary(x => x.alias, x => x.Info);
        }

        static bool IsExit(string s) => string.Equals(s?.Trim(), "q", StringComparison.OrdinalIgnoreCase);

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
                _state.ReturnSavedListTable();
                AnsiConsole.MarkupLine("Enter an index to remove selected item");
                string input = _runtime.CustomReadLine();
                if (!int.TryParse(input, out index)) {
                    AnsiConsole.MarkupLine("[red]Invalid index.[/]");
                    return;
                }
            }

            if (index >= 0 && index < _state.savedAnglesList.Count) {
                _state.savedAnglesList.RemoveAt(index);
                AnsiConsole.MarkupLine("[green]Item removed.[/]");
            }
            else {
                AnsiConsole.MarkupLine("[red]Invalid index.[/]");
            }
        }

        void HandleList(string[] parts) => _state.ReturnSavedListTable();

        void HandleSaveList(string[] parts) {
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            if (Utility.TryParseBool(args, out bool saveListValue)) {
                _state.saveList = saveListValue;
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
            try {
                while (true) {
                    AnsiConsole.WriteLine("Enter \"q\" to exit calculation mode.");
                    Gun gunResult = ReadGun();
                    AnsiConsole.MarkupLine($"[green]Selected gun: {gunResult}[/]");
                    float hozAngle = ReadHozAngle();
                    float km = ReadDistance();
                    int charges = ReadCharges(km);

                    _runtime.Output(km, charges, hozAngle, gunResult);
                }
            }
            catch (ExitException) {
                AnsiConsole.MarkupLine("[yellow]Exited calculation mode.[/]");
            }
            catch (Exception e) {
                AnsiConsole.MarkupLine($"[red]An error occurred: {e.Message}[/]");
            }
        }

        void HandleCalculate(string[] parts) {
            Gun gun = Gun.None;
            string errorMessage;

            if (parts.Length < 3) {
                errorMessage = "[red]Missing arguments. Usage: /calculate <km> <charges> <gun>[/]";
            }
            else {
                bool isKmInvalid = !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float km);
                bool isChargesInvalid = !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int charges);
                bool isGunInvalid = parts.Length >= 4 && !Enum.TryParse(parts[3], true, out gun);

                errorMessage = isKmInvalid ? "[red]Invalid distance value (min: 0.0005 km, max: 30.00 km).[/]"
                   : isChargesInvalid ? "[red]Invalid charges value (min: 1, max: 6).[/]"
                   : isGunInvalid ? "[red]Invalid gun value.[/]"
                   : !Utility.CheckDistanceLimit(km) ? $"[red]{Utility.GetDistanceLimitText(km)}[/]"
                   : !Utility.CheckChargeLimit(charges) ? $"[red]{Utility.GetChargeLimitText(charges)}[/]"
                   : null;

                if (errorMessage == null) {
                    _runtime.Output(km: km, charges: charges, hozAngle: -1f, gunSelected: gun);
                    return;
                }
            }

            AnsiConsole.MarkupLine(errorMessage);
        }

        Gun ReadGun() {
            // Console.WriteLine("Select Gun: Left(L) or Right(R) (can be skipped if not needed)");
            string promt = AnsiConsole.Prompt(
                new TextPrompt<string>("Select Gun: Left(L) or Right(R) (can be skipped if not needed):")
                    .AllowEmpty()
                    .Validate(str => {
                        if (IsExit(str) || string.IsNullOrWhiteSpace(str)) return ValidationResult.Success();
                        return str.Trim().ToUpperInvariant() switch {
                            "L" or "LEFT" => ValidationResult.Success(),
                            "R" or "RIGHT" => ValidationResult.Success(),
                            _ => ValidationResult.Error("[red]Invalid gun selection. Please enter Left(L), Right(R) or leave blank to skip.[/]")
                        };
                    })
            );
            if (IsExit(promt)) throw new ExitException();
            if (string.IsNullOrWhiteSpace(promt)) return Gun.None;

            return promt.Trim().ToUpperInvariant() switch {
                "L" or "LEFT" => Gun.Left,
                "R" or "RIGHT" => Gun.Right,
                _ => Gun.None
            };
        }

        static float ReadHozAngle() {
            string input = AnsiConsole.Prompt(
                new TextPrompt<string>("Set horizontal angle from 0.00 to 360.00 (can be skipped if not needed):")
                    .AllowEmpty()
                    .Validate(str => {
                        if (IsExit(str) || string.IsNullOrWhiteSpace(str)) return ValidationResult.Success();
                        if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float hozAngle)) {
                            return ValidationResult.Error("[red]Invalid angle value.[/]");
                        }
                        return Utility.CheckHozAngleLimit(hozAngle)
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]{Utility.GetHozAngleLimitText(hozAngle)}[/]");
                    })
            );
            if (IsExit(input)) throw new ExitException();
            return string.IsNullOrWhiteSpace(input) ? -1f : float.Parse(input, CultureInfo.InvariantCulture);
        }


        static float ReadDistance() {
            while (true) {
                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter distance in km (min: 0.0005 km, max: 30.00 km):")
                        .AllowEmpty()
                        .Validate(str => {
                            if (IsExit(str) || string.IsNullOrWhiteSpace(str)) return ValidationResult.Success();
                            if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float km)) {
                                return ValidationResult.Error("[red]Invalid distance value.[/]");
                            }
                            return Utility.CheckDistanceLimit(km)
                            ? ValidationResult.Success()
                            : ValidationResult.Error($"[red]{Utility.GetDistanceLimitText(km)}[/]");
                        })
                );
                if (IsExit(input)) throw new ExitException();
                if (float.TryParse(input, CultureInfo.InvariantCulture, out float km) && Utility.CheckDistanceLimit(km)) {
                    return km;
                }
                AnsiConsole.MarkupLine($"[red]Invalid distance value.[/]");
                continue;
            }
        }

        static int ReadCharges(float km) {
            // minCharges assumes km <= 30 (enforced by ReadDistance) GetMinCharges
            // would return -1 otherwise, which this code doesn't currently handle.
            int minCharges = Calculator.GetMinCharges(km);
            while (true) {
                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Enter amount of charges (min: {minCharges}, max: 6):")
                        .AllowEmpty()
                        .Validate(str => {
                            if (IsExit(str) || string.IsNullOrWhiteSpace(str)) return ValidationResult.Success();
                            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int charges)) {
                                return ValidationResult.Error("[red]Invalid charges value.[/]");
                            }


                            if (!Utility.CheckChargeLimit(charges)) {
                                return ValidationResult.Error($"[red]{Utility.GetChargeLimitText(charges)}[/]");
                            }
                            if (charges < minCharges) {
                                return ValidationResult.Error($"[red]Minimum charges for {km} km is {minCharges}.[/]");
                            }
                            return ValidationResult.Success();
                        })
                    );
                if (IsExit(input)) throw new ExitException();
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int charges) && Utility.CheckChargeLimit(charges)) {
                    return charges;
                }
                AnsiConsole.MarkupLine($"[red]Invalid charges value.[/]");
                continue;
            }
        }
    }
}
