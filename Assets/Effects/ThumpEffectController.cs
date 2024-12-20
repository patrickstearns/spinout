using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumpEffectController : MonoBehaviour {

    private const float FadeTime = 0.18f;
    private float startTime = 0f;
    private bool thumped = false;

    public static GameObject Spawn(Vector3 location, AudioClip sfx) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.thumpEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;

        Quaternion rot = effect.transform.rotation;
        rot.SetLookRotation(Vector3.up);
        effect.transform.rotation = rot;

        effect.GetComponent<AudioSource>().clip = sfx;

        effect.GetComponent<ThumpEffectController>().Start();

        return effect;
    }

    public void Start() {
        startTime = Time.time;
        GetComponent<Animator>().Play("thump");
        GetComponent<AudioSource>().Play();
        transform.localScale = Vector3.one * 8;
        thumped = false;
    }

    public void Update() {
        float ratio = (Time.time-startTime)/FadeTime;
        float s = ratio * 8 * 4;
        transform.localScale = new Vector3(s, s, s);
        if (ratio > 1 && !thumped){
            thumped = true;
            GameController.Instance.Thump(transform.position);
            PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.thumpEffectPrefab.name).ReturnObjectToPool(gameObject);
        }
    }
}
