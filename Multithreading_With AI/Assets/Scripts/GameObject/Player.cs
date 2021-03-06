using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector3 _velocity;
    private CharacterController _controller;
    private float _speed = 0.5f;
    private float _gravity = -9.81f;

    // Start is called before the first frame update

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        GameObject[] bushes = GameObject.FindGameObjectsWithTag("Environment");

        if (bushes.Length > 1)
        {
            foreach (var e in bushes)
            {
                if (e.gameObject.GetComponent<EnvironmentID>().TypeName.Equals("Bush"))
                    Physics.IgnoreCollision(this.GetComponent<Collider>(), e.GetComponent<Collider>());
                else
                    continue;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_controller.isGrounded && _velocity.y < 0.0f)
        {
            _velocity.y = 0.0f;
        }

        float mHorizontal = Input.GetAxis("Horizontal");
        float mVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(mHorizontal, 0.0f, mVertical);

        _controller.Move(movement * _speed * Time.deltaTime);

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
