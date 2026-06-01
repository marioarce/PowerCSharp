# PowerCSharp.Utilities - Detailed Documentation

## Overview

PowerCSharp.Utilities provides essential utility classes and helper methods for common programming tasks including file operations, mathematical computations, and validation. This package focuses on providing reliable, performant, and easy-to-use utilities that enhance developer productivity.

## Architecture

### Design Principles

1. **Zero Dependencies**: Minimal external dependencies for maximum compatibility
2. **Static Methods**: Stateless operations for easy usage and testing
3. **Error Handling**: Comprehensive error handling without throwing exceptions
4. **Performance**: Optimized for common usage scenarios
5. **Thread Safety**: All methods are thread-safe when used correctly

### Namespace Structure

```
PowerCSharp.Utilities/
├── FileHelper.cs
├── MathHelper.cs
└── (future utilities)
```

## Class Documentation

### FileHelper

Provides safe and reliable file operations with automatic error handling and directory management.

#### Key Features

- **Safe Operations**: All methods handle exceptions gracefully
- **Automatic Directory Creation**: Directories are created when needed
- **Human-Readable Formats**: File sizes in readable format (KB, MB, GB)
- **Error Recovery**: Methods return success/failure indicators

#### Methods

##### SafeReadAllText

```csharp
public static string SafeReadAllText(string path)
```

**Purpose**: Safely reads all text from a file with comprehensive error handling.

**Parameters**:
- `path`: The file path to read from

**Returns**: 
- File content as string if successful
- Empty string if file doesn't exist or read fails

**Error Handling**:
- `FileNotFoundException`: Returns empty string
- `UnauthorizedAccessException`: Returns empty string
- `IOException`: Returns empty string
- All other exceptions: Returns empty string

**Examples**:

```csharp
// Basic usage
string config = FileHelper.SafeReadAllText("config.json");
if (!string.IsNullOrEmpty(config))
{
    // Process configuration
}

// In a configuration loader
public class ConfigLoader
{
    public T LoadConfig<T>(string filePath) where T : new()
    {
        string json = FileHelper.SafeReadAllText(filePath);
        
        if (string.IsNullOrEmpty(json))
        {
            return new T(); // Return default config
        }
        
        return JsonSerializer.Deserialize<T>(json) ?? new T();
    }
}
```

##### SafeWriteAllText

```csharp
public static bool SafeWriteAllText(string path, string content)
```

**Purpose**: Safely writes text to a file, creating directories as needed.

**Parameters**:
- `path`: The file path to write to
- `content`: The content to write

**Returns**: 
- `true` if write operation succeeded
- `false` if write operation failed

**Error Handling**:
- `UnauthorizedAccessException`: Returns false
- `IOException`: Returns false
- `DirectoryNotFoundException`: Creates directory, then writes
- All other exceptions: Returns false

**Examples**:

```csharp
// Basic usage
bool success = FileHelper.SafeWriteAllText("logs/app.log", "Application started");
if (success)
{
    Console.WriteLine("Log written successfully");
}

// In a logging system
public class FileLogger
{
    private readonly string _logDirectory;
    
    public FileLogger(string logDirectory)
    {
        _logDirectory = logDirectory;
    }
    
    public void LogError(string message, Exception exception)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] ERROR: {message}\n{exception}";
        
        string logFile = Path.Combine(_logDirectory, $"error_{DateTime.Now:yyyyMMdd}.log");
        bool success = FileHelper.SafeWriteAllText(logFile, logEntry + Environment.NewLine, append: true);
        
        if (!success)
        {
            Console.WriteLine($"Failed to write error log: {logFile}");
        }
    }
}
```

##### GetFileSize

```csharp
public static string GetFileSize(long bytes)
```

**Purpose**: Converts file size in bytes to human-readable format.

**Parameters**:
- `bytes`: File size in bytes

**Returns**: Human-readable string (e.g., "1.5 MB", "500 KB")

