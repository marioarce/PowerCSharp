using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PowerCSharp.Extensions.Xml;

/// <summary>
/// Extension methods for XML element operations
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// Gets the <see cref="XElement"/> transformed into a <see cref="Dictionary{string, object}"/> representation.
    /// </summary>
    /// <param name="xmlElement">The XML element to flatten.</param>
    /// <returns>A dictionary representation of the XML element with attributes and nested elements.</returns>
    public static Dictionary<string, object> Flatten(this XElement xmlElement)
    {
        var attributes = xmlElement
            .Attributes()
            .ToDictionary(d => d.Name.LocalName, d => (object)d.Value);

        if (xmlElement.HasElements)
        {
            attributes.Add(
                "_value",
                xmlElement
                    .Elements()
                    .Select(e => Flatten(e))
            );
        }
        else
        {
            if (!xmlElement.IsEmpty)
            {
                attributes.Add("_value", xmlElement.Value);
            }
        }

        var result = new Dictionary<string, object>
        {
            {
                xmlElement.Name.LocalName,
                attributes
            }
        };

        return result;
    }
}
