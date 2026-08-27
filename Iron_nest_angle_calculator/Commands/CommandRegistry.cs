using CalculateAngleViaDistanceIronNest.Calculate;
using CalculateAngleViaDistanceIronNest.Data;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CalculateAngleViaDistanceIronNest.Commands {
    public readonly struct ReadResult<T> {
        public T Value { get; }
        public bool Aborted { get; }
        public static ReadResult<T> Ok(T value) => new(value, false);
        public static ReadResult<T> Abort() => new(default, true);
        private ReadResult(T value, bool aborted) { Value = value; Aborted = aborted; }
    }
    class CommandRegistry {
        private readonly AppState _state;
        private (string[] Aliases, CommandInfo Info)[] _commandsHolder;
        public CommandRegistry(AppState state) => _state = state;
        public record CommandInfo(Action<string[]> Handler, string Usage, string Description, bool Hidden = false);
        public Dictionary<string, CommandInfo> BuildCommandMap() {
            _commandsHolder = new (string[], CommandInfo)[] {
                (new[] { "/help", "/h" }, new(HandleHelp, "/help", "shows this list of commands")),
                (new[] { "/remove", "/rem" }, new(HandleRemove, "/remove <index>", "gives an option to remove one of saved angles from a list")),
                (new[] { "/list" }, new(HandleList, "/list", "shows list of saved angles")),
                (new[] { "/savelist" }, new(HandleSaveList, "/savelist <true/false>", "enables/disables saving angles, but keeps old angles")),
                (new[] { "/alwaysshowlist", "/alwsl" }, new(HandleAlwaysShowList, "/alwaysshowlist <true/false>", "if set to true shows saved list every time")),
                (new[] { "/setmaxlist" }, new(HandleSetMaxList, "/setmaxlist <number>", "makes so it removes old saved angles when it gets bigger than max size list")),
                (new[] { "/calcmode" }, new(HandleCalacMode, "/calcmode", "starts calculation mode")),
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
                var readResult = Program.CustomReadLine();
                if (readResult.Status != InputStatus.Normal || !int.TryParse(readResult.Value, out index)) {
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
            if (Program.TryParseBool(args, out bool saveListValue)) {
                _state.saveList = saveListValue;
            }
            else {
                AnsiConsole.MarkupLine("[red]wrong value[/]");
            }
        }
        void HandleAlwaysShowList(string[] parts) {
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            if (Program.TryParseBool(args, out bool alwaysShowListValue)) {
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

        void HandleCalacMode(string[] parts) {
            while (true) {
                var gunResult = ReadGun();
                if (gunResult.Aborted) return;

                float hozAngle = ReadHozAngle();

                float km = ReadDistance();

                int charges = ReadCharges(km);

                Program.Output(km, charges, hozAngle, gunResult.Value);
            }
        }

        static ReadResult<Gun> ReadGun() {
            Console.WriteLine("Select Gun: Left(L) or Right(R) (can be skipped if not needed)");
            var input = Program.CustomReadLine();
            if (input.Status != InputStatus.Normal) return ReadResult<Gun>.Abort();

            Gun gun = input.Value?.ToLower() switch {
                "right" or "r" => Gun.Right,
                "left" or "l" => Gun.Left,
                _ => Gun.None
            };
            return ReadResult<Gun>.Ok(gun);
        }

        static float ReadHozAngle() {
            return AnsiConsole.Prompt(
                new TextPrompt<float>("Set horizontal angle from 0.00 to 360.00 (can be skipped if not needed):")
                    .Culture(CultureInfo.InvariantCulture)
                    .DefaultValue(-1f)
                    .ShowDefaultValue(false)
                    .Validate(angle => angle switch {
                        <= -1 => ValidationResult.Success(), // skip sentinel
                        > 360 => ValidationResult.Error("[red]Angle can't be bigger than 360.00[/]"),
                        <= 0 => ValidationResult.Error(
                            $"[red]Angle can't be smaller than 0.00, you entered {angle.ToString("F2", CultureInfo.InvariantCulture)}[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }

        static float ReadDistance() {
            return AnsiConsole.Prompt(
                new TextPrompt<float>("Enter distance in km (min: 0.0005 km, max: 30.00 km):")
                    .Culture(CultureInfo.InvariantCulture)
                    .Validate(km => km switch {
                        < 0.0005f => ValidationResult.Error(
                            $"[red]Distance can't be smaller than 0.0005 km, you entered {km.ToString("F2", CultureInfo.InvariantCulture)}[/]"),
                        > 30f => ValidationResult.Error(
                            $"[red]Distance can't be bigger than 30.00 km, you entered {km.ToString("F2", CultureInfo.InvariantCulture)}[/]"),
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
                        > 6 => ValidationResult.Error("[red]You entered a number bigger than 6[/]"),
                        < 1 => ValidationResult.Error("[red]You entered a number smaller than 1[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }
    }
}
