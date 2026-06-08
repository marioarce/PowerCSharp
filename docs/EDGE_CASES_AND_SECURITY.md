# Edge Cases and Security Considerations

This document provides comprehensive guidance on handling edge cases and security considerations when using PowerCSharp extensions and utilities.

## Table of Contents

- [String Extensions](#string-extensions)
- [Path Extensions](#path-extensions)
- [Hash Extensions](#hash-extensions)
- [Validation Utilities](#validation-utilities)
- [HTTP Extensions](#http-extensions)
- [JSON Extensions](#json-extensions)
- [General Security Guidelines](#general-security-guidelines)

---

## String Extensions

### SafeSubstring

**Edge Cases:**
- Null input: Returns empty string
- Empty string: Returns empty string  
- Negative startIndex: Returns empty string
- Negative length: Returns empty string
- startIndex >= string length: Returns empty string
- length exceeds available characters: Returns substring up to end of string

**Security Considerations:**
- No exceptions thrown, safe for user input processing
- Bounds checking prevents index out of range attacks
- Performance: Slightly slower than direct SubString() but safer

**Example Usage:**
```csharp
// Safe processing of user input
string userInput = GetUserInput();
string safeExtract = userInput.SafeSubstring(0, 100); // Never throws

// Defensive programming
string? potentiallyNull = GetExternalData();
string result = potentiallyNull.SafeSubstring(5, 10); // Handles null gracefully
```

### IsNullOrWhiteSpace

**Edge Cases:**
- Null input: Returns true
- Empty string: Returns true
- Whitespace only (spaces, tabs, newlines): Returns true
- String with content: Returns false

**Security Considerations:**
- Null-safe wrapper prevents NullReferenceException
- Safe for validating user input before processing

**Example Usage:**
```csharp
// Input validation
string? username = GetUsername();
if (username.IsNullOrWhiteSpace())
{
    throw new ArgumentException("Username cannot be empty");
}

// Safe string operations
string? configValue = GetConfiguration();
if (!configValue.IsNullOrWhiteSpace())
{
    ProcessConfig(configValue);
}
```

---

## Path Extensions

### CombineAndValidate

**Critical Security Feature:** Implements CWE-73 remediation for path traversal attacks.

**Edge Cases:**
- Null basePath: Throws ArgumentNullException
- Null relativePath: Throws ArgumentNullException
- Empty basePath: Throws ArgumentException
- Directory traversal attempts: Throws SecurityException
- Valid paths: Returns validated absolute path

**Security Considerations:**
- Prevents directory traversal attacks (../, ..\\)
- Canonicalizes paths to resolve symbolic links and shortcuts
- Validates result stays within allowed base directory
- Logs security events for monitoring

**Attack Scenarios Prevented:**
```csharp
// These would throw SecurityException:
string safe1 = PathExtensions.CombineAndValidate(baseDir, "../../etc/passwd");
string safe2 = PathExtensions.CombineAndValidate(baseDir, "..\\..\\windows\\system32\\config\\sam");
string safe3 = PathExtensions.CombineAndValidate(baseDir, "folder/../../../etc/passwd");

// Valid usage:
string safe = PathExtensions.CombineAndValidate(baseDir, "user/uploads/file.jpg");
```

**Example Usage:**
```csharp
// File upload security
string uploadDir = "/var/www/uploads";
string userFile = Request.Form["filename"];
try
{
    string safePath = PathExtensions.CombineAndValidate(uploadDir, userFile);
    SaveFile(safePath, fileContent);
}
catch (SecurityException ex)
{
    LogSecurityEvent("Path traversal attempt", ex);
    return BadRequest("Invalid file path");
}
```

---

## Hash Extensions

### ComputeHash

**Edge Cases:**
- Null objects: Returns "null" string
- Non-serializable objects: Returns fallback hash based on type name
- Circular references: Handled gracefully via ReferenceHandler.IgnoreCycles
- Deep object graphs: Limited to 64 levels depth
- Large objects: Truncated to prevent memory issues

**Security Considerations:**
- Uses SHA256 for cryptographic-strength hashing
- Does not expose sensitive object data in hash output
- Handles circular references to prevent DoS attacks
- Thread-safe for concurrent use

**Performance Characteristics:**
- CPU: SHA256 computation + JSON serialization
- Memory: Temporary string allocation for JSON
- Thread-safe: Can be called from multiple threads

**Example Usage:**
```csharp
// Cache key generation
var cacheKey = user.ComputeHash();
cache.Set(cacheKey, userData, TimeSpan.FromHours(1));

// Change detection
string oldHash = storedObject.ComputeHash();
string newHash = updatedObject.ComputeHash();
if (oldHash != newHash)
{
    // Object changed, update database
    UpdateDatabase(updatedObject);
}

// Safe object identification
object? unknownObject = GetUnknownObject();
string identifier = unknownObject.ComputeHash();
LogEvent($"Processing object: {identifier}");
```

---

## Validation Utilities

### IsValidEmail

**Edge Cases:**
- Null input: Returns false
- Empty string: Returns false
- Whitespace only: Returns false
- Malformed emails: Returns false
- Valid emails with special characters: Returns true

**Security Considerations:**
- Only validates format, not email existence
- No DNS lookups or MX record validation
- Safe for user input validation without external calls
- Prevents email injection attacks through format validation

**Example Usage:**
```csharp
// User registration validation
string email = Request.Form["email"];
if (!ValidationUtility.IsValidEmail(email))
{
    return BadRequest("Invalid email format");
}

// Bulk email validation
var emails = GetEmailList();
var validEmails = emails.Where(ValidationUtility.IsValidEmail).ToList();
```

### IsNumeric

**Edge Cases:**
- Null input: Returns false
- Empty string: Returns false
- Whitespace only: Returns false
- Non-digit characters: Returns false
- Digits only: Returns true

**Security Considerations:**
- Prevents injection attacks through numeric validation
- No decimal points or negative signs allowed
- Fast validation without regex overhead

**Example Usage:**
```csharp
// Phone number validation (digits only)
string phone = Request.Form["phone"];
if (!ValidationUtility.IsNumeric(phone))
{
    return BadRequest("Phone number must contain digits only");
}

// Numeric ID validation
string userId = Request.Form["userId"];
if (!ValidationUtility.IsNumeric(userId))
{
    return BadRequest("Invalid user ID format");
}
```

### IsValidUrl

**Edge Cases:**
- Null input: Returns false
- Empty string: Returns false
- Whitespace only: Returns false
- Relative URLs: Returns false
- Non-HTTP/HTTPS schemes: Returns false
- Malformed URLs: Returns false

**Security Considerations:**
- Only accepts HTTP and HTTPS schemes
- Prevents validation of dangerous schemes (javascript://, file://, etc.)
- No DNS lookups or connectivity checks
- Safe for user input validation

**Example Usage:**
```csharp
// Website URL validation
string website = Request.Form["website"];
if (!ValidationUtility.IsValidUrl(website))
{
    return BadRequest("Invalid website URL");
}

// Redirect URL validation
string redirectUrl = Request.Query["returnUrl"];
if (!ValidationUtility.IsValidUrl(redirectUrl))
{
    redirectUrl = "/default"; // Safe fallback
}
return Redirect(redirectUrl);
```

---

## HTTP Extensions

### HttpStatusCodeExtensions

**Edge Cases:**
- All HTTP status codes are handled
- Custom status codes: Treated based on range
- Null status codes: Not applicable (struct type)

**Security Considerations:**
- Proper status code classification prevents security misconfigurations
- Accurate success/failure detection for logging and monitoring

**Example Usage:**
```csharp
// HTTP response handling
var response = await httpClient.GetAsync(url);
if (response.StatusCode.IsSuccessful())
{
    return await response.Content.ReadAsStringAsync();
}
else if (response.StatusCode.IsClientError())
{
    LogClientError(response.StatusCode);
    throw new ClientRequestException(response.StatusCode);
}
else if (response.StatusCode.IsServerError())
{
    LogServerError(response.StatusCode);
    throw new ServerErrorException(response.StatusCode);
}
```

---

## JSON Extensions

### JsonElementExtensions

**Edge Cases:**
- Null JsonElement: Returns default values
- Missing properties: Returns default values
- Type mismatches: Returns default values or throws based on method
- Circular references: Handled by underlying JSON parser

**Security Considerations:**
- Safe property access prevents exceptions
- Case-insensitive access available when needed
- Prevents JSON injection through safe parsing

**Example Usage:**
```csharp
// Safe JSON property access
JsonElement root = JsonDocument.Parse(jsonString).RootElement;

// Safe property access with default values
string name = root.Get("name")?.GetString() ?? "Unknown";
int age = root.Get("age")?.GetInt32() ?? 0;

// Case-insensitive access (when needed)
bool found = root.TryGetPropertyCaseInsensitive("USERNAME", out var value);
```

---

## General Security Guidelines

### Input Validation

1. **Always validate user input** before processing
2. **Use null-safe methods** to prevent NullReferenceException
3. **Validate file paths** using CombineAndValidate to prevent traversal attacks
4. **Validate URLs** to prevent redirect attacks
5. **Validate email formats** before storage or processing

### Error Handling

1. **Use safe methods** that don't throw exceptions for user input
2. **Log security events** when validation fails
3. **Provide meaningful error messages** without exposing internal details
4. **Use fallback values** for non-critical validation failures

### Performance Considerations

1. **Choose appropriate validation methods** based on performance requirements
2. **Cache validation results** when appropriate
3. **Use compiled regex** for frequent pattern matching
4. **Consider async methods** for I/O-bound validations

### Logging and Monitoring

1. **Log validation failures** for security monitoring
2. **Monitor hash collisions** in caching scenarios
3. **Track path traversal attempts** for security analysis
4. **Audit URL validation failures** for attack detection

---

## Best Practices

### Defensive Programming

```csharp
// GOOD: Safe input processing
string userInput = Request.Form["data"];
string safeData = userInput.SafeSubstring(0, 1000);
if (safeData.IsNullOrWhiteSpace())
{
    return BadRequest("Input cannot be empty");
}

// AVOID: Direct string operations without validation
string unsafeData = Request.Form["data"].Substring(0, 1000); // May throw
```

### Security-First Design

```csharp
// GOOD: Secure file handling
string fileName = Request.Form["filename"];
try
{
    string safePath = PathExtensions.CombineAndValidate(uploadDir, fileName);
    await SaveFileAsync(safePath, fileContent);
}
catch (SecurityException)
{
    LogSecurityEvent("Path traversal attempt", fileName);
    return StatusCode(403);
}

// AVOID: Unsecure file operations
string unsafePath = Path.Combine(uploadDir, fileName); // Vulnerable
await SaveFileAsync(unsafePath, fileContent);
```

### Validation Chains

```csharp
// GOOD: Comprehensive validation
public ValidationResult ValidateUserInput(UserInput input)
{
    var result = new ValidationResult();
    
    if (input.Email.IsNullOrWhiteSpace())
        result.AddError("Email is required");
    else if (!ValidationUtility.IsValidEmail(input.Email))
        result.AddError("Invalid email format");
    
    if (input.Phone.IsNullOrWhiteSpace())
        result.AddError("Phone is required");
    else if (!ValidationUtility.IsNumeric(input.Phone))
        result.AddError("Phone must contain digits only");
    
    if (input.Website.IsNullOrWhiteSpace())
        result.AddError("Website is required");
    else if (!ValidationUtility.IsValidUrl(input.Website))
        result.AddError("Invalid website URL");
    
    return result;
}
```

---

## Testing Edge Cases

When testing PowerCSharp extensions, ensure coverage of these edge cases:

### String Extensions
```csharp
[Test]
public void SafeSubstring_EdgeCases()
{
    // Null input
    Assert.AreEqual("", ((string?)null).SafeSubstring(0, 10));
    
    // Empty string
    Assert.AreEqual("", "".SafeSubstring(0, 10));
    
    // Negative parameters
    Assert.AreEqual("", "hello".SafeSubstring(-1, 5));
    Assert.AreEqual("", "hello".SafeSubstring(0, -1));
    
    // Out of bounds
    Assert.AreEqual("", "hello".SafeSubstring(10, 5));
    
    // Length exceeds available
    Assert.AreEqual("lo", "hello".SafeSubstring(3, 10));
}
```

### Path Extensions
```csharp
[Test]
public void CombineAndValidate_Security()
{
    string basePath = "/var/www/uploads";
    
    // Valid paths
    Assert.DoesNotThrow(() => 
        PathExtensions.CombineAndValidate(basePath, "file.jpg"));
    
    // Path traversal attempts
    Assert.Throws<SecurityException>(() => 
        PathExtensions.CombineAndValidate(basePath, "../../etc/passwd"));
    Assert.Throws<SecurityException>(() => 
        PathExtensions.CombineAndValidate(basePath, "..\\..\\windows\\system32"));
}
```

---

## Conclusion

PowerCSharp provides comprehensive edge case handling and security features built into the extension methods. By following these guidelines and understanding the edge cases covered, you can build robust, secure applications that handle user input safely and gracefully.

Key takeaways:
1. **Always prefer safe methods** over direct framework methods for user input
2. **Validate file paths** using CombineAndValidate to prevent traversal attacks
3. **Use appropriate validation methods** for different data types
4. **Handle edge cases explicitly** in your application logic
5. **Log security events** for monitoring and auditing

For more specific examples and advanced usage patterns, refer to the individual package documentation and sample applications.
