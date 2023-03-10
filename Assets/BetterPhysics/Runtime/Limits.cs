using System;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [Serializable]
    public struct Limits {
        [SerializeField] bool asymmetrical;
        [SerializeField] Vector3 min;
        [SerializeField] Vector3 max;
        [SerializeField] bool xLimited;
        [SerializeField] bool yLimited;
        [SerializeField] bool zLimited;

        public static Limits Default {
            get {
                Limits l = default;
                l.asymmetrical = false;
                return l;
            }
        }
    }
}