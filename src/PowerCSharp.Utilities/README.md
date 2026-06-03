# PowerCSharp.Utilities

![PowerCSharp Banner](../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Utilities](https://img.shields.io/badge/PowerCSharp.Utilities-v0.2.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Utilities.svg)](https://www.nuget.org/packages/PowerCSharp.Utilities)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Utilities.svg)](https://www.nuget.org/packages/PowerCSharp.Utilities)

Essential utility classes and helper methods for common programming tasks including validation, file operations, and mathematical computations.

**Recent Improvements (v0.2.0):**
- **Enhanced Validation**: Improved validation utilities with better error messages
- **File Operations**: Safer file handling with automatic directory creation
- **Math Functions**: Enhanced mathematical utilities with edge case protection
- **Better Testing**: Comprehensive unit test coverage for all utility methods

## 📦 Package Information

- **Package ID:** `PowerCSharp.Utilities`
- **Version:** 0.2.0
- **Target Frameworks:** .NET 8.0, .NET Standard 2.0
- **Dependencies:** 
  - `PowerCSharp.Core` v0.3.0 (for shared interfaces)

## 🚀 Installation

```bash
dotnet add package PowerCSharp.Utilities
```

## 📚 Utility Classes

### 📁 FileHelper

Safe and reliable file operations with error handling and automatic directory creation.

#### Key Features
- **Safe file operations** with comprehensive error handling
- **Automatic directory creation** when writing files
- **Human-readable file size formatting**
- **Graceful error recovery** without exceptions

```csharp
using PowerCSharp.Utilities;

// Safe file reading
string content = FileHelper.SafeReadAllText("config.json");
// Returns empty string if file doesn't exist or can't be read

// Safe file writing with automatic directory creation
bool success = FileHelper.SafeWriteAllText("logs/app.log", "Application started");
// Creates directory if needed, returns success/failure

// Human-readable file sizes
string size1 = FileHelper.GetFileSize(1024);           // "1 KB"
string size2 = FileHelper.GetFileSize(1024 * 1024);   // "1 MB"
string size3 = FileHelper.GetFileSize(1024 * 1024 * 1024); // "1 GB"
```

#### Advanced Usage

```csharp
public class LogManager
{
    private readonly string _logDirectory;
    
    public LogManager(string logDirectory)
    {
        _logDirectory = logDirectory;
    }
    
    public void LogError(string message, Exception exception = null)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] ERROR: {message}";
        
        if (exception != null)
        {
            logEntry += $"\nException: {exception}";
        }
        
        string logFile = Path.Combine(_logDirectory, $"error_{DateTime.Now:yyyyMMdd}.log");
        bool success = FileHelper.SafeWriteAllText(logFile, logEntry + "\n", append: true);
        
        if (!success)
        {
            // Fallback logging
            Console.WriteLine($"Failed to write to log file: {logFile}");
        }
    }
    
    public string GetLogDirectorySize()
    {
        var directory = new DirectoryInfo(_logDirectory);
        long totalSize = directory.GetFiles("*.*", SearchOption.AllDirectories)
                                .Sum(f => f.Length);
        return FileHelper.GetFileSize(totalSize);
    }
}
```

### 🔢 MathHelper

Mathematical operations and utilities for common calculations.

#### Key Features
- **Value clamping** within specified ranges
- **Range checking** for numeric values
- **Percentage calculations** with zero-division protection
- **Angle conversions** between degrees and radians
- **Parity checking** for even/odd numbers

```csharp
using PowerCSharp.Utilities;

// Value clamping
int value = 150;
int clamped = MathHelper.Clamp(value, 0, 100);        // 100

// Range checking
bool inRange = MathHelper.IsInRange(75, 0, 100);       // true
bool outOfRange = MathHelper.IsInRange(150, 0, 100);   // false

// Percentage calculations
double percentage = MathHelper.Percentage(25, 100);    // 25.0
double safePercentage = MathHelper.Percentage(25, 0);   // 0.0 (no division by zero)

// Angle conversions
double radians = MathHelper.ToRadians(180);            // π (3.14159...)
double degrees = MathHelper.ToDegrees(Math.PI);        // 180.0

// Parity checking
bool isEven = MathHelper.IsEven(4);                    // true
bool isOdd = MathHelper.IsOdd(3);                      // true
```

#### Advanced Usage

```csharp
public class DataAnalyzer
{
    public class Statistics
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
        public double StandardDeviation { get; set; }
    }
    
    public Statistics CalculateStatistics(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        
        if (valueList.IsNullOrEmpty())
        {
            return new Statistics { Min = 0, Max = 0, Average = 0, StandardDeviation = 0 };
        }
        
        double min = valueList.Min();
        double max = valueList.Max();
        double average = valueList.Average();
        
        // Calculate standard deviation
        double variance = valueList.Sum(x => Math.Pow(x - average, 2)) / valueList.Count;
        double stdDev = Math.Sqrt(variance);
        
        return new Statistics { Min = min, Max = max, Average = average, StandardDeviation = stdDev };
    }
    
    public List<double> NormalizeValues(IEnumerable<double> values, double newMin = 0, double newMax = 100)
    {
        var stats = CalculateStatistics(values);
        double range = stats.Max - stats.Min;
        
        if (range == 0) return values.Select(v => newMin).ToList();
        
        return values.Select(v =>
        {
            double normalized = (v - stats.Min) / range;
            return MathHelper.Clamp(normalized * (newMax - newMin) + newMin, newMin, newMax);
        }).ToList();
    }
    
    public double CalculateGrowthRate(double oldValue, double newValue)
    {
        if (oldValue == 0) return 0;
        return MathHelper.Percentage(newValue - oldValue, oldValue);
    }
}
```

## 🎯 Use Cases

### File Processing Applications
```csharp
public class FileProcessor
{
    private readonly string _inputDirectory;
    private readonly string _outputDirectory;
    
    public FileProcessor(string inputDir, string outputDir)
    {
        _inputDirectory = inputDir;
        _outputDirectory = outputDir;
    }
    
    public void ProcessAllFiles()
    {
        var inputDir = new DirectoryInfo(_inputDirectory);
        var files = inputDir.GetFiles("*.txt");
        
        Console.WriteLine($"Processing {files.Length} files...");
        
        foreach (var file in files)
        {
            string content = FileHelper.SafeReadAllText(file.FullName);
            
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine($"Skipping empty file: {file.Name}");
                continue;
            }
            
            // Process content
            string processedContent = ProcessContent(content);
            
            // Write to output
            string outputPath = Path.Combine(_outputDirectory, file.Name);
            bool success = FileHelper.SafeWriteAllText(outputPath, processedContent);
            
            if (success)
            {
                Console.WriteLine($"Processed: {file.Name}");
            }
            else
            {
                Console.WriteLine($"Failed to process: {file.Name}");
            }
        }
        
        // Report total size
        var outputDir = new DirectoryInfo(_outputDirectory);
        long totalSize = outputDir.GetFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);
        Console.WriteLine($"Total output size: {FileHelper.GetFileSize(totalSize)}");
    }
    
    private string ProcessContent(string content)
    {
        // Your processing logic here
        return content.ToUpperInvariant();
    }
}
```

### Data Validation and Analysis
```csharp
public class DataValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
    
    public ValidationResult ValidateNumericData(IEnumerable<double> values)
    {
        var result = new ValidationResult { IsValid = true };
        var valueList = values.ToList();
        
        if (valueList.IsNullOrEmpty())
        {
            result.IsValid = false;
            result.Errors.Add("No data provided");
            return result;
        }
        
        // Check for outliers (values beyond 3 standard deviations)
        var stats = new DataAnalyzer().CalculateStatistics(valueList);
        double threshold = stats.StandardDeviation * 3;
        
        var outliers = valueList.Where(v => Math.Abs(v - stats.Average) > threshold).ToList();
        
        if (outliers.Any())
        {
            result.Warnings.Add($"Found {outliers.Count} potential outliers");
        }
        
        // Check for negative values if they shouldn't exist
        var negativeValues = valueList.Where(v => v < 0).ToList();
        if (negativeValues.Any())
        {
            result.Warnings.Add($"Found {negativeValues.Count} negative values");
        }
        
        // Check data range
        double range = stats.Max - stats.Min;
        if (range == 0)
        {
            result.Warnings.Add("All values are identical");
        }
        
        return result;
    }
    
    public void NormalizeDataSet(string inputFile, string outputFile)
    {
        string csvContent = FileHelper.SafeReadAllText(inputFile);
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2)
        {
            Console.WriteLine("Invalid CSV file");
            return;
        }
        
        // Parse numeric data (assuming first column is numeric)
        var numericValues = lines.Skip(1)
                               .Select(line => double.TryParse(line.Split(',')[0], out double val) ? val : 0)
                               .ToList();
        
        // Normalize values
        var analyzer = new DataAnalyzer();
        var normalizedValues = analyzer.NormalizeValues(numericValues);
        
        // Create output CSV
        var outputLines = new List<string> { "Original,Normalized,Percentage" };
        
        for (int i = 0; i < numericValues.Count; i++)
        {
            double original = numericValues[i];
            double normalized = normalizedValues[i];
            double percentage = MathHelper.Percentage(original, numericValues.Max());
            
            outputLines.Add($"{original},{normalized:F2},{percentage:F1}%");
        }
        
        string outputContent = string.Join('\n', outputLines);
        bool success = FileHelper.SafeWriteAllText(outputFile, outputContent);
        
        if (success)
        {
            Console.WriteLine($"Data normalized and saved to {outputFile}");
        }
    }
}
```

### Configuration Management
```csharp
public class ConfigManager
{
    private readonly string _configPath;
    private readonly Dictionary<string, string> _settings;
    
    public ConfigManager(string configPath)
    {
        _configPath = configPath;
        _settings = new Dictionary<string, string>();
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        string content = FileHelper.SafeReadAllText(_configPath);
        
        if (string.IsNullOrEmpty(content))
        {
            CreateDefaultConfiguration();
            return;
        }
        
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                _settings[parts[0].Trim()] = parts[1].Trim();
            }
        }
    }
    
    private void CreateDefaultConfiguration()
    {
        _settings["MaxRetries"] = "3";
        _settings["Timeout"] = "30";
        _settings["LogLevel"] = "Information";
        _settings["EnableCaching"] = "true";
        
        SaveConfiguration();
    }
    
    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (!_settings.TryGetValue(key, out string value))
        {
            return defaultValue;
        }
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
    
    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value?.ToString() ?? string.Empty;
        SaveConfiguration();
    }
    
    private void SaveConfiguration()
    {
        var lines = _settings.Select(kvp => $"{kvp.Key}={kvp.Value}");
        string content = string.Join('\n', lines);
        
        bool success = FileHelper.SafeWriteAllText(_configPath, content);
        
        if (!success)
        {
            Console.WriteLine($"Failed to save configuration to {_configPath}");
        }
    }
    
    public void ValidateConfiguration()
    {
        int maxRetries = GetSetting("MaxRetries", 3);
        if (!MathHelper.IsInRange(maxRetries, 1, 10))
        {
            Console.WriteLine("Warning: MaxRetries should be between 1 and 10");
            SetSetting("MaxRetries", MathHelper.Clamp(maxRetries, 1, 10));
        }
        
        int timeout = GetSetting("Timeout", 30);
        if (!MathHelper.IsInRange(timeout, 5, 300))
        {
            Console.WriteLine("Warning: Timeout should be between 5 and 300 seconds");
            SetSetting("Timeout", MathHelper.Clamp(timeout, 5, 300));
        }
    }
}
```

## 🔗 Dependencies

PowerCSharp.Utilities has minimal dependencies:

- **PowerCSharp.Core** - Shared interfaces and base functionality

This lightweight dependency profile makes PowerCSharp.Utilities ideal for inclusion in any project without adding significant overhead.

## 🧪 Testing

PowerCSharp.Utilities includes comprehensive unit tests. Run tests with:

```bash
dotnet test src/PowerCSharp.Utilities.Tests
```

## 📖 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md) - Extension methods
- [PowerCSharp.Helpers](../PowerCSharp.Helpers/README.md) - Specialized helpers
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Workflow Documentation](../../docs/WORKFLOW.md) - Development workflow

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](../../CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Navigate to PowerCSharp.Utilities project
3. Restore dependencies: `dotnet restore`
4. Run tests: `dotnet test`
5. Make your changes
6. Add tests for new functionality
7. Submit a Pull Request

### Adding New Utilities

When adding new utility classes:

1. **Choose the right category** - File, Math, Validation, etc.
2. **Follow naming conventions** - Use descriptive, PascalCase class names
3. **Add XML documentation** - Include comprehensive XML docs
4. **Write unit tests** - Cover all scenarios and edge cases
5. **Handle edge cases** - Null checks, division by zero, etc.
6. **Provide examples** - Include usage examples in documentation

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 🔗 Related Packages

- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md) - Extension methods
- [PowerCSharp.Helpers](../PowerCSharp.Helpers/README.md) - Specialized helper classes

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp.Utilities** - Essential utilities for everyday C# development! 🚀

[← Back to Main Documentation](../../README.md)
