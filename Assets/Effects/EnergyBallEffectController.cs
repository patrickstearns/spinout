using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBallEffectController : MonoBehaviour {

    public bool Happening = false;

    private int energy;

    public static GameObject Spawn(Vector3 location, int energy) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.energyBallEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<EnergyBallEffectController>().energy = energy;
        effect.GetComponent<EnergyBallEffectController>().Start();
        return effect;
    }

    public void Start() {
        GetComponent<ParticleSystem>().Play();
        GetComponent<Rigidbody>().velocity = new Vector3(0, 1, 0); //start with upward initial velocity
        GetComponent<Light>().intensity = 10;
        GetComponent<Light>().range = 10;
        int sizeEnergy = (energy < 10) ? energy : 10;
        transform.localScale = Vector3.one * sizeEnergy/20f;
        Happening = true;
    }

    public void Update() {
        //start with an initial velocity upward, then look at the energy spot and try to turn toward that, accelerate as we go
        Vector3 targetPoint = GameController.Instance.scoreboard.GetRightmostNotFullEnergyPod().transform.position;
        Quaternion towardTarget = Quaternion.LookRotation(targetPoint, Vector3.up);
        transform.rotation = towardTarget;

        Vector3 targetDirection = (targetPoint-transform.position).normalized;
        GetComponent<Rigidbody>().AddForce(targetDirection * 400 * Time.deltaTime, ForceMode.Acceleration);
    }

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject == GameController.Instance.scoreboard.gameObject) StartCoroutine(stopFlareDie());
    }

    private IEnumerator stopFlareDie() {
        float flareTime = 0.1f;
        float startTime = Time.time;
        while (Time.time-startTime < flareTime) {
            float ratio = (Time.time-startTime)/flareTime;
            GetComponent<Light>().intensity = ratio * 100;
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity/2f;
            yield return new WaitForEndOfFrame();
        }
        startTime = Time.time;
        while (Time.time-startTime < flareTime) {
            float ratio = 1-(Time.time-startTime)/flareTime;
            GetComponent<Light>().intensity = ratio * 100;
            GetComponent<Light>().range = ratio * 10;
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity/2f;
            yield return new WaitForEndOfFrame();
        }

        GameController.Instance.scoreboard.IncrementEnergy(energy);
        Happening = false;
        PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.energyBallEffectPrefab.name).ReturnObjectToPool(gameObject);
    }

}
