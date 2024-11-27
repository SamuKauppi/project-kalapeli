public class BootFish : Fish
{
    /// <summary>
    /// Returns catch chance for boot
    /// </summary>
    /// <param name="lure"></param>
    /// <returns></returns>
    public override int GetCatchChance(LureStats lure)
    {
        return lure.SwimmingDepth > 10f ? 100000000 : 0;
    }
}