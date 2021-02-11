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