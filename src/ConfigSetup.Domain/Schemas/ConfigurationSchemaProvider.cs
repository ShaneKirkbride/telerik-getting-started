using System;
using System.IO;
using System.Reflection;
using System.Xml.Schema;

namespace ConfigSetup.Domain.Schemas;

/// <summary>
/// Provides access to the embedded XML schema used to validate configuration documents.
/// </summary>
public static class ConfigurationSchemaProvider
{
    public const string ResourceName = "ConfigSetup.Domain.Schemas.Configuration.xsd";

    public static XmlSchemaSet CreateSchemaSet()
    {
        using var stream = OpenSchemaStream();
        var schema = XmlSchema.Read(stream, null)
            ?? throw new InvalidOperationException($"Failed to read the XML schema resource '{ResourceName}'.");

        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(schema);
        schemaSet.Compile();
        return schemaSet;
    }

    public static Stream OpenSchemaStream()
    {
        var assembly = typeof(ConfigurationSchemaProvider).GetTypeInfo().Assembly;
        var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Unable to locate the embedded schema resource '{ResourceName}'.");
        }

        return stream;
    }
}
