using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoShift
{
    internal class Helper
    {
        public static double RadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return degrees;
        }

        public static Dictionary<string, int> FERScale =
            new Dictionary<string, int>() {
                {"surprise", 7},
                {"happiness", 6},
                {"neutrality", 5},
                {"sadness", 4},
                {"contempt", 3},
                {"fear", 2},
                {"anger", 1},
                {"disgust", 0},
                // I made this up, verify with team
            };

    }

}
