using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    private static AI _instance;
    public static AI Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    public List<Vector3> environment;
    public GameObject enemy;
    public GameObject player;

    [SerializeField]
    private int EnemyCountForSpawning = 1;

    private void Start()
    {
        environment = new List<Vector3>();
        GameObject[] envs = GameObject.FindGameObjectsWithTag("Environment");
        foreach(var E in envs)
        {
            environment.Add(E.transform.position);
        }

        for (int i = 0; i < EnemyCountForSpawning; i++)
        {
            float randomPosition = Random.Range(-0.1f, 0.3f);
            GameObject.Instantiate(enemy, new Vector3 (_setting.Start.x + randomPosition, 
                1.0f, _setting.Start.z + randomPosition), Quaternion.identity);
        }

        GameObject.Instantiate(player, new Vector3(_setting.End.x + 0.1f,
    1.0f, _setting.End.z + 0.1f), Quaternion.identity);
    }

    // There will be set functions that executions of AI behaviours
    // Pathfinding, State machine, and Steering machine
    // ------------------------------
    [System.Serializable]
    private struct DestinationSet
    {
        public Vector3 Start;
        public Vector3 End;
    }
    [SerializeField]
    private DestinationSet _setting;

    private bool isGridSet;
    public bool gridSet
    {
        set
        {
            Debug.Log("<color=green>Grid has been created</color>");
            isGridSet = value;
        }
        get
        {
            return isGridSet;
        }
    }

    // void ExecuteDFS()

    // void ExecuteAStar()

    // ------------------------------

    // void ExecuteStateMachine()

    // ------------------------------

    // void ExecuteSteeringMachine()

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_setting.Start, 0.25f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_setting.End, 0.25f);
    }
}
