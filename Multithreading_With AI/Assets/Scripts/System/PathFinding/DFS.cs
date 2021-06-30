using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DFS : MonoBehaviour, PathFindInterface
{
    public void Search(PathReqeustInfo requestInfo, Action<PathResultInfo> callback)
    {
        Stack<Node> openList = new Stack<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);

        Vector3[] waypoints = new Vector3[0];

        if(EndNode.walkable == TileType.UnWalkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "EndNode is unwalkable!");
            callback(new PathResultInfo(waypoints, false, requestInfo.callback));
            return;
        }

        openList.Push(StartNode);

        bool found = false;
        while(!found && openList.Count > 0)
        {
            Node current = openList.Pop();
            if (current.gridX == EndNode.gridX && current.gridY == EndNode.gridY)
            {
                found = true;
                break;
            }
            else
            {
                IEnumerable<Node> neighbours = Grid.Instance.GetNeighbours(current);
                int count = neighbours.Count();

                using (IEnumerator<Node> iterator = neighbours.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var neighbour = iterator.Current;
                        int id = neighbour.index;

                        if (closedList.Contains(neighbour))
                            continue;

                        if (neighbour.walkable == TileType.Walkable && !openList.Contains(neighbour))
                        {
                            openList.Push(neighbour);
                            neighbour.parent = current;
                        }
                    }
                }

            }
            closedList.Add(current);
        }

        Stack<Node> trace = new Stack<Node>();

        if (found)
        {
            Node node = EndNode;
            while (node != StartNode)
            {
                if (!trace.Contains(node))
                    trace.Push(node);
                node = node.parent;
            }
            Stack<Vector3> convertToVec3 = new Stack<Vector3>();

            while (trace.Count > 0)
            {
                convertToVec3.Push(trace.Pop().position);
            }
            
            waypoints = convertToVec3.Reverse().ToArray();
            callback(new PathResultInfo(waypoints, true, requestInfo.callback));
        }
        else
        {
            callback(new PathResultInfo(waypoints, false, requestInfo.callback));
            return;
        }

    }
}