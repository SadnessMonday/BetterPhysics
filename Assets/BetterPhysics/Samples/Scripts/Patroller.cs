using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SadnessMonday.BetterPhysics;

[RequireComponent(typeof(BetterRigidbody))]
public class Patroller : MonoBehaviour
{
    [SerializeField]
    Transform[] waypoints;
    [SerializeField]
    BetterRigidbody brb;
    [SerializeField]
    float speed;

    delegate float InterpolationFunction(float t);

    const float Tau = Mathf.PI * 2;
    
    public enum PatrolType {
        Linear,
        Sinusoidal
    }

    [SerializeField]
    PatrolType patrolType = PatrolType.Sinusoidal;

    InterpolationFunction interpolator;
    float t;
    int waypointIndex = 0;
    Transform currentOrigin => waypoints[waypointIndex];
    Transform currentDestination => waypoints[(waypointIndex + 1) % waypoints.Length];

    void FixedUpdate() {
        switch(patrolType) {
            case PatrolType.Sinusoidal:
                interpolator = SineFunc;
                break;
            default:
                interpolator = LinearFunc;
                break;
        }

        t += Time.deltaTime * speed;
        if (t >= 1) {
            t -= 1;
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        Vector3 newPosition = Vector3.Lerp(currentOrigin.position, currentDestination.position, interpolator(t));
        brb.MovePosition(newPosition);
    }

    static InterpolationFunction SineFunc = (input) => {
        float sineOutput = Mathf.Sin(input * Tau);
        return (sineOutput + 1) / 2f; // change range from -1,1 to 0,1
    };

    static InterpolationFunction LinearFunc = (input) => input;
}
