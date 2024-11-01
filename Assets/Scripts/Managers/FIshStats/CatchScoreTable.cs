using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to refer how much every type of stat in lure properties affects catch score
/// </summary>
public class CatchScoreTable : MonoBehaviour
{
    public static CatchScoreTable Instance { get; private set; }

    [SerializeField] private CatchScoreEntry[] catchScoreTable;
    private readonly Dictionary<CatchScoreType, int> catchScoreDict = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (CatchScoreEntry entry in catchScoreTable)
        {
            catchScoreDict.Add(entry.scoreType, entry.catchScore);
        }
    }

    public int GetCatchScoreForType(CatchScoreType type)
    {
        if (!catchScoreDict.ContainsKey(type))
            return 0;


        return catchScoreDict[type];
    }
}
