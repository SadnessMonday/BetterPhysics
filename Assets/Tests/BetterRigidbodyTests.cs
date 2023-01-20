using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SadnessMonday.BetterPhysics.Tests
{
    public class BetterRigidbodyTests {
        private const float MarginOfError = 0.005f;
        private const float RootTwo = 1.41421356237f;
        private const float SineFortyFive = 0.707106781186548f;
        private static int _testNo = 0;

        public class AddForceTestArgs {
            private readonly int _testNumber;
            public LimitType SoftLimitType = LimitType.None;
            public LimitType HardLimitType = LimitType.None;
            public ForceMode ForceMode = ForceMode.VelocityChange;
            public float HardScalarLimit = -1;
            public float SoftScalarLimit = -1;
            public Vector3 SoftVectorLimit = -Vector3.one;
            public Vector3 HardVectorLimit = -Vector3.one;
            public Vector3 StartVel = Vector3.zero;
            public Vector3 StartLocalVel = Vector3.zero;
            public Vector3 ForceVec = Vector3.zero;
            public Vector3 LocalForceVec = Vector3.zero;
            public Vector3 Orientation = Vector3.forward;
            public Vector3 ExpectedVelocity = Vector3.zero;
            public bool ExpectsLocalVelocity { get; private set; } = false;
            public bool ExpectsWorldVelocity { get; private set; } = false;
            
            private bool _hasStartLocalVelocity = false;
            private bool _hasStartWorldVelocity = false;
            private bool _hasLocalForce = false;
            private bool _hasWorldForce = false;
            
            public Vector3 ExpectedChange => ExpectedVelocity - (ExpectsLocalVelocity ? StartLocalVel : StartVel);

            public AddForceTestArgs() {
                this._testNumber = _testNo++;
            }

            public override string ToString() => $"Test no. {_testNumber}";

            public AddForceTestArgs WithSoftLimitType(LimitType limitType) {
                SoftLimitType = limitType;
                return this;
            }

            public AddForceTestArgs WithHardLimitType(LimitType limitType) {
                HardLimitType = limitType;
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
                LocalForceVec = Vector3.zero;
                
                ForceVec = forceVec;
                _hasWorldForce = true;
                return this;
            }

            public AddForceTestArgs WithLocalForceVec(Vector3 forceVec) {
                // clear out world force
                _hasWorldForce = false;
                ForceVec = Vector3.zero;
                
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

            public AddForceTestArgs WithSoftLimitScalar(float scalar) {
                SoftLimitType = LimitType.Omnidirectional;
                SoftScalarLimit = scalar;
                return this;
            }

            public AddForceTestArgs WithHardLimitScalar(float scalar) {
                HardLimitType = LimitType.Omnidirectional;
                HardScalarLimit = scalar;
                return this;
            }

            public AddForceTestArgs WithSoftLimitVector(Vector3 softLimit) {
                SoftVectorLimit = softLimit;
                return this;
            }

            public AddForceTestArgs WithHardLimitVector(Vector3 hardLimit) {
                HardVectorLimit = hardLimit;
                return this;
            }

            public void Prepare(BetterRigidbody brb) {
                brb.hardLimitType = HardLimitType;
                brb.softLimitType = SoftLimitType;
                brb.hardScalarLimit = HardScalarLimit;
                brb.softScalarLimit = SoftScalarLimit;
                brb.hardVectorLimit = HardVectorLimit;
                brb.softVectorLimit = SoftVectorLimit;
                
                brb.rotation = Quaternion.Euler(Orientation);
                if (_hasStartLocalVelocity) {
                    brb.LocalVelocity = StartLocalVel;
                }
                if (_hasStartWorldVelocity) {
                    brb.Velocity = StartVel;
                }
            }

            public Vector3 ApplyForce(BetterRigidbody brb) {
                Vector3 sum = Vector3.zero;
                if (_hasWorldForce) sum += brb.AddForce(ForceVec, ForceMode);
                if (_hasLocalForce) sum += brb.AddRelativeForce(LocalForceVec, ForceMode);
                return sum;
            }
        }

        static AddForceTestArgs SoftScalarArgs(float limit) {
            return new AddForceTestArgs().WithSoftLimitScalar(limit).WithForceVec(Vector3.right * 10);
        }

        static AddForceTestArgs SoftWorldArgs(Vector3 limit) {
            return new AddForceTestArgs()
                .WithSoftLimitType(LimitType.WorldAxes)
                .WithSoftLimitVector(limit)
                .WithForceVec(Vector3.one * 10);
        }
        
        static AddForceTestArgs SoftLocalArgs(Vector3 limit) {
            return new AddForceTestArgs()
                .WithOrientation(Vector3.one)
                .WithSoftLimitType(LimitType.LocalAxes)
                .WithSoftLimitVector(limit)
                .WithLocalForceVec(Vector3.one * 10);
        }
        static AddForceTestArgs[] AddForceCases() {
            _testNo = 1;
            return new[] {
                // Omnidirectional tests
                SoftScalarArgs(float.NegativeInfinity).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(float.PositiveInfinity).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(float.NaN).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(-1).WithExpectedVelocity(Vector3.right * 10),
                SoftScalarArgs(0).WithExpectedVelocity(Vector3.zero),
                SoftScalarArgs(5).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithForceVec(new(10, 10, 0)) // 7
                    .WithExpectedVelocity(new(SineFortyFive * 5, SineFortyFive * 5, 0)),
                SoftScalarArgs(5).WithStartVel(Vector3.left * 10).WithExpectedVelocity(Vector3.zero),
                SoftScalarArgs(5).WithStartVel(Vector3.left * 5).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector3.zero).WithExpectedVelocity(Vector3.right * 5),
                SoftScalarArgs(5).WithStartVel(Vector3.right * 10).WithExpectedVelocity(Vector3.right * 10),
                // World axis tests 
                SoftWorldArgs(Vector3.one * float.NegativeInfinity).WithExpectedVelocity(Vector3.one * 10), //12
                SoftWorldArgs(Vector3.one * float.PositiveInfinity).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.one * float.NaN).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(-Vector3.one).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.zero).WithExpectedVelocity(Vector3.zero), // this one
                SoftWorldArgs(Vector3.one * 5).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * -10).WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * -5).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.zero).WithExpectedVelocity(Vector3.one * 5),
                SoftWorldArgs(Vector3.one * 5).WithStartVel(Vector3.one * 10).WithExpectedVelocity(Vector3.one * 10),
                // Local axis tests
                SoftLocalArgs(Vector3.one * float.NegativeInfinity).WithExpectedLocalVelocity(Vector3.one * 10), //22
                SoftLocalArgs(Vector3.one * float.PositiveInfinity).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one * float.NaN).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(-Vector3.one).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.zero).WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one * 5).WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * -10).WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * -5).WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.zero).WithExpectedLocalVelocity(Vector3.one * 5),
                SoftLocalArgs(Vector3.one * 5).WithStartLocalVel(Vector3.one * 10).WithExpectedLocalVelocity(Vector3.one * 10),
                
                // Adding zero is a no-op, even if we're past our limit already
                SoftScalarArgs(5).WithStartVel(Vector3.zero).WithForceVec(Vector3.zero).WithExpectedVelocity(Vector3.zero), // 32
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero).WithExpectedVelocity(Vector3.one * 10), 
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithForceVec(Vector3.zero).WithExpectedVelocity(Vector3.one * 10),
                SoftScalarArgs(5).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.zero).WithForceVec(Vector3.zero).WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.zero).WithLocalForceVec(Vector3.zero).WithExpectedVelocity(Vector3.zero),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.one * 10).WithForceVec(Vector3.zero).WithExpectedVelocity(Vector3.one * 10),
                SoftWorldArgs(Vector3.one).WithStartVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero).WithExpectedVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.zero).WithForceVec(Vector3.zero).WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.zero).WithLocalForceVec(Vector3.zero).WithExpectedLocalVelocity(Vector3.zero),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.one * 10).WithForceVec(Vector3.zero).WithExpectedLocalVelocity(Vector3.one * 10),
                SoftLocalArgs(Vector3.one).WithStartLocalVel(Vector3.one * 10).WithLocalForceVec(Vector3.zero).WithExpectedLocalVelocity(Vector3.one * 10),
            };
        }

        BetterRigidbody PrepareBody() {
            GameObject obj = new GameObject("Test Body", typeof(Rigidbody), typeof(BetterRigidbody));
            BetterRigidbody brb = obj.GetComponent<BetterRigidbody>();

            return brb;
        }
        
        // A Test behaves as an ordinary method
        [Test]
        [TestCaseSource(nameof(AddForceCases))]
        public void AddForceTest(AddForceTestArgs args) {
            BetterRigidbody brb = PrepareBody();
            
            // set up limits etc
            args.Prepare(brb);
            Vector3 velocityChange = args.ApplyForce(brb);
            
            AreEqual(args.ExpectedChange, velocityChange);
            
            Vector3 expected = args.ExpectedVelocity;
            if (args.ExpectsLocalVelocity) {
                AreEqual(args.ExpectedVelocity, brb.LocalVelocity);
            }
            if (args.ExpectsWorldVelocity) {
                AreEqual(args.ExpectedVelocity, brb.velocity);
            }
            AreEqual(brb.velocity, brb.Velocity);
        }

        private static void AreEqual(Vector3 expected, Vector3 actual, float tolerance = MarginOfError) {
            Assert.AreEqual(expected.x, actual.x, tolerance);
            Assert.AreEqual(expected.y, actual.y, tolerance);
            Assert.AreEqual(expected.z, actual.z, tolerance);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator BetterRigidbodyTestWithEnumeratorPasses() {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
