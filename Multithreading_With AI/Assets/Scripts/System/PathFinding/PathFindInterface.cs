using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PathFindInterface
{
    void Search(PathReqeustInfo requestInfo, Action<PathResultInfo> callback);
}
