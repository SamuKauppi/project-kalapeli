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
    private bool mouseHeld;
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

        // When let go either place it or destroy it
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

    private void MoveObjectToAttachPosition()
    {
        switch (attachPos)
        {
            case AttachPosition.Side:
                AttachObj(attachedObj, Vector3.back, attachBothSides);
                break;
            case AttachPosition.Bottom:
                AttachObj(attachedObj, Vector3.down, attachBothSides);
                break;
            case AttachPosition.Top:
                AttachObj(attachedObj, Vector3.up, attachBothSides);
                break;
            case AttachPosition.Front:
                AttachObj(attachedObj, Vector3.right, attachBothSides);
                break;
            default:
                break;
        }
    }

    private void AttachObj(GameObject attachObject, Vector3 directionAway, bool attachBothSides)
    {
        Vector3 posAwayFromLure = cam.ScreenToWorldPoint(mousePos);
        posAwayFromLure += 0.5f * attachDistance * directionAway;

        if (Physics.Raycast(posAwayFromLure, -directionAway, out RaycastHit hit, attachDistance, blockLayer))
        {
            attachObject.transform.position = hit.point;
            isValidPos = true;

            if (attachBothSides)
            {
                AttachObj(mirrorObj, -directionAway, false);
            }
        }
        else
        {
            attachObject.transform.position = cam.ScreenToWorldPoint(mousePos);
            isValidPos = false;
        }
    }


    /// <summary>
    /// Set new attaching object
    /// </summary>
    /// <param name="type">Type of object being set</param>
    public void StartAttachingObject(AttachingType type)
    {
        if (!IsAttaching) { return; }

        if (!attachDict.ContainsKey(type)) { Debug.LogWarning("key " + type + " was not found."); return; }

        attachedObj = Instantiate(attachDict[type].attachedPrefab);
        attachPos = attachDict[type].attachPosition;
        attachBothSides = attachDict[type].attachBothSides;

        if(attachBothSides)
        {
            mirrorObj = Instantiate(attachDict[type].attachedPrefab);
            mirrorObj.transform.localScale *= -1f;
        }
    }
}
