using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace CalculateAngleViaDistanceIronNest.Utilitys {
    public class Utility {
        public static string F(float v) => v.ToString("F2", CultureInfo.InvariantCulture);
        public static string F(float? v) => v.HasValue ? F(v.Value) : "null";
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

        // Check if the charges is within the limit of 1 to 6
        public static bool CheckChargeLimit(int charges) {
            if (charges > 6 || charges < 1) {
                return false;
            }
            return true;
        }
        public static string GetChargeLimitText(int charges) {
            string text = charges > 6 ? "You entered a number bigger than 6" : charges < 1 ? "You entered a number smaller than 1" : "";
            return text;
        }

        // Check if the distance is within the limit of 0.0005 to 30
        public static bool CheckDistanceLimit(float distance) {
            if (distance > 30 || distance < 0.0005) {
                return false;
            }
            return true;
        }
        public static string GetDistanceLimitText(float distance) {
            string text = distance > 30 ? $"Distance can't be bigger than 30.00 km, you entered {distance}" : distance < 0.0005 ? $"Distance can't be smaller than 0.0005 km, you entered {distance}" : "";
            return text;
        }

        // Check if the horizontal angle is within the limit of 0 to 360
        public static bool CheckHozAngleLimit(float hozAngle) {
            if (hozAngle > 360 || hozAngle < 0) {
                return false;
            }
            return true;
        }
        public static string GetHozAngleLimitText(float hozAngle) {
            string text = hozAngle > 360 ? $"Horizontal angle can't be bigger than 360.00, you entered {hozAngle}" : hozAngle < 0 ? $"Horizontal angle can't be smaller than 0.00, you entered {hozAngle}" : "";
            return text;
        }

        // Check if the application is already running in a known-good terminal emulator
        // Are we already running somewhere that's known-good?
        public static bool IsAlreadyInGoodTerminal() {
            if (OperatingSystem.IsWindows())
                return Environment.GetEnvironmentVariable("WT_SESSION") != null;

            if (OperatingSystem.IsMacOS())
                return Environment.GetEnvironmentVariable("TERM_PROGRAM") != null; // Apple_Terminal, iTerm.app, vscode, etc.

            if (OperatingSystem.IsLinux())
                return Environment.GetEnvironmentVariable("TERM") != "linux"; // "linux" = raw virtual console, everything else = a real emulator

            return true; // unknown platform, don't try to be clever
        }
        public static bool TryRelaunchInBetterTerminal() {
            string exePath = Environment.ProcessPath;
            string exeArgs = $"\"{exePath}\" --relaunched";

            if (OperatingSystem.IsWindows())
                return TryLaunch("wt.exe", exeArgs);

            if (OperatingSystem.IsMacOS()) {
                // Terminal.app can't take an exe path as an argument directly,
                // so drive it through osascript.
                string escaped = exeArgs.Replace("\"", "\\\"");
                string script = $"tell application \"Terminal\" to do script \"{escaped}\"";
                return TryLaunch("osascript", $"-e '{script}'");
            }

            if (OperatingSystem.IsLinux()) {
                // No single "the terminal" on Linux — probe common ones in order.
                (string cmd, string argsFormat)[] candidates = [
                    ("x-terminal-emulator", "-e {0}"),
                    ("gnome-terminal",      "-- {0}"),
                    ("konsole",             "-e {0}"),
                    ("xfce4-terminal",      "-e \"{0}\""),
                    ("alacritty",           "-e {0}"),
                    ("kitty",               "{0}"),
                    ("xterm",               "-e {0}"),
                ];
                foreach (var (cmd, fmt) in candidates) {
                    if (IsCommandAvailable(cmd) && TryLaunch(cmd, string.Format(fmt, exeArgs)))
                        return true;
                }
                return false;
            }

            return false;
        }
        public static bool IsCommandAvailable(string command) {
            try {
                var psi = new ProcessStartInfo {
                    FileName = "which",
                    Arguments = command,
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
        public static bool TryLaunch(string fileName, string arguments) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                return true;
            }
            catch {
                return false;
            }
        }

        public static string[] GetVersion() {
            string raw = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "unknown+unknown";
            string[] parts = raw.Split('+');
            string version = parts[0];
            string hash = parts.Length > 1 ? parts[1] : "unknown";

            return [version, hash];
        }
    }
}
