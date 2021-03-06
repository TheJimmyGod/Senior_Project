﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Concurrent;

public class UI : MonoBehaviour
{
    private struct UI_Element
    {
        public int count;
        public float averageTime;
        public string type;

        public UI_Element(float aver, string _type, int _count)
        {
            averageTime = aver;
            type = _type;
            count = _count;
        }
    }

    private static UI _instance;
    public static UI Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    public Canvas canvas;
    private string[] typenames;

    public GameObject[] images;
    public GameObject icon;

    private GameObject[] _Texts;
    private GameObject[] _TextsForDate;
    private GameObject[] _TextsForTypeName;
    private GameObject[] _TextsForID;

    public GameObject initialText;
    public GameObject position;
    public GameObject position2;
  
    public GameObject[] _Threads;
    public GameObject _ThreadGroup;

    private static EventWaitHandle _childThread = new EventWaitHandle(false, EventResetMode.AutoReset);
    private static Thread _thread;

    private float[] _averages;
    private string[] _times;

    public GameObject _performence;
    private Queue<UI_Info> _queueLock;
    private ConcurrentDictionary<int, UI_Element?> _sortedList;
    
    public int _limitRound = 0;

    [SerializeField]
    private bool Active = false;
    private volatile int _round = 0;
    private uint _units = 0;
    private volatile bool _isRun = false;
    private volatile bool _isWait = false;
    private volatile bool _isInstalled = false;
    private volatile bool _roundEnded = false;

    private void Start()
    {
        if (Active == false)
            return;
        _Threads = new GameObject[AI.Instance.ThreadVaild];

        switch (AI.Instance.ThreadVaild)
        {
            case 1:
                _Threads[0] = _ThreadGroup.transform.Find("Thread1").gameObject;
                Destroy(_ThreadGroup.transform.Find("Thread2").gameObject);
                Destroy(_ThreadGroup.transform.Find("Thread3").gameObject);
                Destroy(_ThreadGroup.transform.Find("Thread4").gameObject);
                break;
            case 2:
                _Threads[0] = _ThreadGroup.transform.Find("Thread1").gameObject;
                _Threads[1] = _ThreadGroup.transform.Find("Thread2").gameObject;
                Destroy(_ThreadGroup.transform.Find("Thread3").gameObject);
                Destroy(_ThreadGroup.transform.Find("Thread4").gameObject);
                break;
            case 3:
                _Threads[0] = _ThreadGroup.transform.Find("Thread1").gameObject;
                _Threads[1] = _ThreadGroup.transform.Find("Thread2").gameObject;
                _Threads[2] = _ThreadGroup.transform.Find("Thread3").gameObject;
                Destroy(_ThreadGroup.transform.Find("Thread4").gameObject);
                break;
            case 4:
                _Threads[0] = _ThreadGroup.transform.Find("Thread1").gameObject;
                _Threads[1] = _ThreadGroup.transform.Find("Thread2").gameObject;
                _Threads[2] = _ThreadGroup.transform.Find("Thread3").gameObject;
                _Threads[3] = _ThreadGroup.transform.Find("Thread4").gameObject;
                break;
            default:
                break;
        }

        if (AI.Instance.EnemyCountForSpawning >= 5)
        {
            Active = false;
            return;
        }

        _units = (uint)AI.Instance.EnemyCountForSpawning;
        _sortedList = new ConcurrentDictionary<int, UI_Element?>();
        _averages = new float[_units];
        _times = new string[_units];
        _Texts = new GameObject[_units];
        _TextsForDate = new GameObject[_units];
        _TextsForID = new GameObject[_units];
        _TextsForTypeName = new GameObject[_units];

        images = new GameObject[_units];

        _limitRound = AI.Instance.ThreadVaild * (int)_units;
        _queueLock = new Queue<UI_Info>(_limitRound);

        position.transform.localPosition = new Vector3(position.transform.localPosition.x + 400, position.transform.localPosition.y + 600.0f);
        position2.transform.localPosition = new Vector3(position2.transform.localPosition.x + 550, position2.transform.localPosition.y + 600.0f);

        Vector3 pos1 = position.transform.localPosition;
        Vector3 pos2 = position2.transform.localPosition;

        for (int i = 0; i < _units; ++i)
        {
            if (i > 0 && i < 4)
            {
                position.transform.localPosition = new Vector3(position.transform.localPosition.x, position.transform.localPosition.y - 225.0f);
                position2.transform.localPosition = new Vector3(position2.transform.localPosition.x, position2.transform.localPosition.y - 225.0f);
            }

            GameObject e = Instantiate(icon, position.transform.localPosition, position.transform.localRotation, canvas.transform);
            images[i] = e;

            GameObject e1 = Instantiate(initialText, position2.transform.localPosition, position.transform.localRotation, canvas.transform);
            _Texts[i] = e1.transform.Find("Average").gameObject;
            _TextsForID[i] = e1.transform.Find("ID").gameObject;
            _TextsForTypeName[i] = e1.transform.Find("TypeName").gameObject;
            _TextsForDate[i] = e1.transform.Find("Date").gameObject;

            
           
        }

        foreach(var e in AI.Instance.enemies)
        {
            if(e.type == ThreadingType.Task)
                _sortedList.TryAdd((int)e.id, new UI_Element(0, "Task", 0));
            else
                _sortedList.TryAdd((int)e.id, new UI_Element(0, "Thread", 0));
        }

        InitializeText();
        _isInstalled = true;
        _thread = new Thread(ExecuteThread);
        _thread.Start();
    }

