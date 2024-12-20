using TMPro;
using UnityEngine;

public class PulseTextUI : MonoBehaviour {

    public float rate = 2f; //wavelengths per second

    private bool pulsing = false;
    private float startTime;
    private Color color;
    private TextMeshProUGUI textMeshProGui;

    void Start(){
        textMeshProGui = GetComponent<TextMeshProUGUI>();
    }

    public void SetPulsing(bool pulsing) {
        this.pulsing = pulsing;
        if (pulsing){
            startTime = Time.time;
            color = textMeshProGui.color;
        }
    }
    
    void Update(){
        if (pulsing) {
            float alpha = Mathf.Sin((Time.time-startTime) * (Mathf.PI*2/rate))/2f + 0.5f;
            textMeshProGui.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
