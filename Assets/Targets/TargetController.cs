using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TargetController : MonoBehaviour {

    private const float PopTime = 0.1f;

    public int hp;
    public bool indestructible;
    public int scoreValue;
    public TargetController childTarget;
    public Material originalMaterial;
    public GameController.TargetEffect targetEffect;

    protected Vector3 initialLocalScale;

    protected void Awake() {
        initialLocalScale = transform.localScale;
    }

    public void Hide() {
        transform.localScale = Vector3.zero;
    }

    public void SetIndestructible(bool indestructible) {
        if (this.indestructible && !indestructible){
            GetComponent<Renderer>().material = originalMaterial;
        }
        else if (!this.indestructible && indestructible) GetComponent<Renderer>().material = PrefabsManager.Instance.gray;
        this.indestructible = indestructible;
    }

    public void OnCollisionEnter(Collision collision) { collided(collision.collider, false); }
    public virtual void collided(Collider collider, bool thump) {
        if (collider != null && collider.GetComponent<BallController>() != null && collider.GetComponent<BallController>().ghost) return;

        if (!indestructible && (thump || collider.GetComponent<BallController>() || collider.GetComponent<BulletController>())){
            hp--;
            if (hp <= 0) Explode();
            else {
                GetComponent<AudioSource>().clip = AudioManager.Instance.ballTarget;
                GetComponent<AudioSource>().Play();

                Renderer renderer = GetComponent<Renderer>();
                if (hp == 1) renderer.material = crackedMaterial(renderer.material);
                else renderer.material = smallCrackedMaterial(renderer.material);
            }
        }
        else {
            GetComponent<AudioSource>().clip = AudioManager.Instance.ballTarget;
            GetComponent<AudioSource>().Play();
        }
    }

    protected Material crackedMaterial(Material original) {
        if (original.name.Contains(" Red ")) return PrefabsManager.Instance.targetRedCrack;
        else if (original.name.Contains(" Orange ")) return PrefabsManager.Instance.targetOrangeCrack;
        else if (original.name.Contains(" Yellow ")) return PrefabsManager.Instance.targetYellowCrack;
        else if (original.name.Contains(" Green ")) return PrefabsManager.Instance.targetGreenCrack;
        else if (original.name.Contains(" Blue ")) return PrefabsManager.Instance.targetBlueCrack;
        else if (original.name.Contains(" Purple ")) return PrefabsManager.Instance.targetPurpleCrack;
        else if (original.name.Contains(" White ")) return PrefabsManager.Instance.targetWhiteCrack;
        else if (original.name.Contains("Trap ")) return PrefabsManager.Instance.targetGlassCrack;
        return original; //shouldn't really ever happen
    }

    protected Material smallCrackedMaterial(Material original) {
        if (original.name.Contains(" Red ")) return PrefabsManager.Instance.targetRedCrackSmall;
        else if (original.name.Contains(" Orange ")) return PrefabsManager.Instance.targetOrangeCrackSmall;
        else if (original.name.Contains(" Yellow ")) return PrefabsManager.Instance.targetYellowCrackSmall;
        else if (original.name.Contains(" Green ")) return PrefabsManager.Instance.targetGreenCrackSmall;
        else if (original.name.Contains(" Blue ")) return PrefabsManager.Instance.targetBlueCrackSmall;
        else if (original.name.Contains(" Purple ")) return PrefabsManager.Instance.targetPurpleCrackSmall;
        else if (original.name.Contains(" White ")) return PrefabsManager.Instance.targetWhiteCrackSmall;
        else if (original.name.Contains("Trap ")) return PrefabsManager.Instance.targetGlassCrackSmall;
        return original; //shouldn't really ever happen
    }

    public virtual void PopUp() { StartCoroutine(popUpInternal()); }
    private IEnumerator popUpInternal() {
        if (indestructible){
            GetComponent<Renderer>().material = PrefabsManager.Instance.gray;
        }

        float startTime = Time.time;
        while (Time.time-startTime < PopTime) {
            float ratio = (Time.time-startTime) / PopTime;
            transform.localScale = initialLocalScale * ratio; 
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = initialLocalScale;
    }

    public virtual void Poof() {
        Destroy(gameObject);
    }

    public virtual void Explode() { StartCoroutine(ExplodeInternal()); }
    private IEnumerator ExplodeInternal() {
        if (childTarget != null && childTarget.indestructible) childTarget.SetIndestructible(false);

        //give sfx time to play but disable collisions
        GetComponent<Collider>().enabled = false;

        GameController.Instance.TargetExploded(this);

        EnergyBallEffectController.Spawn(transform.position, scoreValue);

        if (targetEffect == GameController.TargetEffect.None){
            TurretExplosionEffectController.Spawn(transform.position);
            GetComponent<AudioSource>().clip = AudioManager.Instance.targetDie;
            GetComponent<AudioSource>().Play();
        }
        else{
            SpecialTargetExplosionEffectController.Spawn(transform.position);
            GetComponent<AudioSource>().clip = AudioManager.Instance.specialTargetHit;
            GetComponent<AudioSource>().Play();
        }

        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(1.5f); 
        
        Destroy(gameObject);
    }
}
