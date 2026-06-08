using PowerCSharp.Helpers;

namespace WebSample.Samples.Helpers;

/// <summary>
/// Sample endpoints demonstrating cryptographic helper utilities
/// </summary>
public static class CryptoSampleEndpoints
{
    /// <summary>
    /// Gets cryptography demo data
    /// </summary>
    /// <returns>Demo results showing hash computation and random string generation</returns>
    public static object GetDemoData()
    {
        string data = "PowerCSharp";
        return new
        {
            data = data,
            sha256 = CryptoHelper.ComputeSHA256(data),
            md5 = CryptoHelper.ComputeMD5(data),
            randomString = CryptoHelper.GenerateRandomString(10)
        };
    }
}
