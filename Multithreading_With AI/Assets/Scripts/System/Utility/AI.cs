using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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

    //public List<Node> closedList = new List<Node>();
    public List<EnvironmentID> environment = new List<EnvironmentID>();
    public GameObject enemy;
    public GameObject player;

    public DFS dfs;
    public AStar aStar;
    //public FlowField fField;

    //public bool displayClosedList = false;
    public bool displayGridList = false;

    [SerializeField]
    private PathFindOptions _pathFindOptions = PathFindOptions.AStar;
    [SerializeField]
    private AStarHeuristics _AStarHeuristics = AStarHeuristics.Manhattan;

    [Range(1,10)]
    public int EnemyCountForSpawning = 1;

    [Range(1, 4)]
    public int ThreadVaild = 1;

    [Range(1.0f, 50.0f)]
    public float PlayerSpeed = 1.0f;

    private GameObject _playerObj = null;

    // There will be set functions that executions of AI behaviours
    // Pathfinding, State machine, and Steering machine
    // ------------------------------

    [SerializeField]
    private DestinationSet _setting;

    public DestinationSet Setting
    {
        get { return _setting; }
    }


    private bool isGridSet;
    public bool gridSet
    {
        set { Debug.Log("<color=green>Grid has been created</color>"); isGridSet = value; }
        get { return isGridSet; }
    }

    public PathFindOptions pathFindOptions
    {
        set { _pathFindOptions = value; }
        get { return _pathFindOptions; }
    }

    public AStarHeuristics AStarHeuristics
    {
        set { _AStarHeuristics = value; }
        get { return _AStarHeuristics; }
    }

    private void Start()
    {
        dfs = GetComponent<DFS>();
        aStar = GetComponent<AStar>();
        GameObject[] envs = GameObject.FindGameObjectsWithTag("Environment");
        foreach(var E in envs)
        {
            environment.Add(E.GetComponent<EnvironmentID>());
        }

        Grid.Instance.CreateGrid();

        for (int i = 0; i < EnemyCountForSpawning; i++)
        {
            float randomPosition = Random.Range(-1.0f, 1.0f);
            GameObject e = GameObject.Instantiate(enemy, new Vector3 (_setting.Start.x + randomPosition, 
                1.0f, _setting.Start.z + randomPosition), Quaternion.identity);
            e.gameObject.name = "Enemy" + i;
        }

        GameObject e2 = GameObject.Instantiate(enemy, new Vector3(_setting.Start.x + 13.0f,
     1.0f, _setting.Start.z + 0.5f), Quaternion.identity);

        e2.GetComponent<Enemy>().type = ThreadingType.Task;

        GameObject.Instantiate(player, new Vector3(_setting.End.x + 0.1f,
    1.0f, _setting.End.z + 0.1f), Quaternion.identity);

        _playerObj = GameObject.FindGameObjectWithTag("Player").gameObject;
        _playerObj.GetComponent<Player>().SetSpeed(PlayerSpeed);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            displayGridList = !displayGridList;
        }
        if(_playerObj != null)
        {
            AI.Instance._setting.End = _playerObj.transform.position;
            _playerObj.GetComponent<Player>().SetSpeed(PlayerSpeed);
        }
    }

    public void ExecutePathFindingDFS(PathReqeustInfo request, Action<PathResultInfo> result)
    {
        Debug.Log("<color=blue>DFS executed</color>");
        dfs.Search(request, result);
    }

    public void ExecutePathFindingAStar(PathReqeustInfo request, Action<PathResultInfo> result)
    {
        Debug.Log("<color=blue>A* executed</color>");
        aStar.Search(request, result);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_setting.Start, 0.25f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_setting.End, 0.25f);
    }


}

[System.Serializable]
public struct DestinationSet
{
    public Vector3 Start;
    public Vector3 End;
}