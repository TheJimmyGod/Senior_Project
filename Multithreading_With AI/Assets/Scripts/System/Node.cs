using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 position;
    public int gridX;
    public int gridY;
    public int index;

    public Node parent;

    public float g;
    public float h;
    public float f
    {
        get { return g + h; }
    }

    public Node(bool _walkable, Vector3 _pos, int _gridX, int _gridY, int _index)
    {
        walkable = _walkable;
        position = _pos;
        gridX = _gridX;
        gridY = _gridY;
        index = _index;
        parent = null;
        g = 0.0f;
        h = 0.0f;
    }
}
