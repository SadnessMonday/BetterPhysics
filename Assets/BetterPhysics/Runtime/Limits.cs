using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [Serializable]
    public struct Limits {
        [SerializeField] LimitType limitType;
        [SerializeField] private float scalarLimit;
        [SerializeField] bool asymmetrical;
        [SerializeField] Vector3 min;
        [SerializeField] Vector3 max;
        [SerializeField] bool xLimited;
        [SerializeField] bool yLimited;
        [SerializeField] bool zLimited;

        public LimitType LimitType {
            get => limitType;
            set => limitType = value;
        }

        public float ScalarLimit {
            get => scalarLimit;
            set => scalarLimit = value;
        }

        public bool Asymmetrical {
            get => asymmetrical;
            set => asymmetrical = value;
        }

        public Vector3 Min {
            get => min;
            set => min = value;
        }

        public Vector3 Max {
            get => max;
            set => max = value;
        }

        public bool XLimited {
            get => xLimited;
            set => xLimited = value;
        }

        public bool YLimited {
            get => yLimited;
            set => yLimited = value;
        }

        public bool ZLimited {
            get => zLimited;
            set => zLimited = value;
        }

        public static Limits Default {
            get {
                Limits l = default;
                l.limitType = LimitType.None;
                l.asymmetrical = false;
                l.scalarLimit = 10;
                l.min = -5 * Vector3.one;
                l.max = 5 * Vector3.one;
                return l;
            }
        }
    }
}