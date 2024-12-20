using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumpEffectController : MonoBehaviour {

    private const float FadeTime = 0.18f;
    private float startTime = 0f;

    public static GameObject Spawn(Vector3 location, Vector3 bumpNormal, AudioClip sfx) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.bumpEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;

        Quaternion rot = effect.transform.rotation;
        rot.SetLookRotation(bumpNormal);
        effect.transform.rotation = rot;

        effect.GetComponent<AudioSource>().clip = sfx;

        effect.GetComponent<BumpEffectController>().Start();

        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<Animator>().Play("bump");
        GetComponent<AudioSource>().Play();
    }

    public void Update() {
        float ratio = (Time.time-startTime)/FadeTime;
        if (ratio >= 1) PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.bumpEffectPrefab.name).ReturnObjectToPool(gameObject);
    }
}
