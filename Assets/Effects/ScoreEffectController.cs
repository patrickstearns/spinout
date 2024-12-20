using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreEffectController : MonoBehaviour {

    private const float TimeToLive = 1f;
    private float startTime = 0f;

    public static GameObject Spawn(Vector3 location, string scoreValue) {
        GameObject effect = PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.scoreEffectPrefab.name).GetObjectFromPool();
        effect.transform.position = location;
        effect.GetComponent<TextMeshPro> ().text = scoreValue;

        effect.GetComponent<ScoreEffectController>().Start();
        return effect;
    }

    public void Start() {
        startTime = Time.time;
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0)); 
    }

    public void FixedUpdate() {
        float ratio = (Time.time-startTime)/TimeToLive;
        if (ratio >= 1) PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.scoreEffectPrefab.name).ReturnObjectToPool(gameObject);
        else transform.localPosition += new Vector3(0, 1f * Time.fixedDeltaTime, 0);
    }
}
