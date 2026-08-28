using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CalculateAngleViaDistanceIronNest.Utilitys {
    class UpdateChecker {
        static string owner = "Frozn11";
        static string repo = "iron-nest-angle-calculator";
        public static async Task<(bool Outdated, string lastVersion)> CheckForUpdates(string currentVersion) {
            try {
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("IronNestCalc", currentVersion));
                httpClient.Timeout = TimeSpan.FromSeconds(3);

                string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                using HttpResponseMessage response = await httpClient.GetAsync(url);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode forecastNode = JsonNode.Parse(jsonResponse)!;
                JsonNode getVersionNode = forecastNode!["tag_name"]!;

                string lastVersion = getVersionNode!.ToString().TrimStart('v');
                bool Outdated = new Version(lastVersion) > new Version(currentVersion);

                return (Outdated, lastVersion);

            }
            catch {
                return (false, "Error checking for updates");
            }
        }
        public static string GetVersionFromGit() {
            try {
                var psi = new ProcessStartInfo {
                    FileName = "git",
                    Arguments = "describe --tags",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output)) {
                    var parts = output.TrimStart('v').Split('-');
                    if (parts.Length == 1) {
                        return parts[0];
                    }
                    return $"{parts[0]}-InDev";
                }
            }
            catch { }
            return "0.0.0InDev";
        }
    }
}
