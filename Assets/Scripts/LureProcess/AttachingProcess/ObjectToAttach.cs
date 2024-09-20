using UnityEngine;

/// <summary>
/// A class used to attach objects to lure
/// </summary>
[System.Serializable]
public class ObjectToAttach
{
    // Type of object to attach
    public AttachingType type = AttachingType.NULL;
    // Reference to prefab
    public GameObject attachedPrefab;

    // How the object will be attached
    public AttachPosition attachPosition = AttachPosition.Side;
    public bool attachBothSides = false;     // Attach will be mirrored to the other side
}
