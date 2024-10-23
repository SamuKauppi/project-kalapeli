using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles fish stats on what it preys on
/// Values are determined in prefab variants
/// </summary>
public class Fish : MonoBehaviour
{
    [SerializeField] private FishSpecies species;
    // Swimming depth
    [SerializeField] private float minSwimDepth;
    [SerializeField] private float maxSwimDepth;

    // Preferred looks
    [SerializeField] private Color[] colors;
    [SerializeField] private AttachingType[] attachables;
    [SerializeField] private SwimmingType[] swimStyles;
    [SerializeField] private int[] patternIndices;

    // Base catch chance for fish
    [SerializeField] private int catchChance;
    [SerializeField] private int score;

    // Set in runtime
    private readonly HashSet<AttachingType> attachTable = new();

    // Public getters
    public FishSpecies Species { get { return species; } }
    public float MinSwimDepth { get { return minSwimDepth; } }
    public float MaxSwimDepth { get { return maxSwimDepth; } }

    public Color[] PreferredColors { get { return colors; } }
    public AttachingType[] PreferredAttachables { get { return attachables; } }
    public SwimmingType[] PreferredSwimStyle { get { return swimStyles; } }
    public int[] PreferredPatternIndex { get { return patternIndices; } }

    public float BaseCatchChance { get { return catchChance; } }
    public int ScoreGained { get { return score; } }

    private void Start()
    {
        foreach (var attachable in PreferredAttachables)
        {
            attachTable.Add(attachable);
        }
    }

    private int GetColorScore(Color baseC, Color texC, int patternID)
    {
        int score = 0;
        foreach (Color c in PreferredColors)
        {
            float baseDiff = Mathf.Sqrt(Mathf.Pow(baseC.r - c.r, 2) + Mathf.Pow(baseC.g - c.g, 2) + Mathf.Pow(baseC.b - c.b, 2));
            if (patternID != 0)
            {
                float texDiff = Mathf.Sqrt(Mathf.Pow(texC.r - c.r, 2) + Mathf.Pow(texC.g - c.g, 2) + Mathf.Pow(texC.b - c.b, 2));
                baseDiff = (baseDiff + texDiff) * 0.5f;
            }

            score += Mathf.FloorToInt(Mathf.Lerp(CatchManager.Instance.GetCatchScoreForType(CatchScoreType.Color), 0f, baseDiff));
        }

        return score;
    }

    private int GetDepthScore(float depth)
    {
        float worstDiff = maxSwimDepth - minSwimDepth;
        float depthDiff = Mathf.Abs(depth - ((minSwimDepth + maxSwimDepth) * 0.5f));

        // If depthDiff is greater than or equal to worstDiff, return 0 immediately.
        if (depthDiff >= worstDiff)
            return 0;

        int bestScore = CatchManager.Instance.GetCatchScoreForType(CatchScoreType.Depth);

        // Calculate score directly using Lerp, no need to clamp as depthDiff >= worstDiff is already handled.
        return Mathf.RoundToInt(Mathf.Lerp(bestScore, 0, depthDiff / worstDiff));
    }


    private int GetScoreFromSet<T>(T[] values, HashSet<T> set, CatchScoreType type)
    {
        int score = 0;

        foreach (T value in values)
        {
            if (set.Contains(value))
            {
                score += CatchManager.Instance.GetCatchScoreForType(type);
            }
        }
        return score;
    }

    private int GetScoreFromValue<T>(T value, T[] array, CatchScoreType type)
    {
        int score = 0;
        foreach (T v in array)
        {
            if (v.Equals(value))
            {
                score += CatchManager.Instance.GetCatchScoreForType(type);
            }
        }
        return score;
    }

    public int GetCatchChance(LureStats lure)
    {
        // if lures depth does not match the fish swimming depth, return 0 chance
        if (lure.SwimmingDepth < MinSwimDepth || lure.SwimmingDepth > MaxSwimDepth)
        {
            return 0;
        }

        // Otherwise calculate score
        int catchScore = catchChance;
        catchScore += GetDepthScore(lure.SwimmingDepth);                                                // Depth score
        catchScore += GetColorScore(lure.BaseColor, lure.TexColor, lure.PatternID);                     // Color score
        catchScore += GetScoreFromSet(lure.AttachedTypes, attachTable, CatchScoreType.Attachment);      // Attachment score
        catchScore += GetScoreFromValue(lure.SwimType, PreferredSwimStyle, CatchScoreType.SwimStyle);   // Swim score
        catchScore += GetScoreFromValue(lure.PatternID, PreferredPatternIndex, CatchScoreType.Pattern); // Patthern score
        
        // return score
        return catchScore;
    }
}