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

    private Task _task;

    public volatile bool _isRun = false;

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
        //_isRun = false;
    }

    public void CreateTask()
    {
        RunTask();
    }

    public void RunTask()
    {
        if (!_isRun)
        {
            _isRun = true;
            ExecuteTask(_taskID);
        }

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
        try
        {
            if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
            {
                // DFS
                _task = Task.Run(async () => await Task.Run(() => AI.Instance.ExecutePathFindingDFS(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue)));

            }
            if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
            {
                // AStar
                _task = Task.Run(async () => await Task.Run(() => AI.Instance.ExecutePathFindingAStar(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue)));
            }
            _task.Wait();
        }
        catch (Exception ex)
        {
            Debug.Log("Error in Task: " + ex.ToString());
            throw;
        }
        _stopWatch.Stop();
        UI.Instance.EnqueueStatusInfo(new UI_Info(_info.id, Mathf.Clamp01((float)(_stopWatch.ElapsedMilliseconds * 0.001f)), ThreadingType.Task));
        _stopWatch.Reset();
        _latestTime = _stopWatch.ElapsedMilliseconds;
        _totalTime += _latestTime;
        UI.Instance.EnqueueStatusInfo(new UI_Info(_info.id, (float)_latestTime * 0.001f, ThreadingType.Task));
        if (UI.Instance._approximateTime <= Mathf.Clamp01(_totalTime / 1000.0f))
            UI.Instance._approximateTime = Mathf.Clamp01(_totalTime / 1000.0f);
        _isRun = false;
    }

    public void OnApplicationQuit()
    {
        _isRun = false;
        _task.Dispose();
    }
}
