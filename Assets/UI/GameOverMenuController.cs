using TMPro;
using UnityEngine;

public class GameOverMenuController : MonoBehaviour {

    public TextMeshProUGUI continueOption, giveUpOption;

    private TextMeshProUGUI selectedOption;
    private Vector3 originalPosition;
    private bool visible;
    private bool enaballadebleh = false;

    void Start() {
        originalPosition = gameObject.transform.localPosition;
        Hide();
    }

    private void SetSelectedOption(TextMeshProUGUI option) { 
        if (selectedOption != null) selectedOption.color = Color.white;
        selectedOption = option;
        if (selectedOption != null) selectedOption.color = Color.cyan;
    }

    void Update() {
        if (!visible) return;

        //this is to prevent this from triggering on the first frame it's open
        if (!Input.anyKeyDown) enaballadebleh = true;
        if (!enaballadebleh) return;

        if (Input.GetKeyDown(KeyCode.Space)) {
            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
            if (selectedOption == continueOption) GameController.Instance.ContinueGame();
            else if (selectedOption == giveUpOption) GameController.Instance.GiveUpGame();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
            SetSelectedOption(selectedOption == continueOption ? giveUpOption : continueOption);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
            SetSelectedOption(selectedOption == continueOption ? giveUpOption : continueOption);
        }
    }

    public void Show() {
        gameObject.transform.localPosition = originalPosition;
        SetSelectedOption(continueOption);
        visible = true;
        enaballadebleh = false;
    }

    public void Hide() {
        gameObject.transform.localPosition = new Vector3(0, -1000, 0);
        visible = false;
    }
}
