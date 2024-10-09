using UnityEngine;

/// <summary>
/// Handles fish stats on what it preys on
/// Values are determined in prefab variants
/// </summary>
public class Fish : MonoBehaviour
{
    // Swimming depth
    [SerializeField] private float minSwimDepth;
    [SerializeField] private float maxSwimDepth;

    // Preferred looks
    [SerializeField] private Color[] colors;
    [SerializeField] private AttachingType[] attachables;
    [SerializeField] private SwimmingType[] swimStyles;
    [SerializeField] private int[] patternIndices;

    // Base catch chance for fish
    [SerializeField] private float catchChance;
    [SerializeField] private int score;

    // Public getters

    public float MinSwimDepth { get { return minSwimDepth; } }
    public float MaxSwimDepth { get { return maxSwimDepth; } }

    public Color[] PreferredColors { get { return colors; } }
    public AttachingType[] PreferredAttachables { get { return attachables; } }
    public SwimmingType[] PreferredSwimStyle { get { return swimStyles; } }
    public int[] PreferredPatternIndex { get { return patternIndices; } }

    public float BaseCatchChance { get { return catchChance; } }
    public int ScoreGained { get { return score; } }
}
