using UnityEngine;

/// <summary>
/// Properties of a object attached to lure
/// </summary>
public class AttachProperties : MonoBehaviour
{
    [SerializeField] protected AttachingType attachingType;     // What will be attached
    [SerializeField] protected AttachPosition attachPosition;   // How the object will bet attached
    [SerializeField] protected bool attachBothSides;            // Attach will be mirrored to the other side
    [SerializeField] protected bool matchRotationToNormal;      // Attach will match the rotation to the normal
    [SerializeField] protected float weight;                    // Weight

    protected float baseWeight;

    protected virtual void Start()
    {
        baseWeight = weight;
    }

    public AttachingType AttachingType { get { return attachingType; } }
    public AttachPosition AttachPosition { get { return attachPosition; } }
    public bool AttachBothSides { get { return attachBothSides; } }
    public bool MatchRotationToNormal { get { return matchRotationToNormal; } }
    public float Weight { get { return weight; } }

    public virtual void ScaleAttached(float scale)
    {
        weight = baseWeight * scale;
    }
}
