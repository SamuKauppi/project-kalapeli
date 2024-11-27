using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles fish stats on what it preys on
/// Values are determined in prefab variants
/// </summary>
public class Fish : MonoBehaviour
{
    [SerializeField] private FishSpecies species;
    [TextArea(3, 15)]
    [SerializeField] private string hintText;
    [TextArea(3, 15)]
    [SerializeField] private string flavourText;

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
    [SerializeField] private float timeAttached;

    // Score gained for catching
    [SerializeField] private int score;

    // Private
    private readonly HashSet<AttachingType> attachTable = new();
    private const float DEPTH_TOLERANCE = 10f;
    private readonly float maxColorDiff = Mathf.Sqrt(3) * 0.5f;

    // Public getters
    public FishSpecies Species { get { return species; } }
    public string HintText { get { return hintText; } }
    public string FlavourText { get { return flavourText; } }
    public float MinSwimDepth { get { return minSwimDepth; } }
    public float MaxSwimDepth { get { return maxSwimDepth; } }
    public Color[] PreferredColors { get { return colors; } }
    public AttachingType[] PreferredAttachables { get { return attachables; } }
    public SwimmingType[] PreferredSwimStyle { get { return swimStyles; } }
    public int[] PreferredPatternIndex { get { return patternIndices; } }
    public float BaseCatchChance { get { return catchChance; } }
    public float TimeAttached { get { return timeAttached; } }
    public int ScoreGained { get { return score; } }

    public Sprite miniIcon;
    public Sprite bigIcon;


    /// <summary>
    /// Returns score isCatalogOpen based on how close the colors are to fish desired colors
    /// </summary>
    /// <param name="baseC"></param>
    /// <param name="texC"></param>
    /// <param name="patternID"></param>
    /// <returns></returns>
    private int GetColorScore(Color baseC, Color texC, int patternID)
    {
        int score = 0;
        int baseCatch = CatchScoreTable.Instance.GetCatchScoreForType(CatchScoreType.Color);
        foreach (Color c in PreferredColors)
        {
            float baseDiff = Mathf.Sqrt(Mathf.Pow(baseC.r - c.r, 2) + Mathf.Pow(baseC.g - c.g, 2) + Mathf.Pow(baseC.b - c.b, 2));
            if (patternID != 0)
            {
                float texDiff = Mathf.Sqrt(Mathf.Pow(texC.r - c.r, 2) + Mathf.Pow(texC.g - c.g, 2) + Mathf.Pow(texC.b - c.b, 2));
                baseDiff = (baseDiff + texDiff) * 0.5f;
            }

            baseDiff = baseDiff > maxColorDiff ? maxColorDiff : baseDiff;
            score += Mathf.RoundToInt(Mathf.Lerp(baseCatch, 0f, baseDiff / maxColorDiff));
        }

        return score;
    }

    /// <summary>
    /// Returns score isCatalogOpen based on how close is the depth to average depth
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    private int GetDepthScore(float depth)
    {
        float avgDepth = (maxSwimDepth + minSwimDepth) * 0.5f;
        float depthDiff = Mathf.Abs(depth - avgDepth);

        // If depthDiff is greater than or equal to DEPTH_TOLERANCE, return 0 immediately.
        if (depthDiff >= DEPTH_TOLERANCE)
            return 0;

        int bestScore = CatchScoreTable.Instance.GetCatchScoreForType(CatchScoreType.Depth);

        // Calculate score directly using Lerp, no need to clamp as depthDiff >= avgDepth is already handled.
        return Mathf.RoundToInt(Mathf.Lerp(bestScore, 0, depthDiff / DEPTH_TOLERANCE));
    }

    /// <summary>
    /// Returns catch score for type in a set based on number of matches in array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="set"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetScoreFromSet<T>(T[] values, HashSet<T> set, CatchScoreType type)
    {
        int score = 0;

        foreach (T value in values)
        {
            if (set.Count <= 0)
                break;

            if (set.Contains(value))
            {
                score += CatchScoreTable.Instance.GetCatchScoreForType(type);
                set.Remove(value);
            }
        }
        return score;
    }

    /// <summary>
    /// Returns catch score for type based on number of matches in array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="array"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetScoreFromValue<T>(T value, T[] array, CatchScoreType type)
    {
        int score = 0;
        var comparer = EqualityComparer<T>.Default;
        foreach (T v in array)
        {
            if (comparer.Equals(v, value))
            {
                score += CatchScoreTable.Instance.GetCatchScoreForType(type);
            }
        }
        return score;
    }

    public void InitializeFish()
    {
        attachTable.Clear();
        foreach (var attachable in PreferredAttachables)
        {
            attachTable.Add(attachable);
        }
    }

    /// <summary>
    /// Compares the stats of the lure to this fish and returns a score on how likely this fhis can be caught
    /// </summary>
    /// <param name="lure"></param>
    /// <returns></returns>
    public virtual int GetCatchChance(LureStats lure)
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
        catchScore += GetScoreFromSet(lure.AttachedTypes,
                                      new HashSet<AttachingType>(attachTable),
                                      CatchScoreType.Attachment);                                       // Attachment score
        catchScore += GetScoreFromValue(lure.SwimType, PreferredSwimStyle, CatchScoreType.SwimStyle);   // Swim score
        catchScore += GetScoreFromValue(lure.PatternID, PreferredPatternIndex, CatchScoreType.Pattern); // Patthern score

        if (lure.SwimType == SwimmingType.Bad)
        {
            return catchScore / 4;
        }

        // return score
        return catchScore;
    }
}
