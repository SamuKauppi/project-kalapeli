﻿using UnityEngine;

/// <summary>
/// This class is used to store stats of the lure
/// </summary>
public class LureStats : MonoBehaviour
{
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
}
