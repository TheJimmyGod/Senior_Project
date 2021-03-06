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
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);
        StartNode.h = ComputeHeuristic(StartNode, EndNode);

        Vector3[] waypoints = new Vector3[0];

        if (EndNode.walkable == TileType.UnWalkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "EndNode is unwalkable!");
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
                IEnumerable<Node> neighbours = Grid.Instance.GetNeighbours(current);

                IEnumerator<Node> iterator = neighbours.GetEnumerator();
                while(iterator.MoveNext())
                {
                    var neighbour = iterator.Current;
                    int index = neighbour.index;

                    float cost = current.g + ComputeCost(current, neighbour);

                    if (neighbour.walkable == TileType.UnWalkable || closedList.Contains(neighbour))
                        continue;

                    if (cost < neighbour.g || !openList.Contains(neighbour))
                    {
                        neighbour.parent = current;
                        neighbour.g = cost;
                        neighbour.h = ComputeHeuristic(neighbour, EndNode);

                        int E_index = 0;
                        Node temp = null;
                        IEnumerator<Node> NodeEnumerator = openList.GetEnumerator();
                        if (openList.Count > 0)
                        {
                            while (NodeEnumerator.MoveNext())
                            {
                                E_index++;
                                temp = Grid.Instance.grids[NodeEnumerator.Current.gridX, NodeEnumerator.Current.gridY];
                                if (neighbour.f < temp.g + temp.h)
                                {
                                    break;
                                }
                            }
                        }
                        if (!openList.Contains(temp))
                            openList.Add(neighbour);
                        else
                            openList.Insert(E_index, neighbour);
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
