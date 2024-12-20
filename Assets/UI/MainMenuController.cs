using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour {

    public GameObject arcadeOption, challengeOption, instructionsOption, quitOption;

    private GameObject focused;
    private Vector3 originalPosition;
    private bool visible = true;
    private bool enaballadebleh = false;

    public void Start() {
        originalPosition = gameObject.transform.localPosition;
        focused = arcadeOption;
    }

    public void Show() {
        gameObject.transform.localPosition = originalPosition;
        focused = arcadeOption;
        visible = true;
        enaballadebleh = false;
    }

    public void Hide() {
        gameObject.transform.localPosition = new Vector3(0, -1000, 0);
        visible = false;

        GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
        GetComponent<AudioSource>().Play();
    }

    void Update(){
        if (!visible) return;

        //this is to prevent this from triggering on the first frame it's open
        if (!Input.anyKeyDown) enaballadebleh = true;
        if (!enaballadebleh) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            if (focused == arcadeOption) focused = quitOption;
            else if (focused == challengeOption) focused = arcadeOption;
            else if (focused == instructionsOption) focused = challengeOption;
            else focused = instructionsOption;

            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            if (focused == arcadeOption) focused = challengeOption;
            else if (focused == challengeOption) focused = instructionsOption;
            else if (focused == instructionsOption) focused = quitOption;
            else focused = arcadeOption;

            GetComponent<AudioSource>().clip = AudioManager.Instance.menuMove;
            GetComponent<AudioSource>().Play();
        }

        arcadeOption.GetComponent<TextMeshProUGUI>().color = (focused == arcadeOption) ? Color.cyan : Color.white;
        challengeOption.GetComponent<TextMeshProUGUI>().color = (focused == challengeOption) ? Color.cyan : Color.white;
        instructionsOption.GetComponent<TextMeshProUGUI>().color = (focused == instructionsOption) ? Color.cyan : Color.white;
        quitOption.GetComponent<TextMeshProUGUI>().color = (focused == quitOption) ? Color.cyan : Color.white;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)){
            if (focused == arcadeOption) {
                Hide();
                GameController.Instance.NewArcadeGame(1);
            }
            else if (focused == challengeOption) {
                Hide();
                GameController.Instance.NewChallengeGame(1);
            }
            else if (focused == instructionsOption) {
                Hide();
                GameController.Instance.ShowInstructions();
            }
            else if (focused == quitOption) {
                Application.Quit();
            }
        }
    }
}
