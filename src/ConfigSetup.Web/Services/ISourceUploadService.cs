using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Web.Contracts;

namespace ConfigSetup.Web.Services;

/// <summary>
/// Contract implemented by services capable of uploading source settings to hardware endpoints.
/// </summary>
public interface ISourceUploadService
{
    Task<SourceUploadResult> UploadAsync(SourceUploadRequest request, CancellationToken cancellationToken = default);

    Task<SourceUploadResult> UploadAsync(IEnumerable<SourceUploadRequest> requests, CancellationToken cancellationToken = default);
}
