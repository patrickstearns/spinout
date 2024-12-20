using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroller : MonoBehaviour {

    public Material material;
    public float xRate, yRate;
    private float xOffset = 0, yOffset = 0;

    void Update() {
        xOffset += Time.deltaTime * xRate;
        yOffset += Time.deltaTime * yRate;
        material.mainTextureOffset = new Vector2(xOffset, yOffset);
    }

}
