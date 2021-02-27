using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathFindOptions
{
    DFS,
    AStar
}

public enum AStarHeuristics
{
    Chebyshev,
    Euclidean,
    Manhattan
}

public enum ThreadingType
{
    Thread,
    Task
}

public enum TileType
{
    Walkable,
    UnWalkable,
    Bush
}
