using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPool : MonoBehaviour {

    public GameObject PrefabToPool;
    public int MaxPoolSize = 10;
    public bool EnableWarnings = false;
  
    public Stack<GameObject> inactiveObjects = new Stack<GameObject>();
  
    void Start() {
        if (PrefabToPool != null) {
            for (int i = 0; i < MaxPoolSize; ++i) {
                var newObj = Instantiate(PrefabToPool, transform);
                newObj.SetActive(false);
                inactiveObjects.Push(newObj);
            }

            PrefabPoolManager.Instance.Register(this);
        }
        else if (EnableWarnings) Debug.Log("["+PrefabToPool.name+"] PrefabToPool cannot be null.");
    }

    public GameObject GetObjectFromPool() {
        while (inactiveObjects.Count > 0) {
            var obj = inactiveObjects.Pop();
          
            if (obj != null) {
                obj.SetActive(true);
                return obj;
            }
            else if (EnableWarnings) Debug.LogWarning("["+PrefabToPool.name+"] Found a null object in the pool. Has some code outside the pool destroyed it?");
        }
      
        if (EnableWarnings) Debug.LogError("["+PrefabToPool.name+"] All pooled objects are already in use or have been destroyed");
        return null;
    }
  
    public void ReturnObjectToPool(GameObject objectToDeactivate) {
        if (objectToDeactivate != null) {
            objectToDeactivate.SetActive(false);
            inactiveObjects.Push(objectToDeactivate);
        }
    }
}
