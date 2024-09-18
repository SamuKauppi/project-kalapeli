using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCut : MonoBehaviour
{

    // SerializeFields
    [SerializeField] private Vector3 checkBoxSize = new(1000f, 0.01f, 1000f);   // Size of the cut check
    [SerializeField] private float cameraRotationTime = 0.5f;                   // How fast the camera rotates
    [SerializeField] private float lineDist = 8f;                               // How far from camera is the line used for cutting

    // Cut detection
    private Camera cam;     // Main camera
    private Vector3 mouse;  // Mouse position on screen
    private Vector3 pointA; // Start of the cut
    private Vector3 pointB; // End of the cut

    // Line renderer
    private LineRenderer cutRender;
    private bool animateCut;
    private bool cancelCut;
    private int cutCounter = 5;

    // Camera rotations
    private Quaternion sideRot;
    private Quaternion frontRot;
    private Quaternion backRot;
    private Quaternion otherSideRot;

    // Camera rotation variables
    private int rotIndex = 0;   // Which rotation angle is being used now
    private bool isRotating;    // Is the IEnumerator active


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        cutRender = GetComponent<LineRenderer>();
        cutRender.startWidth = .05f;
        cutRender.endWidth = .05f;
        ResetLineRender();

        sideRot = transform.rotation;
        frontRot = transform.rotation * Quaternion.Euler(0f, 90f, 0f);
        backRot = transform.rotation * Quaternion.Euler(0f, -90f, 0f);
        otherSideRot = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating) { return; }

        mouse = Input.mousePosition;
        mouse.z = lineDist;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(RotateCamera(1, 0));
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(RotateCamera(-1, 0));
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(RotateCamera(0, 1));
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(RotateCamera(0, -1));
        }

        if (Input.GetMouseButtonDown(0))
        {
            pointA = cam.ScreenToWorldPoint(mouse);
            cancelCut = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            cancelCut = true;
            ResetLineRender();
        }

        if (Input.GetMouseButton(0) && !cancelCut)
        {
            animateCut = false;
            cutRender.SetPosition(0, pointA);
            cutRender.SetPosition(1, cam.ScreenToWorldPoint(mouse));
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (cancelCut)
            {
                cancelCut = false;
                return;
            }

            pointB = cam.ScreenToWorldPoint(mouse);
            CreateSlicePlane();
            cutRender.positionCount = 2;
            cutRender.SetPosition(0, pointA);
            cutRender.SetPosition(1, pointB);
            animateCut = true;
        }

        if (animateCut)
        {
            cutRender.SetPosition(0, Vector3.Lerp(pointA, pointB, 1f));
        }
    }

    private void CreateSlicePlane()
    {
        Vector3 pointInPlane = (pointA + pointB) / 2;

        Vector3 cutPlaneNormal = Vector3.Cross(pointA - pointB, cam.transform.forward).normalized;

        Quaternion orientation = Quaternion.FromToRotation(Vector3.up, cutPlaneNormal);

        Collider[] results = new Collider[cutCounter];
        int numColliders = Physics.OverlapBoxNonAlloc(pointInPlane,
                                                      checkBoxSize / 2,
                                                      results,
                                                      orientation);

        if (numColliders <= 0) { return; }

        foreach (Collider hit in results)
        {
            if (hit && !hit.gameObject.CompareTag("Floor") && hit.gameObject.TryGetComponent(out MeshFilter _))
            {
                Cutter.Cut(hit.gameObject, pointInPlane, cutPlaneNormal);
                cutCounter++;
            }
        }
    }

    private IEnumerator RotateCamera(int zRotDir, int xRotDir)
    {
        isRotating = true;
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot;

        if (zRotDir != 0)
        {
            targetRot = zRotDir < 0 ? GetSideRotation(xRotDir) : GetSideRotation(xRotDir) * Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            targetRot = GetSideRotation(xRotDir);
        }

        if (currentRot != targetRot)
        {
            float time = 0.0f;

            while (time < cameraRotationTime)
            {
                time += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, time / cameraRotationTime);
                transform.rotation = Quaternion.Lerp(currentRot, targetRot, t);
                yield return null;
            }
            transform.rotation = targetRot;
        }

        isRotating = false;
    }


    private Quaternion GetSideRotation(int xRot)
    {
        rotIndex = (rotIndex + xRot + 4) % 4;
        return rotIndex switch
        {
            0 => sideRot,
            1 => frontRot,
            2 => otherSideRot,
            3 => backRot,
            _ => sideRot,
        };
    }

    private void ResetLineRender()
    {
        pointA = Vector3.zero;
        pointB = Vector3.zero;
        cutRender.positionCount = 2;
        cutRender.SetPosition(0, pointA);
        cutRender.SetPosition(1, pointB);
    }

}
