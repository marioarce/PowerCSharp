using System;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common mathematical operations
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps a value between a minimum and maximum
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }

    /// <summary>
    /// Checks if a number is within a range (inclusive)
    /// </summary>
    public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Converts bytes to human-readable size
    /// </summary>
    public static double Percentage(double part, double total)
    {
        if (total == 0) return 0;
        return (part / total) * 100;
    }

    /// <summary>
    /// Converts degrees to radians
    /// </summary>
    public static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    /// <summary>
    /// Converts radians to degrees
    /// </summary>
    public static double ToDegrees(double radians)
    {
        return radians * (180.0 / Math.PI);
    }

    /// <summary>
    /// Checks if a number is even
    /// </summary>
    public static bool IsEven(int number)
    {
        return number % 2 == 0;
    }

    /// <summary>
    /// Checks if a number is odd
    /// </summary>
    public static bool IsOdd(int number)
    {
        return number % 2 != 0;
    }
}
