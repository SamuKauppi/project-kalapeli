using UnityEngine;

/// <summary>
/// Class used to determine stats for the lure while creating it
/// After the lure is finished and used, this script will be removed
/// </summary>
public class LureFunctions : MonoBehaviour
{
    public LureStats Stats { get; private set; }
    public bool CanCatch { get { return hookCount > 0; } }

    // SerializeFields
    [SerializeField] private float streamlineMultiplier = 1f;   // Multiplies streamlineRatio to make it more readable
    [SerializeField] private float uncutBlockLengthM = 0.1f;    // The desired length of uncut block in meters (used to convert from mesh to meters)
    [SerializeField] private float materialDensity = 500f;      // kg/m³ (average for wood is 400 - 700 kg/m³)
    [SerializeField] private float thresholdStreamlineRatio;    // Min streamline ratio needed for a proper swimming type
    [SerializeField] private float minLengthOneHook = 1;        // Min length with one hook to maintain proper swimming
    [SerializeField] private float minLengthTwoHook = 2.5f;     // Min length with two hooks to maintain proper swimming
    [SerializeField] private float depthFromMassM = 0.1f;       // How much depth is added for each gram (default 10cm)
   
    // private variables
    private MeshFilter _filter;         // Reference to filter  
    private GameObject arrowObj;        // Arrow used to display direction

    // Calculation
    private float unitConverter;        // The multiplier that converts uncut block length to desired length
    private float streamlineRatio;      // StreamlineRatio
    private float worstStreamlineRatio; // Starting streamline ratio
    private float volume;               // Volume of mesh

    // Stats
    private SwimmingType meshSwim;      // The swimming style gained from mesh
    private SwimmingType attachSwim;    // The swimming style gained from attachments
    private float attachWeight;         // Total weight of all attachments
    private float chubDepth;            // How much depth is gained from the chub
    private int hookCount;
    private int eyeCount;

    // Flags
    private bool isDisplayingColor;     // Prevents multiple functions calls when no color should be shown
    private bool isStatBoundsSet;       // Detects if stat display bounds are set

    private void Start()
    {
        _filter = GetComponent<MeshFilter>();
        unitConverter = uncutBlockLengthM / _filter.mesh.bounds.size.x;
        Stats = GetComponent<LureStats>();
        arrowObj = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        AttachingProcess.OnAttach += CalculateAttachStatsOnly;
        BlockPainter.OnColorChange += UpdateColorStats;
    }

    private void OnDisable()
    {
        AttachingProcess.OnAttach -= CalculateAttachStatsOnly;
        BlockPainter.OnColorChange -= UpdateColorStats;
    }

    /// <summary>
    /// Calculates streamline ratio
    /// </summary>
    /// <returns>streamline ratio</returns>
    private float CalculateStreamlineRatio(Vector3[] vertices, int[] triangles, float volume)
    {
        // Calculate streamline index
        float surfaceArea = CalculateSurfaceArea(vertices, triangles);  // Calculate surface area

        // The higher the surface area is compared to volume, the less streamlined the mesh is
        float streamlineIndex = volume / surfaceArea;

        // Calculate how aligned to forward are normals
        float alignmentFactor = CalculateAlignment();

        // Adjust streamline index based on alignment
        streamlineIndex *= (1 + alignmentFactor);

        // Return index as ratio
        // Increase the streamlineIndex to make it more readable
        return streamlineIndex * streamlineMultiplier;
    }

