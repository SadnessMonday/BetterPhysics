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

        public Bool3 AxisLimited {
            get => axisLimited;
        }
        
        public LimitType LimitType {
            get => limitType;
        }

        public float ScalarLimit {
            get => scalarLimit;
        }

        public bool Asymmetrical {
            get => asymmetrical;
        }

        public Vector3 Min {
            get => min;
        }

        public Vector3 Max {
            get => max;
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

        public static readonly Limits Default = new() {
            limitType = LimitType.None,
            asymmetrical = false,
            scalarLimit = 10,
            min = -5 * Vector3.one,
            max = 5 * Vector3.one,
            axisLimited = Bool3.True,
        };

        public void SetOmniDirectionalLimit(float maxSpeed) {
            limitType = LimitType.Omnidirectional;
            scalarLimit = maxSpeed;
            
            Validate();
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
            Validate();
        }

        public void SetWorldLimits(in Vector3 limits) {
            limitType = LimitType.WorldAxes;
            axisLimited = Bool3.True;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetWorldLimits(in Vector3 min, in Vector3 max) {
            limitType = LimitType.WorldAxes;
            axisLimited = Bool3.True;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }
        
        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 min, in Vector3 max) {
            this.axisLimited = axisLimited;
            limitType = LimitType.LocalAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }

        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 limits) {
            this.axisLimited = axisLimited;
            limitType = LimitType.LocalAxes;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetLocalLimits(in Vector3 min, in Vector3 max) {
            limitType = LimitType.LocalAxes;
            axisLimited = Bool3.True;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }

        public void SetLocalLimits(in Vector3 limits) {
            limitType = LimitType.LocalAxes;
            axisLimited = Bool3.True;
            asymmetrical = false;
            max = limits;
            Validate();
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

        void Validate() {
            switch (limitType) {
                case LimitType.None:
                    break;
                case LimitType.Omnidirectional:
                    if (scalarLimit < 0) {
                        throw new ArgumentException($"Omnidirectional limit must not be negative. Got {scalarLimit}");
                    }
                    break;
                case LimitType.WorldAxes:
                case LimitType.LocalAxes:
                    if (!asymmetrical) {
                        if (max.x < 0 || max.y < 0 || max.z < 0) {
                            throw new ArgumentException($"Symmetrical limits must not be negative. Got {max}");
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}