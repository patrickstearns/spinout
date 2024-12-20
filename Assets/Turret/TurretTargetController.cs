using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTargetController : TargetController {

    private const float PopTime = 0.25f;

    public TurretController turret;

    public override void PopUp() { StartCoroutine(popUpInternal()); }
    private IEnumerator popUpInternal() {
        if (indestructible){
            GetComponent<Renderer>().sharedMaterial = PrefabsManager.Instance.gray;
        }

        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = initialLocalScale;
    }

    public override void Poof() {
        Destroy(gameObject);
    }

    public override void Explode() { StartCoroutine(ExplodeInternal()); }
    private IEnumerator ExplodeInternal() {
        if (childTarget != null && childTarget.indestructible)
            childTarget.SetIndestructible(false);

        GetComponent<AudioSource>().clip = AudioManager.Instance.targetDie;
        GetComponent<AudioSource>().Play();

        TurretExplosionEffectController.Spawn(transform.position);
        turret.activated = false;
        GameController.Instance.TargetExploded(this);

        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            ratio = 1-ratio;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = new Vector3(0, 0, 0);

        turret.Exploded();
    }

}
