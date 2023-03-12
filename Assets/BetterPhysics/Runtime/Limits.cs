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
        [SerializeField] private Bool3 axisLimited;

        public Bool3 AxisLimited => axisLimited;
        
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
            get => axisLimited.x;
            set => axisLimited.x = value;
        }

        public bool YLimited {
            get => axisLimited.y;
            set => axisLimited.y = value;
        }

        public bool ZLimited {
            get => axisLimited.z;
            set => axisLimited.z = value;
        }

        public bool IsAxisLimited(int axis) {
            return axisLimited[axis];
        }

        public void SetAxisLimited(int axis, bool limited) {
            axisLimited[axis] = limited;
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

        public void SetOmniDirectionalLimit(float maxSpeed) {
            limitType = LimitType.Omnidirectional;
            scalarLimit = maxSpeed;
        }

        public void SetWorldLimits(in Bool3 axisLimited, in Vector3 min, in Vector3 max) {
            this.axisLimited = axisLimited;
            limitType = LimitType.WorldAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
        }
        
        public void SetWorldLimits(in Bool3 axisLimited, in Vector3 limits) {
            this.axisLimited = axisLimited;
            limitType = LimitType.WorldAxes;
            asymmetrical = false;
            max = limits;
        }

        public void SetWorldLimits(in Vector3 limits) {
            limitType = LimitType.WorldAxes;
            asymmetrical = false;
            max = limits;
        }

        public void SetWorldLimits(in Vector3 min, in Vector3 max) {
            limitType = LimitType.WorldAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
        }
        
        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 min, in Vector3 max) {
            this.axisLimited = axisLimited;
            limitType = LimitType.LocalAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
        }

        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 limits) {
            this.axisLimited = axisLimited;
            limitType = LimitType.LocalAxes;
            asymmetrical = false;
            max = limits;
        }

        public void SetLocalLimits(in Vector3 min, in Vector3 max) {
            this.SetLocalLimits(this.axisLimited, min, max);
        }

        public void SetLocalLimits(in Vector3 limits) {
            limitType = LimitType.LocalAxes;
            asymmetrical = false;
            max = limits;
        }

        public static Limits SymmetricalLocalLimits(Vector3 limits) {
            Limits toReturn = Default;
            toReturn.SetLocalLimits(limits);
            return toReturn;
        }

        public static Limits SymmetricalWorldLimits(Vector3 limits) {
            Limits toReturn = Default;
            toReturn.SetWorldLimits(limits);
            return toReturn;
        }

        public static Limits OmnidirectionalLimit(float desiredSpeed) {
            Limits toReturn = Default;
            toReturn.SetOmniDirectionalLimit(desiredSpeed);
            return toReturn;
        }
    }
}