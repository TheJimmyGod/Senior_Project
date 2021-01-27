using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1.0f;

    private Rigidbody _rigidbody;
    private bool _isFound = false;

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

    }


    void Update()
    {
        if (AI.Instance == null)
            return;
    }
}
