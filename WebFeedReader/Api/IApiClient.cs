using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IApiClient : IDisposable
    {
        Task<string> GetFeedsAsync(DateTime since, CancellationToken ct = default);

        Task<string> GetSourcesAsync(DateTime since, CancellationToken ct = default);
    }
}