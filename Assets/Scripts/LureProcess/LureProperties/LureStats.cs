using UnityEngine;

/// <summary>
/// This class is used to store stats of the lure
/// </summary>
public class LureStats : MonoBehaviour
{
    public string lureName = "Fishing Lure";
    // Stats
    public float Mass = 0f;                 // Is determined by volume of mesh and attachables (grams)
    public SwimmingType SwimType;           // Is determined by chub and how streamlined is the mesh
                                            // Becomes bad if:
                                            // - Has too many attachables
                                            // - Has too many hooks
                                            // - Has more than one chub
                                            // - Is not streamlined enough 
    public float SwimmingDepth = 0f;        // Is determined by Mass and SwimmingType (meters)
    public Color BaseColor = Color.white;   // Base color (white by default)
    public Color TexColor = Color.black;    // Texture color (black by default)
    public int PatternID = 1;               // Index of paint pattern (0 = no pattern)
    public AttachingType[] AttachedTypes =  // Is set by the player
        new AttachingType[0];
    public int lureRealismValue;            // This is supposed to reward making the lure more realistic

    /// <summary>
    /// Resets all lure stats to their default values.
    /// </summary>
    public void ResetStats()
    {
        lureName = string.Empty;
        Mass = 0f;
        SwimType = SwimmingType.None;
        SwimmingDepth = 0f;
        BaseColor = Color.white;
        TexColor = Color.black;
        PatternID = 1;
        AttachedTypes = new AttachingType[0];
        lureRealismValue = 0;
    }
}
