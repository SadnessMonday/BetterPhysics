using System;
using UnityEditor;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [Serializable]
    public struct Limits {
        [SerializeField] bool symmetrical;
        [SerializeField] Vector3 min;
        [SerializeField] Vector3 max;
        [SerializeField] bool xLimited;
        [SerializeField] bool yLimited;
        [SerializeField] bool zLimited;

        public static Limits Default {
            get {
                Limits l = default;
                l.symmetrical = true;
                return l;
            }
        }
    }
}