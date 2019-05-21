using System.Collections;

static public class shaco_ExtensionsClassCSharp
{
    static public double Round(this double value, int decimals)
    {
        decimal ret = (decimal)value;
        ret = System.Math.Round(ret, decimals, System.MidpointRounding.AwayFromZero);

        return (double)ret;
    }

    static public float Round(this float value, int decimals)
    {
        return (float)Round((double)value, decimals);
    }
}
