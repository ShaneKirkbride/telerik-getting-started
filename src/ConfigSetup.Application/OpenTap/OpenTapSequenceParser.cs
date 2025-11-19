using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using ConfigSetup.Domain.Scpi;

namespace ConfigSetup.Application.OpenTap;

/// <summary>
/// Parses OpenTAP sequence exports into SCPI command lists.
/// </summary>
public sealed class OpenTapSequenceParser : IOpenTapSequenceParser
{
    public OpenTapSequence Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Sequence content is required.", nameof(content));
        }

        var trimmed = content.Trim();
        return trimmed.StartsWith("{", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal)
            ? ParseJson(trimmed)
            : ParsePlainText(trimmed);
    }

    private static OpenTapSequence ParseJson(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var name = TryGetString(root, "name") ?? TryGetString(root, "title");
        var resource = TryGetResource(root);
        var commands = ExtractCommands(root).ToList();

        if (commands.Count == 0)
        {
            throw new InvalidOperationException("The OpenTAP sequence does not declare any SCPI commands.");
        }

        return new OpenTapSequence(name, resource, commands);
    }

    private static IEnumerable<ScpiCommand> ExtractCommands(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var command in ExtractCommands(item))
                {
                    yield return command;
                }
            }

            yield break;
        }

        if (element.TryGetProperty("commands", out var commandsNode))
        {
            foreach (var command in ExtractCommands(commandsNode))
            {
                yield return command;
            }
        }

        if (element.TryGetProperty("steps", out var stepsNode))
        {
            foreach (var step in stepsNode.EnumerateArray())
            {
                if (TryGetString(step, "command") is { } commandText && !string.IsNullOrWhiteSpace(commandText))
                {
                    yield return new ScpiCommand(commandText);
                }
                else if (step.TryGetProperty("scpi", out var scpiNode))
                {
                    foreach (var command in ExtractCommands(scpiNode))
                    {
                        yield return command;
                    }
                }
            }
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return new ScpiCommand(value!);
            }
        }
        else if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("text", out var textNode))
        {
            var value = textNode.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return new ScpiCommand(value!);
            }
        }
    }

    private static string? TryGetResource(JsonElement element)
    {
        if (TryGetString(element, "resource") is { } resource)
        {
            return resource;
        }

        if (element.TryGetProperty("instrument", out var instrumentNode))
        {
            if (TryGetString(instrumentNode, "resource") is { } instrumentResource)
            {
                return instrumentResource;
            }

            if (TryGetString(instrumentNode, "address") is { } instrumentAddress)
            {
                return instrumentAddress;
            }
        }

        return null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static OpenTapSequence ParsePlainText(string content)
    {
        var commands = new List<ScpiCommand>();
        string? resource = null;
        string? name = null;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("name=", StringComparison.OrdinalIgnoreCase))
            {
                name = line[5..].Trim();
                continue;
            }

            if (line.StartsWith("resource=", StringComparison.OrdinalIgnoreCase))
            {
                resource = line[9..].Trim();
                continue;
            }

            if (line.StartsWith("resource:", StringComparison.OrdinalIgnoreCase))
            {
                resource = line[9..].Trim();
                continue;
            }

            commands.Add(new ScpiCommand(line));
        }

        return new OpenTapSequence(name, resource, commands);
    }
}
