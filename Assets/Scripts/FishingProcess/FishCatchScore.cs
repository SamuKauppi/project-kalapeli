/// <summary>
/// Class created during runtime when a lure is placed in water
/// Is used to determine how likely the given fish is caught
/// </summary>
public class FishCatchScore
{
    public FishSpecies species;
    public int minScore;
    public int maxScore;
    public float timeAttached; // How long the fish is attached until it frees itself
}