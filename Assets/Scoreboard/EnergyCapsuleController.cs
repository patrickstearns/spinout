using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnergyCapsuleController : MonoBehaviour {

    public int MaxEnergy = 50;
    public TextMeshPro label;
    public Transform goo;

    private bool current = false;
    private int energy = 0, displayedEnergy = 0;

    void Start() {
        goo.localScale = Vector3.zero;
        label.text = "";
    }

    public bool IsCurrent() { return current; }
    public void SetCurrent(bool current) {
        this.current = current;
    }

    public int GetEnergy() { return energy; }
    public void SetEnergy(int energy) { 
        this.energy = energy; 
        if (this.energy < 0) this.energy = 0;
        if (this.energy > MaxEnergy) this.energy = MaxEnergy;
        displayedEnergy = energy;
        updateShape();
    }
    public void IncrementEnergy(int energy) {
        this.energy += energy;
        if (this.energy < 0) this.energy = 0;
        if (this.energy > MaxEnergy) this.energy = MaxEnergy;
        StartCoroutine(reshape());
    }

    private IEnumerator reshape() {
        while (energy != displayedEnergy) {
            updateShape();
            yield return new WaitForSeconds(0.25f/Mathf.Abs(energy-displayedEnergy));
        }

        if (energy == MaxEnergy) GetComponent<ParticleSystem>().Play();
        else GetComponent<ParticleSystem>().Stop();
    }

    private void updateShape() {
        if (energy < displayedEnergy) displayedEnergy--;
        if (energy > displayedEnergy) displayedEnergy++;

        float gooScale = (displayedEnergy / (float)MaxEnergy) * 0.9f; //0-90%
        goo.localScale = new Vector3(gooScale, gooScale, gooScale);
        label.text = displayedEnergy == 0 || displayedEnergy == MaxEnergy ? "" : ""+displayedEnergy;
    }
}
