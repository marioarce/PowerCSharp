using PowerCSharp.Extensions;
using PowerCSharp.Extensions.Strings;
using PowerCSharp.Helpers;
using PowerCSharp.Utilities;

namespace WebSample.Extensions;

/// <summary>
/// Extension methods for WebApplication to register PowerCSharp demo endpoints
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures Swagger UI middleware
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    public static void UseSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PowerCSharp Web Sample API v1");
            c.RoutePrefix = "swagger";
        });
    }

    /// <summary>
    /// Maps all PowerCSharp demo endpoints
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    public static void MapPowerCSharpDemoEndpoints(this WebApplication app)
    {
        // Root endpoint - redirect to Swagger UI
        app.MapGet("/", () => Results.Redirect("/swagger"));

        // String Extensions Demo
        app.MapGet("/demo/string", () => 
        {
            string text = "hello world from powercsharp";
            return new
            {
                original = text,
                titleCase = text.ToTitleCase(),
                safeSubstring = text.SafeSubstring(0, 5),
                isNullOrWhiteSpace = "".IsNullOrWhiteSpace()
            };
        });

        // Validation Demo
        app.MapGet("/demo/validation", () =>
        {
            string email = "user@example.com";
            string phone = "12345";
            string url = "https://github.com/marioarce/PowerCSharp";
            
            return new
            {
                email = new { value = email, isValid = ValidationHelper.IsValidEmail(email) },
                phone = new { value = phone, isNumeric = ValidationHelper.IsNumeric(phone) },
                url = new { value = url, isValid = ValidationHelper.IsValidUrl(url) }
            };
        });

        // JSON Helpers Demo
        app.MapGet("/demo/json", () =>
        {
            var person = new { Name = "Mario Arce", Age = 30, Country = "Argentina", Skills = new[] { "C#", ".NET", "Web Development" } };
            string json = JsonHelper.SafeSerialize(person);
            var prettyJson = JsonHelper.PrettyPrint(json);
            
            return new
            {
                serialized = json,
                prettyPrinted = prettyJson
            };
        });

        // Crypto Demo
        app.MapGet("/demo/crypto", () =>
        {
            string data = "PowerCSharp";
            return new
            {
                data = data,
                sha256 = CryptoHelper.ComputeSHA256(data),
                md5 = CryptoHelper.ComputeMD5(data),
                randomString = CryptoHelper.GenerateRandomString(10)
            };
        });

        // Environment Demo
        app.MapGet("/demo/environment", () =>
        {
            return new
            {
                machineName = EnvironmentHelper.GetSafeMachineName(),
                isDevelopment = EnvironmentHelper.IsDevelopment(),
                appVersion = EnvironmentHelper.GetApplicationVersion(),
                dotnetEnvironment = EnvironmentHelper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Not set")
            };
        });

        // Math Demo
        app.MapGet("/demo/math", () =>
        {
            int value = 15;
            int min = 10;
            int max = 20;
            
            return new
            {
                clamp = MathHelper.Clamp(value, min, max),
                isInRange = MathHelper.IsInRange(value, min, max),
                percentage = MathHelper.Percentage(25, 100),
                radians = MathHelper.ToRadians(180),
                degrees = MathHelper.ToDegrees(Math.PI),
                isEven = MathHelper.IsEven(4),
                isOdd = MathHelper.IsOdd(3)
            };
        });

        // DateTime Demo
        app.MapGet("/demo/datetime", () =>
        {
            var today = DateTime.Now;
            var birthDate = new DateTime(1990, 1, 1);
            
            return new
            {
                today = today.ToString("yyyy-MM-dd"),
                age = birthDate.GetAge(),
                isWeekend = today.IsWeekend(),
                firstDayOfMonth = today.FirstDayOfMonth().ToString("yyyy-MM-dd"),
                lastDayOfMonth = today.LastDayOfMonth().ToString("yyyy-MM-dd")
            };
        });

        // Collection Demo
        app.MapGet("/demo/collection", () =>
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            
            return new
            {
                numbers = numbers,
                isNullOrEmpty = numbers.IsNullOrEmpty(),
                firstOrDefaultSafe = numbers.FirstOrDefaultSafe(-1),
                page1 = numbers.Page(1, 3),
                page2 = numbers.Page(2, 3)
            };
        });
    }
}
