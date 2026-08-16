using Calculate_angle_via_distance_Iron_Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Iron_nest_angle_calculator {
class CommandEnteredException : Exception { }
    class Program {
        static List<SavedAngle> savedAnglesList = new List<SavedAngle>();
        static bool saveList = true;
        static bool alwaysShowList = true;

        // got this formula from Reddit https://www.reddit.com/r/IronNest/comments/1vjgvbb/math/
        // Elevation = (Distance in km x  12) / charges
        static float Calc_angle(float distnace, int charges) {
            float angle = (distnace * 12) / charges;
            angle = MathF.Floor(angle * 100f) / 100f;
            return angle;
        }

        // the min amout charges needed to shoot 
        // from 5 km to 30 km
        //      charges 1 = 5
        //      charges 2 = 10
        //      charges 3 = 15
        //      charges 4 = 20
        //      charges 5 = 25
        //      charges 6 = 30
        static int Get_Min_Charges(float km) {
            int minCharges = 1;

            if (km > 5 && km < 15) minCharges = 2;
            else if (km > 10 && km < 20) minCharges = 3;
            else if (km > 15 && km < 25) minCharges = 4;
            else if (km > 20 && km < 30) minCharges = 5;
            else if (km > 25) minCharges = 6;

            return minCharges;
        }

        static void SaveNewAngle(float velAngle, string hozAngle, int charges, Gun gunSelected) {
            if (!saveList) return;
            SavedAngle newSavedAngle = new() {
                velAngle = velAngle,
                hozAngle = hozAngle,
                charges = charges,
                gunSelected = gunSelected
            };
            savedAnglesList.Add(newSavedAngle);
        }

        // Console.WriteLine($"{gunSelected} gun, vertical angle set to {velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, horizontal angle set to {hozAngle}, charges needed {charges}\n");

        static string ReturnSaveAnglesList() {
            string text = "";
            for (int i = 0; i < savedAnglesList.Count; i++) {
                SavedAngle savedAngle = savedAnglesList[i];
                text += $"{i}.{savedAngle.gunSelected} gun,\n" +
                        $"  vertical angle {savedAngle.velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\n" +
                        $"  horizontal angle {savedAngle.hozAngle},\n" +
                        $"  charges {savedAngle.charges}\n";
                if (i + 1 < savedAnglesList.Count) {
                    text += " .\n";
                }
            }
            return text;
        }

        static void RemoveSavedAngle(string arg = null) {
            if (savedAnglesList.Count == 0) {
                Console.WriteLine("The list is empty");
                return;
            }
            if (!string.IsNullOrEmpty(arg) && int.TryParse(arg.Trim(), out int index)) {

            }
            else {
                Console.WriteLine(ReturnSavedListPlaneText());
                Console.WriteLine("Enter an index to remove seleted item");
                string input = CustomReadLine();
                if (!int.TryParse(input, out index)) {
                    Console.WriteLine("Invalid index.");
                    return;
                }
            }

            if (index >= 0 && index < savedAnglesList.Count) {
                savedAnglesList.Remove(savedAnglesList[index]);
                Console.WriteLine("Item remvoed.");
            }
            else {
                Console.WriteLine("Invalid index.");
            }
        }

        static string CustomReadLine() {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input != null && input.StartsWith("/")) {
                HandleCommand(input);
                throw new CommandEnteredException();
            }

            return input;
        }

        static void HandleCommand(string commandLine) {
            string[] parts = commandLine.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();
            string args = parts.Length > 1 ? parts[1].Trim() : null;
            switch (command) {
                case "/help":
                    Console.WriteLine("--------Commands--------");
                    Console.WriteLine("  /remove <index> - gives an option to remove one of saved agnles from a list" +
                        "\n  /list - shows list of saved agnles" +
                        "\n  /savelist <true/false> - enables/disables saving angles, but keeps old angles" +
                        "\n  /alwaysshowlist <true/false> - if set to true shows saved list every time" +
                        "\n------------------------");
                    break;
                case "/remove":
                    RemoveSavedAngle(args);
                    break;
                case "/list":
                    Console.WriteLine(ReturnSavedListPlaneText());
                    break;
                case "/savelist":
                    if (TryParseBool(args, out bool saveListValue)) {
                        saveList = saveListValue;
                    }
                    else {
                        Console.WriteLine("wrong value");
                    }
                    break;
                case "/alwaysshowlist":
                    if (TryParseBool(args, out bool alwaysShowListValue)) {
                        alwaysShowList = alwaysShowListValue;
                    }
                    else {
                        Console.WriteLine("wrong value");
                    }
                    break;
                default:
                    WriteMarkup($"%RED%Unknown command%RED%: {command}");
                    break;
            }
        }

        static bool TryParseBool(string input, out bool result) {
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

        static string ReturnSavedListPlaneText() {
            if (savedAnglesList.Count == 0) {
                return "The list is empty";
            }
            string text = $"--------Saved-List--------" +
                $"\n{ReturnSaveAnglesList()}" +
                $"\n{string.Concat(Enumerable.Repeat("-", 40))}";
            return text;
        }
        static void WriteMarkup(string text) {
            var regex = new System.Text.RegularExpressions.Regex(@"%(\w+)%(.*?)%\1%");
            int lastEnd = 0;

            foreach (System.Text.RegularExpressions.Match match in regex.Matches(text)) {
                Console.Write(text.Substring(lastEnd, match.Index - lastEnd));
                if (Enum.TryParse(match.Groups[1].Value, true, out ConsoleColor color)) {
                    Console.ForegroundColor = color;
                    Console.Write(match.Groups[2].Value);
                    Console.ResetColor();
                }
                else {
                    Console.Write(match.Groups[2].Value);
                }
                lastEnd = match.Index + match.Length;
            }
            Console.Write(text.Substring(lastEnd));
            Console.WriteLine();
        }

        static void Main(string[] args) {
            float km;
            float hozAngle = -1;
            int charges;
            Gun gunSelected;

            Console.WriteLine("Use /help for list of commands");

            while (true) {
                try {
                    // User Selected gun (optional for user)
                    while (true) {
                        Console.WriteLine("Selcet Gun: Left(L) or Right(R) (can be skiped if not needed)");
                        string input = CustomReadLine();

                        if (input.ToLower() == "right" || input.ToLower() == "r") gunSelected = Gun.Right;
                        else if (input.ToLower() == "left" || input.ToLower() == "l") gunSelected = Gun.Left;
                        else gunSelected = Gun.None;
                        break;
                    }

                    // User enters hozAngle (optional for user)
                    while (true) {
                        Console.WriteLine("Set horizontal angle from 0.00 to 360.00 (can be skiped if not needed)");
                        float.TryParse(CustomReadLine(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out hozAngle);
                        string hozAng = hozAngle > 0 ? hozAngle.ToString() : null;
                        if (hozAng != null) {
                            if (hozAngle > 0 && hozAngle <= 360) {
                                break;
                            }
                            else {
                                string errorTextHozAngle = hozAngle > 360 ? $"The horizontal angle can't be bigger than 360.00" 
                                    : $"The horizontal angle can't be smaller than 0";
                                WriteMarkup($"%RED%Error%RED%: {errorTextHozAngle}");
                                continue;
                            }
                        }

                        break;
                    }

                    // User enters distance data in km
                    while (true) {
                        Console.WriteLine("Enter distance in km (min: 0.5 km, max: 30 km):");

                        float.TryParse(CustomReadLine(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out km);

                        if (km < 0.5f || km > 30f) {
                            string errorTextDistance = km < 30 ? $"The Distance can't be smaller than 0.5 km, you entered {km}" 
                                : $"The Distance can't be bigger than 30 km, you entered {km}";
                            WriteMarkup($"%RED%Error%RED%: {errorTextDistance}");
                            continue;
                        }
                        break;
                    }

                    // selected amount of charges to use, but can only select min or max limit, low limit changes dynamically based on distance
                    int minCharges = Get_Min_Charges(km);
                    while (true) {
                        Console.WriteLine($"Enter amout of charges (min: {minCharges}, max: 6)");
                        int.TryParse(CustomReadLine(), out charges);

                        if (charges < minCharges) {
                            Console.WriteLine($"Charges that been entered is to small, min charges are {minCharges}");
                            continue;
                        }

                        if (charges > 6 || charges < 1) {
                            string text = charges > 6 ? "You haved entered bigger number than 6" : "You haved entered smaller number than 1";
                            WriteMarkup($"%RED%Error%RED%: {text}");
                            continue;
                        }
                        break;
                    }


                    // Output
                    float velAngle = Calc_angle(km, charges);
                    string hozAngleConvert = hozAngle > 0 ? hozAngle.ToString() : "null";
                    SaveNewAngle(velAngle, hozAngleConvert, charges, gunSelected);
                    Console.WriteLine(string.Concat(Enumerable.Repeat("-", 40)));
                    Console.WriteLine($"  {gunSelected} gun,\n" +
                        $"  vertical angle {velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\n" +
                        $"  horizontal angle {hozAngleConvert},\n" +
                        $"  charges {charges}\n");
                    if (alwaysShowList) {
                        Console.WriteLine(ReturnSavedListPlaneText());
                    }
                }
                catch (CommandEnteredException) {
                    continue;
                }
            }
        }
    }
}
