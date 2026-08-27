using System.Globalization;

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
    }
}
