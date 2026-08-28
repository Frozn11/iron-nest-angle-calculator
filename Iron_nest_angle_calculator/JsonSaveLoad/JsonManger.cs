using System;
using System.IO;
using System.Text.Json;

namespace CalculateAngleViaDistanceIronNest.JsonSaveLoad {
    public class JsonSave {
        public bool? useLegacyConsole { get; set; } = null;
    }
    class JsonManger {
        static string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IronNestCalcSave");
        static string saveNameFilePath = Path.Combine(appDataDir, "save.json");
        static readonly JsonSerializerOptions options = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        static readonly JsonSave save = new();

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

        public static void SaveJson(JsonSave save) {
            CheckFolderFile();
            var options = new JsonSerializerOptions { WriteIndented = true };
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(save, options);
            File.WriteAllBytes(saveNameFilePath, jsonUtf8Bytes);
        }
        public static JsonSave GetLoadManger() {
            if (!Directory.Exists(appDataDir) || !File.Exists(saveNameFilePath)) {
                CheckFolderFile();
            }

            byte[] jsonUtf8Bytes = File.ReadAllBytes(saveNameFilePath);
            var utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
            JsonSave deserializedWeatherForecast = JsonSerializer.Deserialize<JsonSave>(ref utf8Reader)!;
            return deserializedWeatherForecast;
        }
    }
}
