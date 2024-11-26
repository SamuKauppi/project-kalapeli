using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the cutting process of the block
/// </summary>
public class DrawCut : MonoBehaviour
{
    public bool IsCutting { get; set; } = false;

    // References
    [SerializeField] private LayerMask blockLayer;
    private BlockRotation blockRotation;   // Script that rotates the block that will be cut
    private LureFunctions lureProperties;

    // SerializeFields
    [SerializeField] private float lineWidth;
    [SerializeField] private float minCutDist;

    // Cut detection
    private Vector3 checkBoxSize = new(0f, 0.001f, 100f);     // Size of the cut check
    private float lineDist = 8f;                              // How far from camera is the line used for cutting
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
        cam = GameManager.Instance.MainCamera;
        blockRotation = BlockRotation.Instance;
        lureProperties = blockRotation.GetComponent<LureFunctions>();
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
        if (blockRotation.IsRotating)
        {
            wasRotating = true; 
            return;
        }

        if (!IsCutting) 
        {
            cancelCut = true;
            return; 
        }

        // Find mouse pos
        mouse = Input.mousePosition;
        mouse.z = lineDist;

        // Once this script is enabled, update line distance
        if (wasRotating)
        {
            ResetLineRender();
            UpdateLineDistance();
            wasRotating = false;
        }

        // When left mouse is pressed set pointA to the position of mouse
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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
        if (Input.GetMouseButton(0) && !cancelCut && !EventSystem.current.IsPointerOverGameObject())
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
            PerformCut();
        }

        // Animate line renderer
        if (animateCut)
        {
            cutRender.SetPosition(0, Vector3.Lerp(pointA, pointB, 1f));
        }
    }

    private void PerformCut()
    {
        blockRotation.StopRotating = false;
        if (cancelCut)
        {
            cancelCut = false;
            ResetLineRender();
            return;
        }

        Tutorial.Instance.EndCuttingTutorial();
        pointB = cam.ScreenToWorldPoint(mouse);
        if (Vector3.Distance(pointA, pointB) > minCutDist)
        {
            CreateSlicePlane();
        }
        cutRender.positionCount = 2;
        cutRender.SetPosition(0, pointA);
        cutRender.SetPosition(1, pointB);
        animateCut = true;
    }

    /// <summary>
    /// Creates a check for where the cut will be made 
    /// and when hit, cuts hit mesh
    /// </summary>
    private void CreateSlicePlane()
    {
        // Calculate the midpoint between pointA and pointB
        Vector3 pointInPlane = (pointA + pointB) / 2;

        // Define the size of the OverlapBox
        checkBoxSize.x = Vector3.Distance(pointA, pointB) * 0.5f;

        // Calculate the orientation of the OverlapBox based on the normal between pointA and pointB
        Vector3 cutPlaneNormal = Vector3.Cross(pointA - pointB, cam.transform.forward).normalized;

        // Ensure that the box aligns with the correct axes by setting orientation explicitly
        Quaternion orientation = Quaternion.LookRotation(cam.transform.forward, cutPlaneNormal);

        // Run OverlapBoxNonAlloc using the calculated values
        Collider[] results = new Collider[5];
        int numColliders = Physics.OverlapBoxNonAlloc(
            pointInPlane,
            checkBoxSize,
            results,
            orientation,
            blockLayer
        );

        if (numColliders <= 0)
        {

            return;
        }
        foreach (Collider hit in results)
        {
            if (hit && hit.gameObject.TryGetComponent(out MeshFilter _))
            {
                Cutter.Instance.PerformCut(blockRotation.gameObject, pointInPlane, cutPlaneNormal);
            }
        }

        lureProperties.CalculateMeshStatsOnly();
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnBlockCarve);
    }

    private void UpdateLineDistance()
    {
        Collider blockColl = blockRotation.GetComponent<Collider>();
        Vector3 camPos = cam.transform.position;
        lineDist = Vector3.Distance(camPos, blockColl.ClosestPoint(camPos));
    }

    /// <summary>
    /// Resets line renderer
    /// </summary>
    public void ResetLineRender()
    {
        pointA = Vector3.zero;
        pointB = Vector3.zero;
        cutRender.positionCount = 2;
        cutRender.SetPosition(0, pointA);
        cutRender.SetPosition(1, pointB);
    }
}