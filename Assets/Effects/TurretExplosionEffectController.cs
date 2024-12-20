using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretExplosionEffectController : MonoBehaviour {

    private float startTime;

    public static GameObject Spawn(Vector3 location) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.turretExplosionEffect.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<TurretExplosionEffectController>().Start();
        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<ParticleSystem>().Play();
    }

    public void Update() {
        if (Time.time-3.0f > startTime)
            PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.turretExplosionEffect.name).ReturnObjectToPool(gameObject);
    }

}
