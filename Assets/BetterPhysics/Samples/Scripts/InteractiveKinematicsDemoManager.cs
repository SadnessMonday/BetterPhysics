using SadnessMonday.BetterPhysics.Layers;
using UnityEngine;


namespace SadnessMonday.BetterPhysics.Samples {
    public class InteractiveKinematicsDemoManager : MonoBehaviour {
        [SerializeField] private BRBLauncher redSpawner;
        [SerializeField] private BRBLauncher greenSpawner;
        [SerializeField] private BRBLauncher blueSpawner;
        [SerializeField] private BetterRigidbody playerCube;
        [SerializeField] private BetterRigidbody redSpinner;
        [SerializeField] private BetterRigidbody greenSpinner;
        [SerializeField] private BetterRigidbody blueSpinner;

        [SerializeField] private Material redMaterial, greenMaterial, blueMaterial;
        private InteractionLayer redLayer, greenLayer, blueLayer;

        private void Awake() {
            redLayer = InteractionLayer.GetOrCreateLayer("Red");
            greenLayer = InteractionLayer.GetOrCreateLayer("Green");
            blueLayer = InteractionLayer.GetOrCreateLayer("Blue");

            BetterPhysicsSettings.Instance.SetLayerInteraction(redLayer, greenLayer, InteractionType.Kinematic);
            BetterPhysicsSettings.Instance.SetLayerInteraction(greenLayer, blueLayer, InteractionType.Kinematic);
            BetterPhysicsSettings.Instance.SetLayerInteraction(blueLayer, redLayer, InteractionType.Kinematic);

            redSpawner.Init(redLayer, redMaterial);
            greenSpawner.Init(greenLayer, greenMaterial);
            blueSpawner.Init(blueLayer, blueMaterial);

            redSpinner.PhysicsLayer = redLayer;
            greenSpinner.PhysicsLayer = greenLayer;
            blueSpinner.PhysicsLayer = blueLayer;

            redSpinner.GetComponentInChildren<Renderer>().material = redMaterial;
            greenSpinner.GetComponentInChildren<Renderer>().material = greenMaterial;
            blueSpinner.GetComponentInChildren<Renderer>().material = blueMaterial;

            playerCube.PhysicsLayer = redLayer;
        }
    }
}
