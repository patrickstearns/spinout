using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallExplosionEffectController : MonoBehaviour {

    private float startTime;

    public static GameObject Spawn(Vector3 location) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballExplosionEffect.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<BallExplosionEffectController>().Start();
        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<ParticleSystem>().Play();
    }

    public void Update() {
        if (Time.time-3.0f > startTime)
            PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballExplosionEffect.name).ReturnObjectToPool(gameObject);
    }

}
