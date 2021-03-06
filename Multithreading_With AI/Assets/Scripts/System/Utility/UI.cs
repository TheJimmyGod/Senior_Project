using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

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

    private static EventWaitHandle _childThread = new EventWaitHandle(false, EventResetMode.AutoReset);
    private static Thread _thread;

    private float[] _averages;
    private string[] _times;

    private Queue<UI_Info> _queueLock;
    private Dictionary<int, UI_Element?> _sortedList;
    [Range(3,5)]
    public int _limitRound = 3;
    [SerializeField]
    private bool Active = false;
    private int _round = 0;
    private uint _units = 0;
    private volatile bool _isRun = false;
    private volatile bool _isWait = false;
    private volatile bool _isInstalled = false;
    private volatile bool _roundEnded = false;

    private void Start()
    {
        if (AI.Instance.EnemyCountForSpawning >= 3)
            Active = false;
        if (Active == false)
            return;
        _queueLock = new Queue<UI_Info>(_limitRound);
        _units = (uint)AI.Instance.EnemyCountForSpawning * 2;
        _sortedList = new Dictionary<int, UI_Element?>((int)_units);
        _averages = new float[_units];
        _times = new string[_units];

        _Texts = new GameObject[_units];
        _TextsForDate = new GameObject[_units];
        _TextsForID = new GameObject[_units];
        _TextsForTypeName = new GameObject[_units];

        images = new GameObject[_units];

        _limitRound = _limitRound * AI.Instance.ThreadVaild * (int)_units;

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

        InitializeText();
        _isInstalled = true;
        _thread = new Thread(ExecuteThread);
        _thread.Start();
    }

    private void Update()
    {
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
                    if (!_sortedList.ContainsKey((int)result.id))
                    {
                        if (result.type == ThreadingType.Task)
                            _sortedList.Add((int)result.id, new UI_Element(Mathf.Abs(result.time), "Task", 1));
                        else
                            _sortedList.Add((int)result.id, new UI_Element(Mathf.Abs(result.time), "Thread", 1));

                        //Debug.Log("Add in enqueue: " + result.id);
                    }
                    else
                    {
                        int count = _sortedList[(int)result.id].Value.count + 1;
                        if (result.type == ThreadingType.Task)
                            _sortedList[(int)result.id] = new UI_Element(Mathf.Abs((_sortedList[(int)result.id].Value.averageTime) + Mathf.Abs(result.time)), "Task", count);
                        else
                            _sortedList[(int)result.id] = new UI_Element(Mathf.Abs((_sortedList[(int)result.id].Value.averageTime) + Mathf.Abs(result.time)), "Thread", count);

                        //Debug.Log("Revise in enqueue: " + result.id);
                    }
                }
            }
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
            if (_Texts[id] == null && _TextsForDate[id] == null && _TextsForID[id] == null && _TextsForTypeName == null)
                continue;
            _TextsForID[id].GetComponent<Text>().text = "ID: " + id.ToString();
            _TextsForTypeName[id].GetComponent<Text>().text = "Type: " +  _sortedList[id].Value.type;
            _Texts[id].GetComponent<Text>().text = "Average: " + _averages[id].ToString();
            _TextsForDate[id].GetComponent<Text>().text = "Time: " + _times[id];
        }

        _isWait = false;
    }

    public void EnqueueStatusInfo(UI_Info _info)
    {
        if (Active == false)
            return;
        if (_roundEnded == true)
            return;

        lock(_queueLock)
        {
            _queueLock.Enqueue(_info);
        }
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
                    //Debug.Log("Waiting...");
                    _childThread.WaitOne();
                }
                Instance._roundEnded = true;
                if (Instance._sortedList.Count > 0)
                {
                    foreach (var element in Instance._sortedList)
                    {
                        float ave = element.Value.Value.averageTime / element.Value.Value.count;
                        Instance._averages[element.Key] = ave;
                        Instance._times[element.Key] = element.Value.Value.count.ToString();
                    }
                }
                Instance._roundEnded = false;
            }
        }
        catch(Exception ex)
        {
            Debug.Log("UI Thread has been failed to executed! - " + ex.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        if(Active)
        {
            _isRun = false;
            _childThread.Close();
            _thread.Abort();
        }
    }
}
