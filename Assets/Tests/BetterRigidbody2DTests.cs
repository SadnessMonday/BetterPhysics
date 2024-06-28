using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SadnessMonday.BetterPhysics.Tests {
    public class BetterRigidbody2DTests {
        private const float MarginOfError = 0.0005f;
        private const float RootTwo = 1.41421356237f;
        private const float SineFortyFive = 0.707106781186548f;
        private static int _testNo = 0;

        public class AddForceTestArgs {
            private readonly int _testNumber;
            public List<SpeedLimit> limits = new();
            public ForceMode2D ForceMode = ForceMode2D.Impulse;
            public Vector2 StartVel = Vector2.zero;
            public Vector2 StartLocalVel = Vector2.zero;
            public Vector2 ForceVec = Vector2.zero;
            public Vector2 LocalForceVec = Vector2.zero;
            public Vector2 ForceVecWithoutLimit = Vector2.zero;

            public float Orientation = 0;
            public Vector2 ExpectedVelocity = Vector2.zero;
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

            public AddForceTestArgs WithForceMode(ForceMode2D mode) {
                ForceMode = mode;
                return this;
            }

            public AddForceTestArgs WithStartVel(Vector2 startVel) {
                if (_hasStartLocalVelocity) throw new Exception("Can only have one type of starting velocity");
                StartVel = startVel;
                _hasStartWorldVelocity = true;
                return this;
            }

            public AddForceTestArgs WithStartLocalVel(Vector2 startVel) {
                if (_hasStartWorldVelocity) throw new Exception("Can only have one type of starting velocity");
                StartLocalVel = startVel;
                _hasStartLocalVelocity = true;
                return this;
            }

            public AddForceTestArgs WithForceVec(Vector2 forceVec) {
                // clear out local force
                _hasLocalForce = false;
                _hasWorldForceWithoutLimit = false;

                ForceVec = forceVec;
                _hasWorldForce = true;
                return this;
            }

            public AddForceTestArgs WithForceVecWithoutLimit(Vector2 forceVec) {
                // clear out local force
                _hasLocalForce = false;
                _hasWorldForce = false;

                ForceVecWithoutLimit = forceVec;
                _hasWorldForceWithoutLimit = true;
                return this;
            }

            public AddForceTestArgs WithLocalForceVec(Vector2 forceVec) {
                // clear out world force
                _hasWorldForce = false;
                _hasWorldForceWithoutLimit = false;

                LocalForceVec = forceVec;
                _hasLocalForce = true;
                return this;
            }

            public AddForceTestArgs WithOrientation(float orientation) {
                Orientation = orientation;
                return this;
            }

            public AddForceTestArgs WithExpectedLocalVelocity(Vector2 expectedVal) {
                ExpectedVelocity = expectedVal;
                ExpectsLocalVelocity = true;
                return this;
            }

            public AddForceTestArgs WithExpectedVelocity(Vector2 expectedVel) {
                ExpectedVelocity = expectedVel;
                ExpectsWorldVelocity = true;
                return this;
            }

            public IEnumerator Prepare(BetterRigidbody2D brb) {
                brb.rotation = Orientation;
                if (_hasStartLocalVelocity) {
                    // Debug.Log($"Setting start local velocity {StartLocalVel}");
                    brb.LocalVelocity = StartLocalVel;
                    // Debug.Log($"Result {brb.velocity} {brb.LocalVelocity}");
                }

                if (_hasStartWorldVelocity) {
                    // Debug.Log($"Setting start world velocity {StartVel}");
                    brb.Velocity = StartVel;
                    // Debug.Log($"Result {brb.velocity} {brb.LocalVelocity}");
                }

                yield return new WaitForFixedUpdate();
                brb.SetLimits(limits);
                yield return new WaitForFixedUpdate();
            }

            public void ApplyForce(BetterRigidbody2D brb) {
                if (_hasWorldForce) brb.AddForce(ForceVec, ForceMode);
                if (_hasLocalForce) brb.AddRelativeForce(LocalForceVec, ForceMode);
                if (_hasWorldForceWithoutLimit) brb.AddForceWithoutLimit(ForceVecWithoutLimit, ForceMode);
            }
        }

        static AddForceTestArgs SoftScalarArgs(float omniLimit) {
            SpeedLimit limit = SpeedLimit.Soft;
            limit.SetOmniDirectionalLimit(omniLimit);
            return new AddForceTestArgs().WithLimit(limit).WithForceVec(Vector2.right * 10);
        }

        static AddForceTestArgs SoftWorldArgs(Vector2 limit) {
            return new AddForceTestArgs()
                .WithLimit(SpeedLimit.SymmetricalWorldLimits(LimitType.Soft, limit))
                .WithForceVec(Vector2.one * 10);
        }

        static AddForceTestArgs SoftLocalArgs(Vector2 limit) {
            return new AddForceTestArgs()
                .WithOrientation(-45)
                .WithLimit(SpeedLimit.SymmetricalLocalLimits(LimitType.Soft, limit))
                .WithLocalForceVec(Vector2.one * 10);
        }

        static AddForceTestArgs[] SoftLimitsCases() {
            _testNo = 1;
            return new[] {
                // Omnidirectional tests
                SoftScalarArgs(float.PositiveInfinity).WithExpectedVelocity(Vector2.right * 10),
                SoftScalarArgs(float.NaN).WithExpectedVelocity(Vector2.right * 10),
                SoftScalarArgs(0).WithExpectedVelocity(Vector2.zero),
                SoftScalarArgs(5).WithExpectedVelocity(Vector2.right * 5),
                SoftScalarArgs(5).WithForceVec(new(10, 10)) // 5
                    .WithExpectedVelocity(new(SineFortyFive * 5, SineFortyFive * 5)),
                SoftScalarArgs(5).WithStartVel(Vector2.left * 10).WithExpectedVelocity(Vector2.zero),
                SoftScalarArgs(5).WithStartVel(Vector2.left * 5).WithExpectedVelocity(Vector2.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector2.zero).WithExpectedVelocity(Vector2.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector2.right * 10).WithExpectedVelocity(Vector2.right * 10),
                // World axis tests 
                SoftWorldArgs(Vector2.one * float.PositiveInfinity).WithExpectedVelocity(Vector2.one * 10), // 10
                SoftWorldArgs(Vector2.one * float.NaN).WithExpectedVelocity(Vector2.one * 10),
                SoftWorldArgs(Vector2.zero).WithExpectedVelocity(Vector2.zero), // 12
                SoftWorldArgs(Vector2.one * 5).WithExpectedVelocity(Vector2.one * 5),
                SoftWorldArgs(Vector2.one * 5).WithStartVel(Vector2.one * -10).WithExpectedVelocity(Vector2.zero),
                SoftWorldArgs(Vector2.one * 5).WithStartVel(Vector2.one * -5).WithExpectedVelocity(Vector2.one * 5),
                SoftWorldArgs(Vector2.one * 5).WithStartVel(Vector2.zero).WithExpectedVelocity(Vector2.one * 5),
                SoftWorldArgs(Vector2.one * 5).WithStartVel(Vector2.one * 10).WithExpectedVelocity(Vector2.one * 10),
                // Local axis tests
                SoftLocalArgs(Vector2.one * float.PositiveInfinity).WithExpectedLocalVelocity(Vector2.one * 10),
                SoftLocalArgs(Vector2.one * float.NaN).WithExpectedLocalVelocity(Vector2.one * 10),
                SoftLocalArgs(Vector2.zero).WithExpectedLocalVelocity(Vector2.zero),
                SoftLocalArgs(Vector2.one * 5).WithExpectedLocalVelocity(Vector2.one * 5),
                SoftLocalArgs(Vector2.one * 5).WithStartLocalVel(Vector2.one * -10)
                    .WithExpectedLocalVelocity(Vector2.zero),
                SoftLocalArgs(Vector2.one * 5).WithStartLocalVel(Vector2.one * -5)
                    .WithExpectedLocalVelocity(Vector2.one * 5),
                SoftLocalArgs(Vector2.one * 5).WithStartLocalVel(Vector2.zero)
                    .WithExpectedLocalVelocity(Vector2.one * 5),
                SoftLocalArgs(Vector2.one * 5).WithStartLocalVel(Vector2.one * 10)
                    .WithExpectedLocalVelocity(Vector2.one * 10),

                // Adding zero is a no-op, even if we're past our limit already
                SoftScalarArgs(5).WithStartVel(Vector2.zero).WithForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.zero), // 26
                SoftScalarArgs(5).WithStartVel(Vector2.one * 10).WithLocalForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftScalarArgs(5).WithStartVel(Vector2.one * 10).WithForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftScalarArgs(5).WithStartVel(Vector2.one * 10).WithLocalForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftWorldArgs(Vector2.one).WithStartVel(Vector2.zero).WithForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.zero),
                SoftWorldArgs(Vector2.one).WithStartVel(Vector2.zero).WithLocalForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.zero),
                SoftWorldArgs(Vector2.one).WithStartVel(Vector2.one * 10).WithForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftWorldArgs(Vector2.one).WithStartVel(Vector2.one * 10).WithLocalForceVec(Vector2.zero)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftLocalArgs(Vector2.one).WithStartLocalVel(Vector2.zero).WithForceVec(Vector2.zero)
                    .WithExpectedLocalVelocity(Vector2.zero),
                SoftLocalArgs(Vector2.one).WithStartLocalVel(Vector2.zero).WithLocalForceVec(Vector2.zero)
                    .WithExpectedLocalVelocity(Vector2.zero),
                SoftLocalArgs(Vector2.one).WithStartLocalVel(Vector2.one * 10).WithForceVec(Vector2.zero)
                    .WithExpectedLocalVelocity(Vector2.one * 10),
                SoftLocalArgs(Vector2.one).WithStartLocalVel(Vector2.one * 10).WithLocalForceVec(Vector2.zero)
                    .WithExpectedLocalVelocity(Vector2.one * 10),
                
                SoftLocalArgs(new (10, 0)).WithStartLocalVel(new (10, 0))
                    .WithLocalForceVec(new (10, 0))
                    .WithExpectedLocalVelocity(new (10, 0)),
                
                // No limits tests
                SoftScalarArgs(1).WithForceVecWithoutLimit(Vector2.one * 10)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftLocalArgs(Vector2.one).WithForceVecWithoutLimit(Vector2.one * 10)
                    .WithExpectedVelocity(Vector2.one * 10),
                SoftWorldArgs(Vector2.one).WithForceVecWithoutLimit(Vector2.one * 10)
                    .WithExpectedVelocity(Vector2.one * 10),
            };
        }

        BetterRigidbody2D PrepareBody(params SpeedLimit[] limits) {
            GameObject obj = new GameObject("Test Body", typeof(Rigidbody2D), typeof(BetterRigidbody2D));
            BetterRigidbody2D brb = obj.GetComponent<BetterRigidbody2D>();
            brb.gravityScale = 0;
            brb.drag = 0;
            brb.angularDrag = 0;
            brb.velocity = Vector2.zero;
            brb.angularVelocity = 0;
            brb.AddLimits(limits);
            // Debug.Log($"Setting up {brb.GetInstanceID()} with velocity {brb.velocity} and local velocity {brb.LocalVelocity}");
            return brb;
        }

        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator SoftLimitsTest([ValueSource(nameof(SoftLimitsCases))] AddForceTestArgs args) {
            BetterRigidbody2D brb = PrepareBody();            
            // Debug.Log($"About to prepare {brb.GetInstanceID()} with velocity {brb.velocity} and local velocity {brb.LocalVelocity}");
            // set up limits etc
            yield return args.Prepare(brb);
            // Debug.Log($"Finished preparing {brb.GetInstanceID()} with velocity {brb.velocity} and local velocity {brb.LocalVelocity}");

            args.ApplyForce(brb);
            // Debug.Log($"Added a force to {brb.GetInstanceID()} now it has velocity {brb.velocity} and local velocity {brb.LocalVelocity}");
            yield return new WaitForFixedUpdate();
            // Debug.Log($"After waiting one fixedupdate, {brb.GetInstanceID()} has velocity {brb.velocity} and local velocity {brb.LocalVelocity}");

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
            float maxSpeed = 10;
            SpeedLimit limit = SpeedLimit.Hard;
            limit.SetOmniDirectionalLimit(maxSpeed);
            BetterRigidbody2D brb = PrepareBody(limit);
            
            var expectedVelocity = Vector2.right * maxSpeed;
            brb.velocity = expectedVelocity; // starting velocity;
            yield return new WaitForFixedUpdate();

            brb.AddForce(Vector2.right * 100, ForceMode2D.Impulse);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);

            expectedVelocity = Vector2.left * maxSpeed;
            brb.AddForce(Vector2.left * 100, ForceMode2D.Impulse);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);

            expectedVelocity = Vector2.right * maxSpeed;
            brb.AddForceWithoutLimit(Vector2.right * 100,
                ForceMode2D.Impulse); // ForceWithoutLimit is still limited by hard limits
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(expectedVelocity, brb.velocity);
        }

        [Test]
        public void InvalidLimitsTest() {
            Type eType = typeof(ArgumentException);
            Assert.Throws(eType, () => SoftScalarArgs(float.NegativeInfinity));
            Assert.Throws(eType, () => SoftScalarArgs(-1));
            Assert.Throws(eType, () => SoftLocalArgs(-Vector2.one));
            Assert.Throws(eType, () => SoftWorldArgs(-Vector2.one));
            Assert.Throws(eType, () => SoftLocalArgs(Vector2.one * float.NegativeInfinity));
            Assert.Throws(eType, () => SoftWorldArgs(Vector2.one * float.NegativeInfinity));
        }

        private static void AreEqual(Vector2 expected, Vector2 actual, float tolerance = MarginOfError) {
            Assert.AreEqual(expected.x, actual.x, tolerance);
            Assert.AreEqual(expected.y, actual.y, tolerance);
        }
    }
}