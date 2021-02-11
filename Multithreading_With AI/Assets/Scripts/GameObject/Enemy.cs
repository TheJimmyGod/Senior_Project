using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;

    private int _previousIndex = 0;

    private Rigidbody _rigidbody;
    private bool _isStart = false;

    private Vector3 _start = new Vector3();
    private Vector3 _end = new Vector3();

    public Vector3[] path;
    public List<GameObject> lines;
    public GameObject signObj;

    public Material lineColor;

    private Vector3 current;
    private int MaximumPath = 0;

    void Start()
    {
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
                lr.startColor = Color.red;
                lr.endColor = Color.red;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.SetPosition(0, new Vector3(_pathes[i].x, 0.5f, _pathes[i].z));
                lr.SetPosition(1, new Vector3(_pathes[i+1].x, 0.5f, _pathes[i+1].z));
            }
        }
    }

    void Update()
    {
        if (AI.Instance == null)
            return;

        if(!_isStart)
        {
            Debug.Log("Act");
            _isStart = true;
            GetOrderFromAI();
            PathThreadManager.RequestPathInfo(new PathReqeustInfo(transform.position, _end, PathFound));
        }

        if (path == null)
            return;
        if (path.Length < 1)
            return;


        current = path[_previousIndex];

        if(_isStart)
        {
            if (Vector3.Distance(transform.position, current) > 0.75f)
            {
                Vector3 direction = Vector3.Normalize(current - transform.position);
                Vector3 newPos = transform.position + (direction * _speed * Time.deltaTime);
                transform.position = new Vector3(newPos.x,0.0f,newPos.z);
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
                if (_previousIndex == MaximumPath)
                {
                    foreach (var L in lines)
                        Destroy(L.gameObject);
                    _isStart = false;
                    lines.Clear();
                    _previousIndex = 0;
                }
                else
                {
                    _previousIndex++;
                }
                
            }
        }

    }

    public void GetOrderFromAI()
    {
        _start = AI.Instance.Setting.Start;
        _end = AI.Instance.Setting.End;
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
}
