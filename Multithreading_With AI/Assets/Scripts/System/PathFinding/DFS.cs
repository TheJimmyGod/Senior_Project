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
        Queue<Node> openList = new Queue<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);

        Vector3[] waypoints = new Vector3[0];

        if(StartNode.walkable == TileType.UnWalkable || EndNode.walkable == TileType.UnWalkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "Start path or EndNode is unwalkable!");
            return;
        }

        openList.Enqueue(StartNode);

        bool found = false;
        while(!found && openList.Count > 0)
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

                    if (neighbour.walkable == TileType.Walkable && !openList.Contains(neighbour))
                    {
                        openList.Enqueue(neighbour);
                        neighbour.parent = current;
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

            while (trace.Count > 0)
            {
                convertToVec3.Enqueue(trace.Dequeue().position);
            }

            convertToVec3 = new Queue<Vector3>(convertToVec3.Reverse());

            waypoints = convertToVec3.ToArray();
            callback(new PathResultInfo(waypoints, true, requestInfo.callback));
        }
        else
        {
            callback(new PathResultInfo(waypoints, false, requestInfo.callback));
            return;
        }

    }
}