namespace PR.Core;

/// <summary>
/// Exact replica of Java's java.util.Random 48-bit Linear Congruential Generator (LCG).
/// Deterministic compatibility with original J2ME bytecode.
/// </summary>
public sealed class JavaRandom
{
    private long _seed;
    private const long Multiplier = 0x5DEECE66DL;
    private const long Addend = 0xBL;
    private const long Mask = (1L << 48) - 1;

    public JavaRandom() : this(0L) { }

    public JavaRandom(long seed)
    {
        SetSeed(seed);
    }

    public void SetSeed(long seed)
    {
        _seed = (seed ^ Multiplier) & Mask;
    }

    public int Next(int bits)
    {
        _seed = (_seed * Multiplier + Addend) & Mask;
        return (int)((ulong)_seed >> (48 - bits));
    }

    public int NextInt()
    {
        return Next(32);
    }

    public int NextInt(int n)
    {
        if (n <= 0)
            throw new ArgumentException("n must be positive", nameof(n));

        if ((n & -n) == n) // power of 2
            return (int)((n * (long)Next(31)) >> 31);

        int bits, val;
        do
        {
            bits = Next(31);
            val = bits % n;
        } while (bits - val + (n - 1) < 0);

        return val;
    }
}
