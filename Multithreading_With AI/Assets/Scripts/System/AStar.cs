using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AStar : MonoBehaviour, PathFindInterface
{
    private List<Node> openList;
    private List<Node> closedList;


    // Start is called before the first frame update
    void Start()
    {
        openList = new List<Node>();
        closedList = new List<Node>();
    }

    public void ResetPathFinding()
    {
        openList.Clear();
        closedList.Clear();
    }

    public void Search(PathReqeustInfo requestInfo, Action<PathResultInfo> callback)
    {
        ResetPathFinding();

        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);
        StartNode.h = ComputeHeuristic(StartNode, EndNode);

        Vector3[] waypoints = new Vector3[0];

        if (!StartNode.walkable && !EndNode.walkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "Start path or EndNode is unwalkable!");
            return;
        }

        openList.Add(StartNode);

        bool found = false;
        while (!found && openList.Count > 0)
        {
            Node current = openList[0];
            openList.Remove(current);
            closedList.Add(current);
            if (current.gridX == EndNode.gridX && current.gridY == EndNode.gridY)
            {
                found = true;
                break;
            }
            else
            {
                foreach (var neighbour in Grid.Instance.GetNeighbours(current))
                {
                    int index = neighbour.index;

                    float cost = current.g + ComputeCost(current, neighbour);
                    float f = cost + ComputeHeuristic(neighbour, EndNode);

                    if (!neighbour.walkable || closedList.Contains(neighbour))
                        continue;

                    if (cost < neighbour.g || !openList.Contains(neighbour))
                    {
                        openList.Remove(neighbour);
                        neighbour.parent = current;
                        neighbour.g = cost;

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }

        }

        List<Node> trace = new List<Node>();

        if (found)
        {
            Node node = EndNode;
            while (node != StartNode)
            {
                trace.Add(node);
                node = node.parent;
            }
            trace.Reverse();
            List<Vector3> convertToVec3 = new List<Vector3>();
            foreach (var p in trace)
            {
                convertToVec3.Add(p.position);
            }
            waypoints = convertToVec3.ToArray();
        }
        else
        {
            callback(new PathResultInfo(waypoints, false, requestInfo.callback));
            return;
        }
        callback(new PathResultInfo(waypoints, true, requestInfo.callback));
    }

    private float ComputeCost(Node a, Node b)
    {
        return (a.gridX != b.gridX && a.gridY != b.gridY) ? 1.414f : 1.0f;
    }

    private int ComputeHeuristic(Node a, Node b)
    {
        int heuristic = 0;
        switch(AI.Instance.AStarHeuristics)
        {
            case AStarHeuristics.Chebyshev:
                heuristic = Mathf.Max((a.gridX -b.gridX),(a.gridY - b.gridY));
                break;
            case AStarHeuristics.Euclidean:
                {
                    float xDif = a.gridX - b.gridX;
                    float yDif = a.gridY - b.gridY;
                    heuristic = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(xDif,2.0f) + Mathf.Pow(yDif,2.0f)));
                    break;
                }
            case AStarHeuristics.Manhattan:
                heuristic = Mathf.Abs(a.gridX-b.gridX) + Mathf.Abs(a.gridY - b.gridY);
                break;
        }
        return heuristic;
    }
}
