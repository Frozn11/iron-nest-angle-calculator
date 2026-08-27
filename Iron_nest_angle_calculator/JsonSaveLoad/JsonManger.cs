using CalculateAngleViaDistanceIronNest.Data;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalculateAngleViaDistanceIronNest.JsonSaveLoad {
    public class JsonSaveLoad {
        public bool useLegacyConsole { get; set; } = false;
        public Gun selectedGun { get; set; }
    }
    class JsonManger {
        static string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IronNestCalcSave");
        static string saveNameFilePath = Path.Combine(appDataDir, "save.json");
        static readonly JsonSerializerOptions options = new () {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        static readonly JsonSaveLoad save = new JsonSaveLoad {
            useLegacyConsole = false,
            selectedGun = Gun.Left
        };

        public static void CheckFolderFile() {

            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(save, options);

            if (!Directory.Exists(appDataDir)) {
                Directory.CreateDirectory(appDataDir);
                File.WriteAllBytes(saveNameFilePath, jsonUtf8Bytes);
            }
            if (!File.Exists(saveNameFilePath)) {
                File.WriteAllBytes(saveNameFilePath, jsonUtf8Bytes);
            }
        }

        public void SaveManger(JsonSaveLoad save) {
            CheckFolderFile();
            var options = new JsonSerializerOptions { WriteIndented = true };
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(save, options);
        }
        public void LoadManger() {
            if (!Directory.Exists(appDataDir) || !File.Exists(saveNameFilePath)) return;

            byte[] jsonUtf8Bytes = File.ReadAllBytes(saveNameFilePath);
            var utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
            JsonSaveLoad deserializedWeatherForecast = JsonSerializer.Deserialize<JsonSaveLoad>(ref utf8Reader)!;
        }
    }
}
