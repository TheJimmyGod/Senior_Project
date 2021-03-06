using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathResultInfo
{
    public Vector3[] waypoints;
    public bool IsSuccess;
    public Action<Vector3[], bool> callback;

    public PathResultInfo(Vector3[] _waypoints, bool _isSuccess, Action<Vector3[],bool> _callback)
    {
        waypoints = _waypoints;
        IsSuccess = _isSuccess;
        callback = _callback;
    }
}