using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    [SerializeField] private GameObject kokeiluPallo;
    [SerializeField] private GameObject cutter;
    [SerializeField] private float dist;
    [SerializeField] private Vector3 dir;
    void Start()
    {
        dir = dir.normalized;
        Mesh kokmesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] verts = kokmesh.vertices;
        Vector3[] normals = kokmesh.normals;

        for (int i = 0; i < verts.Length; i++)
        {
            Debug.Log("Vert " + i + ": " + verts[i]);
            Debug.Log("Normal " + i + ": " + normals[i]);
            Instantiate(kokeiluPallo, transform.TransformPoint(verts[i]), Quaternion.identity);
        }

        kokmesh.vertices = verts;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool overlap = Physics.ComputePenetration(GetComponent<Collider>(), transform.position, transform.rotation,
                  cutter.GetComponent<Collider>(), cutter.transform.position, cutter.transform.rotation,
                  out dir, out dist);


            if (overlap)
            {
             //   Instantiate(kokeiluPallo, transform.TransformPoint(verts[i]), Quaternion.identity);
                Debug.Log("True");
            }
            else
            {
                Debug.Log("False");
            }
        }
    }
}
