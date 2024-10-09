using UnityEngine;

public class AttachProperties : MonoBehaviour
{
    [SerializeField] protected AttachingType attachingType;     // What will be attached
    [SerializeField] protected AttachPosition attachPosition;   // How the object will bet attached
    [SerializeField] protected bool attachBothSides;            // Attach will be mirrored to the other side
    [SerializeField] protected bool matchRotationToNormal;      // Attach will match the rotation to the normal
    [SerializeField] protected float weight;                    // Weight

    public AttachingType AttachingType { get { return attachingType; } }
    public AttachPosition AttachPosition { get { return attachPosition; } }
    public bool AttachBothSides { get { return attachBothSides; } }
    public bool MatchRotationToNormal { get { return matchRotationToNormal; } }
    public float Weight { get { return weight; } }
}
