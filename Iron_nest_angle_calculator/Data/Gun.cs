using System.Text.Json.Serialization;

namespace CalculateAngleViaDistanceIronNest.Data {
    [JsonConverter(typeof(JsonStringEnumConverter<Gun>))]
    public enum Gun {
        None,
        Left,
        Right
    }
}
