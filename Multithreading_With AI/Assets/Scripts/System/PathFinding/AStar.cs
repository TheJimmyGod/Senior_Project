using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AStar : MonoBehaviour, PathFindInterface
{

    public void Search(PathReqeustInfo requestInfo, Action<PathResultInfo> callback)
    {
        Queue<Node> openList = new Queue<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);
        StartNode.h = ComputeHeuristic(StartNode, EndNode);

        Vector3[] waypoints = new Vector3[0];

        if (StartNode.walkable == TileType.UnWalkable || EndNode.walkable == TileType.UnWalkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "Start path or EndNode is unwalkable!");
            return;
        }

        openList.Enqueue(StartNode);

        bool found = false;
        while (!found && openList.Count > 0)
        {
            Node current = openList.Dequeue();
            closedList.Add(current);
            if (current.gridX == EndNode.gridX && current.gridY == EndNode.gridY)
            {
                found = true;
                break;
            }
            else
            {
                IEnumerable<Node> neighbours = Grid.Instance.GetNeighbours(current);
                int count = neighbours.Count();

                for (int i = 0; i < count; ++i)
                {
                    var neighbour = neighbours.ElementAt(i);
                    int index = neighbour.index;

                    float cost = current.g + ComputeCost(current, neighbour);
                    float f = cost + ComputeHeuristic(neighbour, EndNode);

                    if (neighbour.walkable != TileType.Walkable || closedList.Contains(neighbour))
                        continue;

                    if (cost < neighbour.g || !openList.Contains(neighbour))
                    {
                        neighbour.parent = current;
                        neighbour.g = cost;

                        openList.Enqueue(neighbour);
                    }
                }
            }

        }

        Queue<Node> trace = new Queue<Node>();

        if (found)
        {
            Node node = EndNode;
            while (node != StartNode)
            {
                trace.Enqueue(node);
                node = node.parent;
            }
            Queue<Vector3> convertToVec3 = new Queue<Vector3>();

            while(trace.Count > 0)
            {
                convertToVec3.Enqueue(trace.Dequeue().position);
            }

            convertToVec3 = new Queue<Vector3>(convertToVec3.Reverse());

            waypoints = convertToVec3.ToArray();
        }
        else
        {
            callback(new PathResultInfo(waypoints, false, requestInfo.callback));
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
