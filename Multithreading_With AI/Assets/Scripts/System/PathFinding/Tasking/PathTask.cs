using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathTask
{
    private PathReqeustInfo _info;
    private int _taskID;
    private long _latestTime;
    private long _totalTime;
    public int TaskID
    {
        get { return _taskID; }
    }

    private Stopwatch _stopWatch = new Stopwatch();
    public Stopwatch _stopWatchForApproximate = new Stopwatch();
    private Task _task;

    public volatile bool _isRun = false;
    private bool _isStarted = false;
    public PathTask(object info, int num)
    {
        this._taskID = num;
        if(info is PathReqeustInfo)
            this._info = (PathReqeustInfo)info;
    }

    public void ResetTask(object info)
    {
        _info.ResetContents();
        _stopWatch.Reset();
        if(info is PathReqeustInfo)
            _info = (PathReqeustInfo)info;
    }
    public void RunTask()
    {
        if (!_isRun)
        {
            _isRun = true;
            ExecuteTask(_taskID);
            if (_isStarted == false)
            {
                _isStarted = true;
                _stopWatchForApproximate.Start();
            }
        }

    }

    public void TimeCheckOut()
    {
        _stopWatchForApproximate.Reset();
        _stopWatchForApproximate.Start();
        if((_totalTime / 1000.0f) != 0.0f)
        {
            if(UI.Instance._approximateTime < _totalTime / 1000.0f)
                UI.Instance._approximateTime = _totalTime / 1000.0f;
        }
        _totalTime = 0;
    }


    //  Task asynchronous programming model (TAP): avoid performance bottlenecks and enhance the overall responsiveness 
    //  of your application by using asynchronous programming.
    //  - async and await
    // TODO: Make a function for TAP
    public void ExecuteTask(object id)
    {
        if (!_isRun)
            return;

        _stopWatch.Start();
        var tasks = new List<Task>();
        try
        {
            if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
            {
                // DFS
                _task = Task.Run(async () => 
                {
                    AI.Instance.ExecutePathFindingDFS(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue);
                    await Task.Delay(AI.Instance.sleepTime);
                });
                tasks.Add(_task);
            }
            if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
            {
                // AStar
                _task = Task.Run(async () =>
                {
                    AI.Instance.ExecutePathFindingAStar(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue);
                    await Task.Delay(AI.Instance.sleepTime);
                }
                );
                tasks.Add(_task);
            }
            Task.WaitAll(tasks.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log("Error in Task: " + ex.ToString());
            throw;
        }
        _stopWatch.Stop();
        _latestTime = _stopWatch.ElapsedMilliseconds;
        _totalTime += _latestTime;
        _stopWatch.Reset();
        UI.Instance.EnqueueStatusInfo(new UI_Info(_info.id, (float)_latestTime * 0.001f, ThreadingType.Task));
        _isRun = false;
    }
}
