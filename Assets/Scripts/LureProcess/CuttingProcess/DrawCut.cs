using UnityEngine;

/// <summary>
/// Handles the cutting process of the block
/// </summary>
public class DrawCut : MonoBehaviour
{
    public bool IsCutting { get; set; } = false;

    // References
    [SerializeField] private LayerMask blockLayer;
    private BlockRotation blockRotation;   // Script that rotates the block that will be cut
    private LureProperties lureProperties;

    // SerializeFields
    [SerializeField] private float lineWidth;
    private Vector3 checkBoxSize = new(1000f, 0.001f, 1000f);   // Size of the cut check
    private float lineDist = 8f;                                // How far from camera is the line used for cutting

    // Cut detection
    private Camera cam;             // Main camera
    private Vector3 mouse;          // Mouse position on screen
    private Vector3 pointA;         // Start of the cut (moves with the object) 
    private Vector3 pointB;         // End of the cut
    private Vector3 localPointA;    // Position of transform when click happened 

    // Line renderer
    private LineRenderer cutRender;
    private bool animateCut;
    private bool cancelCut;
    private bool wasRotating;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameManager.Instance.LureCamera;
        blockRotation = BlockRotation.Instance;
        lureProperties = blockRotation.GetComponent<LureProperties>();
        cutRender = GetComponent<LineRenderer>();
        checkBoxSize.y = lineWidth;
        cutRender.startWidth = lineWidth;
        cutRender.endWidth = lineWidth;
        ResetLineRender();
        UpdateLineDistance();
    }

    // Update is called once per frame
    void Update()
    {
        // Stop the ability to cut when the block is rotatig
        if (blockRotation.IsRotating || !IsCutting) { wasRotating = true; return; }

        // Once this script is enabled, update line distance
        if (wasRotating)
        {
            UpdateLineDistance();
            wasRotating = false;
        }

        // Find mouse pos
        mouse = Input.mousePosition;
        mouse.z = lineDist;

        // When left mouse is pressed set pointA to the position of mouse
        if (Input.GetMouseButtonDown(0))
        {
            pointA = cam.ScreenToWorldPoint(mouse);
            localPointA = transform.InverseTransformPoint(pointA);
            cancelCut = false;
            blockRotation.StopRotating = true;
        }

        // When right mouse is pressed, cancel cutting process
        if (Input.GetMouseButtonDown(1))
        {
            blockRotation.StopRotating = false;
            cancelCut = true;
            ResetLineRender();
        }

        // When left mouse is held, set the positions of line renderer
        if (Input.GetMouseButton(0) && !cancelCut)
        {
            pointA = transform.TransformPoint(localPointA);
            animateCut = false;
            cutRender.SetPosition(0, pointA);
            cutRender.SetPosition(1, cam.ScreenToWorldPoint(mouse));
        }

        // When left mouse is released and not canceling:
        // Make the cut
        if (Input.GetMouseButtonUp(0))
        {
            blockRotation.StopRotating = false;
            if (cancelCut)
            {
                cancelCut = false;
                return;
            }

            pointB = cam.ScreenToWorldPoint(mouse);
            if (Vector3.Distance(pointA, pointB) > 0)
            {
                CreateSlicePlane();
                cutRender.positionCount = 2;
                cutRender.SetPosition(0, pointA);
                cutRender.SetPosition(1, pointB);
                animateCut = true;
            }
        }

        // Animate line renderer
        if (animateCut)
        {
            cutRender.SetPosition(0, Vector3.Lerp(pointA, pointB, 1f));
        }
    }

    /// <summary>
    /// Creates a check for where the cut will be made 
    /// and when hit, cuts hit mesh
    /// </summary>
    private void CreateSlicePlane()
    {
        checkBoxSize.x = Vector3.Distance(pointA, pointB);

        Vector3 pointInPlane = (pointA + pointB) / 2;

        Vector3 cutPlaneNormal = Vector3.Cross(pointA - pointB, cam.transform.forward).normalized;

        Quaternion orientation = Quaternion.FromToRotation(Vector3.up, cutPlaneNormal);

        Collider[] results = new Collider[5];
        int numColliders = Physics.OverlapBoxNonAlloc(pointInPlane,
                                                      checkBoxSize / 2,
                                                      results,
                                                      orientation,
                                                      blockLayer);

        if (numColliders <= 0) { return; }

        foreach (Collider hit in results)
        {
            if (hit && hit.gameObject.TryGetComponent(out MeshFilter _))
            {
                Cutter.Cut(hit.gameObject, pointInPlane, cutPlaneNormal);
            }
        }

        lureProperties.CalculateStats();
    }

    /// <summary>
    /// Resets line renderer
    /// </summary>
    private void ResetLineRender()
    {
        pointA = Vector3.zero;
        pointB = Vector3.zero;
        cutRender.positionCount = 2;
        cutRender.SetPosition(0, pointA);
        cutRender.SetPosition(1, pointB);
    }

    private void UpdateLineDistance()
    {
        Collider blockColl = blockRotation.GetComponent<Collider>();
        Vector3 camPos = cam.transform.position;
        lineDist = Vector3.Distance(camPos, blockColl.ClosestPoint(camPos));
    }
}