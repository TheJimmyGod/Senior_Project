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
    }

    private void Update()
    {
        if (QueueLock.Count == 0)
            return;
        else
        {
            bool isEntered = false;
            try
            {
                Debug.Log("<color=green>Confirmed Queue has been enqueued... </color>");
                System.Threading.Monitor.Enter(QueueLock, ref isEntered);
                int count = QueueLock.Count;
                for (int i = 0; i < count; ++i)
                {
                    if(QueueLock.Peek() is PathResultInfo)
                    {
                        PathResultInfo result = (PathResultInfo)QueueLock.Dequeue();
                        result.callback(result.waypoints, result.IsSuccess);
                    }
                }
            }
            catch(SynchronizationLockException ex)
            {
                Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
            }
            finally
            {
                Debug.Log("<color=green>Task has been create for request... </color>");
                System.Threading.Monitor.Exit(QueueLock);
            }
        }
    }

    public void FinalizedProcessingEnqueue(PathResultInfo result)
    {
        bool isEntered = false;
        try
        {
            Debug.Log("<color=green>Entering the process of enqueue... </color>");
            System.Threading.Monitor.Enter(QueueLock, ref isEntered);
            QueueLock.Enqueue(result);
        }
        catch (SynchronizationLockException ex)
        {
            Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
        }
        finally
        {
            Debug.Log("<color=green>Exiting from the request... </color>");
            System.Threading.Monitor.Exit(QueueLock);
        }
    }

    // TODO: Create New finalizedProcessingEnqueue

    public static void RequestInfo(object info)
    {
        for (int counter = 0; counter < Instance._tasks.Length; ++counter)
        {
            if (Instance._tasks[counter] == null)
            {
                if(info is PathReqeustInfo)
                    Instance._tasks[counter] = new PathTask(info, counter);
                Instance._tasks[counter].CreateTask();
            }
            else
            {
                if (info is PathReqeustInfo)
                    Instance._tasks[counter].ResetTask(info, counter);
                Instance._tasks[counter].CreateTask();
            }
        }
        Debug.Log("<color=green>Task has been create for request... </color>");


    }
}
