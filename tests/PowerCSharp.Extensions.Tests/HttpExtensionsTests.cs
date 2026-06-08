using System.Net;
using System.Net.Http.Headers;
using PowerCSharp.Extensions.Http;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class HttpExtensionsTests
{
    #region HttpStatusCode Extensions Tests

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    [InlineData(HttpStatusCode.NotImplemented, false)]
    public void IsSuccessful_WithHttpStatusCode_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsSuccessful();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(199, false)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(500, false)]
    public void IsSuccessful_WithIntStatusCode_ShouldReturnCorrectValue(int statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsSuccessful();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.Unauthorized, true)]
    [InlineData(HttpStatusCode.Forbidden, true)]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.UnprocessableEntity, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.Created, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsClientError_WithHttpStatusCode_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsClientError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(400, true)]
    [InlineData(401, true)]
    [InlineData(403, true)]
    [InlineData(404, true)]
    [InlineData(499, true)]
    [InlineData(399, false)]
    [InlineData(200, false)]
    [InlineData(500, false)]
    public void IsClientError_WithIntStatusCode_ShouldReturnCorrectValue(int statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsClientError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.NotImplemented, true)]
    [InlineData(HttpStatusCode.BadGateway, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.GatewayTimeout, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    public void IsServerError_WithHttpStatusCode_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsServerError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(500, true)]
    [InlineData(501, true)]
    [InlineData(502, true)]
    [InlineData(503, true)]
    [InlineData(599, true)]
    [InlineData(499, false)]
    [InlineData(200, false)]
    [InlineData(400, false)]
    public void IsServerError_WithIntStatusCode_ShouldReturnCorrectValue(int statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsServerError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.Created, false)]
    public void IsError_WithHttpStatusCode_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(400, true)]
    [InlineData(500, true)]
    [InlineData(200, false)]
    [InlineData(201, false)]
    public void IsError_WithIntStatusCode_ShouldReturnCorrectValue(int statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsError();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently, true)]
    [InlineData(HttpStatusCode.Found, true)]
    [InlineData(HttpStatusCode.SeeOther, true)]
    [InlineData(HttpStatusCode.TemporaryRedirect, true)]
    [InlineData(HttpStatusCode.PermanentRedirect, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsRedirect_WithHttpStatusCode_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsRedirect();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(301, true)]
    [InlineData(302, true)]
    [InlineData(303, true)]
    [InlineData(307, true)]
    [InlineData(308, true)]
    [InlineData(299, false)]
    [InlineData(200, false)]
    [InlineData(400, false)]
    public void IsRedirect_WithIntStatusCode_ShouldReturnCorrectValue(int statusCode, bool expected)
    {
        // Act
        var result = statusCode.IsRedirect();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsCaching_WithNotModified_ShouldReturnTrue()
    {
        // Act
        var result = HttpStatusCode.NotModified.IsCaching();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void IsCaching_WithNonNotModifiedStatus_ShouldReturnFalse(HttpStatusCode statusCode)
    {
        // Act
        var result = statusCode.IsCaching();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region MediaType Extensions Tests

    [Fact]
    public void IsJson_WithApplicationJson_ShouldReturnTrue()
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue("application/json");

        // Act
        var result = mediaType.IsJson();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsJson_WithTextJson_ShouldReturnTrue()
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue("text/json");

        // Act
        var result = mediaType.IsJson();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    [InlineData("text/plain")]
    [InlineData("application/octet-stream")]
    public void IsJson_WithNonJsonMediaType_ShouldReturnFalse(string mediaTypeString)
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue(mediaTypeString);

        // Act
        var result = mediaType.IsJson();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsJson_WithNullMediaType_ShouldReturnFalse()
    {
        // Arrange
        MediaTypeHeaderValue? nullMediaType = null;

        // Act
        var result = nullMediaType.IsJson();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsXml_WithApplicationXml_ShouldReturnTrue()
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue("application/xml");

        // Act
        var result = mediaType.IsXml();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsXml_WithTextXml_ShouldReturnTrue()
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue("text/xml");

        // Act
        var result = mediaType.IsXml();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("text/json")]
    [InlineData("text/plain")]
    [InlineData("application/octet-stream")]
    public void IsXml_WithNonXmlMediaType_ShouldReturnFalse(string mediaTypeString)
    {
        // Arrange
        var mediaType = new MediaTypeHeaderValue(mediaTypeString);

        // Act
        var result = mediaType.IsXml();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsXml_WithNullMediaType_ShouldReturnFalse()
    {
        // Arrange
        MediaTypeHeaderValue? nullMediaType = null;

        // Act
        var result = nullMediaType.IsXml();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MediaTypeExtensions_ShouldBeCaseInsensitive()
    {
        // Arrange
        var jsonUpper = new MediaTypeHeaderValue("APPLICATION/JSON");
        var jsonMixed = new MediaTypeHeaderValue("Application/Json");
        var xmlUpper = new MediaTypeHeaderValue("APPLICATION/XML");
        var xmlMixed = new MediaTypeHeaderValue("Application/Xml");

        // Act & Assert
        Assert.True(jsonUpper.IsJson());
        Assert.True(jsonMixed.IsJson());
        Assert.True(xmlUpper.IsXml());
        Assert.True(xmlMixed.IsXml());
    }

    #endregion
}