**Examples**:

```csharp
// Basic usage
long fileSize = new FileInfo("document.pdf").Length;
string readableSize = FileHelper.GetFileSize(fileSize);
Console.WriteLine($"File size: {readableSize}"); // Output: "File size: 2.3 MB"

// In a file browser
public class FileBrowser
{
    public void DisplayDirectory(string path)
    {
        var directory = new DirectoryInfo(path);
        var files = directory.GetFiles();
        
        foreach (var file in files)
        {
            string size = FileHelper.GetFileSize(file.Length);
            Console.WriteLine($"{file.Name,-30} {size,10}");
        }
    }
}
```

#### Advanced Usage Patterns

##### Configuration Management

```csharp
public class ConfigurationManager
{
    private readonly string _configPath;
    private Dictionary<string, string> _settings;
    
    public ConfigurationManager(string configPath)
    {
        _configPath = configPath;
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        string content = FileHelper.SafeReadAllText(_configPath);
        
        if (string.IsNullOrEmpty(content))
        {
            _settings = new Dictionary<string, string>();
            SaveConfiguration(); // Create default config file
            return;
        }
        
        try
        {
            _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(content) 
                      ?? new Dictionary<string, string>();
        }
        catch
        {
            _settings = new Dictionary<string, string>();
        }
    }
    
    public void SaveConfiguration()
    {
        string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        bool success = FileHelper.SafeWriteAllText(_configPath, json);
        
        if (!success)
        {
            Console.WriteLine($"Failed to save configuration to {_configPath}");
        }
    }
    
    public string GetSetting(string key, string defaultValue = "")
    {
        return _settings.TryGetValue(key, out string value) ? value : defaultValue;
    }
    
    public void SetSetting(string key, string value)
    {
        _settings[key] = value;
        SaveConfiguration();
    }
}
```

##### Log File Management

```csharp
public class LogFileManager
{
    private readonly string _logDirectory;
    private readonly long _maxLogSize;
    
    public LogFileManager(string logDirectory, long maxLogSize = 10 * 1024 * 1024) // 10MB
    {
        _logDirectory = logDirectory;
        _maxLogSize = maxLogSize;
        Directory.CreateDirectory(_logDirectory);
    }
    
    public void WriteLog(string level, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] [{level}] {message}";
        
        string logFile = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        
        // Check file size before writing
        if (File.Exists(logFile))
        {
            long fileSize = new FileInfo(logFile).Length;
            if (fileSize > _maxLogSize)
            {
                ArchiveLogFile(logFile);
            }
        }
        
        bool success = FileHelper.SafeWriteAllText(logFile, logEntry + Environment.NewLine, append: true);
        
        if (!success)
        {
            Console.WriteLine($"Failed to write log entry to {logFile}");
        }
    }
    
    private void ArchiveLogFile(string logFile)
    {
        string archiveFile = logFile.Replace(".log", $"_{DateTime.Now:HHmmss}.log");
        bool success = FileHelper.SafeWriteAllText(archiveFile, FileHelper.SafeReadAllText(logFile));
        
        if (success)
        {
            File.Delete(logFile);
        }
    }
    
    public string GetTotalLogSize()
    {
        var directory = new DirectoryInfo(_logDirectory);
        long totalSize = directory.GetFiles("*.log", SearchOption.AllDirectories)
                                .Sum(f => f.Length);
        return FileHelper.GetFileSize(totalSize);
    }
}
```

### MathHelper

Provides mathematical operations and utilities for common calculations with built-in safety checks.

#### Key Features

- **Range Operations**: Clamping and range checking
- **Percentage Calculations**: Safe percentage computations
- **Angle Conversions**: Degrees to radians and vice versa
- **Parity Checking**: Even/odd number detection
- **Zero-Division Protection**: All operations handle edge cases

#### Methods

##### Clamp

```csharp
public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
```

