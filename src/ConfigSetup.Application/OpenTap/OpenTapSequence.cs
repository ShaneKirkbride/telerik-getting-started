using System;
using System.Collections.Generic;
using System.Linq;

using ConfigSetup.Domain.Scpi;

namespace ConfigSetup.Application.OpenTap;

/// <summary>
/// Represents a parsed OpenTAP sequence that can be translated into SCPI commands.
/// </summary>
public sealed class OpenTapSequence
{
    public OpenTapSequence(string? name, string? resourceAddress, IEnumerable<ScpiCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        var commandList = commands.Where(static command => !string.IsNullOrWhiteSpace(command.Text)).ToList();

        if (commandList.Count == 0)
        {
            throw new InvalidOperationException("The OpenTAP sequence does not contain any executable SCPI commands.");
        }

        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        ResourceAddress = string.IsNullOrWhiteSpace(resourceAddress) ? null : resourceAddress.Trim();
        Commands = commandList;
    }

    public string? Name { get; }

    public string? ResourceAddress { get; }

    public IReadOnlyList<ScpiCommand> Commands { get; }
}
