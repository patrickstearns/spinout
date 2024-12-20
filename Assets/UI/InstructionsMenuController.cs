using TMPro;
using UnityEngine;

public class InstructionsMenuController : MonoBehaviour {

    public TextMeshProUGUI continueOption;

    public Vector3 originalPosition;
    private bool visible;
    private bool enaballadebleh = false;

    void Start() {
        originalPosition = gameObject.transform.localPosition;
        Hide();
    }

    void Update() {
        if (!visible) return;

        //this is to prevent this from triggering on the first frame it's open
        if (!Input.anyKeyDown) enaballadebleh = true;
        if (!enaballadebleh) return;

        if (Input.GetKeyDown(KeyCode.Space)) {
            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
            GameController.Instance.HideInstructions();
        }
    }

    public void Show() {
        gameObject.transform.localPosition = originalPosition;
        continueOption.color = Color.cyan;
        visible = true;
        enaballadebleh = false;
    }

    public void Hide() {
        gameObject.transform.localPosition = new Vector3(0, -1000, 0);
        visible = false;
    }
}