**Purpose**: Clamps a value between a minimum and maximum.

**Parameters**:
- `value`: The value to clamp
- `min`: The minimum allowed value
- `max`: The maximum allowed value

**Returns**: The clamped value

**Examples**:

```csharp
// Basic usage
int clampedValue = MathHelper.Clamp(150, 0, 100); // Returns 100
double clampedDouble = MathHelper.Clamp(-5.5, 0.0, 10.0); // Returns 0.0

// In a data processing system
public class DataProcessor
{
    public double ProcessTemperature(double celsius)
    {
        // Clamp temperature to valid sensor range
        return MathHelper.Clamp(celsius, -40.0, 125.0);
    }
    
    public int ProcessScore(int rawScore)
    {
        // Clamp score to 0-100 range
        return MathHelper.Clamp(rawScore, 0, 100);
    }
}
```

##### IsInRange

```csharp
public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
```

**Purpose**: Checks if a number is within a range (inclusive).

**Parameters**:
- `value`: The value to check
- `min`: The minimum value of the range
- `max`: The maximum value of the range

**Returns**: True if value is within range, false otherwise

**Examples**:

```csharp
// Basic usage
bool inRange = MathHelper.IsInRange(75, 0, 100); // Returns true
bool outOfRange = MathHelper.IsInRange(150, 0, 100); // Returns false

// In a validation system
public class InputValidator
{
    public bool ValidateAge(int age)
    {
        return MathHelper.IsInRange(age, 0, 120);
    }
    
    public bool ValidatePercentage(double percentage)
    {
        return MathHelper.IsInRange(percentage, 0.0, 100.0);
    }
    
    public bool ValidateTemperature(double temperature)
    {
        return MathHelper.IsInRange(temperature, -273.15, 1000.0);
    }
}
```

##### Percentage

```csharp
public static double Percentage(double part, double total)
```

**Purpose**: Calculates the percentage of part relative to total with zero-division protection.

**Parameters**:
- `part`: The part value
- `total`: The total value

**Returns**: Percentage (0-100), returns 0 if total is 0

**Examples**:

```csharp
// Basic usage
double percentage = MathHelper.Percentage(25, 100); // Returns 25.0
double safePercentage = MathHelper.Percentage(25, 0); // Returns 0.0 (no division by zero)

// In a reporting system
public class ReportGenerator
{
    public class SalesReport
    {
        public double TotalSales { get; set; }
        public double OnlineSales { get; set; }
        public double RetailSales { get; set; }
        
        public double OnlinePercentage => MathHelper.Percentage(OnlineSales, TotalSales);
        public double RetailPercentage => MathHelper.Percentage(RetailSales, TotalSales);
    }
    
    public SalesReport GenerateReport(IEnumerable<Sale> sales)
    {
        var report = new SalesReport();
        
        foreach (var sale in sales)
        {
            report.TotalSales += sale.Amount;
            
            if (sale.Channel == "Online")
                report.OnlineSales += sale.Amount;
            else
                report.RetailSales += sale.Amount;
        }
        
        return report;
    }
}
```

##### ToRadians / ToDegrees

```csharp
public static double ToRadians(double degrees)
public static double ToDegrees(double radians)
```

**Purpose**: Converts between degrees and radians.

**Examples**:

```csharp
// Basic usage
double radians = MathHelper.ToRadians(180); // Returns π (3.14159...)
double degrees = MathHelper.ToDegrees(Math.PI); // Returns 180.0

// In a graphics system
public class GeometryHelper
{
    public double CalculateDistance(double x1, double y1, double x2, double y2, double angleDegrees)
    {
        double angleRadians = MathHelper.ToRadians(angleDegrees);
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy) * Math.Cos(angleRadians);
    }
    
    public double RotatePoint(double pointX, double pointY, double centerX, double centerY, double angleDegrees)
    {
        double angleRadians = MathHelper.ToRadians(angleDegrees);
        double cosAngle = Math.Cos(angleRadians);
        double sinAngle = Math.Sin(angleRadians);
        
        double translatedX = pointX - centerX;
        double translatedY = pointY - centerY;
        
        double rotatedX = translatedX * cosAngle - translatedY * sinAngle;
        double rotatedY = translatedX * sinAngle + translatedY * cosAngle;
        
        return MathHelper.ToDegrees(Math.Atan2(rotatedY, rotatedX));
    }
}
```