    private void Update()
    {
        if(AI.Instance.pathFindOptions == PathFindOptions.AStar)
        {
            _performence.GetComponent<Text>().text = "PathFind Search: " + "A*" + "\n"
+ "Round(Current/Total): (" + _round + "/" + _limitRound + ")" + "\n" + "Thread vaild values: " + AI.Instance.ThreadVaild;
        }
        else
        {
            _performence.GetComponent<Text>().text = "PathFind Search: " + "DFS" + "\n"
+ "Round(Current/Total): (" + _round + "/" + _limitRound + ")" + "\n" + "Thread vaild values: " + AI.Instance.ThreadVaild;
        }

        if (Active == false)
            return;
        if (_isRun == false)
            return;
        if(_isWait == true)
        {
            if (_queueLock.Count > 0)
            {
                lock (_queueLock)
                {
                    var result = _queueLock.Dequeue();
                    _round++;
                    float time = (result.time <= 0.0f) ? 0.0f : result.time;
                    if (!_sortedList.ContainsKey((int)result.id))
                    {
                        if (result.type == ThreadingType.Task)
                            _sortedList.TryAdd((int)result.id, new UI_Element(time, "Task", 0));
                        else
                            _sortedList.TryAdd((int)result.id, new UI_Element(time, "Thread", 0));
                    }
                    int count = _sortedList[(int)result.id].Value.count + 1;

                    UI_Element Newdata;
                    if (result.type == ThreadingType.Task)
                        Newdata = new UI_Element(_sortedList[(int)result.id].Value.averageTime + time, "Task", count);
                    else
                        Newdata = new UI_Element(_sortedList[(int)result.id].Value.averageTime + time, "Thread", count);
                    _sortedList.TryUpdate((int)result.id, Newdata, _sortedList[(int)result.id]);
                }
            }
            else
                Thread.Sleep(AI.Instance.sleepTime);
        }

        if(_round >= _limitRound)
        {
            _round = 0;
            _childThread.Set();
        }

        if (_roundEnded == true && _isWait == true)
            SetText();

        if (_isInstalled)
            UpdateStatus();
    }

    public void InitializeText()
    {
        if (Active == false)
            return;
        for (int i = 0; i < _units; i++)
        {
            images[i].transform.Find("Idle").gameObject.SetActive(true);
            images[i].transform.Find("Move").gameObject.SetActive(false);
            images[i].transform.Find("Find").gameObject.SetActive(false);

            _Texts[i].GetComponent<Text>().text = "?";
            _TextsForDate[i].GetComponent<Text>().text = "?";
            _TextsForID[i].GetComponent<Text>().text = "?";
            _TextsForTypeName[i].GetComponent<Text>().text = "?";
        }
    }

    public void UpdateExecuteTime(float t, int number)
    {
        _Threads[number].GetComponent<Text>().text = "Thread ID: " + number + "\n" + "Approximate time: " + t + "ms";
    }

    public void UpdateStatus()
    {
        if (Active == false)
            return;
        for (int i = 0; i < AI.Instance.enemies.Count; ++i)
        {
            switch (AI.Instance.enemies[i].currentState)
            {
                case "Idle":
                    {
                        images[i].transform.Find("Idle").gameObject.SetActive(true);
                        images[i].transform.Find("Move").gameObject.SetActive(false);
                        images[i].transform.Find("Find").gameObject.SetActive(false);
                        break;
                    }
                case "Move":
                    {
                        images[i].transform.Find("Idle").gameObject.SetActive(false);
                        images[i].transform.Find("Move").gameObject.SetActive(true);
                        images[i].transform.Find("Find").gameObject.SetActive(false);
                        break;
                    }
                case "Find":
                    {
                        images[i].transform.Find("Idle").gameObject.SetActive(false);
                        images[i].transform.Find("Move").gameObject.SetActive(false);
                        images[i].transform.Find("Find").gameObject.SetActive(true);
                        break;
                    }
                default:
                    {
                        images[i].transform.Find("Idle").gameObject.SetActive(true);
                        images[i].transform.Find("Move").gameObject.SetActive(false);
                        images[i].transform.Find("Find").gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }

    public void SetText()
    {
        if (Active == false)
            return;
        for (int id = 0; id < _units; ++id)
        {
            if (_Texts[id] == null || _TextsForDate[id] == null || _TextsForID[id] == null || _TextsForTypeName == null)
                continue;
            else
            {
                _TextsForID[id].GetComponent<Text>().text = "ID: " + id.ToString();
                if(_sortedList[id].Value.type != null)
                    _TextsForTypeName[id].GetComponent<Text>().text = "Type: " + _sortedList?[id].Value.type;
                _Texts[id].GetComponent<Text>().text = "Average: " + _averages[id];
                _TextsForDate[id].GetComponent<Text>().text = "Time: " + _times[id];
            }

        }

        _isWait = false;
        return;
    }

    public void EnqueueStatusInfo(UI_Info _info)
    {
        if (Active == false)
            return;
        if (_roundEnded == true)
            return;

        lock(_queueLock)
            _queueLock.Enqueue(_info);
    }

    private static void ExecuteThread()
    {
        Instance._isRun = true;
        try
        {
            while (Instance._isRun)
            {
                if (Instance._isWait == false)
                {
                    Instance._isWait = true;
                    _childThread.WaitOne();
                }
                Instance._roundEnded = true;
                foreach (var element in Instance?._sortedList.ToList())
                {
                    float ave = element.Value.Value.averageTime / element.Value.Value.count;
                    Instance._averages[element.Key] = ave;
                    Instance._times[element.Key] = element.Value.Value.count.ToString();
                }
                Instance._roundEnded = false;
            }
        }
        finally
        {
            Instance._isRun = false;
        }
    }
}
