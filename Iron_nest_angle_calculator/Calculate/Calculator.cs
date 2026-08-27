using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("CalculateAngleViaDistanceIronNest.Tests")]

namespace CalculateAngleViaDistanceIronNest.Calculate {
    class Calculator {
        // got this formula from Reddit https://www.reddit.com/r/IronNest/comments/1vjgvbb/math/
        // also can be get from Iron Nest wiki https://ironnestwiki.com/calculator
        // Elevation = (Distance in km x  12) / charges
        public static float CalcAngle(float distance, int charges) {
            float angle = (distance * 12) / charges;
            angle = MathF.Floor(angle * 100f) / 100f;
            return angle;
        }

        // got formula from Iron Nest wiki https://ironnestwiki.com/calculator
        // SHELL FLIGHT TIME = TARGET DISTANCE ÷ ADJUSTED SHELL SPEED
        // u = (POWDER CHARGES − 1) ÷ 5
        // ADJUSTED SHELL SPEED = 0.7 × [0.3 + 0.7 × (3u² − 2u³)]
        public static float CalcTimeTravel(float distance, int charges) {
            float u = (charges - 1) / 5f;
            float adjShellSpeed = 0.7f * (0.3f + 0.7f * (3f * MathF.Pow(u, 2f) - 2f * MathF.Pow(u, 3)));
            return distance / adjShellSpeed;
        }

        // the min amout charges needed to shoot 
        // from 5 km to 30 km
        //      charges 1 = 5
        //      charges 2 = 10
        //      charges 3 = 15
        //      charges 4 = 20
        //      charges 5 = 25
        //      charges 6 = 30
        // NOTE: returns -1 if no charge count (1-6) gets the angle to <= 60.
        // This can only happen if km > 30, but ReadDistance() in CommandRegistry
        // caps input at 30km, so -1 never actually returns today.
        // If that cap is ever removed or raised, ReadCharges() must check for -1.
        public static int GetMinCharges(float km) {
            int minCharges = 1;

            while (CalcAngle(km, minCharges) > 60) {
                if (minCharges > 6) return -1; // Return an error code
                minCharges++;
            }
            return minCharges;
        }
    }
}
