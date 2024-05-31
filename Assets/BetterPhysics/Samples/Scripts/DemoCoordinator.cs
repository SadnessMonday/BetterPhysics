using System.Collections;
using SadnessMonday.BetterPhysics.Layers;
using UnityEngine;
using UnityEngine.UI;

namespace SadnessMonday.BetterPhysics.Samples {
    public class DemoCoordinator : MonoBehaviour {
        [SerializeField] private BRBLauncher[] leftSpawners;
        [SerializeField] private BRBLauncher[] rightSpawners;
        [SerializeField] private Text label;

        private InteractionLayer redLayer, greenLayer, blueLayer;
        [SerializeField] private Material redMaterial, greenMaterial, blueMaterial;

        private IEnumerator Start() {
            redLayer = InteractionLayer.GetOrCreateLayer("Red");
            blueLayer = InteractionLayer.GetOrCreateLayer("Blue");
            greenLayer = InteractionLayer.GetOrCreateLayer("Green");

            BetterPhysicsSettings.Instance.SetLayerInteraction(redLayer, greenLayer, InteractionType.Kinematic);
            BetterPhysicsSettings.Instance.SetLayerInteraction(greenLayer, blueLayer, InteractionType.Kinematic);
            BetterPhysicsSettings.Instance.SetLayerInteraction(blueLayer, redLayer, InteractionType.Kinematic);
            
            label.text = "Red vs Red\nNormal interaction";
            yield return SpawnThenWait(redLayer, redLayer, redMaterial, redMaterial, 5);
            label.text = "Red vs Blue\nRed is ignored by Blue";
            yield return SpawnThenWait(redLayer, blueLayer, redMaterial, blueMaterial, 5);
            label.text = "Red vs Green\nRed ignores Green";
            yield return SpawnThenWait(redLayer, greenLayer, redMaterial, greenMaterial, 5);
            
            label.text = "Blue vs Blue\nNormal interaction";
            yield return SpawnThenWait(blueLayer, blueLayer, blueMaterial, blueMaterial, 5);
            label.text = "Blue vs Red\nBlue ignores Red";
            yield return SpawnThenWait(blueLayer, redLayer, blueMaterial, redMaterial, 5);
            label.text = "Blue vs Green\nBlue is ignored by Green";
            yield return SpawnThenWait(blueLayer, greenLayer, blueMaterial, greenMaterial, 5);
            
            label.text = "Green vs Green\nNormal interaction";
            yield return SpawnThenWait(greenLayer, greenLayer, greenMaterial, greenMaterial, 5);
            label.text = "Green vs Red\nGreen is ignored by Red";
            yield return SpawnThenWait(greenLayer, redLayer, greenMaterial, redMaterial, 5);
            label.text = "Green vs Blue\nGreen ignores Blue";
            yield return SpawnThenWait(greenLayer, blueLayer,  greenMaterial, blueMaterial, 5);
            label.text = "All together now";
            Finale(20);
        }

        private void Finale(float secs) {
            leftSpawners[2].SpawnPrefab(redLayer, redMaterial, secs);
            rightSpawners[2].SpawnPrefab(redLayer, redMaterial, secs);
            
            leftSpawners[3].SpawnPrefab(blueLayer, blueMaterial, secs);
            rightSpawners[3].SpawnPrefab(greenLayer, greenMaterial, secs);
            
            leftSpawners[4].SpawnPrefab(greenLayer, greenMaterial, secs);
            rightSpawners[4].SpawnPrefab(greenLayer, greenMaterial, secs);
            
            leftSpawners[5].SpawnPrefab(redLayer, redMaterial, secs);
            rightSpawners[5].SpawnPrefab(blueLayer, blueMaterial, secs);
            
            leftSpawners[6].SpawnPrefab(blueLayer, blueMaterial, secs);
            rightSpawners[6].SpawnPrefab(blueLayer, blueMaterial, secs);
            
            leftSpawners[7].SpawnPrefab(greenLayer, greenMaterial, secs);
            rightSpawners[7].SpawnPrefab(redLayer, redMaterial, secs);
        }

        IEnumerator SpawnThenWait(int left, int right, Material leftMat, Material rightMat, float secs) {
            leftSpawners[5].SpawnPrefab(left, leftMat, secs);
            rightSpawners[5].SpawnPrefab(right, rightMat, secs);
            yield return new WaitForSeconds(secs);
        }
    }
}