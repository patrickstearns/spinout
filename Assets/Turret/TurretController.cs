using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour {

    private const float BulletSpeed = 10f;
    public float TurnSpeed = 1f;
    public float FireEverySeconds = 0.5f;
    public bool FireAtPaddle = false;

    public GameObject turret;
    public GameObject ballJoint;
    public GameObject targetPlatform;
    public GameObject target;

    public bool BulletsDestroyTargets;
    public bool activated = false;
    private float lastFireTime = 0f;

    protected void FixedUpdate() {
        if (activated) {
            if (FireAtPaddle) {
                Vector3 toPaddle = (GameController.Instance.paddle.transform.position - transform.position).normalized;
                Vector3 pointingAt = ballJoint.transform.rotation * Vector3.forward;
                pointingAt.y = 0;

                float yAngle = Vector3.SignedAngle(pointingAt, toPaddle, Vector3.up);
                if (yAngle > TurnSpeed) yAngle = TurnSpeed;
                if (yAngle < -TurnSpeed) yAngle = -TurnSpeed;

                Quaternion rot = ballJoint.transform.rotation;
                rot.eulerAngles = ballJoint.transform.rotation.eulerAngles + new Vector3(0, yAngle, 0);
                ballJoint.transform.rotation = rot;
            }

            if (Time.time-lastFireTime >= FireEverySeconds) fire();
        }        
    }

    private void fire() {
        lastFireTime = Time.time;

        BulletController.Spawn(this, BulletSpeed, BulletsDestroyTargets);

        GetComponent<AudioSource>().clip = AudioManager.Instance.bulletFired;
        GetComponent<AudioSource>().Play();
    }

    public void Exploded() {
        Destroy(gameObject);
    }
}
