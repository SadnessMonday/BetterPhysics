using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SadnessMonday.BetterPhysics.Samples {
    public class SampleSceneButton : MonoBehaviour
    {
        [SerializeField]
        Text buttonLabel;
        string sceneName;

        public void Setup(string sceneName, string buttonTitle) {
            buttonLabel.text = buttonTitle;
            this.sceneName = sceneName;
        }

        public void WhenButtonPressed() {
            SceneManager.LoadScene(sceneName);
        }
    }
}
