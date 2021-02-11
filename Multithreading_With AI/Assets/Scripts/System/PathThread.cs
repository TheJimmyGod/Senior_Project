using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathThread : MonoBehaviour
{

    private PathReqeustInfo _info;
    private int _threadID = 0;
    public int ThreadID
    {
        get { return _threadID; }
    }
    private Stopwatch _stopWatch = new Stopwatch();

    private ParameterizedThreadStart _thread;

    public bool _isRun = false;

    public PathThread(PathReqeustInfo path, int num)
    {
        this._threadID = num;
        this._info = path;
    }

    public void ResetThread(PathReqeustInfo path, int num)
    {
        _threadID = 0;
        _info.ResetContents();
        _stopWatch = null;

        _info = path;
        _isRun = false;
    }
    
    public void CreateThread()
    {
        // ParameterizedThreadStart = Parameter is required
        // ThreadStart = All okay

        if (_isRun)
            return;
        _isRun = true;
        _thread = new ParameterizedThreadStart( delegate { ExecuteThread(_threadID); });
        _thread.Invoke((object)_threadID);
    }
    public void ExecuteThread(object id)
    {
        if (!_isRun)
            return;
        _stopWatch.Start();
        if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
        {
            // DFS
            AI.Instance.ExecutePathFindingDFS(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
        }
        if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
        {
            // AStar
            AI.Instance.ExecutePathFindingAStar(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
        }
        _stopWatch.Stop();
        Debug.Log("<color=red> Thread# " + id + "</color>'s timer: " + " <color=green>" + _stopWatch.ElapsedMilliseconds * 0.001f + "</color>sec, FPS= " + "<color=green>" +
            1 / Time.deltaTime + "</color>, " + "Start Position(x,y): " + " <color=green>" +
            Mathf.RoundToInt(_info.start.x) + ", " + Mathf.RoundToInt(_info.start.z) + "</color>");
    }
}
