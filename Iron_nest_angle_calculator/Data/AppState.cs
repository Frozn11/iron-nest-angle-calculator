using CalculateAngleViaDistanceIronNest.Utilitys;
using System.Collections.Generic;

namespace CalculateAngleViaDistanceIronNest.Data {
    public class AppState {
        public List<SavedAngle> savedAnglesList { get; } = new();
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

        // Console.WriteLine($"{gunSelected} gun, vertical angle set to {velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, horizontal angle set to {hozAngle}, charges needed {charges}\n");
        public string ReturnSaveAnglesList() {
            string text = "";
            for (int i = 0; i < savedAnglesList.Count; i++) {
                SavedAngle savedAngle = savedAnglesList[i];
                text += $"{i}. {ReturnSaveAngle(savedAngle)}";
                if (i + 1 < savedAnglesList.Count) {
                    text += " .\n";
                }
            }
            return text;
        }

        public string ReturnSaveAngle(SavedAngle savedAngle) {
            string text = $"{savedAngle.gunSelected} gun,\n" +
                        $"  vertical angle: {Utility.F(savedAngle.velAngle)},\n" +
                        $"  horizontal angle: {Utility.F(savedAngle.hozAngle)},\n" +
                        $"  charges: {savedAngle.charges},\n" +
                        $"  time to travel: {Utility.F(savedAngle.timeToTrivel)} secondes\n";
            return text;
        }

        public string ReturnSavedListPlaneText() {
            if (savedAnglesList.Count == 0) {
                return "The list is empty";
            }
            string text = $"--------Saved-List--------" +
                $"\n{ReturnSaveAnglesList()}" +
                $"\n{new string('-', 40)}";
            return text;
        }
    }
}
