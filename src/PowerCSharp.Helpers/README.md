# PowerCSharp.Helpers

![PowerCSharp Banner](../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Helpers](https://img.shields.io/badge/PowerCSharp.Helpers-v0.2.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Helpers.svg)](https://www.nuget.org/packages/PowerCSharp.Helpers)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Helpers.svg)](https://www.nuget.org/packages/PowerCSharp.Helpers)

Specialized helper classes for cryptography, JSON operations, and environment management. These focused utilities provide secure and reliable implementations of common programming tasks.

## 📦 Package Information

- **Package ID:** `PowerCSharp.Helpers`
- **Version:** 0.2.0
- **Target Frameworks:** .NET 8.0, .NET Standard 2.0
- **Dependencies:** 
  - `PowerCSharp.Core` v0.3.0 (for shared interfaces)
  - `System.Text.Json` v10.0.8 (for JSON operations)

## 🚀 Installation

```bash
dotnet add package PowerCSharp.Helpers
```

## 📚 Helper Classes

### 🔐 CryptoHelper

Secure cryptographic operations for hashing and random string generation.

#### Key Features
- **SHA256 hashing** for secure data integrity
- **MD5 hashing** for legacy compatibility
- **Cryptographically secure random string generation**
- **Thread-safe operations**

```csharp
using PowerCSharp.Helpers;

// SHA256 hashing
string password = "mySecurePassword123";
string sha256Hash = CryptoHelper.ComputeSHA256(password);
// Output: "n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg="

// MD5 hashing (for legacy systems)
string data = "test data";
string md5Hash = CryptoHelper.ComputeMD5(data);
// Output: "eb733a00c0c94465be1e6a656c4a2c87"

// Random string generation
string randomString = CryptoHelper.GenerateRandomString(16);
// Output: "Kj8mN2pQ5rT9wX1z" (16 characters, alphanumeric)
```

#### Security Best Practices

```csharp
public class SecurePasswordManager
{
    private readonly string _pepper;
    
    public SecurePasswordManager()
    {
        // Generate a server-wide pepper (store this securely)
        _pepper = CryptoHelper.GenerateRandomString(32);
    }
    
    public string HashPassword(string password, string salt)
    {
        // Combine password with salt and pepper
        string combined = $"{password}{salt}{_pepper}";
        return CryptoHelper.ComputeSHA256(combined);
    }
    
    public bool VerifyPassword(string password, string salt, string hash)
    {
        string computedHash = HashPassword(password, salt);
        return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
    
    public string GenerateSecureToken()
    {
        return CryptoHelper.GenerateRandomString(64);
    }
}

public class DataIntegrityChecker
{
    public string CalculateFileChecksum(string filePath)
    {
        string content = File.ReadAllText(filePath);
        return CryptoHelper.ComputeSHA256(content);
    }
    
    public bool VerifyFileIntegrity(string filePath, string expectedChecksum)
    {
        try
        {
            string actualChecksum = CalculateFileChecksum(filePath);
            return actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public void CreateIntegrityReport(string directory)
    {
        var report = new Dictionary<string, string>();
        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            try
            {
                string checksum = CalculateFileChecksum(file);
                string relativePath = Path.GetRelativePath(directory, file);
                report[relativePath] = checksum;
            }
            catch (Exception)
            {
                // Skip files that can't be read
            }
        }
        
        string reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(directory, "integrity_report.json"), reportJson);
    }
}
```

### 📄 JsonHelper

Safe and reliable JSON serialization and deserialization with comprehensive error handling.

#### Key Features
- **Safe serialization** with null handling
- **Safe deserialization** with fallback to default values
- **Pretty printing** for readable JSON output
- **Exception handling** without throwing exceptions

```csharp
using PowerCSharp.Helpers;

// Safe serialization
var user = new User { Name = "John", Age = 30 };
string json = JsonHelper.SafeSerialize(user);
// Output: {"Name":"John","Age":30}

// Safe deserialization
string jsonInput = "{\"Name\":\"Jane\",\"Age\":25}";
User? deserializedUser = JsonHelper.SafeDeserialize<User>(jsonInput);
// Returns User object or null if deserialization fails

// Pretty printing
string prettyJson = JsonHelper.PrettyPrint(jsonInput);
/* Output:
{
  "Name": "Jane",
  "Age": 25
}
*/

// Handling null objects
User? nullUser = null;
string nullJson = JsonHelper.SafeSerialize(nullUser);
// Output: {} (empty object)
```

#### Advanced Usage

```csharp
public class ConfigurationManager
{
    private readonly string _configPath;
    private Dictionary<string, object> _settings;
    
    public ConfigurationManager(string configPath)
    {
        _configPath = configPath;
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        if (!File.Exists(_configPath))
        {
            CreateDefaultConfiguration();
            return;
        }
        
        string json = File.ReadAllText(_configPath);
        _settings = JsonHelper.SafeDeserialize<Dictionary<string, object>>(json) 
                  ?? new Dictionary<string, object>();
    }
    
    private void CreateDefaultConfiguration()
    {
        _settings = new Dictionary<string, object>
        {
            ["Database"] = new { ConnectionString = "", Timeout = 30 },
            ["Logging"] = new { Level = "Information", File = "app.log" },
            ["Features"] = new { EnableCaching = true, EnableMetrics = false }
        };
        
        SaveConfiguration();
    }
    
    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (!_settings.TryGetValue(key, out var value))
        {
            return defaultValue;
        }
        
        string json = JsonHelper.SafeSerialize(value);
        return JsonHelper.SafeDeserialize<T>(json) ?? defaultValue;
    }
    
    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value;
        SaveConfiguration();
    }
    
    private void SaveConfiguration()
    {
        string json = JsonHelper.SafeSerialize(_settings);
        string prettyJson = JsonHelper.PrettyPrint(json);
        
        try
        {
            File.WriteAllText(_configPath, prettyJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save configuration: {ex.Message}");
        }
    }
    
    public void ExportConfiguration(string exportPath)
    {
        string json = JsonHelper.SafeSerialize(_settings);
        string prettyJson = JsonHelper.PrettyPrint(json);
        
        bool success = FileHelper.SafeWriteAllText(exportPath, prettyJson);
        
        if (success)
        {
            Console.WriteLine($"Configuration exported to {exportPath}");
        }
    }
}

public class ApiClient
{
    private readonly HttpClient _httpClient;
    
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            return JsonHelper.SafeDeserialize<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API request failed: {ex.Message}");
            return default;
        }
    }
    
    public async Task<bool> PostAsync<T>(string endpoint, T data)
    {
        try
        {
            string json = JsonHelper.SafeSerialize(data);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API request failed: {ex.Message}");
            return false;
        }
    }
    
    public string LogApiCall<T>(string method, string endpoint, T? data = default)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Method = method,
            Endpoint = endpoint,
            Data = data,
            Success = true
        };
        
        string json = JsonHelper.SafeSerialize(logEntry);
        string prettyJson = JsonHelper.PrettyPrint(json);
        
        Console.WriteLine($"API Call: {prettyJson}");
        return json;
    }
}
```

### 🌍 EnvironmentHelper

Environment information and system resource management.

#### Key Features
- **Environment variable access** with default values
- **System information** retrieval
- **Resource monitoring** capabilities

```csharp
using PowerCSharp.Helpers;

// Environment variables
string dbConnection = EnvironmentHelper.GetEnvironmentVariable("DATABASE_CONNECTION", 
    "Server=localhost;Database=Default;");
string logLevel = EnvironmentHelper.GetEnvironmentVariable("LOG_LEVEL", "Information");

// System information
string machineName = EnvironmentHelper.MachineName;
string userName = EnvironmentHelper.UserName;
bool is64Bit = EnvironmentHelper.Is64BitProcess;
string osVersion = EnvironmentHelper.OSVersion.ToString();
```

#### Advanced Usage

```csharp
public class SystemDiagnostics
{
    public class SystemInfo
    {
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string OSVersion { get; set; }
        public bool Is64BitProcess { get; set; }
        public long WorkingSet { get; set; }
        public long PrivateMemory { get; set; }
        public int ThreadCount { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }
    }
    
    public SystemInfo GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();
        
        return new SystemInfo
        {
            MachineName = EnvironmentHelper.MachineName,
            UserName = EnvironmentHelper.UserName,
            OSVersion = EnvironmentHelper.OSVersion.ToString(),
            Is64BitProcess = EnvironmentHelper.Is64BitProcess,
            WorkingSet = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
            ThreadCount = process.Threads.Count,
            StartTime = process.StartTime,
            Uptime = DateTime.Now - process.StartTime
        };
    }
    
    public void LogSystemInfo()
    {
        var info = GetSystemInfo();
        string json = JsonHelper.SafeSerialize(info);
        string prettyJson = JsonHelper.PrettyPrint(json);
        
        Console.WriteLine("=== System Information ===");
        Console.WriteLine(prettyJson);
        Console.WriteLine("=========================");
    }
    
    public void MonitorResources(int intervalSeconds = 60)
    {
        var timer = new Timer(_ =>
        {
            var info = GetSystemInfo();
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                WorkingSetMB = info.WorkingSet / 1024 / 1024,
                PrivateMemoryMB = info.PrivateMemory / 1024 / 1024,
                ThreadCount = info.ThreadCount,
                UptimeMinutes = (int)info.Uptime.TotalMinutes
            };
            
            string json = JsonHelper.SafeSerialize(logEntry);
            Console.WriteLine($"Resource Monitor: {json}");
        }, null, 0, intervalSeconds * 1000);
    }
}

public class ApplicationLogger
{
    private readonly string _logDirectory;
    private readonly string _applicationName;
    
    public ApplicationLogger(string applicationName)
    {
        _applicationName = applicationName;
        _logDirectory = EnvironmentHelper.GetEnvironmentVariable("LOG_DIRECTORY", 
            Path.Combine(Environment.CurrentDirectory, "logs"));
        
        Directory.CreateDirectory(_logDirectory);
    }
    
    public void LogInfo(string message, object? data = null)
    {
        LogMessage("INFO", message, data);
    }
    
    public void LogError(string message, Exception? exception = null, object? data = null)
    {
        var logData = new
        {
            Message = message,
            Exception = exception?.ToString(),
            Data = data
        };
        
        LogMessage("ERROR", message, logData);
    }
    
    private void LogMessage(string level, string message, object? data = null)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Application = _applicationName,
            Machine = EnvironmentHelper.MachineName,
            User = EnvironmentHelper.UserName,
            Message = message,
            Data = data
        };
        
        string json = JsonHelper.SafeSerialize(logEntry);
        string logFile = Path.Combine(_logDirectory, $"{_applicationName}_{DateTime.Now:yyyyMMdd}.log");
        
        FileHelper.SafeWriteAllText(logFile, json + Environment.NewLine, append: true);
    }
    
    public void LogSystemStartup()
    {
        var systemInfo = new SystemDiagnostics().GetSystemInfo();
        LogInfo("Application started", systemInfo);
    }
    
    public void LogSystemShutdown()
    {
        LogInfo("Application shutting down");
    }
}
```

## 🎯 Use Cases

### Secure API Development
```csharp
public class SecureApiService
{
    private readonly string _apiKey;
    private readonly string _apiSecret;
    
    public SecureApiService()
    {
        _apiKey = EnvironmentHelper.GetEnvironmentVariable("API_KEY");
        _apiSecret = EnvironmentHelper.GetEnvironmentVariable("API_SECRET");
        
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret))
        {
            throw new InvalidOperationException("API credentials not configured");
        }
    }
    
    public string GenerateRequestSignature(string endpoint, string timestamp, string payload)
    {
        string toSign = $"{endpoint}{timestamp}{payload}{_apiSecret}";
        return CryptoHelper.ComputeSHA256(toSign);
    }
    
    public async Task<T?> MakeSecureRequest<T>(string endpoint, object data)
    {
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string payload = JsonHelper.SafeSerialize(data);
        string signature = GenerateRequestSignature(endpoint, timestamp, payload);
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("X-API-Key", _apiKey);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);
        
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.SendAsync(request);
        string responseJson = await response.Content.ReadAsStringAsync();
        
        return JsonHelper.SafeDeserialize<T>(responseJson);
    }
}
```

### Configuration Management
```csharp
public class ApplicationConfig
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public int Timeout { get; set; }
        public bool EnableRetry { get; set; }
    }
    
    public class SecuritySettings
    {
        public string JwtSecret { get; set; }
        public int TokenExpiryMinutes { get; set; }
        public bool EnableEncryption { get; set; }
    }
    
    public DatabaseSettings Database { get; set; }
    public SecuritySettings Security { get; set; }
    
    public static ApplicationConfig Load(string configPath = "appsettings.json")
    {
        if (!File.Exists(configPath))
        {
            return CreateDefault(configPath);
        }
        
        string json = File.ReadAllText(configPath);
        var config = JsonHelper.SafeDeserialize<ApplicationConfig>(json);
        
        if (config == null)
        {
            return CreateDefault(configPath);
        }
        
        // Override with environment variables
        config.Database.ConnectionString = EnvironmentHelper.GetEnvironmentVariable(
            "DATABASE_CONNECTION", config.Database.ConnectionString);
        config.Security.JwtSecret = EnvironmentHelper.GetEnvironmentVariable(
            "JWT_SECRET", config.Security.JwtSecret);
        
        return config;
    }
    
    private static ApplicationConfig CreateDefault(string configPath)
    {
        var config = new ApplicationConfig
        {
            Database = new DatabaseSettings
            {
                ConnectionString = "Server=localhost;Database=MyApp;",
                Timeout = 30,
                EnableRetry = true
            },
            Security = new SecuritySettings
            {
                JwtSecret = CryptoHelper.GenerateRandomString(64),
                TokenExpiryMinutes = 60,
                EnableEncryption = true
            }
        };
        
        string json = JsonHelper.SafeSerialize(config);
        string prettyJson = JsonHelper.PrettyPrint(json);
        File.WriteAllText(configPath, prettyJson);
        
        return config;
    }
    
    public void Save(string configPath = "appsettings.json")
    {
        string json = JsonHelper.SafeSerialize(this);
        string prettyJson = JsonHelper.PrettyPrint(json);
        File.WriteAllText(configPath, prettyJson);
    }
}
```

## 🔗 Dependencies

PowerCSharp.Utilities depends on:

- **PowerCSharp.Core** - Shared interfaces and base functionality
- **System.Text.Json** - JSON serialization and deserialization

## 🧪 Testing

PowerCSharp.Helpers includes comprehensive unit tests. Run tests with:

```bash
dotnet test src/PowerCSharp.Helpers.Tests
```

## 📖 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md) - Extension methods
- [PowerCSharp.Utilities](../PowerCSharp.Utilities/README.md) - Utility classes
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Workflow Documentation](../../docs/WORKFLOW.md) - Development workflow

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](../../CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Navigate to PowerCSharp.Helpers project
3. Restore dependencies: `dotnet restore`
4. Run tests: `dotnet test`
5. Make your changes
6. Add tests for new functionality
7. Submit a Pull Request

### Adding New Helpers

When adding new helper classes:

1. **Choose the right category** - Crypto, JSON, Environment, etc.
2. **Follow security best practices** - Especially for cryptographic operations
3. **Add comprehensive error handling** - Never let exceptions escape
4. **Include XML documentation** - Document all public methods
5. **Write thorough tests** - Cover edge cases and error scenarios
6. **Provide examples** - Include usage examples in documentation

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 🔗 Related Packages

- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md) - Extension methods
- [PowerCSharp.Utilities](../PowerCSharp.Utilities/README.md) - Utility classes

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp.Helpers** - Specialized helpers for common programming tasks! 🚀

[← Back to Main Documentation](../../README.md)
