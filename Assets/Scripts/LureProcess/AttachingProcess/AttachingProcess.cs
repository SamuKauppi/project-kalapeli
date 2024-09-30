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
    public bool IsAttaching { get; private set; } = false;
    // Distance set from camera to lure
    private float distanceToLure;

    // Attaching refs
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private ObjectToAttach[] objectTypes;                          // Types of object that will be attached
    private readonly Dictionary<AttachingType, ObjectToAttach> attachDict = new();  // Dictionary set in runtime for faster searches
    private Camera cam;
    private BlockRotation blockRotation;                                            // Script that rotates the block

    // Attachable object currently being attached
    private readonly float attachDistance = 20f;    // Used to raycast while attaching
    private GameObject lureObj;         // Lure object the objects will be attached to
    private Collider lureCollider;      // Lure objects meshfilter that is used to create offset to attach object properly
    private GameObject attachedObject;  // Object being currently attached
    private GameObject mirrorObj;       // Mirrored object
    private AttachPosition attachPos;   // Where the object being currently attached can be placed
    private bool attachBothSides;       // Will the object be attached to both sides
    private bool matchRotation;         // Will the rotation of attached object be matched to face normal

    // Positioning
    private Vector3 mousePos;           // Mouse vector
    private Vector3 meshOffset;         // Distance from gameobject center to mesh position
    private bool mouseHeld;             // Mouse input detection for fixed update
    private bool isValidPos = false;    // Is the attached object at a valid position

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
        foreach (ObjectToAttach obj in objectTypes)
        {
            attachDict.Add(obj.type, obj);
        }
        cam = Camera.main;
        lureObj = blockRotation.gameObject;
        distanceToLure = Vector3.Distance(cam.transform.position, lureObj.transform.position);
    }

    private void Update()
    {
        if (!IsAttaching || !attachedObject || blockRotation.IsRotating) { return; }

        // Get mouse pos
        mousePos = Input.mousePosition;
        mousePos.z = distanceToLure;

        // While mouse is being held, set the attached object position to mouse
        if (Input.GetMouseButton(0))
        {
            blockRotation.StopRotating = true;
            mouseHeld = true;
        }
        else
            mouseHeld = false;

        // When mouse is released either place it or destroy it
        if (Input.GetMouseButtonUp(0))
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
        }
    }

    private void FixedUpdate()
    {
        if (!IsAttaching || !attachedObject || blockRotation.IsRotating) { return; }
        if (mouseHeld)
            MoveObjectToAttachPosition();
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

        if (Physics.Raycast(posAwayFromLure, -directionAway, out RaycastHit hit, attachDistance, blockLayer))
        {
            // Move the object to hit position when hits
            attachObject.transform.position = hit.point;

            // Rotate object to match hit plane normal if it's active
            if (matchRotation)
            {
                attachObject.transform.rotation = Quaternion.Euler(lureObj.transform.eulerAngles);
                attachObject.transform.rotation *= Quaternion.FromToRotation(directionAway, hit.normal);
            }

            isValidPos = true;
        }
        else
        {
            // If no hit, move to mouse pos
            attachObject.transform.position = cam.ScreenToWorldPoint(mousePos);
            isValidPos = false;
        }

        // Move the possible mirror object but don't continue the mirroring
        if (attachBothSides)
        {
            MoveObject(mirrorObj, -directionAway, false);
        }
    }

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

    public void StartAttaching()
    {
        IsAttaching = true;
        lureCollider = lureObj.GetComponent<Collider>();
        Vector3 meshLocalCenter = lureCollider.bounds.center;
        meshOffset = transform.TransformPoint(meshLocalCenter) - transform.position;
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

        Vector3 nullPos = new(0f, -1000f, 0f);

        // Set the attached object
        attachedObject = Instantiate(attachDict[type].attachedPrefab,
                                  nullPos,
                                  Quaternion.Euler(lureObj.transform.eulerAngles));
        attachPos = attachDict[type].attachPosition;
        attachBothSides = attachDict[type].attachBothSides;
        matchRotation = attachDict[type].matchRotationToNormal;

        // Save data to the object incase it's moved later
        MoveAttach ma1 = attachedObject.GetComponent<MoveAttach>();
        ma1.EnableOutline(true);
        ma1.AttachPosition = attachPos;
        ma1.MatchRotation = matchRotation;

        // Set the possible mirrored object
        if (attachBothSides)
        {
            mirrorObj = Instantiate(attachDict[type].attachedPrefab,
                                    nullPos,
                                    Quaternion.Euler(lureObj.transform.eulerAngles));
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
        if (attachedObject)
            return;

        blockRotation.ResetRotation();
        attachedObject = obj;
        attachedObject.GetComponent<MoveAttach>().EnableOutline(true);

        attachPos = pos;
        matchRotation = matchRot;
        mirrorObj = mirror;

        if (mirrorObj != null)
        {
            attachBothSides = true;
            mirrorObj.GetComponent<MoveAttach>().EnableOutline(true);
        }
        else
        {
            attachBothSides = false;
        }
    }
}
