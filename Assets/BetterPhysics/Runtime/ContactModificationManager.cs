using System.Collections.Generic;
using UnityEngine;

namespace SadnessMonday.BetterPhysics {
    
    [AddComponentMenu("")] // Disallow adding from Add Component Menu
    internal class ContactModificationManager : MonoBehaviour {
        private static ContactModificationManager _instance;
        public static ContactModificationManager Instance {
            get {
                if (!ReferenceEquals(null, _instance)) return _instance;

                GameObject go = new("BetterPhysics");
                _instance = go.AddComponent<ContactModificationManager>();
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        private Dictionary<int, BetterRigidbody> _perRigidbodyData = new();

        public void Register(BetterRigidbody brb) {
            _perRigidbodyData[brb.GetRigidbodyInstanceID()] = brb;
        }

        public void UnRegister(BetterRigidbody brb) {
            _perRigidbodyData.Remove(brb.GetRigidbodyInstanceID());
        }
    }
}