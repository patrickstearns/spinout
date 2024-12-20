using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraBallEffectController : MonoBehaviour {

    public bool Happening = false;
    
    private const float ChargeTime = 1.0f;

    public static GameObject Spawn(Vector3 location) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.extraBallEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<ExtraBallEffectController>().Start();
        return effect;
    }

    public void Start() {
        GetComponent<ParticleSystem>().Play();
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Light>().intensity = 10;
        GetComponent<Light>().range = 1;
        Happening = true;
    }

    public void Update() {
        //start with an initial velocity upward, then look at the paddle and turn toward that, accelerate as we go
        Vector3 targetPoint = GameController.Instance.paddle.transform.position;
        Quaternion towardTarget = Quaternion.LookRotation(targetPoint, Vector3.up);
        transform.rotation = towardTarget;

        Vector3 targetDirection = (targetPoint-transform.position).normalized;
        GetComponent<Rigidbody>().AddForce(targetDirection * 600 * Time.deltaTime, ForceMode.Acceleration);
    }

    public void OnTriggerEnter(Collider other) {
        if (GameController.Instance.paddle.IsPaddle(other.gameObject))
            StartCoroutine(stopFlareDie());
    }

    private IEnumerator stopFlareDie() {
        float flareTime = 0.1f;
        float startTime = Time.time;
        while (Time.time-startTime < flareTime) {
            float ratio = (Time.time-startTime)/flareTime;
            GetComponent<Light>().intensity = ratio * 200;
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity/2f;
            yield return new WaitForEndOfFrame();
        }
        startTime = Time.time;
        while (Time.time-startTime < flareTime) {
            float ratio = 1-(Time.time-startTime)/flareTime;
            GetComponent<Light>().intensity = ratio * 200;
            GetComponent<Light>().range = ratio * 10;
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity/2f;
            yield return new WaitForEndOfFrame();
        }

        GameController.Instance.Gain1UP();
        Happening = false;
        PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.extraBallEffectPrefab.name).ReturnObjectToPool(gameObject);
    }

}
