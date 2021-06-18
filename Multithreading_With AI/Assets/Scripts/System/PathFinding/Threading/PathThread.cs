using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathThread
{
    private PathReqeustInfo _info;
    private int _threadID = 0;
    public int ThreadID
    {
        get { return _threadID; }
    }
    private Stopwatch _stopWatch = new Stopwatch();
    public Stopwatch _stopWatchForApproximate = new Stopwatch();
    private Thread _thread;
    private long _latestTime;
    private long _totalTime;
    public volatile bool _isRun = false;
    private bool _isStarted = false;
    public PathThread(object info, int num)
    {
        this._threadID = num;
        if(info is PathReqeustInfo)
            this._info = (PathReqeustInfo)info;
    }

    public void ResetThread(object info)
    {
        _info.ResetContents();

        if (info is PathReqeustInfo)
            _info = (PathReqeustInfo)info;
    }

    public void CreateThread()
    {
        // ParameterizedThreadStart = Parameter is required
        // ThreadStart = All okay
        _thread = new Thread(new ParameterizedThreadStart(ExecuteThread));
        //_thread.IsBackground = true;
        RunThread();
    }

    public void RunThread()
    {
        if (!_isRun)
        {
            _isRun = true;
            _thread.Start(_threadID);
            _thread.Join();
            if (_isStarted == false)
            {
                _isStarted = true;
                _stopWatchForApproximate.Start();
            }
        }
        else
            return;
    }

    public void TimeCheckOut()
    {
        _stopWatchForApproximate.Reset();
        _stopWatchForApproximate.Start();
        if ((_totalTime / 1000.0f) != 0.0f)
            UI.Instance._approximateTime = _totalTime / 1000.0f;
        _totalTime = 0;
    }

    public void ExecuteThread(object id = null)
    {
        if (_isRun == false)
            return;
        try
        {
            _stopWatch.Start();
            if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
            {
                // DFS
                AI.Instance.ExecutePathFindingDFS(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
                Thread.Sleep(AI.Instance.sleepTime);
            }
            else if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
            {
                // AStar
                AI.Instance.ExecutePathFindingAStar(_info, PathThreadManager.Instance.FinalizedProcessingEnqueue);
                Thread.Sleep(AI.Instance.sleepTime);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Thread error: " + ex);
        }
        _stopWatch.Stop();
        _latestTime = _stopWatch.ElapsedMilliseconds;
        _totalTime += _latestTime;
        _stopWatch.Reset();
        UI.Instance.EnqueueStatusInfo(new UI_Info(_info.id, (float)_latestTime * 0.001f, ThreadingType.Thread));
        _isRun = false;
    }
}
