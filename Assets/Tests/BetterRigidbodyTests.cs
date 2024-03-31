using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SadnessMonday.BetterPhysics.Tests {
    public class BetterRigidbodyTests {
        private const float MarginOfError = 0.0005f;
        private const float RootTwo = 1.41421356237f;
        private const float SineFortyFive = 0.707106781186548f;
        private static int _testNo = 0;

        public class AddForceTestArgs {
            private readonly int _testNumber;
            public List<SpeedLimit> limits = new();
            public ForceMode ForceMode = ForceMode.VelocityChange;
            public Vector3 StartVel = Vector3.zero;
            public Vector3 StartLocalVel = Vector3.zero;
            public Vector3 ForceVec = Vector3.zero;
            public Vector3 LocalForceVec = Vector3.zero;
            public Vector3 WithoutLimitForceVec = Vector3.zero;
            
            public Vector3 Orientation = Vector3.forward;
            public Vector3 ExpectedVelocity = Vector3.zero;
            public bool ExpectsLocalVelocity { get; private set; } = false;
            public bool ExpectsWorldVelocity { get; private set; } = false;

            private bool _hasStartLocalVelocity = false;
            private bool _hasStartWorldVelocity = false;
            private bool _hasLocalForce = false;
            private bool _hasWorldForce = false;
            private bool _hasWorldForceWithoutLimit = false;
            
            public AddForceTestArgs() {
                this._testNumber = _testNo++;
            }

            public override string ToString() => $"Test no. {_testNumber}";

            public AddForceTestArgs WithLimit(SpeedLimit limit) {
                limits.Add(limit);
                return this;
            }

            public AddForceTestArgs WithForceMode(ForceMode mode) {
                ForceMode = mode;
                return this;
            }

            public AddForceTestArgs WithStartVel(Vector3 startVel) {
                if (_hasStartLocalVelocity) throw new Exception("Can only have one type of starting velocity");
                StartVel = startVel;
                _hasStartWorldVelocity = true;
                return this;
            }

            public AddForceTestArgs WithStartLocalVel(Vector3 startVel) {
                if (_hasStartWorldVelocity) throw new Exception("Can only have one type of starting velocity");
                StartLocalVel = startVel;
                _hasStartLocalVelocity = true;
                return this;
            }

            public AddForceTestArgs WithForceVec(Vector3 forceVec) {
                // clear out local force
                _hasLocalForce = false;
                _hasWorldForceWithoutLimit = false;

                ForceVec = forceVec;
                _hasWorldForce = true;
                return this;
            }

            public AddForceTestArgs WithForceVecWithoutLimit(Vector3 forceVec) {
                // clear out local force
                _hasLocalForce = false;
                _hasWorldForce = false;

                WithoutLimitForceVec = forceVec;
                _hasWorldForceWithoutLimit = true;
                return this;
            }

            public AddForceTestArgs WithLocalForceVec(Vector3 forceVec) {
                // clear out world force
                _hasWorldForce = false;
                _hasWorldForceWithoutLimit = false;

                LocalForceVec = forceVec;
                _hasLocalForce = true;
                return this;
            }

            public AddForceTestArgs WithOrientation(Vector3 orientation) {
                Orientation = orientation;
                return this;
            }

            public AddForceTestArgs WithExpectedLocalVelocity(Vector3 expectedVal) {
                ExpectedVelocity = expectedVal;
                ExpectsLocalVelocity = true;
                return this;
            }

            public AddForceTestArgs WithExpectedVelocity(Vector3 expectedVel) {
                ExpectedVelocity = expectedVel;
                ExpectsWorldVelocity = true;
                return this;
            }

            public void Prepare(BetterRigidbody brb) {
                brb.limits = new List<SpeedLimit>(limits);
                brb.rotation = Quaternion.Euler(Orientation);
                if (_hasStartLocalVelocity) {
                    brb.LocalVelocity = StartLocalVel;
                }

                if (_hasStartWorldVelocity) {
                    brb.Velocity = StartVel;
                }
            }

            public void ApplyForce(BetterRigidbody brb) {
                if (_hasWorldForce) brb.AddForce(ForceVec, ForceMode);
                if (_hasLocalForce) brb.AddRelativeForce(LocalForceVec, ForceMode);
                if (_hasWorldForceWithoutLimit) brb.AddForceWithoutLimit(WithoutLimitForceVec, ForceMode);
            }
        }

        static AddForceTestArgs SoftScalarArgs(float omniLimit) {
            SpeedLimit limit = SpeedLimit.Soft;
            limit.SetOmniDirectionalLimit(omniLimit);
            return new AddForceTestArgs().WithLimit(limit).WithForceVec(Vector3.right * 10);
        }

        static AddForceTestArgs SoftWorldArgs(Vector3 limit) {
            return new AddForceTestArgs()
                .WithLimit(SpeedLimit.SymmetricalWorldLimits(LimitType.Soft, limit))
                .WithForceVec(Vector3.one * 10);
        }

        static AddForceTestArgs SoftLocalArgs(Vector3 limit) {
            return new AddForceTestArgs()
                .WithOrientation(Vector3.one)
                .WithLimit(SpeedLimit.SymmetricalLocalLimits(LimitType.Soft, limit))
                .WithLocalForceVec(Vector3.one * 10);
        }

        static AddForceTestArgs[] SoftLimitsCases() {
            _testNo = 1;
            return new[] {
                // Omnidirectional tests
                SoftScalarArgs(float.PositiveInfinity).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(float.NaN).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(0).WithExpectedVelocity(Vector3.zero),
                SoftScalarArgs(5).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithForceVec(new(10, 10, 0)) // 5
                    .WithExpectedVelocity(new(SineFortyFive * 5, SineFortyFive * 5, 0)),
                SoftScalarArgs(5).WithStartVel(Vector3.left * 10).WithExpectedVelocity(Vector3.zero),
                SoftScalarArgs(5).WithStartVel(Vector3.left * 5).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector3.zero).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector3.right * 10).WithExpectedVelocity(Vector3.right * 10),
                // World axis tests 
                SoftWorldArgs(Vector3.one * float.PositiveInfinity).WithExpectedVelocity(Vector3.one * 10), // 10
                SoftWorldArgs(Vector3.one * float.NaN).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.zero).WithExpectedVelocity(Vector3.zero), // 12
                SoftWorldArgs(Vector3.one * 5).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * -10).WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * -5).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.zero).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * 10).WithExpectedVelocity(Vector3.one * 10),
                // Local axis tests
                SoftLocalArgs(Vector3.one * float.PositiveInfinity).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one * float.NaN).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.zero).WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one * 5).WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * -10)
                    .WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * -5)
                    .WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.zero)
                    .WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * 10)
                    .WithExpectedLocalVelocity(Vector3.one * 10),

                // Adding zero is a no-op, even if we're past our limit already
                SoftScalarArgs(5).WithStartVel(Vector3.zero).WithForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.zero), // 26
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.one * 10),
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.one * 10),
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.zero).WithForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.zero).WithLocalForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.one * 10).WithForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero)
                    .WithExpectedVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.zero).WithForceVec(Vector3.zero)
                    .WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.zero).WithLocalForceVec(Vector3.zero)
                    .WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.one * 10).WithForceVec(Vector3.zero)
                    .WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero)
                    .WithExpectedLocalVelocity(Vector3.one * 10),
            };
        }

        BetterRigidbody PrepareBody(params SpeedLimit[] limits) {
            GameObject obj = new GameObject("Test Body", typeof(Rigidbody), typeof(BetterRigidbody));
            BetterRigidbody brb = obj.GetComponent<BetterRigidbody>();
            brb.useGravity = false;
            brb.drag = 0;
            brb.angularDrag = 0;
            brb.limits.AddRange(limits);

            return brb;
        }

        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator SoftLimitsTest([ValueSource(nameof(SoftLimitsCases))] AddForceTestArgs args) {
            BetterRigidbody brb = PrepareBody();
            // set up limits etc
            args.Prepare(brb);
            args.ApplyForce(brb);
            yield return new WaitForFixedUpdate();

            if (args.ExpectsLocalVelocity) {
                AreEqual(args.ExpectedVelocity, brb.LocalVelocity);
            }

            if (args.ExpectsWorldVelocity) {
                Assert.AreEqual(args.ExpectedVelocity, brb.velocity);
            }

            Assert.AreEqual(brb.velocity, brb.Velocity);
        }

        [UnityTest]
        public IEnumerator HardLimitsTest() {
            SpeedLimit limit = SpeedLimit.Hard;
            float maxSpeed = 10;
            var expectedVelocity = Vector3.right * maxSpeed;
            limit.SetOmniDirectionalLimit(maxSpeed);
            
            BetterRigidbody brb = PrepareBody(limit);
            brb.velocity = expectedVelocity; // starting velocity;
            yield return new WaitForFixedUpdate();

            brb.AddForce(Vector3.right * 100, ForceMode.VelocityChange);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);

            expectedVelocity = Vector3.left * maxSpeed;
            brb.AddForce(Vector3.left * 100, ForceMode.VelocityChange);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);

            expectedVelocity = Vector3.right * maxSpeed;
            brb.AddForceWithoutLimit(Vector3.right * 100,
                ForceMode.VelocityChange); // ForceWithoutLimit is still limited by hard limits
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);
        }

        [Test]
        public void InvalidLimitsTest() {
            Type eType = typeof(ArgumentException);
            Assert.Throws(eType, () => SoftScalarArgs(float.NegativeInfinity));
            Assert.Throws(eType, () => SoftScalarArgs(-1));
            Assert.Throws(eType, () => SoftLocalArgs(-Vector3.one));
            Assert.Throws(eType, () => SoftWorldArgs(-Vector3.one));
            Assert.Throws(eType, () => SoftLocalArgs(Vector3.one * float.NegativeInfinity));
            Assert.Throws(eType, () => SoftWorldArgs(Vector3.one * float.NegativeInfinity));
        }

        private static void AreEqual(Vector3 expected, Vector3 actual, float tolerance = MarginOfError) {
            Assert.AreEqual(expected.x, actual.x, tolerance);
            Assert.AreEqual(expected.y, actual.y, tolerance);
            Assert.AreEqual(expected.z, actual.z, tolerance);
        }
    }
}