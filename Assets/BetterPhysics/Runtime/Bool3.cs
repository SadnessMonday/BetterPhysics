using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [Serializable]
    public struct Bool3 {
        public bool x, y, z;

        public bool this[int axis] {
            get {
                switch (axis) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new Exception($"Invalid axis {axis}. Must be in range [0, 2]");
                }
            }

            set {
                switch (axis) {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new Exception($"Invalid axis {axis}. Must be in range [0, 2]");
                }
            }
        }
    }
}