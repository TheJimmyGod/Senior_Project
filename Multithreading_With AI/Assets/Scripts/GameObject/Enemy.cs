﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string currentState = string.Empty;

    [SerializeField]
    public float _speed = 10.0f;
    public uint id = 0;

    public Queue<float> lastDistanceRecord= new Queue<float>();
    public float lastDistance = 0.0f;

    public int _previousIndex = 0;
    public int MaximumPath = 0;

    public Rigidbody _rigidbody;
    public bool _isStart = false;
    public bool _isFound = false;

    public Vector3[] path;
    public Vector3 current;
    public Vector3 _start = new Vector3();
    public Vector3 _end = new Vector3();

    public List<GameObject> lines;
    public List<GameObject> tiles;

    [SerializeField]
    private List<GameObject> _indicatorList = new List<GameObject>();
    private GameObject _indicator;
    public GameObject indicator
    {
        set { _indicator = value; }
        get { return _indicator; }
    }

    public Material lineColor;

    public StateMachineModule stateMachine;

    public ThreadingType type = ThreadingType.Thread;

    void Start()
    {

        _indicatorList.ForEach(delegate (GameObject obj)
        {
            obj.SetActive(false);
        });

        _rigidbody = GetComponent<Rigidbody>();
        GameObject[] agent = GameObject.FindGameObjectsWithTag("Enemy");

        if(agent.Length > 1)
        {
            foreach (var e in agent)
            {
                Physics.IgnoreCollision(this.GetComponent<Collider>(), e.GetComponent<Collider>());
            }
        }

        _previousIndex = 0;
        GetOrderFromAI();

        stateMachine = gameObject.AddComponent<StateMachineModule>();

        stateMachine.agent = this.gameObject;
        stateMachine.AddState<Idle>(new Idle(), "Idle");
        stateMachine.AddState<Move>(new Move(),"Move");
        stateMachine.AddState<Find>(new Find(), "Find");
        stateMachine.ChangeState("Idle");

    }

    public void PathFound(Vector3[] _pathes, bool _pathFound)
    {
        if(_pathFound)
        {
            path = _pathes;
            MaximumPath = _pathes.Length - 1;
            _previousIndex = 0;

            for (int i = 0; i < MaximumPath; ++i)
            {
                GameObject line = new GameObject("Line");
                line.tag = "Line";
                lines.Add(line);
                line.transform.position = new Vector3(_pathes[i].x, 0.5f, _pathes[i].z);
                line.AddComponent<LineRenderer>();
                LineRenderer lr = line.GetComponent<LineRenderer>();

                lr.material = lineColor;
                if(type == ThreadingType.Thread)
                {
                    lr.startColor = Color.green;
                    lr.endColor = Color.green;
                }
                else
                {
                    lr.startColor = Color.red;
                    lr.endColor = Color.red;
                }
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.SetPosition(0, new Vector3(_pathes[i].x, 0.5f, _pathes[i].z));
                lr.SetPosition(1, new Vector3(_pathes[i+1].x, 0.5f, _pathes[i+1].z));
            }
            stateMachine.ChangeState("Move");
        }
    }

    void Update()
    {
        if (AI.Instance == null)
            return;
        stateMachine.ActivateState();
        if(indicator != null)
            indicator.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
    }

    public void ChangeIndicator()
    {
        switch(stateMachine.GetCurrentState())
        {
            case "Idle":
                {
                    _indicatorList.ForEach(delegate (GameObject obj)
                    {
                        if(obj.name == "Sign-Idle")
                        {
                            obj.SetActive(true);
                            indicator = obj;
                        }
                        else
                        {
                            obj.SetActive(false);
                        }
                    });
                }
                break;
            case "Find":
                _indicatorList.ForEach(delegate (GameObject obj)
                {
                    if (obj.name == "Sign-Find")
                    {
                        obj.SetActive(true);
                        indicator = obj;
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                });
                break;
            case "Move":
                _indicatorList.ForEach(delegate (GameObject obj)
                {
                    if (obj.name == "Sign-Move")
                    {
                        obj.SetActive(true);
                        indicator = obj;
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                });
                break;
            default:
                break;
        }
    }

    public void GetOrderFromAI()
    {
        _start = transform.position;
        _end = AI.Instance.Setting.End;
    }

    public void FinalizePathFinding()
    {
        if(lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                Destroy(lines[i].gameObject);
            }
        }

        lines.Clear();
        _isStart = false;
        _previousIndex = 0;
        MaximumPath = 0;
        path = null;

        if(tiles.Count > 0)
        {
            for (int i = 0; i < tiles.Count; ++i)
            {
                Destroy(tiles[i].gameObject, 0.45f);
            }
        }
        
        tiles.Clear();
    }

    public void EnableDisplaySight()
    {
        this.GetComponent<VisualSensor>().EnableVisualSensor();
    }

    private void OnDrawGizmos()
    {
        if (this.transform.gameObject == null)
            return;
        if(_isStart)
        {
            Vector3 direction = Vector3.Normalize(current - transform.position);
            float xDir = ((transform.position.x + 0.4f));
            float zDir = ((transform.position.z + 0.4f));
            Vector3 newDirection = new Vector3(xDir, 0.0f, zDir) + direction;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, newDirection);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_rigidbody == null)
            return;
        _rigidbody.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    }
}
