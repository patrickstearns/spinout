using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptiveBallTargetController : TargetController {

    private const float PopTime = 0.1f;

    public GameObject parent, shell, ball;
    private bool exploding = false;

    public new void OnCollisionEnter(Collision collision) { collided(collision.collider, false); }

    public override void collided(Collider collider, bool thump) {
        if (collider != null && collider.GetComponent<BallController>() != null && collider.GetComponent<BallController>().ghost) return;

        if (!indestructible && (thump || collider.GetComponent<BallController>() || collider.GetComponent<BulletController>())){
            hp--;
            if (hp <= 0) Explode();
            else {
                Renderer renderer = GetComponent<Renderer>();
                if (hp == 1) renderer.material = crackedMaterial(renderer.material);
                else renderer.material = smallCrackedMaterial(renderer.material);
            }
        }

        if (hp <= 0) GetComponent<AudioSource>().clip = AudioManager.Instance.glassTargetBreak;
        else GetComponent<AudioSource>().clip = AudioManager.Instance.glassTargetCrack;
        GetComponent<AudioSource>().Play();
    }

    public override void PopUp() { StartCoroutine(popUpInternal()); }
    private IEnumerator popUpInternal() {
        if (indestructible) GetComponent<Renderer>().sharedMaterial = PrefabsManager.Instance.gray;

        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = initialLocalScale;
    }

    public override void Poof() { StartCoroutine(poofInternal()); }
    private IEnumerator poofInternal() {
        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            ratio = 1-ratio;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = new Vector3(0, 0, 0);

        Destroy(parent);
    }

    public override void Explode() { 
        if (exploding) return;
        StartCoroutine(explodeInternal()); 
    }

    private IEnumerator explodeInternal() {
        exploding = true;

        if (childTarget != null && childTarget.indestructible)
            childTarget.SetIndestructible(false);

        GameController.Instance.TargetExploded(this);
        GetComponent<AudioSource>().clip = AudioManager.Instance.targetDie;
        GetComponent<AudioSource>().Play();

        GameObject ball = BallController.Spawn(transform.position + new Vector3(0, 0, 1f), false);
        ball.GetComponent<BallController>().ActivateCollisions();
        GameController.Instance.activeBallCount++;

        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            ratio = 1-ratio;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = new Vector3(0, 0, 0);

        Vector3 initialV = Random.insideUnitSphere;
        initialV.y = 0;
        initialV = initialV.normalized * 20;
        ball.GetComponent<Rigidbody>().AddForce(initialV, ForceMode.Impulse);

        Destroy(parent);
    }

}
