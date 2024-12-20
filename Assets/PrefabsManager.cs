using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsManager : MonoBehaviour {

    public GameObject ballPrefab;
    public GameObject captiveBallPrefab;
    public GameObject bulletPrefab;
    public GameObject greenBulletPrefab;
    public GameObject bumpEffectPrefab;
    public GameObject sparkEffectPrefab;
    public GameObject scoreEffectPrefab;

    public GameObject specialTargetExplosionEffect;
    public GameObject turretExplosionEffect; //really "target explosion" effect, not turret
    public GameObject ballExplosionEffect;

    public GameObject starPrefab;
    public GameObject energyBallEffectPrefab, extraBallEffectPrefab;
    public GameObject thumpEffectPrefab;

    public Material gray, special;
    public Material targetRed, targetRedCrack, targetRedCrackSmall;
    public Material targetOrange, targetOrangeCrack, targetOrangeCrackSmall;
    public Material targetYellow, targetYellowCrack, targetYellowCrackSmall;
    public Material targetGreen, targetGreenCrack, targetGreenCrackSmall;
    public Material targetBlue, targetBlueCrack, targetBlueCrackSmall;
    public Material targetPurple, targetPurpleCrack, targetPurpleCrackSmall;
    public Material targetWhite, targetWhiteCrack, targetWhiteCrackSmall;
    public Material targetGlass, targetGlassCrack, targetGlassCrackSmall;
    public Material ballSelfDestructMaterial, ballStandardMaterial;

    private static PrefabsManager _instance;
    public static PrefabsManager Instance { get { return _instance; } }
    void Awake() {
        if (_instance != null && _instance != this) Destroy(gameObject);
        else _instance = this;
    }
}
