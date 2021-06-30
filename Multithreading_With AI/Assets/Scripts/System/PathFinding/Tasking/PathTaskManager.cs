using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PathTaskManager : MonoBehaviour
{
    private static PathTaskManager _instance;
    public static PathTaskManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private Queue<object> QueueLock;
    private PathTask[] _tasks;

    private void Start()
    {
        QueueLock = new Queue<object>();
        _tasks = new PathTask[AI.Instance.ThreadVaild];

        for (int counter = 0; counter < Instance._tasks.Length; ++counter)
        {
            Instance._tasks[counter] = new PathTask();
        }
    }

    private void Update()
    {
        if (QueueLock.Count == 0)
        {
            Task.Delay(AI.Instance.sleepTime);
        }
        else
        {
            bool isEntered = false;
            try
            {
                //Debug.Log("<color=green>Confirmed Queue has been enqueued... </color>");
                System.Threading.Monitor.Enter(QueueLock, ref isEntered);
                int count = QueueLock.Count;
                for (int i = 0; i < count; ++i)
                {
                    if (QueueLock.Peek() is PathResultInfo)
                    {
                        PathResultInfo result = (PathResultInfo)QueueLock.Dequeue();
                        result.callback(result.waypoints, result.IsSuccess);
                    }
                }
            }
            catch (SynchronizationLockException ex)
            {
                Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
            }
            finally
            {
                //Debug.Log("<color=green>Task has been create for request... </color>");
                System.Threading.Monitor.Exit(QueueLock);
            }
        }
        TimeCheck();
    }

    public void FinalizedProcessingEnqueue(PathResultInfo result)
    {
        bool isEntered = false;
        try
        {
            //Debug.Log("<color=green>Entering the process of enqueue... </color>");
            System.Threading.Monitor.Enter(QueueLock, ref isEntered);
            QueueLock.Enqueue(result);
        }
        catch (SynchronizationLockException ex)
        {
            Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
        }
        finally
        {
            System.Threading.Monitor.Exit(QueueLock);
        }
    }
    public static void TimeCheck()
    {
        for (int counter = 0; counter < Instance._tasks.Length; counter++)
        {
            if (Instance._tasks[counter] != null)
            {
                if (Instance._tasks[counter]._stopWatchForApproximate == null)
                    continue;
                if (Instance._tasks[counter]._stopWatchForApproximate.ElapsedMilliseconds >= 1000)
                    Instance._tasks[counter].TimeCheckOut();
            }
        }
    }

    public static void RequestInfo(object info) // 동기 방식
    {
        if (info is PathReqeustInfo)
        {
            int counter = 0;
            while (true)
            {
                if (Instance._tasks[counter]._isRun == false)
                {
                    Instance._tasks[counter].CreateTask(info, counter);
                    break;
                }
                counter++;
                if (counter == Instance._tasks.Length)
                    counter = 0;
                Thread.Sleep(AI.Instance.sleepTime);
            }
        }
    }
}