    /// <summary>
    /// Calculates surface area of given vertices
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private float CalculateSurfaceArea(Vector3[] vertices, int[] triangles)
    {
        float area = 0f;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            area += CalculateTriangleArea(
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            );
        }
        // Multiply with unit converter^2 since it's square meter
        return area * Mathf.Pow(unitConverter, 2);
    }

    /// <summary>
    /// Uses Heron's formula to calculate triangle area
    /// </summary>
    /// <param name="v1">vertex 1</param>
    /// <param name="v2">vertex 2</param>
    /// <param name="v3">vertex 3</param>
    /// <returns>Area of triangle</returns>
    private float CalculateTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Vector3.Distance(v1, v2);
        float b = Vector3.Distance(v2, v3);
        float c = Vector3.Distance(v3, v1);

        float s = (a + b + c) / 2; // Semi-perimeter

        // Use Heron's formula for area 
        float herons = s * (s - a) * (s - b) * (s - c);

        // Confirm that Heron's is positive before taking squareroot
        return herons > 0 ? Mathf.Sqrt(herons) : 0f;
    }


    /// <summary>
    /// Calculates the volume of mesh
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private float CalculateVolume(Vector3[] vertices, int[] triangles)
    {
        // Approximating volume for simple convex meshes
        float volume = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            volume += Vector3.Dot(v1, Vector3.Cross(v2, v3)) / 6f; // Tetrahedron volume formula
        }
        // Multiply with unitconverter^3 since it's cubic meters
        return Mathf.Abs(volume) * Mathf.Pow(unitConverter, 3);
    }
    /// <summary>
    /// Calculates the the normal alignment with transform.forward
    /// </summary>
    /// <returns></returns>
    private float CalculateAlignment()
    {
        Vector3[] normals = _filter.mesh.normals;
        float alignmentFactor = 0f;
        foreach (var normal in normals)
        {
            alignmentFactor += Vector3.Dot(normal.normalized, Vector3.right);
        }

        alignmentFactor /= normals.Length; // Average alignment
        return alignmentFactor;
    }

    /// <summary>
    /// Returns group type from attaching type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetAttachGroup(AttachingType type)
    {
        return type switch
        {
            AttachingType.Hook1 or AttachingType.Hook2 => "Hook",
            AttachingType.Eye1 or AttachingType.Eye2 or AttachingType.Eye3
            or AttachingType.Eye4 or AttachingType.Eye5 => "Eye",
            AttachingType.Chub1 or AttachingType.Chub2
            or AttachingType.Chub3 or AttachingType.Chub4 or AttachingType.Propeller => "Chub",
            AttachingType.Fin1 or AttachingType.Fin2
            or AttachingType.Fin3 or AttachingType.Fin4 => "Fin",
            AttachingType.Tail1 or AttachingType.Tail2
            or AttachingType.Tail3 => "Tail",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Calcuates the volume and streamlineratio
    /// </summary>
    /// <param name="type"></param>
    private void CalculateMeshStats()
    {
        // Get vertices and triangles
        Vector3[] vertices = _filter.mesh.vertices;
        int[] triangles = _filter.mesh.triangles;

        // Calculate volume
        volume = CalculateVolume(vertices, triangles);

        // Calculate streamlineratio and assgin volume
        streamlineRatio = CalculateStreamlineRatio(vertices, triangles, volume);

        // Set worstStreamlineRatio first time
        if (worstStreamlineRatio == 0)
        {
            worstStreamlineRatio = streamlineRatio;
        }

        // Lerp streamline ratio
        streamlineRatio = Mathf.InverseLerp(0, worstStreamlineRatio, streamlineRatio);

        // Set mesh swiming style to bad if not streamlined enough
        meshSwim = streamlineRatio > thresholdStreamlineRatio ? SwimmingType.Bad : SwimmingType.None;
    }

    /// <summary>
    /// Calculates the stats from attached objects
    /// </summary>
    /// <param name="type"></param>
    /// <param name="depth"></param>
    private void CalculateAttachStats()
    {
        // Initialize local variabes
        float length = _filter.mesh.bounds.size.x * unitConverter;  // Convert units
        int chubCount = 0;

        // Initialize class variables
        attachWeight = 0.0f;
        attachSwim = SwimmingType.None;
        chubDepth = 0.0f;
        hookCount = 0;
        eyeCount = 0;

        // Reset attachments
        Stats.AttachedTypes = new AttachingType[transform.childCount - 1];

        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).TryGetComponent(out AttachProperties attachProperties))
            {
                continue;
            }

            // Get the group
            string groupType = GetAttachGroup(attachProperties.AttachingType);

            // Add attachble weight
            attachWeight += attachProperties.Weight;
            Stats.AttachedTypes[i - 1] = attachProperties.AttachingType;

            // Switch statement to handle different attachable types
            switch (groupType)
            {
                case "Hook":
                    HandleHook(length, ref hookCount);
                    break;

                case "Chub":
                    HandleChub(attachProperties as ChubProperties, ref chubCount);
                    break;

                case "Tail":
                    HandleHook(length, ref hookCount);
                    break;

                case "Eye":
                    eyeCount++;
                    break;

                default:
                    // Handle unknown group types if needed
                    break;
            }
        }
    }

    /// <summary>
    /// Handle logic related to hooks
    /// </summary>
    /// <param name="length"></param>
    /// <param name="hookCount"></param>
    /// <returns></returns>
    private void HandleHook(float length, ref int hookCount)
    {
        hookCount++;

        if ((length < minLengthOneHook && hookCount > 2) ||
            (length < minLengthTwoHook && hookCount > 3) ||
            hookCount > 4)
        {
            // Too many hooks
            attachSwim = SwimmingType.Bad;
        }
    }

    /// <summary>
    /// Handles logic related to chubs
    /// </summary>
    /// <param name="cp"></param>
    /// <param name="chubCount"></param>
    /// <returns></returns>
    private void HandleChub(ChubProperties cp, ref int chubCount)
    {
        // Chub was not found
        if (!cp)
        {
            return;
        }

        if (chubCount > 0)
        {
            // Too many chubs
            attachSwim = SwimmingType.Bad;
        }
        else if (attachSwim != SwimmingType.Bad)
        {
            chubDepth = cp.swimmingDepthInMeters;
            attachSwim = cp.swimmingType;
            chubCount++;
        }
    }

    /// <summary>
    /// Change color stats of the lure
    /// </summary>
    /// <param name="baseC"></param>
    /// <param name="texC"></param>
    /// <param name="textureID"></param>
    private void UpdateColorStats(Color baseC, Color texC, int textureID)
    {
        // Update variables
        Stats.BaseColor = baseC;
        Stats.TexColor = texC;
        Stats.PatternID = textureID;

        // Display stat changes
        DisplayStats();

        if (!isDisplayingColor)
        {
            StatDisplay.Instance.DisplayColors(true);
            isDisplayingColor = true;
        }
    }

    /// <summary>
    /// Displays stats if bounds are set
    /// </summary>
    private void DisplayStats()
    {
        if (isStatBoundsSet)
        {
            StatDisplay.Instance.UpdateDisplayStats(Stats.SwimType, streamlineRatio, Stats.SwimmingDepth, Stats.Mass, Stats.BaseColor, Stats.TexColor, Stats.PatternID);
        }
    }

    /// <summary>
    /// Calculates stats
    /// </summary>
    private void CalculateStats()
    {
        Stats.Mass = volume * materialDensity;
        Stats.Mass *= 1000f;
        Stats.Mass += attachWeight;
        Stats.SwimmingDepth = Stats.Mass * depthFromMassM + chubDepth;

        if (meshSwim == SwimmingType.Bad || attachSwim == SwimmingType.Bad)
        {
            Stats.SwimType = SwimmingType.Bad;
        }
        else if (attachSwim != SwimmingType.None)
        {
            Stats.SwimType = attachSwim;
        }
        else
        {
            Stats.SwimType = SwimmingType.None;
        }
    }

    /// <summary>
    /// Set realism values for this lure
    /// If lure has 3 out of 4 from these it's realistic enough for not to catch None
    /// </summary>
    private void SetRealismValue()
    {
        Stats.lureRealismValue = 0;
        Stats.lureRealismValue += attachSwim != SwimmingType.None && attachSwim != SwimmingType.Bad ? 1 : 0;
        Stats.lureRealismValue += hookCount > 0 ? 1 : 0;
        Stats.lureRealismValue += eyeCount == 2 ? 1 : 0;
        Stats.lureRealismValue += streamlineRatio < worstStreamlineRatio ? 1 : 0;
    }

    public void RenameLure(string name)
    {
        Stats.lureName = name;
    }

    /// <summary>
    /// Calculates stats gained from mesh
    /// </summary>
    public void CalculateMeshStatsOnly()
    {
        // Calculates the volume and streamlineratio
        CalculateMeshStats();

        // Calculate stats
        CalculateStats();

        // Update UI display if bounds are found
        DisplayStats();
    }

    /// <summary>
    /// Calculates stats gained from attachments
    /// </summary>
    public void CalculateAttachStatsOnly()
    {
        // Calculate stats gained from attachments
        CalculateAttachStats();

        // Calculate stats
        CalculateStats();

        // Update UI display if bounds are set
        DisplayStats();
    }

    /// <summary>
    /// Resets lure to starting block
    /// </summary>
    public void ResetLure()
    {
        // Color is no longer displayed
        isDisplayingColor = false;
        StatDisplay.Instance.DisplayColors(false);

        // Reset stat values
        Stats.ResetStats();

        // Reset variables
        meshSwim = SwimmingType.None;
        attachSwim = SwimmingType.None;
        attachWeight = 0f;
        chubDepth = 0f;
        hookCount = 0;
        eyeCount = 0;

        // Destroy children, but set arrow object active
        arrowObj.SetActive(true);
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != arrowObj)
            {
                Destroy(child);
            }
        }

        // Recalculate stats to reset them
        CalculateMeshStats();
        CalculateStats();
        DisplayStats();

        // If display bounds have not been made, make them
        if (!isStatBoundsSet)
        {
            StatDisplay.Instance.SetDisplayBounds(streamlineRatio, Stats.Mass);
            isStatBoundsSet = true;
            DisplayStats();
        }
    }

    /// <summary>
    /// Removes colliders and MoveAttach from children
    /// </summary>
    public void FinalizeLure()
    {
        if (transform.childCount == 0) return;

        SetRealismValue();

        arrowObj.SetActive(false);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.TryGetComponent<MoveAttach>(out var moveAttach))
            {
                Destroy(moveAttach);
            }

            if (child.TryGetComponent<Collider>(out var coll))
            {
                Destroy(coll);
            }
        }
    }
}
