using System;
using UnityEngine;

public class PRNG
{
    private readonly System.Random random;

    public PRNG() : this(Environment.TickCount) { }

    public PRNG(int seed)
    {
        random = new System.Random(seed);
    }

    public float GetPseudoRandomNumber(float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
    public int GetPseudoRandomInt(int min, int max)
    {
        return random.Next(min, max);
    }
}
