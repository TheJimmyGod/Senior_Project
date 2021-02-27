using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathTask : MonoBehaviour
{
    private PathReqeustInfo _info;
    private int _taskID;
    public int TaskID
    {
        get { return _taskID; }
    }

    private Stopwatch _stopWatch = new Stopwatch();

    public bool _isRun = false;

    public PathTask(object info, int num)
    {
        this._taskID = num;
        if(info is PathReqeustInfo)
            this._info = (PathReqeustInfo)info;
    }

    public void ResetTask(object info, int num)
    {
        _taskID = 0;
        _info.ResetContents();
        _stopWatch = null;
        if(info is PathReqeustInfo)
            _info = (PathReqeustInfo)info;
        _isRun = false;
    }

    public void CreateTask()
    {
        if (_isRun)
            return;
        _isRun = true;
        ExecuteTask(_taskID);
    }
    //  Task asynchronous programming model (TAP): avoid performance bottlenecks and enhance the overall responsiveness 
    //  of your application by using asynchronous programming.
    //  - async and await
    public async void ExecuteTask(object id)
    {
        if (!_isRun)
            return;
        _stopWatch.Start();
        try
        {
            if (AI.Instance.pathFindOptions == PathFindOptions.DFS)
            {
                // DFS
                await Task.Factory.StartNew(() => AI.Instance.ExecutePathFindingDFS(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue));

            }
            if (AI.Instance.pathFindOptions == PathFindOptions.AStar)
            {
                // AStar
                await Task.Factory.StartNew(() => AI.Instance.ExecutePathFindingAStar(_info, PathTaskManager.Instance.FinalizedProcessingEnqueue));
            }
        }
        catch(Exception ex)
        {
            Debug.Log("Error in Task: " + ex.ToString());
            throw;
        }
        finally
        {
            Debug.Log("<color=red> Task# " + id + "</color>'s timer: " + " <color=green>" + _stopWatch.ElapsedMilliseconds * 0.001f + "</color>sec, FPS= " + "<color=green>" +
    1 / Time.deltaTime + "</color>, " + "Start Position(x,y): " + " <color=green>" + Mathf.RoundToInt(_info.start.x) + ", " + Mathf.RoundToInt(_info.start.z) + "</color>");
        }

        _stopWatch.Stop();

    }
}
