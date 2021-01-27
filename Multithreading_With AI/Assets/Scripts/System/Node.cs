using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Node
{
    public bool walkable;
    public Vector3 position;

    public Node(bool _walkable, Vector3 _pos)
    {
        walkable = _walkable;
        position = _pos;
    }
}
