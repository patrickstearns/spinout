using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolManager : MonoBehaviour {

    private static PrefabPoolManager _instance;
    public static PrefabPoolManager Instance { get { return _instance; } }

    public Dictionary<string, PrefabPool> pools = new Dictionary<string, PrefabPool>();

    void Awake() {
        if (_instance != null && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    public void Register(PrefabPool pool) {
        if (pools.ContainsKey(pool.PrefabToPool.name)) {
            Debug.Log("A second pool was found for key "+pool.PrefabToPool.name+"!");
            return;
        }
        pools.Add(pool.PrefabToPool.name, pool);
    }

    public PrefabPool PoolFor(string name) {
        return pools[name];
    }
}
