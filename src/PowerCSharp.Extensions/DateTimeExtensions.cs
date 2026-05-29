using System;

namespace PowerCSharp.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the age from a birth date
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        var result = age;
        return result;
    }

    /// <summary>
    /// Checks if a date is a weekend
    /// </summary>
    public static bool IsWeekend(this DateTime date)
    {
        var result = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        return result;
    }

    /// <summary>
    /// Gets the first day of the month
    /// </summary>
    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        var result = new DateTime(date.Year, date.Month, 1);
        return result;
    }

    /// <summary>
    /// Gets the last day of the month
    /// </summary>
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        var result = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        return result;
    }
}