##### IsEven / IsOdd

```csharp
public static bool IsEven(int number)
public static bool IsOdd(int number)
```

**Purpose**: Checks if a number is even or odd.

**Examples**:

```csharp
// Basic usage
bool isEven = MathHelper.IsEven(4); // Returns true
bool isOdd = MathHelper.IsOdd(3); // Returns true

// In a scheduling system
public class TaskScheduler
{
    public void ScheduleTasks(IEnumerable<Task> tasks)
    {
        int taskCount = tasks.Count();
        
        if (MathHelper.IsEven(taskCount))
        {
            Console.WriteLine("Even number of tasks - can be perfectly balanced");
        }
        else
        {
            Console.WriteLine("Odd number of tasks - one worker will have an extra task");
        }
    }
    
    public void AssignToWorkers(IEnumerable<Task> tasks, int workerCount)
    {
        var taskList = tasks.ToList();
        int tasksPerWorker = taskList.Count / workerCount;
        
        if (MathHelper.IsOdd(taskList.Count))
        {
            Console.WriteLine("One worker will receive an extra task");
        }
        
        // Assign tasks to workers...
    }
}
```

#### Advanced Usage Patterns

##### Statistical Analysis

```csharp
public class StatisticalAnalyzer
{
    public class Statistics
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StandardDeviation { get; set; }
        public double Variance { get; set; }
    }
    
    public Statistics CalculateStatistics(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        
        if (!valueList.Any())
        {
            return new Statistics();
        }
        
        valueList.Sort();
        
        double min = valueList.First();
        double max = valueList.Last();
        double mean = valueList.Average();
        double median = CalculateMedian(valueList);
        
        double variance = CalculateVariance(valueList, mean);
        double stdDev = Math.Sqrt(variance);
        
        return new Statistics
        {
            Min = min,
            Max = max,
            Mean = mean,
            Median = median,
            Variance = variance,
            StandardDeviation = stdDev
        };
    }
    
    private double CalculateMedian(List<double> sortedValues)
    {
        int count = sortedValues.Count;
        
        if (MathHelper.IsEven(count))
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }
        else
        {
            return sortedValues[count / 2];
        }
    }
    
    private double CalculateVariance(List<double> values, double mean)
    {
        return values.Sum(x => Math.Pow(x - mean, 2)) / values.Count;
    }
    
    public List<double> NormalizeValues(IEnumerable<double> values, double newMin = 0, double newMax = 100)
    {
        var stats = CalculateStatistics(values);
        double range = stats.Max - stats.Min;
        
        if (range == 0)
        {
            return values.Select(_ => newMin).ToList();
        }
        
        return values.Select(value =>
        {
            double normalized = (value - stats.Min) / range;
            return MathHelper.Clamp(normalized * (newMax - newMin) + newMin, newMin, newMax);
        }).ToList();
    }
    
    public double CalculateZScore(double value, double mean, double standardDeviation)
    {
        if (standardDeviation == 0)
        {
            return 0;
        }
        
        return (value - mean) / standardDeviation;
    }
    
    public bool IsOutlier(double value, Statistics stats, double threshold = 3.0)
    {
        double zScore = CalculateZScore(value, stats.Mean, stats.StandardDeviation);
        return Math.Abs(zScore) > threshold;
    }
}
```

##### Data Validation and Quality Control

