using System;
using SadnessMonday.BetterPhysics;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {

    public class BRBLauncher : MonoBehaviour {
        [SerializeField] private BetterRigidbody prefab;
        [SerializeField] private float launchSpeed;
        [SerializeField] private float lifeSpan = 10f;
        [SerializeField] private bool launchOnStart = false;
        [SerializeField] private bool launchPeriodically = false;
        [SerializeField] private float launchInterval;
        [SerializeField] private Material material;
        [SerializeField] private int physicsLayer = -1;

        public void Init(int layer, Material material) {
            this.physicsLayer = layer;
            this.material = material;
        }

        private void Start() {
            if (launchOnStart) SpawnPrefab();
        }

        private float timer = 0;
        private void Update() {
            if (!launchPeriodically) return;

            timer += Time.deltaTime;
            if (timer >= launchInterval) {
                timer -= launchInterval;
                SpawnPrefab();
            }
        }

        public BetterRigidbody SpawnPrefab() {
            return SpawnPrefab(physicsLayer, material, lifeSpan);
        }

        public BetterRigidbody SpawnPrefab(int physicsLayer, Material material, float lifespan) {
            var xForm = transform;
            BetterRigidbody instance = Instantiate(prefab, xForm.position, xForm.rotation);
            instance.AddRelativeForceWithoutLimit(Vector3.forward * launchSpeed, ForceMode.VelocityChange);
            if (material) instance.GetComponentInChildren<Renderer>().material = material;
            if (physicsLayer >= 0) instance.PhysicsLayer = physicsLayer;

            Destroy(instance.gameObject, lifespan);

            return instance;
        }
    }

}
