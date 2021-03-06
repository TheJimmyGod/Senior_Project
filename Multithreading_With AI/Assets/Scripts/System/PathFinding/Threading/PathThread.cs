using System;
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

    private Thread _thread;
 
    public volatile bool _isRun = false;

    public PathThread(object info, int num)
    {
        this._threadID = num;
        if(info is PathReqeustInfo)
            this._info = (PathReqeustInfo)info;
    }

    public void ResetThread(object info, int num = 0)
    {
        _info.ResetContents();
        _stopWatch.Reset();

        if (info is PathReqeustInfo)
            _info = (PathReqeustInfo)info;
 
        _isRun = false;
    }
    
    public void CreateThread()
    {
        // ParameterizedThreadStart = Parameter is required
        // ThreadStart = All okay

        _thread = new Thread(new ParameterizedThreadStart(ExecuteThread));
        RunThread();
    }

    public void RunThread()
    {
        if(!_isRun)
        {
            _isRun = true;
            _thread.Start(_threadID);
            _thread.Join();
        }
    }

    public void ExecuteThread(object id = null)
    {
        if (!_isRun)
            return;
        _stopWatch.Start();
        try
        {
            if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
            {
                // DFS
                AI.Instance.ExecutePathFindingDFS(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
            }
            else if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
            {
                // AStar
                AI.Instance.ExecutePathFindingAStar(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
            }
        }
        catch(Exception ex)
        {
            Debug.Log("Thread error: " + ex);
        }
        _stopWatch.Stop();
//        Debug.Log("<color=red> Thread# " + id + "</color>'s timer: " + " <color=green>" + _stopWatch.ElapsedMilliseconds * 0.001f + "</color>sec, " + "Start Position(x,y): " + " <color=green>" +
//Mathf.RoundToInt(_info.start.x) + ", " + Mathf.RoundToInt(_info.start.z) + "</color>");
        UI.Instance.EnqueueStatusInfo(new UI_Info(_info.id, _stopWatch.ElapsedMilliseconds * 0.001f, ThreadingType.Thread));
        _stopWatch.Reset();
        _isRun = false;
    }

    private void OnApplicationQuit()
    {
        if(_thread != null)
            _thread.Abort();
    }
}
