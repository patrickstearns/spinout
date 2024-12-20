using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    private Collider backWall;
    private PaddleController paddle;
    private bool collisionsActive = false;

    public bool ghost = false;

    public void ActivateCollisions() { collisionsActive = true; }

    public static GameObject Spawn(Vector3 location, bool ghost) {
        GameObject ball = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballPrefab.name).GetObjectFromPool();
        ball.transform.position = new Vector3(location.x, 0, location.z);
        ball.transform.localScale = Vector3.one;
        ball.GetComponent<LineRenderer>().positionCount = 0;
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.GetComponent<BallController>().ghost = ghost;
        ball.GetComponent<BallController>().Start();
        return ball;
    }

    public void Start() {
        paddle = GameController.Instance.paddle;
        backWall = GameController.Instance.backWall;
        GetComponent<Projection>()._obstaclesParent = GameController.Instance.field.transform;
    }

    protected void Update() {
        int armedLevel = GameController.Instance.paddle.GetArmLevel();
        if (armedLevel == 0) GetComponent<Renderer>().material = PrefabsManager.Instance.ballStandardMaterial;
        else {
            float scaledTime = Time.timeSinceLevelLoad * armedLevel * 4;
            float fraction = scaledTime - (int)scaledTime;

            if (fraction < 0.5f) GetComponent<Renderer>().material = PrefabsManager.Instance.ballStandardMaterial;
            else GetComponent<Renderer>().material = PrefabsManager.Instance.ballSelfDestructMaterial;
        }
    }

    public void OnCollisionEnter(Collision collision) {
        if (!collisionsActive) return;
        if (collision.collider.GetComponent<TargetController>() != null && collision.collider.GetComponent<TargetController>().hp == 0) return;

        bool colliderIsPaddle = paddle.IsPaddle(collision.collider.gameObject);
        Rigidbody rigidBody = GetComponent<Rigidbody>();

        if (!ghost) {
            AudioClip clip = AudioManager.Instance.ballWall;
            if (colliderIsPaddle) clip = AudioManager.Instance.ballPaddle;
            else if (collision.collider.GetComponent<TargetController>() != null) clip = null;

            //if we hit the back wall, we die, if a catch-paddle we get caught, otherwise spawn bump and spark effects
            if (collision.collider.gameObject == backWall.gameObject) Explode();
            else if (colliderIsPaddle && paddle.IsCatchMode()) {
                PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballPrefab.name).ReturnObjectToPool(gameObject);
                GameController.Instance.BallCaught();
            }
            else {
                BumpEffectController.Spawn(collision.GetContact(0).point, collision.contacts[0].normal, clip);
                if (rigidBody.angularVelocity != Vector3.zero)
                    SparkEffectController.Spawn(collision.GetContact(0).point, collision.contacts[0].normal, rigidBody.angularVelocity);
            }

            //spin if paddle moving when it hits the balls
            if (colliderIsPaddle && paddle.GetSpeed() != 0f) {
                float multiplier = -100f; //- to go opposite direction paddle was going
                rigidBody.AddTorque(new Vector3(0, paddle.GetSpeed() * multiplier, 0), ForceMode.Impulse);                
            }
        }
        else if (collision.collider.gameObject.name.Contains("Back Wall")) { //this is a terrible check to be making
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        //if we're spinning, convert some of our spin into a change in direction
        if (!colliderIsPaddle && rigidBody.angularVelocity != Vector3.zero){
            //basic amount to turn velocity, scaled by the (90-(angle between the normal and our velocity))/90
            float degreesToTurnVelocity = rigidBody.angularVelocity.y * (90f-Vector3.Angle(rigidBody.velocity, collision.contacts[0].normal))/90f;
            degreesToTurnVelocity *= 0.2f;
            Vector3 rotatedV = Quaternion.AngleAxis(degreesToTurnVelocity, Vector3.up) * rigidBody.velocity;
            rigidBody.velocity = rotatedV;

            //drop spin by half
            Vector3 antiSpin = rigidBody.GetAccumulatedTorque() * -0.5f;
            rigidBody.AddTorque(antiSpin, ForceMode.VelocityChange);
        }
    }

    public void Spawned(){ StartCoroutine(spawnedInternal()); }
    private IEnumerator spawnedInternal() {
        float startTime = Time.time;
        float PopTime = 1f;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            transform.position = new Vector3(transform.position.x, -1.5f + ratio * 1.5f, transform.position.z); 
            transform.localScale = new Vector3(ratio, ratio, ratio);
            yield return new WaitForEndOfFrame();
        }
    }

    public void BeamOut() { StartCoroutine(beamOutInternal()); }
    private IEnumerator beamOutInternal() {
        collisionsActive = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        float startTime = Time.time;
        float fxTime = 1.5f;
        while (Time.time-startTime < fxTime) {
            float ratio = (Time.time-startTime)/fxTime;

            transform.localScale = new Vector3(1-ratio, ratio*10, 1-ratio);

            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;

            yield return new WaitForEndOfFrame();
        }

        GameController.Instance.BallPoofed();
        PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballPrefab.name).ReturnObjectToPool(gameObject);
    }

    public void Explode() { StartCoroutine(explodeInternal()); }
    private IEnumerator explodeInternal() {
        collisionsActive = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        GetComponent<AudioSource>().clip = AudioManager.Instance.ballDie;
        GetComponent<AudioSource>().Play();

        BallExplosionEffectController.Spawn(transform.position);

        float startTime = Time.time;
        float splodeTime = 0.1f;
        while (Time.time-startTime < splodeTime) {
            float ratio = (Time.time-startTime)/splodeTime;
            ratio = 1-ratio;
            transform.localScale = new Vector3(ratio, ratio, ratio);
            yield return new WaitForEndOfFrame();
        }

        GameController.Instance.BallExploded();
        PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballPrefab.name).ReturnObjectToPool(gameObject);
    }
}
