using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DFS : MonoBehaviour, PathFindInterface
{
    public void Search(PathReqeustInfo requestInfo, Action<PathResultInfo> callback)
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        Node StartNode = Grid.Instance.GetNodeFromWorld(requestInfo.start);
        Node EndNode = Grid.Instance.GetNodeFromWorld(requestInfo.end);

        Vector3[] waypoints = new Vector3[0];

        if(!StartNode.walkable && !EndNode.walkable)
        {
            Debug.Log("<color=red>Warning!</color>" + " " + "Start path or EndNode is unwalkable!");
            return;
        }

        openList.Add(StartNode);

        bool found = false;
        while(!found && openList.Count > 0)
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

                    if (neighbour.walkable && !openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                        neighbour.parent = current;
                    }
                }
            }

        }

        List<Node> trace = new List<Node>();
        if(found)
        {
            Node node = EndNode;
            while(node != StartNode)
            {
                trace.Add(node);
                node = node.parent;
            }
            trace.Reverse();
            List<Vector3> convertToVec3 = new List<Vector3>();
            foreach(var p in trace)
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
}