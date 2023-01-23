using System;
using System.Collections;
using System.Collections.Generic;
using SadnessMonday.BetterPhysics;
using UnityEngine;
using UnityEngine.Serialization;

public class BRBLauncher : MonoBehaviour {
    [SerializeField] private BetterRigidbody[] prefabs;
    [SerializeField] private float launchSpeed;
    [SerializeField] private float lifeSpan = 10f;
    [SerializeField] private bool launchOnStart = false;
    [SerializeField] private int launchOnStartIndex = 0;

    private void Start() {
        if (launchOnStart) SpawnPrefab(launchOnStartIndex);
    }

    public BetterRigidbody SpawnPrefab(int index) {
        return SpawnPrefab(index, lifeSpan);
    }

    public BetterRigidbody SpawnPrefab(int index, float lifespan) {
        BetterRigidbody prefab = prefabs[index];
        var xForm = transform;
        BetterRigidbody instance = Instantiate(prefab, xForm.position, xForm.rotation);
        instance.AddRelativeForceWithoutLimit(Vector3.forward * launchSpeed, ForceMode.VelocityChange);
        
        Destroy(instance.gameObject, lifespan);

        return instance;
    }
}
