using System;
using System.Collections.Generic;

namespace ConfigSetup.Web.Contracts;

/// <summary>
/// Describes a request to upload the runtime settings for a single signal source.
/// </summary>
public sealed class SourceUploadRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Source { get; init; }

    public string? Frequency { get; init; }

    public string? Power { get; init; }

    public string? Mode { get; init; }

    public SourceUploadConnection? Connection { get; init; }

    public IList<SourceUploadParameter> Parameters { get; init; } = new List<SourceUploadParameter>();

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Each source upload request must specify a name.");
        }

        foreach (var parameter in Parameters)
        {
            parameter.EnsureIsValid();
        }

        Connection?.EnsureIsValid();
    }
}

/// <summary>
/// Describes the transport required to reach a specific source instrument.
/// </summary>
public sealed class SourceUploadConnection
{
    public string? Protocol { get; init; }

    public string? Address { get; init; }

    public string? Port { get; init; }

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Address) && string.IsNullOrWhiteSpace(Protocol) && string.IsNullOrWhiteSpace(Port))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            throw new InvalidOperationException("Connection details must include an address when provided.");
        }
    }
}

/// <summary>
/// Represents a concrete parameter value associated with a source upload request.
/// </summary>
public sealed class SourceUploadParameter
{
    public SourceUploadParameter()
    {
    }

    public SourceUploadParameter(string name, string? value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; } = string.Empty;

    public string? Value { get; init; }

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Source parameters must specify a name.");
        }
    }
}

/// <summary>
/// Provides summary data returned after an upload completes.
/// </summary>
public sealed class SourceUploadResult
{
    public SourceUploadResult(int uploadedSources, int totalParameters, TimeSpan duration)
    {
        if (uploadedSources < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(uploadedSources));
        }

        if (totalParameters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalParameters));
        }

        UploadedSources = uploadedSources;
        TotalParameters = totalParameters;
        Duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
    }

    public int UploadedSources { get; }

    public int TotalParameters { get; }

    public TimeSpan Duration { get; }

    public static SourceUploadResult Empty => new(0, 0, TimeSpan.Zero);
}
