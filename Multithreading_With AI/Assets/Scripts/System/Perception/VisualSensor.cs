using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSensor : MonoBehaviour
{
    [Range(0,200)]
    public float viewRaidus;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask target;
    public LayerMask obstacleMask;

    public float meshResolution;
    public MeshFilter viewMeshFilter;
    public Mesh viewMesh;

    private bool _active = false;
    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View-Mesh";
        viewMeshFilter.mesh = viewMesh;
    }
    private void LateUpdate()
    {
        if(_active)
            DrawVisualSensor();
        else
        {
            viewMesh.Clear();
        }
    }

    public bool ActivatingVisualSensor()
    {
        Collider[] collider = Physics.OverlapSphere(transform.localPosition, viewRaidus, target);
        if (collider.Length == 0)
            return false;
        for (int i = 0; i < collider.Length; i++)
        {
            Transform targetTransform = collider[i].transform;
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            if (Vector3.Dot(direction, transform.forward) > Mathf.Cos(viewAngle))
            {
                float distance = Vector3.Distance(transform.position, targetTransform.position);
                if (Grid.Instance.GetNodeFromWorld(targetTransform.position).walkable == TileType.Bush)
                {
                    if (this.gameObject.GetComponent<Enemy>().lastDistance > 0.0f &&
                        this.gameObject.GetComponent<Enemy>().lastDistance - distance < 2.0f)
                    {
                        // Continue
                    }
                    else
                        return false;
                }

                bool check = !Physics.Raycast(transform.position, direction, distance, obstacleMask);
                if(check == true)
                {
                    if (this.gameObject.GetComponent<Enemy>().lastDistanceRecord.Count > 2)
                         this.gameObject.GetComponent<Enemy>().lastDistance = this.gameObject.GetComponent<Enemy>().lastDistanceRecord.Dequeue();
                    else
                        this.gameObject.GetComponent<Enemy>().lastDistanceRecord.Enqueue(distance);
                }
                return check;
            }
        }
        return false;
    }

    public void EnableVisualSensor()
    {
        _active = !_active;
    }

    private void DrawVisualSensor()
    {
        int size = Mathf.RoundToInt(viewAngle * meshResolution);
        float angleSize = viewAngle / size;
        Vector3[] viewPoints = new Vector3[size+1];
        for (int i = 0; i <= size; ++i)
        {
            float angle = transform.eulerAngles.y - Mathf.Cos(viewAngle) + angleSize * i;
            RaycastInfo newCast = ViewCast(angle);
            viewPoints[i] = newCast.position;
        }

        int vertexCount = viewPoints.Length + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount -1; ++i)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if(i < vertexCount -2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    RaycastInfo ViewCast(float _angle)
    {
        Vector3 direction = new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0, Mathf.Cos(_angle * Mathf.Deg2Rad));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, viewRaidus, obstacleMask))
            return new RaycastInfo(true, hit.point, hit.distance, _angle);
        else
            return new RaycastInfo(false, transform.position + direction * viewRaidus, viewRaidus, _angle);
    }
}
