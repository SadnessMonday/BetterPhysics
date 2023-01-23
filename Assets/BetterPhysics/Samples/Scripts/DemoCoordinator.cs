using System;
using System.Collections;
using System.Collections.Generic;
using SadnessMonday.BetterPhysics.Layers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SadnessMonday.BetterPhysics.Samples {
    public class DemoCoordinator : MonoBehaviour {
        [SerializeField] private BRBLauncher[] leftSpawners;
        [SerializeField] private BRBLauncher[] rightSpawners;
        [SerializeField] private Text label;

        private IEnumerator Start() {
            BetterPhysicsSettings.Instance.ResetAllLayerInteractions();
            BetterPhysicsSettings.Instance.SetLayerIteraction(0, 1, 0);
            BetterPhysicsSettings.Instance.SetLayerIteraction(1, 2, 0);
            BetterPhysicsSettings.Instance.SetLayerIteraction(2, 0, 0);
            
            label.text = "Red vs Red\nNormal interaction";
            yield return SpawnThenWait(0, 0, 5);
            label.text = "Red vs Blue\nRed Ignores Blue";
            yield return SpawnThenWait(0, 1, 5);
            label.text = "Red vs Green\nRed is ignored by Green";
            yield return SpawnThenWait(0, 2, 5);
            
            label.text = "Blue vs Blue\nNormal interaction";
            yield return SpawnThenWait(1, 1, 5);
            label.text = "Blue vs Red\nBlue is ignored by Red";
            yield return SpawnThenWait(1, 0, 5);
            label.text = "Blue vs Green\nBlue ignores Green";
            yield return SpawnThenWait(1, 2, 5);
            
            label.text = "Green vs Green\nNormal interaction";
            yield return SpawnThenWait(2, 2, 5);
            label.text = "Green vs Red\nGreen ignores Red";
            yield return SpawnThenWait(2, 0, 5);
            label.text = "Green vs Blue\nGreen is ignored by Blue";
            yield return SpawnThenWait(2, 1, 5);
            label.text = "All together now";
            Finale(20);
        }

        private void Finale(float secs) {
            leftSpawners[2].SpawnPrefab(0, secs);
            rightSpawners[2].SpawnPrefab(0, secs);
            
            leftSpawners[3].SpawnPrefab(1, secs);
            rightSpawners[3].SpawnPrefab(2, secs);
            
            leftSpawners[4].SpawnPrefab(2, secs);
            rightSpawners[4].SpawnPrefab(2, secs);
            
            leftSpawners[5].SpawnPrefab(0, secs);
            rightSpawners[5].SpawnPrefab(1, secs);
            
            leftSpawners[6].SpawnPrefab(1, secs);
            rightSpawners[6].SpawnPrefab(1, secs);
            
            leftSpawners[7].SpawnPrefab(2, secs);
            rightSpawners[7].SpawnPrefab(0, secs);
        }

        IEnumerator SpawnThenWait(int left, int right, float secs) {
            leftSpawners[5].SpawnPrefab(left, secs);
            rightSpawners[5].SpawnPrefab(right, secs);
            yield return new WaitForSeconds(secs);
        }
    }
}