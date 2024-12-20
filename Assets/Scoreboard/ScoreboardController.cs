using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardController : MonoBehaviour {

    public TextMeshPro levelField;
    public EnergyCapsuleController capsule1, capsule2, capsule3;

    private int level;

    public int IncrementLevel(int increment) {
        level += increment;
        levelField.text = ""+level;
        return level;
    }

    public int GetLevel() { return level; }

    public void SetLevel(int level) {
        this.level = level;
        levelField.text = ""+level;
    }

    public int GetEnergy() { return capsule1.GetEnergy() + capsule2.GetEnergy() + capsule3.GetEnergy(); }

    public void SetEnergy(int energy) {
        capsule1.SetCurrent(false);
        capsule2.SetCurrent(false);
        capsule3.SetCurrent(false);

        capsule1.SetEnergy(energy);
        capsule1.SetCurrent(true);
        if (energy > capsule1.MaxEnergy){
            capsule2.SetEnergy(energy-capsule1.MaxEnergy);
            capsule2.SetCurrent(true);
        }
        if (energy > capsule1.MaxEnergy+capsule2.MaxEnergy){
            capsule3.SetEnergy(energy-(capsule1.MaxEnergy+capsule2.MaxEnergy));
            capsule3.SetCurrent(true);
        }
    }

    public void IncrementEnergy(int increment) {
        if (increment > 0) {
            if (capsule1.GetEnergy() < capsule1.MaxEnergy) { 
                int capsule1Capacity = capsule1.MaxEnergy-capsule1.GetEnergy();
                if (increment > capsule1Capacity){
                    capsule2.IncrementEnergy(increment - capsule1Capacity);
                    capsule1.SetCurrent(false);
                    capsule2.SetCurrent(true);
                }
                capsule1.IncrementEnergy(increment);
            }
            else if (capsule2.GetEnergy() < capsule2.MaxEnergy) { 
                int capsule2Capacity = capsule2.MaxEnergy-capsule2.GetEnergy();
                if (increment > capsule2Capacity){
                    capsule3.IncrementEnergy(increment - capsule2Capacity);
                    capsule2.SetCurrent(false);
                    capsule3.SetCurrent(true);
                }
                capsule2.IncrementEnergy(increment);
            }
            else if (capsule3.GetEnergy() < capsule3.MaxEnergy) { 
                capsule3.IncrementEnergy(increment);
            }
        }
        else if (increment < 0) {
            if (capsule3.GetEnergy() > 0) { 
                if (-increment > capsule3.GetEnergy()){
                    capsule2.IncrementEnergy(increment + capsule3.GetEnergy());
                    capsule3.SetCurrent(false);
                    capsule2.SetCurrent(true);
                }
                capsule3.IncrementEnergy(increment);
            }
            else if (capsule2.GetEnergy() > 0) { 
                if (-increment > capsule2.GetEnergy()){
                    capsule1.IncrementEnergy(increment + capsule2.GetEnergy());
                    capsule2.SetCurrent(false);
                    capsule1.SetCurrent(true);
                }
                capsule2.IncrementEnergy(increment);
            }
            else if (capsule1.GetEnergy() > 0) { 
                capsule1.IncrementEnergy(increment);
            }
        }
    }

    public GameObject GetRightmostFullEnergyPod() {
        if (GetEnergy() == capsule1.MaxEnergy+capsule2.MaxEnergy+capsule3.MaxEnergy) return capsule3.gameObject;
        else if (GetEnergy() >= capsule1.MaxEnergy+capsule2.MaxEnergy) return capsule2.gameObject;
        else if (GetEnergy() >= capsule1.MaxEnergy) return capsule1.gameObject;
        return null;
    }

    public GameObject GetRightmostNotFullEnergyPod() {
        if (GetEnergy() == capsule1.MaxEnergy+capsule2.MaxEnergy+capsule3.MaxEnergy) return capsule3.gameObject;
        else if (GetEnergy() >= capsule1.MaxEnergy+capsule2.MaxEnergy) return capsule3.gameObject;
        else if (GetEnergy() >= capsule1.MaxEnergy) return capsule2.gameObject;
        return capsule1.gameObject;
    }
}
