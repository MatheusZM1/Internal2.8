using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathFunctions
{
    public static float EaseIn(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Pow(t, strength);
    }

    public static float EaseOut(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return 1 - Mathf.Pow(1 - t, strength);
    }

    public static float EaseInOut(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? Mathf.Pow(t, strength) * Mathf.Pow(2, strength - 1) : 1 - Mathf.Pow(-2 * t + 2, strength) * 0.5f;
    }

    public static float ExponentialIn(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Pow(strength, 10 * t - 10);
    }

    public static float ExponentialOut(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return 1 - Mathf.Pow(strength, -10 * t);
    }

    public static float ExponentialInOut(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? Mathf.Pow(strength, 20 * t - 10) * 0.5f : (2 - Mathf.Pow(strength, -20 * t + 10)) * 0.5f;
    }

    public static float SineIn(float t)
    {
        t = Mathf.Clamp01(t);
        return 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
    }

    public static float SineOut(float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Sin((t * Mathf.PI) * 0.5f);
    }
    public static float SineInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return (-(Mathf.Cos(Mathf.PI * t) - 1) * 0.5f);
    }

    public static float BounceIn(float t, float strength)
    {
        t = Mathf.Clamp01(t);
        return 1 - BounceOut(1 - t);
    }

    public static float BounceOut(float t)
    {
        t = Mathf.Clamp01(t);
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }

    public static float BounceInOut(float t)
    {
        t = Mathf.Clamp01(t);

        if (t < 0.5f)
        {
            return (1f - BounceOut(1f - 2f * t)) / 2f;
        }
        else
        {
            return (1f + BounceOut(2f * t - 1f)) / 2f;
        }
    }

    public static float EaseInBack(float t, float strength = 1.0f)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f * strength;
        float c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    }

    public static float EaseOutBack(float t, float strength = 1.0f)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f * strength;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    public static float EaseInOutBack(float t, float strength = 1.0f)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f * strength;
        float c2 = c1 * 1.525f;

        return t < 0.5f
            ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (2 * t - 2) + c2) + 2) / 2;
    }
}