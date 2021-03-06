using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Grid : MonoBehaviour
{
    private static Grid _instance;
    public static Grid Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    public float WorldSizeX = 10;
    public float WorldSizeY = 10;

    [SerializeField]
    private float radius = 0.5f;

    public Node[,] grids;

    private float nodeDiameter;
    public int gridSizeX, gridSizeY;

    private Thread t;
    private Vector3 centerPos;

    public GameObject signfier;

    int index = 0;

    private void Start()
    {
        nodeDiameter = radius * 2.0f;

        gridSizeX = Mathf.RoundToInt(WorldSizeX / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(WorldSizeY / nodeDiameter);

        centerPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        if (AI.Instance.gameObject != null)
            AI.Instance.gridSet = true;
    }

    public void CreateGrid()
    {
        //Debug.Log("<color=Red>Start to create grids</color>");
        grids = new Node[gridSizeX,gridSizeY];
        Vector3 worldBottomLeft = centerPos - 
            (Vector3.right * WorldSizeX / 2) - 
            (Vector3.forward * WorldSizeY / 2);

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                Vector3 point = worldBottomLeft + Vector3.right * (x * nodeDiameter + radius) + Vector3.forward * (y * nodeDiameter + radius);
                TileType walkable = new TileType();

                foreach(var v in AI.Instance.environment)
                {
                    Vector2 computeDis = new Vector2(point.x - v.position.x, point.z - v.position.z);
                    float distance = ((computeDis.x * computeDis.x) +
                        (computeDis.y * computeDis.y)); // Mgnitude V.x * v.x + v.y * v.y
                    distance = Mathf.Sqrt(distance); // Sqrf(Mgnitude)
                    if (v.TypeName == "Wall")
                    {
                        walkable = (distance > 0.75f) ? TileType.Walkable : TileType.UnWalkable;
                        if (walkable == TileType.UnWalkable) break;
                    }
                    else if (v.TypeName == "Bush")
                    {
                        walkable = (distance > 0.75f) ? TileType.Walkable : TileType.Bush;
                        if (walkable == TileType.Bush) break;
                    }
                }

                grids[x,y] = new Node(walkable, point, x,y,index);
                index++;
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkY < gridSizeX
                    && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(grids[checkX,checkY]);
            }

        }
        return neighbours;
    }

    public Node GetNodeFromWorld(Vector3 worldPos)
    {
        float percentX = (worldPos.x + WorldSizeX / 2) / WorldSizeX;
        float percentY = (worldPos.z + WorldSizeY / 2) / WorldSizeY;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX -1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY -1) * percentY);
        return grids[x,y]; 
    }

    public List<GameObject> GenerateTile(Vector3[] _path)
    {
        List<GameObject> tiles = new List<GameObject>(_path.Length);
        int odd = 0;
        foreach(var p in _path)
        {
            if (odd == _path.Length - 1)
                break;

            odd++;
            if (odd % 2 == 1)
                continue;
            else
            {
                Node node = GetNodeFromWorld(p);
                GameObject e = GameObject.Instantiate(signfier, new Vector3(node.position.x,-0.1f, node.position.z), Quaternion.identity);
                tiles.Add(e);
            }
            
        }
        return tiles;
    }


    private void OnDrawGizmos()
    {
        if (Instance == null)
            return;

        Gizmos.DrawWireCube(transform.position, new Vector3(WorldSizeX, 1f, WorldSizeY));
        if (AI.Instance.displayGridList == false)
            return;
        if(grids != null)
        {
            foreach(var node in grids)
            {
                switch(node.walkable)
                {
                    case TileType.Walkable:
                        Gizmos.color = Color.blue;
                        break;
                    case TileType.UnWalkable:
                        Gizmos.color = Color.red;
                        break;
                    case TileType.Bush:
                        Gizmos.color = Color.green;
                        break;
                    default:
                        Gizmos.color = Color.grey;
                        break;
                }
                if(node.walkable == TileType.UnWalkable)
                {
                    Gizmos.DrawWireCube(new Vector3(node.position.x,0.5f,node.position.z), Vector3.one * (nodeDiameter - 0.1f));
                }
                else
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
                    if (player != null)
                    {
                        Node PlayerPos = GetNodeFromWorld(player.transform.position);
                        if (PlayerPos.position == node.position)
                        {
                            Gizmos.color = Color.green;
                        }
                    }
                    Gizmos.DrawWireCube(node.position, Vector3.one * (nodeDiameter - 0.1f));
                }

            }

        }
    }
}
