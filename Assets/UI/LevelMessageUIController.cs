using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelMessageUIController : MonoBehaviour {

    public const float PopTime = 0.2f;

    public GameObject toContinue;

    void Start() {
        GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);
        toContinue.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);
    }

    public void Show(string message) { StartCoroutine(showInternal(message)); }
    private IEnumerator showInternal(string message) {
        GetComponent<TextMeshProUGUI>().text = message;

        float popTime = Time.time;
        while(Time.time - popTime < PopTime) {
            float ratio = (Time.time-popTime)/PopTime;
            GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, ratio);
            toContinue.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, ratio);
            yield return new WaitForEndOfFrame();
        }
        GetComponent<TextMeshProUGUI>().color = Color.white;
        toContinue.GetComponent<TextMeshProUGUI>().color = Color.white;

        toContinue.GetComponent<PulseTextUI>().SetPulsing(true);
    }

    public void Hide() { StartCoroutine(hideInternal()); }
    private IEnumerator hideInternal() {
        toContinue.GetComponent<PulseTextUI>().SetPulsing(false);

        float popTime = Time.time;
        while(Time.time - popTime < PopTime) {
            float ratio = (Time.time-popTime)/PopTime;
            ratio = 1-ratio;
            GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, ratio);
            toContinue.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, ratio);
            yield return new WaitForEndOfFrame();
        }
        GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);
        toContinue.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);

        toContinue.GetComponent<PulseTextUI>().SetPulsing(false); //just in case...
    }

}
