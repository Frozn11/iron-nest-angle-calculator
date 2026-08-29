using CalculateAngleViaDistanceIronNest.Commands;
using CalculateAngleViaDistanceIronNest.Data;
using CalculateAngleViaDistanceIronNest.JsonSaveLoad;
using CalculateAngleViaDistanceIronNest.Runtime;
using CalculateAngleViaDistanceIronNest.Utilitys;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CalculateAngleViaDistanceIronNest {
    class Program {
        static async Task Main(string[] args) {
            switch (args.FirstOrDefault()) {
                case "--build-release": BuildApp.RunRelease(); return;
                case "--build-debug": BuildApp.RunDebug(); return;
            }

            JsonSave getJson = JsonManger.GetLoadManger();

            bool alreadyRelaunched = args.Contains("--relaunched");
            if (!alreadyRelaunched && !Utility.IsAlreadyInGoodTerminal() && IsLegacyConsoleMode(getJson)) {
                if (Utility.TryRelaunchInBetterTerminal()) return;
                // no better terminal available fall through and run as-is
            }

            var state = new AppState();
            var runtime = new AppRuntime(state);
            var commandRegistry = new CommandRegistry(state, runtime);
            runtime.SetCommandMap(commandRegistry.BuildCommandMap());
            await runtime.Run();
        }
        static bool IsLegacyConsoleMode(JsonSave jsonSave) {
            if (jsonSave.useLegacyConsole != null) {
                return jsonSave.useLegacyConsole.Value;
            }
            bool wantsModern = AnsiConsole.Confirm("[yellow]Do you want to use the modern console mode?[/]");
            jsonSave.useLegacyConsole = wantsModern;
            JsonManger.SaveJson(jsonSave);
            return jsonSave.useLegacyConsole.Value;
        }
    }
}