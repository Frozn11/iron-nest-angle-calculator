using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace CalculateAngleViaDistanceIronNest {
    class BuildApp {
        // Executable name used in the .csproj (AssemblyName), used to name zips
        const string appName = "IronNestCalc";
        static Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        // Runtime identifiers to build for
        static readonly string[] runtimeIds = {
            "win-x64",
            "win-x86",
            "linux-x64",
            "osx-x64",
            "osx-arm64"
        };

        public static void RunRelease() {
            string projectDir = FindProjectDirectory();
            string downloadsPath = GetDownloadsPath();

            if (projectDir == null) {
                Console.WriteLine("Could not locate .csproj file. Aborting.");
                return;
            }

            string releasesDir = Path.Combine(downloadsPath, "releases");
            Directory.CreateDirectory(releasesDir);

            foreach (string rid in runtimeIds) {
                Publish(projectDir, releasesDir, rid, selfContained: true, label: "Standalone");
                Publish(projectDir, releasesDir, rid, selfContained: false, label: "Framework");
            }

            Console.WriteLine("\nAll builds complete. Zips are in: " + releasesDir);
        }

        public static void RunDebug() {
            string projectDir = FindProjectDirectory();
            string downloadsPath = GetDownloadsPath();
            if (projectDir == null) {
                Console.WriteLine("Could not locate .csproj file. Aborting.");
                return;
            }
            string releasesDir = Path.Combine(downloadsPath, "debug");
            Directory.CreateDirectory(releasesDir);
            // Build only for the current platform
            Publish(projectDir, releasesDir, runtimeIds[0], selfContained: true, label: "Standalone");
            Publish(projectDir, releasesDir, runtimeIds[0], selfContained: false, label: "Framework");
        }

        static void Publish(string projectDir, string releasesDir, string rid, bool selfContained, string label) {
            string folderName = $"{appName}-{rid}-{label}";
            string outputDir = Path.Combine(releasesDir, $"{rid}-{label.ToLower()}");
            string zipPath = Path.Combine(releasesDir, $"{folderName}.zip");
            string version = Utilitys.UpdateChecker.GetVersionFromGit();

            Console.WriteLine($"\nBuilding {rid} ({label}) — version {version}...");

            var psi = new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"publish \"{projectDir}\" -c Release -r {rid} " +
                            $"--self-contained {(selfContained ? "true" : "false")} " +
                            $"-p:Version={version} " +
                            $"-p:PublishSingleFile=true -o \"{outputDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi)) {
                process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0) {
                    Console.WriteLine($"Build failed for {rid} ({label}), exit code {process.ExitCode}");
                    return;
                }
            }

            if (File.Exists(zipPath)) {
                File.Delete(zipPath);
            }

            CreateZipWithRootFolder(outputDir, zipPath, folderName);
            Console.WriteLine($"Zipped: {zipPath}");
        }

        static void CreateZipWithRootFolder(string sourceDir, string destinationZip, string rootFolderName) {
            using (var zipStream = new FileStream(destinationZip, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                foreach (string filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)) {
                    string relativePath = Path.GetRelativePath(sourceDir, filePath);
                    string entryName = Path.Combine(rootFolderName, relativePath).Replace('\\', '/');
                    archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        // Walks up from the running executable's directory (bin/Debug/net.../)
        // until it finds a folder containing a .csproj file.
        static string FindProjectDirectory() {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (dir != null) {
                if (dir.GetFiles("*.csproj").Length > 0) {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }

            return null;
        }
        public static string GetDownloadsPath() {
            IntPtr pathPtr = IntPtr.Zero;
            try {
                SHGetKnownFolderPath(ref Downloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }
    }
}