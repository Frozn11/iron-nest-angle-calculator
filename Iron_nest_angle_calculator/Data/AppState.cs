using CalculateAngleViaDistanceIronNest.Utilitys;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CalculateAngleViaDistanceIronNest.Data {
    public sealed class NoTopAsciiBorder : TableBorder {
        public override string GetPart(TableBorderPart part) => part switch {
            TableBorderPart.HeaderTopLeft => "",
            TableBorderPart.HeaderTop => "",
            TableBorderPart.HeaderTopSeparator => "",
            TableBorderPart.HeaderTopRight => "",
            _ => Ascii.GetPart(part)
        };
    }
    public class AppState {
        public List<SavedAngle> savedAnglesList { get; } = [];
        public int maxSaveList { get; set; } = 6;
        public bool saveList { get; set; } = true;
        public bool alwaysShowList { get; set; } = true;

        public SavedAngle SaveNewAngle(float velAngle, float? hozAngle, int charges, Gun gun, float timeToTravel) {
            var saved = new SavedAngle { velAngle = velAngle, hozAngle = hozAngle, charges = charges, gunSelected = gun, timeToTrivel = timeToTravel };
            if (saveList) {
                if (savedAnglesList.Count >= maxSaveList) savedAnglesList.RemoveAt(0);
                savedAnglesList.Add(saved);
            }
            return saved;
        }

        public void ReturnSavedListTable() {
            if (savedAnglesList.Count < 0) {
                AnsiConsole.Markup("Error: list is empty");
                return;
            }

            var table = new Table()
                .Border(new NoTopAsciiBorder())
                .AddColumn("Index", col => col.RightAligned())
                .AddColumns("vertical angle", "horizontal angle", "charges", "time to travel", "gun");


            for (int i = 0; i < savedAnglesList.Count; i++) {
                SavedAngle savedAngle = savedAnglesList[i];
                string lastSaveItem = i + 1 >= maxSaveList ?
                    $"[RED]#{i}[/]"
                    : i == 0 ? $"[Green]*{i}[/]"
                    : $"{i}";

                table.AddRow(
                    lastSaveItem,
                    Utility.F(savedAngle.velAngle),
                    Utility.F(savedAngle.hozAngle),
                    savedAngle.charges.ToString(),
                    Utility.F(savedAngle.timeToTrivel),
                    savedAngle.gunSelected.ToString()
                );
            }
            var writer = new StringWriter();
            var measureConsole = AnsiConsole.Create(new AnsiConsoleSettings {
                Out = new AnsiConsoleOutput(writer),
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors
            });
            measureConsole.Write(table);

            var lines = writer.ToString()
                .Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .Where(l => l.Length > 0)
                .ToList();
            int width = lines.Max(l => l.Length);

            string title = "Saved Angles List";
            string prefix = "+-" + title;
            int dashCount = Math.Max(0, width - prefix.Length - 1);
            string topLine = prefix + new string('-', dashCount) + "+";

            AnsiConsole.WriteLine(topLine);
            AnsiConsole.Write(table);
        }

    }
}
