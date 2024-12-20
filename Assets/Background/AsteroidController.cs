using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour {

    private const float TimeToLive = 60f;
    private float startTime;

    void Start() {
        startTime = Time.time;

        Rigidbody body = GetComponent<Rigidbody>();
        body.AddTorque(Random.insideUnitSphere * 50);
        body.AddForce(new Vector3(0, 0, 100));
    }

    void Update() {
        if (Time.time - startTime > TimeToLive) Destroy(gameObject);
    }

}
