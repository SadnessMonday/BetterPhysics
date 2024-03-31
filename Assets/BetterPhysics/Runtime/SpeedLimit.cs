using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    [Serializable]
    public struct SpeedLimit {
        [SerializeField] private LimitType limitType;
        [SerializeField] Directionality directionality;
        [SerializeField] private float scalarLimit;
        [SerializeField] bool asymmetrical;
        [SerializeField] Vector3 min;
        [SerializeField] Vector3 max;
        [SerializeField] private Bool3 axisLimited;

        public Bool3 AxisLimited => axisLimited;

        public LimitType LimitType => limitType;
        
        public Directionality Directionality => directionality;

        public float ScalarLimit => scalarLimit;

        public bool Asymmetrical => asymmetrical;

        public Vector3 Min => min;

        public Vector3 Max => max;

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

        public void SetAxisLimited(Bool3 newLimits) {
            axisLimited = newLimits;
        }

        public static readonly SpeedLimit Default = new() {
            limitType = LimitType.None,
            directionality = Directionality.Omnidirectional,
            asymmetrical = false,
            scalarLimit = 10,
            min = -5 * Vector3.one,
            max = 5 * Vector3.one,
            axisLimited = Bool3.True,
        };
        
        public static readonly SpeedLimit Soft = new() {
            limitType = LimitType.Soft,
            directionality = Directionality.Omnidirectional,
            asymmetrical = false,
            scalarLimit = 10,
            min = -5 * Vector3.one,
            max = 5 * Vector3.one,
            axisLimited = Bool3.True,
        };
        
        public static readonly SpeedLimit Hard = new() {
            limitType = LimitType.Hard,
            directionality = Directionality.Omnidirectional,
            asymmetrical = false,
            scalarLimit = 10,
            min = -5 * Vector3.one,
            max = 5 * Vector3.one,
            axisLimited = Bool3.True,
        };
        

        public void SetOmniDirectionalLimit(float maxSpeed) {
            directionality = Directionality.Omnidirectional;
            scalarLimit = maxSpeed;
            
            Validate();
        }

        public void SetWorldLimits(in Bool3 axisLimited, in Vector3 min, in Vector3 max) {
            this.axisLimited = axisLimited;
            directionality = Directionality.WorldAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
        }
        
        public void SetWorldLimits(in Bool3 axisLimited, in Vector3 limits) {
            this.axisLimited = axisLimited;
            directionality = Directionality.WorldAxes;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetWorldLimits(in Vector3 limits) {
            directionality = Directionality.WorldAxes;
            axisLimited = Bool3.True;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetWorldLimits(in Vector3 min, in Vector3 max) {
            directionality = Directionality.WorldAxes;
            axisLimited = Bool3.True;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }
        
        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 min, in Vector3 max) {
            this.axisLimited = axisLimited;
            directionality = Directionality.LocalAxes;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }

        public void SetLocalLimits(in Bool3 axisLimited, in Vector3 limits) {
            this.axisLimited = axisLimited;
            directionality = Directionality.LocalAxes;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetLocalLimits(in Vector3 min, in Vector3 max) {
            directionality = Directionality.LocalAxes;
            axisLimited = Bool3.True;
            asymmetrical = true;
            this.min = min;
            this.max = max;
            Validate();
        }

        public void SetLocalLimits(in Vector3 limits) {
            directionality = Directionality.LocalAxes;
            axisLimited = Bool3.True;
            asymmetrical = false;
            max = limits;
            Validate();
        }

        public void SetLimitType(LimitType limitType) {
            this.limitType = limitType;
        }

        public static SpeedLimit SymmetricalLocalLimits(LimitType limitType, Vector3 limits) {
            SpeedLimit toReturn = Default;
            toReturn.SetLimitType(limitType);
            toReturn.SetLocalLimits(limits);
            return toReturn;
        }

        public static SpeedLimit SymmetricalWorldLimits(LimitType limitType, Vector3 limits) {
            SpeedLimit toReturn = Default;
            toReturn.SetLimitType(limitType);
            toReturn.SetWorldLimits(limits);
            return toReturn;
        }

        public static SpeedLimit OmnidirectionalLimit(LimitType limitType, float desiredSpeed) {
            SpeedLimit toReturn = Default;
            toReturn.SetLimitType(limitType);
            toReturn.SetOmniDirectionalLimit(desiredSpeed);
            return toReturn;
        }

        void Validate() {
            switch (directionality) {
                case Directionality.Omnidirectional:
                    if (scalarLimit < 0) {
                        throw new ArgumentException($"Omnidirectional limit must not be negative. Got {scalarLimit}");
                    }
                    break;
                case Directionality.WorldAxes:
                case Directionality.LocalAxes:
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