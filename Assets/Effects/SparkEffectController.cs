using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkEffectController : MonoBehaviour {

    private float startTime;

    public static GameObject Spawn(Vector3 location, Vector3 bumpNormal, Vector3 angularVelocity) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.sparkEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;

        Quaternion rot = effect.transform.rotation;
        rot.SetLookRotation(bumpNormal);
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y + (angularVelocity.y > 0 ? 0 : 180), rot.eulerAngles.z);
        effect.transform.rotation = rot;

        effect.transform.localScale = new Vector3(Mathf.Abs(angularVelocity.y) * 0.025f, 1, 1);

        effect.GetComponent<SparkEffectController>().Start();

        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<ParticleSystem>().Play();
    }

    public void Update() {
        if (Time.time-3.0f > startTime)
            PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.sparkEffectPrefab.name).ReturnObjectToPool(gameObject);
    }

}
