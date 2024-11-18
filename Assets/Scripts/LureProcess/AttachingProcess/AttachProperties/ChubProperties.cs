public class ChubProperties : AttachProperties
{
    public SwimmingType swimmingType = SwimmingType.Standard;
    public float swimmingDepthInMeters = 2f;

    private float baseDepth;
    protected override void Start()
    {
        baseWeight = weight;
        baseDepth = swimmingDepthInMeters;
    }

    public override void ScaleAttached(float scale)
    {
        weight = baseWeight * scale;
        swimmingDepthInMeters = baseDepth * scale;
    }
}
