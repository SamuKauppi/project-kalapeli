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
    // Distance set from camera to lure
    [SerializeField] private float distanceToLure;

    // Attaching refs
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private BlockRotation blockRotation;                           // Script that rotates the block
    [SerializeField] private ObjectToAttach[] objectTypes;                          // Types of object that will be attached
    private readonly Dictionary<AttachingType, ObjectToAttach> attachDict = new();  // Dictionary set in runtime for faster searches
    private Camera cam;

    // Attachable object currently being attached
    [SerializeField] private float attachDistance;
    private GameObject lureObj;         // Lure object the objects will be attached to
    private GameObject attachedObj;     // Object being currently attached
    private GameObject mirrorObj;
    private AttachPosition attachPos;   // Where the object being currently attached can be placed
    private bool attachBothSides;

    // Positioning
    private Vector3 mousePos;           // Mouse vector
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
        if (!IsAttaching || !attachedObj || blockRotation.IsRotating) { return; }

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
                Destroy(attachedObj);
                if (attachBothSides)
                    Destroy(mirrorObj);
            }
            else
            {
                attachedObj.transform.parent = lureObj.transform;
                if (attachBothSides)
                    mirrorObj.transform.parent = lureObj.transform;
            }

            attachedObj = null;
            blockRotation.StopRotating = false;
        }
    }

    private void FixedUpdate()
    {
        if (!IsAttaching || !attachedObj || blockRotation.IsRotating) { return; }
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
                MoveObject(attachedObj, lureObj.transform.right.normalized, attachBothSides);
                break;
            case AttachPosition.Bottom:
                MoveObject(attachedObj, -lureObj.transform.forward.normalized, attachBothSides);
                break;
            case AttachPosition.Top:
                MoveObject(attachedObj, lureObj.transform.forward.normalized, attachBothSides);
                break;
            case AttachPosition.Front:
                MoveObject(attachedObj, -lureObj.transform.up.normalized, attachBothSides);
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
        posAwayFromLure += 0.5f * attachDistance * directionAway;

        if (Physics.Raycast(posAwayFromLure, -directionAway, out RaycastHit hit, attachDistance, blockLayer))
        {
            // Move the object to hit position when hits
            attachObject.transform.position = hit.point;
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
        attachedObj = Instantiate(attachDict[type].attachedPrefab,
                                  nullPos,
                                  attachDict[type].attachedPrefab.transform.rotation);
        attachPos = attachDict[type].attachPosition;
        attachBothSides = attachDict[type].attachBothSides;

        // Set the possible mirrored object
        if (attachBothSides)
        {
            mirrorObj = Instantiate(attachDict[type].attachedPrefab,
                                    nullPos,
                                    attachDict[type].attachedPrefab.transform.rotation);
            mirrorObj.transform.localScale *= -1f;
        }

        blockRotation.ResetRotation();
    }
}
