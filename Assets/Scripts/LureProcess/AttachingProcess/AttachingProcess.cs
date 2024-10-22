using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles attaching objects to lure
/// </summary>
public class AttachingProcess : MonoBehaviour
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static AttachingProcess Instance { get; private set; }

    /// <summary>
    /// Is the script active
    /// </summary>
    public bool IsAttaching { get; set; } = false;

    // Event for when object is attached, moved or removed
    public delegate void AttachEvent();
    public static event AttachEvent OnAttach;

    // Distance set from camera to lure
    private float distanceToLure;

    // Attaching refs
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private AttachProperties[] attachPrefabs;                        // References to prefabs set in inspector
    private readonly Dictionary<AttachingType, AttachProperties> attachDict = new();  // Dictionary set in runtime for faster searches
    private Camera cam;
    private BlockRotation blockRotation;                                            // Script that rotates the block

    // Attachable object currently being attached
    private readonly float attachDistance = 20f;    // Used to raycast while attaching
    private GameObject lureObj;         // Lure object the objects will be attached to
    private GameObject attachedObject;  // Object being currently attached
    private GameObject mirrorObj;       // Mirrored object
    private AttachPosition attachPos;   // Where the object being currently attached can be placed
    private bool attachBothSides;       // Will the object be attached to both sides
    private bool matchRotation;         // Will the rotation of attached object be matched to face normal

    // Positioning
    Vector3 nullPos = new(0f, -1000f, 0f);  // Position where the attachbles are hidden
    private Vector3 mousePos;           // Mouse vector
    private Vector3 meshOffset;         // Distance from gameobject center to mesh position
    private float mouseHeld;            // Mouse held input detection for fixed update
    private bool mouseUp;               // Mouse up input detection for fixed update
    private bool isValidPos = false;    // Is the attached object at a valid position
    private bool wasRotating = false;   // Checks if the block was rotating and fixes attachable rotaion when needed

    /// <summary>
    /// Singleton is set in Awake
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Dictionary values is set on Start
    /// </summary>
    private void Start()
    {
        blockRotation = BlockRotation.Instance;
        foreach (AttachProperties obj in attachPrefabs)
        {
            attachDict.Add(obj.AttachingType, obj);
        }
        cam = GameManager.Instance.LureCamera;
        lureObj = blockRotation.gameObject;
    }

    /// <summary>
    /// Update is used for input detection
    /// </summary>
    private void Update()
    {
        if (!IsAttaching || !attachedObject) { return; }

        // Get mouse pos
        mousePos = Input.mousePosition;
        mousePos.z = distanceToLure;

        if (Input.GetMouseButton(0))
        {
            blockRotation.StopRotating = true;
            mouseHeld += Time.deltaTime;
        }
        else
            mouseHeld = 0f;

        if (Input.GetMouseButtonUp(0))
        {
            mouseUp = true;
        }
    }

    /// <summary>
    /// Fixed update is used for reacting to inputs
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsAttaching || !attachedObject) { return; }
        if (blockRotation.IsRotating) { wasRotating = true; return; }

        // Check if the block was rotating and fix the rotation and position if needed
        if (wasRotating)
        {
            attachedObject.transform.rotation = blockRotation.transform.rotation;
            if (attachBothSides)
            {
                mirrorObj.transform.rotation = blockRotation.transform.rotation;
            }
            if (mouseHeld <= 0.1f)
            {
                MoveObjectToAttachPosition();
            }
            wasRotating = false;
        }

        // While mouse is being held, set the attached object position to mouse
        if (mouseHeld > 0.1f)
        {
            MoveObjectToAttachPosition();
        }

        // When mouse is released either place it or destroy it
        if (mouseUp)
        {
            if (!isValidPos)
            {
                Destroy(attachedObject);
                if (attachBothSides)
                    Destroy(mirrorObj);
            }
            else
            {
                attachedObject.transform.parent = lureObj.transform;
                attachedObject.GetComponent<MoveAttach>().EnableOutline(false);
                if (attachBothSides)
                {
                    mirrorObj.transform.parent = lureObj.transform;
                    mirrorObj.GetComponent<MoveAttach>().EnableOutline(false);
                }
            }

            attachedObject = null;
            mirrorObj = null;
            blockRotation.StopRotating = false;
            OnAttach?.Invoke();
            mouseUp = false;
        }
    }

    /// <summary>
    /// Check which attach position will be used
    /// </summary>
    private void MoveObjectToAttachPosition()
    {
        switch (attachPos)
        {
            case AttachPosition.Side:
                MoveObject(attachedObject, -lureObj.transform.forward.normalized, attachBothSides);
                break;
            case AttachPosition.Bottom:
                MoveObject(attachedObject, -lureObj.transform.up.normalized, attachBothSides);
                break;
            case AttachPosition.Top:
                MoveObject(attachedObject, lureObj.transform.up.normalized, attachBothSides);
                break;
            case AttachPosition.Front:
                MoveObject(attachedObject, lureObj.transform.right.normalized, attachBothSides);
                break;
            case AttachPosition.Behind:
                MoveObject(attachedObject, -lureObj.transform.right.normalized, attachBothSides);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Move object to position based on mouse position
    /// </summary>
    /// <param name="attachObject">Which object will be move</param>
    /// <param name="directionAway">Direction away from the lure</param>
    /// <param name="attachBothSides">If the attaching process will be mirrored</param>
    private void MoveObject(GameObject attachObject, Vector3 directionAway, bool attachBothSides)
    {
        // Get mouse position away from lure
        Vector3 posAwayFromLure = cam.ScreenToWorldPoint(mousePos);
        posAwayFromLure.z += meshOffset.z;
        posAwayFromLure += 0.5f * attachDistance * directionAway;

        // Raycast
        if (Physics.Raycast(posAwayFromLure, -directionAway, out RaycastHit hit, attachDistance, blockLayer))
        {
            // Move the object to hit position when hits
            attachObject.transform.position = hit.point;

            // Rotate object to match hit plane normal if it's active
            if (matchRotation)
            {
                attachObject.transform.rotation = Quaternion.FromToRotation(directionAway, hit.normal) * Quaternion.Euler(lureObj.transform.eulerAngles);
            }

            isValidPos = true;
        }
        else
        {
            // If no hit, move to mouse pos
            attachObject.transform.position = cam.ScreenToWorldPoint(mousePos);
            isValidPos = false;
        }

        // Move the possible mirror object but disable it for the next
        if (attachBothSides)
        {
            MoveObject(mirrorObj, -directionAway, false);
        }
    }

    /// <summary>
    /// Returns vector that can be used to flip scale
    /// </summary>
    /// <returns></returns>
    private Vector3 GetFlippedScale()
    {
        return attachPos switch
        {
            AttachPosition.Side => new Vector3(1f, 1f, -1f),
            AttachPosition.Front => new Vector3(-1f, 1f, 1f),
            AttachPosition.Bottom or AttachPosition.Top => new Vector3(1f, -1f, 1f),
            _ => new Vector3(-1f, -1f, -1f)
        };
    }

    /// <summary>
    /// Calculates the center position of mesh in world space
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    private Vector3 CalculateMeshCenter(Vector3[] vertices, Transform transform)
    {
        // Calculate the average of the vertices
        Vector3 center = Vector3.zero;

        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }

        center /= vertices.Length;

        // Convert the local center to world space
        return transform.TransformPoint(center);
    }

    /// <summary>
    /// Updates distance from camera to block
    /// </summary>
    private void UpdateDistance()
    {
        distanceToLure = Vector3.Distance(cam.transform.position, lureObj.transform.position);
    }

    /// <summary>
    /// Activates the attaching related variables and stuff
    /// </summary>
    public void ActivateAttaching()
    {
        IsAttaching = true;
        Vector3 meshCenter = CalculateMeshCenter(lureObj.GetComponent<MeshFilter>().mesh.vertices, lureObj.transform);
        meshOffset = meshCenter - transform.position;
        UpdateDistance();
    }


    /// <summary>
    /// Set new attaching object
    /// </summary>
    /// <param name="type">Type of object being set</param>
    public void StartAttachingObject(AttachingType type)
    {
        // Check that attaching not disabled
        if (!IsAttaching) { return; }

        // Check if the object is determined
        if (!attachDict.ContainsKey(type)) { Debug.LogWarning("key " + type + " was not found."); return; }

        // Set the attached object
        attachedObject = Instantiate(attachDict[type].gameObject,
                                  nullPos,
                                  GameManager.Instance.LureCamera.transform.rotation,
                                  transform);

        attachPos = attachDict[type].AttachPosition;
        attachBothSides = attachDict[type].AttachBothSides;
        matchRotation = attachDict[type].MatchRotationToNormal;

        // Save data to the object incase it's moved later
        MoveAttach ma1 = attachedObject.GetComponent<MoveAttach>();
        ma1.EnableOutline(true);
        ma1.AttachPosition = attachPos;
        ma1.MatchRotation = matchRotation;

        // Set the possible mirrored object
        if (attachBothSides)
        {
            mirrorObj = Instantiate(attachDict[type].gameObject,
                                    nullPos,
                                    GameManager.Instance.LureCamera.transform.rotation,
                                    transform);
            Vector3 flipScale = GetFlippedScale();

            mirrorObj.transform.localScale = flipScale;

            // Save data to objects incase both need to be moved
            ma1.MirroredObj = mirrorObj;
            MoveAttach ma2 = mirrorObj.GetComponent<MoveAttach>();
            ma2.EnableOutline(true);
            ma2.IsMirrored = true;  // Tell the new object that it's a mirror that cannot be moved
        }

        blockRotation.ResetRotation();
    }

    /// <summary>
    /// Object wants to be moved
    /// </summary>
    /// <param name="obj">Object that will be moved</param>
    /// <param name="pos">What was it's attach position type</param>
    /// <param name="matchRot">Was the rotation matched</param>
    /// <param name="mirror">Was there a mirrored object</param>
    public void MoveAttached(GameObject obj, AttachPosition pos, bool matchRot, GameObject mirror)
    {
        // Check that attaching not disabled and we don't have an attached obj
        if (!IsAttaching || attachedObject) { return; }

        attachedObject = obj;
        attachedObject.transform.parent = transform;
        attachedObject.transform.position = cam.ScreenToWorldPoint(mousePos);
        attachPos = pos;
        matchRotation = matchRot;
        mirrorObj = mirror;

        if (mirrorObj != null)
        {
            mirrorObj.transform.parent = transform;
            mirrorObj.transform.position = cam.WorldToScreenPoint(mousePos);
            attachBothSides = true;
            mirrorObj.GetComponent<MoveAttach>().EnableOutline(true);
        }
        else
        {
            attachBothSides = false;
        }

        blockRotation.ResetRotation();
    }
}
