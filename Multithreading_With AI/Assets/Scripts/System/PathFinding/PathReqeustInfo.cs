using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathReqeustInfo
{
    public Vector3 start;
    public Vector3 end;
    public Action<Vector3[], bool> callback;

    public PathReqeustInfo(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        start = _start;
        end = _end;
        callback = _callback;
    }

    public void ResetContents()
    {
        start = Vector3.zero;
        end = Vector3.zero;
        callback = null;
    }
}
