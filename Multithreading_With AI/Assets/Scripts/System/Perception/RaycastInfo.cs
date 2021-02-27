using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RaycastInfo
{
    public bool hit;
    public Vector3 position;
    public float maxDistance;
    public float angle;

    public RaycastInfo(bool _hit, Vector3 _pos, float _maxDis, float _angle)
    {
        hit = _hit;
        position = _pos;
        maxDistance = _maxDis;
        angle = _angle;
    }
}
