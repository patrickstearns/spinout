using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Cinemachine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    public enum SpecialMode { None, FPS };
    public enum TargetEffect { None, Thump };

    private bool fpsSpecialModeActive = false;

    private AudioManager audioManager;
    private GameObject levelClone;
    private bool challengeMode = false;
    private List<TargetController> targets;
    private int initialDestructibleTargetCount = 0;
    private bool listening = false;

    public int activeBallCount = 0;
    public CinemachineVirtualCamera vcamera, paddleCameraL, paddleCameraM, paddleCameraR;
    public PaddleController paddle;
    public FieldController field;
    public GameObject offBoardSurface;
    public MainMenuController mainMenu;
    public GameOverMenuController gameOverMenu;
    public LevelMessageUIController levelMessageUI;
    public InstructionsMenuController instructionsMenu;
    public Collider backWall;
    public Collider[] walls;
    public ScoreboardController scoreboard;
    public List<GameObject> levelTargets;
    public List<GameObject> arcadeLevelTargets;

    void Awake() {
        if (_instance != null && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    public void Start() {
        scoreboard.gameObject.SetActive(false);
        field.gameObject.SetActive(false);
//        offBoardSurface.SetActive(false);
        paddle.Poof();

        audioManager = GetComponent<AudioManager>();
        audioManager.PlayMenuBGM();
    }

    public void FixedUpdate() {
        if (fpsSpecialModeActive) {
            vcamera.Priority = 9;
            float paddleSpeed = paddle.GetSpeed();
            paddleCameraL.Priority = (paddleSpeed < 0) ? 10 : 9;
            paddleCameraM.Priority = (paddleSpeed == 0) ? 10 : 9;
            paddleCameraR.Priority = (paddleSpeed > 0) ? 10 : 9;
        }
        else {
            vcamera.Priority = 10;
            paddleCameraL.Priority = 9;
            paddleCameraM.Priority = 9;
            paddleCameraR.Priority = 9;
        }
    }

    public void NewArcadeGame(int initialLevel) {
        field.gameObject.SetActive(true);
//        offBoardSurface.SetActive(true);
        scoreboard.SetLevel(initialLevel);
        scoreboard.SetEnergy(0);
        scoreboard.gameObject.SetActive(true);

        audioManager.SetGameMusicPlaying(true);

        //show paddle
        paddle.transform.position = new Vector3(0, 0, 1.5f);
        paddle.SetBallsHeld(3);
        paddle.PopUp();

        challengeMode = false;

        //instantiate and prepare our targets
        BeginLevel(PrepareArcadeLevelTargets(scoreboard.GetLevel()));
    }

    public void NewChallengeGame(int initialLevel) {
        field.gameObject.SetActive(true);
//        offBoardSurface.SetActive(true);
        scoreboard.SetLevel(initialLevel);
        scoreboard.SetEnergy(0);
        scoreboard.gameObject.SetActive(true);

        audioManager.SetGameMusicPlaying(true);

        //show paddle
        paddle.transform.position = new Vector3(0, 0, 1.5f);
        paddle.SetBallsHeld(3);
        paddle.PopUp();

        challengeMode = true;

        //instantiate and prepare our targets
        BeginLevel(PrepareChallengeLevelTargets(scoreboard.GetLevel()));
    }

    private GameObject PrepareArcadeLevelTargets(int level) {
        int originalLevel = level;

        level--; //0-base it

        int arrSize = 10;
        if (level % 5 == 0 || level % 5 == 4) arrSize = 6;
        else if (level % 5 == 1 || level % 5 == 3) arrSize = 8;

        //hardcoded level boost progression, check input
        int[] levelsToRows6 = new int[]{ 6, 5, 6, 4, 5, 6, 0, 3, 4, 5, 6, 0, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 0, 1, 2, 3, 0, 1, 2, 0, 1 };        
        int[] levelsToRows8 = new int[]{ 8, 7, 8, 6, 7, 8, 0, 5, 6, 7, 8, 4, 0, 5, 6, 7, 8, 3, 4, 5, 0, 6, 7, 8, 2, 0, 3, 4, 5, 6, 0, 7, 1, 2, 3, 4, 5, 0, 6, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 1, 0, 2, 3, 1, 2, 1, 0};        
        int[] levelsToRowsT = new int[]{ 10, 9, 10, 8, 9, 10, 0, 7, 8, 9, 10, 6, 7, 0, 8, 9, 10, 5, 6, 7, 8, 0, 9, 10, 4, 5, 6, 7, 8, 0, 9, 3, 4, 5, 6, 7, 8, 0, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 0, 6, 1, 2, 3, 4, 5, 1, 2, 3, 0, 4, 1, 2, 3, 1, 2, 1, 0 };
        int[] levelsToRows = arrSize == 6 ? levelsToRows6 : arrSize == 8 ? levelsToRows8 : levelsToRowsT;

        //initialize array to 6x6, 8x8, or 10x10 depending on level number
        int[][] hardnesses;
        hardnesses = new int[arrSize][];
        for (int i = 0; i < hardnesses.Length; i++) {
            hardnesses[i] = new int[arrSize];
            for (int j = 0; j < hardnesses[i].Length; j++) 
                hardnesses[i][j] = 1;
        }

        //perform target hp boosts
        int turretBoosts = 0;
        for (int i = level-1; i >= 0; i--) {
            if (i >= levelsToRows.Length) continue;
            int rowToBoost = levelsToRows[i];
            if (rowToBoost == 0) turretBoosts++;
            else {
                rowToBoost--;
                for (int t = 0; t < hardnesses.Length; t++) {
                    hardnesses[t][rowToBoost]++;
                }
            }
        }

        //scramble targets a bit - randomly pick two, swap their hardnesses, do [level] times
        for (int i = 0; i < level; i++) {
            int ay = UnityEngine.Random.Range(0, hardnesses.Length-1);
            int ax = UnityEngine.Random.Range(0, hardnesses[ay].Length-1);
            int by = UnityEngine.Random.Range(0, hardnesses.Length-1);
            int bx = UnityEngine.Random.Range(0, hardnesses[by].Length-1);
            int ha = hardnesses[ay][ax];
            int hb = hardnesses[by][bx];
            hardnesses[ay][ax] = hb;
            hardnesses[by][bx] = ha;
        }

        //create appropriate level clone
        GameObject thisLevelTargets = arcadeLevelTargets[(level)%arcadeLevelTargets.Count];
        levelClone = Instantiate(thisLevelTargets, field.transform);
        targets = new List<TargetController>();
        List<TargetController> extraTargets = new List<TargetController>();
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>()){
            target.Hide();
            if (target.name.Contains("_")) targets.Add(target);
            else extraTargets.Add(target); //for ones that aren't in the standard block pattern; e.g. turrets and captive balls
        }
        levelClone.transform.position = Vector3.zero;
        levelClone.GetComponent<LevelInfo>().message = "Level "+originalLevel;

        //sort targets; rename all the targets in inspector with something we can sort on
        targets.Sort(CompareTargetsByName);

        //reassign materials to match hp
        for (int i = 0; i < hardnesses.Length; i++) {
            for (int j = 0; j < hardnesses[i].Length; j++) {
                TargetController target = targets[j*arrSize + (arrSize-i) - 1];
                
                Material mat = PrefabsManager.Instance.targetRed;
                if (hardnesses[j][i] == 2) mat = PrefabsManager.Instance.targetOrange; 
                if (hardnesses[j][i] == 3) mat = PrefabsManager.Instance.targetYellow; 
                if (hardnesses[j][i] == 4) mat = PrefabsManager.Instance.targetGreen; 
                if (hardnesses[j][i] == 5) mat = PrefabsManager.Instance.targetBlue; 
                if (hardnesses[j][i] == 6) mat = PrefabsManager.Instance.targetPurple; 
                if (hardnesses[j][i] == 7) mat = PrefabsManager.Instance.targetWhite; 

                target.hp = hardnesses[j][i];
                target.scoreValue = hardnesses[j][i];
                target.GetComponent<Renderer>().material = mat;
            }
        }

        //destroy some for variety
        for (int i = level; i < 20; i++) {
            TargetController target = targets[UnityEngine.Random.Range(0, targets.Count)];
            targets.Remove(target);
            Destroy(target.gameObject);
        }

        //add back in the extra targets
        targets.AddRange(extraTargets);

        //upgrade turrets
        int rof = 3;
        if (turretBoosts < 4) rof = 5-turretBoosts;
        else if (turretBoosts < 8) rof = 8-turretBoosts;
        foreach (TurretController turret in levelClone.GetComponentsInChildren<TurretController>()){
            turret.FireEverySeconds = rof;
            turret.FireAtPaddle = turretBoosts > 4;
            turret.BulletsDestroyTargets = turretBoosts > 8;
        }

        return levelClone;
    }

    public static int CompareTargetsByName(TargetController one, TargetController two) {
        string[] ones = one.name.Split("_");
        string[] twos = two.name.Split("_");
        int oneX = Int32.Parse(ones[1]);
        int oneY = Int32.Parse(ones[2]);
        int twoX = Int32.Parse(twos[1]);
        int twoY = Int32.Parse(twos[2]);
        if (oneX != twoX) return twoX-oneX;
        return twoY-oneY;
    }

    private GameObject PrepareChallengeLevelTargets(int level) {
        GameObject thisLevelTargets = levelTargets[(level-1)%levelTargets.Count];
        levelClone = Instantiate(thisLevelTargets, field.transform);

        targets = new List<TargetController>();
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>()){
            target.Hide();
            targets.Add(target);
        }

        levelClone.transform.position = Vector3.zero;
        return levelClone;
    }

    public void BeginLevel(GameObject levelClone) { StartCoroutine(beginLevelInternal(levelClone)); }
    private IEnumerator beginLevelInternal(GameObject levelClone) {
        //show targets in random order
        initialDestructibleTargetCount = 0;
        foreach (TargetController target in targets){
            target.PopUp();            
            if (!target.indestructible) initialDestructibleTargetCount++;
            yield return new WaitForSeconds(0.5f/targets.Count);
        }

        //show message UI until we get input, then hide it
        levelMessageUI.Show(levelClone.GetComponent<LevelInfo>().message);
        listening = true;
        yield return new WaitUntil(() => anyInputActive());
        listening = false;
        levelMessageUI.Hide();
        yield return new WaitForSeconds(LevelMessageUIController.PopTime);

        //actually start play
        activeBallCount = 0;
        ActivateLaunchMode(0f);
    }

    private bool anyInputActive(){ return listening && Input.anyKey; }

    public void ActivateLaunchMode(float delay) { StartCoroutine(activateLaunchModeInternal(delay)); }
    private IEnumerator activateLaunchModeInternal(float delay) {
        if (delay > 0) yield return new WaitForSeconds(delay);

        paddle.ActivateLaunchMode();
        paddle.SetBallsHeld(paddle.GetBallsHeld()-1);
        paddle.controllable = true;
    }


    public void LaunchBall() {
        activeBallCount++;
        paddle.ActivateBounceMode();
        paddle.Reshape();
        
        GameObject ball = BallController.Spawn(paddle.transform.position + new Vector3(0, 0, 1f), false);
        ball.GetComponent<BallController>().ActivateCollisions();
        ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 20), ForceMode.Impulse);

        ActivateTurrets();

        paddle.actionable = true;
    }

    private void ActivateTurrets() {
        foreach (TurretController turret in levelClone.GetComponentsInChildren<TurretController>())
            turret.activated = true;
    }

