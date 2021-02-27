using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentID : MonoBehaviour
{
    public string TypeName = string.Empty;
    public Vector3 position;

    private void Awake()
    {
        position = new Vector3();
        position = this.transform.position;
    }
}
