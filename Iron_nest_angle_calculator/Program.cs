using Calculate_angle_via_distance_Iron_Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Iron_nest_angle_calculator {
    class Program {
        static List<SavedAngle> savedAnglesList = new List<SavedAngle>();

        // got this formula from Reddit https://www.reddit.com/r/IronNest/comments/1vjgvbb/math/
        // Elevation = (Distance in km x  12) / charges
        static float Calc_angle(float distnace, int charges) {
            float alngle = (distnace * 12) / charges;
            return alngle;
        }

        // the min amout charges needed to shoot 
        // from 5 km to 30 km
        //      charges 1 = 5
        //      charges 2 = 10
        //      charges 3 = 15
        //      charges 4 = 20
        //      charges 5 = 25
        //      charges 6 = 30
        static int Get_Min_Charges(float km){
            int minCharges = 1;

            if (km > 5 && km < 15) minCharges = 2;
            else if (km > 10 && km < 20) minCharges = 3;
            else if (km > 15 && km < 25) minCharges = 4;
            else if (km > 20 && km < 30) minCharges = 5;
            else if (km > 25) minCharges = 6;

            return minCharges;
        }

        static void SaveNewAngle(float angle, int charges, Gun gunSelected) {
            SavedAngle newSavedAngle = new SavedAngle();
            newSavedAngle.angle = angle;
            newSavedAngle.charges = charges;
            newSavedAngle.gunSelected = gunSelected;
            savedAnglesList.Add(newSavedAngle);
        }

         // Console.WriteLine($"{gunSelected} gun, set angle to {angle}, charges needed {charges}");
         static string ReturnSaveAnglesList() {
            string text = "";
            for (int i = 0; i < savedAnglesList.Count; i++) {
                SavedAngle savedAngle = savedAnglesList[i];
                text += $"{i}. {savedAngle.gunSelected}, set angle to {savedAngle.angle}, charges needed {savedAngle.charges}\n";
            }
            return text;
         }

        static void Main(string[] args) {
            float km;
            int charges;
            Gun gunSelected;

            Console.WriteLine("Use /help for list of commands");

            while (true) {
                // User Selected gun
                while (true) {
                    Console.WriteLine("Selcet Gun: Left(L) or Right(R) (can be skiped if not needed)");
                    string input = Console.ReadLine();

                    if (input.ToLower() == "right" || input.ToLower() == "r") gunSelected = Gun.Right;
                    else if (input.ToLower() == "left" || input.ToLower() == "l") gunSelected = Gun.Left;
                    else gunSelected = Gun.None;
                    break;
                }

                // use enters distance data in km
                while (true) {
                    Console.WriteLine("Enter distance in km (min: 0.5 km):");

                    float.TryParse(Console.ReadLine(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out km);

                    if (km < 0.5f) {
                        Console.WriteLine($"Error: The Distance can't be smaller than 0.5 km, you entered {km}"); 
                        continue;
                    }
                    break;
                }

                // selected amount of charges to use, but can only select min or max limit, low limit changes dynamically based on distance
                int minCharges = Get_Min_Charges(km); 
                while (true) {
                    Console.WriteLine($"Enter amout of charges (min: {minCharges}, max: 6)");
                    int.TryParse(Console.ReadLine(), out charges);

                    if (charges < minCharges) {
                        Console.WriteLine($"Charges that been entered is to small, min charges are {minCharges}");
                        continue;
                    }

                    if (charges > 6 || charges < 1) {
                        string text = charges > 6 ? "You haved entered bigger number than 6" : "You haved entered smaller number than 1";
                        Console.WriteLine($"Error: {text}");
                        continue;
                    }
                    break;
                }


                // Output
                float angle = Calc_angle(km, charges);
                SaveNewAngle(angle, charges, gunSelected);

                Console.WriteLine($"{gunSelected} gun, set angle to {angle}, charges needed {charges}\n");
                Console.WriteLine("--------Saved-List--------");
                Console.WriteLine(ReturnSaveAnglesList());
                Console.WriteLine(string.Concat(Enumerable.Repeat("-", 40)));
                Console.WriteLine();

            }
        }
    }
}
