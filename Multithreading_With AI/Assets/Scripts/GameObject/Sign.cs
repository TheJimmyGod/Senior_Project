using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    // Start is called before the first frame update
    private bool _isEntered = false;
    private bool _isStayed = false;
    private bool _isExited = false;


    private Animator _animator;
    public Animator animator
    {
        get { return _animator; }
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        transform.Rotate(90.0f, 0.0f, 0.0f);

        _isEntered = false;
        _isStayed = false;
        _isExited = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isEntered == true)
            return;
        if (other.gameObject.tag == "Enemy")
        {
            _animator.SetTrigger("Size");
        }
        _isEntered = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if(_isStayed == true)
            return;
        if (other.gameObject.tag == "Enemy")
        {
            _animator.SetTrigger("Continue");
        }
        _isStayed = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isExited == true)
            return;
        if (other.gameObject.tag == "Enemy")
        {
            _animator.SetTrigger("Reset");
        }
        _isExited = true;
    }
}