/*
    private List<TargetController> shuffle(TargetController[] array) {
        List<TargetController> list = new List<TargetController>(array);
        List<TargetController> shuffled = new List<TargetController>();
        while (list.Count > 0) {
            TargetController item = list[UnityEngine.Random.Range(0, list.Count)];
            list.Remove(item);
            shuffled.Add(item);
        }
        return shuffled;
    }
*/

    public void TargetExploded(TargetController target) {
        targets.Remove(target);

        ScoreEffectController.Spawn(target.transform.position, ""+target.scoreValue);

        int remainingValidTargets = 0;
        foreach (TargetController t in targets)
            if (!t.indestructible && !(t is CaptiveBallTargetController))
                remainingValidTargets++;

        if (remainingValidTargets == 0){
            if (activeBallCount > 0) {
                paddle.ActivateCatchMode();
                SetSpecialModeActive(SpecialMode.FPS, true);
            }
            else { //happens if a turret destroys the final target
                LevelComplete();
            }
        }
        else if (remainingValidTargets <= initialDestructibleTargetCount / 2) {
            initialDestructibleTargetCount = 0; //so we don't do this twice
            SpawnThumperTarget();
        }

        //activate any special modes this thing might be able to do
        if (target.targetEffect == TargetEffect.Thump) {
            ThumpEffectController.Spawn(target.transform.position + new Vector3(0, -0.5f, 0), AudioManager.Instance.turretExploded); //TODO: different SFX, something thumpey
        }
    }

    private void SpawnThumperTarget() {
        List<TargetController> destructibles = new List<TargetController>();
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>())
            if (!target.indestructible)
                destructibles.Add(target);

        TargetController rando = destructibles[UnityEngine.Random.Range(0, destructibles.Count)];
        rando.hp = 1;
        rando.scoreValue = 10;
        rando.targetEffect = TargetEffect.Thump;
        rando.GetComponent<Renderer>().material = PrefabsManager.Instance.special;

        rando.GetComponent<AudioSource>().clip = AudioManager.Instance.targetUpgrade;
        rando.GetComponent<AudioSource>().Play();

    }

    public void Thump(Vector3 position) {
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>())
            if (!target.indestructible && target.hp > 0 && Vector3.Distance(position, target.transform.position) < 8f) { 
                float deg = UnityEngine.Random.Range(-45, 45);
                target.transform.localRotation = Quaternion.Euler(new Vector3(0, deg, 0));
                target.collided(null, true);
            }
    }

    public void Gain1UP() {
        paddle.SetBallsHeld(paddle.GetBallsHeld()+1);
        paddle.GetComponent<AudioSource>().clip = AudioManager.Instance.got1UP;
        paddle.GetComponent<AudioSource>().Play();
    }

    public void SelfDestruct() {
        foreach (BallController ball in FindObjectsOfType<BallController>()) {
            ball.Explode();
        }
    }

    public void BallPoofed() {
        activeBallCount--;
    }

    public void BallExploded() {
        activeBallCount--;
        if (activeBallCount == 0) {
            //deactivate any special modes
            foreach (SpecialMode mode in Enum.GetValues(typeof(SpecialMode)).Cast<SpecialMode>())
                SetSpecialModeActive(mode, false);

            if (paddle.GetBallsHeld() <= 0) GameOver();
            else if (paddle.IsCatchMode()) LevelComplete();
            else ActivateLaunchMode(1f);
        }
    }

    public void BallCaught() {
        activeBallCount--;
        paddle.CaughtBall();
        if (activeBallCount == 0) LevelComplete();
    }

    public void LevelComplete() { StartCoroutine(levelCompleteInternal()); }
    private IEnumerator levelCompleteInternal() {
        paddle.actionable = false;

        //poof the indestructibles, captive balls, and the overall level targets clone object
        foreach (BulletController bullet in FindObjectsByType<BulletController>(FindObjectsSortMode.None)) bullet.Poof();
        foreach (TurretController turret in levelClone.GetComponentsInChildren<TurretController>()) turret.Exploded();
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>()) target.Poof();

        bool fpsModeWasActive = fpsSpecialModeActive;
        SetSpecialModeActive(SpecialMode.FPS, false);

        yield return new WaitForSeconds(1f);

        Destroy(levelClone);
        paddle.ActivateBounceMode();

        yield return new WaitForSeconds(2f);

        ScoreEffectController.Spawn(new Vector3(0, 0, 15f), "LEVEL COMPLETE BONUS +10 ENERGY");
        EnergyBallEffectController energyBall = EnergyBallEffectController.Spawn(new Vector3(0, 0, 15f), 10).GetComponent<EnergyBallEffectController>();

        yield return new WaitUntil(() => !energyBall.Happening);

        //drain the full energy pods to create extra balls
        int paddleBallCapacity = 5-paddle.GetBallsHeld();
        int fullEnergyPods = scoreboard.GetEnergy()/50;
        int ballsEarned = Math.Min(paddleBallCapacity, fullEnergyPods);
        List<ExtraBallEffectController> extraBallEffects = new List<ExtraBallEffectController>();
        for (int i = 0; i < ballsEarned; i++) {
            Vector3 effectPos = scoreboard.GetRightmostFullEnergyPod().transform.position;
            scoreboard.IncrementEnergy(-50);
            yield return new WaitForSeconds(0.25f);
            extraBallEffects.Add(ExtraBallEffectController.Spawn(effectPos).GetComponent<ExtraBallEffectController>());
        }
        if (ballsEarned > 0){
            yield return new WaitUntil(() => { 
                bool anyHappening = false;
                foreach (ExtraBallEffectController extraBallEffect in extraBallEffects)
                    if (extraBallEffect.Happening)
                        anyHappening = true;
                return !anyHappening;
            });
        }

        scoreboard.IncrementLevel(1);

        if (challengeMode) BeginLevel(PrepareChallengeLevelTargets(scoreboard.GetLevel()));
        else BeginLevel(PrepareArcadeLevelTargets(scoreboard.GetLevel()));
    }

    private void GameOver() { StartCoroutine(gameOverInternal()); }
    private IEnumerator gameOverInternal() {
        foreach (BallController ball in FindObjectsByType<BallController>(FindObjectsSortMode.None)) ball.BeamOut();
        foreach (BulletController bullet in FindObjectsByType<BulletController>(FindObjectsSortMode.None)) bullet.Poof();
        foreach (TurretController turret in levelClone.GetComponentsInChildren<TurretController>()) turret.Exploded();
        foreach (TargetController target in levelClone.GetComponentsInChildren<TargetController>()) target.Poof();
        paddle.controllable = false;
        paddle.actionable = false;
        Destroy(levelClone);

        ScoreEffectController.Spawn(new Vector3(0, 0, 15f), "GAME OVER");

        yield return new WaitForSeconds(3);

        gameOverMenu.Show();
    }

    public void ContinueGame() {
        gameOverMenu.Hide();
        if (challengeMode) NewChallengeGame(scoreboard.GetLevel());
        else NewArcadeGame(scoreboard.GetLevel());
    }

    public void GiveUpGame() {
        gameOverMenu.Hide();
        Start();
        mainMenu.Show();
    }

    public void ShowInstructions() {
        instructionsMenu.Show();
    }

    public void HideInstructions() {
        instructionsMenu.Hide();
        mainMenu.Show();
    }

    public void SetSpecialModeActive(SpecialMode mode, bool active) {
        if (mode == SpecialMode.FPS) {
            if (active == fpsSpecialModeActive) return;
            fpsSpecialModeActive = active;
        }
    }

    public void AbilityTriggered() { 
        scoreboard.IncrementEnergy(-20);
        foreach (BallController ball in FindObjectsByType<BallController>(FindObjectsSortMode.None)) {
            StartCoroutine(slowBallAndShowTrajectory(ball)); 
        }
    }

    private IEnumerator slowBallAndShowTrajectory(BallController ball) {
        Rigidbody body = ball.GetComponent<Rigidbody>();
        Vector3 ballV = body.velocity;
        Vector3 ballAngularV = body.angularVelocity;
        float musicVolume = AudioManager.Instance.GetVolume();

        AudioManager.Instance.SetLowpassEffectEnabled(true); //underwatery style effect

        //draw the trajectory arrow
        ball.GetComponent<Projection>().Init(SceneManager.GetActiveScene());

        float slowTime = 0.25f;
        float startTime = Time.time;
        while (Time.time-startTime < slowTime) {
            float ratio = 1 - ((Time.time-startTime) / slowTime);
            body.velocity = ballV * ratio;
            body.angularVelocity = ballAngularV * ratio;

            if (ratio >= 0.5f) AudioManager.Instance.ChangeMasterVolume(musicVolume * ratio);

            ball.GetComponent<Projection>().SimulateTrajectory(ball, ballV, ballAngularV, (int)((Time.time-startTime)*100));
            yield return new WaitForEndOfFrame();
        }
        body.velocity = Vector3.zero;

        startTime = Time.time;
        while (Time.time-startTime < 3f) {
            ball.GetComponent<Projection>().SimulateTrajectory(ball, ballV, ballAngularV, 100);
            yield return new WaitForEndOfFrame();
        }
        ball.GetComponent<Projection>().Stop();
        AudioManager.Instance.SetLowpassEffectEnabled(false);

        startTime = Time.time;
        while (Time.time-startTime < slowTime) {
            float ratio = (Time.time-startTime) / slowTime;
            if (ratio >= 0.5f) AudioManager.Instance.ChangeMasterVolume(musicVolume * ratio);
            yield return new WaitForEndOfFrame();
        }

        body.AddForce(ballV, ForceMode.Impulse);
        body.AddTorque(ballAngularV, ForceMode.Impulse);
    }

}