```csharp
public class DataQualityChecker
{
    public class QualityReport
    {
        public int TotalRecords { get; set; }
        public int ValidRecords { get; set; }
        public int InvalidRecords { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public double ValidityPercentage => MathHelper.Percentage(ValidRecords, TotalRecords);
    }
    
    public QualityReport CheckDataQuality(IEnumerable<DataRecord> records)
    {
        var report = new QualityReport();
        var recordList = records.ToList();
        
        report.TotalRecords = recordList.Count;
        
        foreach (var record in recordList)
        {
            bool isValid = ValidateRecord(record);
            
            if (isValid)
            {
                report.ValidRecords++;
            }
            else
            {
                report.InvalidRecords++;
                report.Issues.Add($"Invalid record: {record.Id}");
            }
        }
        
        return report;
    }
    
    private bool ValidateRecord(DataRecord record)
    {
        // Check if age is within reasonable range
        if (!MathHelper.IsInRange(record.Age, 0, 120))
        {
            return false;
        }
        
        // Check if score is within 0-100 range
        if (!MathHelper.IsInRange(record.Score, 0, 100))
        {
            return false;
        }
        
        // Check if percentage values are valid
        if (!MathHelper.IsInRange(record.CompletionPercentage, 0.0, 100.0))
        {
            return false;
        }
        
        return true;
    }
    
    public DataRecord CleanRecord(DataRecord record)
    {
        return new DataRecord
        {
            Id = record.Id,
            Name = record.Name,
            Age = MathHelper.Clamp(record.Age, 0, 120),
            Score = MathHelper.Clamp(record.Score, 0, 100),
            CompletionPercentage = MathHelper.Clamp(record.CompletionPercentage, 0.0, 100.0)
        };
    }
}

public class DataRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int Score { get; set; }
    public double CompletionPercentage { get; set; }
}
```

## Performance Considerations

### FileHelper Performance

1. **Buffer Size**: For large files, consider using buffered operations
2. **Async Operations**: Use async versions for I/O-bound operations
3. **File Caching**: Cache frequently accessed configuration files
4. **Directory Monitoring**: Use FileSystemWatcher for real-time updates

### MathHelper Performance

1. **Avoid Repeated Calculations**: Cache expensive computations
2. **Use Appropriate Types**: Use float for precision-critical operations
3. **Batch Operations**: Process multiple values in single operations
4. **Memory Allocation**: Minimize allocations in hot paths

## Best Practices

### Error Handling

1. **Always Check Return Values**: FileHelper methods return success/failure indicators
2. **Log Failures**: Log failed operations for debugging
3. **Provide Fallbacks**: Have default values for failed operations
4. **Validate Inputs**: Validate parameters before calling utility methods

### Thread Safety

1. **Static Methods**: All methods are thread-safe when used correctly
2. **Shared Resources**: Use proper synchronization for shared file access
3. **Atomic Operations**: Ensure file operations are atomic when needed
4. **Race Conditions**: Be aware of race conditions in concurrent scenarios

## Version History

### v0.1.0
- Initial release with FileHelper and MathHelper
- Comprehensive error handling and safety checks
- Human-readable file size formatting
- Mathematical utilities with zero-division protection
- Performance-optimized implementations

## Future Enhancements

### Planned Utilities

- **ValidationHelper**: Common validation patterns and rules
- **CompressionHelper**: File compression and decompression
- **EncryptionHelper**: Data encryption and decryption utilities
- **NetworkHelper**: Network-related utilities and helpers

### Planned Features

- **Async File Operations**: Async versions of FileHelper methods
- **Streaming Operations**: Large file processing with streaming
- **Advanced Math**: Statistical functions and matrix operations
- **Caching**: Built-in caching for frequently accessed data

---

For more information, see:
- [PowerCSharp.Utilities README](../src/PowerCSharp.Utilities/README.md)
- [PowerCSharp.Core Documentation](PowerCSharp.Core.md)
- [Main PowerCSharp Documentation](../README.md)
