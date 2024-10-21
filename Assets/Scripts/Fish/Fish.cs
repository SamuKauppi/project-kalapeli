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

    public int GetCatchChance(LureProperties lure)
    {
        int catchScore = catchChance;

        // if lures depth does not match the fish swimming depth, return 0 chance
        if (lure.SwimmingDepth < MinSwimDepth || lure.SwimmingDepth > MaxSwimDepth)
        {
            return 0;
        }

        // Otherwise calculate score
        catchScore += GetColorScore(lure.BaseColor, lure.TexColor, lure.PatternID);
        catchScore += GetScoreFromSet(lure.AttachedTypes, attachTable, CatchScoreType.Attachment);
        catchScore += GetScoreFromValue(lure.SwimType, PreferredSwimStyle, CatchScoreType.SwimStyle);
        catchScore += GetScoreFromValue(lure.PatternID, PreferredPatternIndex, CatchScoreType.Pattern);
        
        // return score
        return catchScore;
    }
}