public static class BucketUtils
{
    // Default buckets
    // Read as percentage, uses the highest possible.
    public static readonly int[] Buckets = { 90, 65, 35, 5 };

    public static int PercentToBucket(float pct)
    {
        if (pct <= 0f) return 0;
        foreach (var b in Buckets)
            if (pct >= b) return b;
        return 0;
    }
}
