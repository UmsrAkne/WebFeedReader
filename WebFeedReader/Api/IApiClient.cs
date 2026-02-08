using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IApiClient : IDisposable
    {
        Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default);

        Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default);
    }
}