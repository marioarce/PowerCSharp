using System;
using System.Collections.Generic;
using System.IO;
using PowerCSharp.Core;
using PowerCSharp.Extensions;
using PowerCSharp.Utilities;
using PowerCSharp.Helpers;

namespace ConsoleSample;

/// <summary>
/// Extension methods for demonstrating PowerCSharp library functionality
/// </summary>
public static class ConsoleDemoExtensions
{
    /// <summary>
    /// Runs a comprehensive demo of all PowerCSharp library features
    /// </summary>
    public static void RunPowerCSharpDemo()
    {
        Console.WriteLine("=== PowerCSharp Library Demo ===\n");

        // String Extensions Demo
        Console.WriteLine("1. String Extensions (PowerCSharp.Core):");
        string text = "hello world from powercsharp";
        Console.WriteLine($"Original: '{text}'");
        Console.WriteLine($"Title Case: '{text.ToTitleCase()}'");
        Console.WriteLine($"Safe Substring (0,5): '{text.SafeSubstring(0, 5)}'");
        Console.WriteLine($"IsNullOrWhiteSpace: {"".IsNullOrWhiteSpace()}");
        Console.WriteLine();

        // Collection Extensions Demo
        Console.WriteLine("2. Collection Extensions (PowerCSharp.Extensions):");
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Console.WriteLine($"Numbers: [{string.Join(", ", numbers)}]");
        Console.WriteLine($"IsNullOrEmpty: {numbers.IsNullOrEmpty()}");
        Console.WriteLine($"FirstOrDefaultSafe: {numbers.FirstOrDefaultSafe(-1)}");
        Console.WriteLine($"Page 1 (size 3): [{string.Join(", ", numbers.Page(1, 3))}]");
        Console.WriteLine($"Page 2 (size 3): [{string.Join(", ", numbers.Page(2, 3))}]");
        Console.WriteLine();

        // DateTime Extensions Demo
        Console.WriteLine("3. DateTime Extensions (PowerCSharp.Extensions):");
        var today = DateTime.Now;
        Console.WriteLine($"Today: {today:yyyy-MM-dd}");
        Console.WriteLine($"Age from birth date (1990-01-01): {new DateTime(1990, 1, 1).GetAge()} years");
        Console.WriteLine($"Is weekend: {today.IsWeekend()}");
        Console.WriteLine($"First day of month: {today.FirstDayOfMonth():yyyy-MM-dd}");
        Console.WriteLine($"Last day of month: {today.LastDayOfMonth():yyyy-MM-dd}");
        Console.WriteLine();

        // Validation Utilities Demo
        Console.WriteLine("4. Validation Utilities (PowerCSharp.Utilities):");
        string email = "user@example.com";
        string phone = "12345";
        string url = "https://github.com/marioarce/PowerCSharp";
        Console.WriteLine($"Email '{email}' is valid: {ValidationHelper.IsValidEmail(email)}");
        Console.WriteLine($"Phone '{phone}' is numeric: {ValidationHelper.IsNumeric(phone)}");
        Console.WriteLine($"URL '{url}' is valid: {ValidationHelper.IsValidUrl(url)}");
        Console.WriteLine();

        // File Utilities Demo
        Console.WriteLine("5. File Utilities (PowerCSharp.Utilities):");
        string testFile = "test.txt";
        string content = "Hello from PowerCSharp!";
        bool writeSuccess = FileHelper.SafeWriteAllText(testFile, content);
        string readContent = FileHelper.SafeReadAllText(testFile);
        Console.WriteLine($"Write to file success: {writeSuccess}");
        Console.WriteLine($"Read content: '{readContent}'");
        Console.WriteLine($"1 MB in readable format: {FileHelper.GetFileSize(1024 * 1024)}");
        Console.WriteLine();

        // Math Utilities Demo
        Console.WriteLine("6. Math Utilities (PowerCSharp.Utilities):");
        int value = 15;
        int min = 10;
        int max = 20;
        Console.WriteLine($"Clamp {value} between {min} and {max}: {MathHelper.Clamp(value, min, max)}");
        Console.WriteLine($"Is {value} in range [{min}, {max}]: {MathHelper.IsInRange(value, min, max)}");
        Console.WriteLine($"Percentage (25 of 100): {MathHelper.Percentage(25, 100):F1}%");
        Console.WriteLine();

        // JSON Helpers Demo
        Console.WriteLine("7. JSON Helpers (PowerCSharp.Helpers):");
        var person = new { Name = "Mario Arce", Age = 30, Country = "Argentina" };
        string json = JsonHelper.SafeSerialize(person);
        Console.WriteLine($"Serialized: {json}");
        var prettyJson = JsonHelper.PrettyPrint(json);
        Console.WriteLine($"Pretty JSON:\n{prettyJson}");
        Console.WriteLine();

        // Crypto Helpers Demo
        Console.WriteLine("8. Crypto Helpers (PowerCSharp.Helpers):");
        string password = "mypassword123";
        string data = "PowerCSharp";
        Console.WriteLine($"SHA256 of '{password}': {CryptoHelper.ComputeSHA256(password)}");
        Console.WriteLine($"MD5 of '{data}': {CryptoHelper.ComputeMD5(data)}");
        Console.WriteLine($"Random string (10 chars): {CryptoHelper.GenerateRandomString(10)}");
        Console.WriteLine();

        // Environment Helpers Demo
        Console.WriteLine("9. Environment Helpers (PowerCSharp.Helpers):");
        Console.WriteLine($"Machine name: {EnvironmentHelper.GetSafeMachineName()}");
        Console.WriteLine($"Is development: {EnvironmentHelper.IsDevelopment()}");
        Console.WriteLine($"App version: {EnvironmentHelper.GetApplicationVersion()}");
        Console.WriteLine($"Custom env var (PATH): {EnvironmentHelper.GetEnvironmentVariable("PATH", "Not found")}");
        Console.WriteLine();

        // Clean up test file
        if (File.Exists(testFile))
        {
            File.Delete(testFile);
        }

        Console.WriteLine("=== Demo Complete ===");
        Console.WriteLine("PowerCSharp library is ready for use! 🚀");
    }
}
