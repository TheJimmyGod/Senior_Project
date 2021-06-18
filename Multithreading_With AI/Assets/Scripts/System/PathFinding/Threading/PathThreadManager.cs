using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathThreadManager : MonoBehaviour
{
    private static PathThreadManager _instance;
    public static PathThreadManager Instance {get{return _instance;}}
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private Queue<object> QueueLock;
    private PathThread[] _threads;
    
    private void Start()
    {
        QueueLock = new Queue<object>();
        _threads = new PathThread[AI.Instance.ThreadVaild];
    }

    private void Update()
    {
        if (QueueLock.Count == 0)
            Thread.Sleep(AI.Instance.sleepTime);
        else if (QueueLock.Count > 0)
        {
            bool isEntered = false;
            try
            {
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
                //Debug.Log("<color=green>Thread has been called back for request... </color>");
                System.Threading.Monitor.Exit(QueueLock);
            }
        }
        TimeCheck();
    }

    public static void TimeCheck()
    {
        for (int counter = 0; counter < Instance._threads.Length; counter++)
        {
            if(Instance._threads[counter] != null)
            {
                if (Instance._threads[counter]._stopWatchForApproximate == null)
                    continue;
                if(Instance._threads[counter]._stopWatchForApproximate.ElapsedMilliseconds >= 1000)
                    Instance._threads[counter].TimeCheckOut();
            }
        }
    }

    public void FinalizedProcessingEnqueue(PathResultInfo result)
    {
        bool isEntered = false;
        try
        {
            System.Threading.Monitor.Enter(QueueLock, ref isEntered);
            QueueLock.Enqueue(result);
        }
        catch(SynchronizationLockException ex)
        {
            Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
        }
        finally
        {
            System.Threading.Monitor.Exit(QueueLock);
        }
    }

    public static void RequestInfo(object info)
    {
        if(info is PathReqeustInfo)
        {
            for (int counter = 0; counter < Instance._threads.Length; ++counter)
            {
                if (Instance._threads[counter] == null)
                    Instance._threads[counter] = new PathThread(info, counter);
                else
                    Instance._threads[counter].ResetThread(info);
                Instance._threads[counter].CreateThread();
            }
        }
    }
}


//
// Enemy -> request -> threadStart(request)
//

// Enmey1 -> request -> couldn't enter thread -> fail  ]
// Enmey2 -> request -> threadStart(request) -> success] -> Memory leaks -> rebooting
// Enemy3 -> request -> couldn't enter thread -> fail  ]
// Modified
// Enemy1 -> request -> threadStart[0](request)
// Enemy2 -> request -> threadStart[1](request)
// Enemy3 -> request -> threadStart[2](request)