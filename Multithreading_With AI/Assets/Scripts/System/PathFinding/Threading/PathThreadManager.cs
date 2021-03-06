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
    
    [Range(1,100)]
    public int sleepTime = 10;

    private void Start()
    {
        QueueLock = new Queue<object>();
        _threads = new PathThread[AI.Instance.ThreadVaild];
    }

    private void Update()
    {

        if (QueueLock.Count == 0)
            Thread.Sleep(sleepTime);
        else if (QueueLock.Count > 0)
        {
            // Monitor is no different from lock but the monitor class provides more control 
            // over the synchronization of various threads trying to access the same lock of code.
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
                //Debug.Log("<color=green>Thread has been called back for request... </color>");
                System.Threading.Monitor.Exit(QueueLock);
            }
        }
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
        catch(SynchronizationLockException ex)
        {
            Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
        }
        finally
        {
            //Debug.Log("<color=green>Exiting from the request... </color>");
            System.Threading.Monitor.Exit(QueueLock);
        }
    }

    public static void RequestInfo(object info)
    {
        // Rendering or Physics
        if(info is PathReqeustInfo)
        {
            for (int counter = 0; counter < Instance._threads.Length; ++counter)
            {
                if (Instance._threads[counter] == null)
                {
                    Instance._threads[counter] = new PathThread(info, counter);
                    Instance._threads[counter].CreateThread();
                }
                else
                {
                    Instance._threads[counter].ResetThread(info, counter);
                    Instance._threads[counter].RunThread();
                }
            }
        }

        //Debug.Log("<color=green>Thread has been create for request... </color>");
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