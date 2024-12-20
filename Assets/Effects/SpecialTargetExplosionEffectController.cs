using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTargetExplosionEffectController : MonoBehaviour {

    private float startTime;

    public static GameObject Spawn(Vector3 location) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.specialTargetExplosionEffect.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<SpecialTargetExplosionEffectController>().Start();
        effect.transform.localScale = new Vector3(2f, 2f, 2f);
        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<ParticleSystem>().Play();
    }

    public void Update() {
        if (Time.time-3.0f > startTime)
            PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.specialTargetExplosionEffect.name).ReturnObjectToPool(gameObject);
    }

}
