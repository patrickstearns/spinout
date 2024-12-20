using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projection : MonoBehaviour {
    [SerializeField] private LineRenderer _line;
    [SerializeField] public Transform _obstaclesParent;

    public Scene MainScene;

    private Scene _simulationScene;
    private PhysicsScene _physicsScene;
    private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

    public void Init(Scene MainScene) {
        this.MainScene = MainScene;
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene() {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();

        foreach (Transform obj1 in _obstaclesParent) {
            var ghostObj1 = Instantiate(obj1.gameObject, obj1.position, obj1.rotation);
            if (ghostObj1.GetComponent<Renderer>() != null) 
                ghostObj1.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostObj1, _simulationScene);
            if (!ghostObj1.isStatic) _spawnedObjects.Add(obj1, ghostObj1.transform);

            foreach (Transform ghostObj in ghostObj1.transform) {
                //already created above; var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
                if (ghostObj.GetComponent<Renderer>() != null) 
                    ghostObj.GetComponent<Renderer>().enabled = false;
                //already there; SceneManager.MoveGameObjectToScene(ghostObj.gameObject, _simulationScene);
                if (!ghostObj.gameObject.isStatic) _spawnedObjects.Add(ghostObj, ghostObj.transform);
            }
        }
    }
/*
    private void Update() {
        foreach (var item in _spawnedObjects) {
            item.Value.position = item.Key.position;
            item.Value.rotation = item.Key.rotation;
        }
    }
*/
    public void SimulateTrajectory(BallController ballPrefab, Vector3 velocity, Vector3 angularVelocity, int maxIterations) {        
        GameObject ghostObj = BallController.Spawn(ballPrefab.transform.position, true);
        ghostObj.transform.SetParent(null); //not sure if this'll do bad things later
        SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
        
        ghostObj.GetComponent<BallController>().ActivateCollisions();

        ghostObj.GetComponent<Rigidbody>().velocity = velocity;
        ghostObj.GetComponent<Rigidbody>().angularVelocity = angularVelocity;

        _line.positionCount = maxIterations;

        for (int i = 0; i < maxIterations; i++) {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ghostObj.transform.position);
        }

        SceneManager.MoveGameObjectToScene(ghostObj, MainScene);
        PrefabPoolManager.Instance.PoolFor(PrefabsManager.Instance.ballPrefab.name).ReturnObjectToPool(ghostObj);
    }

    public void Stop() {
        SceneManager.UnloadSceneAsync("Simulation");
        _line.positionCount = 0;
        _spawnedObjects.Clear();
    }
}
