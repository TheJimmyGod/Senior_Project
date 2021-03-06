using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UI_Info
{
    public uint id;
    public float time;
    public ThreadingType type;

    public UI_Info(uint _id, float _time, ThreadingType _type)
    {
        id = _id;
        time = _time;
        type = _type;
    }
}
