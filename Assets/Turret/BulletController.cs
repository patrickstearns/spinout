using UnityEngine;

public class BulletController : MonoBehaviour {

    public const float TimeToLive = 10f;

    public bool DestroysTargets;

    private float startTime;

    public TurretController parentTurret;

    public static GameObject Spawn(TurretController parentTurret, float speed, bool destroysTargets) {
        GameObject prefab = PrefabsManager.Instance.bulletPrefab;
        if (destroysTargets) prefab = PrefabsManager.Instance.greenBulletPrefab;
        GameObject bullet = PrefabPoolManager.Instance.PoolFor(prefab.name).GetObjectFromPool();

        bullet.GetComponent<BulletController>().parentTurret = parentTurret;
        bullet.transform.position = parentTurret.ballJoint.transform.position;
        bullet.transform.position += new Vector3(0, 0.2f, 0); //move up a bit so it matches the turret
        bullet.transform.rotation = parentTurret.ballJoint.transform.rotation;

        Vector3 pointingAt = parentTurret.ballJoint.transform.rotation * Vector3.forward;
        pointingAt.y = 0;
        pointingAt *= speed;
        bullet.GetComponent<Rigidbody>().velocity = pointingAt;

        bullet.GetComponent<BulletController>().DestroysTargets = destroysTargets;

        bullet.GetComponent<BulletController>().Start();

        return bullet;
    }

    protected void Start() {
        startTime = Time.time;
    }

    public void OnTriggerStay(Collider collider) {
        if (GameController.Instance.paddle.IsPaddle(collider.gameObject)) {
            GameController.Instance.paddle.Stunned(this);
            die();
        }
        else if (collider.GetComponent<TargetController>() != null && collider.gameObject != parentTurret.target) {
            if (DestroysTargets) collider.GetComponent<TargetController>().collided(GetComponent<Collider>(), false);
            die();
        }
        else if (collider.gameObject != parentTurret.turret && collider.gameObject != parentTurret.ballJoint && 
                collider.gameObject != parentTurret.target && collider.gameObject != parentTurret.targetPlatform){
            die();
        }
    }

    protected void FixedUpdate() {
        if (Time.time-startTime > TimeToLive) die();
    }

    public void Poof() { die(); }

    private void die() {
        GameObject prefab = PrefabsManager.Instance.bulletPrefab;
        if (DestroysTargets) prefab = PrefabsManager.Instance.greenBulletPrefab;
        PrefabPoolManager.Instance.PoolFor(prefab.name).ReturnObjectToPool(gameObject);
    }

}
