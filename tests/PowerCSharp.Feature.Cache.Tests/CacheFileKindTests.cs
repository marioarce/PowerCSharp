using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Tests;

public class CacheFileKindTests
{
    [Fact]
    public void CacheFileKind_BuiltIn_Kinds_Are_Registered()
    {
        var allKinds = CacheFileKind.All;
        
        Assert.True(allKinds.Count >= 5, "Should have at least 5 built-in cache file kinds");
        
        // Verify built-in kinds exist
        Assert.NotNull(CacheFileKind.GetById("json"));
        Assert.NotNull(CacheFileKind.GetById("binary"));
        Assert.NotNull(CacheFileKind.GetById("compressed"));
        Assert.NotNull(CacheFileKind.GetById("metadata"));
        Assert.NotNull(CacheFileKind.GetById("temp"));
    }

    [Fact]
    public void CacheFileKind_GetById_Returns_Correct_Kind()
    {
        var jsonKind = CacheFileKind.GetById("json");
        
        Assert.NotNull(jsonKind);
        Assert.Equal("json", jsonKind.Id);
        Assert.Equal("JSON Data", jsonKind.Name);
        Assert.Equal(".json", jsonKind.Extension);
        Assert.Equal("Serialized JSON cache data files", jsonKind.Description);
    }

    [Fact]
    public void CacheFileKind_GetById_Returns_Null_For_Unknown_Id()
    {
        var unknownKind = CacheFileKind.GetById("unknown");
        Assert.Null(unknownKind);
    }

    [Fact]
    public void CacheFileKind_Register_Adds_New_Kind()
    {
        var customKind = CacheFileKind.Register("custom", "Custom Data", ".custom", "Custom cache files");
        
        Assert.NotNull(customKind);
        Assert.Equal("custom", customKind.Id);
        Assert.Equal("Custom Data", customKind.Name);
        Assert.Equal(".custom", customKind.Extension);
        Assert.Equal("Custom cache files", customKind.Description);
        
        // Verify it's in the registry
        var retrieved = CacheFileKind.GetById("custom");
        Assert.Same(customKind, retrieved);
    }

    [Fact]
    public void CacheFileKind_Register_Throws_For_Duplicate_Id()
    {
        // First registration should succeed
        CacheFileKind.Register("test", "Test", ".test", "Test files");
        
        // Second registration with same ID should throw
        Assert.Throws<ArgumentException>(() => 
            CacheFileKind.Register("test", "Test 2", ".test2", "Test files 2"));
    }

    [Fact]
    public void CacheFileKind_MatchesFile_Works_Correctly()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        
        Assert.True(jsonKind.MatchesFile("data.json"));
        Assert.True(jsonKind.MatchesFile("path/to/data.json"));
        Assert.False(jsonKind.MatchesFile("data.txt"));
        Assert.False(jsonKind.MatchesFile("data.json.backup"));
    }

    [Fact]
    public void CacheFileKind_MatchesExtension_Works_Correctly()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        
        Assert.True(jsonKind.MatchesExtension(".json"));
        Assert.True(jsonKind.MatchesExtension(".JSON")); // Case insensitive
        Assert.False(jsonKind.MatchesExtension(".txt"));
        Assert.False(jsonKind.MatchesExtension("json")); // Requires dot
    }

    [Fact]
    public void CacheFileKind_Equality_Works()
    {
        var jsonKind1 = CacheFileKind.GetById("json")!;
        var jsonKind2 = CacheFileKind.GetById("json")!;
        var binaryKind = CacheFileKind.GetById("binary")!;
        
        Assert.Equal(jsonKind1, jsonKind2);
        Assert.True(jsonKind1 == jsonKind2);
        Assert.False(jsonKind1 != jsonKind2);
        
        Assert.NotEqual(jsonKind1, binaryKind);
        Assert.False(jsonKind1 == binaryKind);
        Assert.True(jsonKind1 != binaryKind);
    }

    [Fact]
    public void CacheFileKind_ToString_Returns_Name()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        Assert.Equal("JSON Data", jsonKind.ToString());
    }

    [Fact]
    public void CacheFileKind_HashCode_Is_Stable()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        var hashCode1 = jsonKind.GetHashCode();
        var hashCode2 = jsonKind.GetHashCode();
        
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void CacheFileKind_Constructor_Throws_For_Null_Parameters()
    {
        Assert.Throws<ArgumentNullException>(() => 
            CacheFileKind.Register(null!, "Name", ".ext", "Desc"));
        Assert.Throws<ArgumentNullException>(() => 
            CacheFileKind.Register("id", null!, ".ext", "Desc"));
        Assert.Throws<ArgumentNullException>(() => 
            CacheFileKind.Register("id", "Name", null!, "Desc"));
        Assert.Throws<ArgumentNullException>(() => 
            CacheFileKind.Register("id", "Name", ".ext", null!));
    }

    [Fact]
    public void CacheFileKind_MatchesFile_Handles_Null_Input()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        
        Assert.False(jsonKind.MatchesFile(null!));
        Assert.False(jsonKind.MatchesFile(""));
        Assert.False(jsonKind.MatchesFile("   "));
    }

    [Fact]
    public void CacheFileKind_MatchesExtension_Handles_Null_Input()
    {
        var jsonKind = CacheFileKind.GetById("json")!;
        
        Assert.False(jsonKind.MatchesExtension(null!));
        Assert.False(jsonKind.MatchesExtension(""));
        Assert.False(jsonKind.MatchesExtension("   "));
    }
}
