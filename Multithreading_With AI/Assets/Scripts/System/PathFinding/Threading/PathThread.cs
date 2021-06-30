using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathThread
{
    public PathReqeustInfo _info = null;
    private int _threadID = 0;
    public int ThreadID
    {
        get { return _threadID; }
    }
    private Stopwatch _stopWatch = new Stopwatch();
    public Stopwatch _stopWatchForApproximate = new Stopwatch();
    public Thread _thread;
    public Queue<PathReqeustInfo> pending = new Queue<PathReqeustInfo>();

    private long _latestTime;
    private long _totalTime;
    public volatile bool _isRun = false;
    private volatile bool _isStarted = false;

    public void CreateThread()
    {
        // ParameterizedThreadStart = Parameter is required
        // ThreadStart = All okay
        _thread = new Thread(new ParameterizedThreadStart(ExecuteThread));
    }

    public void RunThread()
    {
        _isRun = true;
        _thread.Start(_threadID);
        if (_isStarted == false)
        {
            _isStarted = true;
            _stopWatchForApproximate.Start();
        }
    }

    public void EnqueueItem(object info, int num)
    {
        this._threadID = num;
        lock (pending)
        {
            _info = (PathReqeustInfo)info;
            pending.Enqueue(_info);
        }


    }

    public void TimeCheckOut()
    {
        _stopWatchForApproximate.Reset();
        _stopWatchForApproximate.Start();
        if ((_totalTime / 1000.0f) != 0.0f)
            UI.Instance.UpdateExecuteTime(_totalTime / 1000.0f, _threadID);
        _totalTime = 0;
    }

    public void ExecuteThread(object id = null)
    {
        while(_isRun)
        {
            if(pending.Count == 0)
                Thread.Sleep(AI.Instance.sleepTime);
            else
            {
                try
                {
                    PathReqeustInfo item;
                    lock(pending)
                    {
                        item = pending.Dequeue();
                    }
                    if (item == null)
                        continue;
                    
                    _stopWatch.Start();
                    if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
                    {
                        // DFS
                        AI.Instance.ExecutePathFindingDFS(item, PathThreadManager.Instance.FinalizedProcessingEnqueue);
                    }
                    else if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
                    {
                        // AStar
                        AI.Instance.ExecutePathFindingAStar(item, PathThreadManager.Instance.FinalizedProcessingEnqueue);
                    }
                    _stopWatch.Stop();
                    _latestTime = _stopWatch.ElapsedMilliseconds;
                    _totalTime += _latestTime;
                    _stopWatch.Reset();
                    UI.Instance.EnqueueStatusInfo(new UI_Info(item.id, (float)_latestTime * 0.001f, ThreadingType.Thread));
                    _info = null;
                }
                catch(SynchronizationLockException ex)
                {
                    Debug.Log(ex.Data.ToString());
                    break;
                }

            }

        }
  
    }

    public void StopThread()
    {
        _isRun = false;
    }
}
