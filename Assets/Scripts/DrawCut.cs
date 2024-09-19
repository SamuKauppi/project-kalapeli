#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

public class DrawCut : MonoBehaviour
{
    // References
    [SerializeField] private BlockRotation blockRotation;   // Script that rotates the block that will be cut
    GameObject cutted;                                      // Game object that will be cut (is used fro saving)

    // SerializeFields
    [SerializeField] private Vector3 checkBoxSize = new(1000f, 0.01f, 1000f);   // Size of the cut check
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

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        cutRender = GetComponent<LineRenderer>();
        cutRender.startWidth = .05f;
        cutRender.endWidth = .05f;
        ResetLineRender();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cutted) { DrawCut.SaveGameObjectAsPrefab(cutted); }

        if (blockRotation.IsRotating) { return; }

        mouse = Input.mousePosition;
        mouse.z = lineDist;

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
                cutted = hit.gameObject;
            }
        }
    }
    private void ResetLineRender()
    {
        pointA = Vector3.zero;
        pointB = Vector3.zero;
        cutRender.positionCount = 2;
        cutRender.SetPosition(0, pointA);
        cutRender.SetPosition(1, pointB);
    }
    public static void SaveGameObjectAsPrefab(GameObject obj)
    {
        if (obj != null)
        {
            var savePath = "Assets/savedPrefab.prefab";
            // Save the GameObject as a prefab
            PrefabUtility.SaveAsPrefabAsset(obj, savePath);
            Debug.Log("Saved GameObject to: " + savePath);
        }
        else
        {
            Debug.LogError("GameObject is null, cannot save.");
        }
    }
}
#endif
