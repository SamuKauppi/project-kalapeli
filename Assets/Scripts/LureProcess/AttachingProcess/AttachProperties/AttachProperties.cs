using UnityEngine;

public class AttachProperties : MonoBehaviour
{
    public AttachingType AttachingType { get; set; }   // Type of attachment
    public float Weight { get { return weight; } }

    [SerializeField] private float weight;
}
