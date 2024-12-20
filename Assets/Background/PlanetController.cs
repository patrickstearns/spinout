using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour {

    private const float TimeToLive = 60f;
    private float startTime;

    void Start() {
        startTime = Time.time;

        Rigidbody body = GetComponent<Rigidbody>();
        body.AddTorque(new Vector3(0f, Random.Range(0f, 1f) * 50f, 0f));
        body.AddForce(new Vector3(0, 0, 100));
    }

    void Update() {
        if (Time.time - startTime > TimeToLive) Destroy(gameObject);
    }

}
