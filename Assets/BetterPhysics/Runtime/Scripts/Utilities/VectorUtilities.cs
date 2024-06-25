namespace SadnessMonday.BetterPhysics.Utilities {
    public class MathUtilities {
        public static float DirectionalClamp(float value, float a, float b) {
            if (a > b) {
                // B is smaller
                if (value < b) return b;
                if (value > a) return a;
                return value;
            }

            // A is smaller
            if (value < a) return a;
            if (value > b) return b;
            return value;
        }
    }
}