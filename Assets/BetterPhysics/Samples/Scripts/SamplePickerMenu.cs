using System;
using UnityEngine;

namespace SadnessMonday.BetterPhysics.Samples {
    public class SamplePickerMenu : MonoBehaviour
    {
        [Serializable]
        struct SceneInfo {
            public string title;
            public string sceneName;
        }

        [SerializeField]
        SampleSceneButton buttonPrefab;
        [SerializeField]
        Transform buttonParent;
        [SerializeField]
        SceneInfo[] sceneInfos;

        void Start() {
            foreach (SceneInfo si in sceneInfos) {
                SampleSceneButton instance = Instantiate(buttonPrefab, buttonParent);
                instance.Setup(si.sceneName, si.title);
            }
        }
    }
}
