using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PaddleController : MonoBehaviour {

    private const float PopTime = 0.1f;

    public GameObject leftBall, rightBall, body, message1, message2;
    private List<GameObject> captiveBalls = new List<GameObject>();
    private GameObject launchBall;

    public Material magnet, launch, armed0, armed1, armed2, armed3;

    public bool controllable = false, actionable = false;
    private bool actionDown = false, actionReleased = true;
    private float actionDownTime = -10f;

    private bool launchMode = false, catchMode = false;
    private int ballsHeld = 0, armedLevel = 0, stunLevel = 0;
    private float lastArmTime = 0, lastStunTime = 0;
    private float speed;

    protected void Awake() {
        transform.localScale = new Vector3(0, 0, 0);
    }

    protected void Update() {
        //handle input (or don't)
        if (controllable) {
            //launch or action
            if (launchMode) {
                if (Input.GetKeyDown(KeyCode.Space)){
                    GameController.Instance.LaunchBall();
                    GetComponent<AudioSource>().clip = AudioManager.Instance.ballLaunch;
                    GetComponent<AudioSource>().Play();
                    lastArmTime = Time.time; //to keep this from setting off initial self-d when launching
                }
            }
            else if (actionable){
                int abilityCost = 20;

                if (Input.GetKeyDown(KeyCode.Space)){
                    actionDown = true;
                    actionDownTime = Time.time;
                    actionReleased = false;
                    lastArmTime = Time.time;
                }
                else if (Input.GetKey(KeyCode.Space)) {
                    if (actionDown && Time.time-lastArmTime > 1f) {
                        lastArmTime = Time.time;
                        IncrementArmLevel(1);
                    }
                }
                //if we just let go, we had only tapped it, and we have enough energy, use our ability
                else if (actionDown && !actionReleased && armedLevel == 0 && Time.time-actionDownTime < 0.25f && GameController.Instance.scoreboard.GetEnergy() >= abilityCost) {
                    actionDown = false;
                    actionReleased = true;
                    GameController.Instance.AbilityTriggered();
                }
            }

            //skips track
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                AudioManager.Instance.SetGameMusicPlaying(true);

            //move
            float multiplier = stunLevel > 0 ? 50 : 300;
            speed = Input.GetAxis("Horizontal") * Time.deltaTime * multiplier;         
            transform.position += new Vector3(speed * 0.1f, 0, 0);
        }

        //update arm/stun levels
        if (Time.time-lastArmTime >= 2f && armedLevel > 0) {
            IncrementArmLevel(-1);
        }

        if (Time.time-lastStunTime >= 1f && stunLevel > 0) {
            stunLevel--;
            lastStunTime = Time.time;
        }

        //update materials/colors/shape etc.
        Material material = armed0;
        if (armedLevel == 1) material = armed1;
        else if (armedLevel == 2) material = armed2;
        else if (armedLevel == 3) material = armed3;
        if (launchMode) material = launch;
        if (catchMode) material = magnet;
        leftBall.GetComponent<Renderer>().material = material;
        rightBall.GetComponent<Renderer>().material = material;

        Color emissionColor = Color.black;
        if (stunLevel > 0){
            float s = 1.0f - (0.2f * stunLevel);
            emissionColor = new Color(s, s, 0);
        }
        leftBall.GetComponent<Renderer>().material.SetColor("_EmissionColor", emissionColor);
        rightBall.GetComponent<Renderer>().material.SetColor("_EmissionColor", emissionColor);
        
        float paddleWidth = 5f;
        if (transform.position.x - paddleWidth/2 < -15.5) 
            transform.position = new Vector3(-15.5f + paddleWidth/2, transform.position.y, transform.position.z);
        if (transform.position.x + paddleWidth/2 > 15.5) 
            transform.position = new Vector3(15.5f - paddleWidth/2, transform.position.y, transform.position.z);
    }

    public float GetSpeed() { return speed; }
    public bool IsPaddle(GameObject go) { return go == gameObject || go == leftBall || go == rightBall || go == body; }

    public int GetArmLevel() { return armedLevel; }
    public void IncrementArmLevel(int inc) { 
        armedLevel += inc;
        lastArmTime = Time.time;

        if (armedLevel == 0) HideMessage();
        else if (armedLevel == 1) ShowMessage("DESTRUCT\nACTIVE", Color.red);
        else if (armedLevel == 2) ShowMessage("DESTRUCT\nSET", Color.red);
        else if (armedLevel == 3) ShowMessage("DESTRUCT\nARMED", Color.red);
        else if (armedLevel >= 4){
            HideMessage();
            armedLevel = 0;
            actionDown = false;
            GameController.Instance.SelfDestruct();
        }
    }

    public void CaughtBall() {
        SetBallsHeld(GetBallsHeld()+1);

        GetComponent<AudioSource>().clip = AudioManager.Instance.ballCatch;
        GetComponent<AudioSource>().Play();
    }

    public int GetBallsHeld() { return ballsHeld; }
    public void SetBallsHeld(int ballsHeld) { 
        this.ballsHeld = ballsHeld; 
        Reshape();
    }

    public void AddBall() {
        ballsHeld++;
        Reshape();
    }

    public void Reshape() {
        //remove old captive balls
        foreach (GameObject captive in captiveBalls) Destroy(captive);
        captiveBalls.Clear();

        //adjust length of cylinder and position of end balls
        //paddle body is y=1.6 with three balls or fewer, 2 with 4 balls, presumably 2.75 with 5 balls or more
        float s = ballsHeld <= 3 ? 1.6f : ballsHeld == 4 ? 2.0f : 2.4f;
        body.transform.localScale = new Vector3(1, s, 1);

        //ball x is also s or -s
        leftBall.transform.localPosition = new Vector3(-s, 0, 0);
        rightBall.transform.localPosition = new Vector3(s, 0, 0);

        //create new captive balls and position them
        //if balls odd  -> middle ball at 0,0, next out at +/-.75, next out at +/-1.5
        if (ballsHeld%2 == 1 || ballsHeld > 5) {
            if (ballsHeld >= 1) captiveBalls.Add(addCaptiveBallAt(new Vector3(0, 0, 0)));
            if (ballsHeld >= 3) {
                captiveBalls.Add(addCaptiveBallAt(new Vector3(0.75f, 0, 0)));
                captiveBalls.Add(addCaptiveBallAt(new Vector3(-0.75f, 0, 0)));
            }
            if (ballsHeld >= 5) {
                captiveBalls.Add(addCaptiveBallAt(new Vector3(1.5f, 0, 0)));
                captiveBalls.Add(addCaptiveBallAt(new Vector3(-1.5f, 0, 0)));
            }
        }
        //if balls even -> middle balls at +/-.375, others at +/- 1.125
        else {
            if (ballsHeld >= 2) {
                captiveBalls.Add(addCaptiveBallAt(new Vector3(0.375f, 0, 0)));
                captiveBalls.Add(addCaptiveBallAt(new Vector3(-0.375f, 0, 0)));
            }
            if (ballsHeld >= 4) {
                captiveBalls.Add(addCaptiveBallAt(new Vector3(1.125f, 0, 0)));
                captiveBalls.Add(addCaptiveBallAt(new Vector3(-1.125f, 0, 0)));
            }
        }

        //launch mode ball
        if (launchMode) launchBall = addCaptiveBallAt(new Vector3(0, 0, 0.375f));
        else if (launchBall != null) Destroy(launchBall);
    }

    private GameObject addCaptiveBallAt(Vector3 position) {
        GameObject captive = Instantiate(PrefabsManager.Instance.captiveBallPrefab, transform);
        captive.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        captive.transform.localPosition = position;
        return captive;
    }

    public void PopUp() { StartCoroutine(popupInternal()); }
    private IEnumerator popupInternal() {
        float startTime = Time.time;
        while (transform.localScale.y < 1) {
            float ratio = (Time.time-startTime)/PopTime;
            transform.localScale = new Vector3(ratio, ratio, ratio);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void Poof() { StartCoroutine(poofInternal()); }
    private IEnumerator poofInternal() {
        float startTime = Time.time;
        while (transform.localScale.y > 0) {
            float ratio = (Time.time-startTime)/PopTime;
            ratio = 1-ratio;
            transform.localScale = new Vector3(ratio, ratio, ratio);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = new Vector3(0, 0, 0);
    }

    public bool IsLaunchMode() { return launchMode; }
    public void ActivateLaunchMode() {
        ShowMessage("LAUNCH", Color.yellow, true);
        launchMode = true;
        catchMode = false;

        GetComponent<AudioSource>().clip = AudioManager.Instance.launchMode;
        GetComponent<AudioSource>().Play();
    }

    public bool IsCatchMode() { return catchMode; }
    public void ActivateCatchMode() {
        ShowMessage("CATCH", Color.magenta);
        launchMode = false;
        catchMode = true;

        GetComponent<AudioSource>().clip = AudioManager.Instance.catchMode;
        GetComponent<AudioSource>().Play();
    }

    public bool IsBounceMode() { return !launchMode && !catchMode; }
    public void ActivateBounceMode() {
        HideMessage();
        launchMode = false;
        catchMode = false;
    }

    public void ShowMessage(string message, Color color) { StartCoroutine(showMessageInternal(message, color, false)); }
    public void ShowMessage(string message, Color color, bool pulsing) { StartCoroutine(showMessageInternal(message, color, pulsing)); }
    private IEnumerator showMessageInternal(string message, Color c, bool pulsing) {
        message1.GetComponent<TextMeshPro>().text = message;
        message2.GetComponent<TextMeshPro>().text = message;

        float startTime = Time.time;
        while (Time.time-startTime < 0.1f) {
            float ratio = (Time.time-startTime)/0.1f;
            message1.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, ratio);
            message2.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, ratio);
            yield return new WaitForEndOfFrame();
        }

        message1.GetComponent<PulseText>().SetPulsing(pulsing);
        message2.GetComponent<PulseText>().SetPulsing(pulsing);

        message1.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, 1);
        message2.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, 1);
    }

    public void HideMessage() { StartCoroutine(hideMessageInternal()); }
    private IEnumerator hideMessageInternal() {
        Color c = message1.GetComponent<TextMeshPro>().color;
        float startTime = Time.time;
        while (Time.time-startTime < 0.1f) {
            float ratio = (Time.time-startTime)/0.1f;
            ratio = 1-ratio;
            message1.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, ratio);
            message2.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, ratio);
            yield return new WaitForEndOfFrame();
        }

        message1.GetComponent<PulseText>().SetPulsing(false);
        message2.GetComponent<PulseText>().SetPulsing(false);

        message1.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, 0);
        message2.GetComponent<TextMeshPro>().color = new Color(c.r, c.g, c.b, 0);
    }

    public void Stunned(BulletController bullet) {
        stunLevel++;
        if (stunLevel > 3) stunLevel = 3;
        lastStunTime = Time.time;

        GetComponent<AudioSource>().clip = AudioManager.Instance.bulletHit;
        GetComponent<AudioSource>().Play();
    }
}
