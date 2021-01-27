using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Grid : MonoBehaviour
{
    public LayerMask isUnwalkable;
    [SerializeField]
    private float WorldSizeX = 10;
    [SerializeField]
    private float WorldSizeY = 10;

    [SerializeField]
    private float radius = 0.5f;

    [SerializeField]
    private List<Node> grids;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private Vector3 centerPos;

    private void Start()
    {
        nodeDiameter = radius * 2.0f;

        gridSizeX = (int)(WorldSizeX / nodeDiameter);
        gridSizeY = (int)(WorldSizeY / nodeDiameter);

        grids = new List<Node>();
        centerPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        Thread t = new Thread(CreateGrid);
        t.Start();

        if (AI.Instance.gameObject != null)
            AI.Instance.gridSet = true;
    }

    private void CreateGrid()
    {
        Vector3 worldBottomLeft = centerPos - 
            (Vector3.right * WorldSizeX / 2) - 
            (Vector3.forward * WorldSizeY / 2);

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                Vector3 point = worldBottomLeft + Vector3.right * (x * nodeDiameter + radius) + Vector3.forward * (y * nodeDiameter + radius);
                bool walkable = true;

                foreach(var v in AI.Instance.environment)
                {
                    Vector2 computeDis = new Vector2(point.x - v.x, point.z - v.z);
                    float distance = Mathf.Abs((computeDis.x * computeDis.x) +
                        (computeDis.y * computeDis.y));
                    distance = Mathf.Sqrt(distance);
                    walkable = (distance > 0.75f) ? true : false;
                    if (!walkable) break;
                }

                
                Node grid = new Node(walkable, point);
                grids.Add(grid);
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(WorldSizeX, 1f, WorldSizeY));

        if(grids != null)
        {
            foreach(var node in grids)
            {
                Gizmos.color = (node.walkable) ? Color.blue : Color.red;
                if(node.walkable == false)
                {
                    Gizmos.DrawWireCube(new Vector3(node.position.x,0.5f,node.position.z), Vector3.one * (nodeDiameter - 0.1f));
                }
                else
                {
                    Gizmos.DrawWireCube(node.position, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
    }
}
