using UnityEngine;

public class ChubProperties : AttachProperties
{
    public SwimmingType swimmingType = SwimmingType.Standard;
    public float swimmingDepthInMeters = 2f;

    private float baseDepth;
    private void Start()
    {
        baseDepth = swimmingDepthInMeters;
    }

    public void ScaleChub(float scale)
    {
        swimmingDepthInMeters = baseDepth * scale;
    }
}
