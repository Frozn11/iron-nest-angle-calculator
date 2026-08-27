using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateAngleViaDistanceIronNest.Data {
    class AppState {
        public List<SavedAngle> savedAnglesList { get; } = new();
        public int maxSaveList { get; set; } = 6;
        public bool saveList { get; set; } = true;
        public bool alwaysShowList { get; set; } = true;

        public void SaveNewAngle(float velAngle, float? hozAngle, int charges, Gun gun, float timeToTravel) {
            if (!saveList) return;
            if (savedAnglesList.Count >= maxSaveList) savedAnglesList.RemoveAt(0);
            savedAnglesList.Add(new SavedAngle { velAngle = velAngle, hozAngle = hozAngle, charges = charges, gunSelected = gun, timeToTrivel = timeToTravel });
        }

        // Console.WriteLine($"{gunSelected} gun, vertical angle set to {velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, horizontal angle set to {hozAngle}, charges needed {charges}\n");
        public string ReturnSaveAnglesList() {
            string text = "";
            for (int i = 0; i < savedAnglesList.Count; i++) {
                SavedAngle savedAngle = savedAnglesList[i];
                text += $"{i}.{savedAngle.gunSelected} gun,\n" +
                        $"  vertical angle {savedAngle.velAngle.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\n" +
                        $"  horizontal angle {savedAngle.hozAngle},\n" +
                        $"  charges {savedAngle.charges},\n" +
                        $"  time to travel {savedAngle.timeToTrivel.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} secondes\n";
                if (i + 1 < savedAnglesList.Count) {
                    text += " .\n";
                }
            }
            return text;
        }
        public string ReturnSavedListPlaneText() {
            if (savedAnglesList.Count == 0) {
                return "The list is empty";
            }
            string text = $"--------Saved-List--------" +
                $"\n{ReturnSaveAnglesList()}" +
                $"\n{string.Concat(Enumerable.Repeat("-", 40))}";
            return text;
        }
    }
}
