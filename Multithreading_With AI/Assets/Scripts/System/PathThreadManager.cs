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

    private Queue<PathResultInfo> QueueLock;
    private PathThread[] _threads;
    
    [Range(1,100)]
    public int sleepTime = 10;

    private void Start()
    {
        QueueLock = new Queue<PathResultInfo>();
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
                Debug.Log("<color=green>Confirmed Queue has been enqueued... </color>");
                System.Threading.Monitor.Enter(QueueLock, ref isEntered);
                int count = QueueLock.Count;
                for (int i = 0; i < count; ++i)
                {
                    var resultQueue = QueueLock.Dequeue();
                    resultQueue.callback(resultQueue.waypoints, resultQueue.IsSuccess);
                }
            }
            catch (SynchronizationLockException ex)
            {
                Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
            }
            finally
            {
                Debug.Log("<color=green>Thread has been create for request... </color>");
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
        catch(SynchronizationLockException ex)
        {
            Debug.Log("<color=Red>Synchronization Error</color>:" + " " + ex.Message);
        }
        finally
        {
            Debug.Log("<color=green>Exiting from the request... </color>");
            System.Threading.Monitor.Exit(QueueLock);
        }
    }

    public static void RequestPathInfo(PathReqeustInfo path)
    {
        for (int counter = 0; counter < Instance._threads.Length; ++counter)
        {
            if(Instance._threads[counter] == null)
            {
                Instance._threads[counter] = new PathThread(path, counter);
                Instance._threads[counter].CreateThread();
            }
            else
            {
                Instance._threads[counter].ResetThread(path,counter);
                Instance._threads[counter].CreateThread();
            }
        }
        Debug.Log("<color=green>Thread has been create for request... </color>");
    }
}
